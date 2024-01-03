using ApiChecker.JsonModels;
using ApiChecker.SkendorStockModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiChecker.ToolBox
{
    public static  class ModelConverter
    {
        public static StockModel ConvertToStockModel(this KeyValuePair<string,DateStock> tds)
        {
            var sm= new StockModel();
            sm.Date = DateTime.Parse(tds.Key);
            sm.Open = Convert.ToDecimal(tds.Value._1Open.Replace('.',','));
            sm.High = Convert.ToDecimal(tds.Value._2High.Replace('.', ','));
            sm.Low = Convert.ToDecimal(tds.Value._3Low.Replace('.', ','));
            sm.Close = Convert.ToDecimal(tds.Value._4Close.Replace('.', ','));
            sm.Volume = Convert.ToDecimal(tds.Value._5Volume.Replace('.', ','));
            return sm;
        }
    }
}
