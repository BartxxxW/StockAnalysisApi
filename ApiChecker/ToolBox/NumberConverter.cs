using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiChecker.ToolBox
{
    public static class NumberConverter
    {

        public static double d(this int num)
        {
            return Convert.ToDouble(num);
        }
        public static double d(this double num)
        {
            return Convert.ToDouble(num);
        }
        public static double d(this decimal num)
        {
            return Convert.ToDouble(num);
        }
        public static double d(this float num)
        {
            return Convert.ToDouble(num);
        }
        public static double d(this string num)
        {
            return Convert.ToDouble(num);
        }
    }
}
