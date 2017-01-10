using System;
using System.Collections.Generic;
using MathNet.Numerics.Random;
using MathNet.Numerics.Distributions;

namespace SmartTools.EvolveMe
{
    public class TreeGene : GeneProvider
    {
        Node root;

        public Node Root
        {
            get { return root; }
        }

        public TreeGene() { }
        public TreeGene(int rootParam, NodeType rootType, int maxDepth)
        {
            root = new Node(rootParam, rootType);
            this.maxDepth = maxDepth;
        }
        public TreeGene(Node root, int maxDepth)
        {
            this.root = root;
            this.maxDepth = maxDepth;
        }
        public TreeGene(string path)
        {
            System.IO.StreamReader read = new System.IO.StreamReader(path);
            maxDepth = int.Parse(read.ReadLine());

            root = new Node(0, NodeType.Function);
            root.ParseFromString(read.ReadToEnd());
            read.Close();
            read.Dispose();
        }

        public override T Evaluate<T>(FunctionDictionary<T> dict, T[] inputs)
        {
            return root.Evaluate(dict, inputs);
        }

        public bool ToFile(string path)
        {
            try
            {
                System.IO.StreamWriter write = new System.IO.StreamWriter(path, false);
                write.Write(maxDepth.ToString() + "\n");
                write.Write(root.ToString());
                write.Flush();
                write.Dispose();
            }
            catch { return false; }

            return true;
        }

        public override void Mutate(int numPossibleVariables, int numPossibleOperations)
        {
            int nodeDepth = RNG.Next(0, maxDepth - 1);
            int[] path = new int[nodeDepth];
            for (int i = 0; i < nodeDepth; i++)
            {

                if (Normal.Sample(RNG, double.Epsilon, 0.5) > 0)
                    path[i] = 0;
                else
                    path[i] = 1;
            }

            int aDepth = root.GetLongestExistingPath(path);
            if (aDepth < nodeDepth)
            {
                int[] nPath = new int[aDepth];
                Array.Copy(path, nPath, aDepth);
                nodeDepth = aDepth;
                path = nPath;
            }

            Node sub = new Node(RNG.Next(0, numPossibleOperations), NodeType.Function);
            sub.GenerateSubNode(nodeDepth, maxDepth, numPossibleOperations, numPossibleVariables);
            if (nodeDepth == 0)
                root = sub;
            else
                root.ReplaceSubNode(sub, path);
        }

        public override GeneProvider[] Reproduce(GeneProvider other)
        {
            var p = other as TreeGene;
            if (p != null)
                return Reproduce(this, p);
            else
                return null; // cannot reproduce with other classes
        }

        //always returns 2 children, non deterministic if parents have something in common, repeat if results not as good as wanted
        public static TreeGene[] Reproduce(TreeGene parent1, TreeGene parent2)
        {
            TreeGene[] res = new TreeGene[2];

            //note that it is assumed that roots are equal in type
            if (parent1.Root.Param != parent2.Root.Param)
            {
                //swaps the root function and keeps the rest
                Node root1 = parent1.Root.Clone(false); 
                Node root2 = parent2.Root.Clone(false);

                //the swapping here is a bit overkill, but it should be fine
                root1.ReplaceSubNode(parent2.Root.Children[0].Clone(true), new int[] { 0 });
                root1.ReplaceSubNode(parent2.Root.Children[1].Clone(true), new int[] { 1 });

                root2.ReplaceSubNode(parent1.Root.Children[0].Clone(true), new int[] { 0 });
                root2.ReplaceSubNode(parent1.Root.Children[1].Clone(true), new int[] { 1 });

                res[0] = new TreeGene(root2, parent1.MaxDepth);
                res[1] = new TreeGene(root1, parent2.MaxDepth);
            }
            else
            {
                Node root1 = parent1.Root.Clone(true);
                Node root2 = parent2.Root.Clone(true);

                List<int[]> stacks = new List<int[]>();
                root1.FindNonFits(root2, stacks);

                int[] changeIndices = new int[stacks.Count];
                for (int i = 0; i < changeIndices.Length; i++)
                    changeIndices[i] = i;
                RNG.Shuffle(changeIndices);

                //shuffle the subnodes between the parents
                for (int i = 0; i < stacks.Count; i++)
                {
                    root1.ReplaceSubNode(root2.GetSubNode(stacks[i], false), stacks[changeIndices[i]]);
                    root2.ReplaceSubNode(parent1.Root.GetSubNode(stacks[changeIndices[i]], true), stacks[i]);
                }

                res[0] = new TreeGene(root2, parent1.MaxDepth);
                res[1] = new TreeGene(root1, parent2.MaxDepth);
            }

            return res;
        }

