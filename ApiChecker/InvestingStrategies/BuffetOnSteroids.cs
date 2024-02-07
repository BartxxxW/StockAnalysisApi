using ApiChecker.Models;
using ApiChecker.ToolBox;
using Newtonsoft.Json.Linq;
using ScottPlot.Renderable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ApiChecker.InvestingStrategies
{

    public class BuffetOnSteroids : IStrategy
    {
        //to develop : Stock Prices as TOkens LIst or Dictionary
        public double Simulate(ProcessedStockDataModel dataModel,string startDate, string endDate,double startMoneyUSD, double intervalMoneyUSD, int intervalMonths, List<KeyValuePair<DateTime, double>> stockPricesUSD, bool taxIncluded = false)
        {
            List<KeyValuePair<double, StockToken>> boughtTokens = new List<KeyValuePair<double, StockToken>>();
            double result = 0;
            double moneyToInvest = 0;
            moneyToInvest += startMoneyUSD;

            List<KeyValuePair<DateTime,double>> i7 = dataModel.xAxis.Zip(dataModel.IndicatorsList["EMA7"], (x, v) => new KeyValuePair<DateTime, double>(x, v)).ToList();
            List<KeyValuePair<DateTime,double>> i180 = dataModel.xAxis.Zip(dataModel.IndicatorsList["EMA180"], (x, v) => new KeyValuePair<DateTime, double>(x, v)).ToList();

            double rateUSD_PLN = 1;
            var filteredStockPrices = stockPricesUSD.OrderBy(k => k.Key).Where(k => k.Key.Date >= DateTime.Parse(startDate).Date && k.Key.Date <= DateTime.Parse(endDate).Date).ToList();

            if (i180.Where(i => i.Key == filteredStockPrices[0].Key).FirstOrDefault().Value < i7.Where(i => i.Key == filteredStockPrices[0].Key).FirstOrDefault().Value)
            {
                double numberOfTokens = moneyToInvest / (filteredStockPrices[0].Value * rateUSD_PLN);
                boughtTokens.Add(new KeyValuePair<double, StockToken>(numberOfTokens, new StockToken(filteredStockPrices[0].Value, filteredStockPrices[0].Key)));
                moneyToInvest = 0;
            }

            //while loop {} => iterate through dates  => according to algorithm buy or sell  +  be vigilant for signals from market i7=i180 +3 days => sum up all gains -  19 %
            //use money to invest as modyficator when sell ( full) wneh buy => empty


            // buystoc() => with checking if shoud be bought if invested - then money to invest eqal 0
            // sell => gains to gains/ bought cleared / money to invest 9 after sell)

            // badanie zmiennosci cen => kluczeoe do wybrania  bazowego wskaznika
            // check volatility  in recend 10 /30 days => if  higher or lower => adjust emaPriod as a base signal to entry /close decision
            // checking last crossed price => if similar 1%  volatility => do not change tactic and wait 
            // emas/ sma shoudl be parallel in slopes to   ema7
            // or support wim MACD

            // idea is :
            // 1. Get indicators
            // 2. simple complexity - just EMA for S&P500 example back tesing
            // when ema 7 and ema 180 is crossed => take actions:
            // (ema180 3days before < ema7 3 days before ) &&  (ema180 3days after > ema7 3 days after ) => sell => check  gain/loss result => with date( or without)
            // (ema180 3days before > ema7 3 days before ) &&  (ema180 3days after < ema7 3 days after ) => BUY 
            // alternative to previous 2:
            // (ema180 3days after > ema7 3 days after ) => sell => check  gain/loss result => with date( or without)
            // (ema180 3days after < ema7 3 days after ) => BUY 
            // if (ema180 < ema 7) => keep buying
            // if (ema180 < ema 7) => dont buy or  sell if  you have something already bought
            // on last date sell with end date price => sumUp gains/loss  and substract  Taxtes ( 19 proce from  each registred sell gain)


            //List<KeyValuePair<double,StockToken>> boughtTokens=new List<KeyValuePair<double, StockToken>>();

            //stockPrices order by date
            //filter by date
            //double rateUSD_PLN = 1;
            //var filteredStockPrices = stockPricesUSD.OrderBy(k => k.Key).Where(k => k.Key.Date >= DateTime.Parse(startDate).Date && k.Key.Date <= DateTime.Parse(endDate).Date).ToList();

            //double numberOfTokens = startMoneyUSD / (filteredStockPrices[0].Value*rateUSD_PLN);

            //boughtTokens.Add(new KeyValuePair<double, StockToken>(numberOfTokens, new StockToken(filteredStockPrices[0].Value, filteredStockPrices[0].Key)));

            var checkData = filteredStockPrices[0].Key.AddMonths(intervalMonths);

            double numberOfTokens;
            while (checkData<=DateTime.Parse(endDate))
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

            //boughtTokens.ForEach(t => Console.WriteLine($" date : {t.Value.Date}  , price : {t.Value.Price}"));

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
