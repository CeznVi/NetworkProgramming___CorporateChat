using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using Client.Entity;


namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Статус подключения
        /// </summary>
        private bool _isConnect = false;
        /// <summary>
        /// Статус коректности введенного имени пользователя в техтбокс
        /// </summary>
        private bool _isCorectNick = false;
        /// <summary>
        /// Шаблон правильного ника для RegeX выражения
        /// </summary>
        private string regexTemplateCorectNick = @"^[A-Za-z0-9_-]{3,15}$";
        /// <summary>
        /// Клиент подключения к серверу
        /// </summary>
        private TcpClient? _client;
        /// <summary>
        /// Адрес сервера
        /// </summary>
        private static readonly IPAddress IP = IPAddress.Parse("127.0.0.1");
        /// <summary>
        /// Порт сервера
        /// </summary>
        private static readonly int PORT = 8888;

        ////// -------------------  КОНСТРУКТОР
        public MainWindow()
        {
            InitializeComponent();
        }

        ////// ------------------ МЕТОДЫ взаимодействия с интерфейсом ================= НАЧАЛО


        private void Button_Connect_Click(object sender, RoutedEventArgs e)
        {
            ///Проверить коректен ли введенный ник
            CheckForCorectNick();


            if (_isConnect == false && _isCorectNick == true)
            {

                Button_Connect.Content = "Отключться";
                Button_Connect.Background = Brushes.Red;
                TextBox_NickName.IsEnabled = false;
                _isConnect = true;
                Connect();

            }
            else if (_isConnect == true && _isCorectNick == true)
            {

                Button_Connect.Content = "Подключиться";
                Button_Connect.Background = Brushes.Green;
                TextBox_NickName.IsEnabled = true;

                _isConnect = false;
            }
        }

        ////// ------------------ МЕТОДЫ взаимодействия с интерфейсом ================= КОНЕЦ


        ////// ------------------ МЕТОДЫ взаимодействия с сервером ================= НАЧАЛО

        /// <summary>
        /// Подключится к серверу
        /// </summary>
        private void Connect()
        {

            //try
            //{
            //    _client = new TcpClient();

            //    IPAddress iPAddress = IPAddress.Parse(_ipAdressSever);

            //    _client.Connect(new IPEndPoint(iPAddress, _portServer));

            //    if (_client.Connected)
            //    {
            //        _isConnect = true;

            //        //Передаем на сервер ник подключенного клиента
            //        ///($"{TextBox_NickName.Text}");


            //    }

            //}
            //catch (FormatException fex)
            //{
            //    MessageBox.Show(fex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //}
            //////////////! ТЕСТ
            //finally
            //{
            //    if (_client.Connected)
            //    {
            //        _client.Close();
            //        _isConnect = false;
            //    }
            //}
        }

        /// <summary>
        /// Проверить коректен ли введенный ник
        /// </summary>
        private void CheckForCorectNick()
        {

            if (Regex.IsMatch(TextBox_NickName.Text, regexTemplateCorectNick))
                _isCorectNick = true;
            else
            {
                MessageBox.Show($"Введенное имя должно состоять от 3 до 15 символов и может содержать английский алфавит, цифры", "Не коректное имя", MessageBoxButton.OK, MessageBoxImage.Error);
                _isCorectNick = false;
            }

        }



        ////// ------------------ МЕТОДЫ взаимодействия с сервером ================= КОНЕЦ


    }
}
