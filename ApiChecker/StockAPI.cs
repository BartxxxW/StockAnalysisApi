using ApiChecker.DataProcessing;
using ApiChecker.Models;
using ApiChecker.RequestStockData;
using ApiChecker.Services;
using ApiChecker.SkendorStockModels;
using ApiChecker.ToolBox;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiChecker
{
    public interface IGetStockData
    {
        IStockActions GetStockData(string StockSymbol, Sources sources = Sources.AlphaVintage, TimeSeries timeSeries=TimeSeries.Daily, string startDay = "", string endDay = "");
    }
    public interface IStockActions
    {
        IStockActions AddIndicator(string IndicatorName, int? param = null, string startDay = "", string endDay = "");
        ProcessedStockDataModel ReturnApiData();
    }
    public class StockAPI : IGetStockData, IStockActions, IStockAPI
    {

        public IRequests Requests { get; set; }

        public ProcessedStockDataModel ProcessedStockDataModel = new ProcessedStockDataModel();
        public List<StockModel> StockData { get; set; }


        public IStockActions GetStockData(string StockSymbol, Sources sources = Sources.AlphaVintage, TimeSeries timeSeries = TimeSeries.Daily, string startDay = "", string endDay = "")
        {

            var stockModel = _servicesResolver.GetService(sources).Action(StockSymbol, timeSeries);


            if (startDay != "")
                stockModel = stockModel.Where(s => s.Date > DateTime.Parse(startDay));

            if (endDay != "")
                stockModel = stockModel.Where(s => s.Date < DateTime.Parse(endDay));

            StockData = stockModel.ToList();

            ProcessedStockDataModel.xAxis = StockData.Select(s => s.Date).ToList();
            ProcessedStockDataModel.yValues = StockData.Select(s => Convert.ToDouble(s.Open)).ToList();

            return this;
        }

        public IStockActions AddIndicator(string IndicatorName, int? param = null, string startDay = "", string endDay = "")
        {
            return this;
        }

        public ProcessedStockDataModel ReturnApiData()
        {
            return ProcessedStockDataModel;
        }

        //public static IGetStockData Instance(IRequests requestSource) => new StockAPI(requestSource);
        public static IGetStockData Instance(IServicesResolver servicesResolver) => new StockAPI(servicesResolver);
        public StockAPI(IServicesResolver servicesResolver)
        {
            _servicesResolver = servicesResolver;
        }

        private IServicesResolver _servicesResolver;
    }
}
