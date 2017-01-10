using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniGL;

namespace SmartTools
{
    public static partial class Imaging
    {

        #region SymmetryTransform
        /// <summary>
        /// Applies a simple voting scheme to find symmetries in the given image. O(n^2) runtime
        /// </summary>
        /// <param name="src">The original image</param>
        /// <param name="dst">The image where the result should be placed</param>
        /// <param name="numIntervals">The number of intervals the intensity domain should be quantisized into</param>
        /// <param name="sampleStep">The step at which the image will be sampled</param>
        /// <param name="MAX_VALUE">The maximum value of the given original image</param>
        public static void SymmetryTransform(IImageLike<float> src, IImageLike<int> dst, int numIntervals, int sampleStep, float MAX_VALUE = 255)
        {
            Vec2I[][] sI = new Vec2I[numIntervals][];
            int[] idcs = new int[numIntervals];

            //initialization
            for (int x = 0; x < src.Width; x += sampleStep)
            {
                for (int y = 0; y < src.Height; y += sampleStep)
                {
                    int j = (int)Math.Floor(src[x, y] / MAX_VALUE * (numIntervals - 1));
                    if (idcs[j] == 0)
                        sI[j] = new Vec2I[src.Width * src.Height];
                    sI[j][idcs[j]++] = new Vec2I(x / sampleStep, y / sampleStep);
                }
            }

            //voting
            for (int i = 0; i < numIntervals; i++)
            {
                int j = 0;
                while (j < idcs[i] - 1)
                {
                    for (int n = j + 1; n < idcs[i]; n++)
                    {
                        int x = (sI[i][n].X - sI[i][j].X) / 2 + sI[i][j].X;
                        int y = (sI[i][n].Y - sI[i][j].Y) / 2 + sI[i][j].Y;
                        dst[x, y] = dst[x, y] + 1;
                    }
                    j++;
                }
            }
        }
        /// <summary>
        /// Applies a simple voting scheme to find symmetries in the given image. O(n^2) runtime
        /// </summary>
        /// <param name="src">The original image</param>
        /// <param name="dst">The image where the result should be placed</param>
        /// <param name="numIntervals">The number of intervals the intensity domain should be quantisized into</param>
        /// <param name="sampleStep">The step at which the image will be sampled</param>
        /// <param name="MAX_VALUE">The maximum value of the given original image</param>
        public static void SymmetryTransform(IImageLike<byte> src, IImageLike<int> dst, int numIntervals, int sampleStep)
        {
            Vec2I[][] sI = new Vec2I[numIntervals][];
            int[] idcs = new int[numIntervals];

            //initialization
            for (int x = 0; x < src.Width; x += sampleStep)
            {
                for (int y = 0; y < src.Height; y += sampleStep)
                {
                    int j = src[x, y] * (numIntervals - 1) / 255;
                    if (idcs[j] == 0)
                        sI[j] = new Vec2I[src.Width * src.Height];
                    sI[j][idcs[j]++] = new Vec2I(x / sampleStep, y / sampleStep);
                }
            }

            //voting
            for (int i = 0; i < numIntervals; i++)
            {
                int j = 0;
                while (j < idcs[i] - 1)
                {
                    for (int n = j + 1; n < idcs[i]; n++)
                    {
                        int x = (sI[i][n].X - sI[i][j].X) / 2 + sI[i][j].X;
                        int y = (sI[i][n].Y - sI[i][j].Y) / 2 + sI[i][j].Y;
                        dst[x, y] = dst[x, y] + 1;
                    }
                    j++;
                }
            }
        }

        #endregion

