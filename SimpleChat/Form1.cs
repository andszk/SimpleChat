using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace SimpleChat
{
    public partial class Form1 : Form
    {
        private TcpClient server;
        private Thread richTextThread = null;
        private Thread readMessageThread = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                SendMessage(server, textBoxMessage.Text);
                textBoxMessage.Clear();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        public void SendMessage(TcpClient server, String message)
        {
            //TODO check if stream exists
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
                int size = networkStream.Read(buffer, 0, buffer.Length);
                if (buffer[0] == 0)
                    continue;
                String message = Encoding.ASCII.GetString(buffer, 0, size);
                this.richTextThread = new Thread(() => AppendMessageThreadSafe(message));
                this.richTextThread.Start();
            }
        }

        private void AppendMessageThreadSafe(string text)
        {
            if (this.textBoxMessage.InvokeRequired)
            {
                StringArgReturningVoidDelegate d = new StringArgReturningVoidDelegate(AppendMessageThreadSafe);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.richTextBox1.AppendText(text);
            }
        }

        delegate void StringArgReturningVoidDelegate(string text);

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            this.splitContainer1.Panel2Collapsed = true;
            int port = 0;
            try
            {
                IPAddress.Parse(textBoxIP.Text);
                Int32.TryParse(textBoxPort.Text, out port);
            }
            catch (FormatException)
            {
                //TODO
            }

            this.server = new TcpClient(textBoxIP.Text, port);
            NetworkStream networkStream = server.GetStream();
            SendMessage(server, textBoxName.Text);

            readMessageThread = new Thread(() => ReadMessage(server));
            readMessageThread.Start();
        }

        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            readMessageThread.Abort();
            server.GetStream().Close();
            server.Close();
            this.splitContainer1.Panel2Collapsed = false;
        }
    }
}
