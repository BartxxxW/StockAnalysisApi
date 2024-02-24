using ApiChecker.Extensions;
using ApiChecker.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
        private void AddToken(double quantity, string stockName, DateTime openDate, double openPrice, TradeSubject ts, double levar = 1)
        {
            var token = new Token();
            token.Subject = ts;
            token.StockName = stockName;
            token.OpenDate = openDate;
            token.OpenPrice = openPrice;
            token.Levar = levar;
            Add(new KeyValuePair<double, Token>(quantity, token));
        }
        public TokenList AddLongPosition(double quantity,string stockName,DateTime openDate, double openPrice,double levar)
        {
            AddToken(quantity, stockName, openDate, openPrice, TradeSubject.LongCFD,levar);
            return this;
        }
        public TokenList AddShortPosition(double quantity, string stockName, DateTime openDate, double openPrice, double levar)
        {
            AddToken(quantity, stockName, openDate, openPrice, TradeSubject.ShortCFD,levar);
            return this;
        }
        public TokenList AddStock(double quantity, string stockName, DateTime openDate, double openPrice)
        {
            AddToken(quantity, stockName, openDate, openPrice, TradeSubject.Stock);
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
        public double Levar { get; set; } = 1;
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
        public Account(ITimeLine timeLine, IStrategy strategy=null)
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
        public List<KeyValuePair<double, DateTime>> ClosedTokensBilans = new List<KeyValuePair<double, DateTime>>();
        //public ContractList OpenContracts = new ContractList();

        //public class CfdContract
        //{

        //}
        //public class ContractList:List<CfdContract>
        //{

        //}
        public void MoveToReserveAccount(double amount)
        {
            if (amount > MainAccount)
                return;
            MainAccount-=amount;
            ReserveAccount += amount;
        }
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
        public void PayForSth(double amount, DateTime date)
        {
            WithdrawMoney(amount, date);
        }
        public void PayInMoney(double amount)
        {
            if (amount < 0)
                throw new ArgumentException("wrong amount of money");


            MainAccount += amount;
            PaidInMoneyHistory += amount;
            History.Add(new Operation(OperationType.Payment, TimeLine.Today, amount));

        }
        // event to calculate virtual balance is it exceeded? for date change
        public void OpenLongPosition(double amount, double stockPrice, string StockName, double levar)
        {
            if (amount < stockPrice)
                throw new ArgumentException("not enaugh money to buy");

            MainAccount -= amount;
            double tokensQuantity = (amount * levar) / stockPrice;
            LongPositions.AddLongPosition(tokensQuantity, StockName,TimeLine.Today,stockPrice, levar);
            History.Add(new Operation(OperationType.OpenLong, TimeLine.Today, -amount));

        }
        public void OpenShortPosition(double amount, double stockPrice , string StockName,double levar)
        {

            if (amount < stockPrice)
                throw new ArgumentException("not enaugh money to buy");

            MainAccount -= amount;
            double tokensQuantity = (amount*levar)/stockPrice ;
            ShortPositions.AddShortPosition(tokensQuantity, StockName, TimeLine.Today, stockPrice, levar);
            History.Add(new Operation(OperationType.OpenShort, TimeLine.Today, -amount));
        }
        public void CloseAllLongPositions( double stockPrice)
        {
            var closedTokenTempList = LongPositions.CastToClosedToken(TimeLine.Today, stockPrice);
            double beforeLevar=closedTokenTempList.CalculateBeforeLevar();
            double startValue=closedTokenTempList.CalculateStartValue();
            double endValue=closedTokenTempList.CalculateEndValue();
            double diff = endValue- startValue;

            double amount = beforeLevar + diff;// longs => when raised => there is gain
            MainAccount += amount;
            ClosedTokens.AddClosedRange(closedTokenTempList);
            LongPositions.Clear();
            History.Add(new Operation(OperationType.CloseLong, TimeLine.Today, amount));
            ClosedTokensBilans.Add(new KeyValuePair<double, DateTime>(diff, TimeLine.Today));
        }
        public void CloseAllShortPositions(double stockPrice)
        {
            //bug
            var closedTokenTempList = ShortPositions.CastToClosedToken(TimeLine.Today, stockPrice);
            double beforeLevar = closedTokenTempList.CalculateBeforeLevar();
            double startValue = closedTokenTempList.CalculateStartValue();
            double endValue = closedTokenTempList.CalculateEndValue();
            double diff = endValue - startValue;

            double amount = beforeLevar - diff; // shorts => when decreased => there is gain
            MainAccount += amount;
            ClosedTokens.AddClosedRange(closedTokenTempList);
            ShortPositions.Clear();
            History.Add(new Operation(OperationType.CloseShort, TimeLine.Today, amount));
            ClosedTokensBilans.Add(new KeyValuePair<double, DateTime>(diff, TimeLine.Today));
        }
        public void BuyStock(double amount, double stockPrice, string StockName)
        {
            if (amount < stockPrice)
                throw new ArgumentException("not enaugh money to buy");

            double tokensQuantity = amount/stockPrice;
            Stocks.AddStock(tokensQuantity, StockName, TimeLine.Today, stockPrice);
        }
        public void SellAllStocks(double stockPrice)
        {
            var closedTokenTempList = Stocks.CastToClosedToken(TimeLine.Today, stockPrice);
            var amount = closedTokenTempList.CalculateEndValue();
            // calculate start value
            //calculate diff and add to diff

            MainAccount += amount;
            ClosedTokens.AddClosedRange(closedTokenTempList);
            Stocks.Clear();
            History.Add(new Operation(OperationType.Sell, TimeLine.Today, amount));
            //develop all actions
            //money changes
        }


    }
    public class CFDma:StratedyBase,IStrategy
    {
        public Account Account { get; set; }
        public TimeLine TimeLine { get; set; }
        public string  StockName { get; set; }
        public double  Lever { get; set; }
        public void TakeActionCFD(StockAction action, DateTime investDay)
        {
            double stockPriceNow= filteredStockPrices.GetStockValue(investDay);

            if (action == StockAction.Buy)// will raise
            {
                Account.CloseAllShortPositions(stockPriceNow);
                Account.OpenLongPosition(0.8*Account.MainAccount, stockPriceNow, StockName, Lever);
            }

            if (action == StockAction.Sell) // will drop down
            {
                Account.CloseAllLongPositions(stockPriceNow);
                Account.OpenShortPosition(0.8*Account.MainAccount, stockPriceNow, StockName, Lever);
            }
        }

        public double Simulate(string stockName,double lever, double percentSwapLong, double percentSwapShort, string baseIndicator, string Indicator, ProcessedStockDataModel dataModel, string startDate, string endDate, double startMoneyUSD, double intervalMoneyUSD, int intervalMonths)
        {
            StockName=stockName;
            Lever=lever;

            TimeLine = new TimeLine(startDate);
            Account = new Account(TimeLine);

            i7 = dataModel.GetIndicatorWithDatesFromDataModel(baseIndicator);
            i180 = dataModel.GetIndicatorWithDatesFromDataModel(Indicator);

            filteredStockPrices = dataModel.StockPrices.GetStockRangeByDate(startDate, endDate);

            base.StartDate = DateTime.Parse(startDate);
            base.EndDate = filteredStockPrices.Last().Key.Date;

            GetDatesToBuy(startDate, EndDate.ToString(), intervalMonths,true);


            while (TimeLine.Today <= EndDate)
            {
                if (datesToBuy.Contains(TimeLine.Today) && TimeLine.Today != StartDate)
                {
                    Account.PayInMoney(intervalMoneyUSD);
                }
                if (datesToBuy.Contains(TimeLine.Today) && TimeLine.Today == StartDate)
                {
                    Account.PayInMoney(startMoneyUSD);
                }


                StockActions.Add(CheckAction(TimeLine.Today));

                TakeActionCFD(Last3Days(), TimeLine.Today);


                TimeLine.MoveNext();

                Account.PayForSth(Account.LongPositions.PayForSwap(percentSwapLong, TimeLine.Today),TimeLine.Today);
            }

            //Pay for taxes


            




            return 0;
        }
    }
}
