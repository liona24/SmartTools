namespace SmartTools.Features
{
    public class ScaleSpace
    {
        protected IImageLike<double>[] layers;
        protected int numOctaves;
        protected int width, height;

        public double this[int s, int x, int y] { get { return layers[s][x, y]; } }
        public double this[int s, int i] { get { return layers[s][i]; } }
        public IImageLike<double> this[int s] { get { return layers[s]; } }


        public IImageLike<double>[] Layers { get { return layers; } }
        public int Length { get { return layers.Length; } }
        public int LengthPerOctave { get { return layers.Length / numOctaves; } }
        public int Width { get { return width; } }
        public int Height { get { return height; } }
        public int NumOctaves { get { return numOctaves; } }

        public ScaleSpace(IImageLike<double>[] layers, int width, int height, int numOctaves)
        {
            this.width = width;
            this.height = height;
            this.layers = layers;
            this.numOctaves = numOctaves;
        }
    }
}
