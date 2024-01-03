using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiChecker.JsonModels
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);




    public class DateStock
    {
        [JsonProperty("1. open")]
        public string _1Open { get; set; }

        [JsonProperty("2. high")]
        public string _2High { get; set; }

        [JsonProperty("3. low")]
        public string _3Low { get; set; }

        [JsonProperty("4. close")]
        public string _4Close { get; set; }

        [JsonProperty("5. volume")]
        public string _5Volume { get; set; }
    }

    public class MetaData
    {
        [JsonProperty("1. Information")]
        public string _1Information { get; set; }

        [JsonProperty("2. Symbol")]
        public string _2Symbol { get; set; }

        [JsonProperty("3. Last Refreshed")]
        public string _3LastRefreshed { get; set; }

        [JsonProperty("4. Output Size")]
        public string _4OutputSize { get; set; }

        [JsonProperty("5. Time Zone")]
        public string _5TimeZone { get; set; }
    }

    public class Root
    {
        [JsonProperty("Meta Data")]
        public MetaData MetaData { get; set; }

        [JsonProperty("Time Series (Daily)")]
        public Dictionary<string, DateStock> TimeSeriesDaily { get; set; }
    }

    public class TimeSeriesDaily
    {
        [JsonProperty]
        public List<DateStock> TimeSeriesDailyStock { get; set; }

    }


}
