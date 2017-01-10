using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTools
{
    public static class RawPixelInfoProvider
    {
        public static void CalcMeanSuperPixels<T>(FrameF64 src, IImageLike<double> dst, int size)
            where T : IComparable, IEquatable<T>, IConvertible
        {
            int i = 0;
            double norm = 1.0f / (size * size);
            for (int x = size; x < src.Width; x += size)
            {
                for (int y = size; y < src.Height; y += size)
                    dst[i++] = (src[x, y] - src[x - size, y] - src[x, y - size] + src[x - size, y - size]) * norm; 
            }
        }
    }
}
