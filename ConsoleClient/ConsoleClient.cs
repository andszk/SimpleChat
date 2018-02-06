using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleClient
{
    class ConsoleClient
    {
        private String ip = "127.0.0.1";
        private int port = 8888;
        private NetworkStream networkStream;
        private TcpClient connection;

        public TcpClient Connection { get => connection; set => connection = value; }
        public NetworkStream NetworkStream { get => networkStream; set => networkStream = value; }
        public string IP { get => ip; set => ip = value; }
        public int Port { get => port; set => port = value; }

        static void Main(string[] args)
        {
            ConsoleClient client = new ConsoleClient();
            client.Connect();

            Thread thread = new Thread(() => client.Readmessage(client.Connection));
            thread.Start();
            client.SendMessage("Hello World!");

            while(true)
            {
                client.SendMessage(Console.ReadLine());
            }
        }

        private void Connect()
        {
            Connection = new TcpClient(IP, Port);
            NetworkStream = connection.GetStream();
        }

        public void SendMessage(String message)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(message);
            NetworkStream.Write(bytes, 0, bytes.Length);
        }

        public void Readmessage(TcpClient client)
        {
            while (true)
            {
                NetworkStream networkStream = client.GetStream();
                byte[] buffer = new byte[client.ReceiveBufferSize];
                int bytes = networkStream.Read(buffer, 0, buffer.Length);
                String message = Encoding.ASCII.GetString(buffer, 0, bytes);
                Console.WriteLine(message);
            }
        }
    }
}
