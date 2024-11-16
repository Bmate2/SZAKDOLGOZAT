using System;
using System.Drawing;
using System.Threading.Tasks;
using System.IO;
namespace Teszt
{
    internal class Program
    {
        //    public static int[,,] BilinearInterpolation(int[,,] originalImage)
    //    {
    //        int originalHeight = originalImage.GetLength(0);
    //        int originalWidth = originalImage.GetLength(1);
    //        int[,,] scaledImage = new int[originalHeight * 2, originalWidth * 2, 3];

    //        for (int i = 0; i < originalHeight; i++)
    //        {
    //            for (int j = 0; j < originalWidth; j++)
    //            {
    //                // Az eredeti kép pixelek másolása az új mátrixba
    //                int r = originalImage[i, j, 0];
    //                int g = originalImage[i, j, 1];
    //                int b = originalImage[i, j, 2];

    //                // Az új kép mátrix pozícióinak számítása
    //                int x = i * 2;
    //                int y = j * 2;

    //                // Eredeti pixel helye az új mátrixban
    //                scaledImage[x, y, 0] = r;
    //                scaledImage[x, y, 1] = g;
    //                scaledImage[x, y, 2] = b;

    //                // Jobbra, le, és átlós köztes pixelek interpolációja
    //                if (j + 1 < originalWidth)
    //                {
    //                    scaledImage[x, y + 1, 0] = (r + originalImage[i, j + 1, 0]) / 2;
    //                    scaledImage[x, y + 1, 1] = (g + originalImage[i, j + 1, 1]) / 2;
    //                    scaledImage[x, y + 1, 2] = (b + originalImage[i, j + 1, 2]) / 2;
    //                }

    //                if (i + 1 < originalHeight)
    //                {
    //                    scaledImage[x + 1, y, 0] = (r + originalImage[i + 1, j, 0]) / 2;
    //                    scaledImage[x + 1, y, 1] = (g + originalImage[i + 1, j, 1]) / 2;
    //                    scaledImage[x + 1, y, 2] = (b + originalImage[i + 1, j, 2]) / 2;
    //                }

    //                if (i + 1 < originalHeight && j + 1 < originalWidth)
    //                {
    //                    scaledImage[x + 1, y + 1, 0] = (r + originalImage[i, j + 1, 0] + originalImage[i + 1, j, 0] + originalImage[i + 1, j + 1, 0]) / 4;
    //                    scaledImage[x + 1, y + 1, 1] = (g + originalImage[i, j + 1, 1] + originalImage[i + 1, j, 1] + originalImage[i + 1, j + 1, 1]) / 4;
    //                    scaledImage[x + 1, y + 1, 2] = (b + originalImage[i, j + 1, 2] + originalImage[i + 1, j, 2] + originalImage[i + 1, j + 1, 2]) / 4;
    //                }
    //            }
    //        }

    //        return scaledImage;
    //    }

    //    public static void ScaleImageBilinear(string inputPath, string outputPath, int scaleFactor)
    //    {
    //        using (Bitmap originalBitmap = new Bitmap(inputPath))
    //        {
    //            int newWidth = originalBitmap.Width * scaleFactor;
    //            int newHeight = originalBitmap.Height * scaleFactor;
    //            using (Bitmap scaledBitmap = new Bitmap(newWidth, newHeight))
    //            {
    //                using (Graphics graphics = Graphics.FromImage(scaledBitmap))
    //                {
    //                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
    //                    graphics.DrawImage(originalBitmap, 0, 0, newWidth, newHeight);
    //                }

    //                scaledBitmap.Save(outputPath);
    //            }
    //        }
    //    }


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

