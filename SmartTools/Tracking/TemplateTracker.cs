using System;
using System.Collections.Generic;
using System.Xml;

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Distributions;

using GraphicsUtility;

namespace SmartTools.Tracking
{

    public enum TrackingParams : int
    {
        RotZ = 1 << 0,
        RotY = 1 << 1,
        RotX = 1 << 2,
        Scale = 1 << 3,
        DeltaX = 1 << 4,
        DeltaY = 1 << 5,
        Affine = DeltaX | DeltaY | RotZ | RotX | RotY | Scale,
        Translate = DeltaX | DeltaY,
        Rotate = RotZ | RotX | RotY
    }

    /// <summary>
    /// A tracker using templates, learning linear predictors using a 6 parameter model
    /// </summary>
    public class TemplateTracker
    {
        Matrix<double>[] paramCombinations;
        Vector<double> intensities;
        Matrix<double>[] linearPredictors;

        Vec4[] samples;
        Vec4[] corners;
        protected Vec4[] wSamples;
        

        //decoding: [angleX, angleY, angleZ, scale, dX, dY]
        protected double[] param;
        protected TrackingParams[] paramConfig;
        protected Transform t;

        int numLayers;
        int numIter;

        int sampleStep;

        /// <summary>
        /// Initializes a new instance of TemplateTracker
        /// </summary>
        /// <param name="numLayers">Number of layers of linear predictors</param>
        /// <param name="numIterations">Number of iterations used per layer to fit the template</param>
        /// <param name="sampleStep">Images will be sampled with this distance</param>
        /// <param name="combPerLayer">Combinations per layer, from raw to fine</param>
        public TemplateTracker(int numLayers, int numIterations, int sampleStep, params TrackingParamCombination[] combPerLayer)
        {
            this.numLayers = numLayers;
            numIter = numIterations;
            this.sampleStep = sampleStep;

            if (combPerLayer.Length != numLayers)
                throw new ArgumentException("The number of given TrackingParamCombination must be same as the number of layers!");
            paramConfig = new TrackingParams[numLayers];

            paramCombinations = new Matrix<double>[numLayers];
            for (int i = 0; i < numLayers; i++)
            {
                paramConfig[i] = combPerLayer[i].Configuration;
                paramCombinations[i] = Matrix<double>.Build.Dense(combPerLayer[i].ParameterCount, combPerLayer[i].Length);
                for (int j = 0; j < combPerLayer[i].ParameterCount; j++)
                    paramCombinations[i].SetRow(j, combPerLayer[i][j]);
            }

        }

        //targetBounds containing corner points of the desired rectangle [x1,y1, x2,y2, x3,y3, x4,y4] wheres 1 top left, rotating clockwise
        /// <summary>
        /// Learns linear predictors for the given target in the given frame
        /// </summary>
        /// <param name="poly">A convex polygon describing the bounds of the target</param>
        /// <param name="zFunc">A function describing the raw shape of the object, wheres the return value the 'virtual' depth into the screen, 0 at screen</param>
        public void LockTarget<T>(IImageLike<T> frame, Vec2[] poly, Func<double, double, double> zFunc, double sigmaNoise)
            where T : IComparable, IEquatable<T>, IConvertible
        {
            samplePoints(frame, poly, zFunc);

            wSamples = new Vec4[samples.Length];
            t = new Transform(Transform.GetIdentity());
            linearPredictors = new Matrix<double>[numLayers];

            Normal noise = new Normal(0, sigmaNoise);

            for (int i = 0; i < numLayers; i++)
            {
                int numT = paramCombinations[i].ColumnCount;
                int paramCount = countParams(paramConfig[i]);
                var B = Matrix<double>.Build.Dense(samples.Length, paramCount);
                var deltaInt = Matrix<double>.Build.Dense(samples.Length, numT);
                var currentParams = Vector<double>.Build.Dense(paramCount);
                for (int j = 0; j < numT; j++)
                {
                    paramCombinations[i].Column(j).CopyTo(currentParams);
                    updateTMaker(currentParams, i);
                    transformPoints(samples, wSamples);
                    Vector<double> dI = Vector<double>.Build.Dense(samples.Length, v => Convert.ToDouble(frame[(int)Math.Round(wSamples[v].X), (int)Math.Round(wSamples[v].Y)]));
                    normalizeVec(dI);
                    dI.Add(Vector<double>.Build.Random(samples.Length, noise));
                    deltaInt.SetColumn(j, dI.Subtract(intensities));
                }


                Matrix<double> pre1 = paramCombinations[i].TransposeAndMultiply(paramCombinations[i]).Inverse();
                B = deltaInt.TransposeAndMultiply(paramCombinations[i]).Multiply(pre1);
                Matrix<double> pre2 = B.TransposeThisAndMultiply(B).Inverse();
                linearPredictors[i] = pre2.Multiply(B.Transpose());
            }

            t.LoadIdentity();
            t.Translate(param[4], param[5], 0);
            transformPoints(samples, wSamples);
        }

