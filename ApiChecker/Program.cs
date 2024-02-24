// See https://aka.ms/new-console-template for more information
using ApiChecker;
using ApiChecker.DataProcessing;
using ApiChecker.Entities;
using ApiChecker.InvestingStrategies;
using ApiChecker.PresentationLayer;
using ApiChecker.RequestStockData;
using ApiChecker.Services;
using Autofac;
using Microsoft.EntityFrameworkCore;
using System.Data.Entity;
using System.Drawing;

Console.WriteLine("Check bonds Calculation");

var bonds = new Bonds();

bonds.Simulate(7, 4, 1000, 1000, 5);

Console.WriteLine("Hello, World!");

//var ask = new AskService();

//ask.TestServiceApi();

//var aV = new AlphaVintagrPoc();

//aV.Check();

var builder = new ContainerBuilder();

builder.Register(c => new DbContextOptionsBuilder<StockDbContext>()
                    .UseSqlServer("Server=BARTEK;Database=Stocks;Integrated Security=True;TrustServerCertificate=True;")
                    .Options)
                .SingleInstance();
builder.RegisterType<StockDbContext>()
               .AsSelf()
               .InstancePerLifetimeScope();
builder.RegisterType<RequestAV>().As<IRequests>();
builder.RegisterType<AlphaVintageService>().As<IAlphaVintageService>();
builder.RegisterType<ServicesResolver>().As<IServicesResolver>();
builder.RegisterType<StockAPI>().As<IStockAPI>();

var conteiner = builder.Build();


var stockApi = conteiner.Resolve<IStockAPI>();
var dbContext = conteiner.Resolve<StockDbContext>();
dbContext.Database.EnsureCreated();

//var datamodel = stockApi.GetStockData("QQQ").ReturnApiData(); //SPY etc
// for now just modificator bool update
var datamodel = stockApi.GetStockData("SPY",startDay:"1997-04-04")
    //.AddIndicator(Indicators.EMA,7)
    //.AddIndicator(Indicators.SMA,50)
    //.AddIndicator(Indicators.EMA,180)
    //.AddIndicator(Indicators.SMA,200)
    .AddIndicator(Indicators.EMA,50)
    .AddIndicator(Indicators.EMA,200)
    .AddIndicator(Indicators.SLOPE,30)
//    .AddIndicator(Indicators.RSI_SLOPE)
    //.AddIndicator(Indicators.OBV)
    //.AddIndicator(Indicators.OBV_RSI)
    //.AddIndicator(Indicators.MACD,12,26,9)
    //.AddIndicator(Indicators.MACD_SIGNAL)
    .AddIndicator(Indicators.RSI, 14)
    .ReturnApiData();

//var datamodel=StockAPI.Instance()
//    .GetStockData("QQQ")
//    .ReturnApiData();


var wb = new WarrenBuffet();



wb.Simulate("15-01-2007", "15-01-2012", 1000, 1000, 4, datamodel.StockPrices);


var bos=new BuffetOnSteroids();

bos.Simulate("EMA50","EMA200",datamodel, "15-01-2007", "15-01-2012", 1000, 1000, 4);

var maStrategy = new MA();

maStrategy.Simulate("EMA50", "EMA200", datamodel, "15-01-2007", "15-01-2012", 1000, 1000, 4);

var CFDmaStrategy = new CFDma();

CFDmaStrategy.Simulate("SPY", 20, 0.0259, "EMA50", "EMA200", datamodel, "15-01-2007", "15-01-2012", 1000, 1000, 4);


double[] xs = datamodel.xAxis.Select(tds => tds.ToOADate()).ToArray();
double[] ys = datamodel.yValues.ToArray();


var plotter=DataPlotter.Instance(xs, ys);

datamodel.IndicatorsList.ToList().ForEach(indicator=>plotter.AddScatter(indicator.Value.ToArray(),indicator.Key));

//double[] newxS = { xs[0], xs.Last() };
//double[] ysNew = { 50, 250 };

//plotter.Plt.AddScatter(newxS, ysNew);
//plotter.Plt.AddScatterLines(newxS, ysNew);

double[] sellDates = CFDmaStrategy.SellDates.Select(tds => tds.ToOADate()).ToArray();
double[] buyDates = CFDmaStrategy.BuyDates.Select(tds => tds.ToOADate()).ToArray();

//double[] sellDates = maStrategy.SellDates.Select(tds => tds.ToOADate()).ToArray();
//double[] buyDates = maStrategy.BuyDates.Select(tds => tds.ToOADate()).ToArray();

sellDates.ToList().ForEach(d=>plotter.Plt.AddVerticalLine(d,color:Color.Red));
buyDates.ToList().ForEach(d=>plotter.Plt.AddVerticalLine(d, color: Color.Green));

plotter.Plot();
////---

//var stockApiOBV = conteiner.Resolve<IStockAPI>();

//var datamodelOBV = stockApiOBV.GetStockData("SPY", startDay: "1997-04-04")
//        .AddIndicator(Indicators.OBV)
//    //.AddIndicator(Indicators.OBV_RSI)
//    .ReturnApiData();

//var plotterOBV = DataPlotter.Instance(xs, datamodelOBV.IndicatorsList.ToList()[0].Value.ToArray());
////datamodelOBV.IndicatorsList.ToList().ForEach(indicator => plotter.AddScatter(indicator.Value.ToArray(), indicator.Key));
//plotterOBV.Plot("OBVchart");
//--

Console.WriteLine("THE END");