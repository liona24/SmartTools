using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
//TODO simplify after update of WeakClassifier class
using SmartTools.HaarFeatures;
using GraphicsUtility;

namespace SmartTools.Detection
{
    public class HFObjectDetector : Detector
    {
        int numStages;

        WeakClassifier[][] stages;

        RawFeatureInfo[][] accordingDesiredFeatures;


        /// <summary>
        /// Creates a new instance of DetectorCascade and loads configuration from the specified file
        /// </summary>
        /// <param name="config_path">The path of the file the configuration should be read from</param>
        public static HFObjectDetector FromFile(string config_path)
        {
            try
            {
                string[] lines = System.IO.File.ReadAllLines(config_path);
                string[] values = lines[0].Split(',');

                int numStages = int.Parse(values[0]);

                WeakClassifier[][] stages = new WeakClassifier[numStages][];

                RawFeatureInfo[][] accordingDesiredFeatures = new RawFeatureInfo[numStages][];

                int targetWidth = int.Parse(values[1]);
                int targetHeight = int.Parse(values[2]);

                int i = -1;
                foreach (string x in lines)
                {
                    if (i == -1)
                    {
                        i++;
                        continue;
                    }

                    values = x.Split(',');

                    string[] info = values[0].Split('#');
                    stages[i] = new WeakClassifier[int.Parse(info[0])];
                    for (int j = 1; j + 3 <= info.Length; j += 3)
                    {
                        stages[i][(j - 1) / 3] = new WeakClassifier(double.Parse(info[j], System.Globalization.CultureInfo.InvariantCulture),
                                                                          (short)double.Parse(info[j + 1]),
                                                                          double.Parse(info[j + 2], System.Globalization.CultureInfo.InvariantCulture), new RawFeatureInfo());
                    }

                    accordingDesiredFeatures[i] = new RawFeatureInfo[(values.Length - 1) / 5];
                    for (int j = 1; j + 5 <= values.Length; j += 5)
                    {
                        int xPos = (int)(double.Parse(values[j], System.Globalization.CultureInfo.InvariantCulture));
                        int yPos = (int)(double.Parse(values[j + 1], System.Globalization.CultureInfo.InvariantCulture));
                        int sX = (int)(double.Parse(values[j + 2], System.Globalization.CultureInfo.InvariantCulture));
                        int sY = (int)(double.Parse(values[j + 3], System.Globalization.CultureInfo.InvariantCulture));
                        int basic = (int)(double.Parse(values[j + 4], System.Globalization.CultureInfo.InvariantCulture));
                        accordingDesiredFeatures[i][(j - 1) / 5] = new RawFeatureInfo(basic, xPos, yPos, sX, sY);
                    }

                    i++;
                }

                return new HFObjectDetector(targetWidth, targetHeight, numStages, stages, accordingDesiredFeatures);
            }
            catch
            {
                throw;
            }
        }
        public override RectI[] DetectRect<T>(IImageLike<T> frame, ScanningOptions op, bool useParallel)
        {
            if (frame is FrameF64)
                return base.DetectRect(frame, op, useParallel);
            else
                return base.DetectRect(IntegralImage.ComputeIntegralImage(frame), op, useParallel);
        }
        //TODO also add constructor for scale coeff or some kind of access
        public HFObjectDetector(int minWinWidth, int minWinHeight, int numStages, 
            WeakClassifier[][] classifierPerStage, RawFeatureInfo[][] accordingFeatures)
            :base (minWinWidth, minWinHeight)
        {
            this.numStages = numStages;
            this.stages = classifierPerStage;
            this.accordingDesiredFeatures = accordingFeatures;
        }

        protected override bool classifyWindow<T>(int x, int y, int w, int h, IImageLike<T> frame)
        {
            double scaleX = w / (double)minWinWidth;
            double scaleY = h / (double)minWinHeight;

            for (int i = 0; i < 1; i++) //numStages; i++)
            {
                double[] input = new double[accordingDesiredFeatures[i].Length];
                for (int j = 0; j < input.Length; j++)
                {
                    int fx = (int)Math.Floor(accordingDesiredFeatures[i][j].X * scaleX) + x;
                    int fy = (int)Math.Floor(accordingDesiredFeatures[i][j].Y * scaleY) + y;
                    int sX = (int)Math.Floor(accordingDesiredFeatures[i][j].Width * scaleX);
                    int sY = (int)Math.Floor(accordingDesiredFeatures[i][j].Height * scaleY);
                    int basic = accordingDesiredFeatures[i][j].Basic;
                    input[j] = HaarFeatureProvider.GetCustomAt(basic, fx, fy, sX, sY).GetValue(frame as FrameF64);
                }

                if (!WeakClassifier.EvaluateBoosted(stages[i], input))
                    return false;
            }

            return true;
        }
    }
}

