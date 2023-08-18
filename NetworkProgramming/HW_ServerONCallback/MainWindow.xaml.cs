using HW_Server.Entity;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace HW_ServerONCallback
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly IPAddress IP = IPAddress.Parse("127.0.0.1");
        private static readonly int PORT = 8888;
        private Server _server = new();

        public MainWindow()
        {
            InitializeComponent();

        }

        /// <summary>
        /// Кнопка запустить - остановить сервер
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_ControlServer_Click(object sender, RoutedEventArgs e)
        {
            if (_server._isServerRun == false)
                StartServer();
            else if (_server._isServerRun == true)
                StopServer();

        }

        /// <summary>
        /// Запустить сервер
        /// </summary>
        private async void StartServer()
        {
            ///Изменения интерфейса --------------------------------------  НАЧАЛО
            Button_ControlServer.Background = Brushes.Red;
            Button_ControlServer.Content = "Отключить";
            Label_ServerInf0.Content = "Сервер запущен";
            ///Изменения интерфейса --------------------------------------  КОНЕЦ
            await _server.RunServer(IP, PORT);

        }

        /// <summary>
        /// Остановить сервер
        /// </summary>
        private void StopServer()
        {
            ///Изменения интерфейса --------------------------------------  НАЧАЛО
            Button_ControlServer.Background = Brushes.Green;
            Button_ControlServer.Content = "Включить";
            Label_ServerInf0.Content = "Сервер отключен";
            ///Изменения интерфейса --------------------------------------  КОНЕЦ
            _server.StopServer();
        }





    }
}

