using ApiChecker.Extensions;
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
        // invest every 4 months but be vigilant with  market chenges
        //loss or gains on the end of year should be counted
        // 21/(60)/180
        public List<KeyValuePair<double, StockToken>> boughtTokens = new List<KeyValuePair<double, StockToken>>();
        public List<KeyValuePair<double, ClosedStockToken>> closedTokens = new List<KeyValuePair<double, ClosedStockToken>>();
        public List<KeyValuePair<DateTime, double>> i7 {get;set;}
        public List<KeyValuePair<DateTime, double>> i180 {get;set;}
        public List<DateTime> datesToBuy = new List<DateTime>();
        public List<KeyValuePair<DateTime,double>> filteredStockPrices { get;set;}
        public double moneyToInvest = 0;
        public void TakeAction()
        {
            if (i180Value < i7Value)
            {
                double numberOfTokens2 = moneyToInvest / (filteredStockPrices[0].Value * rateUSD_PLN);
                boughtTokens.Add(new KeyValuePair<double, StockToken>(numberOfTokens2, new StockToken(filteredStockPrices[0].Value, filteredStockPrices[0].Key)));
                moneyToInvest = 0;
            }
            if (i180Value > i7Value)
            {
                double numberOfBoughtTokensIN = boughtTokens.Select(t => t.Key).Sum();

                var sellDate = i7.Where(i => i.Key >= investDay && i.Key < investDay.AddDays(3)).FirstOrDefault().Key; // to replace with end date value
                double priceAtSellDateIn = filteredStockPrices.Where(s => s.Key >= sellDate && s.Key <= sellDate.AddDays(4)).FirstOrDefault().Value;
                closedTokens.AddRange(boughtTokens.Select(t => new KeyValuePair<double, ClosedStockToken>(t.Key, t.Value.ConvertToClosedStockToken(priceAtSellDateIn, sellDate))));
                boughtTokens.Clear();

                moneyToInvest = numberOfBoughtTokensIN * priceAtSellDateIn;
            }

            if (i180Value == i7Value)
            {
                // signal => what will happen in next 3 days
                double numberOfTokens2 = moneyToInvest / (filteredStockPrices[0].Value * rateUSD_PLN);
                boughtTokens.Add(new KeyValuePair<double, StockToken>(numberOfTokens2, new StockToken(filteredStockPrices[0].Value, filteredStockPrices[0].Key)));
                moneyToInvest = 0;
            }
        }
        private void GetDatesToBuy(string startDate,string endDate, int intervalMonths)
        {
            var nextDate = DateTime.Parse(startDate).AddMonths(intervalMonths);
            while (nextDate <= DateTime.Parse(endDate))
            {
                datesToBuy.Add(nextDate);
                nextDate.AddMonths(intervalMonths);
            }
        }
        public double Simulate(ProcessedStockDataModel dataModel,string startDate, string endDate,double startMoneyUSD, double intervalMoneyUSD, int intervalMonths, List<KeyValuePair<DateTime, double>> stockPricesUSD, bool taxIncluded = false)
        {

            double result = 0;
            
            moneyToInvest += startMoneyUSD;

            i7 = dataModel.GetIndicatorWithDatesFromDataModel("EMA7");
            i180 = dataModel.GetIndicatorWithDatesFromDataModel("EMA180");

            GetDatesToBuy(startDate, endDate, intervalMonths);

            double rateUSD_PLN = 1;

            filteredStockPrices = stockPricesUSD.GetStockRangeByDate(startDate, endDate);

            var dt_EndDate = filteredStockPrices.Last().Key.Date;
            var investDay = DateTime.Parse(startDate);
            var i7Value = i7.GetIndicatorValue(investDay);
            var i180Value = i180.GetIndicatorValue(investDay);

            if (i180Value < i7Value)
            {
                double numberOfTokens2 = moneyToInvest / (filteredStockPrices[0].Value * rateUSD_PLN);
                boughtTokens.Add(new KeyValuePair<double, StockToken>(numberOfTokens2, new StockToken(filteredStockPrices[0].Value, filteredStockPrices[0].Key)));
                moneyToInvest = 0;
            }
            if (i180Value > i7Value)
            {
                double numberOfBoughtTokensIN = boughtTokens.Select(t => t.Key).Sum();

                var sellDate = i7.Where(i => i.Key >= investDay && i.Key < investDay.AddDays(3)).FirstOrDefault().Key; // to replace with end date value
                double priceAtSellDateIn = filteredStockPrices.Where(s => s.Key >= sellDate && s.Key <= sellDate.AddDays(4)).FirstOrDefault().Value;
                closedTokens.AddRange(boughtTokens.Select(t=>new KeyValuePair<double, ClosedStockToken>(t.Key,t.Value.ConvertToClosedStockToken(priceAtSellDateIn, sellDate))));
                boughtTokens.Clear();

                moneyToInvest = numberOfBoughtTokensIN * priceAtSellDateIn;
            }

            if (i180Value == i7Value)
            {
                // signal => what will happen in next 3 days
                double numberOfTokens2 = moneyToInvest / (filteredStockPrices[0].Value * rateUSD_PLN);
                boughtTokens.Add(new KeyValuePair<double, StockToken>(numberOfTokens2, new StockToken(filteredStockPrices[0].Value, filteredStockPrices[0].Key)));
                moneyToInvest = 0;
            }

            
            var DayInLoop = DateTime.Parse(startDate).AddDays(1);
            if (boughtTokens.Count > 0)
                DayInLoop = boughtTokens[0].Value.Date;

            while(DayInLoop<= dt_EndDate)
            {
                // decision when indicators are EQUAL or it is IntervalDay to invest 
                var iSmall= i7.GetIndicatorValue(DayInLoop);
                var iLarge= i180.GetIndicatorValue(DayInLoop);

                if (iSmall==iLarge || datesToBuy.Contains(DayInLoop))
                {
                    // market action
                }

                DayInLoop.AddDays(1);

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
