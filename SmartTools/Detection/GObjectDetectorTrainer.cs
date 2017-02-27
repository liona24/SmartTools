using System;

using GraphicsUtility;
using SmartTools.EvolveMe;

namespace SmartTools.Detection
{
    public class GObjectDetectorTrainer : Trainer
    {
        double weightFPR;
        double weightDR;
        double thresh;
        string path_result;
        int numVarVert;
        int numVarHor;
        int maxTreeDepth;
        double mutationRate;
        double reproductionRate;
        int population;
        int maxIterations;
        double cancelThreshFitn;

        public double ClassificatorThreshold
        {
            get { return thresh; }
            set { if (!inProcess) thresh = value; }
        }

        public double WeightFalsePositiveRate
        {
            get { return weightFPR; }
            set { if (!inProcess) weightFPR = value; }
        }

        public double WeightDetectionRate
        {
            get { return weightDR; }
            set { if (!inProcess) weightDR = value; }
        }

        //number of params is ridiculous...
        public GObjectDetectorTrainer(string path_result, int minScanWinWidth, 
            int minScanWinHeight, int numVarVert, 
            int numVarHor, int maxTreeDepth, 
            double mutationRate, double reproductionRate, 
            int population, int maxIterations, double cancelThreshFitn)
        {
            weightFPR = 10.0;
            weightDR = 1.0;
            thresh = 0.0;

            minHeight = minScanWinHeight;
            minWidth = minScanWinWidth;

            this.path_result = path_result;
            this.numVarVert = numVarVert;
            this.numVarHor = numVarHor;
            this.maxTreeDepth = maxTreeDepth;
            this.mutationRate = mutationRate;
            this.reproductionRate = reproductionRate;
            this.population = population;
            this.maxIterations = maxIterations;
            this.cancelThreshFitn = cancelThreshFitn;

            //TODO maybe add that to the parameters
            generousPixelBorder = (int)(Math.Min(minHeight, minWidth) * 0.25);
        }
        public override void Train<T>(IImageLike<T>[] images, RectI[] accordingGroundTruth, int numNegativSamplesPerIm)
        {
            inProcess = true;

            var positives = new SubFrame<T>[images.Length];
            var negatives = new SubFrame<T>[images.Length * numNegativSamplesPerIm];

            for (int i = 0; i < images.Length; i++)
            {
                positives[i] = images[i].GetSubFrame(accordingGroundTruth[i].L, accordingGroundTruth[i].T, accordingGroundTruth[i].Width, accordingGroundTruth[i].Height);
                var tmp = getNegSamples(images[i], accordingGroundTruth[i], numNegativSamplesPerIm);
                for (int j = 0; j < numNegativSamplesPerIm; j++) 
                    negatives[i * numNegativSamplesPerIm + j] = tmp[j];
            }

            Train(positives, negatives);

            inProcess = false;
        }

        public override void Train<T>(IImageLike<T>[] positives, IImageLike<T>[] negatives)
        {
            inProcess = true;

            InputData<double>[] data = new InputData<double>[positives.Length + negatives.Length];

            Array.Copy(extractFromImages(positives, 1), data, positives.Length);
            Array.Copy(extractFromImages(negatives, 0), 0, data, positives.Length, negatives.Length);

            train(data);

            inProcess = false;
        }

        private void train(InputData<double>[] data)
        {
            ClassificationWorld world = new ClassificationWorld(Fitness, Classify, mutationRate, reproductionRate, population, new BasicFDictionary());
            
            world.InitializePopulation(maxTreeDepth, numVarHor * numVarVert * 2);
            double[] ev = world.Evolve(data, maxIterations, cancelThreshFitn);

            writeToFile(path_result, numVarVert, numVarHor, minWidth, minHeight, world.GetBestIndividual());
        }

        private InputData<double>[] extractFromImages<T>(IImageLike<T>[] images, int label)
            where T : IComparable, IEquatable<T>, IConvertible
        {
            InputData<double>[] data = new InputData<double>[images.Length];
            for (int n = 0; n < data.Length; n++)
            {
                var im = IntegralImage.ComputeIntegralImage(images[n]);
                var im2 = IntegralImage.ComputeIntegralImage2(images[n]);

                int w = images[n].Width / numVarHor;
                int h = images[n].Height / numVarVert;
                double[] inp = new double[numVarHor * numVarVert * 2];
                int k = 0;
                for (int i = 0; i < numVarHor; i++)
                {
                    for (int j = 0; j < numVarVert; j++)
                    {
                        inp[k] = IntegralImage.GetMeanOfROI(i * w, j * h, w, h, im);
                        inp[k++ + numVarHor * numVarVert] = IntegralImage.GetStdDevOfROI(i * w, j * h, w, h, im, im2);
                    }
                }
                data[n] = new InputData<double>(inp, label);
            }

            return data;
        }


        private void writeToFile(string path, int numVarV, int numVarH, int minW, int minH, TreeGene tree)
        {
            string[] content = new string[] { "dict=std", 
                                              "numspx=" + numVarH.ToString(),
                                              "numspy=" + numVarV.ToString(),
                                              "minw=" + minW.ToString(),
                                              "minh=" + minH.ToString(),
                                              "clsThresh=" + thresh.ToString(System.Globalization.CultureInfo.InvariantCulture),
                                              "tree=" + path + ".tree.tree" };
            tree.ToFile(path + ".tree.tree");
            System.IO.File.WriteAllLines(path, content);
        }

        private int Classify(double inp)
        {
            if (inp >= thresh)
                return 1;

            return 0;
        }
        private double Fitness(double falsePosR, double detR)
        {
            return falsePosR * weightFPR + (1 - detR) * weightDR;
        }
    }
}
