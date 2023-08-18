namespace ConsoleClientServer
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Reflection.PortableExecutable;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;
    using System.Threading;

    class NetworkContext
    {
        public string? nickname;
        public Socket Socket;
        public NetworkStream Stream;

        public NetworkContext(Socket socket, NetworkStream stream)
        {
            Socket = socket;
            Stream = stream;
        }
    }

    enum NetworkMessageType
    {
        LOGIN,
        REJECT_LOGIN,
        LOGOUT,
        NEW_MESSAGE,
        UPDATE_USER_STATUS
    }

    [Serializable]
    class NetworkMessage
    {
        public NetworkMessage(NetworkMessageType MT, string? Owner = null, string? MessageText = null, string? Channel = null, List<string>? Users = null)
        {
            this.MT = MT;
            this.Owner = Owner;
            this.MessageText = MessageText;
            this.Channel = Channel;
            this.Users = Users;
        }

        public NetworkMessageType MT;
        [OptionalField]
        public string? Owner;

        [OptionalField]
        public string? MessageText;
        [OptionalField]
        public string? Channel;

        [OptionalField]
        public List<string>? Users;

        public static NetworkMessage FromBinary(byte[] Buffer)
        {
            MemoryStream memStream = new MemoryStream(Buffer);
            BinaryFormatter formatter = new BinaryFormatter();

#pragma warning disable SYSLIB0011 // Danger: BinaryFormatter.Deserialize is insecure for untrusted input
            return (NetworkMessage)formatter.Deserialize(memStream);
#pragma warning restore SYSLIB0011
        }

        public static MemoryStream ToStream(NetworkMessage message)
        {
            BinaryFormatter formatter = new BinaryFormatter();
#pragma warning disable SYSLIB0011
            MemoryStream memStream = new MemoryStream();
            formatter.Serialize(memStream, message);
#pragma warning restore SYSLIB0011

            return memStream;
        }
    }

    delegate void NetworkHandlerFunc(NetworkMessage message, NetworkContext context);

    internal class Program
    {
        private static readonly IPAddress IP = IPAddress.Parse("127.0.0.1");
        private static readonly int PORT = 8888;

        private static async Task Main(string[] args)
        {
            Console.WriteLine("Choose application mode (S/C):");

            string? command = Console.ReadLine();

            if (command == "S")
            {
                Console.WriteLine("Run in SERVER mode");
                await Server.RunServer(IP, PORT);
            }
            else if (command == "C")
            {
                Console.WriteLine("Run in CLIENT mode");
                Client.RunClient(IP, PORT);
            }
            else
            {
                Console.WriteLine("Unknown mode. Please try again.");
            }
        }
    }

    class Server
    {
        private static readonly ConcurrentDictionary<string, NetworkContext> userDictionary = new();
        private static readonly Dictionary<NetworkMessageType, NetworkHandlerFunc> Handlers = new()
    {
        { NetworkMessageType.LOGIN, OnLogin },
        { NetworkMessageType.NEW_MESSAGE, OnNewMessage }
    };

        public static void BroadcastMessage(NetworkMessage message, string? excludeNickname)
        {
            MemoryStream memStream = NetworkMessage.ToStream(message);
            foreach (var val in userDictionary)
            {
                if (val.Key != excludeNickname)
                    memStream.WriteTo(val.Value.Stream);
            }
        }

        private static void OnLogin(NetworkMessage message, NetworkContext context)
        {
            if (message.Owner != null)
            {
                if (userDictionary.ContainsKey(message.Owner))
                {
                    Console.WriteLine($"User with {message.Owner} login is already in system!");
                    NetworkMessage.ToStream(
                        new NetworkMessage(MT: NetworkMessageType.REJECT_LOGIN, Owner: message.Owner)
                    ).WriteTo(context.Stream);
                }

                if (userDictionary.TryAdd(message.Owner, context))
                {
                    context.nickname = message.Owner;
                    Console.WriteLine($"New user is arrive: {message.Owner}");
                    BroadcastMessage(
                        new NetworkMessage(MT: NetworkMessageType.UPDATE_USER_STATUS, Users: userDictionary.Keys.ToList()),
                        ""
                    );
                }
                else
                {
                    NetworkMessage.ToStream(
                        new NetworkMessage(MT: NetworkMessageType.REJECT_LOGIN, Owner: message.Owner)
                    ).WriteTo(context.Stream);
                }
            }
        }

        private static void OnNewMessage(NetworkMessage message, NetworkContext context)
        {
            if (message.Channel != null)
            {
                Console.WriteLine($"Message [from {context.nickname} to {message.Channel}]: {message.MessageText}");
                NetworkMessage callBackMessage = new NetworkMessage(
                    MT: NetworkMessageType.NEW_MESSAGE, Owner: message.Owner,
                    Channel: message.Channel, MessageText: message.MessageText
                );

                if (message.Channel == "ALL")
                    BroadcastMessage(callBackMessage, message.Owner);

                else if (userDictionary.ContainsKey(message.Channel))
                    NetworkMessage.ToStream(callBackMessage).WriteTo(userDictionary[message.Channel].Stream);
            }
        }

        private static void Logout(string nickname, NetworkContext context)
        {
            if (
                userDictionary.ContainsKey(nickname) &&
                userDictionary.TryRemove(new KeyValuePair<string, NetworkContext>(nickname, context))
            )
            {
                Console.WriteLine($"User has been logout: {nickname}");
                BroadcastMessage(
                    new NetworkMessage(MT: NetworkMessageType.UPDATE_USER_STATUS, Users: userDictionary.Keys.ToList()),
                    context.nickname
                );

                context.Stream.Close();
                context.Socket.Shutdown(SocketShutdown.Both);
            }
        }

        public static async Task RunServer(IPAddress ip, int port)
        {
            TcpListener listener = new(ip, port);
            listener.Start(10);

            try
            {
                while (true)
                {
                    Socket acceptedSocket = await listener.AcceptSocketAsync();
                    _ = ThreadPool.QueueUserWorkItem(HandleSoket, acceptedSocket);
                }
            }
            finally
            {
                listener.Stop();
            }
        }

        public static async void HandleSoket(object? obj)
        {
            if (obj == null)
                return;

            Socket socket = (Socket)obj;
            using var stream = new NetworkStream(socket);
            NetworkContext context = new(socket, stream);

            byte[] buffer = new byte[4096];

            while ((await stream.ReadAsync(buffer)) != 0)
            {
                NetworkMessage networkMessage = NetworkMessage.FromBinary(buffer);
                if (Handlers.ContainsKey(networkMessage.MT))
                    Handlers[networkMessage.MT].DynamicInvoke(networkMessage, context);
                else if (networkMessage.MT == NetworkMessageType.LOGOUT)
                    break;
            }

            if (context.nickname != null)
                Logout(context.nickname, context);
            else
            {
                stream.Close();
                socket.Shutdown(SocketShutdown.Both);
            }
        }
    }

    class Client
    {
        private static readonly Dictionary<NetworkMessageType, NetworkHandlerFunc> Handlers = new()
    {
        { NetworkMessageType.REJECT_LOGIN, OnRejectLogin },
        { NetworkMessageType.NEW_MESSAGE, OnNewMessage },
        { NetworkMessageType.UPDATE_USER_STATUS, OnUpdateUserStatus },
    };

        private static void OnRejectLogin(NetworkMessage message, NetworkContext context)
        {
            Console.WriteLine("Loggin has been rejected. Please try againg");
            throw new SystemException("Loggin has been rejected.");
        }

        private static void OnNewMessage(NetworkMessage message, NetworkContext context)
        {
            Console.WriteLine($"New message [From {message?.Owner} to {message?.Channel}]: {message?.MessageText}");
        }

        private static void OnUpdateUserStatus(NetworkMessage message, NetworkContext context)
        {
            if (message.Users != null)
            {
                Console.WriteLine("Updated list of users");
                message.Users.ForEach(Console.WriteLine);
            }
        }

        private static void Login(NetworkContext context)
        {
            Console.WriteLine("Enter your nickname:");
            context.nickname = Console.ReadLine();

            NetworkMessage.ToStream(
                new NetworkMessage(MT: NetworkMessageType.LOGIN, Owner: context.nickname)
            ).WriteTo(context.Stream);
        }

        private static void NewMessage(NetworkContext context)
        {
            Console.WriteLine("Enter your message:");
            string? messageText = Console.ReadLine();
            Console.WriteLine("Enter your receiveer (or 'ALL'):");
            string? receiver = Console.ReadLine();

            if (messageText == null || receiver == null)
                return;

            NetworkMessage.ToStream(
                new NetworkMessage(MT: NetworkMessageType.NEW_MESSAGE, Owner: context.nickname, Channel: receiver, MessageText: messageText)
            ).WriteTo(context.Stream);
        }

        private static void Logout(NetworkContext context)
        {
            NetworkMessage.ToStream(
                new NetworkMessage(MT: NetworkMessageType.LOGOUT, Owner: context.nickname)
            ).WriteTo(context.Stream);

            context.Stream.Close();
            context.Socket.Shutdown(SocketShutdown.Both);
        }

        public static void RunClient(IPAddress ip, int port)
        {
            var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(ip, port);

            var stream = new NetworkStream(socket);
            NetworkContext context = new(socket, stream);

            _ = ThreadPool.QueueUserWorkItem(HandleSoket, context);

            Login(context);

            while (true)
            {
                Console.WriteLine("Enter 'NEW MESSAGE' if you want send message");
                Console.WriteLine("Enter 'LOGOUT' if you want to leave");
                Console.WriteLine("Your choice: ");
                string? command = Console.ReadLine();

                if (command == "NEW MESSAGE")
                    NewMessage(context);
                else if (command == "LOGOUT")
                {
                    Logout(context);
                    break;
                }
            }
        }

        public static async void HandleSoket(object? obj)
        {
            if (obj == null)
                return;

            NetworkContext context = (NetworkContext)obj;

            byte[] buffer = new byte[4096];

            while ((await context.Stream.ReadAsync(buffer)) != 0)
            {
                NetworkMessage networkMessage = NetworkMessage.FromBinary(buffer);
                if (Handlers.ContainsKey(networkMessage.MT))
                    Handlers[networkMessage.MT].DynamicInvoke(networkMessage, context);
            }

            Logout(context);
        }
    }
}