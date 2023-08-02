using System.Net;
using System.Net.Sockets;
using System.Text;

namespace AsyncServer_005
{
    class AsyncServer
    {
        private IPAddress _ipAddress;
        private IPEndPoint _ipEndPoint;
        private Socket _serverSocket;

        private ManualResetEvent _mre;

        public AsyncServer(string ip, int port) 
        {
            if (port <= 0) throw new ArgumentException("PortNumber not correct");

            _ipAddress = IPAddress.Parse(ip);
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _ipEndPoint = new IPEndPoint(_ipAddress, port);
            
            _mre = new ManualResetEvent(false);
        }


        public void StartListening()
        {
            _serverSocket.Bind(_ipEndPoint);
            _serverSocket.Listen(100);

            Console.WriteLine("Server start....");
            Console.WriteLine("в режиме входящих подключений");

            while (true) 
            {
                _mre.Reset();
                _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), _serverSocket);
                _mre.WaitOne();
            }

        }

        private void AcceptCallback(IAsyncResult ar)
        {
            _mre.Set();
            ClientState clientState = new ClientState();

            clientState.WorkSocket = ((Socket)ar.AsyncState).EndAccept(ar);

            clientState.WorkSocket.BeginReceive(clientState.Buffer, 0, ClientState.BufferSize, SocketFlags.Partial, new AsyncCallback(ReeadFinishedCallback), clientState);

        }

        private void ReeadFinishedCallback(IAsyncResult ar)
        {
            ClientState clientState = (ClientState)ar.AsyncState;

            int len = clientState.WorkSocket.EndReceive(ar);

            if(len > 0) 
            {
                clientState.Message = Encoding.UTF8.GetString(clientState.Buffer, 0, len);

                Console.WriteLine($"Запрос от клиента: {clientState.WorkSocket.RemoteEndPoint} ");

                Console.WriteLine($"Тело запроса: {clientState.Message} ");

                SendAnswer(clientState.WorkSocket, "Привет запрос принят. Ожидайте");
            }


        }

        private void SendAnswer(Socket workSocket, string answerServerToClient)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(answerServerToClient);

            workSocket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(SendAnswerCallBack), workSocket);

        }

        private void SendAnswerCallBack(IAsyncResult ar)
        {
            try
            {
                Socket clientSocket = (Socket)ar.AsyncState;
                clientSocket.EndSend(ar);

                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }
            catch (Exception ex) 
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }

        }
    }
}