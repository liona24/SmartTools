using System;
using System.Collections;
using System.Collections.Generic;
using MiniGL;

namespace SmartTools
{
    

    public class SuperPixel<T> : PixelCollection<T>
        where T : IComparable, IEquatable<T>, IConvertible
    {
        RectI boundaries;
        Vec2I center;

        public RectI Boundaries { get { return boundaries; } }
        public Vec2I Center { get { return center; } }

        #region Constructors
        public SuperPixel(IEnumerable<Vec2I> indices, IImageLike<T> im)
            : base(indices, im)
        {
            calcCenterAndBounds();
        }
        public SuperPixel(Vec2I[] indices, IImageLike<T> im)
            : base(indices, im)
        {
            calcCenterAndBounds();
        }
        public SuperPixel(IEnumerable<Vec2I> indices, IImageLike<T> im, RectI boundaries, Vec2I center)
            : base(indices, im)
        {
            this.boundaries = boundaries;
            this.center = center;
        }
        public SuperPixel(Vec2I[] indices, IImageLike<T> im, RectI boundaries, Vec2I center)
            : base(indices, im)
        {
            this.boundaries = boundaries;
            this.center = center;
        }
        #endregion

        public Vec2I[] GetBorder()
        {
            int width = boundaries.Width + 2;
            int height = boundaries.Height + 2;
            var start = new Vec2I(indices[0].X + 1 - boundaries.L, indices[0].Y + 1 - boundaries.T);
            var extMap = new int[width][];
            for (int k = 0; k < width; k++)
                extMap[k] = new int[height];
            for (int k = 0; k < indices.Length; k++)
                extMap[indices[k].X - boundaries.L + 1][indices[k].Y - boundaries.T + 1] = 1;
            var result = new List<Vec2I>();

            int i = start.X;
            for (; i < width; i++)
            {
                if (extMap[i][start.Y] != 1)
                    break;
            }
            result.Add(new Vec2I(i - 2 + boundaries.L, start.Y - 1 + boundaries.T));
            int fi = i - 1;
            int fj = start.Y;
            int j = start.Y;
            Vec2I dir = new Vec2I(0, 1);
            //special case of having a contour like this:
            // ##
            // ### <- start here
            // ##
            // requires the stop criteria to be ignored once, therefor special loop that covers this case
            for (int k = 0; k < 4; k++)
            {
                i += dir.X;
                j += dir.Y;

                if (extMap[i][j] == 1)
                {
                    var last = result[result.Count - 1];
                    if (last.X - boundaries.L != i - 1 || last.Y - boundaries.T != j - 1)
                        result.Add(new Vec2I(i - 1 + boundaries.L, j - 1 + boundaries.T));
                    dir = new Vec2I(dir.Y, -dir.X);
                }
                else
                    dir = new Vec2I(-dir.Y, dir.X);
            }
            do
            {
                i += dir.X;
                j += dir.Y;

                if (extMap[i][j] == 1)
                {
                    var last = result[result.Count - 1];
                    if (last.X - boundaries.L != i - 1 || last.Y - boundaries.T != j - 1)
                        result.Add(new Vec2I(i - 1 + boundaries.L, j - 1 + boundaries.T));
                    dir = new Vec2I(dir.Y, -dir.X);
                }
                else
                    dir = new Vec2I(-dir.Y, dir.X);

            } while (i != fi || j != fj);
            result.RemoveAt(result.Count - 1);
            return result.ToArray();
        }

        private void calcCenterAndBounds()
        {
            int minX = int.MaxValue;
            int minY = int.MaxValue;
            int maxX = int.MinValue;
            int maxY = int.MinValue;
            int sX = 0;
            int sY = 0;
            for (int i = 0; i < indices.Length; i++)
            {
                sX += indices[i].X;
                sY += indices[i].Y;
                if (indices[i].X < minX)
                    minX = indices[i].X;
                if (indices[i].X > maxX)
                    maxX = indices[i].X;
                if (indices[i].Y < minY)
                    minY = indices[i].Y;
                if (indices[i].Y > maxY)
                    maxY = indices[i].Y;
            }
            boundaries = new RectI(minX, minY, maxX + 1, maxY + 1);
            center = new Vec2I(sX / indices.Length, sY / indices.Length);
        }

        #region Factory
        public static SuperPixel<byte>[] Calculate(BaseFrameN<byte> im, int count, double spatialWeight, double convergenceThreshold)
        {
            Func<int, int, int, int, double> colorDist = (x, y, cX, cY) => 
            {
                int res = 0;
                for (int i = 0; i < im.NumChannels; i++)
                {
                    int d = im.GetPixelAt(cX, cY, i);
                    d -= im.GetPixelAt(x, y, i);
                    res += d * d;
                }
                return res;
            };

            return _calculate(im, colorDist, spatialWeight, count, convergenceThreshold); 
        }
        public static SuperPixel<byte>[] Calculate(BaseFrame<byte> im, int count, double spatialWeight, double convergenceThreshold)
        {
            Func<int, int, int, int, double> colorDist = (x, y, cX, cY) => 
            {
                var d = im[x, y];
                d -= im[cX, cY];
                return d * d;
            };

            return _calculate(im, colorDist, spatialWeight, count, convergenceThreshold);
        }
        public static SuperPixel<float>[] Calculate(BaseFrameN<float> im, int count, double spatialWeight, double convergenceThreshold)
        {
            Func<int, int, int, int, double> colorDist = (x, y, cX, cY) => 
            {
                double res = 0;
                for (int i = 0; i < im.NumChannels; i++)
                {
                    double d = im[cX, cY, i];
                    d -= im[x, y, i];
                    res += d * d;
                }
                return res;
            };

            return _calculate(im, colorDist, spatialWeight, count, convergenceThreshold); 
        }
        public static SuperPixel<float>[] Calculate(BaseFrame<float> im, int count, double spatialWeight, double convergenceThreshold)
        {
            Func<int, int, int, int, double> colorDist = (x, y, cX, cY) => 
            {
                var d = im[x, y];
                d -= im[cX, cY];
                return d * d;
            };

            return _calculate(im, colorDist, spatialWeight, count, convergenceThreshold);
        }
        public static SuperPixel<double>[] Calculate(BaseFrameN<double> im, int count, double spatialWeight, double convergenceThreshold)
        {
            Func<int, int, int, int, double> colorDist = (x, y, cX, cY) => 
            {
                double res = 0;
                for (int i = 0; i < im.NumChannels; i++)
                {
                    double d = im[cX, cY, i];
                    d -= im[x, y, i];
                    res += d * d;
                }
                return res;
            };

            return _calculate(im, colorDist, spatialWeight, count, convergenceThreshold); 
        }
        public static SuperPixel<double>[] Calculate(BaseFrame<double> im, int count, double spatialWeight, double convergenceThreshold)
        {
            Func<int, int, int, int, double> colorDist = (x, y, cX, cY) => 
            {
                var d = im[x, y];
                d -= im[cX, cY];
                return d * d;
            };

            return _calculate(im, colorDist, spatialWeight, count, convergenceThreshold);
        }
        public static SuperPixel<int>[] Calculate(BaseFrameN<int> im, int count, double spatialWeight, double convergenceThreshold)
        {
            Func<int, int, int, int, double> colorDist = (x, y, cX, cY) => 
            {
                int res = 0;
                for (int i = 0; i < im.NumChannels; i++)
                {
                    int d = im.GetPixelAt(cX, cY, i);
                    d -= im.GetPixelAt(x, y, i);
                    res += d * d;
                }
                return res;
            };

            return _calculate(im, colorDist, spatialWeight, count, convergenceThreshold); 
        }
        public static SuperPixel<int>[] Calculate(BaseFrame<int> im, int count, double spatialWeight, double convergenceThreshold)
        {
            Func<int, int, int, int, double> colorDist = (x, y, cX, cY) => 
            {
                var d = im[x, y];
                d -= im[cX, cY];
                return d * d;
            };

            return _calculate(im, colorDist, spatialWeight, count, convergenceThreshold);
        }

        private static SuperPixel<T2>[] _calculate<T2>(IImageLike<T2> im, 
                           Func<int, int, int, int, double> colorDist,
                           double spatialWeight, 
                           int count, 
                           double convergenceThreshold)
            where T2 : IComparable, IEquatable<T2>, IConvertible
        {
            int s = (int)Math.Ceiling(Math.Sqrt((double)im.Width * im.Height / count));
            int s2 = s / 2;

            var link = new int[im.Width][]; //map of indices storing which center a pixel currently belongs to
            var centers = new Vec2I[count];

            int c = 0;
            for (int i = 0; i < im.Width; i++)
                link[i] = new int[im.Height];
            for (int i = s2; i < im.Width - s2; i += s)
            {
                for (int j = s2; j < im.Height - s2; j += s)
                {
                    for (int i2 = i - s2; i2 < i + s2; i2++)
                    {
                        for (int j2 = j - s2; j2 < j + s2; j2++)
                            link[i2][j2] = c;
                    }
                    centers[c++] = new Vec2I(i, j);
                }
            }
            return _slic(s, link, centers, im, colorDist, convergenceThreshold, spatialWeight);
        }

        private static SuperPixel<T2>[] _calculate<T2>(IImageLike<T2> im,
                Func<int, int, int, int, double> colorDist,
                Vec2I[] initCenters,
                int[][] initLink,
                double spatialWeight,
                double convergenceThreshold)
            where T2 : IComparable, IEquatable<T2>, IConvertible
        {

            int s = (int)Math.Ceiling(Math.Sqrt((double)im.Width * im.Height / initCenters.Length));

            return _slic(s, initLink, initCenters, im, colorDist, convergenceThreshold, spatialWeight);
        }

        private static SuperPixel<T2>[] _slic<T2>(int s, int[][] link,
                Vec2I[] centers,
                IImageLike<T2> im,
                Func<int, int, int, int, double> colorDist,
                double convergenceThreshold,
                double spatialWeight)
            where T2 : IConvertible, IEquatable<T2>, IComparable
        {
            int count = centers.Length;

            var best = new double[im.Width][];
            for (int i = 0; i < im.Width; i++)
            {
                best[i] = new double[im.Height];
                for (int j = 0; j < im.Height; j++)
                    best[i][j] = double.MaxValue;
            }

            double deltaError = 0.0;
            double lastError = double.MaxValue;
            var nCenterX = new int[count];
            var nCenterY = new int[count];
            var nCount = new int[count];
            do
            {
                double nError = 0.0;
                for (int i = 0; i < count; i++)
                {
                    if (centers[i].X == -1)
                        continue;

                    int cX = centers[i].X;
                    int cY = centers[i].Y;
                    int lowX = Math.Max(0, cX - s);
                    int highX = Math.Min(im.Width, cX + s);
                    int lowY = Math.Max(0, cY - s);
                    int highY = Math.Min(im.Height, cY + s);
                    for (int x = lowX; x < highX; x++)
                    {
                        for (int y = lowY; y < highY; y++)
                        {
                            int dSp = (cX - x) * (cX - x) + (cY - y) * (cY - y);
                            double dCo = colorDist(x, y, cX, cY);
                            double d = dCo + spatialWeight / s * dSp;
                            if (best[x][y] > d)
                            {
                                best[x][y] = d;
                                link[x][y] = i;
                            }
                        }
                    }
                }

                for (int i = 0; i < im.Width; i++)
                {
                    for (int j = 0; j < im.Height; j++)
                    {
                        int k = link[i][j];
                        best[i][j] = double.MaxValue;
                        nCenterX[k] += i;
                        nCenterY[k] += j;
                        nCount[k]++;
                    }
                }

                for (int i = 0; i < count; i++)
                {
                    if (nCount[i] == 0)
                    {
                        centers[i] = new Vec2I(-1, -1);
                        continue;
                    }
                    double dX = centers[i].X - (nCenterX[i] / (double)nCount[i]);
                    double dY = centers[i].Y - (nCenterY[i] / (double)nCount[i]);
                    centers[i] = new Vec2I(nCenterX[i] / nCount[i], nCenterY[i] / nCount[i]);
                    nError += dX * dX + dY * dY;
                    nCenterX[i] = 0;
                    nCenterY[i] = 0;
                    nCount[i] = 0;
                }
                nError /= count;
                deltaError = lastError - nError;
                lastError = nError;

            } while (deltaError > convergenceThreshold);

            var linkFin = new FrameI32(im.Width, im.Height);
            linkFin.Apply((x, y) => link[x][y]);
            for (int i = 0; i < count; i++)
            {
                if (centers[i].X > 0)
                    linkFin.Fill(centers[i], -1 - i, (a) => a == i);
            }
                
            linkFin.Apply((a) => -a - 1);

            var pixels = new List<Vec2I>[count];
            for (int i = 0; i < count; i++)
                pixels[i] = new List<Vec2I>();
            for (int i = 0; i < linkFin.Width; i++)
            {
                for (int j = 0; j < linkFin.Height; j++)
                {
                    //if (linkFin[i, j] < 0)
                    //{
                    //    //linkFin[i, j] is unassigned
                    //    var old = linkFin[i, j];
                    //    if (i > 0)
                    //        linkFin[i, j] = linkFin[i - 1, j];
                    //    else if (j > 0)
                    //        linkFin[i, j] = linkFin[i, j - 1];
                    //    else
                    //    {
                    //        int closeX = 0;
                    //        for (; closeX < linkFin.Width; closeX++)
                    //        {
                    //            if (linkFin[closeX, 0] >= 0)
                    //                break;
                    //        }
                    //        int closeY = 0;
                    //        for (; closeY < linkFin.Height; closeY++)
                    //        {
                    //            if (linkFin[0, closeY] >= 0)
                    //                break;
                    //        }
                    //        if (closeY < closeX)
                    //            linkFin[0, 0] = linkFin[0, closeY];
                    //        else
                    //            linkFin[0, 0] = linkFin[closeX, 0];
                    //    }
                    //}
                    if (linkFin[i, j] >= 0) pixels[linkFin[i, j]].Add(new Vec2I(i, j));
                }
            }

            var res = new List<SuperPixel<T2>>(count);
            for (int i = 0; i < count; i++)
            {
                if (pixels[i].Count > 0)
                    res.Add(new SuperPixel<T2>(pixels[i].ToArray(), im));
            }
            return res.ToArray();
        }

        
        #endregion
    }


}