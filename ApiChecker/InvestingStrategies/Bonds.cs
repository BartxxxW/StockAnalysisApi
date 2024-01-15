using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiChecker.InvestingStrategies
{
    public interface IStrategy
    {

    }
    public class Bonds: IStrategy
    {

        public double Simulate(double percent, int intervals, double startMoney, double intervalMoney, double years, bool taxIncluded=false)
        {
            double result = 0;

            //simulation to implement
            return result;
        }
    }
}