        public override void Generate(int maxDepth, int numPossibleVariables, int numPossibleOperations)
        {
            var tree = Create(maxDepth, numPossibleVariables, numPossibleOperations);
            root = tree.Root;
            maxDepth = tree.MaxDepth;
        }

        public static TreeGene Create(int maxDepth, int numPossibleVariables, int numPossibleOperations)
        {
            if (maxDepth == 1)
                return new TreeGene(RNG.Next(0, numPossibleVariables), NodeType.Variable, 1);

            Node root = new Node(RNG.Next(0, numPossibleOperations), NodeType.Function);
            TreeGene result = new TreeGene(root, maxDepth);
            root.GenerateSubNode(0, maxDepth, numPossibleOperations, numPossibleVariables);

            return new TreeGene(root, maxDepth);
        }
    }

    public enum NodeType
    {
        Function,
        Variable
    }

    public class Node
    {
        //dont change that or the source wont work anymore
        protected const int MAX_NUM_CHILDREN = 2;

        protected int numChildren;
        protected int param;

        protected Node[] children;

        protected NodeType type;

        public NodeType Type
        {
            get { return type; }
        }
        public Node[] Children
        {
            get { return children; }
        }
        public int NumChildren
        {
            get { return numChildren; }
        }
        public int Param
        {
            get { return param; }
        }

        public Node(int param, NodeType type)
        {
            this.numChildren = 0;
            this.param = param;
            this.type = type;

            children = new Node[MAX_NUM_CHILDREN];
        }

        public bool AddChild(int param, NodeType t)
        {
            if (numChildren >= MAX_NUM_CHILDREN)
                return false;

            children[numChildren] = new Node(param, t);
            numChildren++;
            return true;
        }
        public bool AddChild(Node child)
        {
            if (numChildren >= MAX_NUM_CHILDREN)
                return false;

            children[numChildren] = child;
            numChildren++;
            return true;
        }

        /// <summary>
        /// Adds child to this node's subtree
        /// </summary>
        /// <param name="indxStack">The path to the node where the child should be added</param>
        /// <param name="start">Recursive parameter, indicates position in stack</param>
        /// <returns>True if adding was successful, false otherwise</returns>
        public bool AddChild(int param, NodeType t, int[] indxStack, int start = 0)
        {
            if (start == indxStack.Length)
                return AddChild(param, t);

            return children[indxStack[start]].AddChild(param, t, indxStack, start - 1);
        }

        public bool AddChild(Node nnode, int[] idxStack, int start = 0)
        {
            if (start == idxStack.Length)
            {
                if (numChildren >= MAX_NUM_CHILDREN)
                    return false;

                children[numChildren] = nnode;
                numChildren++;
                return true;
            }

            return children[idxStack[start]].AddChild(nnode, idxStack, start + 1);
        }

        /// <summary>
        /// Remove child from this node and return it to the caller
        /// </summary>
        /// <param name="childIndex">Index of the child to be removed</param>
        public Node Adopt(int childIndex)
        {
            numChildren--;
            return children[childIndex];
        }

        public void RemoveChild(int childIndex)
        {
            numChildren--;
            children[childIndex] = null;
        }

        /// <summary>
        /// Replaces a child node somewhere in this nodes subtree
        /// </summary>
        /// <param name="nnode">The new node which should replace the old one</param>
        /// <param name="idxStack">The path to the node to be replaced</param>
        /// <param name="start">Recursive parameter, indicates position on stack</param>
        public void ReplaceSubNode(Node nnode, int[] idxStack, int start = 0)
        {
            if (start == idxStack.Length - 1)
            {
                if (idxStack[start] >= numChildren)
                    numChildren = idxStack[start] + 1;
                children[idxStack[start]] = nnode;
            }
            else
                children[idxStack[start]].ReplaceSubNode(nnode, idxStack, start + 1);
        }
        
        public Node GetSubNode(int[] idxStack, bool doClone, int start = 0)
        {
            if (start == idxStack.Length - 1)
            {
                if (doClone)
                    return children[idxStack[start]].Clone(true);

                return children[idxStack[start]];
            }

            return children[idxStack[start]].GetSubNode(idxStack, doClone, start + 1);
        }