        public void LockTarget<T>(IImageLike<T> frame, Vec2[] polygon)
            where T : IComparable, IEquatable<T>, IConvertible
        {
            LockTarget(frame, polygon, ZToZeroSampleFunc, 3.0);
        }
        public void LockTarget<T>(IImageLike<T> frame, Rect rect, Func<double, double, double> zFunc)
            where T : IComparable, IEquatable<T>, IConvertible
        {
            LockTarget(frame, new Vec2[] { new Vec2(rect.L, rect.T),
                                            new Vec2(rect.R, rect.T),
                                            new Vec2(rect.R, rect.B),
                                            new Vec2(rect.L, rect.B)}, zFunc, 3.0);
        }
        public void LockTarget<T>(IImageLike<T> frame, Rect rect)
            where T : IComparable, IEquatable<T>, IConvertible
        {
            LockTarget(frame, new Vec2[] { new Vec2(rect.L, rect.T),
                                            new Vec2(rect.R, rect.T),
                                            new Vec2(rect.R, rect.B),
                                            new Vec2(rect.L, rect.B)}, ZToZeroSampleFunc, 3.0);
        }
        public void LockTarget<T>(IImageLike<T> frame, Vec2[] polygon, double sigmaNoise)
            where T : IComparable, IEquatable<T>, IConvertible
        {
            LockTarget(frame, polygon, ZToZeroSampleFunc, sigmaNoise);
        }
        public void LockTarget<T>(IImageLike<T> frame, Rect rect, Func<double, double, double> zFunc, double sigmaNoise)
            where T : IComparable, IEquatable<T>, IConvertible
        {
            LockTarget(frame, new Vec2[] { new Vec2(rect.L, rect.T),
                                            new Vec2(rect.R, rect.T),
                                            new Vec2(rect.R, rect.B),
                                            new Vec2(rect.L, rect.B)}, zFunc, sigmaNoise);
        }
        public void LockTarget<T>(IImageLike<T> frame, Rect rect, double sigmaNoise)
            where T : IComparable, IEquatable<T>, IConvertible
        {
            LockTarget(frame, new Vec2[] { new Vec2(rect.L, rect.T),
                                            new Vec2(rect.R, rect.T),
                                            new Vec2(rect.R, rect.B),
                                            new Vec2(rect.L, rect.B)}, ZToZeroSampleFunc, sigmaNoise);
        }

        /// <summary>
        /// Finds a locked target in a given frame
        /// </summary>
        /// <returns>Returns the corners of the found target</returns>
        public Vec4[] FindTarget3<T>(IImageLike<T> frame)
            where T : IComparable, IEquatable<T>, IConvertible
        {
            for (int l = 0; l < numLayers; l++)
            {
                for (int i = 0; i < numIter * (l + 1); i++) 
                {
                    Vector<double> nInt = Vector<double>.Build.Dense(samples.Length, j => Convert.ToDouble(frame[(int)wSamples[j].X, (int)wSamples[j].Y]));
                    normalizeVec(nInt);
                    Vector<double> deltaInt = intensities.Subtract(nInt);
                    Vector<double> deltaC = linearPredictors[l].Multiply(deltaInt);
                    updateTMakerAndParams(deltaC, l);
                    transformPoints(samples, wSamples);
                }
            }

            var result = new Vec4[corners.Length];
            transformPoints(corners, result);
            return result;
        }

