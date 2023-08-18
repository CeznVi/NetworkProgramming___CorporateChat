using HW_ConsoleServer.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client.Entity
{
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
