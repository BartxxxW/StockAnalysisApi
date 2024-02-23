using ApiChecker.Extensions;
using ApiChecker.ToolBox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ApiChecker.Extensions.MathExtensions;

namespace ApiChecker.InvestingStrategies
{
    public abstract class StratedyBase
    {
        public int testIterator = 0;
        public DateTime StartDate {  get; set; }
        public DateTime EndDate { get; set; }
        public List<KeyValuePair<double, StockToken>> boughtTokens = new List<KeyValuePair<double, StockToken>>();
        public List<KeyValuePair<double, ClosedStockToken>> closedTokens = new List<KeyValuePair<double, ClosedStockToken>>();
        public List<KeyValuePair<DateTime, double>> i7 { get; set; }
        public List<KeyValuePair<DateTime, double>> i180 { get; set; }
        public List<DateTime> datesToBuy = new List<DateTime>();
        public List<KeyValuePair<DateTime, double>> filteredStockPrices { get; set; }
        public List<DateTime> SellDates = new List<DateTime>();
        public List<DateTime> BuyDates = new List<DateTime>();
        public List<StockAction> StockActions = new List<StockAction>();
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


        public StockAction Last3Days()
        {
            if (StockActions.Count() < 4)
                return StockAction.Default;

            var last4 = StockActions.Skip(Math.Max(0, StockActions.Count - 4)).ToList();

            if (last4[0] != StockAction.Wait)
                return StockAction.Default;

            var last3 = StockActions.Skip(Math.Max(0, StockActions.Count - 3)).ToList();

            if (last3.All(s => s == StockAction.Buy))
                return StockAction.Buy;

            if (last3.All(s => s == StockAction.Sell))
                return StockAction.Sell;

            return StockAction.Default;
        }
        public void TakeAction(StockAction action, DateTime investDay)
        {
            if (action == StockAction.Buy)
                Buy(investDay);

            if (action == StockAction.Sell)
                Sell(investDay);
        }
        public StockAction CheckAction(DateTime investDay)
        {
            var i7Value = i7.GetIndicatorValue(investDay);
            var i180Value = i180.GetIndicatorValue(investDay);
            var stockValue = filteredStockPrices.GetStockValue(investDay);
            bool AreEqual = AreAlmostEqual(i180Value, i7Value);

            if (i180Value < i7Value && AreEqual == false)
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
        public void GetDatesToBuy(string startDate, string endDate, int intervalMonths, bool withStartDay=false)
        {
            if (withStartDay)
                datesToBuy.Add(DateTime.Parse(startDate));

            var nextDate = DateTime.Parse(startDate).AddMonths(intervalMonths);
            while (nextDate <= DateTime.Parse(endDate))
            {
                datesToBuy.Add(nextDate);
                nextDate = nextDate.AddMonths(intervalMonths);
            }
        }
        public void AddMoneyToInvest(DateTime investDay, double money)
        {
            if (datesToBuy.Contains(investDay))
            {
                moneyToInvest += money;
                investedMoney += money;
            }
        }

        public double CalculateGain(List<KeyValuePair<double, ClosedStockToken>> closedTokens, int Year)
        {
            List<double> gainsPerTransaction = new List<double>();
            var tokensInTaxYear = closedTokens.Where(t => t.Value.ClosedDate.Year == Year).ToList();

            tokensInTaxYear.ForEach(t => gainsPerTransaction.Add(t.Key * t.Value.ClosedTokenGain()));

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
        public double PayTaxes()
        {
            double TaxToPay = 0;
            HashSet<int> taxYears = new HashSet<int>();
            closedTokens.Select(t => t.Value.ClosedDate.Year).ToList().ForEach(date => taxYears.Add(date));

            foreach (var taxYear in taxYears)
            {
                double bilans = 0;
                double gain = CalculateGain(closedTokens, taxYear);
                double loss = CalculateLoss(closedTokens, taxYear);

                if (gain > loss)
                    bilans = gain - loss;

                TaxToPay += Calculate19Tax(bilans);

            }

            return TaxToPay;
        }
    }
}
