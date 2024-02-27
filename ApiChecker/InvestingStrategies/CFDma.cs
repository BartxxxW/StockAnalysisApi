using ApiChecker.Extensions;
using ApiChecker.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
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
        public Account(ITimeLine timeLine, IStrategy strategy = null)
        {
            TimeLine = timeLine;
            Strategy = strategy;
        }

        public double PaidInMoneyHistory = 0;
        public double MainAccount = 0;
        public double ReserveAccount = 0;
        public double VirtualAccountBalance = 0;
        public List<Operation> History = new List<Operation>();
        public TokenList LongPositions = new TokenList();
        public TokenList ShortPositions = new TokenList();
        public TokenList Stocks = new TokenList();
        public TokenList ClosedTokens = new TokenList();
        public List<KeyValuePair<double, DateTime>> ClosedTokensBilans = new List<KeyValuePair<double, DateTime>>();
        public double GetCfdBilansForNow(double stockPrice)
        {
            double bilans = 0;

            //Long Positions
            double amountLP = 0;
            if (LongPositions.Count > 0)
            {
                double beforeLevarLP = LongPositions.CalculateBeforeLevar();
                double startValueLP = LongPositions.CalculateStartValue();
                double endValueLP = LongPositions.CalculateEndValue(stockPrice);
                double diffLP = endValueLP - startValueLP;

                amountLP = beforeLevarLP + diffLP;

            }


            //Short Positions
            double amountSP = 0;
            if (ShortPositions.Count>0) 
            {
                double beforeLevarSP = ShortPositions.CalculateBeforeLevar();
                double startValueSP = ShortPositions.CalculateStartValue();
                double endValueSP = ShortPositions.CalculateEndValue(stockPrice);
                double diffSP = endValueSP - startValueSP;

                amountSP = beforeLevarSP - diffSP;
            }



            bilans = amountLP + amountSP + MainAccount + ReserveAccount;

            return bilans;
        } // bilans to develop
          //public ContractList OpenContracts = new ContractList();

        //public class CfdContract
        //{

        //}
        //public class ContractList:List<CfdContract>
        //{

        //}
        public void BackToMainAccount()
        {
            MainAccount += ReserveAccount;
            ReserveAccount = 0;
        }
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

        public void PayWithReserveAccount(double amount, bool withDebt=false)
        {
            if (amount < 0)
                throw new ArgumentException("wrong amount of money");

            if ((ReserveAccount < 0 || ReserveAccount < amount) && withDebt==false)
            {
                throw new Exception("No Money");
            }

            ReserveAccount -= amount;
            History.Add(new Operation(OperationType.Withdrawal, TimeLine.Today, -amount));

        }
        public void PayForSth(double amount, DateTime? date=null)
        {
            DateTime date_ = TimeLine.Today;
            if (date != null)
                date_ = date.Value;

            WithdrawMoney(amount, date_);
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
            if (LongPositions.Count == 0)
                return;

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
            if (ShortPositions.Count == 0)
                return;

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
            ClosedTokensBilans.Add(new KeyValuePair<double, DateTime>(-diff, TimeLine.Today));
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
            if (Stocks.Count == 0)
                return;

            var closedTokenTempList = Stocks.CastToClosedToken(TimeLine.Today, stockPrice);
            var amount = closedTokenTempList.CalculateEndValue();
            double startValue = closedTokenTempList.CalculateStartValue();
            double diff = amount - startValue;

            MainAccount += amount;
            ClosedTokens.AddClosedRange(closedTokenTempList);
            Stocks.Clear();
            History.Add(new Operation(OperationType.Sell, TimeLine.Today, amount));
            ClosedTokensBilans.Add(new KeyValuePair<double, DateTime>(diff, TimeLine.Today));

        }


    }
    public class CFDma:StratedyBase,IStrategy
    {
        public Account Account { get; set; }
        public TimeLine TimeLine { get; set; }
        public string  StockName { get; set; }
        public double  Lever { get; set; }

        public double StockPriceNow=> filteredStockPrices.GetStockValue(TimeLine.Today);
        public void TakeActionCFD(StockAction action)
        {
            if (action == StockAction.Buy)// will raise
            {
                Account.BackToMainAccount();
                Account.CloseAllShortPositions(StockPriceNow);
                Account.OpenLongPosition(0.6*Account.MainAccount, StockPriceNow, StockName, Lever);
                Account.MoveToReserveAccount(Account.MainAccount);
                BuyDates.Add(TimeLine.Today);
            }

            if (action == StockAction.Sell) // will drop down
            {
                Account.BackToMainAccount();
                Account.CloseAllLongPositions(StockPriceNow);
                Account.OpenShortPosition(0.6*Account.MainAccount, StockPriceNow, StockName, Lever);
                Account.MoveToReserveAccount(Account.MainAccount);
                SellDates.Add(TimeLine.Today);
            }
        }

        public double Simulate(string stockName,double lever, double percentSwapLong, string baseIndicator, string Indicator, ProcessedStockDataModel dataModel, string startDate, string endDate, double startMoneyUSD, double intervalMoneyUSD, int intervalMonths)
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

                TakeActionCFD(Last3Days());


                TimeLine.MoveNext();

                Account.PayForSwap(percentSwapLong, TimeLine.Today);

                if(Account.ReserveAccount<0) 
                {
                    Account.BackToMainAccount();
                    Account.SellAllStocks(StockPriceNow);
                    Account.CloseAllLongPositions(StockPriceNow);
                    Account.CloseAllShortPositions(StockPriceNow);

                    SellDates.Add(TimeLine.Today);
                }
                if(Account.GetCfdBilansForNow(StockPriceNow)<=0)
                {
                    break;
                }
            }

            Account.SellAllStocks(StockPriceNow);
            Account.CloseAllLongPositions(StockPriceNow);
            Account.CloseAllShortPositions(StockPriceNow);

            SellDates.Add(TimeLine.Today);

            //Account.PayTaxes();

            Console.WriteLine($"MA=> PaidIn:{Account.PaidInMoneyHistory} ; result after and other costs taxes: {Account.MainAccount}");

            //strategy to be further developed :
            // =>> need to pay swap from nominal value of contract?
            // ==> playing with alorithm  to  buy  when RSI has appropriate value (waiting for it ) additionaly when Take Action trigger

            // debug - check passing object and so on


            return 0;
        }
    }
}
