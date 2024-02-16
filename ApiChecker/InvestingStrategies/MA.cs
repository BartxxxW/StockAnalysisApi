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
using static ApiChecker.Extensions.MathExtensions;

namespace ApiChecker.InvestingStrategies
{
    public class MA : IStrategy
    {
        public int testIterator = 0;
        public List<KeyValuePair<double, StockToken>> boughtTokens = new List<KeyValuePair<double, StockToken>>();
        public List<KeyValuePair<double, ClosedStockToken>> closedTokens = new List<KeyValuePair<double, ClosedStockToken>>();
        public List<KeyValuePair<DateTime, double>> i7 {get;set;}
        public List<KeyValuePair<DateTime, double>> i180 {get;set;}
        public List<DateTime> datesToBuy = new List<DateTime>();
        public List<KeyValuePair<DateTime,double>> filteredStockPrices { get;set;}
        public List<DateTime> SellDates = new List<DateTime>();
        public List<DateTime> BuyDates = new List<DateTime>();
        public double moneyToInvest = 0;
        public double investedMoney = 0;
        public double intervalSavings = 0;


        public void Buy(DateTime investDay)
        {
            if (moneyToInvest == 0)
                return;

            var stockValue = filteredStockPrices.GetStockValue(investDay);
            double numberOfTokens2 = moneyToInvest / stockValue;
            boughtTokens.Add(new KeyValuePair<double, StockToken>(numberOfTokens2, new StockToken(filteredStockPrices.GetStockValue(investDay), filteredStockPrices.GetStockDate(investDay))));
            moneyToInvest = 0;
            BuyDates.Add(investDay);
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

            moneyToInvest += numberOfBoughtTokens * stockValue;
            SellDates.Add(investDay);
        }
        public DateTime TakeAction(DateTime investDay)
        {
            var i7Value = i7.GetIndicatorValue(investDay);
            var i180Value = i180.GetIndicatorValue(investDay);
            var stockValue = filteredStockPrices.GetStockValue(investDay);
            DateTime nextDate = investDay;
            bool AreEqual = AreAlmostEqual(i180Value, i7Value);

            if (i180Value < i7Value && AreEqual == false)
            {
                Buy(investDay);
            }
            if (i180Value > i7Value && AreEqual == false)
            {
                Sell(investDay);
            }

            if (AreEqual)
            {
                bool end = false;
                var iDay = investDay;
                while (end == false)
                {
                    //here is wrong because it  add each time new invested an so on
                    // day mover  and event shoud be used
                    // or in while loop - just flag to monitor next 3 days and waiting for result
                    var nextDays = VerifyNext3Days(iDay).ToList();
                    nextDate = iDay.AddDays(3);
                    if (nextDays.All(d => d == StockAction.Buy) && nextDays.Count() == 3)
                    {
                        Buy(nextDate);
                        end = true;
                    }
                    else if (nextDays.All(d => d == StockAction.Sell) && nextDays.Count() == 3)
                    {
                        Sell(nextDate);
                        end = true;
                    }
                    else
                    {
                        iDay = iDay.AddDays(1);
                    }
                }
            }

            return nextDate;
        }
        private StockAction CheckAction(DateTime investDay)
        {
            var i7Value = i7.GetIndicatorValue(investDay);
            var i180Value = i180.GetIndicatorValue(investDay);
            var stockValue = filteredStockPrices.GetStockValue(investDay);
            bool AreEqual = AreAlmostEqual(i180Value, i7Value);

            if (i180Value < i7Value && AreEqual==false)
            {
                return StockAction.Buy;
            }
            if (i180Value > i7Value && AreEqual == false)
            {
                return StockAction.Sell;
            }

            if (AreEqual)
            {
                return StockAction.Wait;
            }
            return StockAction.Default;
        }

        private IEnumerable<StockAction> VerifyNext3Days( DateTime investDay )
        {
            for (int i=1;i<4;i++)
            {
                var day=investDay.AddDays(i);
                if (datesToBuy.Contains(day))
                {
                    AddMoneyToInvest(day, intervalSavings);
                    testIterator++;
                }
                var stockAction=CheckAction(day);
                yield return stockAction;

            }
        }
        private void GetDatesToBuy(string startDate,string endDate, int intervalMonths)
        {
            var nextDate = DateTime.Parse(startDate).AddMonths(intervalMonths);
            while (nextDate <= DateTime.Parse(endDate))
            {
                datesToBuy.Add(nextDate);
                nextDate=nextDate.AddMonths(intervalMonths);
            }
        }
        public void AddMoneyToInvest(DateTime investDay,double money)
        {
            if (datesToBuy.Contains(investDay))
            { 
                moneyToInvest += money;
                investedMoney += money;
            }
        }

