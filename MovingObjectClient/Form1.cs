using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;

namespace MovingObjectClient
{
    public partial class Form1 : Form
    {
        private TcpClient client;
        private Thread receiveThread;
        private int x = 50, y = 50;

        public Form1()
        {
            InitializeComponent();
            ConnectToServer();
            this.DoubleBuffered = true; // Supaya animasi smooth
        }

        private void ConnectToServer()
        {
            try
            {
                client = new TcpClient("127.0.0.1", 5000); // Ganti IP jika server di PC lain
                receiveThread = new Thread(ReceiveData);
                receiveThread.IsBackground = true;
                receiveThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal terhubung ke server: " + ex.Message);
            }
        }

        private void ReceiveData()
        {
            try
            {
                var stream = client.GetStream();
                byte[] buffer = new byte[1024];

                while (true)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        string msg = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        var parts = msg.Split(',');
                        if (parts.Length == 2 &&
                            int.TryParse(parts[0], out int newX) &&
                            int.TryParse(parts[1], out int newY))
                        {
                            x = newX;
                            y = newY;
                            this.Invoke((MethodInvoker)(() => this.Invalidate()));
                        }
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Koneksi ke server terputus.");
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.FillRectangle(Brushes.Blue, x, y, 50, 50);
        }
    }
}
