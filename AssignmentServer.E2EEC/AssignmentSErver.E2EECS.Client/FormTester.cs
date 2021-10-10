using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AssignmentSErver.E2EECS.Client
{
    public partial class FormTester : Form
    {
        Socket connectorSocket;
        byte[] buffer = new byte[10240];

        public FormTester()
        {
            InitializeComponent();
        }

        private void UIAccess(Action act)
        {
            this.Invoke(new MethodInvoker(act));
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            var addressParsed = IPAddress.TryParse(serverAddressBox.Text, out var address);
            var portParsed = int.TryParse(serverPortBox.Text, out var port);

            if (!addressParsed)
            {
                var entry = Dns.GetHostEntry(serverAddressBox.Text);
                address = entry.AddressList[0];
            }

            if (!portParsed)
            {
                MessageBox.Show("Wrong port format");
                return;
            }

            var ep = new IPEndPoint(address, port);

            connectorSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            connectorSocket.BeginConnect(ep, ConnectCallback, null);

            connectButton.Enabled = false;
        }

        private void ConnectCallback(IAsyncResult iar)
        {
            try
            {
                connectorSocket.EndConnect(iar);
                UIAccess( () => Text = "CONNECTED" );
            }
            catch
            {
                MessageBox.Show("connect failed");
                UIAccess(() => connectButton.Enabled = true);
            }
        }

        private void ConnectionKilled()
        {
            UIAccess(() =>
            {
                connectorSocket = null;

                connectButton.Enabled = true;
                sendPayloadButton.Enabled = true;

                this.Text = "CONNECTION KILLED!";
            });
        }

        private void sendPayloadButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (connectorSocket?.Connected != true)
                {
                    MessageBox.Show("not connected");
                    return;
                }

                sendPayloadButton.Enabled = false;

                var normalized = senderPayloadBox.Text
                                        .Trim().Replace("\r", string.Empty);
                var bytes = Encoding.UTF8.GetBytes(normalized).ToList();
                bytes.Add(0x00);

                var buff = bytes.ToArray();

                connectorSocket.BeginSend(buff, 0, buff.Length, SocketFlags.None, SendCallback, null);
            }
            catch
            {
                ConnectionKilled();
            }
        }

        private void SendCallback(IAsyncResult iar)
        {
            try
            {
                var sentBytes = connectorSocket.EndSend(iar);

                UIAccess(() => Text = $"{sentBytes} sent");

                connectorSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveCallback, null);
            }
            catch
            {
                ConnectionKilled();
            }
        }

        private void ReceiveCallback(IAsyncResult iar)
        {
            try
            {
                var readResult = connectorSocket.EndReceive(iar);

                if (readResult > 0)
                {
                    var zeroPos = Array.IndexOf<byte>(buffer, 0);
                    var str = Encoding.UTF8.GetString(buffer).Substring(0, zeroPos).Replace("\n", "\r\n").Trim();

                    UIAccess(() =>
                    {
                        receiverBox.AppendText(str.Trim());
                        receiverBox.AppendText("\r\n---- recv end ----\r\n");
                        sendPayloadButton.Enabled = true;
                    });
                }

                connectorSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveCallback, null);
            }
            catch
            {
                ConnectionKilled();
            }
        }
    }
}
