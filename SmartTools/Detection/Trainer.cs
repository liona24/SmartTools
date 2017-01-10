using System;

using MiniGL;

namespace SmartTools.Detection
{
    public abstract class Trainer
    {
        public static readonly Random RNG = new Random();

        //refers to the border around each positive sample which determines if the classification is correct
        protected int generousPixelBorder;
        protected int minWidth, minHeight;
        protected bool inProcess;

        public bool IsWorking
        {
            get { return inProcess; }
        }

        public Trainer()
        {
            inProcess = false;
        }

        public abstract void Train<T>(IImageLike<T>[] images, RectI[] accordingGroundTruth, int numNegativSamplesPerIm)
            where T : IComparable, IEquatable<T>, IConvertible;
        public abstract void Train<T>(IImageLike<T>[] positives, IImageLike<T>[] negatives)
            where T : IComparable, IEquatable<T>, IConvertible;

        protected SubFrame<T>[] getNegSamples<T>(IImageLike<T> im, RectI groundTruth, int num)
            where T : IComparable, IEquatable<T>, IConvertible
        {
            var res = new SubFrame<T>[num];
            if (groundTruth.Height == im.Height || groundTruth.Width == im.Width)
                throw new Exception("Cannot find negative sample in full positive image");

            var area51 = new RectI(groundTruth.L - generousPixelBorder, groundTruth.T - generousPixelBorder, groundTruth.R + generousPixelBorder, groundTruth.T + generousPixelBorder);
            for (int i = 0; i < num; i++)
            {
                bool accepted = false;
                int w = minWidth;
                int h = minHeight;
                int x = 0;
                int y = 0;
                do
                {
                    x = RNG.Next(im.Width - w);
                    y = RNG.Next(im.Height - h);
                    if (area51.Contains(RectI.FromXYWH(x, y, w, h)))
                        accepted = false;
                    else
                        accepted = true;
                } while (!accepted);
                res[i] = new SubFrame<T>(x, y, w, h, im);
            }
            return res;
        }
    }
}
