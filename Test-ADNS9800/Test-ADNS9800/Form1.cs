using System;
using System.Drawing;
using System.IO.Ports;
using System.Text;
using System.Windows.Forms;

namespace Test_ADNS9800
{
    public partial class Form1 : Form
    {
        private SerialPort serialPort;
        private Bitmap currentFrame;
        private int currentRow = 0;
        private const int FrameWidth = 30; // Képkocka szélessége
        private const int FrameHeight = 30; // Képkocka magassága
        private StringBuilder buffer = new StringBuilder(); // Adatbuffer az összefűzéshez

        public Form1()
        {
            InitializeComponent();
            InitializeSerialPort();
            InitializeImage();
        }

        private void InitializeSerialPort()
        {
            serialPort = new SerialPort("COM4", 9600);
            serialPort.DataReceived += SerialPort_DataReceived;
            serialPort.Open();
        }

        private void InitializeImage()
        {
            currentFrame = new Bitmap(FrameWidth, FrameHeight);
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox.Image = currentFrame;
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string data = serialPort.ReadExisting();
            ProcessSerialData(data);
        }

        private void ProcessSerialData(string data)
        {
            if (string.IsNullOrEmpty(data)) return;

            buffer.Append(data); // Új adatok hozzáfűzése a bufferhez

            while (buffer.ToString().Contains("\n"))
            {
                int newlineIndex = buffer.ToString().IndexOf("\n");
                string fullLine = buffer.ToString().Substring(0, newlineIndex).Trim(); // Egy teljes sor kinyerése
                buffer.Remove(0, newlineIndex + 1); // Eltávolítjuk a már feldolgozott sort a bufferből

                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() => listBox1.Items.Add(fullLine)));
                }
                else
                {
                    listBox1.Items.Add(fullLine);
                }

                if (fullLine.Trim() == "#") // Új frame kezdődik
                {
                    currentRow = 0; // Új képkocka kezdődik
                    Invoke(new Action(() =>
                    {
                        pictureBox.Image = new Bitmap(currentFrame); // Új képet állítunk be
                        pictureBox.Refresh();
                    }));
                }
                else if (fullLine.Contains(",")) // Ha egy valódi pixel sor érkezett
                {
                    string[] pixels = fullLine.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    if (pixels.Length == FrameWidth && currentRow < FrameHeight)
                    {
                        for (int col = 0; col < FrameWidth; col++)
                        {
                            if (byte.TryParse(pixels[col], out byte grayscale))
                            {
                                Color color = Color.FromArgb(grayscale, grayscale, grayscale);
                                currentFrame.SetPixel(col, currentRow, color);
                            }
                        }

                        currentRow++; // Következő sor
                    }
                }
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Close();
            }
        }
    }
}
