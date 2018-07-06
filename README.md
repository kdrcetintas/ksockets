# kSockets Helpers

It's a helper module for creating server / client communications.
Supports any object could be serialized as Json

Just reference the main module to your project,
Create instance of module and select your type (server / client)
Register for events of module.

Start the communication!
Example server and client projects included in the latest branch.

* Info: An update will come soon as possible for Send any objects contains any properties like (Image/Bitmap/File/Audio)

![alt text](https://raw.githubusercontent.com/kdrcetintas/ksockets/master/kSockChannel/Example.png)

    # SampleTransferObject.cs
    public class sampleObject
    {
        public string Name { get; set; }
        public string Message { get; set; }
    }

    # Server.cs
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
    
    # Client.cs
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