        public Vec2[] FindTarget2<T>(IImageLike<T> frame, bool keepLearning)
            where T : IComparable, IEquatable<T>, IConvertible
        {
            var v4 = FindTarget3(frame);
            var result = new Vec2[v4.Length];
            for (int i = 0; i < v4.Length; i++)
                result[i] = new Vec2(v4[i].X / v4[i].W, v4[i].Y / v4[i].W);
            return result;
        }

        public Transform GetCurrentTrafo()
        {
            return (Transform)t.Clone();
        }

        protected virtual void samplePoints<T>(IImageLike<T> frame, Vec2[] poly, Func<double, double, double> zFunc)
            where T : IComparable, IEquatable<T>, IConvertible
        {
            var bounds = (RectI)GUtility.GetBounds(poly);
            var indicatorMap = new byte[bounds.Width * bounds.Height];

            Action<Vec2, Vec2> indicateScanLine = (from, to) =>
            {
                int x1 = (int)from.X;
                int x2 = (int)to.X;
                int y1 = (int)from.Y;
                int y2 = (int)to.Y;

                int dx = Math.Abs(x2 - x1);
                if (dx == 0)
                    return;
                int dy = -Math.Abs(y2 - y1);

                int sx = x2 > x1 ? 1 : -1;
                int sy = y2 > y1 ? 1 : -1;
                int e2;
                int err = dx + dy;
                do
                {
                    if (y1 == y2 && x1 == x2)
                        break;

                    e2 = err + err;
                    if (e2 > dy)
                    {
                        err += dy;
                        x1 += sx;

                        indicatorMap[x1 * bounds.Height + y1] = 1;
                    }
                    if (e2 < dx)
                    {
                        err += dx;
                        y1 += sy;
                    }
                } while (true);
            };

            for (int i = 0; i < poly.Length - 1; i++)
                indicateScanLine(poly[i], poly[i + 1]);
            indicateScanLine(poly[poly.Length - 1], poly[0]);

            int k = 0;
            var points = new List<Vec2I>();
            for (int i = 0; i < bounds.Width; i++)
            {
                bool inside = false;
                for (int j = 0; j < bounds.Height; j++)
                {
                    if (indicatorMap[k] != 0)
                    {
                        inside = !inside;
                        points.Add(new Vec2I(i, j));
                    }
                    else if (inside)
                        points.Add(new Vec2I(i, j));
                    k++;
                }
            }

            var listSamples = new List<Vec4>(bounds.Area / sampleStep + poly.Length);

            double w2 = bounds.Width * 0.5;
            double h2 = bounds.Height * 0.5;
            foreach (Vec2I p in points)
            {
                if (p.X % sampleStep == 0 && p.Y % sampleStep == 0)
                    listSamples.Add(new Vec4(p.X - w2, p.Y - h2, zFunc(p.X - w2, p.Y - h2), 1));
            }

            double offX = bounds.L + w2;
            double offY = bounds.T + h2;
            corners = new Vec4[poly.Length];
            for (int i = 0; i < poly.Length; i++)
            {
                corners[i] = new Vec4(poly[i].X - offX, poly[i].Y - offY, zFunc(poly[i].X - offX, poly[i].Y - offY), 1);
                listSamples.Add(new Vec4(poly[i].X - w2, poly[i].Y - h2, zFunc(poly[i].X - w2, poly[i].Y - h2), 1));
            }
            samples = listSamples.ToArray();
            intensities = Vector<double>.Build.Dense(samples.Length, i => Convert.ToDouble(frame[(int)(samples[i].X + offX), (int)(samples[i].Y + offY)]));
            normalizeVec(intensities);

            //                  RotZ, RotY, RotX, Scale, DeltaX, DeltaY    
            param = new double[] { 0, 0, 0, 1.0, offX, offY };
        }

