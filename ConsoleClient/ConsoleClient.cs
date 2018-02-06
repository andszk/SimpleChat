using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ConsoleClient
{
    class ConsoleClient
    {
        private static String ip = "127.0.0.1";
        private static int port = 8888;

        static void Main(string[] args)
        {
            ConsoleClient client = new ConsoleClient();

            TcpClient server = new TcpClient(ip, port);
            NetworkStream networkStream = server.GetStream();
            Thread thread = new Thread(() => client.ReadMessage(server));
            thread.Start();
            client.SendMessage(server, "Hello World!");

            while(true)
            {
                client.SendMessage(server, Console.ReadLine());
            }
        }

        public void SendMessage(TcpClient server, String message)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(message);
            NetworkStream networkStream = server.GetStream();
            networkStream.Write(bytes, 0, bytes.Length);
        }

        public void ReadMessage(TcpClient client)
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
