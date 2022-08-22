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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Server
{
    public class Client{
        public String Name { get; set; }
        public Socket Socket { get; set; }
        public String Status { get; set; }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //public List<Socket> clients = new List<Socket>(); 
        public List<Client> Clients = new List<Client>();
        //public Dictionary<String, Socket> Clients = new Dictionary<String, Socket>();

        public Socket server;
        public String host;
        public int port;

        public bool Islistening=true;

        public MainWindow()
        {
            InitializeComponent();
        }

        //start a socket server
        private void Start(object sender, RoutedEventArgs e)
        {
            server = null;
            if (server == null)
            {
                host = server_ip.Text;
                port = Int32.Parse(server_port.Text);
                receive_msg.Text = "Server Start\n";
                IPAddress ip = IPAddress.Parse(host);
                //socket()
                server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //bind()
                server.Bind(new IPEndPoint(ip, port));
                //listen()
                server.Listen(10);
                Islistening = true;

                Thread thread = new Thread(Listen);
                thread.Start();
            }
        }

        //listen to socket client
        private void Listen()
        {
            while (Islistening)
            {
                try
                {
                    //accept()
                    Socket client = server.Accept();

                    Thread receive = new Thread(ReceiveMsg);
                    receive.Start(client);
                }
                catch
                {
                    break;
                }
                
            }
        }

        //receive client message and send to client
        public void ReceiveMsg(object client)
        {
            Socket connection = (Socket)client;
            IPAddress clientIP = (connection.RemoteEndPoint as IPEndPoint).Address;

            //send welcome message to client
            byte[] result = new byte[1024];
            int receive_num = connection.Receive(result);
            String receive_str = Encoding.ASCII.GetString(result, 0, receive_num);
            dynamic tmp = JsonConvert.DeserializeObject<dynamic>(receive_str);
            //String name = tmp.name;

            Clients.Add(new Client() { Name = tmp.name, Socket = connection, Status = tmp.status});
            //Clients.Add(name, connection);

            receive_msg.Dispatcher.BeginInvoke(
                new Action(() => { receive_msg.Text += tmp.name + "(" + clientIP + ")" + " connect\n"; }), null);

            if(tmp.name != "test")
                connection.Send(Encoding.ASCII.GetBytes("Welcome " + tmp.name + "\n"));

            while (Islistening)
            {
                try
                {
                    result = new byte[1024];
                    
                    receive_num = connection.Receive(result);
                    //
                    if(receive_num == 0) { break; }
                    receive_str = Encoding.ASCII.GetString(result, 0, receive_num);

                    if (receive_num > 0)
                    {
                        tmp = JsonConvert.DeserializeObject<dynamic>(receive_str);

                        if(tmp.status == "disconnect")
                        {
                            connection.Send(Encoding.ASCII.GetBytes("Disconnect success\n"));
                            receive_msg.Dispatcher.BeginInvoke(
                                                        new Action(() => { receive_msg.Text += tmp.name + " disconnect\n"; }), null);

                            Clients.RemoveAll(a => a.Socket == connection);

                            connection.Shutdown(SocketShutdown.Both);
                            connection.Close();
                            break;
                        }

                        String send_str = tmp.name + "(" + clientIP + ")" + " : " + tmp.msg + "\n";
                           
                        /*
                        if(clients.Count > 1)
                        {
                            for(int i=0; i<clients.Count; i++)
                            {
                                if(clients[i] != connection)
                                {
                                    clients[i].Send(Encoding.ASCII.GetBytes(send_str));
                                }
                                    
                            }
                        }
                        */

                        if (Clients.Count > 1)
                        {
                            foreach(var Client in Clients)
                            {
                                if(Client.Socket != connection )
                                {
                                    Client.Socket.Send(Encoding.ASCII.GetBytes(send_str));
                                }
                            }
                        } 

                        connection.Send(Encoding.ASCII.GetBytes("You send: " + tmp.msg + "\n"));

                        receive_msg.Dispatcher.BeginInvoke(
                            new Action(() => { receive_msg.Text += send_str; }), null);
                        
                    }
                }
                catch (Exception e)
                {
                    //exception close()

                    //connection.Send(Encoding.ASCII.GetBytes("Disconnect success\n"));
                    //receive_msg.Dispatcher.BeginInvoke(
                    //                            new Action(() => { receive_msg.Text += tmp.name + " disconnect\n"; }), null);

                    //Clients.Remove(tmp.name);

                    //Console.WriteLine(e);
                    //connection.Shutdown(SocketShutdown.Both);
                    //connection.Close();
                    break;
                }
            }
        }

        private void Stop(object sender, RoutedEventArgs e)
        {
            try
            {
                Islistening = false;
                //server.Shutdown(SocketShutdown.Both);

                foreach(var client in Clients)
                {
                    
                    client.Socket.Shutdown(SocketShutdown.Both);
                    client.Socket.Close();
                }
                Clients.Clear();

                

                server.Close();

                receive_msg.Dispatcher.BeginInvoke(
                           new Action(() => { receive_msg.Text += "success"; }), null);
            }
            catch
            {
                receive_msg.Dispatcher.BeginInvoke(
                           new Action(() => { receive_msg.Text += "error" ; }), null);
                
            }
            
        }

        //close() when close window
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
