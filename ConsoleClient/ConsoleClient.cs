using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleClient
{
    class ConsoleClient
    {
        private String ip = "127.0.0.1";
        private int port = 8888;
        private NetworkStream networkStream;

        public NetworkStream NetworkStream { get => networkStream; set => networkStream = value; }
        public string IP { get => ip; set => ip = value; }
        public int Port { get => port; set => port = value; }

        static void Main(string[] args)
        {
            ConsoleClient client = new ConsoleClient();
            client.Connect();
            client.SendMesseage("Hello World!");
            Console.ReadKey();
        }

        private void Connect()
        {
            TcpClient connection = new TcpClient(IP, Port);
            NetworkStream = connection.GetStream();
        }

        public void SendMesseage(String messeage)
        {
            var bytes = Encoding.ASCII.GetBytes(messeage);
            NetworkStream.Write(bytes, 0, bytes.Length);
        }

        public int ReadMesseage()
        {
            return 1;
        }
    }
}
