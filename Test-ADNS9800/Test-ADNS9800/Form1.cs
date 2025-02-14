using System;
using System.Drawing;
using System.IO;
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
        const int gridWidth = 20;             // 20 blokk szélességben
        const int gridHeight = 20;
        int[,,] grid = new int[gridWidth, gridHeight, FrameWidth * FrameHeight];
        public Form1()
        {
            InitializeComponent();
            InitializeSerialPort();
            InitializeImage();
        }
        
        private void InitializeSerialPort()
        {
            serialPort = new SerialPort("COM4", 115200);
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
                    this.Invoke(new Action(() => listBox1.Items.Add($"Beérkező sor: {fullLine}")));
                }
                else
                {
                    listBox1.Items.Add($"Beérkező sor: {fullLine}");
                }

                if (fullLine.StartsWith("FRAME:"))
                {
                    string[] pixels = fullLine.Substring(6).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    if (pixels.Length == FrameWidth * FrameHeight)
                    {
                        int[] frameData = Array.ConvertAll(pixels, int.Parse);
                        DisplayFrame(frameData);
                    }
                    else
                    {
                        if (this.InvokeRequired)
                        {
                            this.Invoke(new Action(() => listBox1.Items.Add($"Hibás FRAME sor hossz: {pixels.Length}")));
                        }
                        else
                        {
                            listBox1.Items.Add($"Hibás FRAME sor hossz: {pixels.Length}");
                        }
                    }
                }







                //if (fullLine.Trim() == "#")
                //{
                //    currentRow = 0;
                //    Invoke(new Action(() =>
                //    {
                //        pictureBox.Image = new Bitmap(currentFrame); 
                //        pictureBox.Invalidate();
                //        pictureBox.Update();
                //    }));
                //}
                //else if (fullLine.StartsWith("MOTION"))
                //{
                //    string coordinates=fullLine.Substring(7);
                    
                //    if (this.InvokeRequired)
                //    {
                //        this.Invoke(new Action(() => listBox1.Items.Add($"Koordináták: {coordinates}")));
                //    }
                //    else
                //    {
                //        listBox1.Items.Add($"Koordináták: {coordinates}");
                //    }



                //    continue;
                //}

                //else if (fullLine.Contains(",")) 
                //{
                //    string[] pixels = fullLine.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                //    if ((pixels.Length == FrameWidth || pixels.Length == FrameWidth - 1) && currentRow < FrameHeight - 1)
                //    {
                //        for (int col = 0; col < FrameWidth - 1; col++)
                //        {
                //            if (int.TryParse(pixels[col], out int grayscale))
                //            {
                //                Color color = Color.FromArgb(grayscale, grayscale, grayscale);
                //                currentFrame.SetPixel(col, currentRow, color);
                //            }
                //        }

                //        if (this.InvokeRequired)
                //        {
                //            currentRow++;
                //            this.Invoke(new Action(() => listBox1.Items.Add($"Új sor: {currentRow}")));
                //        }
                //        else
                //        {
                //            currentRow++;
                //            listBox1.Items.Add($"Új sor: {currentRow}");
                //        }
                //    }
                //    else
                //    {
                //        if (this.InvokeRequired)
                //        {
                //            this.Invoke(new Action(() => listBox1.Items.Add($"Hibás sor hossz: {pixels.Length}")));
                //        }
                //        else
                //        {
                //            listBox1.Items.Add($"Hibás sor hossz: {pixels.Length}");
                //        }
                //    }
                //}
            }
        }
        private void DisplayFrame(int[] frameData)
        {
            for (int row = 0; row < FrameHeight; row++)
            {
                for (int col = 0; col < FrameWidth; col++)
                {
                    int pixelValue = frameData[row * FrameWidth + col];
                    Color color = Color.FromArgb(pixelValue, pixelValue, pixelValue);
                    currentFrame.SetPixel(col, row, color);
                }
            }

            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    pictureBox.Image = new Bitmap(currentFrame);
                    pictureBox.Invalidate();
                    pictureBox.Update();
                }));
            }
            else
            {
                pictureBox.Image = new Bitmap(currentFrame);
                pictureBox.Invalidate();
                pictureBox.Update();
            }
        }

        //private void SaveFrame()
        //{
        //    try
        //    {
        //        string folderPath = Path.Combine(Application.StartupPath, "SavedFrames");
        //        if (!Directory.Exists(folderPath))
        //        {
        //            Directory.CreateDirectory(folderPath);
        //        }

        //        string filePath = Path.Combine(folderPath, $"frame_{frameCounter:D4}.png");
        //        currentFrame.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
        //        frameCounter++; // Következő frame számláló növelése
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Hiba a képfájl mentésekor: {ex.Message}", "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //}

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Close();
            }
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            if (serialPort.IsOpen)
            {
                serialPort.WriteLine("start");
            }
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            if (serialPort.IsOpen)
            {
                serialPort.WriteLine("stop");
            }
        }

        private void deleteListBtn_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
        }
    }
}