        public static double[,,] InterpolateRGBMatrix(double[,,] matrix)
        {
            int originalRows = matrix.GetLength(0);
            int originalCols = matrix.GetLength(1);
            int newRows = originalRows * 2 - 1;
            int newCols = originalCols * 2 - 1;

            double[,,] result = new double[newRows, newCols, 3];

            for (int i = 0; i < newRows; i++)
            {
                for (int j = 0; j < newCols; j++)
                {
                    // Eredeti mátrix indexek
                    int rowBase = i / 2;
                    int colBase = j / 2;

                    if (i % 2 == 0 && j % 2 == 0)
                    {
                        // Másoljuk az eredeti mátrix elemeit
                        result[i, j, 0] = matrix[rowBase, colBase, 0];
                        result[i, j, 1] = matrix[rowBase, colBase, 1];
                        result[i, j, 2] = matrix[rowBase, colBase, 2];
                    }
                    else if (i % 2 == 0)
                    {
                        // Soron belüli interpoláció
                        result[i, j, 0] = (matrix[rowBase, colBase, 0] + matrix[rowBase, colBase + 1, 0]) / 2.0;
                        result[i, j, 1] = (matrix[rowBase, colBase, 1] + matrix[rowBase, colBase + 1, 1]) / 2.0;
                        result[i, j, 2] = (matrix[rowBase, colBase, 2] + matrix[rowBase, colBase + 1, 2]) / 2.0;
                    }
                    else if (j % 2 == 0)
                    {
                        // Oszlopon belüli interpoláció
                        result[i, j, 0] = (matrix[rowBase, colBase, 0] + matrix[rowBase + 1, colBase, 0]) / 2.0;
                        result[i, j, 1] = (matrix[rowBase, colBase, 1] + matrix[rowBase + 1, colBase, 1]) / 2.0;
                        result[i, j, 2] = (matrix[rowBase, colBase, 2] + matrix[rowBase + 1, colBase, 2]) / 2.0;
                    }
                    else
                    {
                        // Négy elem átlaga
                        result[i, j, 0] = (matrix[rowBase, colBase, 0] + matrix[rowBase, colBase + 1, 0]
                                        + matrix[rowBase + 1, colBase, 0] + matrix[rowBase + 1, colBase + 1, 0]) / 4.0;
                        result[i, j, 1] = (matrix[rowBase, colBase, 1] + matrix[rowBase, colBase + 1, 1]
                                        + matrix[rowBase + 1, colBase, 1] + matrix[rowBase + 1, colBase + 1, 1]) / 4.0;
                        result[i, j, 2] = (matrix[rowBase, colBase, 2] + matrix[rowBase, colBase + 1, 2]
                                        + matrix[rowBase + 1, colBase, 2] + matrix[rowBase + 1, colBase + 1, 2]) / 4.0;
                    }
                }
            }

            return result;
        }

        public static double[,] InterpolateMatrix(double[,] matrix)
        {
            int originalRows = matrix.GetLength(0);
            int originalCols = matrix.GetLength(1);
            int newRows = originalRows * 2-1;
            int newCols = originalCols * 2-1;

            double[,] result = new double[newRows, newCols];

            for (int i = 0; i < newRows; i++)
            {
                for (int j = 0; j < newCols; j++)
                {
                    // Eredeti mátrix indexek
                    int rowBase = i / 2;
                    int colBase = j / 2;

                    if (i % 2 == 0 && j % 2 == 0)
                    {
                        // Másoljuk az eredeti mátrix elemeit
                        result[i, j] = matrix[rowBase, colBase];
                                
                    }
                    else if (i % 2 == 0)
                    {
                        // Soron belüli interpoláció
                        result[i, j] = (matrix[rowBase, colBase] + matrix[rowBase, colBase + 1]) / 2.0;
                    }
                    else if (j % 2 == 0)
                    {
                        // Oszlopon belüli interpoláció
                        result[i, j] = (matrix[rowBase, colBase] + matrix[rowBase + 1, colBase]) / 2.0;
                    }
                    else
                    {
                        // Négy elem átlaga
                        result[i, j] = (matrix[rowBase, colBase] + matrix[rowBase, colBase + 1]
                                        + matrix[rowBase + 1, colBase] + matrix[rowBase + 1, colBase + 1]) / 4.0;
                    }
                }
            }

            return result;
        }

        public static double BicubicInterpolate(double[] p, double x)
        {
            // p: A 4 közeli pixel értéke
            // x: Az interpolációs pozíció (0-1 között)

            double[] a = new double[4];

            // Interpolációs súlyok kiszámítása (Cubic Hermite Spline)
            for (int i = 0; i < 4; i++)
            {
                a[i] = p[i];
            }

            return a[1] + 0.5 * x * (a[2] - a[0] + x * (2 * a[0] - 5 * a[1] + 4 * a[2] - a[3] + x * (3 * (a[1] - a[2]) + a[3] - a[0])));
        }

