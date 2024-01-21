using ApiChecker.ToolBox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiChecker.InvestingStrategies
{
    public class StockToken
    {
        public double Price { get; set; }
        public DateTime Date { get; set; }
    }
    public class WarrenBuffet : IStrategy
    {
        //to develop : Stock Prices as TOkens LIst or Dictionary
        public double Simulate(string startDate, string endDate,double startMoney, double intervalMoney, int intervalMonths, List<StockToken> stockPrices, bool taxIncluded = false)
        {
            double result = 0;

            List<double> resultMoneyList = new List<double>();

            List<double> paymentsList = new List<double>();

            List<StockToken> boughtTokens=new List<StockToken>();



            //1. start money
            //2. buy every period with certian amount of money
            //3. always buy
            //4. after years sell

            //prerequistse:
            // 1. start day
            // 2. end date
            // 3. buy token   for certain price
            // 4. sum up all bought money after certian period of times


            return result;
        }
    }
}
