namespace TradingBot
{
    public class Project
    {
        public List<Candle> data { get; set; } = new List<Candle>();
        public List<string> latestCandle { get; set; } = new List<string>();
        public string clientId { get; set; } = "";
        public int latestTime { get; set; }
        public int maxDataLen { get; set; } = 50; // Everything else is removed
        public Portfolio portfolio { get; set; }
        public Dictionary<long,string> snapshots = new Dictionary<long, string>();
        public TaskHandler tradeDecision { get; set; }
        public string market { get; set; }
        public string candleCode { get; set; } = "";
        public string name { get; set; }
        public int period { get; set; }
        public bool initialUpdate { get; set; }

        /* Indicator Optimisation Storage */
        public GeneralOpt genOpt { get; set; } = new GeneralOpt();
        public HighLowOpt hiloOpt { get; set; } = new HighLowOpt(9);
        public MacdSigOpt macdSOpt { get; set; } = new MacdSigOpt(12, 26, 9);
        public RsiOpt rsiOpt { get; set; } = new RsiOpt(14, 14);
        public StochRsiOpt stochRsiOpt { get; set; } = new StochRsiOpt(14, 3);
        public ChandelierOpt chandalierOpt { get; set; } = new ChandelierOpt(22, 3m);

        public Project(TaskHandler _d, Portfolio _p, string _m, string _n, int _pr)
        {
            name = _n;
            market = _m; // market
            portfolio = _p;
            period = _pr;
            tradeDecision = _d;
            tradeDecision.HandleTask(this);
        }


        public void AddSnap(long unix, string input)
        {
            if (snapshots.TryGetValue(unix, out string? d))
            {
                snapshots[unix] = input;
                return;
            }   
            snapshots.Add(unix, input);
        }

        
        /* Buy & Sell Methods */
        public void NormalBuy()
        {
            Candle candle = data.Last();
            //portfolio.stopLoss = data[data.Count - 3].close;
            
            if (Dummy.positionStatus.Equals("Short"))
            {
                portfolio.stopLoss = candle.close + candle.close * 0.1m;
            }
            else if (Dummy.positionStatus.Equals("Long"))
            {
                portfolio.stopLoss = candle.close - candle.close * 0.1m;
            }
            
            portfolio.buyOrder = candle.close;

            /* allowance allows constant buy amount at set price */
            decimal buy = Math.Min(portfolio.allowance, portfolio.valueB);
            buy = portfolio.allowance != 0? buy : portfolio.valueB;
            
            /* Buy pairA using pairB */
            //portfolio.valueA = buy / portfolio.buyOrder;
            portfolio.valueB -= buy;
            candle.finalDecision = "buy";
            if (portfolio.buyOrder == 0)
            {
                Console.WriteLine("WUAWIUABDUIAWFAUGAW");
            }
            /* Add a snapshot of the portfolio */
            AddSnap(candle.unix, $"{portfolio.valueA:0.###},{portfolio.valueB:0.###}");
        }

        public void NormalSell()
        {
            Candle candle = data.Last();

            /* Sell pairA and get pairB */
            portfolio.valueB += portfolio.valueA * candle.close;
            portfolio.valueA = 0;
            candle.finalDecision = "sell";

            /* Add a snapshot of the portfolio */
            //decimal profit = (candle.close / portfolio.buyOrder - 1) * 100;
            //portfolio.allProfit += profit;
            //AddSnap(candle.unix, $"{portfolio.valueA:0.###},{portfolio.valueB:0.###},{profit:0.###}%,{portfolio.allProfit:0.###}%");
        }


        /* Candle Methods */
        public bool ProcessCandle(Candle candle, bool canTrade)
        {
            // Data fill is consistent (time spacing)
            if (data.Count > 0)
            {
                if (candle.unix - data.Last().unix != period * 60)
                    return false;
            }

            if (data.Count == 0)
                AddSnap(candle.unix, $"{portfolio.valueA:0.###},{portfolio.valueB:0.###}");
                
            data.Add(candle);
            MakeTradeDecision(canTrade);
            return true;
        }

        public void MakeTradeDecision(bool canTrade)
        {
            /* Apply HiLo Indicator */
            HighLow.ApplyIndicator(this);

            /* Apply MacdSignal Indicator */
            MacdSig.ApplyIndicator(this);

            /* Apply Rel Str Idx Indicator */
            RelStrIdx.ApplyIndicator(this);

            /* Apply Stoch Rsi Indicator */
            StochRsi.ApplyIndicator(this);

            /* Apply Chandelier Indicator */
            Chandelier.ApplyIndicator(this);

            /* Make Trade Decision */
            if (canTrade)
                tradeDecision.HandleTask(this);

            /*\
            if (data.Count() < 2)
                return;

            var candle = data[data.Count - 2];
            Console.WriteLine(candle.finalDecision);
            Console.WriteLine("-------------------");
            if (candle.finalDecision.Equals("-") || candle.finalDecision.Equals("hodl") || candle.finalDecision.Equals("sell"))
            {
                data[data.Count - 1].finalDecision = "buy";
            }
            else
            {
                data[data.Count - 1].finalDecision = "sell";
            }
            Console.WriteLine(candle.finalDecision);
            */
        }



        /* Pinescript Methods */
        public decimal? PivotHigh()
        {
            return PineSim.GetPivotHighLow(this, true);
        }

        public decimal? PivotLow()
        {
            return PineSim.GetPivotHighLow(this, false);
        }

        public decimal? ValueWhen(string cond, string src, int ocr)
        {
            return (decimal?)PineSim.GetValueWhen(data, cond, src, ocr);
        }

        public decimal? Past(string src, int back)
        {
            return PineSim.GetPastValue(data, src, back);
        }

        public decimal? Sma(string src, int len)
        {
            return PineSim.GetSma(data, src, len);
        }

        public decimal? Ema(string src, string prop, int len)
        {
            return PineSim.GetEma(data, src, prop, len);
        }
    }
}