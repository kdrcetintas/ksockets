using System;
using kdrcts.kSockChannel;

namespace kSockets.Client
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var mySocket = new kSocketChannel();
            mySocket.OutgoingConnected += Socket =>
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("Client is connected to outgoing socket");
                var byteobject = kSocketChannelHelpers.ObjectSerializer(new sampleObject
                {
                    Name = "kdrcts",
                    Message = "Hello, I'm client"
                }).StringToBytes();
                mySocket.SendData(Socket, byteobject);
            };
            mySocket.OutgoingDisconnected += Socket =>
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("Client is disconnected");
            };
            mySocket.ChannelLog += (Log, Message) =>
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("Channel Log: {0} - {1}", Log.ToString(), Message);
            };
            mySocket.ChannelError += (Error, Message, Ex, Socket) =>
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Channel Error: {0} - {1}", Error.ToString(), Message);
            };
            mySocket.DataReceived += (Item, Socket) =>
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine(Item.TransferedData.ToArray().BytesToString());
            };

            mySocket.Connect("127.0.0.1", 1920);
            Console.ReadLine();
        }

        public class sampleObject
        {
            public string Message;
            public string Name;
        }
    }
}