        public static double[,] BicubicResize(double[,] inputMatrix)
        {
            int oldWidth = inputMatrix.GetLength(1);
            int oldHeight = inputMatrix.GetLength(0);
            int newWidth = oldWidth * 2 - 1;
            int newHeight= oldHeight * 2 - 1;
            double[,] outputMatrix = new double[newHeight, newWidth];

            // Vízszintes interpoláció
            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    // Az új pozíciók kiszámítása az eredeti méret alapján
                    double gx = (double)x / (newWidth - 1) * (oldWidth - 1);
                    int x0 = (int)gx;
                    int x1 = Math.Min(x0 + 1, oldWidth - 1);
                    int x2 = Math.Min(x0 + 2, oldWidth - 1);
                    int x3 = Math.Min(x0 + 3, oldWidth - 1);

                    // Interpolálás az egyes sorokra
                    double[] row = new double[4];
                    row[0] = inputMatrix[y, x0];
                    row[1] = inputMatrix[y, x1];
                    row[2] = inputMatrix[y, x2];
                    row[3] = inputMatrix[y, x3];

                    double interpolatedX = BicubicInterpolate(row, gx - x0);

                    outputMatrix[y, x] = interpolatedX;
                }
            }

            // Függőleges interpoláció
            double[,] tempMatrix = new double[newHeight, newWidth];
            for (int x = 0; x < newWidth; x++)
            {
                for (int y = 0; y < newHeight; y++)
                {
                    // Az új pozíciók kiszámítása az eredeti méret alapján
                    double gy = (double)y / (newHeight - 1) * (oldHeight - 1);
                    int y0 = (int)gy;
                    int y1 = Math.Min(y0 + 1, oldHeight - 1);
                    int y2 = Math.Min(y0 + 2, oldHeight - 1);
                    int y3 = Math.Min(y0 + 3, oldHeight - 1);

                    // Interpolálás az egyes oszlopokra
                    double[] col = new double[4];
                    col[0] = outputMatrix[y0, x];
                    col[1] = outputMatrix[y1, x];
                    col[2] = outputMatrix[y2, x];
                    col[3] = outputMatrix[y3, x];

                    double interpolatedY = BicubicInterpolate(col, gy - y0);

                    tempMatrix[y, x] = interpolatedY;
                }
            }

