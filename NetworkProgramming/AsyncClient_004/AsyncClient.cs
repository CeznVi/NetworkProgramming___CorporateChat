using System.Net;
using System.Net.Sockets;
using System.Text;

namespace AsyncClient_004
{
    internal class AsyncClient
    {
        private Socket _socket;
        private IPEndPoint _ipEndPoint;
        private IPAddress _ipAddress;
        private byte[] _byteMessageResponce;
       

        private void ConsoleWriteErorr(string erorrText)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(erorrText);
            Console.ResetColor();
        }

        public void SendMessage(string ip, int port, string message)
        {
            try
            {
                _ipAddress = IPAddress.Parse(ip);
                _ipEndPoint = new IPEndPoint(_ipAddress, port);
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                 _socket.BeginConnect(_ipEndPoint, 
                    new AsyncCallback(ConnectCallback), 
                    new ClientState() 
                    { 
                        Socket = _socket,
                        Message = message
                    });



            }
            catch (FormatException formatEx)
            {
                ConsoleWriteErorr(formatEx.Message);
            }
            catch (ArgumentException argEx) 
            {
                ConsoleWriteErorr(argEx.Message);
            }
            catch (Exception ex) 
            {
                ConsoleWriteErorr(ex.Message);
            }

        }


        /// <summary>
        /// CALL BACK FUNCTION которая будет передано упрввление при установлении соеденения с удаленнім сервером
        /// </summary>
        /// <param name="ar"></param>
        private void ConnectCallback(IAsyncResult ar)
        {
            ClientState clientState = (ClientState)ar.AsyncState;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Подключение установлено.");
            Console.WriteLine($"{clientState.Socket.RemoteEndPoint}");
            Console.ResetColor();

            ///send data to server
            Send(clientState.Message);

        }

        private void Send(string message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            _socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(SendMessageEndCallBack), _socket);

        }

        //Callback method при успешной передаче всех данніх на сервер
        private void SendMessageEndCallBack(IAsyncResult ar)
        {
            Socket server = (Socket)ar.AsyncState;
            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine("Сообщение было успешно передано на сервер");
            Console.WriteLine($"Обьем переданных данных {server.EndSend(ar)} байт");

            Console.ResetColor();

            ///ПОлучаем ответ от сервера
            ReciveResponce();
        }

        private void ReciveResponce()
        {
            _byteMessageResponce = new byte[1024];
            _socket.BeginReceive(_byteMessageResponce, 0, _byteMessageResponce.Length, SocketFlags.None, new AsyncCallback(ReciveResponceCallBack), _socket);
        
        }

        /// <summary>
        /// Колбек функция управление которой будет передано при получении ответа от сервера
        /// </summary>
        /// <param name="ar"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void ReciveResponceCallBack(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;

            int lenUploadetByte = socket.EndReceive(ar);
            
            //var dataArray = new byte[_byteSegmentResponce.Count];
            //Array.Copy(_byteSegmentResponce.ToArray(), dataArray, _byteSegmentResponce.Count);
            
            string responceFromServer = Encoding.UTF8.GetString(_byteMessageResponce, 0, lenUploadetByte);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Ответ от сервера: {responceFromServer}");
            Console.ResetColor();
        }
    }

}