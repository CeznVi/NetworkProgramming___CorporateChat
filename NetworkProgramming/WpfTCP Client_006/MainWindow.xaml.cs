using System;
using System.Collections.Generic;
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

namespace WpfTCP_Client_006
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


        private void Button_SendMessage_Click(object sender, RoutedEventArgs e)
        {

            TcpClient client = new TcpClient();

            try
            {
                IPAddress iPAddress = IPAddress.Parse(TextBox_Ip.Text);


                client.Connect(new IPEndPoint(iPAddress, int.Parse(TextBox_Port.Text)));

                if (TextBox_Message.Text.Length <= 3)
                {
                    throw new ArgumentException("Заполните сообщение для отправки");
                }

                if (client.Connected)
                {
                    byte[] byteMessage = Encoding.UTF8.GetBytes(TextBox_Message.Text);
                    
                    NetworkStream networkStream = client.GetStream();
                    networkStream.Write(byteMessage, 0, byteMessage.Length);
                    
                    TextBox_Message.Text = String.Empty;
                    client.Close();
                }

            }
            catch (FormatException fex)
            {
                MessageBox.Show("Error", fex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error", ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            ////////////! ТЕСТ
            finally
            {
                if (client.Connected)
                {
                    client.Close();
                }



            }
        }
    }
}
