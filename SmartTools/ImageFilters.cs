using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;


using MiniGL;

namespace SmartTools
{
    /// <summary>
    /// A class providing utilities and filters for image processing
    /// </summary>
    public static partial class Imaging
    {
        /// <summary>
        /// Random Number Generator used for the K Means Algorithm
        /// </summary>
        public static Random RNG = new Random();


        /*
        //complexity linear in pixel count
        //TODO rework for channelwidth != 256
        //TODO rework in general, this function is a mess!!!
        /// <summary>
        /// Applies a median filter to the given frame. ONLY WORKING FOR COLOR INTERVAL [0, 255]
        /// </summary>
        /// <param name="inp">The input frame</param>
        /// <param name="result">The frame where the result should be placed. Can be same as input</param>
        /// <param name="kernelSize">The size of the kernel</param>
        /// <param name="numColorIntervals">The number of intervals the color spectrum is quantisized into</param>
        public static void MedianFilter(Frame inp, Frame result, int kernelSize, int numColorIntervals)
        {
            kernelSize = kernelSize | 1;

            int rangeInterval = 256 / numColorIntervals;
            IntegralImage[] grayscaleBins = new IntegralImage[numColorIntervals];
            for (int i = 0; i < numColorIntervals; i++)
            {
                var values = new double[inp.Width * inp.Height];
                int m = i * rangeInterval + rangeInterval / 2;
                for (int j = 0; j < inp.Height * inp.Width; j++)
                    values[j] = Math.Abs(m - inp[j]);
                grayscaleBins[i] = IntegralImage.ComputeIntegralImage(values, inp.Width, inp.Height);
            }

            int a = kernelSize / 2;
            int b = 0;
            int aL = 0;
            int bTCustom= 0;
            for (int i = 0; i <= kernelSize / 2; i++, a++)
            {
                b = kernelSize / 2;
                for (int j = 0; j <= kernelSize / 2; j++, b++)
                {
                    double min = 256;
                    double min2 = 256;
                    double min3 = 256;
                    int x0 = -1;
                    int x1 = -1;
                    int x2 = -1;

                    for (int k = 0; k < numColorIntervals; k++)
                    {
                        if (grayscaleBins[k][a, b] < min)
                        {
                            min3 = min2;
                            min2 = min;
                            min = grayscaleBins[k][a, b];

                            x2 = x1;
                            x1 = x0;
                            x0 = k * rangeInterval + rangeInterval / 2;
                        }
                    }

                    result[i, j] = getMinMedian(x0, min, x1, min2, x2, min3);
                }
                b = kernelSize;
                bTCustom= 1;
                for (int j = kernelSize / 2 + 1; j < inp.Height - kernelSize / 2; j++, b++, bT++)
                {
                    double min = 256;
                    double min2 = 256;
                    double min3 = 256;
                    int x0 = -1;
                    int x1 = -1;
                    int x2 = -1;

                    for (int k = 0; k < numColorIntervals; k++)
                    {
                        if (grayscaleBins[k][a, b] - grayscaleBins[k][a, bT] < min)
                        {
                            min3 = min2;
                            min2 = min;
                            min = grayscaleBins[k][a, b] - grayscaleBins[k][a, bT];

                            x2 = x1;
                            x1 = x0;
                            x0 = k * rangeInterval + rangeInterval / 2;
                        }
                    }

                    result[i, j] = getMinMedian(x0, min, x1, min2, x2, min3);
                }
                b = inp.Height - 1;
                bTCustom= inp.Height - kernelSize;
                for (int j = inp.Height - kernelSize / 2; j < inp.Height; j++, bT++)
                {
                    double min = 256;
                    double min2 = 256;
                    double min3 = 256;
                    int x0 = -1;
                    int x1 = -1;
                    int x2 = -1;
                    for (int k = 0; k < numColorIntervals; k++)
                    {
                        if (grayscaleBins[k][a, b] - grayscaleBins[k][a, bT] < min)
                        {
                            min3 = min2;
                            min2 = min;
                            min = grayscaleBins[k][a, b] - grayscaleBins[k][a, bT];
                            x2 = x1;
                            x1 = x0;
                            x0 = k * rangeInterval + rangeInterval / 2;
                        }
                    }

                    result[i, j] = getMinMedian(x0, min, x1, min2, x2, min3);
                }

            }
            a = kernelSize;
            aL = 1;
            for (int i = kernelSize / 2 + 1; i < inp.Width - kernelSize / 2; i++, a++, aL++)
            {
                b = kernelSize / 2;
                for (int j = 0; j <= kernelSize / 2; j++, b++)
                {
                    double min = 256;
                    double min2 = 256;
                    double min3 = 256;
                    int x0 = -1;
                    int x1 = -1;
                    int x2 = -1;

                    for (int k = 0; k < numColorIntervals; k++)
                    {
                        if (grayscaleBins[k][a, b] - grayscaleBins[k][aL, b] < min)
                        {
                            min3 = min2;
                            min2 = min;
                            min = grayscaleBins[k][a, b] - grayscaleBins[k][aL, b];
                            x2 = x1;
                            x1 = x0;
                            x0 = k * rangeInterval + rangeInterval / 2;
                        }
                    }

                    result[i, j] = getMinMedian(x0, min, x1, min2, x2, min3);
                }
                bTCustom= 1;
                for (int j = kernelSize / 2 + 1; j < inp.Height - kernelSize / 2; j++, b++, bT++)
                {
                    double min = 256;
                    double min2 = 256;
                    double min3 = 256;
                    int x0 = -1;
                    int x1 = -1;
                    int x2 = -1;

                    for (int k = 0; k < numColorIntervals; k++)
                    {
                        if (grayscaleBins[k][a, b] + grayscaleBins[k][aL, bT] - grayscaleBins[k][aL, b] - grayscaleBins[k][a, bT] < min)
                        {
                            min3 = min2;
                            min2 = min;
                            min = grayscaleBins[k][a, b] + grayscaleBins[k][aL, bT] - grayscaleBins[k][aL, b] - grayscaleBins[k][a, bT];
                            x2 = x1;
                            x1 = x0;
                            x0 = k * rangeInterval + rangeInterval / 2;
                        }
                    }

                    result[i, j] = getMinMedian(x0, min, x1, min2, x2, min3);
                }
                b = inp.Height - 1;
                bTCustom= inp.Height - kernelSize;
                for (int j = inp.Height - kernelSize / 2; j < inp.Height; j++, bT++)
                {
                    double min = 256;
                    double min2 = 256;
                    double min3 = 256;
                    int x0 = -1;
                    int x1 = -1;
                    int x2 = -1;
                    for (int k = 0; k < numColorIntervals; k++)
                    {
                        if (grayscaleBins[k][a, b] + grayscaleBins[k][aL, bT] - grayscaleBins[k][aL, b] - grayscaleBins[k][a, bT] < min)
                        {
                            min3 = min2;
                            min2 = min;
                            min = grayscaleBins[k][a, b] + grayscaleBins[k][aL, bT] - grayscaleBins[k][aL, b] - grayscaleBins[k][a, bT];
                            x2 = x1;
                            x1 = x0;
                            x0 = k * rangeInterval + rangeInterval / 2;
                        }
                    }

                    result[i, j] = getMinMedian(x0, min, x1, min2, x2, min3);
                }
            }
            a = inp.Width - 1;
            aL = inp.Width - kernelSize;
            for (int i = inp.Width - kernelSize / 2; i < inp.Width; i++, aL++)
            {
                b = kernelSize / 2;
                for (int j = 0; j <= kernelSize / 2; j++, b++)
                {
                    double min = 256;
                    double min2 = 256;
                    double min3 = 256;
                    int x0 = -1;
                    int x1 = -1;
                    int x2 = -1;

                    for (int k = 0; k < numColorIntervals; k++)
                    {
                        if (grayscaleBins[k][a, b] - grayscaleBins[k][aL, b] < min)
                        {
                            min3 = min2;
                            min2 = min;
                            min = grayscaleBins[k][a, b] - grayscaleBins[k][aL, b];
                            x2 = x1;
                            x1 = x0;
                            x0 = k * rangeInterval + rangeInterval / 2;
                        }
                    }

                    result[i, j] = getMinMedian(x0, min, x1, min2, x2, min3);
                }
                b = kernelSize;
                bTCustom= 1;
                for (int j = kernelSize / 2 + 1; j < inp.Height - kernelSize / 2; j++, b++, bT++)
                {
                    double min = 256;
                    double min2 = 256;
                    double min3 = 256;
                    int x0 = -1;
                    int x1 = -1;
                    int x2 = -1;

                    for (int k = 0; k < numColorIntervals; k++)
                    {
                        if (grayscaleBins[k][a, b] + grayscaleBins[k][aL, bT] - grayscaleBins[k][aL, b] - grayscaleBins[k][a, bT] < min)
                        {
                            min3 = min2;
                            min2 = min;
                            min = grayscaleBins[k][a, b] + grayscaleBins[k][aL, bT] - grayscaleBins[k][aL, b] - grayscaleBins[k][a, bT];
                            x2 = x1;
                            x1 = x0;
                            x0 = k * rangeInterval + rangeInterval / 2;
                        }
                    }

                    result[i, j] = getMinMedian(x0, min, x1, min2, x2, min3);
                }
                b = inp.Height - 1;
                bTCustom= inp.Height - kernelSize;
                for (int j = inp.Height - kernelSize / 2; j < inp.Height; j++, bT++)
                {
                    double min = 256;
                    double min2 = 256;
                    double min3 = 256;
                    int x0 = -1;
                    int x1 = -1;
                    int x2 = -1;
                    for (int k = 0; k < numColorIntervals; k++)
                    {
                        if (grayscaleBins[k][a, b] + grayscaleBins[k][aL, bT] - grayscaleBins[k][aL, b] - grayscaleBins[k][a, bT] < min)
                        {
                            min3 = min2;
                            min2 = min;
                            min = grayscaleBins[k][a, b] + grayscaleBins[k][aL, bT] - grayscaleBins[k][aL, b] - grayscaleBins[k][a, bT];
                            x2 = x1;
                            x1 = x0;
                            x0 = k * rangeInterval + rangeInterval / 2;
                        }
                    }

                    result[i, j] = getMinMedian(x0, min, x1, min2, x2, min3);
                }
            }
        }
        */

