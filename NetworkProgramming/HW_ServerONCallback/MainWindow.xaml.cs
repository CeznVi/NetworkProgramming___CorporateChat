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
        private TcpListener? _tcpListener;
        private bool isNeedStop = false;


        private static readonly ConcurrentDictionary<string, ServerContext> userDictionary =
            new ConcurrentDictionary<string, ServerContext>();

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
        private async void StartServer()
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

                    await Task.Run(() =>
                    {
                        while (!isNeedStop)
                        {
                            _tcpListener.BeginAcceptTcpClient(
                                new AsyncCallback(DoBeginAcceptTcpClient),
                                _tcpListener
                            );
                        }
                    });
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
                finally
                {
                    StopServer();
                }

        }

        /// <summary>
        /// Остановить сервер
        /// </summary>
        private void StopServer()
        {
            try
            {
                if (_tcpListener != null)
                {
                    isNeedStop = true;
                    _tcpListener.Stop();
                    _tcpListener = null;
                }

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


        private static void DoBeginAcceptTcpClient(IAsyncResult ar)
        {

            TcpListener? listener = ar.AsyncState as TcpListener;

            if (listener == null)
                return;

            ServerContext context = new ServerContext();
            context.Client = listener.EndAcceptTcpClient(ar);
            context.Stream = context.Client.GetStream();

            context.Stream.BeginRead(
                context.Buffer, 0, context.Buffer.Length,
                new AsyncCallback(DoBeginRead),
                context
            );
        }

        private static void DoBeginRead(IAsyncResult ar)
        {
            ServerContext? context = ar.AsyncState as ServerContext;

            if (context == null)
                return;

            try
            {
                int read = context.Stream.EndRead(ar);

                if (read == 0)
                {
                    context.Client.Close();
                    context.Stream.Dispose();
                    context = null;

                    return;
                }

                OnMessageReceived(context);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                context?.Client.Close();
                context?.Stream.Dispose();
                context = null;
            }
            finally
            {
                if (context != null)
                {
                    context.Stream.BeginRead(context.Buffer, 0, context.Buffer.Length, DoBeginRead, context);
                }
            }
        }


        private static void OnMessageReceived(ServerContext context)
        {
            MemoryStream memStream = new MemoryStream(context.Buffer);
            BinaryFormatter formatter = new BinaryFormatter();

    #pragma warning disable SYSLIB0011 // Danger: BinaryFormatter.Deserialize is insecure for untrusted input
            Message m = (Message)formatter.Deserialize(memStream);
    #pragma warning restore SYSLIB0011

            if (m.MT == MessageType.LOGIN)
            {
                if (m.payload == null)
                    return;

                Console.WriteLine($"New user is arrive: {m.payload}");
                context.nickname = m.payload;
                userDictionary.TryAdd(context.nickname, context);

            }
            else if (m.MT == MessageType.MESSAGE)
            {
                if (m.channel == null)
                    return;

                Console.WriteLine($"Message from [{context.nickname}]: {m.payload}");

                if (m.channel == "ALL")
                {
                    foreach (var val in userDictionary)
                    {
                        if (val.Key != context.nickname)
                            memStream.WriteTo(val.Value.Stream);
                    }
                }
                else
                {
                    ServerContext ct = userDictionary[m.channel];
                    if (ct != null)
                        memStream.WriteTo(ct.Stream);
                }
            }

        }


    }
}

