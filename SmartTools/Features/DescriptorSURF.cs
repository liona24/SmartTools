using GraphicsUtility;

namespace SmartTools.Features
{
    public class DescriptorSURF : IFeatureDescripting
    {
        double[] info;
        Vec2 pos;

        public double this[int i] { get { return info[i]; } }
        public int Length { get { return info.Length; } }

        public Vec2 Position { get { return pos; } }

        public double Angle { get { return info[info.Length - 2]; } }
        public int Sign { get { return (int)info[info.Length - 1]; } }

        public DescriptorSURF(Vec2 position, double[][] u, double angle, int sign)
        {
            info = new double[64 + 2];
            int k = 0;
            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 4; j++)
                    info[k++] = u[i][j];
            }
            info[k++] = angle;
            info[k] = sign;

            pos = position;
        }
    }
}
