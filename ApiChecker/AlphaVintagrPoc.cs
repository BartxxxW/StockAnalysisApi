using ApiChecker.JsonModels;
using ApiChecker.SkendorStockModels;
using ApiChecker.ToolBox;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using ScottPlot.Plottable;
using Skender.Stock.Indicators;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ApiChecker
{
    public class AlphaVintagrPoc
    {
        public void Check()
        {
            // Replace "YOUR_API_KEY" with your actual Alpha Vantage API key
            string apiKey = "EE0ACBEPU2WY3O7R";

            // Specify the Alpha Vantage API endpoint for time series data
            string baseURL = $"https://www.alphavantage.co/query";

            // Specify the symbol for Tesla stock
            //string symbol = "ANXU.LON"; => best for now for nasdaq
            //string symbol = "QQQ"; ==> even better
            //https://scottplot.net/
            //https://swharden.com/csdv/plotting-free/microsoft-charting/
            string symbol = "QQQ";


            // Create a RestSharp client
            RestClient client = new RestClient(baseURL);

            // Create a request with the desired method (GET) and parameters
            RestRequest request = new RestRequest();
            request.AddParameter("function", "TIME_SERIES_DAILY");
            request.AddParameter("symbol", symbol);
            request.AddParameter("apikey", apiKey);
            request.AddParameter("outputsize", "full"); // Alternatively, use "compact" for a compact dataset
            request.AddParameter("datatype", "json"); // Specify the response format
            //request.AddParameter("interval", "1d"); // Specify the interval (daily)

            // If you want to include the start date and end date
            //request.AddParameter("start_date", startDate);
            //request.AddParameter("end_date", endDate);

            // Execute the request and get the response
            var response = client.Execute(request);


            //var parsedJSON= JObject.Parse(response.Content);
            //var data = JsonConvert.DeserializeObject<JObject>(response.Content);

            //// Accessing elements
            //var metaData = data["Meta Data"];
            //var timeSeries = data["Time Series (Daily)"];
            //var dates = timeSeries.Children().ToList().Select(c=>((JProperty)c).Name);
            //dates.ToList().ForEach(d => Console.WriteLine(d));

            //=> https://dotnet.stockindicators.dev/guide/#historical-quotes seems to be greate dictionary
            // !!!!!!!!!!!!!!!!!!!!!!!!!!
            // get rsi 
            // get MACAD
            // better chart
            // implement startegy
            // back testing for strategies
            // then linear regression back etsting and so on
            /// !!!!!!!!!!!!!

            //next way:
            // json2 Csharp  (Deserializr object)


            //PROCESS DATA => CHANGE TO KEY VALUE PAIR!!! TO be able to combine apropriate values  for  X axis
            var deserializedClass = JsonConvert.DeserializeObject<Root>(response.Content);
            //whwere 20 03-2000
            var dates= deserializedClass.TimeSeriesDaily.Where(i=> DateTime.Parse(i.Key)>DateTime.Parse("23 March 2000")).Select(tds => DateTime.Parse(tds.Key)).ToArray();
            var values = deserializedClass.TimeSeriesDaily.Where(i => DateTime.Parse(i.Key) > DateTime.Parse("23 March 2000")).Select(tds=>Convert.ToDouble(tds.Value._4Close.Replace('.',','))).ToArray();


            var smaPoints = deserializedClass.TimeSeriesDaily.Where(i => DateTime.Parse(i.Key) > DateTime.Parse("23 March 2000")).Select(tds => tds.ConvertToStockModel());

            var sma = smaPoints.GetSma(30).Reverse();
            var sma200 = smaPoints.GetSma(180).Reverse();
            var mcad = smaPoints.GetMacd(16).Reverse();
            //var smaRSI = smaPoints.GetRsi(60);

            //double[] rsiMax=smaRSI.Select(v => {
            //
            //    if (v.Rsi == null) { return 0; }
            //    return Convert.ToDouble(v.Rsi.Value);
            //
            //}).ToArray();

            double[] smaValues = sma.Select(v => {

                if (v.Sma == null) { return 0; }
                return Convert.ToDouble(v.Sma);
                
            }).ToArray();

            double[] smaValues200 = sma200.Select(v => {

                if (v.Sma == null) { return 0; }
                return Convert.ToDouble(v.Sma);

            }).ToArray();

            double[] xs = dates.Select(tds=>tds.ToOADate()).ToArray();
            double[] ys = values;
            var plt = new ScottPlot.Plot(6000, 2500);

            // Basic xs  AND  for range of xs 
            plt.AddScatter(xs, ys);
            plt.AddScatter(xs, smaValues);
            plt.AddScatter(xs, smaValues200);
            //plt.AddScatter(xs, rsiMax);


            plt.XAxis.DateTimeFormat(true);
        
            plt.SaveFig("quickstart.png");




            // select values
            //var dates = parsedJSON.Children()[1].Values();

            //var stockDates = parsedJSON.
            //deserialie json
            //plot chart
            //sma20/50/200 library
            //implement stategies
            // BACK TESTING
            // sum up and add web UI for just selected functionalities to this and push on azure
        }
    }
}
