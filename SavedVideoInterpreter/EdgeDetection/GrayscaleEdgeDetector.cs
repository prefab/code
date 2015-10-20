using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prefab;

namespace EdgeDetection
{
    public class GrayscaleEdgeDetector
    {
        private static int[,] sobel = new int[3, 3] { { 1, 2, 1 }, { 0, 0, 0 }, { -1, -2, -1 } };
        private static int[,] sobelT = new int[3, 3] { { 1, 0, -1 }, { 2, 0, -2 }, {1, 0, - 1 } };
        public static Bitmap Gradient(Bitmap bmp)
        {
            Bitmap cpy = Bitmap.DeepCopy(bmp);

            for (int row = 1; row < cpy.Height - 1; row++)
            {
                for (int col = 1; col < cpy.Width - 1; col++)
                {

                    double lx = -0.5 * bmp[ row, col - 1] + 0.5 * bmp[ row, col + 1];
                    double ly = -0.5 * bmp[row - 1, col] + 0.5 * bmp[row + 1, col];
                    
                    
                    double grad = Math.Sqrt(lx * lx + ly * ly);
                    cpy[ row , col + 1] = (int)Math.Round(grad);
                }
            }

            return Bitmap.Crop(cpy, 1, 1, cpy.Width - 2, cpy.Height - 2);
        }

        public static Bitmap GradientLeft(Bitmap bmp)
        {
            Bitmap cpy = Bitmap.Crop(bmp, 1, 1, bmp.Width - 2, bmp.Height - 2);
            for (int row = 1; row < bmp.Height - 1; row++)
            {
                for (int col = 1; col < bmp.Width - 1; col++)
                {

                    double lx = -0.5 * bmp[ row, col - 1] + 0.5 * bmp[row, col];
                    cpy[row - 1, col - 1] = (int)Math.Max(0, Math.Round(lx));
                }
            }
            return cpy;
        }
        public static Bitmap GradientRight(Bitmap bmp)
        {
            Bitmap cpy = Bitmap.Crop(bmp, 1, 1, bmp.Width - 2, bmp.Height - 2);
            for (int row = 1; row < bmp.Height - 1; row++)
            {
                for (int col = 1; col < bmp.Width - 1; col++)
                {
                    double lx = -0.5 * bmp[row, col] + 0.5 * bmp[row, col + 1];
                    cpy[ row - 1, col - 1] = (int)Math.Max(0, Math.Round(lx));
                }
            }
            return cpy;
        }
        public static Bitmap GradientUp(Bitmap bmp)
        {
            Bitmap cpy = Bitmap.Crop(bmp, 1, 1, bmp.Width - 2, bmp.Height - 2);
            for (int row = 1; row < bmp.Height - 1; row++)
            {
                for (int col = 1; col < bmp.Width - 1; col++)
                {
                    double ly = -0.5 * bmp[row - 1, col] + 0.5 * bmp[ row, col];
                    cpy[row - 1, col - 1] = (int)Math.Max(0, Math.Round(ly));
                }
            }
            return cpy;
        }
        public static Bitmap GradientDown(Bitmap bmp)
        {
            Bitmap cpy = Bitmap.Crop(bmp, 1, 1,  bmp.Width - 2, bmp.Height - 2);
            for (int row = 1; row < bmp.Height - 1; row++)
            {
                for (int col = 1; col < bmp.Width - 1; col++)
                {
                    double ly = -0.5 * bmp[ row, col] + 0.5 * bmp[row + 1, col];
                    cpy[row - 1, col - 1] = (int)Math.Max(0, Math.Round(ly));
                }
            }
            return cpy;
        }
        public static int[] GrayscaleHistogram(Bitmap bmp)
        {
            int[] hist = new int[255];
            for (int row = 0; row < bmp.Height; row++)
            {
                for (int col = 0; col < bmp.Width; col++)
                {
                    hist[bmp[row, col]]++;
                }
            }
            return hist;
        }

        public static Bitmap GrayscaleGradient(Bitmap bmp)
        {
            Bitmap gray = ConvertToGrayscale(bmp, false);
            return Gradient(gray);
        }

        public static Bitmap GradientAll(Bitmap left, Bitmap up, Bitmap right, Bitmap down)
        {
            Bitmap bmp = Bitmap.DeepCopy(left);
            for (int row = 0; row < bmp.Height; row++)
            {
                for (int col = 0; col < bmp.Width; col++)
                {
                    bmp[ row, col] = (int)Math.Max(Math.Max(Math.Max(left[ row, col], right[row, col]), up[ row, col]), down[row, col]);
                }
            }
            return bmp;
        }

        public static int[] CountOfEachGradient(Bitmap gradientBitmap)
        {
            int[] buckets = new int[256];
            for (int i = 0; i < 256; i++)
                buckets[i] = 0;

            for (int row = 0; row < gradientBitmap.Height; row++)
            {
                for (int col = 0; col < gradientBitmap.Width; col++)
                {

                    buckets[gradientBitmap[ row, col]]++;
                }
            }

            return buckets;
        }

        public static Bitmap BinaryImage(Bitmap gradientBmp, int thresh)
        {
            Bitmap cpy = Bitmap.DeepCopy(gradientBmp);
            for (int row = 0; row < cpy.Height; row++)
            {
                for (int col = 0; col < cpy.Width; col++)
                {
                    if (cpy[row, col] > thresh)
                        cpy[row, col] = -1; //foreground
                    else
                        cpy[row, col] = 0xff << 24; //background
                }
            }
            return cpy;
        }

        public static Bitmap ConvertToGrayscale(Bitmap bmp, bool forShow) {
            Bitmap grayscale = Bitmap.DeepCopy(bmp);
            for (int row = 0; row < grayscale.Height; row++) {
                for (int col = 0; col < grayscale.Width; col++) {
                    int pixel = grayscale[row, col];
                    int r = Bitmap.Red(pixel);
                    int g = Bitmap.Green(pixel);
                    int b = Bitmap.Blue(pixel);

                    pixel = (int)Math.Round(.3 * r + .59 * g + .11 * b);
                    if (!forShow)
                        grayscale[row, col] = pixel;
                    else
                        grayscale[row, col] = (0xff << 24) | (pixel << 16) | (pixel << 8) | pixel;
                }
            }
            return grayscale;
        }
    }
}
