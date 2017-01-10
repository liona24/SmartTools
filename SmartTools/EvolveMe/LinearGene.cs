using System;
using System.Collections.Generic;

using MathNet.Numerics.Distributions;

namespace SmartTools.EvolveMe
{
    public class LinearGene : GeneProvider
    {
        int[] configuration;
        NodeType[] types;
        int length;

        public int Length { get { return length; } }

        public LinearGene() { }
        public LinearGene(int maxDepth, int[] configuration, NodeType[] types, int length)
        {
            this.maxDepth = maxDepth;
            this.configuration = configuration;
            this.types = types;
            this.length = length;
        }

        public override void Generate(int maxDepth, int numPossibleVariables, int numPossibleOperations)
        {
            length = RNG.Next(1, maxDepth);
            configuration = new int[maxDepth];
            types = new NodeType[maxDepth];

            var sVar = arange(numPossibleVariables);
            var sOp = arange(numPossibleOperations);
            var possibleVariables = new int[sVar.Count];
            var possibleOperations = new int[sOp.Count];
            sVar.CopyTo(possibleVariables);
            sOp.CopyTo(possibleOperations);
            RNG.Shuffle(possibleVariables);
            RNG.Shuffle(possibleOperations);

            int kV = 0;
            int kO = 0;
            for (int i = 0; i < length; i++)
            {
                NodeType choice = NodeType.Function;
                double sample = Normal.Sample(RNG, double.Epsilon, 1);
                if (sample < 0)
                    choice = NodeType.Variable;

                switch (choice)
                {
                    case NodeType.Function:
                        configuration[i] = possibleOperations[kO++];
                        break;
                    case NodeType.Variable:
                        configuration[i] = possibleVariables[kV++];
                        break;
                }

                types[i] = choice;
            }
        }

        public override T Evaluate<T>(FunctionDictionary<T> dict, T[] inputs)
        {
            throw new NotImplementedException();
        }

        public override void Mutate(int numPossibleVariables, int numPossibleOperations)
        {
            int n = (int)Math.Ceiling((RNG.Next(1, length) + RNG.Next(1, length)) * 0.33333);
            int[] indices = new int[length];
            var sVar = arange(numPossibleVariables);
            var sOp = arange(numPossibleOperations);
            for (int i = 0; i < length; i++)
                indices[i] = i;

            for (int i = 0; i < length; i++)
            {
                if (types[i] == NodeType.Function)
                    sOp.Remove(configuration[i]);
                else if (types[i] == NodeType.Variable)
                    sVar.Remove(configuration[i]);
            }

            RNG.Shuffle(indices);

            for (int i = 0; i < n; i++)
            {
                int j = indices[i];
                if (types[j] == NodeType.Function)
                    sOp.Add(configuration[j]);
                else if (types[j] == NodeType.Variable)
                    sVar.Add(configuration[j]);
            }

            int kO = 0;
            int kV = 0;
            var possibleVariables = new int[sVar.Count];
            var possibleOperations = new int[sOp.Count];
            sVar.CopyTo(possibleVariables);
            sOp.CopyTo(possibleOperations);
            RNG.Shuffle(possibleVariables);
            RNG.Shuffle(possibleOperations);
            for (int i = 0; i < n; i++)
            {
                int j = indices[i];
                double sample = Normal.Sample(RNG, double.Epsilon, 1);
                if (sample > 0)
                {
                    types[j] = NodeType.Variable;
                    configuration[j] = possibleVariables[kV++];
                }
                else
                {
                    types[j] = NodeType.Function;
                    configuration[j] = possibleOperations[kO++];
                }
            }

        }

        public override GeneProvider[] Reproduce(GeneProvider other)
        {
            var p = other as LinearGene;
            if (p == null)
                return null;
            return Reproduce(this, p);
        }

        public static LinearGene[] Reproduce(LinearGene p1, LinearGene p2)
        {
            throw new NotImplementedException();
        }

        private SortedSet<int> arange(int length)
        {
            var s = new SortedSet<int>();
            for (int i = 0; i < length; i++)
                s.Add(i);
            return s;
        }
    }
}
