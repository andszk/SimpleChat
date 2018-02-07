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

            Thread.CurrentThread.Join();

            listener.Stop();
        }

        public void Run(TcpClient client)
        {
            string name = ReadMessage(client);
            SendMessage($"Hello {name}!\n");

            while (true)
            {
                string message = null;

                try
                {
                    message = ReadMessage(client);
                }
                catch (System.IO.IOException)
                {
                    clients.Remove(client);
                    SendMessage($"{name} has been disconnected\n");
                    break;
                }

                string formattedMessage = $"({DateTime.Now.ToString("HH:mm:ss")}) {name}: {message}\n";

                if (message != String.Empty)
                {
                    foreach (var c in clients)
                        SendMessage(c, formattedMessage);
                    Console.WriteLine(formattedMessage);
                }
            }
        }

        private void SendMessage(string Message)
        {
            foreach (var c in clients)
                SendMessage(c, Message);
        }

        private void SendMessage(TcpClient client, string message)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(message);
            NetworkStream networkStream = client.GetStream();
            networkStream.Write(bytes, 0, bytes.Length);
        }

        private string ReadMessage(TcpClient client)
        {
            NetworkStream networkStream = client.GetStream();
            byte[] buffer = new byte[client.ReceiveBufferSize];
            int bytes = networkStream.Read(buffer, 0, buffer.Length);
            String message = Encoding.ASCII.GetString(buffer, 0, bytes);
            return message;
        }

        ~Server()
        {
            foreach (var client in clients)
                client.Close();
        }
    }
}
