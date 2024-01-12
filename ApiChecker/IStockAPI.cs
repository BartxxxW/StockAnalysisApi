using ApiChecker.Models;
using ApiChecker.RequestStockData;
using ApiChecker.SkendorStockModels;

namespace ApiChecker
{
    public interface IStockAPI
    {
        IRequests Requests { get; set; }
        List<StockModel> StockData { get; set; }

        IStockActions AddIndicator(string IndicatorName, int? param = null, string startDay = "", string endDay = "");
        IStockActions GetStockData(string StockSymbol, Sources sources = Sources.AlphaVintage, TimeSeries timeSeries = TimeSeries.Daily, string startDay = "", string endDay = "");
        ProcessedStockDataModel ReturnApiData();
    }
}