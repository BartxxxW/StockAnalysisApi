using ApiChecker.ToolBox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ApiChecker.InvestingStrategies
{
    public class ClosedStockToken
    {
        public ClosedStockToken()
        {

        }
        public ClosedStockToken(double price, DateTime date,double closePrice,DateTime closeDate)
        {
            Price = price;
            Date = date;
            ClosedDate = closeDate;
            ClosedPrice = closePrice;
        }

        public double Price { get; set; }
        public double ClosedPrice { get; set; }
        public DateTime Date { get; set; }
        public DateTime ClosedDate { get; set; }
    }
    public class StockToken
    {
        public StockToken()
        {
                
        }
        public StockToken(double price, DateTime date)
        {
            Price = price;
            Date = date;
        }

        public double Price { get; set; }
        public DateTime Date { get; set; }
    }
    public class WarrenBuffet : IStrategy
    {
        //to develop : Stock Prices as TOkens LIst or Dictionary
        public double Simulate(string startDate, string endDate,double startMoneyUSD, double intervalMoneyUSD, int intervalMonths, List<KeyValuePair<DateTime, double>> stockPricesUSD, bool taxIncluded = false)
        {
            double result = 0;


            List<KeyValuePair<double,StockToken>> boughtTokens=new List<KeyValuePair<double, StockToken>>();

            //stockPrices order by date
            //filter by date
            double rateUSD_PLN = 1;
            var filteredStockPrices = stockPricesUSD.OrderBy(k => k.Key).Where(k => k.Key.Date >= DateTime.Parse(startDate).Date && k.Key.Date <= DateTime.Parse(endDate).Date).ToList();

            double numberOfTokens = startMoneyUSD / (filteredStockPrices[0].Value*rateUSD_PLN);

            boughtTokens.Add(new KeyValuePair<double, StockToken>(numberOfTokens, new StockToken(filteredStockPrices[0].Value, filteredStockPrices[0].Key)));

            var checkData = filteredStockPrices[0].Key.AddMonths(intervalMonths);

            while(checkData<=DateTime.Parse(endDate))
            {
                var rangeData = checkData.AddDays(5);
                var stockPrice= filteredStockPrices.Where(s=>(s.Key>= checkData && s.Key< rangeData)).FirstOrDefault();

                numberOfTokens = intervalMoneyUSD / (stockPrice.Value * rateUSD_PLN);

                if (Double.IsInfinity(numberOfTokens) ||Double.IsNaN(numberOfTokens))
                {
                    checkData=checkData.AddDays(1);
                    continue;
                }

                boughtTokens.Add(new KeyValuePair<double, StockToken>(numberOfTokens, new StockToken(stockPrice.Value, stockPrice.Key)));


                checkData = checkData.AddMonths(intervalMonths);
            }




            //final date sell all stocks

            double numberOfBoughtTokens = boughtTokens.Select(t => t.Key).Sum();
            int howManyTimesBought = boughtTokens.Count();

            var dt_EndDate= filteredStockPrices.Last().Key.Date;
            var endRange= dt_EndDate.AddDays(5);
            double priceAtSellDate = filteredStockPrices.Where(s => s.Key >= dt_EndDate && s.Key <=endRange).FirstOrDefault().Value;


            result = numberOfBoughtTokens * priceAtSellDate;

            double paidInMoney = startMoneyUSD + intervalMoneyUSD * (howManyTimesBought - 1);

            Console.WriteLine($"paid in money {paidInMoney}");
            Console.WriteLine($"end result: {result}");
            // add months to date from [0]
            //while  next date is <= addeMontsDate - buy new stock

            //01 => buy token and 0 date with start money for price
            //02 every intervalOf MOnths buy token with   appripriate price ( add.Months)
            //add to tokens list

            // ont the end period in end date sell all token with thaht price ( number of token times actual price)


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
