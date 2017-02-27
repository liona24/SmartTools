using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using MathNet.Numerics.LinearAlgebra;

using GraphicsUtility;

namespace SmartTools.Detection
{
    public enum ScanningOptions
    {
        Grow,
        Shrink
    }
    public abstract class Detector
    {
        protected double scaleCoeffG = 1.3;
        protected int minWinWidth, minWinHeight;

        public Detector(int minWinWidth, int minWinHeight)
           : this(minWinWidth, minWinHeight, 1.3)
        {  }
        public Detector(int minWinWidth, int minWinHeight, double scaleCoefficientGreater)
        {
            this.minWinHeight = minWinHeight;
            this.minWinWidth = minWinWidth;

            this.scaleCoeffG = scaleCoefficientGreater;
        }

        public virtual RectI[] DetectRect<T>(IImageLike<T> frame, ScanningOptions op, bool useParallel)
            where T : IComparable, IEquatable<T>, IConvertible
        {
            RectI[] result = scanImage(frame, op, useParallel);
            return removeDuplicates(result);
        }
        public virtual Vector<double>[] DetectRect8<T>(IImageLike<T> frame, ScanningOptions op, bool useParallel)
            where T : IComparable, IEquatable<T>, IConvertible
        {
            var rect4 = DetectRect(frame, op, useParallel);
            Vector<double>[] rect8 = new Vector<double>[rect4.Length];
            for (int i = 0; i < rect4.Length; i++)
            {
                rect8[i] = Vector<double>.Build.DenseOfArray(new double[] { rect4[i].L, rect4[i].T, 
                                                                            rect4[i].R, rect4[i].T, 
                                                                            rect4[i].R, rect4[i].B, 
                                                                            rect4[i].L, rect4[i].B });
            }
            return rect8;
        }

        protected abstract bool classifyWindow<T>(int x, int y, int w, int h, IImageLike<T> frame)
            where T : IComparable, IEquatable<T>, IConvertible;

        private bool areEqual(RectI rect1, RectI rect2)
        {
            /* Hard selection by intersection for testing
            return !(rect4_2[0] > rect4_1[0] + rect4_1[2] || 
                rect4_2[0] + rect4_2[2] < rect4_1[0] || 
                rect4_2[1] > rect4_1[1] + rect4_1[3] ||
                rect4_2[1] + rect4_2[3] < rect4_1[1]);
            **/
            return (rect1.L - 0.2 * rect1.Width <= rect2.L && rect1.L + 0.2 * rect1.Width >= rect2.L &&
                rect1.T - 0.2 * rect1.Height <= rect2.T && rect1.T + 0.2 * rect1.Height >= rect2.T &&
                rect1.L + 0.8 * rect1.Width <= rect2.R && rect1.L + 1.2 * rect1.Width >= rect2.R &&
                rect1.T + 0.8 * rect1.Height <= rect2.B && rect1.T + 1.2 * rect1.Height >= rect2.B);
        }

        private RectI[] removeDuplicates(RectI[] a)
        {
            //TODO better ideas welcome...
            var result = new List<RectI>();
            for (int i = 0; i < a.Length; i++)
            {
                bool keep = true;
                foreach (RectI r in result)
                {
                    if (areEqual(r, a[i]))
                    {
                        keep = false;
                        break;
                    }
                }
                if (keep)
                {
                    result.Add(a[i]);
                }

            }
            return result.ToArray();
        }
        //helper func
        private double disSqr(Vector<double> v1, Vector<double> v2)
        {
            return (v1[0] - v2[0]) * (v1[0] - v2[0]) + (v1[1] - v2[1]) * (v1[1] - v2[1]);
        }

        private RectI[] scanImage<T>(IImageLike<T> frame, ScanningOptions op, bool useParallel)
            where T : IComparable, IEquatable<T>, IConvertible
        {
            var bag = new ConcurrentBag<RectI>();

            if (op == ScanningOptions.Shrink)
            {
                double scaleStep = 1 / scaleCoeffG;
                int scale = Math.Min(frame.Width / minWinWidth, frame.Height / minWinHeight);
                int winW = minWinWidth * scale;
                int winH = minWinHeight * scale;

                while (winW >= minWinWidth && winH >= minWinHeight)
                {
                    innerScanLoop(frame, winW, winH, bag, useParallel);
                    winW = (int)Math.Floor(winW * scaleStep);
                    winH = (int)Math.Floor(winH * scaleStep);
                }
            }
            else if (op == ScanningOptions.Grow)
            {

                double scaleStep = scaleCoeffG;
                int winW = minWinWidth;
                int winH = minWinHeight;
                while (winW < frame.Width && winH < frame.Height)
                {
                    innerScanLoop(frame, winW, winH, bag, useParallel);
                    winW = (int)Math.Ceiling(winW * scaleStep);
                    winH = (int)Math.Ceiling(winH * scaleStep);
                }

            }

            return bag.ToArray();
        }

        private void innerScanLoop<T>(IImageLike<T> frame, int winW, int winH, ConcurrentBag<RectI> results, bool parallel)
            where T : IComparable, IEquatable<T>, IConvertible
        {
            int stepX = winW >> 3;
            int stepY = winH >> 3;

            if (!parallel)
            {
                for (int x = 0; x < frame.Width - winW; x += stepX)
                {
                    for (int y = 0; y < frame.Height - winH; y += stepY)
                    {
                        if (classifyWindow(x, y, winW, winH, frame))
                            results.Add(RectI.FromXYWH(x, y, winW, winH));
                    }
                }
            }
            else
            {
                Parallel.For(0, (frame.Width - winW) / stepX, (x) =>
                {
                    for (int y = 0; y < frame.Height - winH; y += stepY)
                    {
                        if (classifyWindow(x * stepX, y, winW, winH, frame))
                            results.Add(RectI.FromXYWH(stepX * x, y, winW, winH));
                    }
                });
            }
        }
    }
}
