using System;
using GraphicsUtility;

namespace SmartTools
{
    public enum ColorSpace
        {
            RGB,
            XYZ,
        }
    public static class ColorConverter
    {

        public static Vec3 RGB2XYZ(double r, double g, double b)
        {
            r = gammaCorrectionInv(r / 255);
            g = gammaCorrectionInv(g / 255);
            b = gammaCorrectionInv(b / 255);

            double x = 0.4124564 * r +
                        0.3575761 * g +
                        0.1804375 * b;
            double y = 0.2126729 * r +
                        0.7151522 * g +
                        0.0721750 * b;
            double z = 0.0193339 * r +
                        0.1191920 * g +
                        0.9503041 * b;
            return new Vec3(x * 100, y * 100, z * 100);
        }
        public static Vec3 XYZ2RGB(double x, double y, double z)
        {
            x /= 100;
            y /= 100;
            z /= 100;
            double r = 3.2404542 * x +
                        -1.5371385 * y +
                        -0.4985314 * z;
            double g = -0.9692660 * x +
                        1.8760108 * y +
                        0.0415560 * z;
            double b = 0.0556434 * x +
                        -0.2040259 * y +
                        1.0570000 * z;
            return new Vec3(gammaCorrection(r) * 255, gammaCorrection(g) * 255, gammaCorrection(b) * 255);
        }

        private static double gammaCorrection(double lin)
        {
            if (lin <= 0.0031308)
                return lin * 12.92;
            return 1.055 * Math.Pow(lin, 1 / 2.4) - 0.055;
        }
        private static double gammaCorrectionInv(double nonLin)
        {
            if (nonLin <= 0.04045)
                return nonLin / 12.92;
            return Math.Pow((nonLin + 0.055) / 1.055, 2.4);
        }
    }
}