            // Az eredmény visszaadása
            return tempMatrix;
        }

        //public static float[,,] BicubicInterpolation(
        //double[,,] data,
        //int outWidth,
        //int outHeight,
        //int outDepth)
        //{
        //    if (outWidth < 1 || outHeight < 1 || outDepth < 1)
        //    {
        //        throw new ArgumentException(
        //            "BicubicInterpolation: Expected output size to be " +
        //            $"[1, 1, 1] or greater, got [{outHeight}, {outWidth}, {outDepth}].");
        //    }

        //    // Bikubikus interpolációt végző függvény
        //    double InterpolateCubic(double v0, double v1, double v2, double v3, double fraction)
        //    {
        //        double p = (v3 - v2) - (v0 - v1);
        //        double q = (v0 - v1) - p;
        //        double r = v2 - v0;

        //        return (fraction * ((fraction * ((fraction * p) + q)) + r)) + v1;
        //    }

        //    // A chunk méretének meghatározása a párhuzamos feldolgozáshoz
        //    int rowsPerChunk = 6000 / outWidth;
        //    if (rowsPerChunk == 0)
        //    {
        //        rowsPerChunk = 1;
        //    }

        //    int chunkCount = (outHeight / rowsPerChunk)
        //                     + (outHeight % rowsPerChunk != 0 ? 1 : 0);

        //    var depth = data.GetLength(2);
        //    var width = data.GetLength(1);
        //    var height = data.GetLength(0);
        //    var ret = new float[outHeight, outWidth, outDepth];

        //    Parallel.For(0, chunkCount, (chunkNumber) =>
        //    {
        //        int jStart = chunkNumber * rowsPerChunk;
        //        int jStop = jStart + rowsPerChunk;
        //        if (jStop > outHeight)
        //        {
        //            jStop = outHeight;
        //        }

        //        for (int j = jStart; j < jStop; ++j)
        //        {
        //            float jLocationFraction = j / (float)outHeight;
        //            var jFloatPosition = height * jLocationFraction;
        //            var j2 = (int)jFloatPosition;
        //            var jFraction = jFloatPosition - j2;
        //            var j1 = j2 > 0 ? j2 - 1 : j2;
        //            var j3 = j2 < height - 1 ? j2 + 1 : j2;
        //            var j4 = j3 < height - 1 ? j3 + 1 : j3;

        //            for (int i = 0; i < outWidth; ++i)
        //            {
        //                float iLocationFraction = i / (float)outWidth;
        //                var iFloatPosition = width * iLocationFraction;
        //                var i2 = (int)iFloatPosition;
        //                var iFraction = iFloatPosition - i2;
        //                var i1 = i2 > 0 ? i2 - 1 : i2;
        //                var i3 = i2 < width - 1 ? i2 + 1 : i2;
        //                var i4 = i3 < width - 1 ? i3 + 1 : i3;

        //                // Bikubikus interpoláció X irányban
        //                float jValue1 = (float)InterpolateCubic(
        //                    data[j1, i1, 0], data[j1, i2, 0], data[j1, i3, 0], data[j1, i4, 0], iFraction);
        //                float jValue2 = (float)InterpolateCubic(
        //                    data[j2, i1, 0], data[j2, i2, 0], data[j2, i3, 0], data[j2, i4, 0], iFraction);
        //                float jValue3 = (float)InterpolateCubic(
        //                    data[j3, i1, 0], data[j3, i2, 0], data[j3, i3, 0], data[j3, i4, 0], iFraction);
        //                float jValue4 = (float)InterpolateCubic(
        //                    data[j4, i1, 0], data[j4, i2, 0], data[j4, i3, 0], data[j4, i4, 0], iFraction);

        //                // Bikubikus interpoláció Y irányban
        //                float iValue1 = (float)InterpolateCubic(jValue1, jValue2, jValue3, jValue4, jFraction);

        //                // Most Z irányú interpoláció (a mélység dimenzió)
        //                for (int k = 0; k < outDepth; ++k)
        //                {
        //                    float kLocationFraction = k / (float)outDepth;
        //                    var kFloatPosition = depth * kLocationFraction;
        //                    var k2 = (int)kFloatPosition;
        //                    var kFraction = kFloatPosition - k2;
        //                    var k1 = k2 > 0 ? k2 - 1 : k2;
        //                    var k3 = k2 < depth - 1 ? k2 + 1 : k2;
        //                    var k4 = k3 < depth - 1 ? k3 + 1 : k3;

        //                    // Bikubikus interpoláció Z irányban (mélységi interpoláció)
        //                    ret[j, i, k] = InterpolateCubic(
        //                        iValue1, iValue1, iValue1, iValue1, kFraction); // Egyszerűsített, Z irányban egyértékű interpoláció
        //                }
        //            }
        //        }
        //    });

        //    return ret;
        //}

        public static double[,,] BicubicInterpolation3D(double[,,] data, int outWidth, int outHeight)
        {
            if (outWidth < 1 || outHeight < 1)
            {
                throw new ArgumentException("Expected output size to be [1, 1] or greater.");
            }

            int depth = data.GetLength(2); // Színtér (például RGB)
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

        public static void Main()
        {
            #region
            //int width = 128;
            //int height = 128;
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
            //double[,,] scaledImage = BicubicInterpolation3D(originalImage,256,256);

            //for (int i = 0; i < scaledImage.GetLength(0); i++)
            //{
            //    for (int j = 0; j < scaledImage.GetLength(1); j++)
            //    {
            //        Console.Write($"({scaledImage[i, j, 0]}, {scaledImage[i, j, 1]}, {scaledImage[i, j, 2]}) |");
            //    }
            //    Console.WriteLine("\n------------------------------------------------------------------------");
            //}
            //Bitmap originalBitmap = MatrixToBitmap(originalImage);
            //Bitmap scaledBitmap = MatrixToBitmap(scaledImage);

            Bitmap originalImage;
            try
            {
                originalImage = new Bitmap(@"C:\Users\Egyetem\Desktop\Szakdolgozat\SZAKDOGA\Teszt\Teszt\bin\Debug\smiley.png");
                Console.WriteLine("Kép betöltése sikeres!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hiba a kép betöltése során: {ex.Message}");
            }
            //double[,,] originalMatrix = BitmapToMatrix(originalImage);
            //double[,,] scaledMatrix = BicubicInterpolation3D(originalMatrix, originalMatrix.GetLength(0)*2, originalMatrix.GetLength(1)*2);
            //Bitmap scaledImage = MatrixToBitmap(scaledMatrix);
            //scaledImage.Save("scaled_image.png");


            //originalBitmap.Save("original_image.png");
            //scaledBitmap.Save("scaled_image.png");
            ////ScaleImageBilinear("original_image.png", "scaled_image.png", 2);
            ////Console.WriteLine("Images saved as 'original_image.png' and 'scaled_image.png'.");
            #endregion
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

            Console.ReadKey();
        }
    }
}
