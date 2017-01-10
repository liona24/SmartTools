using System;

namespace SmartTools.EvolveMe
{
    /// <summary>
    /// Represents a world in which an individual's fitness is evaluated directly off its output
    /// </summary>
    public class BasicWorld<T> : World<T, TreeGene>
    {
        Func<T, double> fit;

        //note that fitness functions is evaluated as the lower the better
        /// <summary>
        /// Creates new instance of StdWorld
        /// </summary>
        /// <param name="fit">The fitness function</param>
        /// <param name="mutRate">Mutation Rate of each generation</param>
        /// <param name="repRate">Reproduction Rate, refers to the percentage of individuals reproducing itself of each generation</param>
        /// <param name="maxPop">Number of individuals</param>
        /// <param name="dict">Set of functions to select off</param>
        public BasicWorld(Func<T, double> fit,
            double mutRate, double repRate, int maxPop, FunctionDictionary<T> dict)
            : base(maxPop, mutRate, repRate, dict)
        {
            this.fit = fit;
        }

        /// <summary>
        /// Evaluates the batch on each individual and sorts the individuals from best to least
        /// </summary>
        /// <param name="batch">The set of input data to evaluate on</param>
        /// <returns>Returns fitness of best individual</returns>
        public override double EvaluateIndividuals(InputData<T>[] batch)
        {
            System.Threading.Tasks.Parallel.For(0, maxPopulation, (i) =>
            {
                double res = 0;

                for (int j = 0; j < batch.Length; j++)
                {
                    res += fit(population[i].Evaluate(dict, batch[j].Data));
                }

                population[i].LastResult = res;
            });

            Array.Sort(population, (x, y) => x.LastResult.CompareTo(y.LastResult));

            return population[0].LastResult;
        }
    }
}
