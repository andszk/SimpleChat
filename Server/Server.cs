using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Server
    {
        private String ip = "127.0.0.1";
        private int port = 8888;
        private NetworkStream networkStream;
        private TcpClient client;
        private TcpListener listener;


        public string IP { get => ip; set => ip = value; }
        public int Port { get => port; set => port = value; }
        public NetworkStream NetworkStream { get => networkStream; set => networkStream = value; }
        public TcpClient Client { get => client; set => client = value; }
        public TcpListener Listener { get => listener; set => listener = value; }

        ~Server()
        {
            Listener.Stop();
            client.Close();
        }
        static void Main(string[] args)
        {
            Server server = new Server();
            server.StartService();
            while (true)
            {
                Console.WriteLine(server.GetMessage());
            }
        }

        private void StartService()
        {
            Listener = new TcpListener(IPAddress.Parse(IP), port);
            Listener.Start();
            Client = listener.AcceptTcpClient();
        }

        public String GetMessage()
        {
            NetworkStream = Client.GetStream();
            byte[] buffer = new byte[Client.ReceiveBufferSize];
            var bytes = NetworkStream.Read(buffer, 0, buffer.Length);
            return String.Format("{0} {1}",
                DateTime.Now.ToString("HH:mm:ss"),
                Encoding.ASCII.GetString(buffer, 0, bytes));
        }
    }
}
