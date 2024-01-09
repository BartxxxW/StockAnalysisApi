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

        public List<KeyValuePair<string, List<double>>> IndicatorsList { get; set; }
    }
}
