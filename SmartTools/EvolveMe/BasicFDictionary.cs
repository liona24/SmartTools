using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTools.EvolveMe
{
    public class BasicFDictionary : FunctionDictionary<double>
    {
        public BasicFDictionary(Func<double, double, double>[] functions)
            : base(functions)
        { }

        public BasicFDictionary()
            : base(new Func<double, double, double>[] { Add, Sub, Mul, Div })
        { }

        public static double Add(double left, double right)
        {
            return left + right;
        }
        public static double Sub(double left, double right)
        {
            return left - right;
        }
        public static double Mul(double left, double right)
        {
            return left * right;
        }
        public static double Div(double left, double right)
        {
            if (right == 0.0)
                return 0;
            return left / right;
        }
    }
}
