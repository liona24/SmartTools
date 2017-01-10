using System;

namespace SmartTools
{
    public static class IntegralImage
    {
        #region Doubles
        /// <summary>
        /// Computes integral image of a given image, whilst extending the image by borderHor, borderVer pixels in x, y direction respectively
        /// </summary>
        /// <returns>Returns the computed integral image</returns>
        public static FrameF64 ComputeIntegralImage<T>(IImageLike<T> rIm, int borderHor, int borderVer)
            where T : IComparable, IEquatable<T>, IConvertible
        {
            var extended = new FrameF64(rIm.Width + borderHor * 2, rIm.Height + borderVer * 2);
            for (int i = 0; i < extended.Width; i++)
            {
                for (int j = 0; j < extended.Height; j++)
                {
                    extended[i, j] = Convert.ToDouble(rIm[i - borderHor, j - borderVer]);
                }
            }
            return ComputeIntegralImage(extended);
        }
        /// <summary>
        /// Computes integral image of the given image
        /// </summary>
        /// <returns>Returns the computed integral image</returns>
        public static FrameF64 ComputeIntegralImage<T>(IImageLike<T> rIm)
            where T : IComparable, IEquatable<T>, IConvertible
        {
            double sumR = 0;
            double[] intIm = new double[rIm.Width * rIm.Height];
            for (int i = 0; i < rIm.Height; i++)
            {
                sumR += Convert.ToDouble(rIm[i]);
                intIm[i] = sumR;
            }

            for (int j = 1; j < rIm.Width; j++)
            {
                sumR = 0;
                for (int i = 0; i < rIm.Height; i++)
                {
                    sumR += Convert.ToDouble(rIm[j * rIm.Height + i]);
                    intIm[j * rIm.Height + i] = sumR + intIm[j * rIm.Height + i - rIm.Height];
                }
            }

            return new FrameF64(rIm.Width, rIm.Height, intIm);
        }
        /// <summary>
        /// Computes the integral image of the given image using its squared values
        /// </summary>
        public static FrameF64 ComputeIntegralImage2<T>(IImageLike<T> rIm)
            where T : IComparable, IEquatable<T>, IConvertible
        {
            double sumR = 0;
            double[] intIm = new double[rIm.Width * rIm.Height];
            for (int i = 0; i < rIm.Height; i++)
            {
                sumR += Convert.ToDouble(rIm[i]) * Convert.ToDouble(rIm[i]);
                intIm[i] = sumR;
            }

            for (int j = 1; j < rIm.Width; j++)
            {
                sumR = 0;
                for (int i = 0; i < rIm.Height; i++)
                {
                    sumR += Convert.ToDouble(rIm[j * rIm.Height + i]) * Convert.ToDouble(rIm[j * rIm.Height + i]);
                    intIm[j * rIm.Height + i] = sumR + intIm[j * rIm.Height + i - rIm.Height];
                }
            }

            return new FrameF64(rIm.Width, rIm.Height, intIm);
        }
        #endregion

        public static double GetMeanOfROI(int x, int y, int w, int h, FrameF64 integralImage)
        {
            return (integralImage[x, y] + integralImage[x + w, y + h] - integralImage[x, y + h] - integralImage[x + w, y]) / (w * h);
        }
        public static double GetStdDevOfROI(int x, int y, int w, int h, FrameF64 integralImage, FrameF64 integralImage2)
        {
            double sum = integralImage[x, y] + integralImage[x + w, y + h] - integralImage[x, y + h] - integralImage[x + w, y];
            return GetMeanOfROI(x, y, w, h, integralImage2) - sum * sum / (w * h * w * h);
        }
    }
}
