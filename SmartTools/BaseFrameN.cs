using System;
using System.Collections;
using System.Collections.Generic;

using GraphicsUtility;

namespace SmartTools
{
    public abstract class BaseFrameN<T> : IImageLike<T>, IEnumerable<T>
        where T : IComparable, IEquatable<T>, IConvertible
    {
        protected T[][] values;
        protected int ch;
        protected readonly int numChannels;
        protected int width, height;

        public int ActiveChannel
        {
            get { return ch; }
            set { if (value < numChannels && value >= 0) ch = value; }
        }
        public int NumChannels { get { return numChannels; } }
        public T this[int i]
        {
            get { return values[ch][i]; }
            set { values[ch][i] = value; }
        }
        public T this[int x, int y]
        {
            get
            {
                if (x >= 0 && y >= 0 && x < width && y < height) return values[ch][x * height + y];
                else
                {
                    switch (BorderOptions)
                    {
                        case Border.Empty:
                            if (x < 0 || x >= width)
                                throw new ArgumentOutOfRangeException("x", x, "Argument out of range when accessing underlaying values. Make sure coordinates are within boundaries!");
                            else
                                throw new ArgumentOutOfRangeException("y", y, "Argument out of range when accessing underlaying values. Make sure coordinates are within boundaries!");
                        case Border.Zeros:
                            return default(T); 
                        case Border.Infinity:
                            return this[x < 0 ? x + width : x < width ? x : x - width, y < 0 ? y + height : y < height ? y : y - height];
                        case Border.Mirror:
                            return this[x < 0 ? Math.Abs(x) : x < width ? x : 2 * width - x - 1, y < 0 ? Math.Abs(y) : y < height ? y : 2 * height - y - 1];
                        default:
                            throw new NotImplementedException("Specified BorderOptions are not implemented");
                    }

                }
            }
            set { values[ch][x * height + y] = value; }
        }
        public T this[int x, int y, int c]
        {
            get
            {
                if (x >= 0 && y >= 0 && x < width && y < height) return values[c][x * height + y];
                else
                {
                    switch (BorderOptions)
                    {
                        case Border.Empty:
                            if (x < 0 || x >= width)
                                throw new ArgumentOutOfRangeException("x", x, "Argument out of range when accessing underlaying values. Make sure coordinates are within boundaries!");
                            else
                                throw new ArgumentOutOfRangeException("y", y, "Argument out of range when accessing underlaying values. Make sure coordinates are within boundaries!");
                        case Border.Zeros:
                            return default(T); 
                        case Border.Infinity:
                            return this[x < 0 ? x + width : x < width ? x : x - width, y < 0 ? y + height : y < height ? y : y - height];
                        case Border.Mirror:
                            return this[x < 0 ? Math.Abs(x) : x < width ? x : 2 * width - x - 1, y < 0 ? Math.Abs(y) : y < height ? y : 2 * height - y - 1];
                        default:
                            throw new NotImplementedException("Specified BorderOptions are not implemented");
                    }

                }
            }
            set { values[c][x * height + y] = value; }
        }
        public int Width { get { return width; } }
        public int Height { get { return height; } }

        public Border BorderOptions { get; set; }

        #region Constructors
        public BaseFrameN(int width, int height, int numChannels)
        {
            this.width = width;
            this.height = height;
            this.numChannels = numChannels;

            BorderOptions = Border.Zeros;

            init();
        }
        public BaseFrameN(int width, int height, T[][] values)
        {
            this.width = width;
            this.height = height;

            BorderOptions = Border.Zeros;
            numChannels = values.Length;
            init(values);
        }
        public BaseFrameN(int width, int height, int numChannels, Border borderOptions)
            : this(width, height, numChannels)
        {
            BorderOptions = borderOptions;
        }
        public BaseFrameN(int width, int height, T[][] values, Border borderOptions)
            : this(width, height, values)
        { BorderOptions = borderOptions; }
        public BaseFrameN(BaseFrameN<T> org, int sampleStep)
        {
            numChannels = org.NumChannels;
            BorderOptions = org.BorderOptions;
            init(org, sampleStep);
        }
        public BaseFrameN(BaseFrameN<T> org, int sampleStep, Border borderOptions)
        {
            numChannels = org.NumChannels;
            BorderOptions = borderOptions;
            init(org, sampleStep);
        }
        #endregion

        public virtual T GetPixelAt(int x, int y)
        {
            return values[ch][x * height + y];
        }
        public virtual T GetPixelAt(int i)
        {
            return values[ch][i];
        }
        public virtual T GetPixelAt(int x, int y, int c)
        {
            return values[c][x * height + y];
        }
        public virtual void SetPixelAt(int x, int y, T v)
        {
            values[ch][x * height + y] = v;
        }
        public virtual void SetPixelAt(int x, int y, int c, T v)
        {
            values[c][x * height + y] = v;
        }
        public virtual void SetPixelAt(int i, T v)
        {
            values[ch][i] = v;
        }

        public virtual SubFrame<T> GetSubFrame(int x, int y, int w, int h)
        {
            return new SubFrame<T>(x, y, w, h, this);
        }
        public virtual SubFrame<T> GetSubFrame(RectI rect)
        {
            return new SubFrame<T>(rect.L, rect.T, rect.Width, rect.Height, this);
        }

        public abstract object Clone();

        public virtual void Apply(Func<T, T> f)
        {
            for (int i = 0; i < width * height; i++)
                this[i] = f(this[i]);
        }
        public virtual void Apply(Func<int, int, T> f)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                    this[i,j] = f(i, j);
            }
        }

        public virtual void Map2<T2>(Func<T, T2> f, IImageLike<T2> dst)
            where T2 : IEquatable<T2>, IComparable, IConvertible
        {
            for (int i = 0; i < width * height; i++)
                dst[i] = f(this[i]);
        }
        public virtual void Map2<T2>(Func<int, int, T2> f, IImageLike<T2> dst)
            where T2 : IEquatable<T2>, IComparable, IConvertible
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                    dst[i,j] = f(i, j);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new PixelEnumerator<T>(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        { return this.GetEnumerator(); }

        protected virtual void init()
        {
            values = new T[numChannels][];
            for (int i = 0; i < numChannels; i++)
                values[i] = new T[width * height];
        }
        protected virtual void init(T[][] v)
        {
            values = v;
        }
        protected virtual void init(BaseFrameN<T> org, int sampleStep)
        {
            width = org.Width / sampleStep;
            height = org.Height / sampleStep;

            values = new T[org.NumChannels][];
            for (int c = 0; c < numChannels; c++)
            {
                values[c] = new T[width * height];
                org.ActiveChannel = c;
                int k = 0;
                for (int i = 0; i < org.Width; i += sampleStep)
                {
                    for (int j = 0; j < org.Height; j += sampleStep)
                        values[c][k++] = org[i, j];
                }
            }
        }
    }
}