        #region Normalization
        /// <summary>
        /// normalizes an image to have sum 1
        /// </summary>
        /// <param name="frame">the image to be normalized</param>
        public static void NormalizeSum(IImageLike<double> frame)
        {
            double sum = 0;
            for (int i = 0; i < frame.Width * frame.Height; i++)
                sum += frame[i];
            for (int i = 0; i < frame.Width * frame.Height; i++)
                frame[i] = frame[i] / sum;
        }
        /// <summary>
        /// normalizes an image to have sum 1
        /// </summary>
        /// <param name="frame">the image to be normalized</param>
        public static void NormalizeSum(IImageLike<float> frame)
        {
            float sum = 0;
            for (int i = 0; i < frame.Width * frame.Height; i++)
                sum += frame[i];
            for (int i = 0; i < frame.Width * frame.Height; i++)
                frame[i] = frame[i] / sum;
        }
        /// <summary>
        /// Normalizes an image to zero mean and standard deviation 1
        /// </summary>
        /// <param name="frame">the image to be normalized</param>
        public static void Normalize(IImageLike<double> frame)
        {
            double sum = 0;
            for (int i = 0; i < frame.Width * frame.Height; i++)
                sum += frame[i];
            sum = sum / (frame.Width * frame.Height);
            double sum2 = 0;
            for (int i = 0; i < frame.Width * frame.Height; i++)
            {
                frame[i] = frame[i] - (double)sum;
                sum2 += frame[i] * frame[i];
            }
            sum2 = Math.Sqrt(sum2 / (frame.Width * frame.Height - 1));
            if (sum2 <= double.Epsilon)
                return;
            sum2 = 1 / sum2;
            for (int i = 0; i < frame.Width * frame.Height; i++)
                frame[i] = frame[i] * sum2;
        }
        /// <summary>
        /// Normalizes an image to zero mean and standard deviation 1
        /// </summary>
        /// <param name="frame">the image to be normalized</param>
        public static void Normalize(IImageLike<float> frame)
        {
            float sum = 0;
            for (int i = 0; i < frame.Width * frame.Height; i++)
                sum += frame[i];
            sum = sum / (frame.Width * frame.Height);
            float sum2 = 0;
            for (int i = 0; i < frame.Width * frame.Height; i++)
            {
                frame[i] = frame[i] - sum;
                sum2 += frame[i] * frame[i];
            }
            sum2 = (float)Math.Sqrt(sum2 / (frame.Width * frame.Height - 1));
            if (sum2 <= float.Epsilon)
                return;
            sum2 = 1 / sum2;
            for (int i = 0; i < frame.Width * frame.Height; i++)
                frame[i] = frame[i] * sum2;
        } 
        /// <summary>
        /// normalizes an image to standard deviation 1
        /// </summary>
        /// <param name="frame">the image to be normalized</param>
        /// <param name="mean">the mean of the given image</param>
        public static void NormalizeVariance(IImageLike<double> frame, double mean)
        {
            double sum = 0;
            for (int i = 0; i < frame.Width * frame.Height; i++)
                sum += (frame[i] - mean) * (frame[i] - mean);
            sum = sum / (frame.Height * frame.Width - 1);
            if (sum <= double.Epsilon)
                return;
            sum = 1 / sum;
            for (int i = 0; i < frame.Width * frame.Height; i++)
                frame[i] = frame[i] * sum;
        }

