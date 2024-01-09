// See https://aka.ms/new-console-template for more information
using ApiChecker;
using ApiChecker.RequestStockData;

Console.WriteLine("Hello, World!");

//var ask = new AskService();

//ask.TestServiceApi();

var aV = new AlphaVintagrPoc();

aV.Check();

var a = new RequestAV();

StockAPI.Instance(a)
    .GetStockData("QQQ")
    .ReturnApiData();
