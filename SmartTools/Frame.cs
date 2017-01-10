using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace SmartTools
{
    public class Frame : BaseFrame<byte>
    {
        #region Constructors
        public Frame(int width, int height) : base(width, height)
        { }

        public Frame(Bitmap org) : base(org)
        { }

        public Frame(int width, int height, byte[] values)
            : base(width, height, values)
        { }

        public Frame(int width, int height, Border borderOptions)
            : base(width, height, borderOptions)
        { }
        public Frame(Bitmap org, Border borderOptions)
            : base(org, borderOptions)
        { }
        public Frame(int width, int height, byte[] values, Border borderOptions)
            : base(width, height, values, borderOptions)
        { }
        public Frame(IImageLike<byte> org, int sampleStep)
            : base(org, sampleStep)
        { }
        public Frame(IImageLike<byte> org, int sampleStep, Border borderOptions)
            : base(org, sampleStep, borderOptions)
        { }
        #endregion

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
                    byte value = (byte)(values[x * height / bytesPerPixel + y]);

                    pixels[x + cLine] = value;
                    pixels[x + 1 + cLine] = value;
                    pixels[x + 2 + cLine] = value;
                    pixels[x + 3 + cLine] = alpha;
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(pixels, 0, first, byteCount);
            res.UnlockBits(bmpD);
            return res;
        }

        public override object Clone()
        {
            byte[] nValues = new byte[values.Length];
            Array.Copy(values, nValues, values.Length);
            return new Frame(width, height, nValues, BorderOptions);
        }

        private static byte[] grayscaleFromBmp(Bitmap bmp)
        {
            BitmapData bmpD = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);

            int bytesPerPixel = Bitmap.GetPixelFormatSize(bmp.PixelFormat) / 8;
            int byteCount = bmpD.Stride * bmp.Height;
            byte[] pixels = new byte[byteCount];
            IntPtr first = bmpD.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(first, pixels, 0, byteCount);

            int pxHeight = bmp.Height;
            int widthInBytes = bytesPerPixel * bmp.Width;
            var pixelVector = new byte[bmp.Width * pxHeight];

            for (int y = 0; y < pxHeight; y++)
            {
                int cLine = y * bmpD.Stride;
                for (int x = 0; x < widthInBytes; x += bytesPerPixel)
                {
                    int blue = pixels[x + cLine];
                    int green = pixels[x + 1 + cLine];
                    int red = pixels[x + 2 + cLine];

                    pixelVector[x * pxHeight / bytesPerPixel + y] = (byte)((red * 0.3) + (green * 0.59) + (blue * 0.11));
                }
            }
            bmp.UnlockBits(bmpD);

            return pixelVector;
        }

        protected override void init(Bitmap org)
        {
            width = org.Width;
            height = org.Height;
            values = grayscaleFromBmp(org);
        }
    }
    public class FrameI32 : BaseFrame<int>
    {
        #region Constructors
        public FrameI32(int width, int height) : base(width, height)
        { }

        public FrameI32(Bitmap org) : base(org)
        { }

        public FrameI32(int width, int height, int[] values)
            : base(width, height, values)
        { }

        public FrameI32(int width, int height, Border borderOptions)
            : base(width, height, borderOptions)
        { }
        public FrameI32(Bitmap org, Border borderOptions)
            : base(org, borderOptions)
        { }
        public FrameI32(int width, int height, int[] values, Border borderOptions)
            : base(width, height, values, borderOptions)
        { }
        public FrameI32(IImageLike<int> org, int sampleStep)
            : base(org, sampleStep)
        { }
        public FrameI32(IImageLike<int> org, int sampleStep, Border borderOptions)
            : base(org, sampleStep, borderOptions)
        { }
        #endregion

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
                    byte value = (byte)(values[x * height / bytesPerPixel + y]);

                    pixels[x + cLine] = value;
                    pixels[x + 1 + cLine] = value;
                    pixels[x + 2 + cLine] = value;
                    pixels[x + 3 + cLine] = alpha;
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(pixels, 0, first, byteCount);
            res.UnlockBits(bmpD);
            return res;
        }

        public override object Clone()
        {
            var nValues = new int[values.Length];
            Array.Copy(values, nValues, values.Length);
            return new FrameI32(width, height, nValues, BorderOptions);
        }

        private static int[] grayscaleFromBmp(Bitmap bmp)
        {
            BitmapData bmpD = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);

            int bytesPerPixel = Bitmap.GetPixelFormatSize(bmp.PixelFormat) / 8;
            int byteCount = bmpD.Stride * bmp.Height;
            byte[] pixels = new byte[byteCount];
            IntPtr first = bmpD.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(first, pixels, 0, byteCount);

            int pxHeight = bmp.Height;
            int widthInBytes = bytesPerPixel * bmp.Width;
            var pixelVector = new int[bmp.Width * pxHeight];

            for (int y = 0; y < pxHeight; y++)
            {
                int cLine = y * bmpD.Stride;
                for (int x = 0; x < widthInBytes; x += bytesPerPixel)
                {
                    int blue = pixels[x + cLine];
                    int green = pixels[x + 1 + cLine];
                    int red = pixels[x + 2 + cLine];

                    pixelVector[x * pxHeight / bytesPerPixel + y] = (int)((red * 0.3) + (green * 0.59) + (blue * 0.11));
                }
            }
            bmp.UnlockBits(bmpD);

            return pixelVector;
        }

        protected override void init(Bitmap org)
        {
            width = org.Width;
            height = org.Height;
            values = grayscaleFromBmp(org);
        }
    }
    public class FrameF32 : BaseFrame<float>
    {
        #region Constructors
        public FrameF32(int width, int height) : base(width, height)
        { }

        public FrameF32(Bitmap org) : base(org)
        { }

        public FrameF32(int width, int height, float[] values)
            : base(width, height, values)
        { }

        public FrameF32(int width, int height, Border borderOptions)
            : base(width, height, borderOptions)
        { }
        public FrameF32(Bitmap org, Border borderOptions)
            : base(org, borderOptions)
        { }
        public FrameF32(int width, int height, float[] values, Border borderOptions)
            : base(width, height, values, borderOptions)
        { }
        public FrameF32(IImageLike<float> org, int sampleStep)
            : base(org, sampleStep)
        { }
        public FrameF32(IImageLike<float> org, int sampleStep, Border borderOptions)
            : base(org, sampleStep, borderOptions)
        { }
        #endregion

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
                    byte value = (byte)(values[x * height / bytesPerPixel + y]);

                    pixels[x + cLine] = value;
                    pixels[x + 1 + cLine] = value;
                    pixels[x + 2 + cLine] = value;
                    pixels[x + 3 + cLine] = alpha;
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(pixels, 0, first, byteCount);
            res.UnlockBits(bmpD);
            return res;
        }

        public override object Clone()
        {
            float[] nValues = new float[values.Length];
            Array.Copy(values, nValues, values.Length);
            return new FrameF32(width, height, nValues, BorderOptions);
        }

        private static float[] grayscaleFromBmp(Bitmap bmp)
        {
            BitmapData bmpD = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);

            int bytesPerPixel = Bitmap.GetPixelFormatSize(bmp.PixelFormat) / 8;
            int byteCount = bmpD.Stride * bmp.Height;
            byte[] pixels = new byte[byteCount];
            IntPtr first = bmpD.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(first, pixels, 0, byteCount);

            int pxHeight = bmp.Height;
            int widthInBytes = bytesPerPixel * bmp.Width;
            var pixelVector = new float[bmp.Width * pxHeight];

            for (int y = 0; y < pxHeight; y++)
            {
                int cLine = y * bmpD.Stride;
                for (int x = 0; x < widthInBytes; x += bytesPerPixel)
                {
                    int blue = pixels[x + cLine];
                    int green = pixels[x + 1 + cLine];
                    int red = pixels[x + 2 + cLine];

                    pixelVector[x * pxHeight / bytesPerPixel + y] = (float)((red * 0.3) + (green * 0.59) + (blue * 0.11));
                }
            }
            bmp.UnlockBits(bmpD);

            return pixelVector;
        }

        protected override void init(Bitmap org)
        {
            width = org.Width;
            height = org.Height;
            values = grayscaleFromBmp(org);
        }
    }
    public class FrameF64 : BaseFrame<double>
    {
        #region Constructors
        public FrameF64(int width, int height) : base(width, height)
        { }

        public FrameF64(Bitmap org) : base(org)
        { }

        public FrameF64(int width, int height, double[] values)
            : base(width, height, values)
        { }

        public FrameF64(int width, int height, Border borderOptions)
            : base(width, height, borderOptions)
        { }
        public FrameF64(Bitmap org, Border borderOptions)
            : base(org, borderOptions)
        { }
        public FrameF64(int width, int height, double[] values, Border borderOptions)
            : base(width, height, values, borderOptions)
        { }
        public FrameF64(IImageLike<double> org, int sampleStep)
            : base(org, sampleStep)
        { }
        public FrameF64(IImageLike<double> org, int sampleStep, Border borderOptions)
            : base(org, sampleStep, borderOptions)
        { }
        #endregion

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
                    byte value = (byte)(values[x * height / bytesPerPixel + y]);

                    pixels[x + cLine] = value;
                    pixels[x + 1 + cLine] = value;
                    pixels[x + 2 + cLine] = value;
                    pixels[x + 3 + cLine] = alpha;
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(pixels, 0, first, byteCount);
            res.UnlockBits(bmpD);
            return res;
        }

        public override object Clone()
        {
            var nValues = new double[values.Length];
            Array.Copy(values, nValues, values.Length);
            return new FrameF64(width, height, nValues, BorderOptions);
        }

        private static double[] grayscaleFromBmp(Bitmap bmp)
        {
            BitmapData bmpD = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);

            int bytesPerPixel = Bitmap.GetPixelFormatSize(bmp.PixelFormat) / 8;
            int byteCount = bmpD.Stride * bmp.Height;
            byte[] pixels = new byte[byteCount];
            IntPtr first = bmpD.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(first, pixels, 0, byteCount);

            int pxHeight = bmp.Height;
            int widthInBytes = bytesPerPixel * bmp.Width;
            var pixelVector = new double[bmp.Width * pxHeight];

            for (int y = 0; y < pxHeight; y++)
            {
                int cLine = y * bmpD.Stride;
                for (int x = 0; x < widthInBytes; x += bytesPerPixel)
                {
                    int blue = pixels[x + cLine];
                    int green = pixels[x + 1 + cLine];
                    int red = pixels[x + 2 + cLine];

                    pixelVector[x * pxHeight / bytesPerPixel + y] = (red * 0.3) + (green * 0.59) + (blue * 0.11);
                }
            }
            bmp.UnlockBits(bmpD);

            return pixelVector;
        }

        protected override void init(Bitmap org)
        {
            width = org.Width;
            height = org.Height;
            values = grayscaleFromBmp(org);
        }
    }
}