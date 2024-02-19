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
        public Operation(OperationType operationType, DateTime date, double balance)
        {
            OperationType = operationType;
            Date = date;
            Balance = balance;
        }

        public OperationType OperationType { get; set; }
        public DateTime Date { get; set; }
        public double Balance { get;set; }

    }
    public class Calendar
    {
        public DateTime StartDay { get; set; }
        public DateTime Today { get; set; }
        public Calendar(string startDay)
        {
            StartDay=DateTime.Parse(startDay);
            Today= StartDay;

        }
        public void MoveNext()
        {
            Today = Today.AddDays(1);
        }
    }
    public class Account
    {
        public Account(Calendar calendar)
        {

            //should account know sth about calendar ?
            Calendar=calendar;
        }

        public Calendar Calendar { get; set; }
        public double PaidInMoneyHistory = 0;
        public double MainAccount= 0;
        public double ReserveAccount= 0;
        public double VirtualAccountBalance= 0;
        public  List<Operation> History = new List<Operation>();
        public List<Token> LongPositions = new List<Token>();    
        public List<Token> ShortPositions = new List<Token>(); 
        public List<Token> Stocks = new List<Token>(); 

        public void WithdrawMoney(double amount)
        {
            if (amount < 0)
                throw new ArgumentException("wrong amount of money");

            if(MainAccount<0 || MainAccount<amount)
            {
                throw new Exception("No Money");
            }

            MainAccount -= amount;
            History.Add(new Operation(OperationType.Withdrawal, Calendar.Today, amount));

        }
        public void PayInMoney(double amount)
        {
            if (amount < 0)
                throw new ArgumentException("wrong amount of money");


            MainAccount += amount;
            History.Add(new Operation(OperationType.Payment, Calendar.Today, amount));

        }
        // event to calculate virtual balance is it exceeded? for date change
        public void OpenLongPosition(double amount, double stockPrice)
        {

        }
        public void OpenShortPosition(double amount, double stockPrice)
        {

        }
        public void CloseLongPosition( double stockPrice)
        {

        }
        public void CloseShortPosition(double stockPrice)
        {

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
