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


        public void Buy(DateTime investDay)
        {
            var stockValue = filteredStockPrices.GetStockValue(investDay);
            double numberOfTokens2 = moneyToInvest / stockValue;
            boughtTokens.Add(new KeyValuePair<double, StockToken>(numberOfTokens2, new StockToken(filteredStockPrices.GetStockValue(investDay), filteredStockPrices.GetStockDate(investDay))));
            moneyToInvest = 0;
        }

        public void Sell(DateTime investDay)
        {
            if (boughtTokens.Count == 0)
                return;

            double numberOfBoughtTokens = boughtTokens.Select(t => t.Key).Sum();
            var stockValue = filteredStockPrices.GetStockValue(investDay);
            var sellDate = filteredStockPrices.GetStockDate(investDay);
            closedTokens.AddRange(boughtTokens.Select(t => new KeyValuePair<double, ClosedStockToken>(t.Key, t.Value.ConvertToClosedStockToken(stockValue, sellDate))));
            boughtTokens.Clear();

            moneyToInvest = numberOfBoughtTokens * stockValue;
        }
        public void TakeAction(DateTime investDay)
        {
            var i7Value = i7.GetIndicatorValue(investDay);
            var i180Value = i180.GetIndicatorValue(investDay);
            var stockValue = filteredStockPrices.GetStockValue(investDay);

            if (i180Value < i7Value)
            {
                Buy(investDay);
            }
            if (i180Value > i7Value)
            {
                Sell(investDay);
            }

            if (i180Value == i7Value)
            {
                //for loop
                VerifyNext3Days(investDay);
            }

            //return day
        }
        private int VerifyNext3Days( DateTime investDay )
        {
            //to develop
            return 0;
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
        public void AddMoneyToInvest(DateTime investDay,double money)
        {
            if (datesToBuy.Contains(investDay))
                moneyToInvest += money;
        }
        public double Simulate(ProcessedStockDataModel dataModel,string startDate, string endDate,double startMoneyUSD, double intervalMoneyUSD, int intervalMonths, List<KeyValuePair<DateTime, double>> stockPricesUSD, bool taxIncluded = false)
        {

            double result = 0;
            
            moneyToInvest += startMoneyUSD;

            i7 = dataModel.GetIndicatorWithDatesFromDataModel("EMA7");
            i180 = dataModel.GetIndicatorWithDatesFromDataModel("EMA180");

            GetDatesToBuy(startDate, endDate, intervalMonths);


            filteredStockPrices = stockPricesUSD.GetStockRangeByDate(startDate, endDate);

            var dt_EndDate = filteredStockPrices.Last().Key.Date;
            var investDay = DateTime.Parse(startDate);
            var i7Value = i7.GetIndicatorValue(investDay);
            var i180Value = i180.GetIndicatorValue(investDay);

            if (i180Value < i7Value)
            {
                Buy(investDay);
            }




            
            var DayInLoop = DateTime.Parse(startDate).AddDays(1);
            if (boughtTokens.Count > 0)
                DayInLoop = boughtTokens[0].Value.Date.AddDays(1);

            while(DayInLoop<= dt_EndDate)
            {
                var iSmall= i7.GetIndicatorValue(DayInLoop);
                var iLarge= i180.GetIndicatorValue(DayInLoop);

                if (iSmall==iLarge || datesToBuy.Contains(DayInLoop))
                {
                    AddMoneyToInvest(DayInLoop, intervalMoneyUSD);

                    TakeAction(DayInLoop); //maybe should return a day ?
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








            return result;
        }
    }
}
