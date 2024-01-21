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
        //to develop : int years shoudl be also  in double as a part
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

                double partToRaise= 1.d() + percent.d() / 100.d();
                var iresult = paymentsList[i] * Math.Pow(partToRaise, power.d());
                double iModGain = 0;

                if (modPart>0)
                {
                    //int lastYearMonths = 12 - modPart;
                    double yearPart = modPart.d() / 12.d();
                    iModGain = iresult.d() * (percent.d() / 100.d()).d() * yearPart;
                }

                var cResult = iresult + iModGain;
                resultMoneyList.Add(cResult);
            }


            for ( int i = 0;i < paymentsList.Count;i++)
            {
                Console.WriteLine($"id:{i}  payment:{paymentsList[i]}  resut After Years:{resultMoneyList[i]}");
            }



            var paidMoney = paymentsList.Sum();
            var gainedMoney = resultMoneyList.Sum();

            Console.WriteLine($" total paid money:{paidMoney}  total gainedMoney:{gainedMoney}");




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
