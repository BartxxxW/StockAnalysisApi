using ApiChecker.DataProcessing;
using ApiChecker.Entities;
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
        IStockActions GetStockData(string StockSymbol, Sources sources = Sources.AlphaVintage, TimeSeries timeSeries=TimeSeries.Daily, string startDay = "", string endDay = "", bool apiUp = false);
    }
    public interface IStockActions
    {
        IStockActions AddIndicator(string indicator, params int[] param);
        ProcessedStockDataModel ReturnApiData();
    }
    public class StockAPI : IGetStockData, IStockActions, IStockAPI
    {

        public IRequests Requests { get; set; }

        public ProcessedStockDataModel ProcessedStockDataModel = new ProcessedStockDataModel();
        public List<StockModel> StockData { get; set; }


        public IStockActions GetStockData(string StockSymbol, Sources sources = Sources.AlphaVintage, TimeSeries timeSeries = TimeSeries.Daily, string startDay = "", string endDay = "", bool apiUp=false)
        {
            var dbData=_dbContext.Stocks.Where(s=>s.Name.Equals(StockSymbol));
            var stockModel = dbData.Select(s => s.ConvertToStockModel()) as IEnumerable<StockModel>;

            if(apiUp==true)
            {
                _dbContext.Stocks.RemoveRange(dbData);

                stockModel = _servicesResolver.GetService(sources).Action(StockSymbol, timeSeries);
                var stockListToSave = stockModel.Select(s => s.ConvertToStockDto().WithName(StockSymbol)).ToList();
                _dbContext.AddRange(stockListToSave);
                _dbContext.SaveChanges();
            }


            if (startDay != "")
                stockModel = stockModel.Where(s => s.Date > DateTime.Parse(startDay));

            if (endDay != "")
                stockModel = stockModel.Where(s => s.Date < DateTime.Parse(endDay));

            StockData = stockModel.ToList();

            ProcessedStockDataModel.xAxis = StockData.Select(s => s.Date).ToList();
            ProcessedStockDataModel.yValues = StockData.Select(s => Convert.ToDouble(s.Open)).ToList();

            return this;
        }

        public IStockActions AddIndicator(string indicator, params int[] param )
        {
            string paramMod = "_";
            if (param.Length > 0)
                paramMod = param[0].ToString();

            ProcessedStockDataModel.IndicatorsList.Add(indicator + paramMod, StockData.GetIndicator(indicator, param));
            return this;
        }

        public ProcessedStockDataModel ReturnApiData()
        {
            return ProcessedStockDataModel;
        }

        //public static IGetStockData Instance(IRequests requestSource) => new StockAPI(requestSource);
        public static IGetStockData Instance(IServicesResolver servicesResolver, StockDbContext stockDbContext) 
            => new StockAPI(servicesResolver,stockDbContext);
        public StockAPI(IServicesResolver servicesResolver,StockDbContext stockDbContext)
        {
            _servicesResolver = servicesResolver;
            _dbContext = stockDbContext;
        }

        private IServicesResolver _servicesResolver;
        private StockDbContext _dbContext;
    }
}
