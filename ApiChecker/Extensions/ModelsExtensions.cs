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
    }
}
