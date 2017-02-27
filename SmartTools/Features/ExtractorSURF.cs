using System;
using System.Collections.Generic;

using GraphicsUtility;

namespace SmartTools.Features
{
    /// <summary>
    /// SURF Feature extractor
    /// </summary>
    public class ExtractorSURF : IFeatureExtracting
    {
        protected double threshold;
        protected int numOctaves;
        protected int numAngleSectors;

        public ExtractorSURF(double threshold, int numOctaves, int numAngleSectors)
        {
            this.threshold = threshold;
            this.numOctaves = numOctaves;
            this.numAngleSectors = numAngleSectors;
        }
        public ExtractorSURF(double threshold, int numOctaves)
            : this(threshold, numOctaves, 40)
        { }
        public ExtractorSURF()
            : this (1000, 3, 40)
        { }

        public IFeatureDescripting[] Extract(FrameF64 integralImage)
        {
            var spaceCons = new BoxDoHSpaceConstructor();
            var space = spaceCons.Construct(integralImage, numOctaves);
            var interest = new List<DescriptorSURF>();

            var max = Imaging.NonMaximaSuppression(space.Layers, 3);
            for (int i = 0; i < max.Length; i++)
            {
                var c = max[i];
                if (space[c.X][c.Y] > threshold)
                {
                    int oct = c.X / 4;
                    int level = c.X - oct * 4;
                    int x = c.Y / space.Height;
                    int y = c.Y % space.Height;

                    var refined = refine(x, y, level, oct, space);
                    if (refined.X > -1)
                        interest.Add(makeDesc(refined, integralImage));
                       
                }
            }

            return interest.ToArray();
        }

