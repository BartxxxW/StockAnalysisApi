using ApiChecker.DataProcessing;
using ApiChecker.Models;
using ApiChecker.RequestStockData;
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
        IStockAPI GetStockData(string StockSymbol,TimeSeries timeSeries=TimeSeries.Daily, string startDay = "", string endDay = "");
    }
    public interface IStockAPI
    {
        IStockAPI AddIndicatior(string IndicatorName, int? param = null, string startDay = "", string endDay = "");
        ProcessedStockDataModel ReturnApiData();
    }
    public class StockAPI: IGetStockData, IStockAPI
    {

        public IRequests Requests { get; set; }

        public ProcessedStockDataModel ProcessedStockDataModel = new ProcessedStockDataModel();
        public List<StockModel> StockData { get; set; }


        public IStockAPI GetStockData(string StockSymbol, TimeSeries timeSeries = TimeSeries.Daily, string startDay = "", string endDay = "")
        {
            var deserialized = DeserializeAvResponse.TimeSeriesDaily(Requests.TimeSeriesDaily(StockSymbol).Content);

            var stockModel= deserialized.TimeSeriesDaily.Select(tds => tds.ConvertToStockModel());

            // up to now create  isnatnce from  test factory and   get stock model data from that

            if (startDay != "")
                stockModel = stockModel.Where(s => s.Date > DateTime.Parse(startDay));

            if (endDay != "")
                stockModel = stockModel.Where(s => s.Date < DateTime.Parse(endDay));

            StockData=stockModel.ToList();

            ProcessedStockDataModel.xAxis=StockData.Select(s=>s.Date).ToList();
            ProcessedStockDataModel.yValues=StockData.Select(s=> Convert.ToDouble(s.Open)).ToList();

            return this;
        }

        public IStockAPI AddIndicatior( string IndicatorName,int? param=null, string startDay="",string endDay="")
        {
            return this;
        }

        public ProcessedStockDataModel ReturnApiData()
        {
            return ProcessedStockDataModel;
        }

        public static IGetStockData Instance(IRequests requestSource) => new StockAPI(requestSource);
        private StockAPI(IRequests requestSource) { 
               this.Requests = requestSource;
        }

    }
}