        /// <summary>
        /// normalizes an image to standard deviation 1
        /// </summary>
        /// <param name="frame">the image to be normalized</param>
        public static void NormalizeVariance(IImageLike<double> frame)
        {
            NormalizeVariance(frame, CalcMean(frame));
        }

        /// <summary>
        /// normalizes an image to standard deviation 1
        /// </summary>
        /// <param name="frame">the image to be normalized</param>
        /// <param name="mean">the mean of the given image</param>
        public static void NormalizeVariance(IImageLike<float> frame, float mean)
        {
            float sum = 0;
            for (int i = 0; i < frame.Width * frame.Height; i++)
                sum += (frame[i] - mean) * (frame[i] - mean);
            sum = sum / (frame.Height * frame.Width - 1);
            if (sum <= float.Epsilon)
                return;
            sum = 1 / sum;
            for (int i = 0; i < frame.Width * frame.Height; i++)
                frame[i] = frame[i] * sum;
        }

        /// <summary>
        /// normalizes an image to standard deviation 1
        /// </summary>
        /// <param name="frame">the image to be normalized</param>
        public static void NormalizeVariance(IImageLike<float> frame)
        {
            NormalizeVariance(frame, CalcMean(frame));
        }
        /// <summary>
        /// Normalizes a frame to have the given maximum value
        /// </summary>
        public static void NormalizeMax(IImageLike<double> frame, double maxValue)
        {
            double max = 0;
            for (int i = 0; i < frame.Width * frame.Height; i++)
            {
                if (frame[i] > max)
                    max = frame[i];
            }
            max = maxValue / max;
            for (int i = 0; i < frame.Width * frame.Height; i++)
                frame[i] = frame[i] * max;
        }
        /// <summary>
        /// Normalizes a frame to have the given maximum value
        /// </summary>
        public static void NormalizeMax(IImageLike<float> frame, float maxValue)
        {
            float max = 0;
            for (int i = 0; i < frame.Width * frame.Height; i++)
            {
                if (frame[i] > max)
                    max = frame[i];
            }
            max = maxValue / max;
            for (int i = 0; i < frame.Width * frame.Height; i++)
                frame[i] = frame[i] * max;
        }
        #endregion

        public static double CalcMean(IImageLike<double> frame)
        {
            double mean = 0;
            for (int i = 0; i < frame.Width * frame.Height; i++)
                mean += frame[i];
            return mean / (frame.Width * frame.Height);
        }
        public static float CalcMean(IImageLike<float> frame)
        {
            double mean = 0;
            for (int i = 0; i < frame.Width * frame.Height; i++)
                mean += frame[i];
            return (float)(mean / (frame.Width * frame.Height));
        }

