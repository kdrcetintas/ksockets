///-----------------------------------------------------------------
///   Namespace:        kdrcts
///   Class:            kSockChannel
///   Description:      Custom socket channel has an asynchronous data receiving and sending and support server or client instances.
///   Author:           @kdrcetintas
///   Date:             2018-05-28
///   Version:          1.0
///   Notes:            Enjoy it.
///   Revision History:
///

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace kdrcts.kSockChannel
{
    public class kSocketChannel : IDisposable
    {
        #region Vars
        private List<Socket> LocalSockets;
        private List<Socket> RemoteSockets;
        private int SocketBufferSize { get; set; }
        #endregion

        #region Ctors
        public kSocketChannel(int _SocketBufferSize = 1024)
        {

            this.SocketBufferSize = _SocketBufferSize;
            this.LocalSockets = new List<Socket>();
            this.RemoteSockets = new List<Socket>();
        }
        public void Dispose()
        {
            try
            {
                this.LocalSockets.ForEach(r =>
                {
                    r.Shutdown(SocketShutdown.Both);
                    r.Close();
                    r = null;
                });
                this.RemoteSockets.ForEach(r =>
                {
                    r.Shutdown(SocketShutdown.Both);
                    r.Close();
                    r = null;
                });
                this.LocalSockets = null;
                this.RemoteSockets = null;
            }
            catch (Exception) { }
        }
        #endregion

        #region Public Stuffs

        /// <summary>
        /// Will change the buffer size for socket data receiving actions.
        /// The changed value will be use at next data receiving actions
        /// </summary>
        /// <param name="_BufferSize"></param>
        /// <returns></returns>
        public bool SetSocketBufferSize(int _BufferSize)
        {
            try
            {
                this.SocketBufferSize = _BufferSize;
                return true;
            }
            catch (Exception ex)
            {
                this.ChannelError?.Invoke(ChannelErrorTypes.BufferChangeError, ex.Message.ToString(), ex, null);
                return false;
            }
        }

        /// <summary>
        /// Returns type of System.Net.Socket list has containing local sockets.
        /// </summary>
        /// <returns></returns>
        public List<Socket> GiveLocalSockets()
        {
            return this.LocalSockets;
        }

        /// <summary>
        /// Returns type of System.Net.Socket list has containing remote sockets to connected any local socket.
        /// </summary>
        /// <returns></returns>
        public List<Socket> GiveConnectedSockets()
        {
            return this.RemoteSockets;
        }

        /// <summary>
        /// Will create a local socket end start listenining port parameter number.
        /// Port number should be 0-65535 and shouldn't not be used other applications.
        /// </summary>
        /// <param name="Port"></param>
        public void Listen(int Port)
        {
            try
            {
                var listenerSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listenerSock.Bind(new IPEndPoint(IPAddress.Any, Port));
                listenerSock.Listen(1);
                listenerSock.BeginAccept(new AsyncCallback(incomingAccept), listenerSock);
                this.LocalSockets.Add(listenerSock);
                this.ChannelLog?.Invoke(ChannelLogTypes.ListenerStarted, "inSocket currently listenning on " + Port.ToString());
            }
            catch (Exception ex)
            {
                this.ChannelError?.Invoke(ChannelErrorTypes.ListenerError, ex.Message.ToString(), ex, null);
            }
        }

        /// <summary>
        /// Will create a local socket and try to connect remote socket with specific ipendpoint
        /// </summary>
        /// <param name="IP"></param>
        /// <param name="Port"></param>
        public void Connect(string IP, int Port)
        {
            try
            {
                Socket connectorSock = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                connectorSock.BeginConnect(new IPEndPoint(IPAddress.Parse(IP), Port), new AsyncCallback(outgoingConnect), connectorSock);
                this.LocalSockets.Add(connectorSock);
                this.ChannelLog?.Invoke(ChannelLogTypes.ConnectionStarted, "Client connection started to outgoing socket " + IP + ":" + Port);
            }
            catch (Exception ex)
            {
                this.ChannelError?.Invoke(ChannelErrorTypes.ConnectError, ex.Message.ToString(), ex, null);
            }
        }

        /// <summary>
        /// You could give any socket at parameter from you have got variables event callbacks. It's will try to disconnect and shutdown socket and remove from events.
        /// </summary>
        /// <param name="mySocket"></param>
        public void Disconnect(Socket mySocket)
        {
            try
            {
                mySocket.Shutdown(SocketShutdown.Both);
                mySocket.Close();
                this.LocalSockets.Remove(mySocket);
                this.RemoteSockets.Remove(mySocket);
            }
            catch (Exception ex)
            {
                this.ChannelError?.Invoke(ChannelErrorTypes.DisconnectError, ex.Message.ToString(), ex, mySocket);
            }
        }

        /// <summary>
        /// You should give target socket at OtherSocket list property from your channel classs and give any byte array, The byte array will send and receive other sock asynchronously and seperated with spliitted by specific EOF byte array
        /// </summary>
        /// <param name="TargetSock"></param>
        /// <param name="TransferByte"></param>
        public void SendData(Socket TargetSock, byte[] TransferByte)
        {
            try
            {
                if (TargetSock.Connected)
                {
                    byte[] BytesToSend = Combine(TransferByte, kSocketTransferItem.TransferSplitData);

                    TargetSock.BeginSend(BytesToSend, 0, BytesToSend.Length, 0, new AsyncCallback((IAsyncResult ar) =>
                    {
                        try
                        {
                            int bytesSent = ((Socket)ar.AsyncState).EndSend(ar);
                            this.ChannelLog?.Invoke(ChannelLogTypes.Sent, bytesSent + " bytes to client is done");
                        }
                        catch (Exception ex)
                        {
                            this.ChannelError?.Invoke(ChannelErrorTypes.SendError, ex.Message, ex, (Socket)ar.AsyncState);
                        }
                    }), TargetSock);

                    this.ChannelLog?.Invoke(ChannelLogTypes.SendStarted, BytesToSend.Length + " byte send is started");
                }
                else
                {
                    this.OutgoingDisconnected?.Invoke(TargetSock);
                    this.ChannelError?.Invoke(ChannelErrorTypes.SendError, "Target socket has no active connection", null, TargetSock);
                }
            }
            catch (Exception ex)
            {
                this.ChannelError?.Invoke(ChannelErrorTypes.SendError, ex.Message, ex, TargetSock);
            }
        }
        #endregion

        #region Incoming Methods
        private void incomingAccept(IAsyncResult ar)
        {
            string clientIP = "";
            try
            {
                Socket inSock = (System.Net.Sockets.Socket)ar.AsyncState;
                Socket clientSock = inSock.EndAccept(ar);
                clientIP = (clientSock.RemoteEndPoint as IPEndPoint).Address.ToString();
                inSock.BeginAccept(new AsyncCallback(incomingAccept), inSock);
                this.IncomingConnected?.Invoke(clientSock);
                this.RemoteSockets.Add(clientSock);

                kSocketTransferItem TransferItem = new kSocketTransferItem(this.SocketBufferSize) { TransferSocket = clientSock };
                clientSock.BeginReceive(TransferItem.TempData, 0, TransferItem.TransferSize, SocketFlags.Partial, new AsyncCallback(incomingReceive), TransferItem);

                this.ChannelLog?.Invoke(ChannelLogTypes.ClientConnected, String.Concat("Client connected from ", clientIP));
                inSock = null;
                clientSock = null;
            }
            catch (Exception ex)
            {
                this.IncomingDisconnected?.Invoke((System.Net.Sockets.Socket)ar.AsyncState);
                this.RemoteSockets.Remove((System.Net.Sockets.Socket)ar.AsyncState);
                this.ChannelError?.Invoke(ChannelErrorTypes.ConnectError, ex.Message.ToString(), ex, (System.Net.Sockets.Socket)ar.AsyncState);
            }
        }
        private void incomingReceive(IAsyncResult ar)
        {
            kSocketTransferItem TransferItem = (kSocketTransferItem)ar.AsyncState;
            try
            {
                int bytesRead = TransferItem.TransferSocket.EndReceive(ar);
                if (bytesRead > 0)
                {
                    Array.Resize(ref TransferItem.TempData, bytesRead);
                    TransferItem.TransferedData.AddRange(TransferItem.TempData.ToList());
                    Array.Resize(ref TransferItem.TempData, TransferItem.TransferSize);
                    TransferItem.TransferSocket.BeginReceive(TransferItem.TempData, 0, TransferItem.TransferSize, SocketFlags.Partial, new AsyncCallback(incomingReceive), TransferItem);
                }
                if (bytesRead != TransferItem.TransferSize)
                {
                    if (!IsEndOfMessage(TransferItem.TransferedData.ToArray(), kSocketTransferItem.TransferSplitData))
                    {
                        TransferItem.TransferSocket.BeginReceive(TransferItem.TempData, 0, TransferItem.TransferSize, SocketFlags.Partial, new AsyncCallback(incomingReceive), TransferItem);
                    }
                    else
                    {
                        // stream is completed.
                        Array.Clear(TransferItem.TempData, 0, TransferItem.TempData.Length);
                        TransferItem.TransferedData.RemoveRange(TransferItem.TransferedData.Count() - kSocketTransferItem.TransferSplitData.Length, kSocketTransferItem.TransferSplitData.Length);
                        this.DataReceived?.Invoke(TransferItem, TransferItem.TransferSocket);
                    }
                }
            }
            catch (Exception ex)
            {
                this.IncomingDisconnected?.Invoke(TransferItem.TransferSocket);
                this.RemoteSockets.Remove(TransferItem.TransferSocket);
            }
        }
        #endregion

        #region Outgoing Methods
        private void outgoingConnect(IAsyncResult ar)
        {
            try
            {
                Socket inSock = (Socket)ar.AsyncState;
                inSock.EndConnect(ar);

                kSocketTransferItem TransferItem = new kSocketTransferItem(this.SocketBufferSize) { TransferSocket = inSock };
                inSock.BeginReceive(TransferItem.TempData, 0, TransferItem.TransferSize, SocketFlags.Partial, new AsyncCallback(outgoingReceive), TransferItem);

                this.ChannelLog?.Invoke(ChannelLogTypes.ClientConnected, "Client has successfully connected to listener");
                this.OutgoingConnected?.Invoke((Socket)ar.AsyncState);
                inSock = null;
            }
            catch (Exception ex)
            {
                this.ChannelError?.Invoke(ChannelErrorTypes.ConnectError, ex.Message.ToString(), ex, (Socket)ar.AsyncState);
            }
        }
        private void outgoingReceive(IAsyncResult ar)
        {
            kSocketTransferItem TransferItem = (kSocketTransferItem)ar.AsyncState;
            try
            {
                int bytesRead = TransferItem.TransferSocket.EndReceive(ar);
                if (bytesRead > 0)
                {
                    Array.Resize(ref TransferItem.TempData, bytesRead);
                    TransferItem.TransferedData.AddRange(TransferItem.TempData.ToList());
                    Array.Resize(ref TransferItem.TempData, TransferItem.TransferSize);
                    TransferItem.TransferSocket.BeginReceive(TransferItem.TempData, 0, TransferItem.TransferSize, SocketFlags.Partial, new AsyncCallback(incomingReceive), TransferItem);
                }
                if (bytesRead != TransferItem.TransferSize)
                {
                    if (!IsEndOfMessage(TransferItem.TransferedData.ToArray(), kSocketTransferItem.TransferSplitData))
                    {
                        TransferItem.TransferSocket.BeginReceive(TransferItem.TempData, 0, TransferItem.TransferSize, SocketFlags.Partial, new AsyncCallback(incomingReceive), TransferItem);
                    }
                    else
                    {
                        kSocketTransferItem _TransferItem = TransferItem;
                        Array.Clear(TransferItem.TempData, 0, TransferItem.TempData.Length);
                        TransferItem.TransferedData.RemoveRange(TransferItem.TransferedData.Count() - kSocketTransferItem.TransferSplitData.Length, kSocketTransferItem.TransferSplitData.Length);
                        this.DataReceived?.Invoke(_TransferItem, _TransferItem.TransferSocket);
                    }
                }
            }
            catch (Exception)
            {
                this.OutgoingDisconnected?.Invoke(TransferItem.TransferSocket);
                this.RemoteSockets.Remove(TransferItem.TransferSocket);
            }
        }
        #endregion

        #region Private Methods
        private bool IsEndOfMessage(byte[] MessageToCheck, byte[] EndOfMessage)
        {
            for (int i = 0; i < EndOfMessage.Length; i++)
            {
                if (MessageToCheck[MessageToCheck.Length - (EndOfMessage.Length - i)] != EndOfMessage[i])
                    return false;
            }
            return true;
        }

        private byte[] RemoveEndOfMessage(byte[] MessageToClear, byte[] EndOfMessage)
        {
            byte[] Return = new byte[MessageToClear.Length - EndOfMessage.Length];
            Array.Copy(MessageToClear, Return, Return.Length);
            return Return;
        }

        private byte[] Combine(params byte[][] arrays)
        {
            byte[] rv = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                System.Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }
        #endregion

        #region Enums
        public enum ChannelErrorTypes
        {
            Disconnected,
            ConnectError,
            ListenerError,
            SendError,
            ReceiveError,
            DisconnectError,
            BufferChangeError
        }

        public enum ChannelLogTypes
        {
            ListenerStarted,
            ClientConnected,
            ReceiveStarted,
            Received,
            SendStarted,
            Sent,
            ConnectionStarted
        }
        #endregion

        #region Events
        public delegate void ServerErrorCallback(ChannelErrorTypes Error, string Message, Exception Ex, Socket Socket);
        public delegate void ListenerLogCallback(ChannelLogTypes Log, string Message);
        public delegate void ClientAction(Socket Socket);
        public delegate void ClientMessageCallback(kSocketTransferItem Item, Socket Socket);

        /// <summary>
        /// Triggered when an error corrupted at the socket channel
        /// </summary>
        public event ServerErrorCallback ChannelError;

        /// <summary>
        /// Triggered at specific actions has been runned at the socket channel
        /// </summary>
        public event ListenerLogCallback ChannelLog;

        /// <summary>
        /// Triggered when socket channel has an a listener and other channel connected to local socket channel
        /// </summary>
        public event ClientAction IncomingConnected;

        /// <summary>
        /// Triggered when socket channel has an a listener and other channel disconnected to local socket channel
        /// </summary>
        public event ClientAction IncomingDisconnected;

        /// <summary>
        /// Triggered when socket channel has a remote connection via Connect method and local socket successfully connected to remote socket channel
        /// </summary>
        public event ClientAction OutgoingConnected;

        /// <summary>
        /// Triggered when socket channel has a remote connection via Connect method and local socket disconnected from remote socket channel
        /// </summary>
        public event ClientAction OutgoingDisconnected;

        /// <summary>
        /// Triggered when a data has been received from other socket channel
        /// </summary>
        public event ClientMessageCallback DataReceived;
        #endregion
    }
}
