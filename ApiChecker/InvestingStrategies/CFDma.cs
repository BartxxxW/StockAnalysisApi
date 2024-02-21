using ApiChecker.Extensions;
using ApiChecker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiChecker.InvestingStrategies
{

    public enum TradeSubject
    {
        LongCFD,
        ShortCFD,
        Stock
    }
    public class TokenList: List<KeyValuePair<double,Token>>
    {
        private void AddCFD(double quantity, string stockName, DateTime openDate, double openPrice, TradeSubject ts)
        {
            var token = new Token();
            token.Subject = ts;
            token.StockName = stockName;
            token.OpenDate = openDate;
            token.OpenPrice = openPrice;
            Add(new KeyValuePair<double, Token>(quantity, token));
        }
        public TokenList AddLongPosition(double quantity,string stockName,DateTime openDate, double openPrice)
        {
            AddCFD(quantity, stockName, openDate, openPrice, TradeSubject.LongCFD);
            return this;
        }
        public TokenList AddShortPosition(double quantity, string stockName, DateTime openDate, double openPrice)
        {
            AddCFD(quantity, stockName, openDate, openPrice, TradeSubject.ShortCFD);
            return this;
        }
        public TokenList AddStock(double quantity, string stockName, DateTime openDate, double openPrice)
        {
            AddCFD(quantity, stockName, openDate, openPrice, TradeSubject.Stock);
            return this;
        }
        public TokenList AddClosedRange(TokenList listToClose)
        {
            AddRange(listToClose);
            return this;
        }
    }
    public class Token
    {
        public TradeSubject Subject {  get; set; }
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

    public interface ITimeLine
    {
        DateTime StartDay { get; set; }
        DateTime Today { get; set; }

        void MoveNext();
    }

    public class TimeLine : ITimeLine
    {
        public DateTime StartDay { get; set; }
        public DateTime Today { get; set; }
        public TimeLine(string startDay)
        {
            StartDay = DateTime.Parse(startDay);
            Today = StartDay;

        }
        public void MoveNext()
        {
            Today = Today.AddDays(1);
        }
    }
    public class Account
    {
        public ITimeLine TimeLine { get; set; }
        public IStrategy Strategy { get; set; }
        public Account(ITimeLine timeLine, IStrategy strategy)
        {
            TimeLine = timeLine;
            Strategy = strategy;
        }

        public double PaidInMoneyHistory = 0;
        public double MainAccount= 0;
        public double ReserveAccount= 0;
        public double VirtualAccountBalance= 0;
        public  List<Operation> History = new List<Operation>();
        public TokenList LongPositions = new TokenList() ;    
        public TokenList ShortPositions = new TokenList(); 
        public TokenList Stocks = new TokenList(); 
        public TokenList ClosedTokens = new TokenList(); 

        public void WithdrawMoney(double amount,DateTime date)
        {
            if (amount < 0)
                throw new ArgumentException("wrong amount of money");

            if(MainAccount<0 || MainAccount<amount)
            {
                throw new Exception("No Money");
            }

            MainAccount -= amount;
            History.Add(new Operation(OperationType.Withdrawal, date, -amount));

        }
        public void PayInMoney(double amount, DateTime date)
        {
            if (amount < 0)
                throw new ArgumentException("wrong amount of money");


            MainAccount += amount;
            History.Add(new Operation(OperationType.Payment, date, amount));

        }
        // event to calculate virtual balance is it exceeded? for date change
        public void OpenLongPosition(double amount, double stockPrice, string StockName)
        {
            if (amount < stockPrice)
                throw new ArgumentException("not enaugh money to buy");

            MainAccount -= amount;
            double tokensQuantity = stockPrice / amount;
            LongPositions.AddLongPosition(tokensQuantity, StockName,TimeLine.Today,stockPrice);
            History.Add(new Operation(OperationType.OpenLong, TimeLine.Today, -amount));

        }
        public void OpenShortPosition(double amount, double stockPrice , string StockName)
        {

            if (amount < stockPrice)
                throw new ArgumentException("not enaugh money to buy");

            MainAccount -= amount;
            double tokensQuantity = stockPrice / amount;
            LongPositions.AddShortPosition(tokensQuantity, StockName, TimeLine.Today, stockPrice);
            History.Add(new Operation(OperationType.OpenShort, TimeLine.Today, -amount));
        }
        public void CloseAllLongPositions( double stockPrice)
        {
            var closedTokenTempList = LongPositions.CastToClosedToken(TimeLine.Today, stockPrice);
            var amount=closedTokenTempList.CalculateEndValue();
            MainAccount += amount;
            ClosedTokens.AddClosedRange(closedTokenTempList);
            LongPositions.Clear();
            History.Add(new Operation(OperationType.CloseLong, TimeLine.Today, amount));
        }
        public void CloseAllShortPositions(double stockPrice)
        {
            var closedTokenTempList = ShortPositions.CastToClosedToken(TimeLine.Today, stockPrice);
            var amount = closedTokenTempList.CalculateEndValue();
            MainAccount += amount;
            ClosedTokens.AddClosedRange(closedTokenTempList);
            ShortPositions.Clear();
            History.Add(new Operation(OperationType.CloseShort, TimeLine.Today, amount));
        }
        public void BuyStock(double amount, double stockPrice, string StockName)
        {
            if (amount < stockPrice)
                throw new ArgumentException("not enaugh money to buy");

            double tokensQuantity = stockPrice / amount;
            LongPositions.AddStock(tokensQuantity, StockName, TimeLine.Today, stockPrice);
        }
        public void SellAllStocks(double stockPrice)
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
