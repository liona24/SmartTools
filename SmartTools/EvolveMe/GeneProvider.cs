namespace SmartTools.EvolveMe
{
    public abstract class GeneProvider
    {
        public static MathNet.Numerics.Random.SystemRandomSource RNG = new MathNet.Numerics.Random.SystemRandomSource();

        protected int maxDepth;
        public int MaxDepth { get { return maxDepth; } }

        public double LastResult { get; set; }

        public GeneProvider() { }

        public abstract void Generate(int maxDepth, int numPossibleVariables, int numPossibleOperations);
        public abstract GeneProvider[] Reproduce(GeneProvider other);

        public abstract void Mutate(int numPossibleVariables, int numPossibleOperations);

        public abstract T Evaluate<T>(FunctionDictionary<T> dict, T[] inputs);

    }
}
