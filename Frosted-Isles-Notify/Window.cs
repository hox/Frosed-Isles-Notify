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
            new Thread(Client).Start();
        }

        private void Window_FormClosing(object sender, FormClosingEventArgs e)
        {
            client.Close();
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

        void Client()
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
                    MessageBox.Show("Unable to connect", "Connection refused", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                Console.Error.WriteLine(ex.Message);
                return;
            }

            using (NetworkStream stream = client.GetStream())
            {
                Console.WriteLine("Connected!");
                while (stream.CanRead)
                {
                    if (stream.DataAvailable)
                    {
                        byte[] data = ReadData(stream);
                        Console.WriteLine("New data! Content: {0}", data.ToString());
                        stream.Flush();
                        //TODO: Split the data to pass to Notify()
                    }
                }
                Console.WriteLine("Connection closed.");
            }
        }
    }
}