        protected virtual void updateTMakerAndParams(Vector<double> deltaParam, int layer)
        {
            t.LoadIdentity();
            int k = 0;
            if (paramConfig[layer].HasFlag(TrackingParams.DeltaY))
            {
                double n = param[5] - deltaParam[k++];
                t.Translate(0, n, 0);
                param[5] = n;
            }
            else
                t.Translate(0, param[5], 0);
            if (paramConfig[layer].HasFlag(TrackingParams.DeltaX))
            {
                double n = param[4] - deltaParam[k++];
                t.Translate(n, 0, 0);
                param[4] = n;
            }
            else
                t.Translate(param[4], 0, 0);
            if (paramConfig[layer].HasFlag(TrackingParams.Scale))
            {
                double n = param[3] - deltaParam[k++];
                t.Scale(n);
                param[3] = n;
            }
            if (paramConfig[layer].HasFlag(TrackingParams.RotX))
            {
                double n = param[2] - deltaParam[k++];
                t.RotateX(n);
                param[2] = n;
            }
            if (paramConfig[layer].HasFlag(TrackingParams.RotY))
            {
                double n = param[1] - deltaParam[k++];
                t.RotateY(n);
                param[1] = n;
            }
            if (paramConfig[layer].HasFlag(TrackingParams.RotZ))
            {
                double n = param[0] - deltaParam[k++];
                t.RotateZ(n);
                param[0] = n;
            }
        }

        protected virtual void updateTMaker(Vector<double> deltaParam, int layer)
        {
            t.LoadIdentity();

            int k = 0;
            if (paramConfig[layer].HasFlag(TrackingParams.DeltaY))
                t.Translate(0, param[5] - deltaParam[k++], 0);
            else
                t.Translate(0, param[5], 0);
            if (paramConfig[layer].HasFlag(TrackingParams.DeltaX))
                t.Translate(param[4] - deltaParam[k++], 0, 0);
            else
                t.Translate(param[4], 0, 0);
            if (paramConfig[layer].HasFlag(TrackingParams.Scale))
                t.Scale(param[3] - deltaParam[k++]);
            if (paramConfig[layer].HasFlag(TrackingParams.RotX))
                t.RotateX(param[2] - deltaParam[k++]);
            if (paramConfig[layer].HasFlag(TrackingParams.RotY))
                t.RotateY(param[1] - deltaParam[k++]);
            if (paramConfig[layer].HasFlag(TrackingParams.RotZ))
                t.RotateZ(param[0] - deltaParam[k++]);
        }

        private void transformPoints(Vec4[] src, Vec4[] dst)
        {
            for (int i = 0; i < src.Length; i++)
                dst[i] = t * src[i];
        }

        private static void normalizeVec(Vector<double> v)
        {
            int l = v.Count;
            double mean = v.Sum() / l;
            v.MapInplace(j => j - mean);
            double sum = v.PointwiseMultiply(v).Sum();
            double stdR = 1.0;
            if (sum > double.Epsilon)
                stdR = 1 / Math.Sqrt(sum / (l - 1));
            v.MapInplace(j => stdR * j);
        }

        private static int countParams(TrackingParams p)
        {
            int count = 0;
            if (p.HasFlag(TrackingParams.DeltaX))
                count++;
            if (p.HasFlag(TrackingParams.DeltaY))
                count++;
            if (p.HasFlag(TrackingParams.RotY))
                count++;
            if (p.HasFlag(TrackingParams.RotX))
                count++;
            if (p.HasFlag(TrackingParams.RotZ))
                count++;
            if (p.HasFlag(TrackingParams.Scale))
                count++;

            return count;
        }

        private static Func<double, double, double> ZToZeroSampleFunc = (x, y) =>
        {
            return 0;
        };

        
    }



    public class TrackingParamCombination
    {
        int count;
        TrackingParams config;

        public TrackingParams Configuration { get { return config; } }
        public int ParameterCount { get { return count; } }

        public int Length { get { return values[0].Length; } }
        public double[] this[int i] { get { return values[i]; } set { values[i] = value; } }

        double[][] values;