        #region Blur
        /// <summary>
        /// Convolves the given frame with a gaussian kernel of size kSize, sigma will be chosen accordingly
        /// </summary>
        public static void GaussianBlur(IImageLike<double> frame, int kSize)
        {
            double sigma = kSize / 6.0;
            gaussianBlur(frame, sigma, kSize);
        }
        /// <summary>
        /// Convolves the given frame with a gaussian kernel with the specified standard deviation
        /// </summary>
        public static void GaussianBlur(IImageLike<double> frame, double sigma)
        {
            int kSize = Math.Max(3, (int)(sigma * 6.0));
            gaussianBlur(frame, sigma, kSize);
        }
        /// <summary>
        /// Convolves the given frame with a gaussian kernel of size kSize, sigma will be chosen accordingly
        /// </summary>
        public static void GaussianBlur(IImageLike<float> frame, int kSize)
        {
            double sigma = kSize / 6.0;
            gaussianBlur(frame, sigma, kSize);
        }
        /// <summary>
        /// Convolves the given frame with a gaussian kernel with the specified standard deviation
        /// </summary>
        public static void GaussianBlur(IImageLike<float> frame, double sigma)
        {
            int kSize = Math.Max(3, (int)(sigma * 6.0));
            gaussianBlur(frame, sigma, kSize);
        }
        private static void gaussianBlur(IImageLike<float> frame, double sigma, int kSize)
        {
            var tmp = (IImageLike<float>)frame.Clone();
            if (kSize == 3)
            {
                var conv = new double[] { 1 / Math.Sqrt(2 * Math.PI * sigma * sigma) * Math.Exp(-1 / (2 * sigma * sigma)),
                    1 / Math.Sqrt(2 * Math.PI * sigma * sigma),
                    1 / Math.Sqrt(2 * Math.PI * sigma * sigma) * Math.Exp(-1 / (2 * sigma * sigma)) };
                double sum = conv[0] + conv[1] + conv[2];
                conv[0] /= sum;
                conv[1] /= sum;
                conv[2] /= sum;

                for (int i = 0; i < frame.Width; i++)
                {
                    for (int j = 0; j < frame.Height; j++)
                        tmp[i, j] = (float)(frame[i - 1, j] * conv[0] + frame[i, j] * conv[1] + frame[i + 1, j] * conv[2]);
                }
                for (int i = 0; i < frame.Width; i++)
                {
                    for (int j = 0; j < frame.Height; j++)
                        frame[i, j] = (float)(tmp[i, j - 1] * conv[0] + tmp[i, j] * conv[1] + frame[i, j + 1] * conv[2]);
                }
            }
            else
            {
                const int numIter = 3;

                int w = (int)Math.Floor(Math.Sqrt(12 * sigma * sigma / 3 + 1)) | 1;
                int m = (int)Math.Round((12 * sigma * sigma - numIter * w * w - 4 * numIter * w - 3 * numIter) / (-4 * w - 4));
                var sizes = new int[numIter] { 0 < m ? w : w + 2, 1 < m ? w : w + 2, 2 < m ? w : w + 2 };
                //checkout peter kovesi's website for more information on this

                var intIm = IntegralImage.ComputeIntegralImage(frame);
                for (int i = 0; i < numIter; i++)
                    BoxBlur(intIm, frame, sizes[i]);
            }
        }
        private static void gaussianBlur(IImageLike<double> frame, double sigma, int kSize)
        {
            var tmp = (IImageLike<double>)frame.Clone();
            if (kSize == 3)
            {
                var conv = new double[] { 1 / Math.Sqrt(2 * Math.PI * sigma * sigma) * Math.Exp(-1 / (2 * sigma * sigma)),
                    1 / Math.Sqrt(2 * Math.PI * sigma * sigma),
                    1 / Math.Sqrt(2 * Math.PI * sigma * sigma) * Math.Exp(-1 / (2 * sigma * sigma)) };
                double sum = conv[0] + conv[1] + conv[2];
                conv[0] /= sum;
                conv[1] /= sum;
                conv[2] /= sum;

                for (int i = 0; i < frame.Width; i++)
                {
                    for (int j = 0; j < frame.Height; j++)
                        tmp[i, j] = frame[i - 1, j] * conv[0] + frame[i, j] * conv[1] + frame[i + 1, j] * conv[2];
                }
                for (int i = 0; i < frame.Width; i++)
                {
                    for (int j = 0; j < frame.Height; j++)
                        frame[i, j] = tmp[i, j - 1] * conv[0] + tmp[i, j] * conv[1] + frame[i, j + 1] * conv[2];
                }
            }
            else
            {
                const int numIter = 3;

                int w = (int)Math.Floor(Math.Sqrt(12 * sigma * sigma / 3 + 1)) | 1;
                int m = (int)Math.Round((12 * sigma * sigma - numIter * w * w - 4 * numIter * w - 3 * numIter) / (-4 * w - 4));
                var sizes = new int[numIter] { 0 < m ? w : w + 2, 1 < m ? w : w + 2, 2 < m ? w : w + 2 };
                //checkout peter kovesi's website for more information on this

                var intIm = IntegralImage.ComputeIntegralImage(frame);
                for (int i = 0; i < numIter; i++)
                    BoxBlur(intIm, frame, sizes[i]);
            }
        }

        /// <summary>
        /// Convolves the given frame with a box kernel. Computes IntegralImage first.
        /// </summary>
        /// <param name="frame">The frame to be convolved.</param>
        /// <param name="kSize">The size of the kernel, must be odd number</param>
        public static void BoxBlur(IImageLike<float> frame, int kSize)
        {
            var intIm = IntegralImage.ComputeIntegralImage(frame);
            BoxBlur(intIm, frame, kSize);
        }

        /// <summary>
        /// Convolves the given frame with a box kernel using the given IntegralImage
        /// </summary>
        /// <param name="intIm">The IntegralImage of the frame to be convolved</param>
        /// <param name="result">The image where the result should be placed</param>
        /// <param name="kSize">The size of the kernel</param>
        public static void BoxBlur(FrameF64 intIm, IImageLike<float> result, int kSize)
        {
            if (kSize % 2 == 0)
                throw new Exception("Kernel must be of odd size!");

            double normX = 0;
            double normY = 0;
            int k2 = kSize / 2;

            for (int i = 0; i < result.Width; i++)
            {
                if (i < k2 || i >= result.Width - k2)
                    normX = 1.0 / (Math.Min(i + 1, result.Width - i) + k2);
                for (int j = 0; j < result.Height; j++)
                {
                    if (j < k2 || j >= result.Height - k2)
                        normY = 1.0 / (Math.Min(j + 1, result.Height - j) + k2);

                    result[i, j] = (float)(normX * normY *
                        (intIm[i - k2, j - k2] +
                        intIm[i + k2, j + k2] -
                        intIm[i - k2, j + k2] -
                        intIm[i + k2, j - k2]));
                }
            }
        }
        /// <summary>
        /// Convolves the given frame with a box kernel. Computes IntegralImage first.
        /// </summary>
        /// <param name="frame">The frame to be convolved.</param>
        /// <param name="kSize">The size of the kernel, must be odd number</param>
        public static void BoxBlur(IImageLike<double> frame, int kSize)
        {
            var intIm = IntegralImage.ComputeIntegralImage(frame);
            BoxBlur(intIm, frame, kSize);
        }

        /// <summary>
        /// Convolves the given frame with a box kernel using the given IntegralImage
        /// </summary>
        /// <param name="intIm">The IntegralImage of the frame to be convolved</param>
        /// <param name="result">The image where the result should be placed</param>
        /// <param name="kSize">The size of the kernel</param>
        public static void BoxBlur(FrameF64 intIm, IImageLike<double> result, int kSize)
        {
            if (kSize % 2 == 0)
                throw new Exception("Kernel must be of odd size!");

            double normX = 0;
            double normY = 0;
            int k2 = kSize / 2;

            for (int i = 0; i < result.Width; i++)
            {
                if (i < k2 || i >= result.Width - k2)
                    normX = 1.0 / (Math.Min(i + 1, result.Width - i) + k2);
                for (int j = 0; j < result.Height; j++)
                {
                    if (j < k2 || j >= result.Height - k2)
                        normY = 1.0 / (Math.Min(j + 1, result.Height - j) + k2);

                    result[i, j] = normX * normY *
                        (intIm[i - k2, j - k2] +
                        intIm[i + k2, j + k2] -
                        intIm[i - k2, j + k2] -
                        intIm[i + k2, j - k2]);
                }
            }
        }
        #endregion

