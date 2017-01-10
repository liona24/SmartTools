using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

//TODO check dependencies

namespace SmartTools.HaarFeatures
{
    public struct RawFeatureInfo
    {
        public int X;
        public int Y;
        public int Basic;
        public int Width;
        public int Height;

        public RawFeatureInfo(int basic, int x, int y, int sX, int sY)
        {
            X = x;
            Y = y;
            Basic = basic;
            Width = sX;
            Height = sY;
        }

        public override string ToString()
        {
            return "FeatureInfo {B=" + Basic + " X=" + X + " Y=" + Y + " W=" + Width + " H=" + Height + "}";
        }
    }

    public static class HaarFeatureProvider
    {
        public static readonly HaarFeature[] BasicFeatures = {
                                                                 new HaarFeature(
                                                                     new Point[] { new Point(0, 0), new Point(1, 0) },
                                                                     new int[] { -1, 1 }),
                                                                 new HaarFeature(
                                                                     new Point[] { new Point(0, 0), new Point(0, 1) },
                                                                     new int[] { 1, -1 }),
                                                                 new HaarFeature(
                                                                     new Point[] { new Point(0, 0), new Point(1, 0), new Point(2, 0) },
                                                                     new int[] { -1, 2, -1}),
                                                                 new HaarFeature(
                                                                     new Point[] { new Point(0, 0), new Point(0, 1), new Point(0, 2) },
                                                                     new int[] { -1, 2, -1}),
                                                                 new HaarFeature(
                                                                     new Point[] { new Point(0, 0), new Point(0, 1), new Point(1, 0), new Point(1, 1)},
                                                                     new int[] {1, -1, -1, 1}),
                                                                 new HaarFeature(
                                                                     new Point[] { new Point(0, 0), new Point(1, 0), new Point(2, 0),
                                                                                   new Point(0, 1), new Point(1, 1), new Point(2, 1),
                                                                                   new Point(0, 2), new Point(1, 2), new Point(2, 2) },
                                                                     new int[] { -1, -1, -1, -1, 8, -1, -1, -1, -1} )
                                                             };

        /// <summary>
        /// Returns a customized HaarFeature instance
        /// </summary>
        /// <param name="basic">Index of the basic parent haarfeature(see list HaarFeatureProvider.BasicFeatures</param>
        /// <param name="x">Left most position on x-axis</param>
        /// <param name="y">Top most position on y-axis</param>
        /// <param name="partSizeX">Width of each square within the feature</param>
        /// <param name="partSizeY"></param>
        /// <returns></returns>
        public static HaarFeature GetCustomAt(int basic, int x, int y, int partSizeX, int partSizeY)
        {
            Point[] basicRe = (Point[])BasicFeatures[basic].BasicRectangles.Clone();
            for (int i = 0; i < basicRe.Length; i++)
                basicRe[i] = new Point(BasicFeatures[basic].BasicRectangles[i].X * partSizeX + x, BasicFeatures[basic].BasicRectangles[i].Y * partSizeY + y);

            HaarFeature result = new HaarFeature(basicRe, BasicFeatures[basic].BasicRectangleValues);

            result.xScale = partSizeX;
            result.yScale = partSizeY;

            return result;
        }

        public static HaarFeature GetCustomAt(RawFeatureInfo feature)
        {
            return GetCustomAt(feature.Basic, feature.X, feature.Y, feature.Width, feature.Height);
        }

        public static RawFeatureInfo[] CreateFullSet(int imWidth, int imHeight, int featureRange)
        {
            List<RawFeatureInfo> res = new List<RawFeatureInfo>();
            for (int i = 0; i < featureRange; i++)
            {
                int numX = 2;
                int numY = 1;
                switch (i)
                {
                    case 0:
                        numX = 2;
                        numY = 1;
                        break;
                    case 1:
                        numX = 1;
                        numY = 2;
                        break;
                    case 2:
                        numX = 3;
                        numY = 1;
                        break;
                    case 3:
                        numX = 1;
                        numY = 3;
                        break;
                    case 4:
                        numX = 2;
                        numY = 2;
                        break;
                    case 5:
                        numX = 3;
                        numY = 3;
                        break;
                }
                for (int sX = imWidth / numX; sX >= 2; sX--)
                {
                    for (int sY = imHeight / numY; sY >= 2; sY--)
                    {
                        for (int x = 0; x < imWidth - numX * sX; x++)
                        {
                            for (int y = 0; y < imHeight - numY * sY; y++)
                                res.Add(new RawFeatureInfo(i, x, y, sX, sY));
                        }
                    }
                }
            }

            return res.ToArray();
        }
    }
}
