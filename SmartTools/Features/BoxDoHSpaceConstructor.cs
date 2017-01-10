namespace SmartTools.Features
{
    public class BoxDoHSpaceConstructor : IScaleSpaceConstructing
    {

        public ScaleSpace Construct(FrameF64 integralImage, int numOctaves)
        {
            var space = new FrameF64[numOctaves * 4];
            for (int i = 0; i < numOctaves;i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    int l = (2 << i) * (j+1) + 1;
                    int l2 = l / 2;
                    int step = 1 << i;
                    step = 1;
                    var active = new FrameF64(integralImage.Width, integralImage.Height);
                    
                    for (int x = 0; x < integralImage.Width; x += step)
                    {
                        for (int y = 0; y < integralImage.Height; y += step)
                        {
                            double dyy = integralImage[x - l, y - l - l2] + integralImage[x + l, y + l + l2] - integralImage[x - l, y + l + l2] - integralImage[x + l, y - l - l2];
                            dyy -= 3 * (integralImage[x - l, y - l2] + integralImage[x + l, y + l2] - integralImage[x + l, y - l2] - integralImage[x - l, y + l2]);

                            double dxx = integralImage[x - l - l2, y - l] + integralImage[x + l + l2, y + l] - integralImage[x + l + l2, y - l] - integralImage[x - l - l2, y + l];
                            dxx -= 3 * (integralImage[x - l2, y - l] + integralImage[x + l2, y + l] - integralImage[x - l2, y + l] - integralImage[x + l2, y - l]);

                            double dxy = -integralImage[x - l, y - l] - integralImage[x - 1, y - 1] + integralImage[x - l, y - 1] + integralImage[x - 1, y - l];
                            dxy -= integralImage[x + 1, y + 1] + integralImage[x + l, y + l] - integralImage[x + 1, y + l] - integralImage[x + l, y + 1];
                            dxy += integralImage[x + 1, y - l] + integralImage[x + l, y - 1] - integralImage[x + 1, y - 1] - integralImage[x + l, y - l];
                            dxy += integralImage[x - l, y + 1] + integralImage[x - 1, y + l] - integralImage[x - l, y + l] - integralImage[x - 1, y + 1];

                            active[x, y] = (double)(1.0 / (l*l*l*l) * (dxx * dyy - 0.8317 * dxy * dxy));
                        }
                    }

                    space[i * 4 + j] = active;
                }
            }

            return new ScaleSpace(space, integralImage.Width, integralImage.Height, numOctaves);
        }
    }
}