        #region Derivation
        public static void DeriveX(IImageLike<double> src, IImageLike<double> dst)
        {
            for (int i = 0; i < src.Width; i++)
            {
                for (int j = 0; j < src.Height; j++)
                    dst[i, j] = src[i + 1, j] - src[i - 1, j];
            }
        }
        public static void DeriveY(IImageLike<double> src, IImageLike<double> dst)
        {
            for (int i = 0; i < src.Width; i++)
            {
                for (int j = 0; j < src.Height; j++)
                    dst[i, j] = src[i, j + 1] - src[i, j - 1];
            }
        }
        public static void DeriveX(IImageLike<float> src, IImageLike<float> dst)
        {
            for (int i = 0; i < src.Width; i++)
            {
                for (int j = 0; j < src.Height; j++)
                    dst[i, j] = src[i + 1, j] - src[i - 1, j];
            }
        }
        public static void DeriveY(IImageLike<float> src, IImageLike<float> dst)
        {
            for (int i = 0; i < src.Width; i++)
            {
                for (int j = 0; j < src.Height; j++)
                    dst[i, j] = src[i, j + 1] - src[i, j - 1];
            }
        }
        public static void DeriveX(IImageLike<int> src, IImageLike<int> dst)
        {
            for (int i = 0; i < src.Width; i++)
            {
                for (int j = 0; j < src.Height; j++)
                    dst[i, j] = src[i + 1, j] - src[i - 1, j];
            }
        }
        public static void DeriveY(IImageLike<int> src, IImageLike<int> dst)
        {
            for (int i = 0; i < src.Width; i++)
            {
                for (int j = 0; j < src.Height; j++)
                    dst[i, j] = src[i, j + 1] - src[i, j - 1];
            }
        }
        #endregion

#region Generic
        public static void Convolve(IImageLike<int> src, IImageLike<int> dst, float[] lineKernel, float[] rowKernel)
        {
            int w2 = lineKernel.Length / 2;
            int h2 = rowKernel.Length / 2;
            float acc = 0.0f;
            var oldBorder = src.BorderOptions;
            if (src.BorderOptions == Border.Empty)
                src.BorderOptions = Border.Zeros;
            var tmp = new FrameF32(src.Width, src.Height, src.BorderOptions);

            for (int j = 0; j < src.Height; j++)
            {
                for (int i = 0; i < src.Width; i++)
                {
                    for (int i2 = i - w2; i2 < i + w2; i++)
                        acc += lineKernel[i2 - i + w2] * src[i2, j];
                    tmp[i, j] = acc;
                    acc = 0.0f;
                }
            }

            src.BorderOptions = oldBorder;

            for (int i = 0; i < src.Width; i++)
            {
                for (int j = 0; j < src.Height; j++)
                {
                    for (int j2 = j - h2; j2 < j + h2; j++)
                        acc += rowKernel[j2 - j + h2] * tmp[i, j2];
                    dst[i, j] = (int)acc;
                    acc = 0.0f;
                }
            }
        }
        public static void Convolve(IImageLike<byte> src, IImageLike<byte> dst, float[] lineKernel, float[] rowKernel)
        {
            int w2 = lineKernel.Length / 2;
            int h2 = rowKernel.Length / 2;
            float acc = 0.0f;
            var oldBorder = src.BorderOptions;
            if (src.BorderOptions == Border.Empty)
                src.BorderOptions = Border.Zeros;
            var tmp = new FrameF32(src.Width, src.Height, src.BorderOptions);

            for (int j = 0; j < src.Height; j++)
            {
                for (int i = 0; i < src.Width; i++)
                {
                    for (int i2 = i - w2; i2 < i + w2; i++)
                        acc += lineKernel[i2 - i + w2] * src[i2, j];
                    tmp[i, j] = acc;
                    acc = 0.0f;
                }
            }

            src.BorderOptions = oldBorder;

            for (int i = 0; i < src.Width; i++)
            {
                for (int j = 0; j < src.Height; j++)
                {
                    for (int j2 = j - h2; j2 < j + h2; j++)
                        acc += rowKernel[j2 - j + h2] * tmp[i, j2];
                    dst[i, j] = (byte)acc;
                    acc = 0.0f;
                }
            }
        }
        public static void Convolve(IImageLike<float> src, IImageLike<float> dst, float[] lineKernel, float[] rowKernel)
        {
            int w2 = lineKernel.Length / 2;
            int h2 = rowKernel.Length / 2;
            float acc = 0.0f;
            var oldBorder = src.BorderOptions;
            if (src.BorderOptions == Border.Empty)
                src.BorderOptions = Border.Zeros;
            var tmp = new FrameF32(src.Width, src.Height, src.BorderOptions);

            for (int j = 0; j < src.Height; j++)
            {
                for (int i = 0; i < src.Width; i++)
                {
                    for (int i2 = i - w2; i2 < i + w2; i++)
                        acc += lineKernel[i2 - i + w2] * src[i2, j];
                    tmp[i, j] = acc;
                    acc = 0.0f;
                }
            }

            src.BorderOptions = oldBorder;

            for (int i = 0; i < src.Width; i++)
            {
                for (int j = 0; j < src.Height; j++)
                {
                    for (int j2 = j - h2; j2 < j + h2; j++)
                        acc += rowKernel[j2 - j + h2] * tmp[i, j2];
                    dst[i, j] = acc;
                    acc = 0.0f;
                }
            }
        }
        public static void Convolve(IImageLike<byte> src, IImageLike<byte> dst, float[][] kernel)
        {
            int w2 = kernel.Length / 2;
            int h2 = kernel[0].Length / 2;

            float acc = 0.0f;
            var oldBorder = src.BorderOptions;
            if (src.BorderOptions == Border.Empty)
                src.BorderOptions = Border.Zeros;

            for (int i = 0; i < src.Width; i++)
            {
                for (int j = 0; j < src.Height; j++)
                {
                    for (int i2 = i - w2; i < i + w2; i++)
                    {
                        for (int j2 = j - h2; j < j + h2; j++)
                            acc += src[i2, j2] * kernel[i2 - i + w2][j2 - j + h2];
                    }
                    dst[i, j] = (byte)acc;
                    acc = 0.0f;
                }
            }
            src.BorderOptions = oldBorder;
        }
        public static void Convolve(IImageLike<int> src, IImageLike<int> dst, float[][] kernel)
        {
            int w2 = kernel.Length / 2;
            int h2 = kernel[0].Length / 2;

            float acc = 0.0f;
            var oldBorder = src.BorderOptions;
            if (src.BorderOptions == Border.Empty)
                src.BorderOptions = Border.Zeros;

            for (int i = 0; i < src.Width; i++)
            {
                for (int j = 0; j < src.Height; j++)
                {
                    for (int i2 = i - w2; i < i + w2; i++)
                    {
                        for (int j2 = j - h2; j < j + h2; j++)
                            acc += src[i2, j2] * kernel[i2 - i + w2][j2 - j + h2];
                    }
                    dst[i, j] = (int)acc;
                    acc = 0.0f;
                }
            }
            src.BorderOptions = oldBorder;
        }
        public static void Convolve(IImageLike<float> src, IImageLike<float> dst, float[][] kernel)
        {
            int w2 = kernel.Length / 2;
            int h2 = kernel[0].Length / 2;

            float acc = 0.0f;
            var oldBorder = src.BorderOptions;
            if (src.BorderOptions == Border.Empty)
                src.BorderOptions = Border.Zeros;

            for (int i = 0; i < src.Width; i++)
            {
                for (int j = 0; j < src.Height; j++)
                {
                    for (int i2 = i - w2; i < i + w2; i++)
                    {
                        for (int j2 = j - h2; j < j + h2; j++)
                            acc += src[i2, j2] * kernel[i2 - i + w2][j2 - j + h2];
                    }
                    dst[i, j] = acc;
                    acc = 0.0f;
                }
            }
            src.BorderOptions = oldBorder;
        }
#endregion

#region Edge Detection
        public static void Sobel(IImageLike<int> src, IImageLike<int> dst)
        {
            
        }

#endregion