        #region Harris Corners
        public static FrameF64 HarrisCorners(IImageLike<double> im, int windowSize, double k)
        {
            var x = new FrameF64(im.Width, im.Height);
            var y = new FrameF64(im.Width, im.Height);
            DeriveX(im, x);
            DeriveY(im, y);
            var xx = IntegralImage.ComputeIntegralImage2(x);
            var yy = IntegralImage.ComputeIntegralImage2(y);
            var xy = new FrameF64(im.Width, im.Height);
            xy.Apply((i, j) => { return x[i, j] * y[i, j]; });
            xy = IntegralImage.ComputeIntegralImage2(xy);
            int w2 = windowSize / 2;
            for (int i = w2; i < im.Width - w2; i++)
            {
                for (int j = w2; j < im.Height - w2; j++)
                {
                    //| a b |
                    //| b c |
                    double a = xx[i + w2, j + w2] + xx[i - w2, j + w2] - xx[i - w2, j + w2] - xx[i + w2, j - w2];
                    double b = yy[i + w2, j + w2] + yy[i - w2, j + w2] - yy[i - w2, j + w2] - yy[i + w2, j - w2];
                    double c = xy[i + w2, j + w2] + xy[i - w2, j + w2] - xy[i - w2, j + w2] - xy[i + w2, j - w2];

                    x[i, j] = a * c - b * b - (a + c) * (a + c) * k;
                }
            }
            return x;
        }
        public static FrameF64 HarrisCorners(IImageLike<float> im, int windowSize, double k)
        {
            var res = new FrameF64(im.Width, im.Height);
            var x = new FrameF32(im.Width, im.Height);
            var y = new FrameF32(im.Width, im.Height);
            DeriveX(im, x);
            DeriveY(im, y);
            var xx = IntegralImage.ComputeIntegralImage2(x);
            var yy = IntegralImage.ComputeIntegralImage2(y);
            var xy = new FrameF64(im.Width, im.Height);
            xy.Apply((i, j) => { return x[i, j] * y[i, j]; });
            xy = IntegralImage.ComputeIntegralImage2(xy);
            int w2 = windowSize / 2;
            for (int i = w2; i < im.Width - w2; i++)
            {
                for (int j = w2; j < im.Height - w2; j++)
                {
                    //| a b |
                    //| b c |
                    double a = xx[i + w2, j + w2] + xx[i - w2, j + w2] - xx[i - w2, j + w2] - xx[i + w2, j - w2];
                    double b = yy[i + w2, j + w2] + yy[i - w2, j + w2] - yy[i - w2, j + w2] - yy[i + w2, j - w2];
                    double c = xy[i + w2, j + w2] + xy[i - w2, j + w2] - xy[i - w2, j + w2] - xy[i + w2, j - w2];

                    res[i, j] = a * c - b * b - (a + c) * (a + c) * k;
                }
            }
            return res;
        }
        public static FrameF64 HarrisCorners(IImageLike<int> im, int windowSize, double k)
        {
            var res = new FrameF64(im.Width, im.Height);
            var x = new FrameI32(im.Width, im.Height);
            var y = new FrameI32(im.Width, im.Height);
            DeriveX(im, x);
            DeriveY(im, y);
            var xx = IntegralImage.ComputeIntegralImage2(x);
            var yy = IntegralImage.ComputeIntegralImage2(y);
            var xy = new FrameF64(im.Width, im.Height);
            xy.Apply((i, j) => { return x[i, j] * y[i, j]; });
            xy = IntegralImage.ComputeIntegralImage2(xy);
            int w2 = windowSize / 2;
            for (int i = w2; i < im.Width - w2; i++)
            {
                for (int j = w2; j < im.Height - w2; j++)
                {
                    //| a b |
                    //| b c |
                    double a = xx[i + w2, j + w2] + xx[i - w2, j + w2] - xx[i - w2, j + w2] - xx[i + w2, j - w2];
                    double b = yy[i + w2, j + w2] + yy[i - w2, j + w2] - yy[i - w2, j + w2] - yy[i + w2, j - w2];
                    double c = xy[i + w2, j + w2] + xy[i - w2, j + w2] - xy[i - w2, j + w2] - xy[i + w2, j - w2];

                    res[i, j] = a * c - b * b - (a + c) * (a + c) * k;
                }
            }
            return res;
        }
        #endregion

