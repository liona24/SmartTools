using System;

using GraphicsUtility;

namespace SmartTools
{
    public enum Border
    {
        Mirror,
        Zeros,
        Empty,
        Infinity
    }

    public interface IImageLike<T> : ICloneable
        where T : IComparable, IEquatable<T>, IConvertible
    {
        T this[int i] { get; set; }
        T this[int x, int y] { get; set; }

        int Width { get; }
        int Height { get; }

        Border BorderOptions { get; set; }

        //these methods should ignore the border options
        T GetPixelAt(int x, int y);
        T GetPixelAt(int i);
        void SetPixelAt(int x, int y, T v);
        void SetPixelAt(int i, T v);

        void Apply(Func<T, T> f);
        void Apply(Func<int, int, T> f);

        void Map2<T2>(Func<T, T2> f, IImageLike<T2> dst)
            where T2 : IComparable, IEquatable<T2>, IConvertible;
        void Map2<T2>(Func<int, int, T2> f, IImageLike<T2> dst)
            where T2 : IComparable, IEquatable<T2>, IConvertible;

        SubFrame<T> GetSubFrame(int x, int y, int w, int h);
        SubFrame<T> GetSubFrame(RectI rect);
    }
}
