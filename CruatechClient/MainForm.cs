using System;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace CruatechClient
{
    public partial class MainForm : Form
    {
        private bool _connected = false;
        private Socket _client;

        public MainForm()
        {
            InitializeComponent();
        }

        private async void ConnectBtn_Click(object sender, EventArgs e)
        {
            _client = new Socket(
                SocketType.Stream,
                ProtocolType.Tcp);

            await _client.ConnectAsync("localhost", 11111);
            _connected = true;
            while (_connected)
            {
                byte[] byteBuffer = new byte[256];
                ArraySegment<byte> buffer = new ArraySegment<byte>(byteBuffer);

                _ = await _client.ReceiveAsync(buffer, SocketFlags.None);
                var index = 0;
                for (var i = 0; i < 256; i++)
                    if (buffer.Array[i] == 0x01)
                        index = i;

                var initialOffset = index + 2;

                var length = Convert.ToInt16(buffer.Array[index + 1]);
                var checkSum = Convert.ToInt16(buffer.Array[length + initialOffset]);
                var response = Encoding.UTF8.GetString(buffer.Array, initialOffset, length);

                var bufferValues = 0;

                for (var i = initialOffset - 2; i < length + initialOffset; i++)
                    bufferValues += buffer.Array[i];

                var realChecksum = bufferValues & 0xFF;
                if (realChecksum != checkSum)
                    throw new Exception("Data is corrupted");

                ResultRichTextBox.Text = response;
            }

            _client.Shutdown(SocketShutdown.Both);
        }

        private void DisconnectBtn_Click(object sender, EventArgs e)
        {
            _connected = false;
            _client.Shutdown(SocketShutdown.Both);
        }
    }
}
