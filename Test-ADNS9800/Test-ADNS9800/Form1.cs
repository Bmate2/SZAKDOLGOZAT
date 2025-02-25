﻿using System;
using System.Data.Common;
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
        const int gridWidth = 2;             // 20 blokk szélességben
        const int gridHeight = 2;
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
            pictureBox2.SizeMode = PictureBoxSizeMode.Normal;
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string data = serialPort.ReadExisting();
            ProcessSerialData(data);
        }


        private void ProcessSerialData(string data)
        {

            int row = 0;
            int column = 0;
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
                    string[] pixels = fullLine.Substring(6).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (pixels.Length == FrameWidth * FrameHeight)
                    {
                        int[] frameData = Array.ConvertAll(pixels, int.Parse);
                        
                        DisplayFrame(frameData,pictureBox,FrameHeight,FrameWidth);

                        DisplayFrame(BilinearInterpolation(frameData,2), pictureBox2, FrameHeight*2, FrameWidth*2);
                        
                        AddFrameToGrid(row, column, frameData);
                        row++;
                        if (row == gridWidth)
                        {
                            row = 0;
                            column++;
                        }
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

            }
        }

        #region Bicubic Interpolation

        private int[] BilinearInterpolation(int[] framedata,int scale)
        {
            int newWidth = FrameWidth * scale;
            int newHeight = FrameHeight * scale;
            int[] interpolatedData = new int[newWidth * newHeight];

            int[,] original = new int[FrameHeight, FrameWidth];
            for (int y = 0; y < FrameHeight; y++)
            {
                for (int x = 0; x < FrameWidth; x++)
                {
                    original[y, x] = framedata[y * FrameWidth + x];
                }
            }
            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    float srcY = y / (float)scale;
                    float srcX = x / (float)scale;

                    int y0 = (int)Math.Floor(srcY);
                    int x0 = (int)Math.Floor(srcX);

                    float dy = srcY - y0;
                    float dx = srcX - x0;

                    int value = BicubicInterpolate(original, x0, y0, dx, dy);
                    interpolatedData[y * newWidth + x] = Math.Min(Math.Max((int)value, 0), 255);
                }
            }

            return interpolatedData;
        }

        
        private int BicubicInterpolate(int[,] image, int x, int y, float dx, float dy)
        {
            int result = 0;
            for (int m = -1; m <= 2; m++)
            {
                for (int n = -1; n <= 2; n++)
                {
                    int px= Math.Min(Math.Max(x + n, 0), FrameWidth - 1);
                    int py= Math.Min(Math.Max(y + m, 0), FrameHeight - 1);
                    float weight = CubicWeight(n - dx) * CubicWeight(m - dy);
                    result += (int)(image[py, px] * weight);
                }
            }
            return result;
        }

        private float CubicWeight(float x)
        {
            x = Math.Abs(x);
            if (x <= 1) return (1.5f * x * x * x) - (2.5f * x * x) + 1;
            if (x < 2) return (-0.5f * x * x * x) + (2.5f * x * x) - (4 * x) + 2;
            return 0;
        }

        #endregion

        private void AddFrameToGrid(int gridX, int gridY, int[] frameData)
        {
            for (int i = 0; i < FrameWidth * FrameHeight; i++)
            {
                grid[gridX, gridY, i] = frameData[i];
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
                    Color color = Color.FromArgb(pixelValue < 25 ? 0 : (pixelValue > 230) ? 255 : pixelValue, 
                                                 pixelValue < 25 ? 0 : (pixelValue > 230) ? 255 : pixelValue,
                                                 pixelValue < 25 ? 0 : (pixelValue > 230) ? 255 : pixelValue);
                    frameBitmap.SetPixel(x, y, color);
                }
            }
            Bitmap adjusted = AdjustBrightnessContrast(frameBitmap, 50, 50);
            Bitmap unsharped = UnsharpMask(adjusted,2.5f,1,7);

            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    pictureBox.Image = unsharped;
                    pictureBox.Invalidate();
                    pictureBox.Update();
                }));
            }
            else
            {
                pictureBox.Image = unsharped;
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

        

        #region Sharp and Brightness
        private Bitmap AdjustBrightnessContrast(Bitmap image, float brightness, float contrast)
        {
            Bitmap adjustedImage = new Bitmap(image.Width, image.Height);
            float contrastFactor = (100.0f + contrast) / 100.0f;
            contrastFactor *= contrastFactor;

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color pixel = image.GetPixel(x, y);

                    float r = pixel.R / 255.0f;
                    r += brightness / 255.0f;

                    r = (((r - 0.5f) * contrastFactor) + 0.5f) * 255.0f;
                    r = Math.Max(0, Math.Min(255, r));

                    Color newColor = Color.FromArgb((int)r, (int)r, (int)r);
                    adjustedImage.SetPixel(x, y, newColor);
                }
            }

            return adjustedImage;
        }

        private Bitmap UnsharpMask(Bitmap image, float amount, int radius, int threshold)
        {
            Bitmap blurred = new Bitmap(image.Width, image.Height);
            using (Graphics g = Graphics.FromImage(blurred))
            {
                System.Drawing.Imaging.ImageAttributes attributes = new System.Drawing.Imaging.ImageAttributes();
                attributes.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
                g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
            }

            Bitmap sharpened = new Bitmap(image.Width, image.Height);

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color original = image.GetPixel(x, y);
                    Color blur = blurred.GetPixel(x, y);

                    int r = Math.Abs(original.R - blur.R) >= threshold ? (int)(original.R + amount * (original.R - blur.R)) : original.R;
                    int g = Math.Abs(original.G - blur.G) >= threshold ? (int)(original.G + amount * (original.G - blur.G)) : original.G;
                    int b = Math.Abs(original.B - blur.B) >= threshold ? (int)(original.B + amount * (original.B - blur.B)) : original.B;

                    r = Math.Min(255, Math.Max(0, r));
                    g = Math.Min(255, Math.Max(0, g));
                    b = Math.Min(255, Math.Max(0, b));

                    sharpened.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
            }

            return sharpened;
        }

        #endregion

        #region Buttons
        private void startButton_Click(object sender, EventArgs e)
        {
            if (serialPort.IsOpen)
            {
                serialPort.WriteLine("start");
            }
            else 
            { 
                serialPort.Open();
                serialPort.WriteLine("start");
            }
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            if (serialPort.IsOpen)
            {
                serialPort.WriteLine("stop");
                serialPort.Close();
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
                serialPort.Close();
            }
        }
    }
}
