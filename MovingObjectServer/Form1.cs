using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MovingObject
{
    public partial class Form1 : Form
    {
        Pen red = new Pen(Color.Red);
        Rectangle rect = new Rectangle(20, 20, 30, 30);
        SolidBrush fillBlue = new SolidBrush(Color.Blue);
        int slide = 10;

        private TcpListener server;
        private Thread serverThread;
        private List<TcpClient> clients = new List<TcpClient>();

        public Form1()
        {
            InitializeComponent();
            timer1.Interval = 50;
            timer1.Enabled = true;

            StartServer();
        }

        private void StartServer()
        {
            serverThread = new Thread(() =>
            {
                try
                {
                    server = new TcpListener(IPAddress.Any, 5000);
                    server.Start();
                    while (true)
                    {
                        var client = server.AcceptTcpClient();
                        lock (clients)
                        {
                            clients.Add(client);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Server error: " + ex.Message);
                }
            });
            serverThread.IsBackground = true;
            serverThread.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            back();
            rect.X += slide;
            Invalidate();

            BroadcastPosition();
        }

        private void back()
        {
            if (rect.X >= this.Width - rect.Width * 2)
                slide = -10;
            else if (rect.X <= rect.Width / 2)
                slide = 10;
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.DrawRectangle(red, rect);
            g.FillRectangle(fillBlue, rect);
        }

        private void BroadcastPosition()
        {
            string message = $"{rect.X},{rect.Y}";
            byte[] data = Encoding.UTF8.GetBytes(message);

            lock (clients)
            {
                foreach (var client in clients)
                {
                    try
                    {
                        if (client.Connected)
                        {
                            var stream = client.GetStream();
                            stream.Write(data, 0, data.Length);
                        }
                    }
                    catch
                    {
                        // Jika client error, abaikan
                    }
                }
            }
        }
    }
}
