using System;
using System.Collections;
using System.Collections.Generic;

using GraphicsUtility;

namespace SmartTools
{
    public class PixelEnumerator<T> : IEnumerator<T>
        where T : IComparable, IEquatable<T>, IConvertible
    {
        int _index;

        int length;
        IImageLike<T> im;

        T _current;
        public T Current { get { return _current; } }

        public PixelEnumerator(IImageLike<T> im)
        {
            this.im = im;
            length = im.Width * im.Height;
            _index = -1;
        }

        public bool MoveNext()
        {
            if (++_index >= length)
                return false;
            _current = im[_index];
            return true;
        }
        public void Reset()
        {
            _index = -1;
        }
        void IDisposable.Dispose() { }
        object IEnumerator.Current { get { return Current; } }
    }

    public class PixelPositionEnumerator<T> : IEnumerator<T>
        where T : IEquatable<T>, IComparable, IConvertible
    {
        int _index;
        Vec2I[] positions;
        IImageLike<T> im;

        T _current;
        public T Current { get { return _current; } }

        public PixelPositionEnumerator(IImageLike<T> im, Vec2I[] positions)
        {
            this.im = im;
            this.positions = positions;
            _index = -1;
        }

        public bool MoveNext()
        {
            if (++_index >= positions.Length)
                return false;
            _current = im[positions[_index].X, positions[_index].Y];
            return true;
        }
        public void Reset()
        {
            _index = -1;
        }
        void IDisposable.Dispose() { }
        object IEnumerator.Current { get { return Current; } }

    }

    public class PixelCollection<T> : IEnumerable<T>
        where T : IEquatable<T>, IComparable, IConvertible
    {
        protected Vec2I[] indices;
        protected IImageLike<T> map;

        public int Count { get { return indices.Length; } }
        public Vec2I this[int i] { get { return indices[i]; } }

        #region Constructors
        public PixelCollection(IEnumerable<Vec2I> indices, IImageLike<T> im)
        {
            var list = new List<Vec2I>();
            foreach (var i in indices)
                list.Add(i);
            this.indices = list.ToArray();
            map = im;
        }
        public PixelCollection(Vec2I[] indices, IImageLike<T> im)
        {
            this.indices = indices;
            map = im;
        }
        #endregion

        #region IEnumerable
        public IEnumerator<T> GetEnumerator()
        {
            return new PixelPositionEnumerator<T>(map, indices);
        }

        IEnumerator IEnumerable.GetEnumerator()
        { return this.GetEnumerator(); }
        #endregion

        public void AddRange(Vec2I[] range)
        {
            var tmp = new Vec2I[indices.Length + range.Length];
            Array.Copy(indices, tmp, indices.Length);
            for (int i = indices.Length; i < tmp.Length; i++)
                tmp[i] = range[i - indices.Length];
            indices = tmp;
        }
    }
}
