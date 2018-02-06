using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    class Server
    {
        private static String ip = "127.0.0.1";
        private static int port = 8888;
        private List<TcpClient> clients = new List<TcpClient>();

        public List<TcpClient> Clients { get => clients; set => clients = value; }

        static void Main(string[] args)
        {
            Server s = new Server();

            int threadCount = 10;
            TcpListener listener = new TcpListener(IPAddress.Parse(ip), port);
            listener.Start();

            do
            {
                if (listener.Pending())
                {
                    TcpClient currentClient = listener.AcceptTcpClient();
                    s.Clients.Add(currentClient);
                    Thread thread = new Thread(() => s.Run(currentClient));
                    thread.Start();
                }
            } while (s.Clients.Count <= threadCount);

            listener.Stop();
        }

        public void Run(TcpClient client)
        {
            while (true)
            {
                NetworkStream networkStream = client.GetStream();
                byte[] buffer = new byte[client.ReceiveBufferSize];
                int bytes = networkStream.Read(buffer, 0, buffer.Length);
                String message = Encoding.ASCII.GetString(buffer, 0, bytes);
                string formattedMessage = String.Format("{0} {1}\n",
                    DateTime.Now.ToString("HH:mm:ss"),
                    message);

                if (message != String.Empty)
                {
                    foreach (var c in clients)
                        SendMessage(c, formattedMessage);
                    Console.WriteLine(formattedMessage);
                }
            }
        }

        private void SendMessage(TcpClient client, string message)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(message);
            NetworkStream networkStream = client.GetStream();
            networkStream.Write(bytes, 0, bytes.Length);
        }

        ~Server()
        {
            foreach (var client in clients)
                client.Close();
        }
    }
}
