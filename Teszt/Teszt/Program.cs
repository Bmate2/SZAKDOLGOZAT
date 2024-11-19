using System;
using System.Drawing;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;
using System.Threading;
namespace Teszt
{
    internal class Program
    {
        public static Bitmap MatrixToBitmap(double[,,] matrix)
        {
            int height = matrix.GetLength(0);
            int width = matrix.GetLength(1);
            Bitmap bitmap = new Bitmap(width, height);

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    double r = matrix[i, j, 0];
                    double g = matrix[i, j, 1];
                    double b = matrix[i, j, 2];
                    Color color = Color.FromArgb((int)r, (int)g, (int)b);
                    bitmap.SetPixel(j, i, color);
                }
            }

            return bitmap;
        }
        
        public static double[,,] GenerateRandomRGBMatrix(int width, int height)
        {
            Random random = new Random();
            double[,,] matrix = new double[height, width, 3]; // 3 dimenzió: magasság, szélesség, RGB

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    matrix[i, j, 0] = random.NextDouble() * 255; // R érték
                    matrix[i, j, 1] = random.NextDouble() * 255; // G érték
                    matrix[i, j, 2] = random.NextDouble() * 255; // B érték
                }
            }

            return matrix;
        }
        public static double[,] GenerateRandomMatrix(int width, int height)
        {
            Random random = new Random();
            double[,] matrix = new double[height, width]; 

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    matrix[i, j] = random.NextDouble() * 255; 
                }
            }

            return matrix;
        }

        //public static double[,] Bilinear(double[,] originalMatrix)
        //{
        //    int originalHeight = originalMatrix.GetLength(0);
        //    int originalWidth = originalMatrix.GetLength(1);
        //    double[,] scaledMatrix = new double[originalHeight * 2, originalWidth * 2];

        //    for (int i = 0; i < originalHeight; i++)
        //    {
        //        for (int j = 0; j < originalWidth; j++)
        //        {
        //            int x = i * 2;
        //            int y = j * 2;
        //            scaledMatrix[x,y] = originalMatrix[i, j];

        //            if (j+1 < originalWidth)
        //            {
        //                scaledMatrix[x, y + 1] = originalMatrix[i,j+1]/2;
        //            }
        //            if(i+1 < originalHeight)
        //            {
        //                scaledMatrix[x+1, y] = originalMatrix[i+1, j] / 2;
        //            }
        //            if (i + 1 < originalHeight && j + 1 < originalWidth)
        //            {
        //                scaledMatrix[x + 1, y + 1] = (originalMatrix[i, j] + originalMatrix[i, j + 1] + originalMatrix[i + 1, j] + originalMatrix[i + 1, j + 1]) / 4;
        //            }

        //        }

        //    }
        //    return scaledMatrix;

        //}

        //public static double[,,] InterpolateRGBMatrix(double[,,] matrix)
        //{
        //    int originalRows = matrix.GetLength(0);
        //    int originalCols = matrix.GetLength(1);
        //    int newRows = originalRows * 2 - 1;
        //    int newCols = originalCols * 2 - 1;

        //    double[,,] result = new double[newRows, newCols, 3];

        //    for (int i = 0; i < newRows; i++)
        //    {
        //        for (int j = 0; j < newCols; j++)
        //        {
        //            // Eredeti mátrix indexek
        //            int rowBase = i / 2;
        //            int colBase = j / 2;

        //            if (i % 2 == 0 && j % 2 == 0)
        //            {
        //                // Másoljuk az eredeti mátrix elemeit
        //                result[i, j, 0] = matrix[rowBase, colBase, 0];
        //                result[i, j, 1] = matrix[rowBase, colBase, 1];
        //                result[i, j, 2] = matrix[rowBase, colBase, 2];
        //            }
        //            else if (i % 2 == 0)
        //            {
        //                // Soron belüli interpoláció
        //                result[i, j, 0] = (matrix[rowBase, colBase, 0] + matrix[rowBase, colBase + 1, 0]) / 2.0;
        //                result[i, j, 1] = (matrix[rowBase, colBase, 1] + matrix[rowBase, colBase + 1, 1]) / 2.0;
        //                result[i, j, 2] = (matrix[rowBase, colBase, 2] + matrix[rowBase, colBase + 1, 2]) / 2.0;
        //            }
        //            else if (j % 2 == 0)
        //            {
        //                // Oszlopon belüli interpoláció
        //                result[i, j, 0] = (matrix[rowBase, colBase, 0] + matrix[rowBase + 1, colBase, 0]) / 2.0;
        //                result[i, j, 1] = (matrix[rowBase, colBase, 1] + matrix[rowBase + 1, colBase, 1]) / 2.0;
        //                result[i, j, 2] = (matrix[rowBase, colBase, 2] + matrix[rowBase + 1, colBase, 2]) / 2.0;
        //            }
        //            else
        //            {
        //                // Négy elem átlaga
        //                result[i, j, 0] = (matrix[rowBase, colBase, 0] + matrix[rowBase, colBase + 1, 0]
        //                                + matrix[rowBase + 1, colBase, 0] + matrix[rowBase + 1, colBase + 1, 0]) / 4.0;
        //                result[i, j, 1] = (matrix[rowBase, colBase, 1] + matrix[rowBase, colBase + 1, 1]
        //                                + matrix[rowBase + 1, colBase, 1] + matrix[rowBase + 1, colBase + 1, 1]) / 4.0;
        //                result[i, j, 2] = (matrix[rowBase, colBase, 2] + matrix[rowBase, colBase + 1, 2]
        //                                + matrix[rowBase + 1, colBase, 2] + matrix[rowBase + 1, colBase + 1, 2]) / 4.0;
        //            }
        //        }
        //    }

        //    return result;
        //}

        //public static double[,] InterpolateMatrix(double[,] matrix)
        //{
        //    int originalRows = matrix.GetLength(0);
        //    int originalCols = matrix.GetLength(1);
        //    int newRows = originalRows * 2-1;
        //    int newCols = originalCols * 2-1;

        //    double[,] result = new double[newRows, newCols];

        //    for (int i = 0; i < newRows; i++)
        //    {
        //        for (int j = 0; j < newCols; j++)
        //        {
        //            // Eredeti mátrix indexek
        //            int rowBase = i / 2;
        //            int colBase = j / 2;

        //            if (i % 2 == 0 && j % 2 == 0)
        //            {
        //                // Másoljuk az eredeti mátrix elemeit
        //                result[i, j] = matrix[rowBase, colBase];

        //            }
        //            else if (i % 2 == 0)
        //            {
        //                // Soron belüli interpoláció
        //                result[i, j] = (matrix[rowBase, colBase] + matrix[rowBase, colBase + 1]) / 2.0;
        //            }
        //            else if (j % 2 == 0)
        //            {
        //                // Oszlopon belüli interpoláció
        //                result[i, j] = (matrix[rowBase, colBase] + matrix[rowBase + 1, colBase]) / 2.0;
        //            }
        //            else
        //            {
        //                // Négy elem átlaga
        //                result[i, j] = (matrix[rowBase, colBase] + matrix[rowBase, colBase + 1]
        //                                + matrix[rowBase + 1, colBase] + matrix[rowBase + 1, colBase + 1]) / 4.0;
        //            }
        //        }
        //    }

        //    return result;
        //}

        //public static double BicubicInterpolate(double[] p, double x)
        //{
        //    // p: A 4 közeli pixel értéke
        //    // x: Az interpolációs pozíció (0-1 között)
        //    return p[1] + 0.5 * x * (p[2] - p[0] + x * (2 * p[0] - 5 * p[1] + 4 * p[2] - p[3] + x * (3 * (p[1] - p[2]) + p[3] - p[0])));
        //}

        //public static double[,,] BicubicResize(double[,,] inputMatrix)
        //{
        //    int oldWidth = inputMatrix.GetLength(1);
        //    int oldHeight = inputMatrix.GetLength(0);
        //    int newWidth = oldWidth * 2 - 1;
        //    int newHeight = oldHeight * 2 - 1;
        //    int channels = inputMatrix.GetLength(2);

        //    double[,,] outputMatrix = new double[newHeight, newWidth, channels];

        //    // Vízszintes interpoláció
        //    for (int y = 0; y < oldHeight; y++)
        //    {
        //        for (int x = 0; x < newWidth; x++)
        //        {
        //            double gx = (double)x / (newWidth - 1) * (oldWidth - 1);
        //            int x0 = Math.Max((int)gx - 1, 0);
        //            int x1 = Math.Min((int)gx, oldWidth - 1);
        //            int x2 = Math.Min(x1 + 1, oldWidth - 1);
        //            int x3 = Math.Min(x2 + 1, oldWidth - 1);
        //            double dx = gx - x1;

        //            for (int c = 0; c < channels; c++)
        //            {
        //                double[] values = {
        //            inputMatrix[y, x0, c],
        //            inputMatrix[y, x1, c],
        //            inputMatrix[y, x2, c],
        //            inputMatrix[y, x3, c]
        //        };

        //                outputMatrix[y, x, c] = BicubicInterpolate(values, dx);
        //            }
        //        }
        //    }

        //    // Függőleges interpoláció
        //    double[,,] tempMatrix = new double[newHeight, newWidth, channels];
        //    for (int x = 0; x < newWidth; x++)
        //    {
        //        for (int y = 0; y < newHeight; y++)
        //        {
        //            double gy = (double)y / (newHeight - 1) * (oldHeight - 1);
        //            int y0 = Math.Max((int)gy - 1, 0);
        //            int y1 = Math.Min((int)gy, oldHeight - 1);
        //            int y2 = Math.Min(y1 + 1, oldHeight - 1);
        //            int y3 = Math.Min(y2 + 1, oldHeight - 1);
        //            double dy = gy - y1;

        //            for (int c = 0; c < channels; c++)
        //            {
        //                double[] values = {
        //            outputMatrix[y0, x, c],
        //            outputMatrix[y1, x, c],
        //            outputMatrix[y2, x, c],
        //            outputMatrix[y3, x, c]
        //        };

        //                tempMatrix[y, x, c] = BicubicInterpolate(values, dy);
        //            }
        //        }
        //    }

        //    return tempMatrix;
        //}


        public static double[,] BicubicInterpolation(double[,] data,int outWidth,int outHeight)
        {
            if (outWidth < 1 || outHeight < 1)
            {
                throw new ArgumentException(
                    "BicubicInterpolation: Expected output size to be " +
                    $"[1, 1, 1] or greater, got [{outHeight}, {outWidth}].");
            }

            // Bikubikus interpolációt végző függvény
            double InterpolateCubic(double v0, double v1, double v2, double v3, double fraction)
            {
                double p = (v3 - v2) - (v0 - v1);
                double q = (v0 - v1) - p;
                double r = v2 - v0;

                return (fraction * ((fraction * ((fraction * p) + q)) + r)) + v1;
            }

            // A chunk méretének meghatározása a párhuzamos feldolgozáshoz
            int rowsPerChunk = 6000 / outWidth;
            if (rowsPerChunk == 0)
            {
                rowsPerChunk = 1;
            }

            int chunkCount = (outHeight / rowsPerChunk)
                             + (outHeight % rowsPerChunk != 0 ? 1 : 0);

            var width = data.GetLength(1);
            var height = data.GetLength(0);
            var ret = new double[outHeight, outWidth];

            Parallel.For(0, chunkCount, (chunkNumber) =>
            {
                int jStart = chunkNumber * rowsPerChunk;
                int jStop = jStart + rowsPerChunk;
                if (jStop > outHeight)
                {
                    jStop = outHeight;
                }

                for (int j = jStart; j < jStop; ++j)
                {
                    float jLocationFraction = j / (float)outHeight;
                    var jFloatPosition = height * jLocationFraction;
                    var j2 = (int)jFloatPosition;
                    var jFraction = jFloatPosition - j2;
                    var j1 = j2 > 0 ? j2 - 1 : j2;
                    var j3 = j2 < height - 1 ? j2 + 1 : j2;
                    var j4 = j3 < height - 1 ? j3 + 1 : j3;

                    for (int i = 0; i < outWidth; ++i)
                    {
                        float iLocationFraction = i / (float)outWidth;
                        var iFloatPosition = width * iLocationFraction;
                        var i2 = (int)iFloatPosition;
                        var iFraction = iFloatPosition - i2;
                        var i1 = i2 > 0 ? i2 - 1 : i2;
                        var i3 = i2 < width - 1 ? i2 + 1 : i2;
                        var i4 = i3 < width - 1 ? i3 + 1 : i3;

                        // Bikubikus interpoláció X irányban
                        float jValue1 = (float)InterpolateCubic(
                            data[j1, i1], data[j1, i2], data[j1, i3], data[j1, i4], iFraction);
                        float jValue2 = (float)InterpolateCubic(
                            data[j2, i1], data[j2, i2], data[j2, i3], data[j2, i4], iFraction);
                        float jValue3 = (float)InterpolateCubic(
                            data[j3, i1], data[j3, i2], data[j3, i3], data[j3, i4], iFraction);
                        float jValue4 = (float)InterpolateCubic(
                            data[j4, i1], data[j4, i2], data[j4, i3], data[j4, i4], iFraction);

                        // Bikubikus interpoláció Y irányban
                        float iValue1 = (float)InterpolateCubic(jValue1, jValue2, jValue3, jValue4, jFraction);

                        // Most Z irányú interpoláció (a mélység dimenzió)
                        for (int k = 0; k < 1; ++k)
                        {
                            float kLocationFraction = k / (float)1;
                            var kFloatPosition = 1 * kLocationFraction;
                            var k2 = (int)kFloatPosition;
                            var kFraction = kFloatPosition - k2;
                            var k1 = k2 > 0 ? k2 - 1 : k2;
                            var k3 = k2 < 1 - 1 ? k2 + 1 : k2;
                            var k4 = k3 < 1 - 1 ? k3 + 1 : k3;

                            // Bikubikus interpoláció Z irányban (mélységi interpoláció)
                            ret[j, i] = (float)InterpolateCubic(
                                iValue1, iValue1, iValue1, iValue1, kFraction); // Egyszerűsített, Z irányban egyértékű interpoláció
                        }
                    }
                }
            });

            return ret;
        }

        public static double[,,] BicubicInterpolation3D(double[,,] data, int outWidth, int outHeight)
        {
            if (outWidth < 1 || outHeight < 1)
            {
                throw new ArgumentException("Expected output size to be [1, 1] or greater.");
            }

            int depth = data.GetLength(2);
            int rows = data.GetLength(0);
            int cols = data.GetLength(1);
            double[,,] result = new double[outHeight, outWidth, depth];

            // Bicubikus interpoláció egy dimenzióban
            double InterpolateCubic(double v0, double v1, double v2, double v3, double fraction)
            {
                double p = (v3 - v2) - (v0 - v1);
                double q = (v0 - v1) - p;
                double r = v2 - v0;

                return (fraction * ((fraction * ((fraction * p) + q)) + r)) + v1;
            }

            // Végigmegyünk minden színcsatornán (harmadik dimenzió)
            for (int d = 0; d < depth; d++)
            {
                for (int j = 0; j < outHeight; j++)
                {
                    float jLocationFraction = j / (float)outHeight;
                    var jFloatPosition = rows * jLocationFraction;
                    var j2 = (int)jFloatPosition;
                    var jFraction = jFloatPosition - j2;
                    var j1 = j2 > 0 ? j2 - 1 : j2;
                    var j3 = j2 < rows - 1 ? j2 + 1 : j2;
                    var j4 = j3 < rows - 1 ? j3 + 1 : j3;

                    for (int i = 0; i < outWidth; i++)
                    {
                        float iLocationFraction = i / (float)outWidth;
                        var iFloatPosition = cols * iLocationFraction;
                        var i2 = (int)iFloatPosition;
                        var iFraction = iFloatPosition - i2;
                        var i1 = i2 > 0 ? i2 - 1 : i2;
                        var i3 = i2 < cols - 1 ? i2 + 1 : i2;
                        var i4 = i3 < cols - 1 ? i3 + 1 : i3;

                        // Bicubikus interpoláció x irányban
                        float jValue1 = (float)InterpolateCubic(data[j1, i1, d], data[j1, i2, d], data[j1, i3, d], data[j1, i4, d], iFraction);
                        float jValue2 = (float)InterpolateCubic(data[j2, i1, d], data[j2, i2, d], data[j2, i3, d], data[j2, i4, d], iFraction);
                        float jValue3 = (float)InterpolateCubic(data[j3, i1, d], data[j3, i2, d], data[j3, i3, d], data[j3, i4, d], iFraction);
                        float jValue4 = (float)InterpolateCubic(data[j4, i1, d], data[j4, i2, d], data[j4, i3, d], data[j4, i4, d], iFraction);

                        // Bicubikus interpoláció y irányban
                        float finalValue = (float)InterpolateCubic(jValue1, jValue2, jValue3, jValue4, jFraction);

                        // Normalizálás 0 és 255 közé, majd kerekítés
                        int clampedValue = Math.Max(0, Math.Min(255, (int)Math.Round(finalValue)));
                        result[j, i, d] = clampedValue;
                    }
                }
            }

            return result;
        }

        public static double[,,] BitmapToMatrix(Bitmap bitmap)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;

            // A mátrix, ahol a harmadik dimenzió az R, G, B csatornák
            double[,,] matrix = new double[height, width, 3];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Az aktuális pixel színe
                    Color pixelColor = bitmap.GetPixel(x, y);

                    // Az RGB értékek beállítása
                    matrix[y, x, 0] = pixelColor.R; // Piros csatorna
                    matrix[y, x, 1] = pixelColor.G; // Zöld csatorna
                    matrix[y, x, 2] = pixelColor.B; // Kék csatorna
                }
            }

            return matrix;
        }

        static SerialPort _serialPort;

        public static void Main()
        {
            #region
            int width = 16;
            int height = 16;
            double[,] originalMatrix = GenerateRandomMatrix(width, height);
            for (int i = 0; i < originalMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < originalMatrix.GetLength(1); j++)
                {
                    Console.Write($"({originalMatrix[i,j]}) | ");
                }
                Console.WriteLine("\n------------------------------------------------------------------------");
            }
            double[,] scaledMatrix=BicubicInterpolation(originalMatrix, 32, 32);
            for (int i = 0; i < scaledMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < scaledMatrix.GetLength(1); j++)
                {
                    Console.Write($"({scaledMatrix[i, j]}) | ");
                }
                Console.WriteLine("\n------------------------------------------------------------------------");
            }

            //double[,,] originalImage = GenerateRandomRGBMatrix(width, height);

            //for (int i = 0; i < originalImage.GetLength(0); i++)
            //{
            //    for (int j = 0; j < originalImage.GetLength(1); j++)
            //    {
            //        Console.Write($"({originalImage[i, j, 0]}; {originalImage[i, j, 1]}; {originalImage[i, j, 2]}) |");
            //    }
            //    Console.WriteLine("\n------------------------------------------------------------------------");
            //}
            //Console.WriteLine("--------------------");
            //double[,,] scaledImage = BicubicInterpolation3D(originalImage, 32, 32);

            //for (int i = 0; i < scaledImage.GetLength(0); i++)
            //{
            //    for (int j = 0; j < scaledImage.GetLength(1); j++)
            //    {
            //        Console.Write($"({scaledImage[i, j, 0]}, {scaledImage[i, j, 1]}, {scaledImage[i, j, 2]}) |");
            //    }
            //    Console.WriteLine("\n------------------------------------------------------------------------");
            //}



            //Bitmap image = new Bitmap("smiley.jpg");
            //double[,,] imageMatrix = BitmapToMatrix(image);
            //double[,,] scaledImageMatrix = BicubicInterpolation3D(imageMatrix, imageMatrix.GetLength(0) * 2, imageMatrix.GetLength(1) * 2);
            //Bitmap scaledImage = MatrixToBitmap(scaledImageMatrix);
            //scaledImage.Save("scaled_smiley.png");

            //Bitmap originalBitmap = MatrixToBitmap(originalMatrix);
            //Bitmap scaledBitmap = MatrixToBitmap(BicubicInterpolation3D(originalMatrix,originalMatrix.GetLength(0)*2,originalMatrix.GetLength(1)*2));

            //originalBitmap.Save("original_image.png");
            //scaledBitmap.Save("scaled_image.png");


            #endregion
            #region
            //Random rnd = new Random();
            //int width = 4;
            //int height = 4;
            //float[,] originalMatrix = new float[height, width];
            //for (int i = 0; i < height; i++)
            //{
            //    for (int j = 0; j < width; j++)
            //    {
            //        originalMatrix[i, j] = rnd.Next(10);
            //        Console.Write(originalMatrix[i, j] + " ");
            //    }
            //    Console.WriteLine();
            //}

            //Console.WriteLine("---------------------------");
            //float[,] scaledMatrix = BicubicInterpolation(originalMatrix, 8,8);

            //for (int i = 0; i < scaledMatrix.GetLength(0) - 1; i++)
            //{
            //    for (int j = 0; j < scaledMatrix.GetLength(1) - 1; j++)
            //    {
            //        Console.Write(scaledMatrix[i, j] + "|");
            //    }
            //    Console.WriteLine("\n---------------");
            //}
            #endregion
            #region
            //string portName = "COM3"; // Az Arduino portja
            //int baudRate = 9600; // Az Arduino baud rate értéke

            //using (SerialPort serialPort = new SerialPort(portName, baudRate))
            //{
            //    serialPort.Open(); // Soros port megnyitása

            //    Console.WriteLine("Várakozás adatokra...");
            //    string data = string.Empty;

            //    while (true)
            //    {
            //        // Olvasd be a teljes adatot
            //        data = serialPort.ReadLine();
            //        Console.WriteLine($"Kapott adat: {data}");

            //        // Adatok feldolgozása mátrixként
            //        string[] rows = data.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            //        int[,] matrix = new int[rows.Length, rows[0].Split(',').Length];

            //        for (int i = 0; i < rows.Length; i++)
            //        {
            //            string[] values = rows[i].Split(',');
            //            for (int j = 0; j < values.Length; j++)
            //            {
            //                matrix[i, j] = int.Parse(values[j]);
            //            }
            //        }

            //        // Kiírás a konzolra a mátrix tartalmával
            //        Console.WriteLine("Kapott mátrix:");
            //        for (int i = 0; i < matrix.GetLength(0); i++)
            //        {
            //            for (int j = 0; j < matrix.GetLength(1); j++)
            //            {
            //                Console.Write(matrix[i, j] + " ");
            //            }
            //            Console.WriteLine();
            //        }
            //    }
            //}



            //SerialPort serialPort = new SerialPort("COM3", 9600); // A helyes COM port megadása
            //serialPort.Open();

            //try
            //{
            //    // Első sor: Mátrix dimenzióinak beolvasása
            //    string dimensionsLine = serialPort.ReadLine();
            //    string[] dimensions = dimensionsLine.Split(',');
            //    int rows = int.Parse(dimensions[0]);
            //    int cols = int.Parse(dimensions[1]);
            //    int depth = int.Parse(dimensions[2]);

            //    // 3D mátrix inicializálása
            //    int[,,] matrix = new int[rows, cols, depth];

            //    // Értékek beolvasása
            //    for (int i = 0; i < rows; i++)
            //    {
            //        for (int j = 0; j < cols; j++)
            //        {
            //            string[] rgbValues = serialPort.ReadLine().Split(',');
            //            for (int k = 0; k < depth; k++)
            //            {
            //                matrix[i, j, k] = int.Parse(rgbValues[k]);
            //            }
            //        }
            //    }

            //    // Mátrix kiírása ellenőrzéshez
            //    for (int i = 0; i < rows; i++)
            //    {
            //        for (int j = 0; j < cols; j++)
            //        {
            //            Console.Write($"[ ");
            //            for (int k = 0; k < depth; k++)
            //            {
            //                Console.Write(matrix[i, j, k] + " ");
            //            }
            //            Console.Write("] ");
            //        }
            //        Console.WriteLine();
            //    }
            //}
            //finally
            //{
            //    serialPort.Close();
            //}
            #endregion
            Console.ReadKey();
        }
    }
}