        public TrackingParamCombination()
        { }
        public TrackingParamCombination(TrackingParams config, double[] rotZ, double[] rotY, double[] rotX, double[] scale, double[] dX, double[] dY)
        {
            this.config = config;
            count = countParams(config);

            values = new double[count][];
            int i = 0;

            if (config.HasFlag(TrackingParams.DeltaY)) values[i++] = dY;
            if (config.HasFlag(TrackingParams.DeltaX)) values[i++] = dX;
            if (config.HasFlag(TrackingParams.Scale))  values[i++] = scale;
            if (config.HasFlag(TrackingParams.RotX))   values[i++] = rotX;
            if (config.HasFlag(TrackingParams.RotY))   values[i++] = rotY;
            if (config.HasFlag(TrackingParams.RotZ))   values[i++] = rotZ;
        }
        public TrackingParamCombination(TrackingParams config, double[][] values)
        {
            this.config = config;
            count = countParams(config);
            this.values = values;
        }

        public void WriteToFile(string file)
        {
            var writer = XmlWriter.Create(file);
            writer.WriteStartDocument();
            writer.WriteStartElement("TrackingParamCombination");

            writer.WriteStartElement("Configuration");
            writer.WriteValue((int)config);
            writer.WriteEndElement();

            writer.WriteStartElement("Values");
            writer.WriteStartElement("Count");
            writer.WriteValue(Length);
            writer.WriteEndElement();
            for (int i = 0; i < Length; i++)
            {
                writer.WriteStartElement("Line" + i.ToString());

                int k = 0;
                if (config.HasFlag(TrackingParams.RotZ))
                {
                    writer.WriteStartElement("RotZ");
                    writer.WriteValue(values[k++][i]);
                    writer.WriteEndElement();
                }
                if (config.HasFlag(TrackingParams.RotY))
                {
                    writer.WriteStartElement("RotY");
                    writer.WriteValue(values[k++][i]);
                    writer.WriteEndElement();
                }
                if (config.HasFlag(TrackingParams.RotX))
                {
                    writer.WriteStartElement("RotX");
                    writer.WriteValue(values[k++][i]);
                    writer.WriteEndElement();
                }
                if (config.HasFlag(TrackingParams.Scale))
                {
                    writer.WriteStartElement("Scale");
                    writer.WriteValue(values[k++][i]);
                    writer.WriteEndElement();
                }
                if (config.HasFlag(TrackingParams.DeltaX))
                {
                    writer.WriteStartElement("DX");
                    writer.WriteValue(values[k++][i]);
                    writer.WriteEndElement();
                }
                if (config.HasFlag(TrackingParams.DeltaY))
                {
                    writer.WriteStartElement("DY");
                    writer.WriteValue(values[k++][i]);
                    writer.WriteEndElement();
                }

                writer.WriteEndElement(); //line
            }
            writer.WriteEndElement(); //values
            writer.WriteEndElement(); // trackingParams
            writer.Flush();
            writer.Close();
            writer.Dispose();
        }

