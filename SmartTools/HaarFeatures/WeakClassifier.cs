
namespace SmartTools.HaarFeatures
{
    public class WeakClassifier
    {
        public double Threshold { get; private set; }
        public int Parity { get; private set; }

        //for boosted set of classifiers
        public double Alpha { get; private set; }
        public RawFeatureInfo Feature { get; private set; }

        public WeakClassifier(double thresh, int parity)
            : this(thresh, parity, 0, new RawFeatureInfo())
        { }

        public WeakClassifier(double thresh, int parity, double alpha, RawFeatureInfo f)
        {
            Threshold = thresh;
            Parity = parity;
            Alpha = alpha;
            Feature = f;
        }

        public int Evaluate(double input)
        {
            if (input * Parity > Threshold * Parity)
                return 1;

            return 0;
        }

        public static bool EvaluateBoosted(WeakClassifier[] classifiers, double[] inputs)
        {
            double left = 0;
            double right = 0;
            for (int i = 0; i < inputs.Length; i++)
            {
                left += classifiers[i].Evaluate(inputs[i]) * classifiers[i].Alpha;
                right += classifiers[i].Alpha;
            }

            if (left >= right * 0.5)
                return true;

            return false;
        }
    }
}
