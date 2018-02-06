using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SimpleChat
{
    public partial class Form1 : Form
    {
        private TcpClient server;
        private Thread richTextThread = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                SendMessage(server, textBox1.Text);
                textBox1.Clear();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            String ip = "127.0.0.1";
            int port = 8888;
            this.server = new TcpClient(ip, port);
            NetworkStream networkStream = server.GetStream();
            SendMessage(server, "Hello World!");

            Thread thread = new Thread(() => ReadMessage(server));
            thread.Start();
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
                this.richTextThread = new Thread(() => AppendMessage(message));
                this.richTextThread.Start();
            }
        }


        private void AppendMessage(string text)
        {
            if (this.textBox1.InvokeRequired)
            {
                StringArgReturningVoidDelegate d = new StringArgReturningVoidDelegate(AppendMessage);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.richTextBox1.AppendText(text);
            }
        }

        delegate void StringArgReturningVoidDelegate(string text);
    }
}
