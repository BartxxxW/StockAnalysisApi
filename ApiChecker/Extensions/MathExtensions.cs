using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiChecker.Extensions
{
    public static  class MathExtensions
    {
        public static bool AreAlmostEqual(double num1, double num2)
        {
            return Math.Abs(num1 - num2) <= 1;
        }
    }
}
