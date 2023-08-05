using HW_Server.Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HW_Server
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcpListener _tcpListener;

        private List<ConnectedClients> _connectedClientsList;

        public MainWindow()
        {
            InitializeComponent();
        }



        private void Button_ControlServer_Click(object sender, RoutedEventArgs e)
        {
            if (_tcpListener == null)
                StartServer();
            else if (_tcpListener != null)
                StopServer();

        }


        /// <summary>
        /// Запустить сервер
        /// </summary>
        private void StartServer()
        {
            try
            {
                if (_tcpListener == null)
                {
                    IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, 54000);
                    _tcpListener = new TcpListener(iPEndPoint);

                    _tcpListener.Start();

                    ///Изменения интерфейса --------------------------------------  НАЧАЛО
                    Button_ControlServer.Background = Brushes.Red;
                    Button_ControlServer.Content = "Отключить";
                    Label_ServerInf0.Content = "Сервер запущен";
                    ///Изменения интерфейса --------------------------------------  КОНЕЦ
                    
                    
                    
                    //GetClientRequestAsync();

                }

            }
            catch (FormatException fex)
            {
                MessageBox.Show(fex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Остановить сервер
        /// </summary>
        private void StopServer()
        {
            try
            {
                _tcpListener.Stop();
                _tcpListener = null;

                ///Изменения интерфейса --------------------------------------  НАЧАЛО
                Button_ControlServer.Background = Brushes.Green;
                Button_ControlServer.Content = "Включить";
                Label_ServerInf0.Content = "Сервер отключен";
                ///Изменения интерфейса --------------------------------------  КОНЕЦ

            }
            catch (SocketException socEx)
            {
                MessageBox.Show(socEx.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private async void GetClientRequestAsync()
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    if (_tcpListener != null)
                    {
                        try
                        {
                            TcpClient tcpClient = _tcpListener.AcceptTcpClient();
                            StreamReader clientStreemReader = new StreamReader(tcpClient.GetStream(), Encoding.UTF8);

                            string clientMessage = clientStreemReader.ReadToEnd();

                            //if (clientMessage != string.Empty)
                            //    Dispatcher.Invoke(() =>
                            //    {
                            //        ListBox_Message.Items.Add($"{tcpClient.Client.RemoteEndPoint} : {clientMessage}");
                            //    });

                            tcpClient.Close();
                        }
                        catch (Exception)
                        {
                            break;
                        }

                    }

                }


            });

        }



    }
}