        #region Histogram Equalization

        public static void EqualizeHist(IImageLike<double> im, double MAX_VALUE = 255)
        {
            const int NUM_BINS = 256;
            const double NORM = 1.0 / (NUM_BINS - 1);

            var hist = new int[NUM_BINS];
            int len = im.Width * im.Height;
            for (int i = 0; i < len; i++)
                hist[(int)Math.Floor(im[i] * NORM * MAX_VALUE)]++;
            int min = hist[0];
            for (int i = 1; i < NUM_BINS; i++)
            {
                hist[i] += hist[i - 1];
                if (hist[i] > 0 && min == 0)
                    min = hist[i];
            }
            for (int i = 0; i < len; i++)
                im[i] = (hist[(int)Math.Floor(im[i] * NORM * MAX_VALUE)] - min) / (len - 1.0) * (NUM_BINS - 1.0);
        }
        public static void EqualizeHist(IImageLike<float> im, float MAX_VALUE = 255)
        {
            const int NUM_BINS = 256;
            const float NORM = 1.0f / (NUM_BINS - 1);

            var hist = new int[NUM_BINS];
            int len = im.Width * im.Height;
            for (int i = 0; i < len; i++)
                hist[(int)Math.Floor(im[i] * NORM * MAX_VALUE)]++;
            int min = hist[0];
            for (int i = 1; i < NUM_BINS; i++)
            {
                hist[i] += hist[i - 1];
                if (hist[i] > 0 && min == 0)
                    min = hist[i];
            }
            for (int i = 0; i < len; i++)
                im[i] = (hist[(int)Math.Floor(im[i] * NORM * MAX_VALUE)] - min) / (len - 1.0f) * (NUM_BINS - 1.0f);
        }
        public static void EqualizeHist(IImageLike<int> im, int MAX_VALUE = 255)
        {
            const int NUM_BINS = 256;

            var hist = new int[NUM_BINS];
            int len = im.Width * im.Height;
            for (int i = 0; i < len; i++)
                hist[im[i] * MAX_VALUE / (NUM_BINS - 1)]++;
            int min = hist[0];
            for (int i = 1; i < NUM_BINS; i++)
            {
                hist[i] += hist[i - 1];
                if (hist[i] > 0 && min == 0)
                    min = hist[i];
            }
            for (int i = 0; i < len; i++)
                im[i] = (hist[im[i] * MAX_VALUE / (NUM_BINS - 1)] - min) * (NUM_BINS - 1) / (len - 1);
        }
        public static void EqualizeHist(IImageLike<byte> im)
        {
            const int NUM_BINS = 256;

            var hist = new int[NUM_BINS];
            int len = im.Width * im.Height;
            for (int i = 0; i < len; i++)
                hist[im[i] * 255 / (NUM_BINS - 1)]++;
            int min = hist[0];
            for (int i = 1; i < NUM_BINS; i++)
            {
                hist[i] += hist[i - 1];
                if (hist[i] > 0 && min == 0)
                    min = hist[i];
            }
            for (int i = 0; i < len; i++)
                im[i] = (byte)((hist[im[i] * 255 / (NUM_BINS - 1)] - min) * (NUM_BINS - 1) / (len - 1));
        }
        #endregion

        
        /*
        /// <summary>
        /// Uses the k means algorithm starting once to find color clusters in the given frame (multichannel)
        /// </summary>
        /// <param name="k">Number of clusters</param>
        /// <param name="maxIter">Maximum number of iterations</param>
        /// <param name="frame">The frame to find clusters in</param>
        /// <param name="epsilon">A threshold which indicates when clusters are considered to not change anymore</param>
        /// <param name="MAX_VALUE">The channel width of the given frame</param>
        /// <returns>Returns the calculated clusters, dimensions (k, 3)</returns>
        public static double[][] KMeansCenter2D(int k, int maxIter, Frame3D frame, double epsilon = 1.0f, double MAX_VALUE = 256)
        {
            double[][] clusters = new double[k][];
            for (int i = 0; i < k; i++)
            {
                clusters[i] = new double[3];
                clusters[i][0] = (double)RNG.NextDouble() * MAX_VALUE;
                clusters[i][1] = (double)RNG.NextDouble() * MAX_VALUE;
                clusters[i][2] = (double)RNG.NextDouble() * MAX_VALUE;
            }

            double maxDist = double.MaxValue;
            int iter = 0;
            while (iter < maxIter)
            {
                iter++;
                if (maxDist <= epsilon)
                    break;

                double[][] sum = new double[k][];
                for (int i = 0; i < k; i++)
                    sum[i] = new double[3];
                int[] count = new int[k];
                for (int x = 0; x < frame.Width; x++)
                {
                    for (int y = 0; y < frame.Height; y++)
                    {
                        double r = frame.GetPixelAt(x, y, 0);
                        double g = frame.GetPixelAt(x, y, 1);
                        double b = frame.GetPixelAt(x, y, 2);

                        double minDist = double.MaxValue;
                        int s = 0;
                        for (int j = 0; j < k; j++)

                        {
                            double d = dist22(r, clusters[j][0], g, clusters[j][1], b, clusters[j][2]);
                            if (d < minDist)
                            {
                                minDist = d;
                                s = j;
                            }
                        }
                        sum[s][0] += r;
                        sum[s][1] += g;
                        sum[s][2] += b;
                        count[s]++;
                    }
                }
                for (int i = 0; i < k; i++)
                {
                    if (count[i] == 0)
                        continue;
                    double avR = (double)(sum[i][0] / count[i]);
                    double avG = (double)(sum[i][1] / count[i]);
                    double avB = (double)(sum[i][2] / count[i]);
                    double d = dist22(avR, clusters[i][0], avG,  clusters[i][1], avB, clusters[i][2]);
                    if (Math.Sqrt(d) < maxDist)
                        maxDist = Math.Sqrt(d);
                    clusters[i][0] = avR;
                    clusters[i][1] = avG;
                    clusters[i][2] = avB;
                }
            }
            
            return clusters;
        }
        */
       
