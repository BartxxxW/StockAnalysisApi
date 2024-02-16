using ApiChecker.Extensions;
using ApiChecker.Models;
using ApiChecker.ToolBox;
using Newtonsoft.Json.Linq;
using ScottPlot.Renderable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using static ApiChecker.Extensions.MathExtensions;

namespace ApiChecker.InvestingStrategies
{
    public class MA :StratedyBase, IStrategy
    {

        public double Simulate(string baseIndicator,string Indicator, ProcessedStockDataModel dataModel,string startDate, string endDate,double startMoneyUSD, double intervalMoneyUSD, int intervalMonths,  bool taxIncluded = false)
        {

            double result = 0;
            
            moneyToInvest += startMoneyUSD;
            investedMoney += startMoneyUSD;

            i7 = dataModel.GetIndicatorWithDatesFromDataModel(baseIndicator);
            i180 = dataModel.GetIndicatorWithDatesFromDataModel(Indicator);
            intervalSavings = intervalMoneyUSD;
            


            filteredStockPrices = dataModel.StockPrices.GetStockRangeByDate(startDate, endDate);

            var dt_EndDate = filteredStockPrices.Last().Key.Date;
            GetDatesToBuy(startDate, dt_EndDate.ToString(), intervalMonths);

            var DayInLoop = DateTime.Parse(startDate);

            while(DayInLoop<= dt_EndDate)
            {
                if(datesToBuy.Contains(DayInLoop))
                {
                    AddMoneyToInvest(DayInLoop, intervalMoneyUSD);
                    testIterator++;
                }

                StockActions.Add(CheckAction(DayInLoop));

                TakeAction(Last3Days(), DayInLoop);


                DayInLoop=DayInLoop.AddDays(1);

            }

            Sell(dt_EndDate);
            Console.WriteLine("iterations:"+ testIterator);
            double paidInMoney = startMoneyUSD + datesToBuy.Count * intervalMoneyUSD;
            double resultWithoutTaxes = moneyToInvest;

            double resultAfterTaxes = resultWithoutTaxes - PayTaxes();

            Console.WriteLine($"MA=> PaidIn:{paidInMoney} ; result without taxes: {resultWithoutTaxes} ; afterTAxes!: {resultAfterTaxes} ; afterTAinvestedmoneyxes!: {investedMoney}");


            return result;
        }
    }
}
