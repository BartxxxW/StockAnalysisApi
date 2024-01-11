// See https://aka.ms/new-console-template for more information
using ApiChecker;
using ApiChecker.PresentationLayer;
using ApiChecker.RequestStockData;

Console.WriteLine("Hello, World!");

//var ask = new AskService();

//ask.TestServiceApi();

var aV = new AlphaVintagrPoc();

aV.Check();

var a = new RequestAV();

var datamodel=StockAPI.Instance(a)
    .GetStockData("QQQ")
    .ReturnApiData();


// plot first chsrt in that way
double[] xs = datamodel.xAxis.Select(tds => tds.ToOADate()).ToArray();
double[] ys = datamodel.yValues.ToArray();

DataPlotter.Instance(xs, ys).Plot();

Console.WriteLine("THE END");