        /*
        /// <summary>
        /// Sets every color in the given frame to match the specified colors given using nearest neighbor selection
        /// </summary>
        /// <param name="colors">The colors present in the resulting frame</param>
        /// <param name="frame">The frame to have the color spectrum reduced</param>
        public static void ReduceColorSpect(double[][] colors, Frame3D frame)
        {
            for (int x = 0; x < frame.Width; x++)
            {
                for (int y = 0; y < frame.Height; y++)
                {
                    double r = frame.GetPixelAt(x, y, 0);
                    double g = frame.GetPixelAt(x, y, 1);
                    double b = frame.GetPixelAt(x, y, 2);
                    double dist = double.MaxValue;
                    int index = 0;
                    for (int j = 0; j < colors.Length; j++)
                    {
                        double d = dist22(r, colors[j][0], g, colors[j][1], b, colors[j][2]);
                        if (d < dist)
                        {
                            index = j;
                            dist = d;
                        }
                    }
                    frame.SetPixelAt(x, y, colors[index][0], 0);
                    frame.SetPixelAt(x, y, colors[index][1], 1);
                    frame.SetPixelAt(x, y, colors[index][2], 2);
                }
            }
        }
        */
        

        #region NonMaximaSuppression
        /// <summary>
        /// Finds local maxima in a given frame
        /// </summary>
        /// <param name="frame">The frame to be searched</param>
        /// <param name="size">The size of the local neighborhood to be considered</param>
        /// <returns>Returns array of indices where maxima were found</returns>
        public static int[] NonMaximaSuppression<T>(IImageLike<T> frame, int size, bool useParallelization = false)
            where T : IComparable, IEquatable<T>, IConvertible
        {
            if (size % 2 == 0)
                throw new ArgumentException("size has to be odd number!");
            int n = size / 2;
            if (useParallelization)
            {
                var maxima = new ConcurrentBag<int>();
                Parallel.For(0, (frame.Width - 2 * n) / (n + 1), (x) =>
                {
                    int i = x * (n + 1) + n;
                    int mi = i;
                    for (int j = n; j < frame.Height - n; j += n + 1)
                    {
                        bool max = true;
                        int mj = j;
                        T im = frame[mi, mj];
                        for (int i2 = i; i2 <= i + n; i2++)
                        {
                            for (int j2 = j; j2 <= j + n; j2++)
                            {
                                if (frame[i2, j2].CompareTo(im) > 0)
                                {
                                    im = frame[i2, j2];
                                    mj = j2;
                                    mi = i2;
                                }
                            }
                        }

                        for (int i2 = mi - n; i2 <= mi + n; i2++)
                        {
                            for (int j2 = mj - n; j2 <= mj + n; j2++)
                            {
                                if (i2 >= i && i2 <= i + n && j2 >= j && j2 <= j + n)
                                {
                                    j2 = j + n;
                                    continue;
                                }
                                if (frame[i2, j2].CompareTo(im) >= 0)
                                {
                                    max = false;
                                    break;
                                }
                            }
                            if (!max)
                                break;
                        }
                        if (max)
                            maxima.Add(mi * frame.Height + mj);
                    }
                });

                return maxima.ToArray();
            }
            else
            {
                var maxima = new List<int>();
               
                for (int i = n; i < frame.Width - n; i += n + 1)
                {
                    int mi = i;
                    for (int j = n; j < frame.Height - n; j += n + 1)
                    {
                        bool max = true;
                        int mj = j;
                        var im = frame[mi, mj];
                        for (int i2 = i; i2 <= i + n; i2++)
                        {
                            for (int j2 = j; j2 <= j + n; j2++)
                            {
                                if (frame[i2, j2].CompareTo(im) >= 0)
                                {
                                    im = frame[i2, j2];
                                    mj = j2;
                                    mi = i2;
                                }
                            }
                        }

                        for (int i2 = mi - n; i2 <= mi + n; i2++)
                        {
                            for (int j2 = mj - n; j2 <= mj + n; j2++)
                            {
                                if (i2 >= i && i2 <= i + n && j2 >= j && j2 <= j + n)
                                {
                                    j2 = j + n;
                                    continue;
                                }
                                if (frame[i2, j2].CompareTo(im) >= 0)
                                {
                                    max = false;
                                    break;
                                }
                            }
                            if (!max)
                                break;
                        }
                        if (max)
                            maxima.Add(mi * frame.Height + mj);
                    }
                }

                return maxima.ToArray();
            }
        }
        
