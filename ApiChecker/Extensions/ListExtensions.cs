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

        public static double PayForSwap(this TokenList tokenList, double percent,DateTime date)
        {
            if (tokenList.Count == 0 || percent==0)
            {
                return 0;
            }

            return tokenList.Where(t=>t.Value.OpenDate!=date)
                .Select(t => t.Key * t.Value.OpenPrice* t.Value.Levar * percent)
                .Sum();
        }
    }
}
