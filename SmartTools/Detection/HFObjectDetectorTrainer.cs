using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using SmartTools.HaarFeatures;
using GraphicsUtility;

namespace SmartTools.Detection
{
    //    
    // tightly adapted to viola/jones implementation
    //


    class Sample
    {
        public FrameF64 IntegralImage { get; private set; }
        public double Weight { get; set; }
        public int Label { get; private set; }

        public Sample(FrameF64 intIm, int label)
        {
            IntegralImage = intIm;
            Weight = 0;
            Label = label;
        }
    }

    struct Point3
    {
        public double X;
        public double Y;
        public int Level;

        public Point3(double x, double y, int level)
        {
            X = x;
            Y = y;
            Level = level;
        }
    }

    //TODO detection rate adaption in stage trainer is still ignored
    //TODO maybe remove the status update stuff, or wrap it 
    public class HFObjectDetectorTrainer : Trainer
    {
        string path_result;
        double desiredFPStage;
        double desiredFPTotal;

        int featureRange;
        
        public HFObjectDetectorTrainer(string path_result, double desiredFPStage, double desiredFPTotal, int imWidth, int imHeight, int featureRange)
        {
            this.path_result = path_result;
            this.desiredFPStage = desiredFPStage;
            this.desiredFPTotal = desiredFPTotal;

            minWidth = imWidth;
            minHeight = imHeight;

            this.featureRange = featureRange;

            //idk.. TODO..
            generousPixelBorder = 3;
        }

