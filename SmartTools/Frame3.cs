using System;
using System.Drawing;
using System.Drawing.Imaging;

using GraphicsUtility;

namespace SmartTools
{

    public class Frame3 : BaseFrameN<byte>
    {
        private const int NUM_CH = 3;

        private ColorSpace colorSpace;

        public ColorSpace ColorSpace { get { return colorSpace; } }

        #region Constructors
        public Frame3(int width, int height) 
            : base(width, height, NUM_CH)
        { colorSpace = ColorSpace.RGB; }
        public Frame3(Bitmap org)
            : base(org.Width, org.Height, 3)
        {
            colorSpace = ColorSpace.RGB;
            values = rgbFromBmp(org);
        }
        public Frame3(Bitmap org, ColorSpace space)
            : this(org)
        {
            ConvertColor(space);
        }
        public Frame3(int width, int height, byte[][] values)
            : this(width, height, values, ColorSpace.RGB)
        { }
        public Frame3(int width, int height, byte[][] values, ColorSpace colorSpace)
            : base(width, height, values)
        {
            if (values.Length != NUM_CH)
                throw new ArgumentException("Values must have length 3", "values");
            this.colorSpace = colorSpace;
        }
        public Frame3(int width, int height, byte[][] values, ColorSpace colorSpace, Border borderOptions)
            : this(width, height, values, colorSpace)
        {
            if (values.Length != NUM_CH)
                throw new ArgumentException("Values must have length 3", "values");
            BorderOptions = borderOptions;
        }
        public Frame3(Frame3 org, int sampleStep)
            : base(org, sampleStep)
        { colorSpace = org.ColorSpace; }
        public Frame3(Frame3 org, int sampleStep, Border borderOptions)
            : base(org, sampleStep, borderOptions)
        { colorSpace = org.ColorSpace; }
        #endregion

        protected byte[][] rgbFromBmp(Bitmap bmp)
        {
            var channels = new byte[NUM_CH][];
            for (int i = 0; i < NUM_CH; i++)
                channels[i] = new byte[bmp.Width * bmp.Height];

            BitmapData bmpD = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);

            int bytesPerPixel = Bitmap.GetPixelFormatSize(bmp.PixelFormat) / 8;
            int byteCount = bmpD.Stride * bmp.Height;
            byte[] pixels = new byte[byteCount];
            IntPtr first = bmpD.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(first, pixels, 0, byteCount);

            int pxHeight = bmp.Height;
            int widthInBytes = bytesPerPixel * bmp.Width;

            for (int y = 0; y < pxHeight; y++)
            {
                int cLine = y * bmpD.Stride;
                for (int x = 0; x < widthInBytes; x += bytesPerPixel)
                {
                    byte blue = pixels[x + cLine];
                    byte green = pixels[x + 1 + cLine];
                    byte red = pixels[x + 2 + cLine];

                    channels[0][x * pxHeight / bytesPerPixel + y] = blue;
                    channels[1][x * pxHeight / bytesPerPixel + y] = green;
                    channels[2][x * pxHeight / bytesPerPixel + y] = red;
                }
            }
            bmp.UnlockBits(bmpD);

            return channels;
        }

        public Vec3I GetValueAt(int x, int y)
        {
            return new Vec3I(values[2][x * height + y], values[1][x * height + y], values[0][x * height + y]);
        }
        public Vec3I GetValueAt(int i)
        {
            return new Vec3I(values[2][i], values[1][i], values[0][i]);
        }

        public override object Clone()
        {
            var nValues = new byte[NUM_CH][];
            for (int i = 0; i < NUM_CH; i++)
            {
                nValues[i] = new byte[values[0].Length];
                Array.Copy(values[i], nValues[i], values[0].Length);
            }
            return new Frame3(width, height, nValues, colorSpace, BorderOptions);
        }

        public Bitmap GetBitmap()
        {
            return GetBitmap(255);
        }
        public Bitmap GetBitmap(byte alpha)
        {
            Bitmap res = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            BitmapData bmpD = res.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, res.PixelFormat);

            int bytesPerPixel = 4;
            int byteCount = 4 * width * height;
            byte[] pixels = new byte[byteCount];
            IntPtr first = bmpD.Scan0;

            int widthInBytes = bytesPerPixel * width;

