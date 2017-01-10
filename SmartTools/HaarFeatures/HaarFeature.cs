using System.Drawing;

namespace SmartTools.HaarFeatures
{
    public class HaarFeature
    {
        public Point[] BasicRectangles;
        public int[] BasicRectangleValues;

        public int yScale;
        public int xScale;

        public HaarFeature(Point[] basicRectangles, int[] basicRectangleValues)
        {
            BasicRectangles = basicRectangles;
            BasicRectangleValues = basicRectangleValues;

            yScale = 2;
            xScale = 2;
        }

        public double GetValue(FrameF64 integralImage)
        {
            double val = 0;
            for (int i = 0; i < BasicRectangles.Length; i++)
            {
                //note yScale, xScale respectevly: width and height of basicRectangles is 1
                val += BasicRectangleValues[i] * GetRectangleValue(integralImage,
                                                                        BasicRectangles[i].Y,
                                                                        BasicRectangles[i].Y + yScale - 1,
                                                                        BasicRectangles[i].X,
                                                                        BasicRectangles[i].X + xScale - 1);
            }

            return val;
        }

        private double GetRectangleValue(FrameF64 integralImage, int top, int bot, int left, int right)
        {
            return integralImage.GetPixelAt(left, top) +
                integralImage.GetPixelAt(right, bot) -
                integralImage.GetPixelAt(right, top) -
                integralImage.GetPixelAt(left, bot);
        }
    }
}