        /// <summary>
        /// Make sure the groundTruth values have same size
        /// </summary>
        public override void Train<T>(IImageLike<T>[] images, RectI[] accordingGroundTruth, int numNegativSamplesPerIm)
        {
            inProcess = true;

            var positives = new IImageLike<T>[images.Length];
            var negatives = new IImageLike<T>[images.Length * numNegativSamplesPerIm];
            for (int i = 0; i < images.Length; i++)
            {
                positives[i] = new SubFrame<T>(accordingGroundTruth[i].L, accordingGroundTruth[i].T, accordingGroundTruth[i].Width, accordingGroundTruth[i].Height, images[i]);
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

            var pos = new FrameF64[positives.Length];
            var neg = new FrameF64[negatives.Length];
            for (int i = 0; i < negatives.Length; i++)
                neg[i] = IntegralImage.ComputeIntegralImage(negatives[i]);
            for (int i = 0; i < positives.Length; i++)
                pos[i] = IntegralImage.ComputeIntegralImage(positives[i]);
            Sample[] full = createTrainingSet(pos, neg);
            Trainer.RNG.Shuffle(full);
            int lenEv = full.Length / 4;
            Sample[] ev = new Sample[lenEv];
            Array.Copy(full, ev, lenEv);
            Sample[] train = new Sample[full.Length - lenEv];
            Array.Copy(full, lenEv, train, 0, train.Length);

            trainCascade(train, ev);

            inProcess = false;
        }

        private Sample[] createTrainingSet(FrameF64[] positivesIntegral, FrameF64[] negativesIntegral)
        {
            Sample[] res = new Sample[positivesIntegral.Length + negativesIntegral.Length];
            for (int i = 0; i < positivesIntegral.Length; i++)
                res[i] = new Sample(positivesIntegral[i], 1);
            for (int i = positivesIntegral.Length; i < res.Length; i++)
                res[i] = new Sample(negativesIntegral[i - positivesIntegral.Length], 0);
            return res;
        }

        private void trainCascade(Sample[] tSet, Sample[] evalSet)
        {
            double FP = 1;
            int numP = 0;
            foreach (Sample s in tSet)
                numP += s.Label;
            int numN = tSet.Length - numP;

            int numNEval = 0;
            foreach (Sample s in evalSet)
                numNEval += s.Label;
            numNEval = tSet.Length - numNEval;

            List<List<WeakClassifier>> cascade = new List<List<WeakClassifier>>();
            RawFeatureInfo[] features = HaarFeatureProvider.CreateFullSet(minWidth, minHeight, featureRange);
            while (FP > desiredFPTotal)
            {
                int n = cascade.Count;
                List<WeakClassifier> result = new List<WeakClassifier>();
                double nFP = 1;
                while (nFP > desiredFPStage)
                {
                    int n0 = n;
                    if (result.Count == 0)
                        n0 = 0;

                    n += 1;

                    result.AddRange(adaBoost(numP, numN, tSet, features, n, n0));

                    nFP = 0;
                    for (int i = 0; i < evalSet.Length; i++)
                    {
                        if (evalSet[i].Label == 0)
                            nFP += evaluate(evalSet[i].IntegralImage, result);
                    }
                    nFP /= numNEval;
                }

                cascade.Add(result);
                FP *= nFP;
                Console.WriteLine("Stage completed.");
                Console.WriteLine("False Positive Total: " + FP.ToString());
                int nCount = 0;
                int top = tSet.Length;
                for (int i = 0; i < top; i++)
                {
                    if (tSet[i].Label == 1)
                        continue;
                    if (evaluate(tSet[i].IntegralImage, result) == 0)
                        tSet[i--] = tSet[--top];
                    else
                        nCount++;
                }
                Sample[] nextGen = new Sample[nCount + numP];
                Array.Copy(tSet, nextGen, nextGen.Length);
                tSet = nextGen;
            }

            //Pattern: numStages,imWidth,imHeight
            //         numWeakClassifiersStage0#Threshold0#Parity0#Alpha0#Threshold1#....,x0,y0,w0,h0,b0,x1...
            string[] file = new string[cascade.Count + 1];
            file[0] = cascade.Count + "," + minWidth + "," + minHeight;
            for (int i = 0; i < cascade.Count; i++)
            {
                List<WeakClassifier> stage = cascade[i];
                file[i+1] = stage.Count.ToString();
                foreach (WeakClassifier w in stage)
                    file[i+1] += "#" + w.Threshold.ToString(System.Globalization.CultureInfo.InvariantCulture) + "#" + w.Parity.ToString() + "#" + w.Alpha.ToString(System.Globalization.CultureInfo.InvariantCulture);
                foreach (WeakClassifier w in stage)
                    file[i+1] += "," + w.Feature.X + "," + w.Feature.Y + "," + w.Feature.Width + "," + w.Feature.Height + "," + w.Feature.Basic;
            }
            System.IO.File.WriteAllLines(path_result, file);
        }

        private List<WeakClassifier> adaBoost(int pCount, int nCount, Sample[] tSet, RawFeatureInfo[] featureRange, int numSelects, int init = 0)
        {
            double i_weight_p = 1.0 / pCount * 0.5;
            double i_weight_n = 1.0 / nCount * 0.5;
            int t0 = 0;
            if (init == 0)
            {
                for (int i = 0; i < tSet.Length; i++)
                {
                    if (tSet[i].Label == 1)
                        tSet[i].Weight = i_weight_p;
                    else
                        tSet[i].Weight = i_weight_n;
                }
            }
            else
                t0 = init;

            List<WeakClassifier> selected = new List<WeakClassifier>();

            for (int t = t0; t < numSelects; t++)
            {
                double sum = 0;
                for (int i = 0; i < tSet.Length; i++)
                    sum += tSet[i].Weight;
                double sumTP = 0;
                double sumTN = 0;
                for (int i = 0; i < tSet.Length; i++)
                {
                    double w = tSet[i].Weight / sum;
                    if (tSet[i].Label == 1)
                        sumTP += w;
                    else
                        sumTN += w;
                    tSet[i].Weight = w;
                }

                double e = double.MaxValue;
                double fVal = 0.0;
                int p = 0;
                RawFeatureInfo f = new RawFeatureInfo();
                for (int i = 0; i < featureRange.Length; i++)
                {
                    Point3[] weak = new Point3[tSet.Length];
                    Parallel.For(0, tSet.Length, (j) =>
                    {
                        weak[j] = new Point3(HaarFeatureProvider.GetCustomAt(featureRange[i]).GetValue(tSet[j].IntegralImage), tSet[j].Weight, tSet[j].Label);
                    });

                    Array.Sort<Point3>(weak, delegate(Point3 p1, Point3 p2) { return p1.X.CompareTo(p2.X); });

                    double sp = 0;
                    double sn = 0;

                    for (int j = 0; j < tSet.Length; j++)
                    {
                        double up = sp + sumTN - sn;
                        double down = sn + sumTP - sp;
                        if (up < e)
                        {
                            e = up;
                            fVal = weak[j].X;
                            p = 1;
                            f = featureRange[i];
                        }
                        else if (down < e)
                        {
                            e = down;
                            fVal = weak[j].X;
                            p = -1;
                            f = featureRange[i];
                        }

                        if (weak[j].Level == 1)
                            sp += weak[j].Y;
                        else
                            sn += weak[j].Y;
                    }
                }

                Console.WriteLine("New Classifier selected:\nT=" + fVal.ToString() + "\nP=" + p.ToString() + "\n" + f.ToString());
                double beta = e / (1 - e);
                double alpha = 100.0; //random high
                if (beta > double.Epsilon)
                    alpha = Math.Log(1 / beta);

                Console.WriteLine("A=" + alpha.ToString() + "\n");
                WeakClassifier cl = new WeakClassifier(fVal, p, alpha, f);

                Parallel.For(0, tSet.Length, (j) =>
                {
                    double v = HaarFeatureProvider.GetCustomAt(f).GetValue(tSet[j].IntegralImage);
                    tSet[j].Weight = tSet[j].Weight * Math.Pow(beta, 1 - Math.Abs(tSet[j].Label - cl.Evaluate(v)));
                });
                selected.Add(cl);
            }
            return selected;
        }

        //TODO this method is kind of redundant, update corresponding one in WeakClassifier and link
        private static int evaluate(FrameF64 intIm, List<WeakClassifier> cset)
        {
            double left = 0;
            double right = 0;
            for (int i = 0; i < cset.Count; i++)
            {
                left += cset[i].Alpha * cset[i].Evaluate(HaarFeatureProvider.GetCustomAt(cset[i].Feature).GetValue(intIm));
                right += cset[i].Alpha;
            }

            if (left >= right * 0.5)
                return 1;
            return 0;
        }
    }

    static class RandomExtensions
    {
        public static void Shuffle<T>(this Random rng, T[] array)
        {
            int n = array.Length;
            while (n > 1)
            {
                int k = rng.Next(n--);
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }
    }
}
