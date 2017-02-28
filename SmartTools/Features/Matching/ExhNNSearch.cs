using System.Collections.Generic;

namespace SmartTools.Features
{
    public class ExhNNSearch : IFeatureMatching
    {
        public GraphicsUtility.Vec2I[] Match(IFeatureDescripting[] set1, IFeatureDescripting[] set2, double thresh)
        {
            var matches = new List<GraphicsUtility.Vec2I>();

            double min;
            int m;

            thresh *= thresh;

            for (int i = 0; i < set1.Length; i++)
            {
                m = -1;
                min = double.MaxValue;
                for (int j = 0; j < set2.Length; j++)
                {
                    double dis2 = distance2(set1[i], set2[j]);
                    if (dis2 < thresh)
                    {
                        if (min > dis2)
                        {
                            min = dis2;
                            m = j;
                        }
                    }
                }

                if (m != -1)
                    matches.Add(new GraphicsUtility.Vec2I(i, m));
            }

            return matches.ToArray();
        }

        protected virtual double distance2(IFeatureDescripting f1, IFeatureDescripting f2)
        {
            double sum = 0;
            for (int i = 0; i < f1.Length; i++)
                sum += (f1[i] - f2[i]) * (f1[i] - f2[i]);

            return sum;
        }
    }
}
