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

namespace FI_NTF_WKR
{
    public partial class Window : Form
    {
        public Window()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            TcpClient connection = new TcpClient("144.217.14.27", 1338);
        }

        private void notify(String title, String description, int timeout)
        {
            timeout *= 1000;

            var notification = new System.Windows.Forms.NotifyIcon()
            {
                Visible = true,
                Icon = System.Drawing.SystemIcons.Information,
                BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info,
                BalloonTipTitle = title,
                BalloonTipText = description,
            };
            notification.ShowBalloonTip(timeout);
            Thread.Sleep(timeout);
            notification.Dispose();
        }
    }
}