        /// <summary>
        /// Finds local maxima in a given frame
        /// </summary>
        /// <param name="frame">The frame to be searched</param>
        /// <param name="size">The size of the local neighborhood considered</param>
        /// <param name="resultField">A bytemap where maxima will be marked by the label</param>
        /// <param name="useParallelization">Indicates whether parallelization should be used or not</param>
        public static void NonMaximaSuppression<T>(IImageLike<T> frame, int size, byte[] result, byte label = 1, bool useParallelization = false)
            where T : IComparable, IEquatable<T>, IConvertible
        {
            foreach (int i in NonMaximaSuppression(frame, size, useParallelization))
                result[i] = label;
        }


        /// <summary>
        /// Finds local maxima in given space
        /// </summary>
        /// <param name="space">The space to be searched</param>
        /// <param name="size">The size of the local neighborhood considered</param>
        /// <param name="useParallelization">Indicates whether parallelization should be used or not</param>
        /// <returns>Returns an array of points where maxima were found: .X - index of the frame, .Y index within the frame</returns>
        public static Vec2I[] NonMaximaSuppression<T>(IImageLike<T>[] space, int size, bool useParallelization = false)
            where T : IComparable, IEquatable<T>, IConvertible
        {
            
            if (size % 2 == 0)
                throw new ArgumentException("size has to be odd number!");
            int n = size / 2;
            int height = space[0].Height;
            int width = space[0].Width;
            if (useParallelization)
            {
                var maxima = new ConcurrentBag<Vec2I>();

                Parallel.For(0, (space.Length - 2 * n) / (n + 1), (x) =>
                {
                    int k = n + x * (n + 1);

                    int mk = k;
                    for (int i = n; i < width - n; i += n + 1)
                    {
                        int mi = i;
                        for (int j = n; j < height - n; j += n + 1)
                        {
                            bool max = true;
                            int mj = j;
                            T im = space[mk][mi, mj];
                            for (int k2 = k; k2 <= k + n; k2++)
                            {
                                for (int i2 = i; i2 <= i + n; i2++)
                                {
                                    for (int j2 = j; j2 <= j + n; j2++)
                                    {
                                        if (space[k2][i2, j2].CompareTo(im) > 0)
                                        {
                                            im = space[k2][i2, j2];
                                            mj = j2;
                                            mi = i2;
                                            mk = k2;
                                        }
                                    }
                                }
                            }

                            for (int k2 = mk - n; k2 <= mk + n; k2++)
                            {
                                for (int i2 = mi - n; i2 <= mi + n; i2++)
                                {
                                    for (int j2 = mj - n; j2 <= mj + n; j2++)
                                    {
                                        if (i2 >= i && i2 <= i + n && k2 >= k && k2 <= k + n && j2 >= j && j2 <= j + n)
                                        {
                                            j2 = j + n;
                                            continue;
                                        }
                                        if (space[k2][i2, j2].CompareTo(im) >= 0)
                                        {
                                            max = false;
                                            break;
                                        }
                                    }
                                    if (!max)
                                        break;
                                }
                            }
                            if (max)
                                maxima.Add(new Vec2I(mk, mi * height + mj));
                        }
                    }
                });

                return maxima.ToArray();
            }
            else
            {
                var maxima = new List<Vec2I>();

                for (int k = n; k < space.Length - n; k += n + 1)
                {
                    int mk = k;
                    for (int i = n; i < width - n; i += n + 1)
                    {
                        int mi = i;
                        for (int j = n; j < height - n; j += n + 1)
                        {
                            bool max = true;
                            int mj = j;
                            T im = space[mk][mi, mj];
                            for (int k2 = k; k2 <= k + n; k2++)
                            {
                                for (int i2 = i; i2 <= i + n; i2++)
                                {
                                    for (int j2 = j; j2 <= j + n; j2++)
                                    {
                                        if (space[k2][i2, j2].CompareTo(im) > 0)
                                        {
                                            im = space[k2][i2, j2];
                                            mj = j2;
                                            mi = i2;
                                            mk = k2;
                                        }
                                    }
                                }
                            }

                            for (int k2 = mk - n; k2 <= mk + n; k2++)
                            {
                                for (int i2 = mi - n; i2 <= mi + n; i2++)
                                {
                                    for (int j2 = mj - n; j2 <= mj + n; j2++)
                                    {
                                        if (i2 >= i && i2 <= i + n && k2 >= k && k2 <= k + n && j2 >= j && j2 <= j + n)
                                        {
                                            j2 = j + n;
                                            continue;
                                        }
                                        if (space[k2][i2, j2].CompareTo(im) >= 0)
                                        {
                                            max = false;
                                            break;
                                        }
                                    }
                                    if (!max)
                                        break;
                                }
                            }
                            if (max)
                                maxima.Add(new Vec2I(mk, mi * height + mj));
                        }
                    }
                }
                return maxima.ToArray();
            }
        }

        #endregion

        private static int getMinMedian(int x0, double m0, int x1, double m1, int x2, double m2)
        {
            //arg min of quadratic interpolation is returned
            //minimum number is m0
            return x0;
            //TODO find error in quadratic interpolation step
            double a = x2 - x0;
            double b = -x1 + x0;
            double c = -x2 * x2 + x0 * x0;
            double d = x1 * x1 - x0 * x0;
            double det = a * d - b * c;
            a /= det;
            b /= det;
            c /= det;
            d /= det;
            return (int)Math.Round(-((m1 - m0) * c + (m2 - m0) * d) / (2 * ((m1 - m0) * a + (m2 - m0) * b)));

        }

    }
}
