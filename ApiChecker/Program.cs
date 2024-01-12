// See https://aka.ms/new-console-template for more information
using ApiChecker;
using ApiChecker.PresentationLayer;
using ApiChecker.RequestStockData;
using ApiChecker.Services;
using Autofac;

Console.WriteLine("Hello, World!");

//var ask = new AskService();

//ask.TestServiceApi();

var aV = new AlphaVintagrPoc();

aV.Check();

var builder = new ContainerBuilder();

builder.RegisterType<RequestAV>().As<IRequests>();
builder.RegisterType<AlphaVintageService>().As<IAlphaVintageService>();
builder.RegisterType<ServicesResolver>().As<IServicesResolver>();
builder.RegisterType<StockAPI>().As<IStockAPI>();

var conteiner = builder.Build();

var stockApi = conteiner.Resolve<IStockAPI>();

var datamodel = stockApi.GetStockData("QQQ").ReturnApiData();

//var datamodel=StockAPI.Instance()
//    .GetStockData("QQQ")
//    .ReturnApiData();


// plot first chsrt in that way
double[] xs = datamodel.xAxis.Select(tds => tds.ToOADate()).ToArray();
double[] ys = datamodel.yValues.ToArray();

DataPlotter.Instance(xs, ys).Plot();

Console.WriteLine("THE END");