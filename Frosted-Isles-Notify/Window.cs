using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace FI_NTF_WKR
{
    public partial class Window : Form
    {
        private TcpClient client;
        public Window()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            new Thread(StartClient).Start();
        }

        private void Window_FormClosing(object sender, FormClosingEventArgs e)
        {
            client.Close();
            notification.Dispose();
        }

        private void Notify(string title, string description, int timeout)
        {
            timeout *= 1000;

            notification.BalloonTipTitle = title;
            notification.BalloonTipText = description;

            notification.ShowBalloonTip(timeout);
            Thread.Sleep(timeout);
        }

        public static byte[] ReadData(Stream stream)
        {
            byte[] buffer = new byte[32768];
            using (MemoryStream ms = new MemoryStream()) {
                while (true)
                {
                    int r = stream.Read(buffer, 0, buffer.Length);

                    if (r <= 0)
                    {
                        return ms.ToArray();
                    }
                    ms.Write(buffer, 0, r);
                }
            }
        }

        void StartClient()
        {
            client = new TcpClient();
            try
            {
                client.Connect("frostedisles.ddns.net", 1338); //158.69.60.8
            }
            catch(Exception ex)
            {
                if (ex is SocketException)
                {
                    notification.ShowBalloonTip(5, "Unable to connect", "Connection refused", ToolTipIcon.Error);
                }

                contextMenu.Items.Find("toolStripConnect", true)[0].Enabled = true;
                contextMenu.Items.Find("toolStripDisconnect", true)[0].Enabled = false;

                Console.Error.WriteLine(ex.Message);
                return;
            }

            using (NetworkStream stream = client.GetStream())
            {
                Console.WriteLine("Connected!");
                notification.ShowBalloonTip(5, "Connected", "Connection established with Frosted Isles", ToolTipIcon.Info);

                //contextMenu.Items.Find("toolStripConnect", true)[0].Enabled = false;
                //contextMenu.Items.Find("toolStripDisconnect", true)[0].Enabled = true;

                while (stream.CanRead)
                {
                    try
                    {
                        stream.WriteByte(0); //Ping the server to keep connection alive

                        if (stream.DataAvailable)
                        {
                            byte[] data = ReadData(stream);
                            Console.WriteLine("New data! Content: {0}", data.ToString());
                            stream.Flush();
                            //TODO: Split the data to pass to Notify()
                            Notify("New data recieved!", data.ToString(), 5);
                        }
                    }
                    catch { break; }
                }
                Console.WriteLine("Connection closed.");

                //contextMenu.Items.Find("toolStripConnect", true)[0].Enabled = true;
                //contextMenu.Items.Find("toolStripDisconnect", true)[0].Enabled = false;
            }
        }

        private void contextMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            switch (e.ClickedItem.Text)
            {
                case "Exit":
                    client.Close();
                    Close();
                    break;
                case "Connect":
                    if (!client.Connected) {
                        new Thread(StartClient).Start();
                    }
                    break;
                case "Disconnect":
                    if (client.Connected)
                    {
                        const string word = "DISCONNECT";
                        byte[] buffer = new byte[10];
                        for (int i = 0; i < 10; i++)
                        {
                            buffer[i] = Convert.ToByte(word[i]);
                        }

                        client.GetStream().Write(buffer, 0, 10);
                        client.Close();
                    }
                    contextMenu.Items.Find("toolStripConnect", true)[0].Enabled = true;
                    e.ClickedItem.Enabled = false;

                    break;
            }
        }
    }
}
