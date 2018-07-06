using kdrcts.kSockChannel;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace kSockets.Client
{
    class Program
    {
        public class sampleObject
        {
            public string Name;
            public string Message;
        }

        static void Main(string[] args)
        {
            kSocketChannel mySocket = new kSocketChannel();
            mySocket.OutgoingConnected += (Socket) =>
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("Client is connected to outgoing socket");
                byte[] byteobject = kdrcts.kSockChannel.kSocketChannelHelpers.ObjectSerializer<sampleObject>(new sampleObject()
                {
                    Name = "kdrcts",
                    Message = "Hello, I'm client"
                }).StringToBytes();
                mySocket.SendData(Socket, byteobject);
            };
            mySocket.OutgoingDisconnected += (Socket) =>
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("Client is disconnected");
            };
            mySocket.ChannelLog += (kSocketChannel.ChannelLogTypes Log, string Message) =>
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("Channel Log: {0} - {1}", Log.ToString(), Message);
            };
            mySocket.ChannelError += (kSocketChannel.ChannelErrorTypes Error, string Message, Exception Ex, System.Net.Sockets.Socket Socket) =>
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Channel Error: {0} - {1}", Error.ToString(), Message);
            };
            mySocket.DataReceived += (kSocketTransferItem Item, System.Net.Sockets.Socket Socket) =>
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine(Item.TransferedData.ToArray().BytesToString());
            };

            mySocket.Connect("127.0.0.1", 1920);
            Console.ReadLine();
        }
    }
}