        public void GenerateSubNode(int depth, int maxDepth, int numOp, int numVar)
        {
            if (depth >= maxDepth - 1)
            {
                while (numChildren < MAX_NUM_CHILDREN)
                    AddChild(GeneProvider.RNG.Next(0, numVar), NodeType.Variable);

                return;
            }

            for (int i = 0; i < MAX_NUM_CHILDREN; i++)
            {

                NodeType choice = NodeType.Function;
                double sample = Normal.Sample(GeneProvider.RNG, 0, maxDepth * 0.7);
                if (Math.Abs(sample) < depth)
                    choice = NodeType.Variable;

                switch (choice)
                {
                    case NodeType.Function:
                        AddChild(GeneProvider.RNG.Next(0, numOp), NodeType.Function);
                        children[i].GenerateSubNode(depth + 1, maxDepth, numOp, numVar);
                        break;
                    case NodeType.Variable:
                        AddChild(GeneProvider.RNG.Next(0, numVar), NodeType.Variable);
                        break;
                }
            }
        }

        //given that the origin fits
        /// <summary>
        /// Creates a list of stacks (int[]) to the nodes where the other Nodetree is different from this one
        /// </summary>
        /// <param name="misses">The stack to be created</param>
        /// <param name="stack">Internal stack value to handle depth</param>
        public void FindNonFits(Node other, List<int[]> misses, List<int> stack = null)
        {
            if (stack == null)
                stack = new List<int>();

            for (int i = 0; i < numChildren; i++)
            {
                stack.Add(i);
                if (children[i].Type != other.Children[i].Type ||
                    other.Children[i].Param != children[i].Param)
                {
                    misses.Add(stack.ToArray());
                }
                else
                {
                    children[i].FindNonFits(other.Children[i], misses, stack);
                }
                stack.RemoveAt(stack.Count - 1);
            }
        }

        /// <summary>
        /// Function to check whether the given path exists in this node
        /// </summary>
        /// <param name="path">The path to check</param>
        /// <param name="start">Internal index to keep track of the recursion</param>
        /// <returns>Returns the length of the existing part of the path</returns>
        public int GetLongestExistingPath(int[] path, int start = 0)
        {
            if (type == NodeType.Variable || start == path.Length)
                return start;

            return children[path[start]].GetLongestExistingPath(path, start + 1);
        }

        public T Evaluate<T>(FunctionDictionary<T> dict, T[] variables)
        {
            switch (type)
            {
                case NodeType.Function:
                    if (numChildren < 2)
                        throw new Exception("Tree is incomplete, child in Function Node missing."); //was basicly just for debugging, should never happen
                    T r = children[0].Evaluate(dict, variables);
                    for (int i = 1; i < numChildren; i++)
                        r = dict[param](r, children[i].Evaluate(dict, variables));
                    return r;
                case NodeType.Variable:
                    return variables[param];
            }

            throw new Exception(); // not possible
        }

        public Node Clone(bool deep)
        {
            if (!deep)
                return new Node(param, type);

            Node n = new Node(param, type);
            for (int i = 0; i < numChildren; i++)
                n.AddChild(children[i].Clone(deep));

            return n;
        }

        public override string ToString()
        {
            if (type == NodeType.Variable || numChildren == 0)
                return param.ToString();

            string me = "<" + param.ToString() + ">";
            string res = "(";
            for (int i = 0; i < numChildren - 1; i++)
            {
                res += children[i].ToString();
                res += me;
            }
            res += children[numChildren - 1].ToString() + ")";

            return res;
        }

        public void ParseFromString(string node)
        {
            int open = 0;
            int i = 0;
            for (; i < node.Length; i++)
            {
                if (node[i] == '(')
                    open++;
                else if (node[i] == ')')
                    open--;
                else if (node[i] == '<' && open == 1)
                    break;

                if (open == 0)
                {
                    type = NodeType.Variable;
                    param = int.Parse(node);
                    return;
                }
            }

            string left = node.Substring(1, i - 1);
            i++;
            open = i;
            for (; i < node.Length; i++)
            {
                if (node[i] == '>')
                    break;
            }
            type = NodeType.Function;
            param = int.Parse(node.Substring(open, i - open));
            string right = node.Substring(i + 1, node.Length - i - 2);

            Node child = new Node(0, NodeType.Function);
            child.ParseFromString(left);
            AddChild(child);
            child = new Node(0, NodeType.Function);
            child.ParseFromString(right);
            AddChild(child);
        }
    }
}

