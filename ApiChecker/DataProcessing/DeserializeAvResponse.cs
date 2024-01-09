using ApiChecker.JsonModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiChecker.DataProcessing
{
    public static class DeserializeAvResponse
    {

        public static RootTimeSeriesDaily? TimeSeriesDaily(string? responseContent)
        {
            if (responseContent == null) throw new ArgumentNullException(nameof(responseContent));

            return JsonConvert.DeserializeObject<RootTimeSeriesDaily>(responseContent);
        }
            
    }
}