        private Vec3 refine(int x, int y, int level, int oct, ScaleSpace s)
        {
            int p = 1 << oct;
            int l = (2 << oct) * (level + 1) + 1;
            int scale = oct * 4 + level;
            double hxx, hyy, hxy, hxl, hyl, hll;

            var d = new Vec3(-0.5 / p * (s[scale][x + p, y] - s[scale][x - p, y]),
                -0.5 / p * (s[scale][x, y + p] - s[scale][x, y - p]),
                -0.25 / p * (s[scale + 1][x, y] - s[scale - 1][x, y]));

            hxx = 1.0 / (p * p) * (s[scale][x - p, y] + s[scale][x + p, y] - 2 * s[scale][x, y]);
            hyy = 1.0 / (p * p) * (s[scale][x, y - p] + s[scale][x, y - p] - 2 * s[scale][x, y]);
            hll = 0.25 / (p * p) * (s[scale - 1][x, y] + s[scale + 1][x, y] - 2 * s[scale][x, y]);

            hxy = 0.25 / (p * p) * (s[scale][x - p, y - p] + s[scale][x + p, y + p] - s[scale][x - p, y + p] - s[scale][x + p, y - p]);
            hxl = 0.125 / (p * p) * (s[scale + 1][x + p, y] + s[scale - 1][x - p, y] - s[scale + 1][x - p, y] - s[scale - 1][x + p, y]);
            hyl = 0.125 / (p * p) * (s[scale + 1][x, y + p] + s[scale - 1][x, y - p] - s[scale + 1][x, y - p] - s[scale - 1][x, y + p]);

            Matrix3 hess = new Matrix3(new double[] { hxx, hxy, hxl,
                                                      hxy, hyy, hyl,
                                                      hxl, hyl, hll });
            if (hess.Inverse())
            {
                var z = hess * d;
                if (Math.Abs(z.X) < p && Math.Abs(z.Y) < p && Math.Abs(z.Z) < 2 * p)
                    return new Vec3(x + z.X, y + z.Y, l + z.Z);
            }

            return new Vec3(-1, -1, -1);

        }
        private double calcOrientation(double x, double y, double l, FrameF64 intIm)
        {
            double sigma = 0.4 * l;
            int k = (int)Math.Round(l * 0.8);
            double dx, dy;
            int ix, iy;
            int s;
            double sectorWI = numAngleSectors / (2 * Math.PI); //inversed width
            var thetaX = new double[numAngleSectors];
            var thetaY = new double[numAngleSectors];

            for (int i = -6; i < 7; i++)
            {
                ix = (int)(x + i * sigma);
                for (int j = -6; j < 7; j++)
                {
                    if (i * i + j * j > 36)
                        continue;
                    iy = (int)(y + j * sigma);
                    dx = intIm[ix - k, iy - k] + intIm[ix - 1, iy + k] - intIm[ix - k, iy + k] - intIm[ix - 1, iy - k] -
                        (intIm[ix + 1, iy - k] + intIm[ix + k, iy + k] - intIm[ix + k, iy - k] - intIm[ix + 1, iy + k]);
                    dy = intIm[ix - k, iy - k] + intIm[ix + k, iy - 1] - intIm[ix + k, iy - k] - intIm[ix - k, iy - 1] -
                        (intIm[ix - k, iy + 1] + intIm[ix + k, iy + k] - intIm[ix - k, iy + k] - intIm[ix + k, iy + 1]);

                    dx *= gauss1(ix, iy);
                    dy *= gauss1(ix, iy);

                    s = (int)Math.Round(Math.Atan2(dy, dx) * sectorWI);
                    if (s < 0)
                        s += numAngleSectors;
                    for (int i2 = -numAngleSectors / 12; i2 <= numAngleSectors / 12; i2++)
                    {
                        int index = i2 + s;
                        if (index < 0)
                            index += numAngleSectors;
                        if (index >= numAngleSectors)
                            index -= numAngleSectors;
                        thetaX[index] += dx;
                        thetaY[index] += dy;
                    }
                }
            }

            double max = thetaX[0] * thetaX[0] + thetaY[0] * thetaY[0];
            int maxI = 0;
            for (int i = 1; i < numAngleSectors; i++)
            {
                double n = thetaX[i] * thetaX[i] + thetaY[i] * thetaY[i];
                if (n > max)
                {
                    max = n;
                    maxI = i;
                }
            }

            return Math.Atan2(thetaY[maxI], thetaX[maxI]);
        }
        private DescriptorSURF makeDesc(Vec3 k, FrameF64 intIm)
        {
            double theta = calcOrientation(k.X, k.Y, k.Z, intIm);
            var kxy = new Vec2(k.X, k.Y);
            var rot = new Matrix2(Math.Cos(theta), -Math.Sin(theta), Math.Sin(theta), Math.Cos(theta)) * (k.Z * 0.4);
            var rotI = (Matrix2)rot.Clone();
            rotI.Transpose();

            int x, y;
            double dx, dy;
            double g;
            double sumX, sumY, sumAX, sumAY;
            double sum2 = 0;
            int L = (int)Math.Round(k.Z * 0.8);

            double[][] d = new double[16][];

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    d[i * 4 + j] = new double[4];
                    sumX = 0;
                    sumY = 0;
                    sumAY = 0;
                    sumAX = 0;
                    for (int u = -9; u < 10; u++)
                    {
                        for (int v = -9; v < 10; v++)
                        {
                            var xy = rot * new Vec2(u + 0.5, v + 0.5) + kxy;
                            x = (int)Math.Round(xy.X);
                            y = (int)Math.Round(xy.Y);

                            dx = intIm[x - L, y - L] + intIm[x - 1, y + L] - intIm[x - L, y + L] - intIm[x - 1, y - L] -
                                (intIm[x + 1, y - L] + intIm[x + L, y + L] - intIm[x + L, y - L] - intIm[x + 1, y + L]);
                            dy = intIm[x - L, y - L] + intIm[x + L, y - 1] - intIm[x + L, y - L] - intIm[x - L, y - 1] -
                                (intIm[x - L, y + 1] + intIm[x + L, y + L] - intIm[x - L, y + L] - intIm[x + L, y + 1]);

                            g = gauss1((u + 0.5) / 3.33, (v + 0.5) / 3.33);
                            var ngrad = rotI * new Vec2(dx * g, dy * g);
                            
                            sumX += ngrad.X;
                            sumY += ngrad.Y;
                            sumAX += Math.Abs(ngrad.X);
                            sumAY += Math.Abs(ngrad.Y);
                        }
                    }
                    sum2 += sumX * sumX + sumY * sumY + sumAX * sumAX + sumAY * sumAY;
                    d[i * 4 + j][0] = sumX;
                    d[i * 4 + j][1] = sumY;
                    d[i * 4 + j][2] = sumAX;
                    d[i * 4 + j][3] = sumAY;

                }
            }

            sum2 = 1 / Math.Sqrt(sum2);
            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 4; j++)
                    d[i][j] *= sum2;
            }

            int L2 = L / 2;
            x = (int)Math.Round(k.X);
            y = (int)Math.Round(k.Y);
            double dyy = intIm[x - L, y - L - L2] + intIm[x + L, y + L + L2] - intIm[x - L, y + L + L2] - intIm[x + L, y - L - L2];
            dyy -= 3 * (intIm[x - L, y - L2] + intIm[x + L, y + L2] - intIm[x + L, y - L2] - intIm[x - L, y + L2]);

            double dxx = intIm[x - L - L2, y - L] + intIm[x + L + L2, y + L] - intIm[x + L + L2, y - L] - intIm[x - L - L2, y + L];
            dxx -= 3 * (intIm[x - L2, y - L] + intIm[x + L2, y + L] - intIm[x - L2, y + L] - intIm[x + L2, y - L]);

            return new DescriptorSURF(new Vec2(k.X, k.Y), d, theta, Math.Sign(dxx + dyy));
        }

        // 2d gauss kernel with sigma=1 
        private static double gauss1(double x, double y)
        {
            return 0.159154943 * Math.Exp(-(x * x + y * y) * 0.5);
        }
    }
}
