using System.Net.Sockets;
using System.Net;
using System.Text;

namespace NetSimpleClient_003
{
    internal class Program
    {
        static void Main(string[] args)
        {

            SendMessage("127.0.0.1", "Привет Сервер, я Т800 мне нужно обновление прошивки");
            SendMessage("127.0.0.1", "Привет Сервер");
            SendMessage("127.0.0.1", "Привет Сервер");
            SendMessage("127.0.0.1", "Привет Сервер");

            SendMessage("127.0.0.1", "KILL_SERVER");

        }

        private static void SendMessage(string ipAdressString, string message)
        {
            IPAddress ipAddress;
            if (IPAddress.TryParse(ipAdressString, out ipAddress))
            {
                IPEndPoint iPEndPoint = new IPEndPoint(ipAddress, 49000);

                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

                try
                {
                    socket.Connect(iPEndPoint);

                    if (socket.Connected)
                    {
                       
                        socket.Send(Encoding.UTF8.GetBytes(message));

                        int lenResponse = 0;
                        byte[] buffer = new byte[1024];
                        string response = string.Empty;

                        do
                        {
                            lenResponse = socket.Receive(buffer);

                            response += Encoding.UTF8.GetString(buffer, 0, lenResponse);


                        } while (lenResponse > 0);

                        Console.WriteLine($"Responce from server ----------- START");
                        Console.ForegroundColor = ConsoleColor.Green;

                        Console.WriteLine(response);
                        Console.ResetColor();
                        Console.WriteLine($"Responce from server ----------- END");
                    }
                    else
                    {
                        Console.WriteLine("Incorect socket connection");
                    }
                }
                catch (SocketException sEx)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(sEx.Message);
                    Console.ResetColor();
                }
                finally
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }

            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("IP address is not valid");
                Console.ResetColor();
            }
        
    }
    }
}