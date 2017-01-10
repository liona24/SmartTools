using System;
using System.Collections.Generic;

namespace SmartTools.EvolveMe
{
    /// <summary>
    /// Delegate for the fitness function of the ClassificationWorld
    /// </summary>
    /// <param name="fpr">Achieved False Positive Rate of the individual to be evaluated</param>
    /// <param name="dr">Achieved Detection Rate of the individual to be evaluated</param>
    /// <returns>Returns fitness of the individual, the lower the 'fitter'</returns>
    public delegate double FitnessFunctionCls(double fpr, double dr);

    /// <summary>
    /// Delegate for the classification function of the ClassificationWorld
    /// </summary>
    /// <param name="inp">The output of an individual</param>
    /// <returns>Returns the according binary label to the given input</returns>
    public delegate int ClassificationFunction(double inp);

    /// <summary>
    /// Represents a world in which a certain amount of individuals can evolve according to a given fitness function on given data
    /// </summary>
    public class ClassificationWorld : World<double, TreeGene>
    {
        FitnessFunctionCls fit;
        ClassificationFunction cls;

        //note that fitness functions is evaluated as the lower the better
        /// <summary>
        /// Creates new instance of ClassificationWorld
        /// </summary>
        /// <param name="fit">The fitness function for classification</param>
        /// <param name="cls">The classification function, returning the corresponding label to given input</param>
        /// <param name="mutRate">Mutation Rate of each generation</param>
        /// <param name="repRate">Reproduction Rate, refers to the percentage of individuals reproducing itself of each generation</param>
        /// <param name="maxPop">Number of individuals</param>
        /// <param name="dict">Set of functions to select off</param>
        public ClassificationWorld(FitnessFunctionCls fit, ClassificationFunction cls,
            double mutRate, double repRate, int maxPop, FunctionDictionary<double> dict)
            : base(maxPop, mutRate, repRate, dict)
        {
            this.fit = fit;
            this.cls = cls;
        }

        /// <summary>
        /// Evaluates the batch on each individual and sorts the individuals from best to least
        /// </summary>
        /// <param name="batch">The set of input data to evaluate on</param>
        /// <returns>Returns fitness of best individual</returns>
        public override double EvaluateIndividuals(InputData<double>[] batch)
        {
            double totP = 0;
            for (int i = 0; i < batch.Length; i++)
                if (batch[i].Label != 0) totP += 1.0;
            totP = 1 / totP;

            System.Threading.Tasks.Parallel.For(0, maxPopulation, (i) =>
            {
                int numP = 0;
                int numFP = 0;

                for (int j = 0; j < batch.Length; j++)
                {
                    int res = cls(population[i].Evaluate(dict, batch[j].Data));
                    if (res != 0)
                    {
                        if (batch[j].Label != 0)
                            numP++;
                        else
                            numFP++;
                    }
                }

                population[i].LastResult = fit(numFP * totP, numP * totP);
            });

            Array.Sort(population, (x, y) => x.LastResult.CompareTo(y.LastResult));

            return population[0].LastResult;
        }
    }
}
