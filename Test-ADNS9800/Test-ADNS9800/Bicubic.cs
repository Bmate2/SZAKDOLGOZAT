using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_ADNS9800
{
    internal class Bicubic
    {
        private int width,height;
        private int scale;

        public Bicubic(int width, int height, int scale)
        {
            this.width = width;
            this.height = height;
            this.scale = scale;
        }

        public int[] BicubicInterpolation (int[] frameData)
        {
            int newWidth = width * scale;
            int newHeight = height * scale;
            int[] interpolatedData = new int[newWidth * newHeight];

            int[,] original = new int[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    original[y, x] = frameData[y * width + x];
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

                    int value = 0;
                    for (int j = -1; j < 3; j++)
                    {
                        for (int i = -1; i < 3; i++)
                        {
                            int xIndex = Reflect(x0 + i);
                            int yIndex = Reflect(y0 + j);
                            float weight = GetWeight(dy - j) * GetWeight(dx - i);
                            value += (int)(original[yIndex, xIndex] * weight);
                        }
                    }
                    interpolatedData[y * newWidth + x] = Math.Min(Math.Max((int)value, 0), 255);
                }
            }
            return interpolatedData;
        }

        private float GetWeight(float x)
        {
            x = Math.Abs(x);
            if (x < 1) return (1.5f * x * x * x) - (2.5f * x * x) + 1;
            if (1 <= x && x < 2) return (-0.5f * x * x * x) + (2.5f * x * x) - (4 * x) + 2;
            return 0;
        }

        private int Reflect(int i)
        {
            if (i < 0) return -i;
            if (i >= 30) return 2 * 30 - i - 1;
            return i;
        }
    }
}
