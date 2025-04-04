using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.IO;
using System.IO.Ports;
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
                        DisplayFrame(frameData, pictureBox, FrameHeight * scale, FrameWidth * scale);

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


        private void DisplayFrame(int[] frameData,PictureBox pictureBox,int height,int width)
        {
            Bitmap frameBitmap = new Bitmap(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int pixelValue = frameData[y * width + x];
                    Color color = Color.FromArgb(pixelValue, pixelValue, pixelValue);
                    frameBitmap.SetPixel(x, y, color);
                }
            }

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




        private Bitmap matrixToBitmap(List<List<int[]>> bigGrid)
        {
            if (bigGrid.Count == 0 || bigGrid[0].Count == 0 || bigGrid[0][0] == null)
            {
                MessageBox.Show("A mátrix üres, nem lehet képet generálni.", "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            
            
            int width = bigGrid[0].Count*FrameWidth* scale;
            
            int height = bigGrid.Count*FrameHeight*scale;
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => MessageBox.Show(this, "Height: " + height + "\nWidth: " + width)));
            }
            else
            {
                MessageBox.Show(this, "Height: " + height + "\nWidth: " + width);
            }
                Bitmap bmp = new Bitmap(width, height);
            for (int row = 0; row < bigGrid.Count; row++)
            {
                for (int col = 0; col < bigGrid[row].Count; col++)
                {
                    int[] frameData = bigGrid[row][col];

                    int startX = col * FrameWidth*scale;
                    int startY = row * FrameHeight*scale;

                    for (int y = 0; y < FrameHeight*scale; y++)
                    {
                        for (int x = 0; x < FrameWidth*scale; x++)
                        {
                            int pixelValue = frameData[y * (FrameWidth*scale) + x];
                            Color color = Color.FromArgb(pixelValue, pixelValue, pixelValue);
                            bmp.SetPixel(startX + x, startY + y, color);
                        }
                    }
                }
            }
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

                    builder.InsertImage(matrixToBitmap(listGrid));
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
                    matrixToBitmap(listGrid).Save(filePath + "\\frame.png", System.Drawing.Imaging.ImageFormat.Png);
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
