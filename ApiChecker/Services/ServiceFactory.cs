using ApiChecker.RequestStockData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiChecker.Services
{
    public static class ServiceFactory
    {
        public static Dictionary<Sources,string> ServicesMap= new Dictionary<Sources, string> 
        {
            {Sources.AlphaVintage,"ApiChecker.Services.AlphaVintageService" }
        };

        public static IService Instance(Sources sources) => (IService)Activator.CreateInstance(Type.GetType(ServicesMap[sources]));
    }
}
