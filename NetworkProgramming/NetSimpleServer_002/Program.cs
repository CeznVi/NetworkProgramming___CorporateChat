using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NetSimpleServer_002
{
    internal class Program
    {
        static string[] wishes = new string[]
        {
            "У вас будет удачный день",
            "Ви найдете свою любвовь",
            "Займитесь спортом",
            "Погода будет хорошей",
            "Дождь польет ваш урожай",
            "Вам необходим релакс",
        };


        static void Main(string[] args)
        {
            Console.WriteLine("Server started.....");
            Random random = new Random();

            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                IPAddress iPAddress = IPAddress.Parse("127.0.0.1");
                IPEndPoint iPEndPoint = new IPEndPoint(iPAddress, 49000);

                server.Bind(iPEndPoint);
                server.Listen(10);

                while (true)
                {
                    Socket client = server.Accept();
                    string requestClient = "";

                    Console.WriteLine($"Client: {client.RemoteEndPoint.ToString()}");

                    byte[] bytes = new byte[1024];
                    int byteReciveLen = client.Receive(bytes);

                    requestClient += Encoding.UTF8.GetString(bytes, 0, byteReciveLen);


                    Console.WriteLine($"Request from client: {requestClient}");

                    //client.Send(Encoding.UTF8.GetBytes($"{DateTime.Now} - Hello you request was recive"));
                    client.Send(Encoding.UTF8.GetBytes(wishes[random.Next(0, wishes.Length)]));
                   
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                    
                    if (requestClient == "KILL_SERVER")
                    {
                        break;
                    }



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
                server.Shutdown(SocketShutdown.Both);
                server.Close();
            }


        }
    }
}