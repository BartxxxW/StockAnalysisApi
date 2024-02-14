using ApiChecker.InvestingStrategies;
using ApiChecker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiChecker.Extensions
{
    public static class ModelsExtensions
    {
        public static List<KeyValuePair<DateTime,double>> GetIndicatorWithDatesFromDataModel(this ProcessedStockDataModel dataModel,string indicator)
        {
            return dataModel.xAxis.Zip(dataModel.IndicatorsList[indicator], (x, v) => new KeyValuePair<DateTime, double>(x, v)).ToList();
        }
        public static double ClosedTokenGain(this ClosedStockToken token)
        {
            double res = 0;
            if(token.ClosedPrice>token.Price)
            {
                res= token.ClosedPrice-token.Price;
            }

            return res;
        }
        public static double ClosedTokenLoss(this ClosedStockToken token)
        {
            double res = 0;
            if (token.ClosedPrice < token.Price)
            {
                res = token.Price - token.ClosedPrice;
            }

            return res;
        }
    }
}
