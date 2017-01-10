using System;

using SmartTools.EvolveMe;
using MiniGL;

namespace SmartTools.Detection
{
    public class GObjectDetector : Detector
    {
        int numRectV, numRectH;

        TreeGene predictor;
        FunctionDictionary<double> dict;

        double thresh;

        FrameF64 im;
        FrameF64 im2;

        public static GObjectDetector FromFile(string config_file)
        {
            FunctionDictionary<double> dict = null;
            int numRectH = 0;
            int numRectV = 0;
            int minWWidth = 0;
            int minWHeight = 0;
            double thresh = 0.0;
            TreeGene predictor = null;

            string[] lines = System.IO.File.ReadAllLines(config_file);
            if (lines.Length < 7)
                throw new Exception("Configuration file of GObjectDetector is incomplete! File: " + config_file);
            foreach (string s in lines)
            {
                string[] values = s.Split('=');
                if (values.Length != 2)
                    throw new FormatException("GObjectDetector: Configuration File has wrong format! At value " + values[0] + " in file: " + config_file);
                switch (values[0].ToLower())
                {
                    case "dict":
                        if (values[1].ToLower() == "std")
                            dict = new BasicFDictionary();
                        else
                            throw new NotImplementedException("Cannot accept other dictionaries than standard. In file: " + config_file);
                        break;
                    case "numspx":
                        numRectH = int.Parse(values[1]);
                        break;
                    case "numspy":
                        numRectV = int.Parse(values[1]);
                        break;
                    case "minw":
                        minWWidth = int.Parse(values[1]);
                        break;
                    case "minh":
                        minWHeight = int.Parse(values[1]);
                        break;
                    case "clsthresh":
                        thresh = double.Parse(values[1], System.Globalization.CultureInfo.InvariantCulture);
                        break;
                    case "tree":
                        predictor = new TreeGene(values[1]);
                        break;
                    default:
                        throw new FormatException("Unknown argument in GObjectDetector configuration: " + values[0] + " in file: " + config_file);
                }
            }

            return new GObjectDetector(minWWidth, minWHeight, numRectV, numRectH, thresh, dict, predictor);
        }
       
        //TODO add 2nd constructor accepting scale coefficent parameter
        public GObjectDetector(int minWindowWidth, 
            int minWindowHeight, 
            int numVariablesV, 
            int numVariablesH, 
            double clssThresh, 
            FunctionDictionary<double> dict, 
            TreeGene predict)
            : base(minWindowWidth, minWindowHeight)
        {
            numRectH = numVariablesH;
            numRectV = numVariablesV;
            thresh = clssThresh;
            this.dict = dict;
            predictor = predict;
        }

        public override RectI[] DetectRect<T>(IImageLike<T> frame, ScanningOptions op, bool useParallel)
        {
            im = IntegralImage.ComputeIntegralImage(frame);
            im2 = IntegralImage.ComputeIntegralImage2(frame);
            return base.DetectRect(frame, op, useParallel);
        }

        protected override bool classifyWindow<T>(int x, int y, int w, int h, IImageLike<T> frame)
        {
            double[] inpVars = new double[numRectH * numRectV * 2];
            int miniW = w / numRectH;
            int miniH = h / numRectV;
            int k = 0;
            for (int i = 0; i < numRectH; i++)
            {
                for (int j = 0; j < numRectV; j++)
                {
                    inpVars[k] = IntegralImage.GetMeanOfROI(x + i * miniW, y + j * miniH, miniW, miniH, im);
                    inpVars[k++ + numRectH * numRectV] = IntegralImage.GetStdDevOfROI(x + i * miniW, y + j * miniH, miniW, miniH, im, im2);
                }
            }

            return classify(predictor.Evaluate(dict, inpVars));
        }

        private bool classify(double inp)
        {
            if (inp >= thresh)
                return true;

            return false;
        }
    }
}
