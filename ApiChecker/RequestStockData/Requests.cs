using ApiChecker.StaticToolBox;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiChecker.RequestStockData
{
    public class Requests
    {

        public RestResponse TimeSeriesDaily(string stockSymbol)
        {
            //to dependency injection
            RestClient client = new RestClient(Statics.BaseURL);
            RestRequest request = new RestRequest();

            request.AddParameter("function", "TIME_SERIES_DAILY");
            request.AddParameter("symbol", stockSymbol);
            request.AddParameter("apikey", Statics.ApiKey);
            request.AddParameter("outputsize", "full");
            request.AddParameter("datatype", "json");

            return client.Execute(request);
        }
    }
}
