using ApiChecker.DataProcessing;
using ApiChecker.Models;
using ApiChecker.RequestStockData;
using ApiChecker.SkendorStockModels;

namespace ApiChecker
{
    public interface IStockAPI
    {
        IRequests Requests { get; set; }
        List<StockModel> StockData { get; set; }

        IStockActions AddIndicator(Indicators indicator, params int[] param);
        IStockActions GetStockData(string StockSymbol, Sources sources = Sources.AlphaVintage, TimeSeries timeSeries = TimeSeries.Daily, string startDay = "", string endDay = "");
        ProcessedStockDataModel ReturnApiData();
    }
}