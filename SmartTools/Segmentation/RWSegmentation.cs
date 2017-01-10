using System;
using System.Collections.Generic;

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double.Solvers;
using MathNet.Numerics.LinearAlgebra.Solvers;

using MiniGL;

namespace SmartTools
{
    //TODO add iSegmenting interface

    //TODO extend for prior model selection
    
    /// <summary>
    /// Used to segment images using preassigned seed points
    /// </summary>
    public class RandomWalkerSegmenter<T>
        where T : IComparable, IEquatable<T>, IConvertible
    {
        Vec2I[] seeds;
        int[] labels;
        int numUnique;

        Func<IImageLike<T>, Vec2I, Vec2I, double> weightingFunc = expIntWeight; //has to be >= 0 for all inputs

        #region Constructors
        public RandomWalkerSegmenter(Vec2I[] seeds)
        {
            this.seeds = seeds;
            labels = new int[seeds.Length];
            for (int i = 0; i < labels.Length; i++)
                labels[i] = i + 1;
            numUnique = labels.Length;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SmartTools.RandomWalkerSegmenter"/> class.
        /// </summary>
        /// <param name="seedPoints">Seed Points.</param>
        /// <param name="labels">According labels, indexing starting at 1</param>
        public RandomWalkerSegmenter(Vec2I[] seedPoints, int[] labels)
        {
            this.seeds = seedPoints;
            this.labels = labels;
            Array.Sort(labels, seeds);
            numUnique = 1;
            for (int i = 1; i < labels.Length; i++)
            {
                if (labels[i - 1] != labels[i])
                    numUnique++;
            }

            weightingFunc = expIntWeight;
        }
        public RandomWalkerSegmenter(Vec2I[] seeds, Func<IImageLike<T>, Vec2I, Vec2I, double> weightingFunc)
            : this (seeds)
        {
            this.weightingFunc = weightingFunc;
        }
        public RandomWalkerSegmenter(Vec2I[] seedPoints, int[] labels, Func<IImageLike<T>, Vec2I, Vec2I, double> weightingFunc)
            : this (seedPoints, labels)
        {
            this.weightingFunc = weightingFunc;
        }
        #endregion


        public int[][] Segmentate(IImageLike<T> im)
        {
            var map = new int[im.Width][];
            for (int i = 0; i < im.Width; i++)
                map[i] = new int[im.Height];
            for (int i = 0; i < seeds.Length; i++)
                map[seeds[i].X][seeds[i].Y] = labels[i];
            int k = -1;
            for (int i = 0; i < im.Width; i++)
            {
                for (int j = 0; j < im.Height; j++)
                {
                    if (map[i][j] == 0)
                        map[i][j] = k--;
                }
            }
            //map is abused for some status information: if value < 0: holds index for the matrix L: *-1 - 1

            //building first sparse matrix: L, square with size [im.Width * im.Height - seeds.Length]
            var indexL = new List<Tuple<int, int, double>>();
            double sumW, w;
            for (int i = 0; i < im.Width; i++)
            {
                for (int j = 0; j < im.Height; j++)
                {
                    if (map[i][j] > 0)
                        continue;
                    int n = -map[i][j] - 1;
                    var s = new Vec2I(i, j);
                    sumW = 0;
                    if (i > 0)
                    {
                        w = weightingFunc(im, s, new Vec2I(i - 1, j));
                        sumW += w;

                        if (map[i - 1][j] < 0)
                            indexL.Add(Tuple.Create(n, -map[i - 1][j] - 1, -w));
                    }
                    if (i < im.Width - 1)
                    {
                        w = weightingFunc(im, s, new Vec2I(i + 1, j));
                        sumW += w;

                        if (map[i + 1][j] < 0)
                            indexL.Add(Tuple.Create(n, -map[i + 1][j] - 1, -w));
                    }
                    if (j > 0)
                    {
                        w = weightingFunc(im, s, new Vec2I(i, j - 1));
                        sumW += w;

                        if (map[i][j - 1] < 0)
                            indexL.Add(Tuple.Create(n, -map[i][j - 1] - 1, -w));
                    }
                    if (j < im.Height - 1)
                    {
                        w = weightingFunc(im, s, new Vec2I(i, j + 1));
                        sumW += w;

                        if (map[i][j + 1] < 0)
                            indexL.Add(Tuple.Create(n, -map[i][j + 1] - 1, -w));
                    }
                    indexL.Add(Tuple.Create(n, n, sumW));
                }
            }

            //building the second sparse matrix: B negativ (-B), [seeds.Length] rows and [im.Width * im.Height - rows] collumns
            var B_neg = new List<double>[seeds.Length];
            var B_ind = new List<int>[seeds.Length];
            var indexBNeg_T = new List<Tuple<int, int, double>>();
            for (int i = 0; i < seeds.Length; i++)
            {
                Vec2I s = seeds[i];

                if (s.X > 0 && map[s.X - 1][s.Y] < 0)
                {
                    w = weightingFunc(im, s, new Vec2I(s.X - 1, s.Y));
                    indexBNeg_T.Add(Tuple.Create(-map[s.X - 1][s.Y] - 1, i, -w));
                }
                if (s.X < im.Width - 1 && map[s.X + 1][s.Y] < 0)
                {
                    w = weightingFunc(im, s, new Vec2I(s.X + 1, s.Y));
                    indexBNeg_T.Add(Tuple.Create(-map[s.X + 1][s.Y] - 1, i, -w));
                }
                if (s.Y > 0 && map[s.X][s.Y - 1] < 0)
                {
                    w = weightingFunc(im, s, new Vec2I(s.X, s.Y - 1));
                    indexBNeg_T.Add(Tuple.Create(-map[s.X][s.Y - 1] - 1, i, -w));
                }
                if (s.Y < im.Height - 1 && map[s.X][s.Y + 1] < 0)
                {
                    w = weightingFunc(im, s, new Vec2I(s.X, s.Y + 1));
                    indexBNeg_T.Add(Tuple.Create(-map[s.X][s.Y + 1] - 1, i, -w));
                }
            }

            var B_mat = Matrix<double>.Build.SparseOfIndexed(im.Width * im.Height - seeds.Length, seeds.Length, indexBNeg_T);
            var L_mat = Matrix<double>.Build.SparseOfIndexed(im.Width * im.Height - seeds.Length, im.Width * im.Height - seeds.Length, indexL);
            var solver = new TFQMR();
            var cond = new UnitPreconditioner<double>();
            var iter = new Iterator<double>(new IterationCountStopCriterion<double>(5000), new DivergenceStopCriterion<double>());

            indexL = null;
            indexBNeg_T = null;

            var prop = new Vector<double>[numUnique];
            prop[numUnique - 1] = Vector<double>.Build.Dense(L_mat.RowCount, 1.0);
            var assLabel = new int[numUnique];
            int l = 0;
            assLabel[0] = labels[0];
            for (int i = 1; i < labels.Length; i++)
            {
                if (assLabel[l] == labels[i])
                    continue;
                assLabel[++l] = labels[i];
            }
            for (int i = 0; i < numUnique - 1; i++)
            {
                Vector<double> x = Vector<double>.Build.Dense(L_mat.RowCount);
                iter.Reset();
                var b = B_mat * Vector<double>.Build.Dense(labels.Length, (index) => labels[index] == assLabel[i] ? -1 : 0);
                solver.Solve(L_mat, b, x, iter, cond);
                prop[i] = x;
                prop[numUnique - 1].Subtract(x, prop[numUnique - 1]);
            }

            for (int i = 0; i < im.Width; i++)
            {
                for (int j = 0; j < im.Height; j++)
                {
                    if (map[i][j] > 0)
                        continue;

                    int m = -map[i][j] - 1;
                    int lab = 0;
                    double max = 0;
                    for (int n = 0; n < numUnique; n++)
                    {
                        if (prop[n][m] > max)
                        {
                            max = prop[n][m];
                            lab = assLabel[n];
                        }
                    }
                    map[i][j] = lab;
                }
            }

            return map;
        }

        private static double expIntWeight(IImageLike<T> im, Vec2I src, Vec2I dst)
        {
           return Math.Exp(-ExpIntWeight * Math.Pow(Convert.ToDouble(im[dst.X, dst.Y]) - Convert.ToDouble(im[src.X, src.Y]), 2)) + 0.0001;
        }
        /// <summary>
        /// Parameter for the standard pixel difference weighting function
        /// </summary>
        public static double ExpIntWeight = 0.05;
    }
}
