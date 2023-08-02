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

namespace WpfTCP_Server__007
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcpListener _tcpListener;


        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonStartServer_Click(object sender, RoutedEventArgs e)
        {
            if (_tcpListener == null)
                ServerStart();
            else if (_tcpListener != null)
            { 
                ServerStop(); 
            }
        }

        private void ServerStop()
        {
            try
            {
                _tcpListener.Stop();

                TextBlock_Info.Foreground = Brushes.Red;
                TextBlock_Info.Text = "Сервер остановлен";

                ButtonStartServer.Content = "Start Server";
                _tcpListener = null;
                
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

                            if (clientMessage != string.Empty)
                                Dispatcher.Invoke(() =>
                                {
                                    ListBox_Message.Items.Add($"{tcpClient.Client.RemoteEndPoint} : {clientMessage}");
                                });

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


        private void ServerStart()
        {
            try
            {
                IPAddress iPAddress = IPAddress.Parse(TextBox_Ip.Text);

                if (_tcpListener == null)
                {
                    IPEndPoint iPEndPoint = new IPEndPoint(iPAddress, int.Parse(TextBox_Port.Text));
                    _tcpListener = new TcpListener(iPEndPoint);

                    _tcpListener.Start();

                    TextBlock_Info.Foreground = Brushes.Green;
                    TextBlock_Info.Text = "Сервер запущен";

                    ButtonStartServer.Content = "Stop Server";

                    GetClientRequestAsync();

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
    }
}
