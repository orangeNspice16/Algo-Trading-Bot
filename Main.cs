using System.Diagnostics;

namespace TradingBot
{
    class EntryPoint
    {
        static async Task Main(string[] args)
        {
            var manager = Manager.Global;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            

            /*
            var project = ProcessFile.BackTest(
                "./testdata/input/BNB_ETH_USDT_1D.csv",
                "./testdata/output/testFile.csv",
                // [pairA, pairB, valueA, valueB, allowance, maker, taker]
                new Portfolio("ETH","USDT",0,100,100,0.001m,0.001m)
            );
            */
            
            /*
            1. Create market (will initially fill 200 data points in local storage, and loop every 1 minute add to the storage FiFo)
            2. Create project (by using the manager, and link the project with a market)
            3. Everytime that the market adds a new kline, it will send a trigger to the manager which will then see if any projects depend on the market and, if so, will
                provide the info to the project.
                What this means is that I need to have a list of all the projects inside of manager, and I need to have communication (market(s) -> manager | manager -> project((s))
            */

            // 1: adding a new security to the kucoin market
            bool success = await manager.AddSecurityToMarket("KuCoin", "BTC-USDT");
            
            // TO-DO: after adding the new security, it should start a timer which adds more klines to the list every 1 minute and ensures only 200 klines exist in the list

            // 2: create a project with KuCoin as the market. Initially, all data in a market should be inputted into the project's Candle data list.
            // After that, everytime a new kline is added to the market, the manager should ensure that it is added to the project's data list as well.

            var x = Console.ReadLine();

            stopWatch.Stop();
            Console.WriteLine(stopWatch.ElapsedMilliseconds);
        }
    }
}