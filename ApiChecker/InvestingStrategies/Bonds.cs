using ApiChecker.ToolBox;
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
    public class Payments
    {
        public double Amount { get; set; }
        public double Months { get; set; }

    }
    public class Bonds: IStrategy
    {

        public double Simulate(double percent, int intervalsMonths, double startMoney, double intervalMoney, int years, bool taxIncluded=false)
        {
            double result = 0;

            List<double> resultMoneyList = new List<double>();

            List<double> paymentsList = new List<double>();
            // first add start money - can be 0
            paymentsList.Add(startMoney);

            // interval payments => years*12 /intervalMonths
            int paymentsQuantity = (years * 12)/ intervalsMonths;

            for (int i = 0;i<paymentsQuantity;i++)
            {
                paymentsList.Add(intervalMoney);
            }


            // algorytm
            // dla kazdej wplaty => wylicz zysk po latach  po zsumowaniu zyskow z  proc

            for (int i = 0; i < paymentsList.Count; i++)
            {
                int allMonths = 12 * years - i * intervalsMonths;
                int power = allMonths / 12;
                int modPart = allMonths % 12;
                var iresult = paymentsList[i] * Math.Pow((1.d() + percent.d() / 100.d()).d(), power.d());
                double iModGain = 0;

                if (modPart>0)
                {
                    int lastYearMonths = 12 - modPart;
                    double yearPart = lastYearMonths.d() / 12.d();
                    iModGain = iresult.d() * (percent.d() / 100.d()).d() * yearPart;
                }

                var cResult = iresult + iModGain;
                resultMoneyList.Add(cResult);
            }
            // sum Up all moneys from resultMoneyList
            //test this method



            //simulation to implement
            //intervalMoney*proc
            // 100 * 4^2.5= 100*32proc=132
            // (100 * 4^2) + [((100 * 4^2)*4)-(100 * 4^2)]/2=116 + 164

            //m+(m*p) + (m+(m*p))*p= m + mp + m*p +mp^2=m*(1 + p +p +p^2)=m(1+2p+p^2)=m* (p+1)^2 => to jest wzor

            //1 rok => 104
            //2 rok =>108.16
            //3 rok => 112,48

            // 100 zl na 1.5 roku => 104 zl + 0,5*(zysk z drugiego okresu) =>106.08

            return result;
        }
    }
}
