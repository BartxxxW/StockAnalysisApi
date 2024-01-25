﻿// See https://aka.ms/new-console-template for more information
using ApiChecker;
using ApiChecker.DataProcessing;
using ApiChecker.InvestingStrategies;
using ApiChecker.PresentationLayer;
using ApiChecker.RequestStockData;
using ApiChecker.Services;
using Autofac;

Console.WriteLine("Check bonds Calculation");

var bonds = new Bonds();

bonds.Simulate(7, 4, 1000, 1000, 7);

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

//var datamodel = stockApi.GetStockData("QQQ").ReturnApiData();
var datamodel = stockApi.GetStockData("QQQ",startDay:"2004-04-04")
    .AddIndicator(Indicators.EMA,7)
    .AddIndicator(Indicators.EMA,30)
    .AddIndicator(Indicators.EMA,90)
    .AddIndicator(Indicators.EMA,180)
    .AddIndicator(Indicators.MACD,12,26,9)
    .AddIndicator(Indicators.MACD_SIGNAL)
    .AddIndicator(Indicators.RSI).ReturnApiData();

//var datamodel=StockAPI.Instance()
//    .GetStockData("QQQ")
//    .ReturnApiData();


var wb= new WarrenBuffet();

// whe end resut =0? hhow many invested / how many end the end / why displayed all simulation with bonds?
wb.Simulate("15-01-2004", "15-01-2011", 1000, 1000,4, datamodel.StockPrices);
//var filtredStockPrices =  datamodel.StockPrices.OrderBy(k => k.Key).Where(k => (k.Key >= DateTime.Parse("01-01-2024") && k.Key <= DateTime.Parse("21-01-2024"))).ToList();

//Console.WriteLine($"start  position : {filtredStockPrices[0].Key} :{filtredStockPrices[0].Value} ");
//Console.WriteLine($"start  position : {filtredStockPrices.Last().Key} :{filtredStockPrices.Last().Value} ");
//Console.WriteLine("   ---------       ");
//filtredStockPrices.ForEach(i => Console.WriteLine($"start  position : {i.Key} :{i.Value} "));

// plot first chsrt in that way
double[] xs = datamodel.xAxis.Select(tds => tds.ToOADate()).ToArray();
double[] ys = datamodel.yValues.ToArray();

//DataPlotter.Instance(xs, ys).Plot();
var plotter=DataPlotter.Instance(xs, ys);

datamodel.IndicatorsList.ToList().ForEach(indicator=>plotter.AddScatter(indicator.Value.ToArray()));

plotter.Plot();

Console.WriteLine("THE END");