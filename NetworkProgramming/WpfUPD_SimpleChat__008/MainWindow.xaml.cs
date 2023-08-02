using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfUPD_SimpleChat__008
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        public MainWindow()
        {
            InitializeComponent();

            

        }

        private async void GetMessagesAsync(int port)
        {
            await Task.Run(() =>
            {

            try
            {
                while (true)
                {

                    UdpClient udpClient = new UdpClient(port);
                    IPEndPoint iPEndPoint = null;
                    byte[] request = udpClient.Receive(ref iPEndPoint);

                    Dispatcher.Invoke(() =>
                    {
                        ListBox_Chat.Items.Add($"From {udpClient.Client.RemoteEndPoint}: " + Encoding.UTF8.GetString(request));

                    });
                        udpClient.Close();
                    }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error", ex.Message, MessageBoxButton.OK, MessageBoxImage.Error); return;

                    }

            });
        }

        private void Btn_sendMessage_Click(object sender, RoutedEventArgs e)
        {
            UdpClient udpClient = null;
            
            try
            {
                udpClient = new UdpClient();

                IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse(TextBox_RemoteIp.Text), int.Parse(TextBox_RemotePort.Text));


                if (TextBox_Message.Text.Length < 3)
                    throw new ArgumentException("Insert Message Please");
                
                byte[] message = Encoding.UTF8.GetBytes(TextBox_Message.Text);
                
                udpClient.Send(message, message.Length, iPEndPoint);

                TextBox_Message.Text = String.Empty;

            }
            catch (Exception ex)
            { 
                MessageBox.Show("Error", ex.Message, MessageBoxButton.OK, MessageBoxImage.Error); return;
            }
            finally
            {
                udpClient.Close();
            }



        }

        private void Btn_Listen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int localport = int.Parse(TextBox_LocalPort.Text);
                TextBox_LocalPort.IsReadOnly = true;

                GetMessagesAsync(localport);
                
            }
            catch (Exception ex) 
            {
                MessageBox.Show("Error", "Insert port number", MessageBoxButton.OK, MessageBoxImage.Error); return;
            }
            
            

            
        }
    }
}
