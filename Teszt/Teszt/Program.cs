using System;
using System.Drawing;
namespace Teszt
{
    internal class Program
    {
        public static int[,,] BilinearInterpolation(int[,,] originalImage)
        {
            int originalHeight = originalImage.GetLength(0);
            int originalWidth = originalImage.GetLength(1);
            int[,,] scaledImage = new int[originalHeight * 2, originalWidth * 2, 3];

            for (int i = 0; i < originalHeight; i++)
            {
                for (int j = 0; j < originalWidth; j++)
                {
                    // Az eredeti kép pixelek másolása az új mátrixba
                    int r = originalImage[i, j, 0];
                    int g = originalImage[i, j, 1];
                    int b = originalImage[i, j, 2];

                    // Az új kép mátrix pozícióinak számítása
                    int x = i * 2;
                    int y = j * 2;

                    // Eredeti pixel helye az új mátrixban
                    scaledImage[x, y, 0] = r;
                    scaledImage[x, y, 1] = g;
                    scaledImage[x, y, 2] = b;

                    // Jobbra, le, és átlós köztes pixelek interpolációja
                    if (j + 1 < originalWidth)
                    {
                        scaledImage[x, y + 1, 0] = (r + originalImage[i, j + 1, 0]) / 2;
                        scaledImage[x, y + 1, 1] = (g + originalImage[i, j + 1, 1]) / 2;
                        scaledImage[x, y + 1, 2] = (b + originalImage[i, j + 1, 2]) / 2;
                    }

                    if (i + 1 < originalHeight)
                    {
                        scaledImage[x + 1, y, 0] = (r + originalImage[i + 1, j, 0]) / 2;
                        scaledImage[x + 1, y, 1] = (g + originalImage[i + 1, j, 1]) / 2;
                        scaledImage[x + 1, y, 2] = (b + originalImage[i + 1, j, 2]) / 2;
                    }

                    if (i + 1 < originalHeight && j + 1 < originalWidth)
                    {
                        scaledImage[x + 1, y + 1, 0] = (r + originalImage[i, j + 1, 0] + originalImage[i + 1, j, 0] + originalImage[i + 1, j + 1, 0]) / 4;
                        scaledImage[x + 1, y + 1, 1] = (g + originalImage[i, j + 1, 1] + originalImage[i + 1, j, 1] + originalImage[i + 1, j + 1, 1]) / 4;
                        scaledImage[x + 1, y + 1, 2] = (b + originalImage[i, j + 1, 2] + originalImage[i + 1, j, 2] + originalImage[i + 1, j + 1, 2]) / 4;
                    }
                }
            }

            return scaledImage;
        }

        public static void ScaleImageBilinear(string inputPath, string outputPath, int scaleFactor)
        {
            using (Bitmap originalBitmap = new Bitmap(inputPath))
            {
                int newWidth = originalBitmap.Width * scaleFactor;
                int newHeight = originalBitmap.Height * scaleFactor;
                using (Bitmap scaledBitmap = new Bitmap(newWidth, newHeight))
                {
                    using (Graphics graphics = Graphics.FromImage(scaledBitmap))
                    {
                        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
                        graphics.DrawImage(originalBitmap, 0, 0, newWidth, newHeight);
                    }

                    scaledBitmap.Save(outputPath);
                }
            }
        }


        public static Bitmap MatrixToBitmap(int[,,] matrix)
        {
            int height = matrix.GetLength(0);
            int width = matrix.GetLength(1);
            Bitmap bitmap = new Bitmap(width, height);

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int r = matrix[i, j, 0];
                    int g = matrix[i, j, 1];
                    int b = matrix[i, j, 2];
                    Color color = Color.FromArgb(r, g, b);
                    bitmap.SetPixel(j, i, color);
                }
            }

            return bitmap;
        }
        public static int[,,] GenerateRandomRGBMatrix(int width, int height)
        {
            Random random = new Random();
            int[,,] matrix = new int[height, width, 3]; // 3 dimenzió: magasság, szélesség, RGB

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    matrix[i, j, 0] = random.Next(256); // R érték
                    matrix[i, j, 1] = random.Next(256); // G érték
                    matrix[i, j, 2] = random.Next(256); // B érték
                }
            }

            return matrix;
        }


        public static void Main()
        {
            int width = 128;
            int height = 128;
            int[,,] originalImage = GenerateRandomRGBMatrix(width, height);




            //for (int i = 0; i < originalImage.GetLength(0); i++)
            //{
            //    for (int j = 0; j < originalImage.GetLength(1); j++)
            //    {
            //        Console.Write($"({originalImage[i, j, 0]}, {originalImage[i, j, 1]}, {originalImage[i, j, 2]}) ");
            //    }
            //    Console.WriteLine();
            //}
            //Console.WriteLine("--------------------");
            int[,,] scaledImage = BilinearInterpolation(originalImage);

            //for (int i = 0; i < scaledImage.GetLength(0)-1; i++)
            //{
            //    for (int j = 0; j < scaledImage.GetLength(1)-1; j++)
            //    {
            //        Console.Write($"({scaledImage[i, j, 0]}, {scaledImage[i, j, 1]}, {scaledImage[i, j, 2]}) ");
            //    }
            //    Console.WriteLine();
            //}
            Bitmap originalBitmap = MatrixToBitmap(originalImage);
            Bitmap scaledBitmap = MatrixToBitmap(scaledImage);

            originalBitmap.Save("original_image.png");
            //scaledBitmap.Save("scaled_image.png");
            ScaleImageBilinear("original_image.png", "scaled_image.png", 2);
            Console.WriteLine("Images saved as 'original_image.png' and 'scaled_image.png'.");

            Console.ReadKey();
        }
    }
}
