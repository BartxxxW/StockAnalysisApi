using ApiChecker.DataProcessing;
using ApiChecker.SkendorStockModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiChecker.Models
{
    public class ProcessedStockDataModel
    {
        public List<DateTime> xAxis { get; set; }
        public List<double> yValues { get; set; }

        public List<KeyValuePair<DateTime,double>> StockPrices =>xAxis.Zip(yValues,(x,v)=>new KeyValuePair<DateTime, double>(x,v)).ToList();


        public Dictionary<string, List<double>> IndicatorsList { get; set; } = new Dictionary<string, List<double>>();
    }
}