            for (int y = 0; y < height; y++)
            {
                int cLine = y * bmpD.Stride;
                for (int x = 0; x < widthInBytes; x += bytesPerPixel)
                {
                    pixels[x + cLine] = (byte)(values[0][x * height / bytesPerPixel + y]);
                    pixels[x + 1 + cLine] = (byte)(values[1][x * height / bytesPerPixel + y]);
                    pixels[x + 2 + cLine] = (byte)(values[2][x * height / bytesPerPixel + y]);
                    pixels[x + 3 + cLine] = alpha;
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(pixels, 0, first, byteCount);
            res.UnlockBits(bmpD);
            return res;
        }
        public Bitmap GetBitmap(byte alpha, int singleChannel)
        {
            if (singleChannel >= NUM_CH)
                throw new ArgumentOutOfRangeException("singleChannel");

            Bitmap res = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            BitmapData bmpD = res.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, res.PixelFormat);

            int bytesPerPixel = 4;
            int byteCount = 4 * width * height;
            byte[] pixels = new byte[byteCount];
            IntPtr first = bmpD.Scan0;

            int widthInBytes = bytesPerPixel * width;

            for (int y = 0; y < height; y++)
            {
                int cLine = y * bmpD.Stride;
                for (int x = 0; x < widthInBytes; x += bytesPerPixel)
                {
                    for (int i = 0; i < NUM_CH; i++)
                    {
                        if (i != singleChannel)
                            pixels[x + i + cLine] = 0;
                    }
                    pixels[x + singleChannel + cLine] = (byte)(values[singleChannel][x * height / bytesPerPixel + y]);
                    pixels[x + 3 + cLine] = alpha;
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(pixels, 0, first, byteCount);
            res.UnlockBits(bmpD);
            return res;
        }

        public void ConvertColor(ColorSpace newSpace)
        {
            if (colorSpace == newSpace)
                return;

            Func<double, double, double, Vec3> cvtFunc = null;
            switch (newSpace)
            {
                case ColorSpace.RGB:
                    switch (colorSpace)
                    {
                        case ColorSpace.XYZ:
                            cvtFunc = ColorConverter.XYZ2RGB;
                            break;
                        default:
                            throw new NotImplementedException(string.Format("Conversion from {0} to {1} is not supported!", newSpace.ToString(), ColorSpace.ToString()));
                    }
                    break;
                case ColorSpace.XYZ:
                    switch (colorSpace)
                    {
                        case ColorSpace.RGB:
                            cvtFunc = ColorConverter.RGB2XYZ;
                            break;
                        default:
                            throw new NotImplementedException(string.Format("Conversion from {0} to {1} is not supported!", newSpace.ToString(), ColorSpace.ToString()));
                    }
                    break;
                default:
                    throw new NotImplementedException(string.Format("Conversion from {0} to {1} is not supported!", newSpace.ToString(), ColorSpace.ToString()));
            }

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    var r = cvtFunc(values[2][i * height + j], values[1][i * height + j], values[0][i * height + j]);
                    values[2][i * height + j] = (byte)r.X;
                    values[1][i * height + j] = (byte)r.Y;
                    values[0][i * height + j] = (byte)r.Z;
                }
            }
            colorSpace = newSpace;
        }
    }

    public class Frame3F32 : BaseFrameN<float>
    {
        private const int NUM_CH = 3;

        private ColorSpace colorSpace;

        public ColorSpace ColorSpace { get { return colorSpace; } }

        #region Constructors
        public Frame3F32(int width, int height) 
            : base(width, height, NUM_CH)
        { colorSpace = ColorSpace.RGB; }
        public Frame3F32(Bitmap org)
            : base(org.Width, org.Height, 3)
        {
            colorSpace = ColorSpace.RGB;
            values = rgbFromBmp(org);
        }
        public Frame3F32(Bitmap org, ColorSpace space)
            : this(org)
        {
            ConvertColor(space);
        }
        public Frame3F32(int width, int height, float[][] values)
            : this(width, height, values, ColorSpace.RGB)
        { }
        public Frame3F32(int width, int height, float[][] values, ColorSpace colorSpace)
            : base(width, height, values)
        {
            if (values.Length != NUM_CH)
                throw new ArgumentException("Values must have length 3", "values");
            this.colorSpace = colorSpace;
        }
        public Frame3F32(int width, int height, float[][] values, ColorSpace colorSpace, Border borderOptions)
            : this(width, height, values, colorSpace)
        {
            if (values.Length != NUM_CH)
                throw new ArgumentException("Values must have length 3", "values");
            BorderOptions = borderOptions;
        }
        public Frame3F32(Frame3F32 org, int sampleStep)
            : base(org, sampleStep)
        { colorSpace = org.ColorSpace; }
        public Frame3F32(Frame3F32 org, int sampleStep, Border borderOptions)
            : base(org, sampleStep, borderOptions)
        { colorSpace = org.ColorSpace; }
        #endregion

        protected float[][] rgbFromBmp(Bitmap bmp)
        {
            var channels = new float[NUM_CH][];
            for (int i = 0; i < NUM_CH; i++)
                channels[i] = new float[bmp.Width * bmp.Height];

            BitmapData bmpD = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);

            int bytesPerPixel = Bitmap.GetPixelFormatSize(bmp.PixelFormat) / 8;
            int byteCount = bmpD.Stride * bmp.Height;
            byte[] pixels = new byte[byteCount];
            IntPtr first = bmpD.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(first, pixels, 0, byteCount);

            int pxHeight = bmp.Height;
            int widthInBytes = bytesPerPixel * bmp.Width;

            for (int y = 0; y < pxHeight; y++)
            {
                int cLine = y * bmpD.Stride;
                for (int x = 0; x < widthInBytes; x += bytesPerPixel)
                {
                    byte blue = pixels[x + cLine];
                    byte green = pixels[x + 1 + cLine];
                    byte red = pixels[x + 2 + cLine];

                    channels[0][x * pxHeight / bytesPerPixel + y] = blue;
                    channels[1][x * pxHeight / bytesPerPixel + y] = green;
                    channels[2][x * pxHeight / bytesPerPixel + y] = red;
                }
            }
            bmp.UnlockBits(bmpD);

            return channels;
        }

        public Vec3 GetValueAt(int x, int y)
        {
            return new Vec3(values[2][x * height + y], values[1][x * height + y], values[0][x * height + y]);
        }
        public Vec3 GetValueAt(int i)
        {
            return new Vec3(values[2][i], values[1][i], values[0][i]);
        }

        public override object Clone()
        {
            var nValues = new float[NUM_CH][];
            for (int i = 0; i < NUM_CH; i++)
            {
                nValues[i] = new float[values[0].Length];
                Array.Copy(values[i], nValues[i], values[0].Length);
            }
            return new Frame3F32(width, height, nValues, colorSpace, BorderOptions);
        }

        public Bitmap GetBitmap()
        {
            return GetBitmap(255);
        }
        public Bitmap GetBitmap(byte alpha)
        {
            Bitmap res = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            BitmapData bmpD = res.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, res.PixelFormat);

            int bytesPerPixel = 4;
            int byteCount = 4 * width * height;
            byte[] pixels = new byte[byteCount];
            IntPtr first = bmpD.Scan0;

            int widthInBytes = bytesPerPixel * width;

            for (int y = 0; y < height; y++)
            {
                int cLine = y * bmpD.Stride;
                for (int x = 0; x < widthInBytes; x += bytesPerPixel)
                {
                    pixels[x + cLine] = (byte)(values[0][x * height / bytesPerPixel + y]);
                    pixels[x + 1 + cLine] = (byte)(values[1][x * height / bytesPerPixel + y]);
                    pixels[x + 2 + cLine] = (byte)(values[2][x * height / bytesPerPixel + y]);
                    pixels[x + 3 + cLine] = alpha;
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(pixels, 0, first, byteCount);
            res.UnlockBits(bmpD);
            return res;
        }
        public Bitmap GetBitmap(byte alpha, int singleChannel)
        {
            if (singleChannel >= NUM_CH)
                throw new ArgumentOutOfRangeException("singleChannel");

            Bitmap res = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            BitmapData bmpD = res.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, res.PixelFormat);

            int bytesPerPixel = 4;
            int byteCount = 4 * width * height;
            byte[] pixels = new byte[byteCount];
            IntPtr first = bmpD.Scan0;

            int widthInBytes = bytesPerPixel * width;

            for (int y = 0; y < height; y++)
            {
                int cLine = y * bmpD.Stride;
                for (int x = 0; x < widthInBytes; x += bytesPerPixel)
                {
                    for (int i = 0; i < NUM_CH; i++)
                    {
                        if (i != singleChannel)
                            pixels[x + i + cLine] = 0;
                    }
                    pixels[x + singleChannel + cLine] = (byte)(values[singleChannel][x * height / bytesPerPixel + y]);
                    pixels[x + 3 + cLine] = alpha;
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(pixels, 0, first, byteCount);
            res.UnlockBits(bmpD);
            return res;
        }

        public void ConvertColor(ColorSpace newSpace)
        {
            if (colorSpace == newSpace)
                return;

            Func<double, double, double, Vec3> cvtFunc = null;
            switch (newSpace)
            {
                case ColorSpace.RGB:
                    switch (colorSpace)
                    {
                        case ColorSpace.XYZ:
                            cvtFunc = ColorConverter.XYZ2RGB;
                            break;
                        default:
                            throw new NotImplementedException(string.Format("Conversion from {0} to {1} is not supported!", newSpace.ToString(), ColorSpace.ToString()));
                    }
                    break;
                case ColorSpace.XYZ:
                    switch (colorSpace)
                    {
                        case ColorSpace.RGB:
                            cvtFunc = ColorConverter.RGB2XYZ;
                            break;
                        default:
                            throw new NotImplementedException(string.Format("Conversion from {0} to {1} is not supported!", newSpace.ToString(), ColorSpace.ToString()));
                    }
                    break;
                default:
                    throw new NotImplementedException(string.Format("Conversion from {0} to {1} is not supported!", newSpace.ToString(), ColorSpace.ToString()));
            }

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    var r = cvtFunc(values[2][i * height + j], values[1][i * height + j], values[0][i * height + j]);
                    values[2][i * height + j] = (float)r.X;
                    values[1][i * height + j] = (float)r.Y;
                    values[0][i * height + j] = (float)r.Z;
                }
            }
            colorSpace = newSpace;
        }
    }
}
