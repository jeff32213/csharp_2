using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;


namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {

        public JObject data = new JObject();

        public Socket client;
        public Socket test;
        public String host;
        public int port;

        public String tmp;


        public MainWindow()
        {
            InitializeComponent();

            //Thread thread = new Thread(Testing);

        }

        public void Testing ()
        {
            while (true)
            {
                try
                {
                    IPAddress ip2 = IPAddress.Parse("127.0.0.1");
                    test = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    test.Connect(new IPEndPoint(ip2, 8888));
                }
                catch
                {

                }
                
            }
        }

        private void Connect(object sender, RoutedEventArgs e)
        {
            data["name"] = input_name.Text;

            host = input_ip.Text;
            port = Int32.Parse(input_port.Text);

            //connect to socket server
            IPAddress ip = IPAddress.Parse(host);
            //socket()
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //connect()
            client.Connect(new IPEndPoint(ip, port));
            status.Text = "server is connected";

            tmp = JsonConvert.SerializeObject(data);
            client.Send(Encoding.ASCII.GetBytes(tmp));

            Thread receiveThread = new Thread(ReceiveMessage);
            receiveThread.Start();

            
        }

        //send()
        private void Send(object sender, RoutedEventArgs e)
        {
            try
            {
                data["msg"] = msg.Text;
                data["status"] = "connect";
                tmp = JsonConvert.SerializeObject(data);

                //send message to server
                client.Send(Encoding.ASCII.GetBytes(tmp));
                msg.Text = "";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                receive_msg.Text += "send Fail\n";
            }
            
        }

        //receive()
        public void ReceiveMessage()
        {
            while (true)
            {
                try
                {
                    byte[] result = new byte[1024];
                    int receiveNumber = client.Receive(result);
                    if(receiveNumber == 0)
                    {
                        client.Shutdown(SocketShutdown.Both);
                        client.Close();
                        break;
                    }
                    String recStr = Encoding.ASCII.GetString(result, 0, receiveNumber);
                    receive_msg.Dispatcher.BeginInvoke(
                           new Action(() => { receive_msg.Text += recStr; }), null);
                }
                catch (Exception ex)
                {
                    //exception close()
                    //client.Shutdown(SocketShutdown.Both);
                    //client.Close();
                    break;
                }
            }

        }

        private void Disconnect(object sender, RoutedEventArgs e)
        {
            try
            {
                data["status"] = "disconnect";
                tmp = JsonConvert.SerializeObject(data);
                client.Send(Encoding.ASCII.GetBytes(tmp));
            }
            catch
            {
                //client.Shutdown(SocketShutdown.Both);
                //client.Close();

            }
           
        }

    }
}
