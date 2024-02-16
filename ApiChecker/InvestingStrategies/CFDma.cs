using ApiChecker.Extensions;
using ApiChecker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiChecker.InvestingStrategies
{
    public class Token
    {
        public string StockName {get; set;}
        public DateTime OpenDate { get; set; }
        public DateTime CloseDate { get; set; }
        public double OpenPrice { get; set; }
        public double ClosePrice { get; set; }
    }
    public enum OperationType
    {
        Payment,
        Withdrawal,
        Buy,
        Sell,
        OpenLong,
        CloseLong,
        OpenShort,
        CloseShort

    }
    public class Operation
    {
        public OperationType OperationType { get; set; }
        public DateTime Date { get; set; }
        public double Balance { get;set; }

    }
    public class Account
    {
        public DateTime Calendar { get; set; }
        public double PaidInMoneyHistory = 0;
        public double MainAccount= 0;
        public double ReserveAccount= 0;
        public double VirtualAccountBalance= 0;
        public  List<Operation> History { get; set; }
        public List<Token> LongPositions { get; set; } 
        public List<Token> ShortPositions { get; set; } 
        public List<Token> Stocks { get; set; } 

        public void WithdrawMoney(double amount)
        {
            if (amount < 0)
                throw new ArgumentException("wrong amount of money");

            if(MainAccount<0 || MainAccount<amount)
            {
                throw new Exception("No Money");
            }

            MainAccount -= amount;
            History.Add(new Ope)

        }
    }
    public class CFDma:StratedyBase,IStrategy
    {
        public double Simulate(string baseIndicator, string Indicator, ProcessedStockDataModel dataModel, string startDate, string endDate, double startMoneyUSD, double intervalMoneyUSD, int intervalMonths)
        {

            i7 = dataModel.GetIndicatorWithDatesFromDataModel(baseIndicator);
            i180 = dataModel.GetIndicatorWithDatesFromDataModel(Indicator);
            filteredStockPrices = dataModel.StockPrices.GetStockRangeByDate(startDate, endDate);

            base.EndDate = filteredStockPrices.Last().Key.Date;
            base.StartDate = DateTime.Parse(startDate);

            GetDatesToBuy(startDate, EndDate.ToString(), intervalMonths);


            var DayInLoop = StartDate;

            while (DayInLoop <= EndDate)
            {
                if (datesToBuy.Contains(DayInLoop))
                {
                    AddMoneyToInvest(DayInLoop, intervalMoneyUSD);
                }

                //CheckMAstatus()
                //CheckRSIstatus()



            }

            return 0;
        }
    }
}
