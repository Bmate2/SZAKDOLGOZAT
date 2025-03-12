using System;
using System.Collections.Generic;
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
        private const int FrameWidth = 30; // Képkocka szélessége
        private const int FrameHeight = 30; // Képkocka magassága
        private StringBuilder buffer = new StringBuilder(); // Adatbuffer az összefűzéshez
        private Bicubic resizer = new Bicubic(30, 30, 2);

        int row = 0;
        int column = 0;


        private List<List<int[]>> listGrid = new List<List<int[]>>();
        
        public Form1()
        {
            InitializeComponent();
            InitializeSerialPort();
            InitializeImage();
            listGrid.Add(new List<int[]>()); //első sor hozzáadása
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

                        int[] upscaled = resizer.BicubicResize(frameData);

                        DisplayFrame(upscaled, pictureBox2, FrameHeight*2, FrameWidth*2);

                        

                        listGrid[row].Add(upscaled);
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
                if (fullLine.StartsWith("NEW_ROW"))
                {
                    row++;
                    listGrid.Add(new List<int[]>());

                }
                if (fullLine.StartsWith("END"))
                {
                    serialPort.WriteLine("stop");
                    SaveDoc();
                }
            }

        }

        //#region Bicubic Interpolation

        //private int[] BicubicInterpolation(int[] framedata,int scale)
        //{
        //    int newWidth = FrameWidth * scale;
        //    int newHeight = FrameHeight * scale;
        //    int[] interpolatedData = new int[newWidth * newHeight];

        //    int[,] original = new int[FrameHeight, FrameWidth];
        //    for (int y = 0; y < FrameHeight; y++)
        //    {
        //        for (int x = 0; x < FrameWidth; x++)
        //        {
        //            original[y, x] = framedata[y * FrameWidth + x];
        //        }
        //    }
        //    for (int y = 0; y < newHeight; y++)
        //    {
        //        for (int x = 0; x < newWidth; x++)
        //        {
        //            float srcY = y / (float)scale;
        //            float srcX = x / (float)scale;

        //            int y0 = (int)Math.Floor(srcY);
        //            int x0 = (int)Math.Floor(srcX);

        //            float dy = srcY - y0;
        //            float dx = srcX - x0;

        //            int value = 0;
        //            for (int m = -1; m <= 2; m++)
        //            {
        //                for (int n = -1; n <= 2; n++)
        //                {
        //                    int px = Reflect(x0 + n, FrameWidth);
        //                    int py = Reflect(y0 + m, FrameHeight);
        //                    float weight = CubicWeight(dx - n) * CubicWeight(dy - m);
        //                    value += (int)(original[py, px] * weight);
        //                }
        //            }
        //            interpolatedData[y * newWidth + x] = Math.Min(Math.Max((int)value, 0), 255);
        //        }
        //    }
        //    return interpolatedData;
        //}

        //private float CubicWeight(float x)
        //{
        //    x = Math.Abs(x);
        //    if (x <= 1) return (1.5f * x * x * x) - (2.5f * x * x) + 1;
        //    if (x < 2) return (-0.5f * x * x * x) + (2.5f * x * x) - (4 * x) + 2;
        //    return 0;
        //}

        //int Reflect(int value, int max)
        //{
        //    if (value < 0) return -value;
        //    if (value >= max) return 2 * max - value - 1;
        //    return value;
        //}

        //#endregion


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




        private Bitmap matrixToBitmap(List<List<int[]>> bigGrid)
        {
            if (bigGrid.Count == 0 || bigGrid[0].Count == 0 || bigGrid[0][0] == null)
            {
                MessageBox.Show("A mátrix üres, nem lehet képet generálni.", "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            
            
            int width = bigGrid[0].Count*FrameWidth*2;
            MessageBox.Show("Width: " + width);
            int height = bigGrid.Count*FrameHeight*2;
            MessageBox.Show("Height: " + height);

            Bitmap bmp = new Bitmap(width, height);
            for (int row = 0; row < bigGrid.Count; row++)
            {
                for (int col = 0; col < bigGrid[row].Count; col++)
                {
                    int[] frameData = bigGrid[row][col];

                    int startX = col * FrameWidth*2;
                    int startY = row * FrameHeight*2;

                    for (int y = 0; y < FrameHeight*2; y++)
                    {
                        for (int x = 0; x < FrameWidth*2; x++)
                        {
                            int pixelValue = frameData[y * (FrameWidth*2) + x]; // Az adott pixel értéke a FRAME-ből
                            Color color = Color.FromArgb(pixelValue, pixelValue, pixelValue);

                            // Beírjuk a megfelelő helyre a képen
                            bmp.SetPixel(startX + x, startY + y, color);
                        }
                    }
                }
            }
                return bmp;
        }

        private void SaveDoc()
        {
            try
            {
                string folderPath = Path.Combine(Application.StartupPath, "SavedFrames");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string filePath = Path.Combine(folderPath);
                matrixToBitmap(listGrid).Save(filePath + "\\frame.png", System.Drawing.Imaging.ImageFormat.Png);
                MessageBox.Show("A kép mentése sikeresen megtörtént.", "Siker", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba a képfájl mentésekor: {ex.Message}", "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



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

        private void resetBtn_Click(object sender, EventArgs e)
        {
            if (serialPort.IsOpen)
            {
                serialPort.WriteLine("reset");
                listBox1.Items.Clear();
            }
        }
    }
}
