using HW_ConsoleServer.Entity;
using System.Net;

namespace HW_ConsoleServer
{
    internal class Program
    {
        private static readonly IPAddress IP = IPAddress.Parse("127.0.0.1");
        private static readonly int PORT = 8888;

        private static async Task Main(string[] args)
        {
            Console.WriteLine("Run in SERVER mode");
            await Server.RunServer(IP, PORT);
        }

    }
}