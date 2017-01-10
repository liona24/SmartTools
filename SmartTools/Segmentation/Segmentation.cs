/*
    Implementation of a Segmentation algorithm proposed by Chan and Vese
    in "Active Contours Without Edges" in IEEE Transactions on Image Pro-
    cessing, vol. 10, 2001
*/

using System;
using MiniGL;

namespace SmartTools
{
    public class CurveEvolutionSegmenter
    {
        double lIn = 1.0; //weight for difference inside the curve
        double lOut = 1.0; // ~ outside
        double uLength = 0.5; //weight for length of the curve
        double uArea = 0.5; //weight for area inside the curve

        double epsilon2 = 1E-16;

        double[][] contour = null;

        #region Constructors
        public CurveEvolutionSegmenter(double uLength, double uArea)
        {
            this.uLength = uLength;
            this.uArea = uArea;
        }
        public CurveEvolutionSegmenter(double uLength, double uArea, double lOut, double lIn)
            : this(uLength, uArea)
        {
            this.lOut = lOut;
            this.lIn = lIn;
        }
        public CurveEvolutionSegmenter() { }
        public CurveEvolutionSegmenter(double[][] initContour)
        {
            contour = initContour;
        }
        public CurveEvolutionSegmenter(FrameF64 initContour)
        {
            copyImToContour(initContour);
        }
        #endregion

        //one time use
        public void SetInitContour(double[][] contour)
        {
            this.contour = contour;
        }
        public void SetInitContour(FrameF64 contour)
        {
            copyImToContour(contour);
        }

        public int[][] Segmentate<T>(IImageLike<T> im, double dt, int maxIter, double stopThresh = 0.005)
            where T : IComparable, IEquatable<T>, IConvertible
        {
            if (contour == null)
                initContour(im.Width, im.Height);

            stopThresh *= stopThresh;

            for (int n = 0; n < maxIter; n++)
            {
                //x -> inside, y -> outside
                var f = estimateIO(im);
                double diff = 0;

                for (int i = 0; i < im.Width; i++)
                {
                    for (int j = 0; j < im.Height; j++)
                    {
                        //name convention: p_ -> _ axis - 1
                        //                 n_ -> _ axis + 1
                        //                 a, b centered at x, y respectivly
                        //                 c contour

                        double b = centY(i, j);
                        double c = contour[i][j];
                        double a = centX(i, j);
                        int x = Math.Max(0, i - 1);
                        int y = Math.Max(0, j - 1);
                        double px_a = centX(x, j);
                        double py_b = centY(i, y);
                        double px_c = contour[x][j];
                        double py_c = contour[i][y];
                        x = Math.Min(im.Width - 1, i + 1);
                        y = Math.Min(im.Height - 1, j + 1);
                        double nx_c = contour[x][j];
                        double ny_c = contour[i][y];
                        double nx_a = centX(x, j);
                        double ny_b = centY(i, y);

                        double next = (c + dt * delta(c) *
                                        (a * nx_c + px_a * px_c + b * ny_c + py_b * py_c - uArea -
                                            lIn * Math.Pow(Convert.ToDouble(im[i, j]) - f.X, 2) - lOut * Math.Pow(Convert.ToDouble(im[i, j]) - f.Y, 2)))
                                    / (1 + delta(c) * (a + px_a + b + py_b));
                        contour[i][j] = next;
                        diff += (next - c) * (next - c);
                    }
                }

                diff /= im.Width * im.Height;
                if (diff <= stopThresh)
                    break;
            }

            var map = new int[im.Width][];
            for (int i = 0; i < im.Width; i++)
            {
                map[i] = new int[im.Height];
                for (int j = 0; j < im.Height; j++)
                    map[i][j] = Math.Sign(contour[i][j]);
            }

            return map;
        }

        private void copyImToContour(FrameF64 im)
        {
            contour = new double[im.Width][];
            for (int i = 0; i < im.Width; i++)
            {
                contour[i] = new double[im.Height];
                for (int j = 0; j < im.Height; j++)
                    contour[i][j] = im[i, j];
            }
        }

        private static double heavy(double t)
        {
            return 0.5 + 1 / Math.PI * Math.Atan(t);
        }
        private static double delta(double t)
        {
            return 1 / (Math.PI * (1 + t * t));
        }

        private Vec2 estimateIO<T>(IImageLike<T> im)
            where T : IComparable, IEquatable<T>, IConvertible
        {
            double sum_cIn = 0;
            double sum_cOut = 0;
            double sum_h = 0;
            for (int i = 0; i < im.Width; i++)
            {
                for (int j = 0; j < im.Height; j++)
                {
                    double h = heavy(contour[i][j]);
                    sum_h += h;
                    sum_cIn += Convert.ToDouble(im[i, j]) * h;
                    sum_cOut += Convert.ToDouble(im[i, j]) * (1 - h);
                }
            }
            double sum_hI = im.Width * im.Height - sum_h;

            return new Vec2(sum_cIn / sum_h, sum_cOut / sum_hI);
        }

        private double centX(int x, int y)
        {
            int py = Math.Max(0, y - 1);
            int ny = Math.Min(contour[0].Length - 1, y + 1);
            int nx = Math.Min(contour.Length - 1, x + 1);
            return uLength / Math.Sqrt(epsilon2 + Math.Pow(contour[nx][y] - contour[x][y], 2) +
                                        0.25 * Math.Pow(contour[x][ny] - contour[x][py], 2));
        }
        private double centY(int x, int y)
        {
            int px = Math.Max(0, x - 1);
            int ny = Math.Min(contour[0].Length - 1, y + 1);
            int nx = Math.Min(contour.Length - 1, x + 1);
            return uLength / Math.Sqrt(epsilon2 + Math.Pow(contour[nx][y] - contour[px][y], 2) * 0.25 +
                                        Math.Pow(contour[x][ny] - contour[x][y], 2));
        }

        private void initContour(int width, int height)
        {
            //TODO maybe add more customization, aka allow to change the function wich is used to init the level set function
            //currently: sin(pi/5 * x) * sin(pi/5 * y)

            const double PI_over_5 = 0.6283185307179586;

            contour = new double[width][];
            for (int i = 0; i < width; i++)
            {
                contour[i] = new double[height];
                for (int j = 0; j < height; j++)
                    contour[i][j] = Math.Sin(PI_over_5 * i) * Math.Sin(PI_over_5 * j);
            }
        }
    }

}
