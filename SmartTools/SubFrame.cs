using System;
using System.Drawing;

namespace SmartTools
{
    public class SubFrame<T> : BaseFrame<T>
        where T : IConvertible, IComparable, IEquatable<T>
    {
        int offX, offY;

        IImageLike<T> root;

        public override T this[int i] 
        { 
            get { return root[i / height + offX, i % height + offY]; }
            set { root[i / height + offX, i % height + offY] = value; }
        }
        public override T this[int x, int y]
        {
            get { return root[x + offX, y + offY]; }
            set { root[x + offX, y + offY] = value; }
        }

        public int X { get { return offX; } }
        public int Y { get { return offY; } }

        public SubFrame(int x, int y, int w, int h, IImageLike<T> src)
            : base(w, h)
        {
            offX = x;
            offY = y;
            root = src;
        }
        public void Copy(IImageLike<T> destination)
        {
            for (int i = 0; i < width * height; i++)
                destination[i] = this[i];
        }

        public override object Clone()
        {
            return new SubFrame<T>(offX, offY, width, height, root);
        }

        public override T GetPixelAt(int x, int y)
        {
            return root[x + offX, y + offY];
        }
        public override T GetPixelAt(int i)
        {
            return root[i / height + offX, i % height + offY];
        }

        public override void SetPixelAt(int x, int y, T v)
        {
            root[x + offX, y + offY] = v;
        }
        public override void SetPixelAt(int i, T v)
        {
            root[i / height + offX, i % height + offY] = v;
        }

        //same here
        protected override void init(Bitmap org)
        { }
    }
}
