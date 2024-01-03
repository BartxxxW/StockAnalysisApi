using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiChecker
{
    //EE0ACBEPU2WY3O7R
    public class AskService
    {
        public void TestServiceApi()
        {
            string baseUrl = "http://localhost:8004/";

            // Combine URL and port
            string apiUrl = $"api/StockValues";

            // Create a RestSharp client
            RestClient client = new RestClient(baseUrl);

            // Create a request with the desired method (GET in this case)
            RestRequest request = new RestRequest(apiUrl,Method.Get);

            // Execute the request and get the response
            var response = client.Execute(request);
        }
    }
}
