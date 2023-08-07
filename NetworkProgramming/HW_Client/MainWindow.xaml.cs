using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace HW_Client
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
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


            if(_isConnect == false && _isCorectNick == true) 
            {

                Button_Connect.Content = "Отключться";
                Button_Connect.Background = Brushes.Red;
                TextBox_NickName.IsEnabled = false;
                _isConnect = true;
                Connect();
                
            }
            else if(_isConnect == true && _isCorectNick == true) 
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
