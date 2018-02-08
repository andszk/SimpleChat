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

            int maxClients = 10;
            bool firstClientConnected = false;
            TcpListener listener = new TcpListener(IPAddress.Parse(ip), port);

            try
            {
                listener.Start();
            }
            catch (SocketException e)
            {
                Console.WriteLine("Cannot connect to target socket. " + e.Message);
            }

            while (s.Clients.Count < maxClients)
            {
                if (listener.Pending())
                {
                    TcpClient currentClient = listener.AcceptTcpClient();
                    s.Clients.Add(currentClient);
                    firstClientConnected = true;
                    Thread thread = new Thread(() => s.Run(currentClient));
                    thread.Start();
                }

                if (s.Clients.Count == 0 && firstClientConnected)
                    break;

                Thread.Sleep(10);
            }

            listener.Stop();
            }

        public void Run(TcpClient client)
        {
            string name = ReadMessage(client);
            SendMessage($"User {name} has connected!\n");

            while (IsConnected(client))
            {
                string message = null;

                try
                {
                    if(client.GetStream().DataAvailable)
                        message = ReadMessage(client);
                }
                catch (ObjectDisposedException)
                {
                    clients.Remove(client);
                    SendMessage($"{name} has been disconnected\n");
                    break;
                }

                if (message != null)
                {
                    string formattedMessage = $"({DateTime.Now.ToString("HH:mm:ss")}) {name}: {message}\n";
                    SendMessage(formattedMessage);
                }

                Thread.Sleep(10);
            }

            clients.Remove(client);
            SendMessage($"{name} has been disconnected\n");
        }

        private void SendMessage(string Message)
        {
            Console.WriteLine(Message);
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
            int size = networkStream.Read(buffer, 0, buffer.Length);
            String message = Encoding.ASCII.GetString(buffer, 0, size);
            return message;
        }

        private bool IsConnected(TcpClient client)
        {
            try
            {
                byte[] bytes = new byte[1] { 0 };
                client.GetStream().Write(bytes, 0, 1);
            }
            catch
            {
                return false;
            }

            return true;
        }

        ~Server()
        {
            foreach (var client in clients)
                client.Close();
        }
    }
}
