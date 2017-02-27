using System;
using System.Collections;
using System.Collections.Generic;

using GraphicsUtility;

namespace SmartTools
{
    public abstract class BaseFrame<T> : IImageLike<T>, IEnumerable<T>
            where T : IEquatable<T>, IComparable, IConvertible
    {
        protected T[] values;
        protected int width, height;

        public virtual T this[int i]
        {
            get { return values[i]; }
            set { values[i] = value; }
        }
        public virtual T this[int x, int y]
        {
            get
            {
                if (x >= 0 && y >= 0 && x < width && y < height) return values[x * height + y];
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
            set { values[x * height + y] = value; }
        }
        public int Width { get { return width; } }
        public int Height { get { return height; } }

        public Border BorderOptions { get; set; }

        #region Constructors
        public BaseFrame()
        {
            BorderOptions = Border.Zeros;
        }

        public BaseFrame(int width, int height)
        {
            this.width = width;
            this.height = height;

            BorderOptions = Border.Zeros;

            init();
        }
        public BaseFrame(int width, int height, T[] values)
        {
            this.width = width;
            this.height = height;

            BorderOptions = Border.Zeros;
            init(values);
        }
        public BaseFrame(System.Drawing.Bitmap org)
        {
            width = org.Width;
            height = org.Height;

            BorderOptions = Border.Zeros;

            init(org);
        }
        public BaseFrame(int width, int height, Border borderOptions)
            : this(width, height)
        {
            BorderOptions = borderOptions;
        }
        public BaseFrame(int width, int height, T[] values, Border borderOptions)
            : this(width, height, values)
        { BorderOptions = borderOptions; }
        public BaseFrame(System.Drawing.Bitmap org, Border borderOptions)
            : this (org)
        {
            BorderOptions = borderOptions;
        }
        public BaseFrame(IImageLike<T> org, int sampleStep)
        {
            BorderOptions = org.BorderOptions;
            init(org, sampleStep);
        }
        public BaseFrame(IImageLike<T> org, int sampleStep, Border borderOptions)
        {
            BorderOptions = borderOptions;
            init(org, sampleStep);
        }
        #endregion

        public virtual T GetPixelAt(int x, int y)
        {
            return values[x * height + y];
        }
        public virtual T GetPixelAt(int i)
        {
            return values[i];
        }
        public virtual void SetPixelAt(int x, int y, T v)
        {
            values[x * height + y] = v;
        }
        public virtual void SetPixelAt(int i, T v)
        {
            values[i] = v;
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

        public void Fill(Vec2I start, T fill, Func<T, bool> isInside)
        {
            if (!isInside(values[start.X * height + start.Y]))
                return;

            var q = new Queue<Vec2I>();
            q.Enqueue(start);

            while (q.Count > 0)
            {
                var pos = q.Dequeue();
                for (int i = pos.X; i < width; i++)
                {
                    if (isInside(values[i * height + pos.Y]))
                    {
                        values[i * height + pos.Y] = fill;
                        if (pos.Y > 0 && isInside(values[i * height + pos.Y - 1]))
                            q.Enqueue(new Vec2I(i, pos.Y - 1));
                        if (pos.Y < height - 1 && isInside(values[i * height + pos.Y + 1]))
                            q.Enqueue(new Vec2I(i, pos.Y + 1));
                    }
                    else
                        break;
                }

                for (int i = pos.X - 1; i >= 0; i--)
                {
                    if (isInside(values[i * height + pos.Y]))
                    {
                        values[i * height + pos.Y] = fill;
                        if (pos.Y > 0 && isInside(values[i * height + pos.Y - 1]))
                            q.Enqueue(new Vec2I(i, pos.Y - 1));
                        if (pos.Y < height - 1 && isInside(values[i * height + pos.Y + 1]))
                            q.Enqueue(new Vec2I(i, pos.Y + 1));
                    }
                    else
                        break;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        { return this.GetEnumerator(); }

        protected virtual void init()
        {
            values = new T[width * height];
        }
        protected virtual void init(T[] v)
        {
            values = v;
        }
        protected virtual void init(IImageLike<T> org, int sampleStep)
        {
            width = org.Width / sampleStep;
            height = org.Height / sampleStep;

            values = new T[width * height];
            int k = 0;
            for (int i = 0; i < org.Width; i += sampleStep)
            {
                for (int j = 0; j < org.Height; j += sampleStep)
                    values[k++] = org[i, j];
            }
        }
        protected abstract void init(System.Drawing.Bitmap org);
    }
}
