using ApiChecker.RequestStockData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiChecker.Services
{
    public interface IServicesResolver
    {
        IService GetService(Sources source);
    }

    public class ServicesResolver : IServicesResolver
    {
        private IService _alphaVintageService;
        public ServicesResolver(IAlphaVintageService alphaVintageService)
        {
            _alphaVintageService = (IService)alphaVintageService;
            ServicesMap.Add(Sources.AlphaVintage, _alphaVintageService);
        }
        private Dictionary<Sources, IService> ServicesMap = new Dictionary<Sources, IService>();
        public IService GetService(Sources source) => ServicesMap[source];
    }
}
