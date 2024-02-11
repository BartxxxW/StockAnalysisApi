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
    .AddIndicator(Indicators.EMA,45)
    //.AddIndicator(Indicators.EMA,23)
    //.AddIndicator(Indicators.EMA,30)
    .AddIndicator(Indicators.EMA,90)
    //.AddIndicator(Indicators.EMA,90)
    .AddIndicator(Indicators.EMA, 120)
    .AddIndicator(Indicators.SMA, 80)
    //.AddIndicator(Indicators.EMA,180)
    .AddIndicator(Indicators.MACD,12,26,9)
    .AddIndicator(Indicators.MACD_SIGNAL)
    .AddIndicator(Indicators.RSI,14).ReturnApiData();

//var datamodel=StockAPI.Instance()
//    .GetStockData("QQQ")
//    .ReturnApiData();


//var wb= new WarrenBuffet();



//wb.Simulate("15-01-2007", "15-01-2012", 1000, 1000,4, datamodel.StockPrices);


var bos=new BuffetOnSteroids();

bos.Simulate(datamodel, "15-01-2007", "15-01-2012", 1000, 1000, 4);


double[] xs = datamodel.xAxis.Select(tds => tds.ToOADate()).ToArray();
double[] ys = datamodel.yValues.ToArray();


var plotter=DataPlotter.Instance(xs, ys);

datamodel.IndicatorsList.ToList().ForEach(indicator=>plotter.AddScatter(indicator.Value.ToArray()));

double[] sellDates=bos.SellDates.Select(tds => tds.ToOADate()).ToArray();
double[] buyDates=bos.BuyDates.Select(tds => tds.ToOADate()).ToArray();

sellDates.ToList().ForEach(d=>plotter.Plt.AddVerticalLine(d,color:Color.Red));
buyDates.ToList().ForEach(d=>plotter.Plt.AddVerticalLine(d, color: Color.Green));

plotter.Plot();

Console.WriteLine("THE END");