using HW_ConsoleServer.Entity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HW_ConsoleServer.Entity
{
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

}