        public static TrackingParamCombination FromFile(string file)
        {
            using (XmlReader read = XmlReader.Create(file))
            {
                read.ReadStartElement("TrackingParamCombination");
                read.ReadStartElement("Configuration");
                var config = (TrackingParams)read.ReadContentAsInt();
                int numParams = countParams(config);
                read.ReadEndElement();
                read.ReadStartElement("Values");
                read.ReadStartElement("Count");
                int numLines = read.ReadContentAsInt();
                read.ReadEndElement();
                var values = new double[countParams(config)][];
                for (int i = 0; i < values.Length; i++)
                    values[i] = new double[numLines];
                for (int i =0; i < numLines; i++)
                {
                    read.ReadStartElement("Line" + i.ToString());
                    int k = 0;
                    while (k < numParams)
                    switch (read.Name)
                    {
                        case "RotZ":
                            if (config.HasFlag(TrackingParams.RotZ))
                            {
                                read.ReadStartElement("RotZ");
                                values[k++][i] = read.ReadContentAsDouble();
                                read.ReadEndElement();
                            }
                            else goto default;
                            break;
                        case "RotY":
                            if (config.HasFlag(TrackingParams.RotY))
                            {
                                read.ReadStartElement("RotY");
                                values[k++][i] = read.ReadContentAsDouble();
                                read.ReadEndElement();
                            }
                            else goto default;
                            break;
                        case "RotX":
                            if (config.HasFlag(TrackingParams.RotX))
                            {
                                read.ReadStartElement("RotX");
                                values[k++][i] = read.ReadContentAsDouble();
                                read.ReadEndElement();
                            }
                            else goto default;
                            break;
                        case "Scale":
                            if (config.HasFlag(TrackingParams.Scale))
                            {
                                read.ReadStartElement("Scale");
                                values[k++][i] = read.ReadContentAsDouble();
                                read.ReadEndElement();
                            }
                            else goto default;
                            break;
                        case "DX":
                            if (config.HasFlag(TrackingParams.DeltaX))
                            {
                                read.ReadStartElement("DX");
                                values[k++][i] = read.ReadContentAsDouble();
                                read.ReadEndElement();
                            }
                            else goto default;
                            break;
                        case "DY":
                            if (config.HasFlag(TrackingParams.DeltaY))
                            {
                                read.ReadStartElement("DY");
                                values[k++][i] = read.ReadContentAsDouble();
                                read.ReadEndElement();
                            }
                            else goto default;
                            break;
                        default:
                            read.ReadStartElement();
                            read.ReadContentAsDouble();
                            read.ReadEndElement();
                            break;
                         
                    }
                    
                    
                    read.ReadEndElement();
                }

                return new TrackingParamCombination(config, values);
            }
        }

        /// <summary>
        /// Creates a new set of combinations used in the TemplateTracker
        /// </summary>
        /// <param name="config">The set of parameters to be used</param>
        /// <param name="ranges">Their value ranges, interpreted in order RotZ, RotY, RotX, Scale, DeltaX, DeltaY: .X - low limit, .Y - high limit, .Z - step</param>
        public static TrackingParamCombination Create(TrackingParams config, params Vec3[] ranges)
        {
            int count = countParams(config);
            if (ranges.Length != count)
                throw new ArgumentException("ranges", "Dimensions do not fit! Number of parameters specified is not equal to number of ranges given!");
            int numComb = 1;
            for (int i = 0; i < ranges.Length; i++)
                numComb *= (int)((ranges[i].Y - ranges[i].X) / ranges[i].Z);

            var comb = new double[6][];
            for (int i = 0; i < comb.Length; i++)
                comb[i] = new double[numComb];
            var indices = new int[count];
            int k = 0;
            for (int i = 0; i < 6; i++)
            {
                if (config.HasFlag((TrackingParams)(1 << i)))
                {
                    indices[k] = i;
                    comb[i][0] = ranges[k++].X;
                }
            }

            int n = 1; //index from 0 to numComb
            int j = count - 1;
            //this basicly just loops through all possible combinations of parameters
            while (n < numComb)
            {
                k = indices[j];
                double x = comb[k][n - 1] + ranges[j].Z;
                if (x > ranges[j].Y)
                {
                    j--;
                    continue;
                }

                for (int i = 0; i < count; i++)
                {
                    int l = indices[i];
                    if (l > k)
                        comb[l][n] = ranges[i].X;
                    else if (l == k)
                        comb[l][n] = x;
                    else
                        comb[l][n] = comb[l][n - 1];
                }
                n++;
                if (j < count - 1)
                    j++;
            }

            return new TrackingParamCombination(config, comb[0], comb[1], comb[2], comb[3], comb[4], comb[5]);
        }

        private static int countParams(TrackingParams p)
        {
            int count = 0;
            if (p.HasFlag(TrackingParams.DeltaX))
                count++;
            if (p.HasFlag(TrackingParams.DeltaY))
                count++;
            if (p.HasFlag(TrackingParams.RotY))
                count++;
            if (p.HasFlag(TrackingParams.RotX))
                count++;
            if (p.HasFlag(TrackingParams.RotZ))
                count++;
            if (p.HasFlag(TrackingParams.Scale))
                count++;

            return count;
        }
    }
}