        #region KMeans
        /// <summary>
        /// Uses the k means algorithm starting once to find color clusters in the given frame (single channel)
        /// </summary>
        /// <param name="k">Number of clusters</param>
        /// <param name="maxIter">Maximum number of iterations</param>
        /// <param name="frame">The frame to find clusters in</param>
        /// <param name="epsilon">A threshold which indicates when clusters are considered to not change anymore</param>
        /// <param name="maxValue">The channel width of the given frame</param>
        /// <returns>Returns the calculated clusters</returns>
        public static double[] KMeansCenter(int k, int maxIter, IImageLike<double> frame, double maxValue = 256, double epsilon = 1.0f)
        {
            var clusters = new double[k];
            for (int i = 0; i < k; i++)
                clusters[i] = RNG.NextDouble() * maxValue;

            double maxDist = double.MaxValue;
            int iter = 0;
            while (iter < maxIter)
            {
                iter++;
                if (maxDist <= epsilon * epsilon)
                    break;

                var sum = new double[k]; //initialized to zero
                int[] count = new int[k];
                for (int i = 0; i < frame.Height * frame.Width; i++)
                {
                    double minDist = double.MaxValue;
                    int s = 0;
                    for (int j = 0; j < k; j++)
                    {
                        double d = (frame[i] - clusters[j]) * (frame[i] - clusters[j]);
                        if (d < minDist)
                        {
                            minDist = d;
                            s = j;
                        }
                    }
                    sum[s] += frame[i];
                    count[s]++;
                }
                for (int i = 0; i < k; i++)
                {
                    if (count[i] == 0)
                        continue;
                    double av = sum[i] / count[i];
                    var d = (clusters[i] - av) * (clusters[i] - av);
                    if (d < maxDist)
                        maxDist = d;
                    clusters[i] = av;
                }
            }
            
            return clusters;
        }
        /// <summary>
        /// Uses the k means algorithm starting once to find color clusters in the given frame (single channel)
        /// </summary>
        /// <param name="k">Number of clusters</param>
        /// <param name="maxIter">Maximum number of iterations</param>
        /// <param name="frame">The frame to find clusters in</param>
        /// <param name="epsilon">A threshold which indicates when clusters are considered to not change anymore</param>
        /// <param name="maxValue">The channel width of the given frame</param>
        /// <returns>Returns the calculated clusters</returns>
        public static float[] KMeansCenter(int k, int maxIter, IImageLike<float> frame, float maxValue = 256, float epsilon = 1.0f)
        {
            var clusters = new float[k];
            for (int i = 0; i < k; i++)
                clusters[i] = (float)RNG.NextDouble() * maxValue;

            float maxDist = float.MaxValue;
            int iter = 0;
            while (iter < maxIter)
            {
                iter++;
                if (maxDist <= epsilon * epsilon)
                    break;

                var sum = new float[k]; //initialized to zero
                int[] count = new int[k];
                for (int i = 0; i < frame.Height * frame.Width; i++)
                {
                    float minDist = float.MaxValue;
                    int s = 0;
                    for (int j = 0; j < k; j++)
                    {
                        float d = (frame[i] - clusters[j]) * (frame[i] - clusters[j]);
                        if (d < minDist)
                        {
                            minDist = d;
                            s = j;
                        }
                    }
                    sum[s] += frame[i];
                    count[s]++;
                }
                for (int i = 0; i < k; i++)
                {
                    if (count[i] == 0)
                        continue;
                    float av = sum[i] / count[i];
                    var d = (clusters[i] - av) * (clusters[i] - av);
                    if (d < maxDist)
                        maxDist = d;
                    clusters[i] = av;
                }
            }
            
            return clusters;
        }
        /// <summary>
        /// Uses the k means algorithm starting once to find color clusters in the given frame (single channel)
        /// </summary>
        /// <param name="k">Number of clusters</param>
        /// <param name="maxIter">Maximum number of iterations</param>
        /// <param name="frame">The frame to find clusters in</param>
        /// <param name="epsilon">A threshold which indicates when clusters are considered to not change anymore</param>
        /// <param name="maxValue">The channel width of the given frame</param>
        /// <returns>Returns the calculated clusters</returns>
        public static int[] KMeansCenter(int k, int maxIter, IImageLike<int> frame, int maxValue = 256, float epsilon = 1.0f)
        {
            var clusters = new int[k];
            for (int i = 0; i < k; i++)
                clusters[i] = (int)(RNG.NextDouble() * maxValue);

            int maxDist = int.MaxValue;
            int iter = 0;
            while (iter < maxIter)
            {
                iter++;
                if (maxDist <= epsilon * epsilon)
                    break;

                var sum = new int[k]; //initialized to zero
                int[] count = new int[k];
                for (int i = 0; i < frame.Height * frame.Width; i++)
                {
                    var minDist = int.MaxValue;
                    int s = 0;
                    for (int j = 0; j < k; j++)
                    {
                        var d = (frame[i] - clusters[j]) * (frame[i] - clusters[j]);
                        if (d < minDist)
                        {
                            minDist = d;
                            s = j;
                        }
                    }
                    sum[s] += frame[i];
                    count[s]++;
                }
                for (int i = 0; i < k; i++)
                {
                    if (count[i] == 0)
                        continue;
                    int av = sum[i] / count[i];
                    var d = (clusters[i] - av) * (clusters[i] - av);
                    if (d < maxDist)
                        maxDist = d;
                    clusters[i] = d;
                }
            }
            
            return clusters;
        }
        /// <summary>
        /// Uses the k means algorithm starting once to find color clusters in the given frame (single channel)
        /// </summary>
        /// <param name="k">Number of clusters</param>
        /// <param name="maxIter">Maximum number of iterations</param>
        /// <param name="frame">The frame to find clusters in</param>
        /// <param name="epsilon">A threshold which indicates when clusters are considered to not change anymore</param>
        /// <param name="maxValue">The channel width of the given frame</param>
        /// <returns>Returns the calculated clusters</returns>
        public static byte[] KMeansCenter(int k, int maxIter, IImageLike<int> frame, float epsilon = 1.0f)
        {
            var clusters = new byte[k];
            for (int i = 0; i < k; i++)
                clusters[i] = (byte)(RNG.NextDouble() * 256);

            int maxDist = int.MaxValue;
            int iter = 0;
            while (iter < maxIter)
            {
                iter++;
                if (maxDist <= epsilon * epsilon)
                    break;

                var sum = new int[k]; //initialized to zero
                int[] count = new int[k];
                for (int i = 0; i < frame.Height * frame.Width; i++)
                {
                    var minDist = int.MaxValue;
                    int s = 0;
                    for (int j = 0; j < k; j++)
                    {
                        var d = (frame[i] - clusters[j]) * (frame[i] - clusters[j]);
                        if (d < minDist)
                        {
                            minDist = d;
                            s = j;
                        }
                    }
                    sum[s] += frame[i];
                    count[s]++;
                }
                for (int i = 0; i < k; i++)
                {
                    if (count[i] == 0)
                        continue;
                    int av = sum[i] / count[i];
                    var d = (clusters[i] - av) * (clusters[i] - av);
                    if (d < maxDist)
                        maxDist = d;
                    clusters[i] = (byte)d;
                }
            }
            
            return clusters;
        }

