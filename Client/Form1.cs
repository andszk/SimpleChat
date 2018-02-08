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
        private bool connected = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void textBoxMessage_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r' && connected)
            {
                SendMessage(server, textBoxMessage.Text);
                textBoxMessage.Clear();
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
                this.server = new TcpClient(textBoxIP.Text, port);
            }
            catch (FormatException)
            {
                richTextBox1.AppendText("Wrong IP or Socket value, try again.\n");
                this.splitContainer1.Panel2Collapsed = false;
                connected = false;
                return;
            }
            catch (SocketException ex)
            {
                richTextBox1.AppendText("Cannot connect to target server. " + ex.Message);
                this.splitContainer1.Panel2Collapsed = false;
                connected = false;
                return;
            }

            NetworkStream networkStream = server.GetStream();
            SendMessage(server, textBoxName.Text);

            readMessageThread = new Thread(() => ReadMessage(server));
            readMessageThread.Start();
            connected = true;
        }

        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            if (connected)
            {
                richTextBox1.AppendText("You have been disconnected.\n");
                readMessageThread.Abort();
                server.GetStream().Close();
                server.Close();
                this.splitContainer1.Panel2Collapsed = false;
                connected = false;
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            buttonDisconnect_Click(sender, e);
        }
    }
}
