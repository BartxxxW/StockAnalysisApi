using ApiChecker.RequestStockData;
using ApiChecker.SkendorStockModels;
using Skender.Stock.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ApiChecker.DataProcessing
{
    //public enum Indicators
    //{
    //    SMA,
    //    RSI,
    //    MACD,
    //    MACD_SIGNAL,
    //    MACD_HISTOGRAM,
    //    MACD_SLOW_EMA,
    //    MACD_FAST_EMA,
    //    EMA
    //}
    public static class Indicators
    {
        public const string SMA = "SMA";
        public const string RSI = "RSI";
        public const string MACD = "MACD";
        public const string MACD_SIGNAL = "MACD_SIGNAL";
        public const string MACD_HISTOGRAM = "MACD_HISTOGRAM";
        public const string MACD_SLOW_EMA = "MACD_SLOW_EMA";
        public const string MACD_FAST_EMA = "MACD_FAST_EMA";
        public const string EMA = "EMA";
        public const string OBV = "OBV";
        public const string OBV_RSI = "OBV_RSI";
        public const string SLOPE = "SLOPE";
        public const string RSI_SLOPE = "RSI_SLOPE";
    }

    public static class StockIndicators
    {
        private static Dictionary<string, Func<IEnumerable<StockModel>,int[],List<double>>> CallIndicator =
            new Dictionary<string, Func<IEnumerable<StockModel>, int[], List<double>>>
            {
                { Indicators.SMA,SMA},
                { Indicators.RSI,RSI},
                { Indicators.MACD,MACD},
                { Indicators.MACD_SIGNAL,MACD_SIGNAL},
                { Indicators.EMA,EMA},
                { Indicators.OBV,OBV},
                { Indicators.OBV_RSI,OBV_RSI},
                { Indicators.SLOPE,SLOPE},
                { Indicators.RSI_SLOPE,RSI_SLOPE}
            };

        private static IEnumerable<MacdResult>? _macdResult {get; set;} =null;

        private static List<double> EMA(IEnumerable<StockModel> stockModel, params int[] param)
        {
            var parameter = 30;
            if (param.Length != 0)
            {
                parameter = param[0];
            }
            var sma = stockModel.GetEma(parameter).Reverse();

            List<double> smaValues = sma.Select(v => {

                if (v.Ema == null) { return Double.NaN; }
                return Convert.ToDouble(v.Ema);

            }).ToList();

            return smaValues;
        }
        private static List<double> SMA(IEnumerable<StockModel> stockModel, params int[] param )
        {
            var parameter = 30;
            if(param.Length != 0) 
            {
                parameter = param[0];
            }
            var sma = stockModel.GetSma(parameter).Reverse();

            List<double> smaValues = sma.Select(v => {

                if (v.Sma == null) { return Double.NaN; }
                return Convert.ToDouble(v.Sma);

            }).ToList();

            return smaValues;
        }
        private static List<double> RSI(IEnumerable<StockModel> stockModel, params int[] param)
        {
            var parameter = 14;
            if (param.Length!=0)
            {
                parameter = param[0];
            }
            var sma = stockModel.GetRsi(parameter).Reverse();

            List<double> smaValues = sma.Select(v => {

                if (v.Rsi == null) { return Double.NaN; }
                return Convert.ToDouble(v.Rsi);

            }).ToList();

            return smaValues;
        }
        private static List<double> OBV(IEnumerable<StockModel> stockModel, params int[] param)
        {

            var sma = stockModel.GetObv().Reverse();

            List<double> smaValues = sma.Select(v => {

                if (v.Obv == null) { return Double.NaN; }
                return Convert.ToDouble(v.Obv);

            }).ToList();

            return smaValues;
        }
        private static List<double> RSI_SLOPE(IEnumerable<StockModel> stockModel, params int[] param)
        {
            var parameter = 14;
            if (param.Length != 0)
            {
                parameter = param[0];
            }
            var sma = stockModel.GetRsi(14).GetSlope(parameter).Reverse();

            List<double> smaValues = sma.Select(v => {

                if (v.Slope == null) { return Double.NaN; }
                return Convert.ToDouble(v.Slope);

            }).ToList();

            return smaValues;
        }
        private static List<double> SLOPE(IEnumerable<StockModel> stockModel, params int[] param)
        {
            var parameter = 14;
            if (param.Length != 0)
            {
                parameter = param[0];
            }
            var sma = stockModel.GetSlope(parameter).Reverse();

            List<double> smaValues = sma.Select(v => {
                return Convert.ToDouble(v.Line);

            }).ToList();

            return smaValues;
        }
        private static List<double> OBV_RSI(IEnumerable<StockModel> stockModel, params int[] param)
        {

            var sma = stockModel.GetObv().GetRsi(14).Reverse();

            List<double> smaValues = sma.Select(v => {

                if (v.Rsi == null) { return Double.NaN; }
                return Convert.ToDouble(v.Rsi);

            }).ToList();

            return smaValues;
        }
        private static List<double> MACD(IEnumerable<StockModel> stockModel, params int[] param)
        {
            int fastperiods = 12;
            int slowperiods = 26;
            int signalperiods = 9;
            if (param.Length != 0)
            {
                fastperiods = param[0];
                slowperiods = param[1];
                signalperiods = param[2];
            }
            _macdResult = stockModel.GetMacd(fastperiods,slowperiods,signalperiods).Reverse();

            List<double> smaValues = _macdResult.Select(v => {

                if (v.Macd == null) { return Double.NaN; }
                return Convert.ToDouble(v.Macd);

            }).ToList();

            return smaValues;
        }

        private static List<double> MACD_SIGNAL(IEnumerable<StockModel> stockModel, params int[] param)
        {
            if (_macdResult == null)
            {
                MACD(stockModel, param);
            }
            
            List<double> smaValues = _macdResult.Select(v => {

                if (v.Signal == null) { return Double.NaN; }
                return Convert.ToDouble(v.Signal);

            }).ToList();

            return smaValues;
        }



        public static List<double> GetIndicator(this IEnumerable<StockModel> stockModel,string indicator, params int[] param)
        {
            return CallIndicator[indicator](stockModel,param);
        }
    }
}
