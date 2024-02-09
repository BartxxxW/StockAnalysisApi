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
    }
}
