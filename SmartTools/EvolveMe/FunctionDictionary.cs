using System;

namespace SmartTools.EvolveMe
{
    public class FunctionDictionary<T>
    {
        Func<T, T, T>[] functions;
        
        public Func<T, T, T> this[int i]
        {
            get { return functions[i]; }
        }
        public int Length
        {
            get { return functions.Length; }
        }

        public FunctionDictionary(Func<T, T, T>[] functions)
        {
            this.functions = functions;
        }
    }
}
