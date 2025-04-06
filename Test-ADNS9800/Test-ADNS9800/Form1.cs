using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Aspose.Words;

namespace Test_ADNS9800
{
    public partial class Form1 : Form
    {
        private SerialPort serialPort;
        private const int FrameWidth = 30; 
        private const int FrameHeight = 30; 
        private StringBuilder dataBuffer = new StringBuilder();
        private const int scale = 2;
        private Bicubic resizer = new Bicubic(FrameWidth, FrameHeight, scale);

        int row = 0;
        int column = 0;

        private List<List<int[]>> listGrid = new List<List<int[]>>();
        public Form1()
        {
            InitializeComponent();
            InitializeSerialPort();
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            listGrid.Add(new List<int[]>());
        }
        
        private void InitializeSerialPort()
        {
            serialPort = new SerialPort("COM3", 115200);
            serialPort.DataReceived += DataReceived; 
            serialPort.Open();
            serialPort.Write("reset");
        }

        private void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string data = serialPort.ReadExisting();
            ProcessSerialData(data);
        }


        private void ProcessSerialData(string data)
        {

            if (string.IsNullOrEmpty(data))
            {
                MessageBox.Show("A beérkezett adat üres", "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            dataBuffer.Append(data); 
            string bufferString = dataBuffer.ToString();
            int newlineIndex;
            while ((newlineIndex=bufferString.IndexOf("\n"))!= -1)
            {
                string fullLine = bufferString.Substring(0, newlineIndex).Trim(); 
                dataBuffer.Remove(0, newlineIndex + 1);
                bufferString = bufferString.Substring(newlineIndex + 1);
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() => listBox1.Items.Add("Beérkező sor: "+fullLine)));
                }
                else
                {
                    listBox1.Items.Add("Beérkező sor: " + fullLine);
                }

                if (fullLine.StartsWith("FRAME:"))
                {
                    string[] pixels = fullLine.Substring(6).Split(new[] { ' ' });

                    if (pixels.Length == FrameWidth * FrameHeight)
                    {
                        int[] frameData = resizer.BicubicResize(Array.ConvertAll(pixels, int.Parse));
                        DisplayFrame(frameData, FrameHeight * scale, FrameWidth * scale);

                        if (row >= listGrid.Count)
                        {
                            listGrid.Add(new List<int[]>());
                        }
                        listGrid[row].Add(frameData);
                    }
                    else
                    {
                        if (this.InvokeRequired)
                        {
                            this.Invoke(new Action(() => listBox1.Items.Add("Hibás FRAME sor hossz: "+pixels.Length)));
                        }
                        else
                        {
                            listBox1.Items.Add("Hibás FRAME sor hossz: "+pixels.Length);
                        }
                    }
                    
                }
                if (fullLine.StartsWith("NEW_ROW"))
                {
                    row++;

                }
                if (fullLine.StartsWith("END"))
                {
                    serialPort.WriteLine("stop");
                    SaveDoc();
                }
            }
        }


        private void DisplayFrame(int[] frameData, int height,int width)
        {
            Bitmap frameBitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            Rectangle rect = new Rectangle(0, 0, width, height);
            BitmapData bmpData = frameBitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int stride = bmpData.Stride;
            byte[] bytes = new byte[stride * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int pixelValue = (int)(frameData[y * width + x] * 1.15);
                    int offset = y * stride + x * 3;

                    bytes[offset] = (byte)pixelValue;         
                    bytes[offset + 1] = (byte)pixelValue;     
                    bytes[offset + 2] = (byte)pixelValue;     
                }
            }

            Marshal.Copy(bytes, 0, bmpData.Scan0, bytes.Length);
            frameBitmap.UnlockBits(bmpData);

            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    pictureBox.Image = frameBitmap;
                    pictureBox.Invalidate();
                    pictureBox.Update();
                }));
            }
            else
            {
                pictureBox.Image = frameBitmap;
                pictureBox.Invalidate();
                pictureBox.Update();
            }
        }




        private Bitmap matrixToBitmap()
        {
            if (listGrid.Count == 0 || listGrid[0].Count == 0 || listGrid[0][0] == null)
            {
                MessageBox.Show("A mátrix üres, nem lehet képet generálni.", "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            int frameWidth = FrameWidth * scale;
            int frameHeight = FrameHeight * scale;
            int width = listGrid[0].Count * frameWidth;
            int height = listGrid.Count * frameHeight;

            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            Rectangle rect = new Rectangle(0, 0, width, height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.WriteOnly, bmp.PixelFormat);

            int stride = bmpData.Stride;
            byte[] bytes = new byte[stride * height];

            for (int row = 0; row < listGrid.Count; row++)
            {
                for (int col = 0; col < listGrid[row].Count; col++)
                {
                    int[] frameData = listGrid[row][col];

                    int startX = col * frameWidth;
                    int startY = row * frameHeight;

                    for (int y = 0; y < frameHeight; y++)
                    {
                        for (int x = 0; x < frameWidth; x++)
                        {
                            int pixelValue = (int)(frameData[y * frameWidth + x] * 1.15); 

                            int globalX = startX + x;
                            int globalY = startY + y;

                            int offset = globalY * stride + globalX * 3;

                            bytes[offset] = (byte)pixelValue;       
                            bytes[offset + 1] = (byte)pixelValue;   
                            bytes[offset + 2] = (byte)pixelValue;     
                        }
                    }
                }
            }

            Marshal.Copy(bytes, 0, bmpData.Scan0, stride * height);
            bmp.UnlockBits(bmpData);
            return bmp;
        }


        private void SaveDoc()
        {
            string folderPath = Path.Combine(Application.StartupPath, "SavedFrames");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string filePath = Path.Combine(folderPath);
            try 
            {
                if (pdfCheckBox.Checked)
                {
                    var doc = new Document();
                    var builder = new DocumentBuilder(doc);

                    builder.InsertImage(matrixToBitmap());
                    doc.Save(filePath+"\\Document.pdf");
                    if (this.InvokeRequired)
                    {
                        this.Invoke(new Action(() => MessageBox.Show("A pdf mentése sikeresen megtörtént.", "Siker", MessageBoxButtons.OK, MessageBoxIcon.Information)));
                    }
                    else
                    {
                        MessageBox.Show("A pdf mentése sikeresen megtörtént.", "Siker", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    matrixToBitmap().Save(filePath + "\\frame.png", System.Drawing.Imaging.ImageFormat.Png);
                    if (this.InvokeRequired)
                    {
                        this.Invoke(new Action(() => MessageBox.Show("A kép mentése sikeresen megtörtént.", "Siker", MessageBoxButtons.OK, MessageBoxIcon.Information)));
                    }
                    else
                    {
                        MessageBox.Show("A kép mentése sikeresen megtörtént.", "Siker", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba a képfájl mentésekor: {ex.Message}", "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



     

        #region Buttons
        private void resetBtn_Click(object sender, EventArgs e)
        {
            if (serialPort.IsOpen)
            {
                serialPort.WriteLine("reset");
                listBox1.Items.Clear();
                listGrid.Clear();
                row = 0;
                column = 0;
                pdfCheckBox.Checked = false;
                resizer = new Bicubic(FrameWidth, FrameHeight, scale);
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

        #endregion
        
        
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Write("stop");
                serialPort.Close();
            }
        }

        
    }
}
