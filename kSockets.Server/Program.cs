using System;
using System.Linq;
using System.Net;
using kdrcts.kSockChannel;

namespace kSockets.Server
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var mySocket = new kSocketChannel();
            mySocket.SetSocketBufferSize(4096);

            mySocket.IncomingConnected += Socket =>
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("Connection is comed from {0} and Handle {1}",
                    (Socket.RemoteEndPoint as IPEndPoint).Address.ToString(), Socket.Handle.ToInt32());
            };
            mySocket.IncomingDisconnected += Socket =>
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("Connection is disconnected from {0} and Handle {1}",
                    (Socket.RemoteEndPoint as IPEndPoint).Address.ToString(), Socket.Handle.ToInt32());
            };

            mySocket.ChannelLog += (Log, Message) =>
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("Channel Log: {0} - {1}", Log.ToString(), Message);
            };
            mySocket.ChannelError += (Error, Message, Ex, Socket) =>
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Channel Log: {0} - {1}", Error.ToString(), Message);
            };
            mySocket.DataReceived += (Item, Socket) =>
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                var receivedObject =
                    kSocketChannelHelpers.ObjectDeserializer<sampleObject>(Item.TransferedData.ToArray());
                Console.WriteLine("{0} says: {1}", receivedObject.Name, receivedObject.Message);
                mySocket.SendData(Socket, "You are welcome".StringToBytes());
            };
            mySocket.Listen(1920);
            listenKeys:
            var b = Console.ReadLine();
            if (b == "l")
                Console.WriteLine("Active Connection List, Count: {0} \n", mySocket.GiveConnectedSockets().Count());
            goto listenKeys;
        }
        public class sampleObject
        {
            public string Name { get; set; }
            public string Message { get; set; }
        }
    }
}