        public double CalculateGain(List<KeyValuePair<double,ClosedStockToken>> closedTokens,int Year)
        {
            List<double> gainsPerTransaction = new List<double>();
            var tokensInTaxYear=closedTokens.Where(t=>t.Value.ClosedDate.Year== Year).ToList();

            tokensInTaxYear.ForEach(t=>gainsPerTransaction.Add(t.Key*t.Value.ClosedTokenGain()));

            return gainsPerTransaction.Sum();
        }
        public double CalculateLoss(List<KeyValuePair<double, ClosedStockToken>> closedTokens, int Year)
        {
            List<double> lossPerTransaction = new List<double>();
            var tokensInTaxYear = closedTokens.Where(t => t.Value.ClosedDate.Year == Year).ToList();

            tokensInTaxYear.ForEach(t => lossPerTransaction.Add(t.Key * t.Value.ClosedTokenLoss()));

            return lossPerTransaction.Sum();
        }
        public double Calculate19Tax(double money)
        {
            double TaxToPay = 0;

            TaxToPay = money * 0.19;
            return TaxToPay;
        }
        private double PayTaxes()
        {
            double TaxToPay = 0;
            HashSet<int> taxYears= new HashSet<int>();
            closedTokens.Select(t => t.Value.ClosedDate.Year).ToList().ForEach(date => taxYears.Add(date));

            foreach(var taxYear in taxYears)
            {
                double bilans = 0;
                double gain = CalculateGain(closedTokens,taxYear);
                double loss = CalculateLoss(closedTokens,taxYear);

                if (gain > loss)
                    bilans = gain - loss;

                TaxToPay += Calculate19Tax(bilans);

            }

            return TaxToPay;
        }
        public double Simulate(string baseIndicator,string Indicator, ProcessedStockDataModel dataModel,string startDate, string endDate,double startMoneyUSD, double intervalMoneyUSD, int intervalMonths,  bool taxIncluded = false)
        {

            double result = 0;
            
            moneyToInvest += startMoneyUSD;
            investedMoney += startMoneyUSD;

            i7 = dataModel.GetIndicatorWithDatesFromDataModel(baseIndicator);
            i180 = dataModel.GetIndicatorWithDatesFromDataModel(Indicator);
            intervalSavings = intervalMoneyUSD;
            


            filteredStockPrices = dataModel.StockPrices.GetStockRangeByDate(startDate, endDate);

            var dt_EndDate = filteredStockPrices.Last().Key.Date;
            GetDatesToBuy(startDate, dt_EndDate.ToString(), intervalMonths);

            var DayInLoop = DateTime.Parse(startDate);
            int iterator = 0;

            while(DayInLoop<= dt_EndDate)
            {
                var iSmall= i7.GetIndicatorValue(DayInLoop);
                var iLarge= i180.GetIndicatorValue(DayInLoop);
                if(datesToBuy.Contains(DayInLoop))
                {
                    AddMoneyToInvest(DayInLoop, intervalMoneyUSD);
                    testIterator++;
                }


                //just monitor status
                //add to list of statuses
                //check last 4 => if wait and 3 buys => buy else sell

                if (AreAlmostEqual(iSmall, iLarge))
                {
                    //check status
                    //add status to list
                    //fix here
                    DayInLoop=TakeAction(DayInLoop);

                }

                //if 3 lats statuses == buy => buy  else sell

                DayInLoop=DayInLoop.AddDays(1);

            }

            Sell(dt_EndDate);
            Console.WriteLine("iterations:"+ testIterator);
            double paidInMoney = startMoneyUSD + datesToBuy.Count * intervalMoneyUSD;
            double resultWithoutTaxes = moneyToInvest;

            double resultAfterTaxes = resultWithoutTaxes - PayTaxes();

            Console.WriteLine($"MA=> PaidIn:{paidInMoney} ; result without taxes: {resultWithoutTaxes} ; afterTAxes!: {resultAfterTaxes} ; afterTAinvestedmoneyxes!: {investedMoney}");


            return result;
        }
    }
}
