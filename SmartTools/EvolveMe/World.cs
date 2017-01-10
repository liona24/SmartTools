using System;
using System.Collections.Generic;
using MathNet.Numerics.Random;

namespace SmartTools.EvolveMe
{
    /// <summary>
    /// Set of data linked with corresponding label
    /// </summary>
    public struct InputData<T>
    {
        public T[] Data;

        /// <summary>
        /// Label of the data
        /// </summary>
        public int Label; 

        public InputData(T[] data, int label)
        {
            Data = data;
            Label = label;
        }
    }

    /// <summary>
    /// Base structure for the environment of evolving individuals
    /// </summary>
    public abstract class World<T, G> where G : GeneProvider, new()
    {
        protected FunctionDictionary<T> dict;

        protected int numMutations;
        protected int numSurvivors;

        protected int maxPopulation;

        protected G[] population;

        protected int inpLength;

        public World(int maxPop, double mutRate, double repRate, FunctionDictionary<T> dict)
        {
            numMutations = (int)(mutRate * maxPop);
            numSurvivors = (int)((1 - repRate) * maxPop);
            maxPopulation = maxPop;
            this.dict = dict;
        }

        /// <summary>
        /// Creates the initial population, needs to be called before any evolving can be done.
        /// </summary>
        /// <param name="maxTreeDepth">The maximum depth of the function trees of each individual</param>
        /// <param name="numParams">The number of inputs each individual has to select off; has to be the length of each InputData.Data given for training</param>
        public virtual void InitializePopulation(int maxTreeDepth, int numParams)
        {
            inpLength = numParams;
            population = new G[maxPopulation];
            for (int i = 0; i < maxPopulation; i++)
            {
                population[i] = new G();
                population[i].Generate(maxTreeDepth, numParams, dict.Length);
            }
        }

        /// <summary>
        /// Evaluates the batch on each individual and sorts the individuals from best to least
        /// </summary>
        /// <param name="batch">The set of input data to evaluate on</param>
        /// <returns>Returns fitness of best individual</returns>
        public abstract double EvaluateIndividuals(InputData<T>[] batch);

        /// <summary>
        /// Simulates one generation, includes evaluation and selection of the next generation
        /// </summary>
        /// <param name="batch">The input data set to evaluate on</param>
        /// <returns>Returns the best fitness of the current generation</returns>
        public virtual double SimulateGeneration(InputData<T>[] batch)
        {
            EvaluateIndividuals(batch);

            return _simulateGeneration(batch);
        }

        /// <summary>
        /// Evolves the individuals according to the given scenarios until a certain fitness value or a maximum of iterations is reached. 
        /// </summary>
        /// <returns>Returns development of the best fitness values</returns>
        public virtual double[] Evolve(InputData<T>[] scenarios, int numIterations, double fitnessThresh)
        {
            List<double> curve = new List<double>();
            curve.Add(EvaluateIndividuals(scenarios));
            for (int i = 0; i < numIterations; i++)
            {
                double res = _simulateGeneration(scenarios);
                curve.Add(res);
                if (res <= fitnessThresh)
                    break;
            }

            return curve.ToArray();
        }

        public G GetBestIndividual()
        {
            return population[0];
        }

        /// <summary>
        /// Intern generation simulation for increased performance on multiple iterations
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected double _simulateGeneration(InputData<T>[] data)
        {
            selectNextGen(inpLength);

            return EvaluateIndividuals(data);
        }

        /// <summary>
        /// Selects the next generation off pre sorted individual list
        /// </summary>
        /// <param name="inpLength">The number of input parameters to choose off</param>
        protected void selectNextGen(int inpLength)
        {
            var nextGen = new G[maxPopulation];
            for (int i = 0; i < numSurvivors; i++)
                nextGen[i] = population[i];

            int count = 0;
            while (count < maxPopulation - numSurvivors)
            {
                int p1 = GeneProvider.RNG.Next(0, numSurvivors);
                int p2 = GeneProvider.RNG.Next(0, numSurvivors);
                if (p1 == p2)
                {
                    p2++;
                    if (p2 >= numSurvivors)
                        p2 = 0;
                }
                var children = nextGen[p1].Reproduce(nextGen[p2]);
                count += children.Length;
                Array.Copy(children, 0, nextGen, numSurvivors + count - children.Length, Math.Min(children.Length, maxPopulation - numSurvivors - count + children.Length));
            }

            int[] mutations = new int[numMutations];
            int[] ids = new int[maxPopulation];
            for (int i = 0; i < maxPopulation; i++)
                ids[i] = i;
            GeneProvider.RNG.Shuffle(ids);
            Array.Copy(ids, mutations, numMutations);

            foreach (int i in mutations)
                nextGen[i].Mutate(inpLength, dict.Length);

            population = nextGen;
        }

    }

    static class RandomExtensions
    {
        public static void Shuffle<T>(this SystemRandomSource rng, T[] array)
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
