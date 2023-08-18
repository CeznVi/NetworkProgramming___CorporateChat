using System.Net.Sockets;

namespace HW_ConsoleServer.Entity
{
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
}
