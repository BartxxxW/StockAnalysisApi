using ApiChecker.DataProcessing;
using ApiChecker.RequestStockData;
using ApiChecker.SkendorStockModels;
using ApiChecker.ToolBox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiChecker.Services
{
    public interface IService
    {
        public IEnumerable<StockModel>? Action(string StockSymbol,TimeSeries ts = TimeSeries.Daily);
    }
    public class AlphaVintageService: IService
    {
        public Dictionary<TimeSeries, Func<string, IEnumerable<StockModel>?>> CallMethod =
            new Dictionary<TimeSeries, Func<string, IEnumerable<StockModel>?>>();

        public IRequests Requests { get; set; }
        public AlphaVintageService(IRequests requests)
        { 
            Requests = requests;
            CallMethod.Add(TimeSeries.Daily, TimeSeriesDaily);
        }

        public IEnumerable<StockModel>? TimeSeriesDaily(string StockSymbol)
        {
            var deserialized = DeserializeAvResponse.TimeSeriesDaily(Requests.TimeSeriesDaily(StockSymbol).Content);

            return deserialized.TimeSeriesDaily.Select(tds => tds.ConvertToStockModel());
        }

        public IEnumerable<StockModel>? Action(string StockSymbol,TimeSeries ts = TimeSeries.Daily)
        {
            return CallMethod[ts](StockSymbol);
        }
    }
}
