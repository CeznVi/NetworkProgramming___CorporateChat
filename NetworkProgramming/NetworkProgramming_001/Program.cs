using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NetworkProgramming_001
{
    internal class Program
    {

        static void Main(string[] args)
        {

            IPAddress[] ips = Dns.GetHostAddresses("google.com");

            foreach (var iPAddress in ips)
            {
                Console.WriteLine(iPAddress.ToString());
                ConnectToRemoteServer(iPAddress.ToString());
            }

            //foreach (var item in ips)
            //{
            //    Byte[] bytes = item.GetAddressBytes();

            //    for (int i = 0; i < bytes.Length; i++)
            //    {
            //        Console.Write(bytes[i].ToString());
            //    }
            //}




        }

        private static void ConnectToRemoteServer(string ipAddressStr)
        {
            //try
            //{
            //    IPAddress ipAdress = IPAddress.Parse(ipAddress);
            //}
            //catch(FormatException) 
            //{ 
            //    Console.ForegroundColor = ConsoleColor.Red;
            //    Console.WriteLine("IP address is not valid");
            //    Console.ResetColor();
            //}
            //catch(Exception ex) 
            //{
            //    Console.WriteLine(ex.Message);
            //}

            IPAddress ipAddress;
            if (IPAddress.TryParse(ipAddressStr, out ipAddress))
            {
                IPEndPoint iPEndPoint = new IPEndPoint(ipAddress, 80);

                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                
                try 
                {
                    socket.Connect(iPEndPoint);

                    if(socket.Connected) 
                    {
                        string request = "GET\r\n\r\n";

                        socket.Send(Encoding.UTF8.GetBytes(request));

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