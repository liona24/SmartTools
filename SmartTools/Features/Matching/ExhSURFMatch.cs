namespace SmartTools.Features
{
    public class ExhSURFMatch : ExhNNSearch
    {
        public MiniGL.Vec2I[] Match (DescriptorSURF[] set1, DescriptorSURF[] set2, double thresh)
        {
            return base.Match(set1, set2, thresh);
        }
        protected override double distance2(IFeatureDescripting f1, IFeatureDescripting f2)
        {
            var s1 = f1 as DescriptorSURF;
            var s2 = f2 as DescriptorSURF;

            if (s1.Sign != s2.Sign)
                return double.MaxValue;

            double sum = 0;
            for (int i = 0; i < s1.Length - 2; i++)
                sum += (s1[i] - s2[i]) * (s1[i] - s2[i]);

            return sum;
        }
    }
}
