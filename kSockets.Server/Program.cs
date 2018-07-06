using kdrcts.kSockChannel;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace kSockets.Server
{
    class Program
    {
        public class sampleObject
        {
            public string Name { get; set; }
            public string Message { get; set; }
        }

        static void Main(string[] args)
        {
            kSocketChannel mySocket = new kSocketChannel();
            mySocket.SetSocketBufferSize(4096);
            
            mySocket.IncomingConnected += (Socket) =>
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("Connection is comed from {0} and Handle {1}", (Socket.RemoteEndPoint as IPEndPoint).Address.ToString(), Socket.Handle.ToInt32());
            };
            mySocket.IncomingDisconnected += (Socket) =>
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("Connection is disconnected from {0} and Handle {1}", (Socket.RemoteEndPoint as IPEndPoint).Address.ToString(), Socket.Handle.ToInt32());
            };

            mySocket.ChannelLog += (kSocketChannel.ChannelLogTypes Log, string Message) =>
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("Channel Log: {0} - {1}", Log.ToString(), Message);
            };
            mySocket.ChannelError += (kSocketChannel.ChannelErrorTypes Error, string Message, Exception Ex, System.Net.Sockets.Socket Socket) =>
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Channel Log: {0} - {1}", Error.ToString(), Message);
            };
            mySocket.DataReceived += (kSocketTransferItem Item, System.Net.Sockets.Socket Socket) =>
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                sampleObject receivedObject = kdrcts.kSockChannel.kSocketChannelHelpers.ObjectDeserializer<sampleObject>(Item.TransferedData.ToArray());
                Console.WriteLine("{0} says: {1}", receivedObject.Name, receivedObject.Message);
                mySocket.SendData(Socket, "You are welcome".StringToBytes());
            };
            mySocket.Listen(1920);
            listenKeys:
            string b = Console.ReadLine();
            if (b == "l")
            {
                Console.WriteLine("Active Connection List, Count: {0} \n", mySocket.GiveConnectedSockets().Count());
            }
            goto listenKeys;
        }

    }
}