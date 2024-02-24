using ApiChecker.InvestingStrategies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiChecker.Extensions
{
    public static class ListExtensions
    {

        public static double GetIndicatorValue(this List<KeyValuePair<DateTime,double>> list,DateTime date) 
        {
            double res = 0;
            res=list.Where(i => i.Key >= date && i.Key < date.AddDays(4)).FirstOrDefault().Value;
            return res;
        }
        public static double GetIndicatorValue(this List<KeyValuePair<DateTime, double>> list, string sDate)
        {
            double res = 0;
            var date=DateTime.Parse(sDate);
            res = list.Where(i => i.Key >= date && i.Key < date.AddDays(4)).FirstOrDefault().Value;
            return res;
        }
        public static  List<KeyValuePair<DateTime, double>> GetStockRangeByDate(this List<KeyValuePair<DateTime,double>> stocks,string startDate, string endDate)
        {
            return stocks.OrderBy(k => k.Key).Where(k => k.Key.Date >= DateTime.Parse(startDate).Date && k.Key.Date <= DateTime.Parse(endDate).Date).ToList();
        }
        public static double GetStockValue(this  List<KeyValuePair<DateTime, double>> stocks, DateTime date)
        {
            return stocks.Where(s => s.Key >= date && s.Key <= date.AddDays(4)).FirstOrDefault().Value;
        }
        public static DateTime GetStockDate(this List<KeyValuePair<DateTime, double>> stocks, DateTime date)
        {
            return stocks.Where(i => i.Key >= date && i.Key < date.AddDays(4)).FirstOrDefault().Key;
        }
        public static TokenList CastToClosedToken (this TokenList tokenList, DateTime date, double stockPrice) 
        {
            tokenList.ForEach(e=>e.Value.CastToClosedToken(date,stockPrice));
            return tokenList;

        }
        public static double CalculateEndValue(this TokenList tokenList) 
        {
            return tokenList.Select(t=>t.Value.ClosePrice).Sum();
        }
        public static double CalculateStartValue(this TokenList tokenList)
        {
            return tokenList.Select(t => t.Value.OpenPrice).Sum();
        }
        public static double CalculateBeforeLevar(this TokenList tokenList)
        {
            return tokenList.Select(t => t.Value.OpenPrice / t.Value.Levar).Sum();
        }

        public static void PayForSwap(this Account account, double percent,DateTime date)
        {
            if (account.LongPositions.Count == 0 || percent==0)
            {
                return;
            }
            percent = percent / 100;

            var valueToPay= account.LongPositions.Where(t => t.Value.OpenDate != date)
                .Select(t => t.Key * t.Value.OpenPrice * percent)
                .Sum();

            if(valueToPay>=account.ReserveAccount)
            {
                Console.WriteLine(  "it is");
            }

            account.PayWithReserveAccount(valueToPay,true);
        }
        public static void PayTaxes( this Account account)
        {
            double TaxToPay = 0;
            HashSet<int> taxYears = new HashSet<int>();
            account.ClosedTokensBilans.Select(t => t.Value.Year).ToList().ForEach(date => taxYears.Add(date));

            foreach (var taxYear in taxYears)
            {
                double bilans = account.ClosedTokensBilans.Where(i => i.Value.Year == taxYear).Select(i=>i.Key).Sum();

                TaxToPay += bilans.Calculate19Tax();

            }

            account.PayForSth(TaxToPay);
        }

        public static double Calculate19Tax(this double money)
        {
            if (money < 0)
                return 0;

            return money * 0.19; ;
        }
    }
}