        /// <summary>
        /// Uses the k means algorithm starting once to cluster the given frame's active channel(single channel only)
        /// </summary>
        /// <param name="k">Number of clusters</param>
        /// <param name="maxIter">Maximum number of iterations</param>
        /// <param name="frame">The frame to be clustered</param>
        /// <param name="result">The frame the result will be stored in(can be same as frame)</param>
        /// <param name="epsilon">A threshold which indicates when clusters are considered to not change anymore</param>
        /// <param name="MAX_VALUE">The channel width of the given frame</param>
        /// <returns>Returns the calculated clusters</returns>
        public static double[] KMeansColorClustering(int k, int maxIter, IImageLike<double> frame, IImageLike<double> result, double epsilon = 1.0f, double MAX_VALUE = 256)
            
        {
            var clusters = KMeansCenter(k, maxIter, frame, epsilon, MAX_VALUE);
            ReduceColorSpect(clusters, result);

            return clusters;
        }
        /*
        /// <summary>
        /// Uses the k means algorithm starting once to cluster the given frame
        /// </summary>
        /// <param name="k">Number of clusters</param>
        /// <param name="maxIter">Maximum number of iterations</param>
        /// <param name="frame">The frame to be clustered</param>
        /// <param name="result">The frame the result will be stored in(can be same as frame)</param>
        /// <param name="epsilon">A threshold which indicates when clusters are considered to not change anymore</param>
        /// <param name="MAX_VALUE">The channel width of the given frame</param>
        /// <returns>Returns the clusters found, of size (k, 3)</returns>
        public static double[][] KMeansColorClustering(int k, int maxIter, Frame3D frame, Frame3D result, double epsilon = 1.0f, double MAX_VALUE = 256)
            where TCustom: NumericValueType<TCustom, TValue>
        {
            double[][] clusters = KMeansCenter2D(k, maxIter, frame, epsilon, MAX_VALUE);
            ReduceColorSpect(clusters, result);

            return clusters;
        }
        */
        /// <summary>
        /// Sets every color in the given frame to match the specified colors given using nearest neighbor selection
        /// </summary>
        /// <param name="colors">The colors present in the resulting frame</param>
        /// <param name="frame">The frame to have the color spectrum reduced</param>
        public static void ReduceColorSpect(double[] colors, IImageLike<double> frame)
        {
            Array.Sort(colors);
            for (int i = 0; i < frame.Height * frame.Width; i++)
            {
                var c = frame[i];
                double dist = double.MaxValue;
                int index = 0;
                for (int j = 0; j < colors.Length; j++)
                {
                    if (Math.Abs(c - colors[j]) < dist)
                    {
                        index = j;
                        dist = Math.Abs(c - colors[j]);
                    }
                    else
                        break;
                }
                frame[i] = colors[index];
            }
        }

        #endregion


    }
}
