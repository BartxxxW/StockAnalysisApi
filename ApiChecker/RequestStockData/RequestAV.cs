using ApiChecker.StaticToolBox;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiChecker.RequestStockData
{
    public enum TimeSeries
    {
        IntraDay,
        Daily,
        Weekly,
        Monthly
    }
    public interface IRequests
    {
        RestResponse TimeSeriesDaily(string stockSymbol);
    }

    public class RequestAV : IRequests
    {
        public static RequestAV Instance = new RequestAV();

        private const string BaseUrl= $"https://www.alphavantage.co/query";
        private const string ApiKey= "EE0ACBEPU2WY3O7R";
        public RestResponse TimeSeriesDaily(string stockSymbol)
        {
            //to dependency injection
            RestClient client = new RestClient(BaseUrl);
            RestRequest request = new RestRequest();

            request.AddParameter("function", "TIME_SERIES_DAILY");
            request.AddParameter("symbol", stockSymbol);
            request.AddParameter("apikey", ApiKey);
            request.AddParameter("outputsize", "full");
            request.AddParameter("datatype", "json");

            return client.Execute(request);
        }
    }
}
