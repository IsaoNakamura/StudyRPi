using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using UtilityBitflyer;
using UtilityTrade;
using UtilityCryptowatch;
using UtilitySlack;

namespace CryptoBoxer
{
    public delegate int updateView();

    public class Boxer
    {
        // 設定パラメタ
        private BoxerConfig m_config { get; set; }

        // 状態用
        private CandleBuffer m_candleBuf { get; set; }
		private CandleBuffer m_candleBufTop { get; set; }
        public double m_min { get; set; }
        public double m_max { get; set; }
        public Position m_position { get; set; }
        private List<Position> m_posArray { get; set; }

        public double m_profitSum { get; set; }

        public int m_curShortBollLv { get; private set; }
        public int m_preShortBollLv { get; private set; }

        public int m_curLongBollLv { get; private set; }
        public int m_preLongBollLv { get; private set; }

        private bool m_isDotenShort { get; set; }
        private bool m_isDotenLong { get; set; }

        private bool m_isDoten { get; set; }

        public double m_frontlineLong { get; set; }
        public double m_frontlineShort { get; set; }

        public int m_upBreakCnt { get; set; }


        // デリゲートメソッド
        private updateView UpdateViewDelegate { get; set; }

        // 認証用
        private AuthBitflyer m_authBitflyer { get; set; }
        private AuthSlack m_authSlack { get; set; }

        private bool m_stopFlag { get; set; }
        public void setStopFlag(bool flag)
        {
            m_stopFlag = flag;
        }

        public bool getStopFlag()
        {
            return m_stopFlag;
        }

        private Boxer()
        {
            m_config = null;

            m_candleBuf = null;
			m_candleBufTop = null;

            m_posArray = null;

            m_min = Double.MaxValue;
            m_max = Double.MinValue;
            m_position = null;

            m_authBitflyer = null;

            m_stopFlag = false;

            m_curShortBollLv = -1;
            m_preShortBollLv = -1;
            m_curLongBollLv = -1;
            m_preLongBollLv = -1;

            m_profitSum = 0.0;

            m_isDotenShort = false;
            m_isDotenLong = false;

            m_isDoten = false;

            m_frontlineLong = 0.0;
            m_frontlineShort = 0.0;

            m_upBreakCnt = 0;

            return;
        }

        ~Boxer()
        {
            return;
        }

        public CandleBuffer getCandleBuffer()
        {
            return m_candleBuf;
        }

        public List<Position> getPositionList()
        {
            return m_posArray;
        }

        public Position getPosition()
        {
            return m_position;
        }

        public static Boxer createBoxer
        (
            updateView UpdateViewDelegate,
            string     configPath

        )
        {
            Boxer result = null;
            try
            {
                Boxer boxer = new Boxer();
                if (boxer == null)
                {
                    result = null;
                    return result;
                }

                BoxerConfig config = BoxerConfig.loadBoxerConfig(configPath);
                if (config == null)
                {
                    result = null;
                    return result;
                }

                CandleBuffer candleBuf = CandleBuffer.createCandleBuffer(config.buffer_num);
                if (candleBuf == null)
                {
                    result = null;
                    return result;
                }

				CandleBuffer candleBufTop = CandleBuffer.createCandleBuffer(config.buffer_num);
                if (candleBufTop == null)
                {
					result = null;
                    return result;
                }

                updateView _UpdateViewDelegate = null;
                if (UpdateViewDelegate != null)
                {
                    _UpdateViewDelegate = new updateView(UpdateViewDelegate);
                    if (_UpdateViewDelegate == null)
                    {
                        result = null;
                        return result;
                    }
                }

                Position position = new Position();
                if (position == null)
                {
                    result = null;
                    return result;
                }

                List<Position> posArray = new List<Position>();
                if (posArray == null)
                {
                    result = null;
                    return result;
                }
                posArray.Add(position);

                boxer.m_config              = config;
                boxer.m_posArray            = posArray;
                boxer.m_position            = position;
                boxer.UpdateViewDelegate    = _UpdateViewDelegate;
                boxer.m_candleBuf           = candleBuf;
				boxer.m_candleBufTop        = candleBufTop;

                result = boxer;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = null;
            }
            finally
            {
            }
            return result;
        }

        public string getPositionName()
        {
            string result = "NONE";
            try
            {
                if (m_position == null)
                {
                    result = "NONE";
                    return result;
                }

                result = m_position.getPositionStateStr();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = "NONE";
            }
            finally
            {
            }
            return result;
        }

        public double getEntryPrice()
        {
            double result = 0.0;
            try
            {
                if (m_position == null)
                {
                    result = 0.0;
                    return result;
                }

                result = m_position.entry_price;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = 0.0;
            }
            finally
            {
            }
            return result;
        }

        public double getExitPrice()
        {
            double result = 0.0;
            try
            {
                if (m_position == null)
                {
                    result = 0.0;
                    return result;
                }

                result = m_position.exit_price;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = 0.0;
            }
            finally
            {
            }
            return result;
        }

        public double calcProfit()
        {
            double result = 0.0;
            try
            {
                if (m_position == null)
                {
                    result = 0.0;
                    return result;
                }

                Candlestick curCandle = m_candleBuf.getLastCandle();
                if (curCandle == null)
                {
                    result = 0.0;
                    return result;
                }

                result = m_position.calcProfit(curCandle.last);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = 0.0;
            }
            finally
            {
            }
            return result;
        }

        // 過去のキャンドル情報群をバッファに適用
		private int applyCandlestick(CandleBuffer candleBuf, ref BitflyerOhlc ohlc, int periods)
        {
            int result = 0;
            try
            {
                if (ohlc == null)
                {
                    Console.WriteLine("failed to GetOhlcAfterAsync()");
                    result = -1;
                    return result;
                }

                if (ohlc.result == null)
                {
                    Console.WriteLine("ohlc's result is null");
                    result = -1;
                    return result;
                }

                List<List<double>> candles = ohlc.result.getResult(periods);

                if (candles == null)
                {
                    Console.WriteLine("ohlc's candle is null. periods={0}", periods);
                    result = -1;
                    return result;
                }

                foreach (List<double> candleFactor in candles)
                {
                    if (candleFactor == null)
                    {
                        continue;
                    }

                    double closeTime = candleFactor[0];
                    double openPrice = candleFactor[1];
                    double highPrice = candleFactor[2];
                    double lowPrice = candleFactor[3];
                    double closePrice = candleFactor[4];
                    double volume = candleFactor[5];

                    // Cryptowatchでとれるohlcは閉じてないキャンドルの値も取得される。
                    //  1回目 2018/04/11 10:14:00, open=743093, close=743172, high=743200, low=743093
                    //  2回目 2018/04/11 10:14:00, open=743093, close=743194, high=743200, low=743020
                    // Timestampが10:14:00なら、10:13:00～10:13:59のキャンドル


                    // 2018/04/10 19:21:00
                    DateTime timestamp = DateTimeOffset.FromUnixTimeSeconds((long)closeTime).LocalDateTime;
                    Console.WriteLine("{0}, open={1}, close={2}, high={3}, low={4}", timestamp.ToString(), openPrice, closePrice, highPrice, lowPrice);

                    Candlestick candle = candleBuf.addCandle(highPrice, lowPrice, openPrice, closePrice, timestamp.ToString());
                    if (candle == null)
                    {
                        Console.WriteLine("failed to addCandle.");
                        continue;
                    }

                    calcIndicator(candleBuf, ref candle);

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = -1;
            }
            finally
            {
            }
            return result;
        }

        private int applyCandlestick(CandleBuffer candleBuf, ref BitflyerOhlc ohlc, int periods, int begIdx, int count, bool isCalcIndicator=true)
        {
            int result = 0;
            try
            {
                if (ohlc == null)
                {
                    Console.WriteLine("failed to GetOhlcAfterAsync()");
                    result = -1;
                    return result;
                }

                if (ohlc.result == null)
                {
                    Console.WriteLine("ohlc's result is null");
                    result = -1;
                    return result;
                }

				List<List<double>> candles = ohlc.result.getResult(periods);

                if (candles == null)
                {
					Console.WriteLine("ohlc's candle is null. periods={0}", periods);
                    result = -1;
                    return result;
                }

                int limit = begIdx + count;
                if (limit > candles.Count)
                {
                    limit = candles.Count;
                }

                //foreach (List<double> candleFactor in candles)
                for (int i=begIdx; i<limit; i++)
                {
                    List<double> candleFactor = candles[i];
                    if (candleFactor == null)
                    {
                        continue;
                    }

                    double closeTime = candleFactor[0];
                    double openPrice = candleFactor[1];
                    double highPrice = candleFactor[2];
                    double lowPrice = candleFactor[3];
                    double closePrice = candleFactor[4];
                    double volume = candleFactor[5];

                    DateTime timestamp = DateTimeOffset.FromUnixTimeSeconds((long)closeTime).LocalDateTime;

                    if (closePrice <= Double.Epsilon)
                    {
                        Candlestick prevCandle = candleBuf.getLastCandle();
                        if (prevCandle == null)
                        {
                            Console.WriteLine("cur's candle-value 0. and prev's candle is null. timestamp=", timestamp.ToString());
                            result = -1;
                            return result;
                        }
                        closePrice = prevCandle.last;
                        openPrice = prevCandle.open;
                        highPrice = prevCandle.high;
                        lowPrice = prevCandle.low;
                        volume = prevCandle.volume;
                    }

                    // Cryptowatchでとれるohlcは閉じてないキャンドルの値も取得される。
                    //  1回目 2018/04/11 10:14:00, open=743093, close=743172, high=743200, low=743093
                    //  2回目 2018/04/11 10:14:00, open=743093, close=743194, high=743200, low=743020
                    // Timestampが10:14:00なら、10:13:00～10:13:59のキャンドル

                    Candlestick candle = candleBuf.addCandle(highPrice, lowPrice, openPrice, closePrice, timestamp.ToString());
                    if (candle == null)
                    {
                        Console.WriteLine("failed to addCandle.");
                        continue;
                    }
                    candle.volume = volume;

                    if (isCalcIndicator)
                    {
                        calcIndicator(candleBuf, ref candle);
                        Console.WriteLine("{0}, open={1}, close={2}, high={3}, low={4}, ema={5:0}", timestamp.ToString(), openPrice, closePrice, highPrice, lowPrice, candle.ema);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = -1;
            }
            finally
            {
            }
            return result;
        }

        public int calcIndicator(CandleBuffer candleBuf, ref Candlestick candle)
        {
            int result = 0;
            try
            {
                if (candle == null)
                {
                    result = -1;
                    return result;
                }

                // MAX更新
                {
                    if (m_max < candle.last)
                    {
                        m_max = candle.last;
                    }
                }


                // MIN更新
                {
                    if (m_min > candle.last)
                    {
                        m_min = candle.last;
                    }
                }

                // EMAを算出
                {
                    double ema = 0.0;
                    if (candleBuf.calcEma2(out ema, m_config.ema_sample_num) == 0)
                    {
                        candle.ema = ema;
                    }
                }

                {
                    double ema = 0.0;
                    if (candleBuf.calcEma2(out ema, m_config.ema_sub_sample_num) == 0)
                    {
                        candle.ema_sub = ema;
                    }
                }

                // 標準偏差、移動平均、ボリンジャーバンドを算出
                {
                    double stddev = 0.0;
                    double ma = 0.0;
                    if (candleBuf.calcStddevAndMA(out stddev, out ma, m_config.boll_sample_num) == 0)
                    {
                        candle.stddev = stddev;
                        candle.ma = ma;

                        candle.boll_high = ma + (2.0 * stddev);
                        candle.boll_low = ma - (2.0 * stddev);
                    }
                }

                {
                    double stddev = 0.0;
                    double ma = 0.0;
                    if (candleBuf.calcStddevAndMA(out stddev, out ma, m_config.boll_top_sample_num) == 0)
                    {
                        candle.ma_top = ma;
                        candle.boll_high_top = ma + (2.0 * stddev);
                        candle.boll_low_top = ma - (2.0 * stddev);
                    }
                }

                // ボラリティの移動平均を算出
                {
                    double vola_ma = 0.0;
                    if (candleBuf.calcVolatilityMA(out vola_ma, 20) == 0)
                    {
                        candle.vola_ma = vola_ma;
                    }
                }

                // Volumeの移動平均を算出
                {
                    double volume_ma = 0.0;
                    if (candleBuf.calcVolumeMA(out volume_ma, 20) == 0)
                    {
                        candle.volume_ma = volume_ma;
                    }
                }

                {
                    double angle_ma = 0.0;
                    if (candleBuf.calcEmaAngleMA(out angle_ma, 20) == 0)
                    {
                        candle.ema_angle = angle_ma;
                    }
                }

                {
                    double increase_child = 0.0;
                    if (candleBuf.calcMATopIncrease(out increase_child, 20) == 0)
                    {
                        candle.ma_top_increase = increase_child;
                    }

                    double increase_parent = 0.0;
                    if (candleBuf.calcMATopIncreaseMA(out increase_parent, 20) == 0)
                    {
                        if (Math.Abs(increase_parent) > double.Epsilon)
                        {
                            candle.ma_top_increase_rate = increase_child / increase_parent * 100.0;
                            //Console.WriteLine("rate={0:0.0}, child={1}, parent={2}", candle.ma_top_increase_rate, increase_child, increase_parent);
                        }
                    }
                }

                {
                    double top_length = 0.0;
                    double bottom_length = 0.0;
                    double range_min = 0.0;
                    double range_max = 0.0;
                    double body_min = 0.0;
                    double body_max = 0.0;
                    if (candleBuf.calcHigeLength(
                            out top_length,
                            out bottom_length,
                            out range_min,
                            out range_max,
                            out body_min,
                            out body_max,
                            60
                        ) == 0
                    )
                    {
                        //Console.WriteLine("HIGE_MAX top={0:0}, bottom={1:0}", top_length, bottom_length);
                        candle.hige_top_max = top_length;
                        candle.hige_bottom_max = bottom_length;
                        candle.range_min = range_min;
                        candle.range_max = range_max;
                        candle.body_min = body_min;
                        candle.body_max = body_max;

                        Candlestick lastCandle = candleBuf.getCandle(candleBuf.getLastCandleIndex()-1);
                        if (lastCandle != null)
                        {
                            // 下に拡張
                            if (candle.range_min < lastCandle.range_min)
                            {
                                if (lastCandle.range_min_cnt < 0)
                                {
                                    candle.range_min_cnt = 0;
                                }
                                else
                                {
                                    candle.range_min_cnt = lastCandle.range_min_cnt + 1;
                                }
                                candle.range_min_keep = 0;
                            }
                            else if (candle.range_min > lastCandle.range_min)
                            {
                                // 上に収縮
                                if (lastCandle.range_min_cnt > 0)
                                {
                                    candle.range_min_cnt = 0;
                                }
                                else
                                {
                                    candle.range_min_cnt = lastCandle.range_min_cnt - 1;
                                }
                                //candle.range_min_keep = 0;
                                candle.range_min_keep = lastCandle.range_min_keep + 1;
                            }
                            else
                            {
                                // 維持
                                candle.range_min_cnt=0;
                                candle.range_min_keep = lastCandle.range_min_keep + 1;
                            }

                            if (candle.range_max > lastCandle.range_max)
                            {
                                // 上に拡張
                                if (lastCandle.range_max_cnt < 0)
                                {
                                    candle.range_max_cnt = 0;
                                }
                                else
                                {
                                    candle.range_max_cnt = lastCandle.range_max_cnt + 1;
                                }
                                candle.range_max_keep = 0;
                            }
                            else if (candle.range_max < lastCandle.range_max)
                            {
                                // 下に収縮
                                if (candle.range_max_cnt > 0)
                                {
                                    candle.range_max_cnt = 0;
                                }
                                else
                                {
                                    candle.range_max_cnt = lastCandle.range_max_cnt - 1;
                                }
                                //candle.range_max_keep = 0;
                                candle.range_max_keep = lastCandle.range_max_keep + 1;
                            }
                            else
                            {
                                // 維持
                                candle.range_max_cnt = 0;
                                candle.range_max_keep = lastCandle.range_max_keep + 1;
                            }

                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = -1;
            }
            finally
            {
            }
            return result;
        }

        private async void postSlack(string text, bool onlyConsole=false)
        {
            Console.WriteLine(text);
            //onlyConsole = true;
            if (!onlyConsole)
            {
				await PostMessage.Request(m_authSlack, text);
            }
        }

        private async void applyPositions()
        {
            try
            {

                JArray retArray = await Trade.getPositions(m_authBitflyer, m_config.product_bitflyer);
                if (retArray != null)
                {
                    string sideTotal = "NONE";
                    double priceTotal = 0.0;
                    double amountTotal = 0.0;
                    string dateTotal = "";

                    bool isFirst = true;
                    foreach (JObject jobj in retArray)
                    {
                        string side = (string)jobj["side"];
                        double price = (double)jobj["price"];
                        double amount = (double)jobj["size"];
                        string open_date = (string)jobj["open_date"];

                        sideTotal = side;

                        DateTime dateTimeUtc = DateTime.Parse(open_date);// 2018-04-10T10:34:16.677 UTCタイム
                        DateTime timestamp = System.TimeZoneInfo.ConvertTimeFromUtc(dateTimeUtc, System.TimeZoneInfo.Local);


                        if (side == "BUY")
                        {
                            amountTotal += amount;
                            if (isFirst)
                            {
                                isFirst = false;

                                priceTotal = price;
                                dateTotal = open_date;
                            }
                            else
                            {
                                if (price > priceTotal)
                                {
                                    priceTotal = price;
                                }

                                DateTime timestampTotal = System.TimeZoneInfo.ConvertTimeFromUtc(DateTime.Parse(dateTotal), System.TimeZoneInfo.Local);
                                if (timestamp < timestampTotal)
                                {
                                    dateTotal = timestamp.ToString();
                                }


                            }

                        }
                        else if (side == "SELL")
                        {
                            amountTotal += amount;
                            if (isFirst)
                            {
                                isFirst = false;

                                priceTotal = price;
                                dateTotal = open_date;
                            }
                            else
                            {
                                if (price < priceTotal)
                                {
                                    priceTotal = price;
                                }

                                DateTime timestampTotal = System.TimeZoneInfo.ConvertTimeFromUtc(DateTime.Parse(dateTotal), System.TimeZoneInfo.Local);
                                if (timestamp < timestampTotal)
                                {
                                    dateTotal = timestamp.ToString();
                                }
                            }
                        }
                    }

                    amountTotal = Math.Round(amountTotal, 2);
                    if (sideTotal == "BUY")
                    {
                        if (m_position.isLong() && !m_position.isLongReserved())
                        {
                        }
                        else
                        {
                            m_position.entryLongOrder("hoge", dateTotal, amountTotal);

                            m_position.entry(priceTotal);

                            m_frontlineLong = priceTotal;

                            postSlack(string.Format("Apply Long-position. amount={0} entry={1:0} from={2}", amountTotal, priceTotal, dateTotal));
                        }
                    }
                    else if (sideTotal == "SELL")
                    {
                        if (m_position.isShort() && !m_position.isShortReserved())
                        {
                        }
                        else
                        {
                            m_position.entryShortOrder("hoge", dateTotal, amountTotal);

                            m_position.entry(priceTotal);

                            m_frontlineShort = priceTotal;

                            postSlack(string.Format("Apply Short-position. amount={0} entry={1:0} from={2}", amountTotal, priceTotal, dateTotal));
                        }
                    }
                    else if (sideTotal == "NONE")
                    {
                        if (m_position.isNone())
                        {
                        }
                        else
                        {
							postSlack(string.Format("Init position."));
                            m_position.init();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
            }
        }

        public async void MainLoop()
        {
            try
            {
                postSlack("====   START TRADE  ====");
                //System.Threading.Thread.Sleep(3000);

                postSlack(string.Format("amount={0}", m_config.amount));
                postSlack(string.Format("periods={0}", m_config.periods));
                //postSlack(string.Format("product={0}", m_config.product_bitflyer));
                //postSlack(string.Format("ema_sample_num={0}", m_config.ema_sample_num));
                //postSlack(string.Format("boll_sample_num={0}", m_config.boll_sample_num));
                //postSlack(string.Format("boll_top_sample_num={0}", m_config.boll_top_sample_num));
                //postSlack(string.Format("boll_over_candle_num={0}", m_config.boll_over_candle_num));
                //postSlack(string.Format("ema_diff_far={0}", m_config.ema_diff_far));
                //postSlack(string.Format("ema_diff_near={0}", m_config.ema_diff_near));
                postSlack(string.Format("losscut_value={0}", m_config.losscut_value));
                //postSlack(string.Format("buffer_num={0}", m_config.buffer_num));
                //postSlack(string.Format("backtest_hour={0}", m_config.backtest_hour));

                //System.Threading.Thread.Sleep(3000);

                // Cryptowatchから過去のデータを取得
                long after_secounds = m_candleBuf.m_buffer_num * m_config.periods;
                BitflyerOhlc ohlc = await BitflyerOhlc.GetOhlcAfterAsync(m_config.product_cryptowatch, m_config.periods, after_secounds);
				if (applyCandlestick(m_candleBuf, ref ohlc, m_config.periods, 0, m_candleBuf.m_buffer_num) != 0)
                {
                    Console.WriteLine("failed to applyCandlestick()");
                    return;
                }

                // CandleStick更新用
                double open_price = 0.0;
                double high_price = 0.0;
                double low_price = 0.0;

                // 過去のデータ群の最後のキャンドルを取得
                //  閉じたキャンドルでなければ更新することになる。
                Candlestick curCandle = m_candleBuf.getLastCandle();
                DateTime nextCloseTime = DateTime.Parse(curCandle.timestamp);

                bool isLastConnect = false;
                int pre_tick_id = 0;
                int cycle_cnt = 0;
                double pre_volume = 0.0;

				bool isSfd = true;

                m_frontlineLong = m_frontlineShort = curCandle.ema;


                applyPositions();


                while (true)
                {
                    System.Threading.Thread.Sleep(4000);

                    //applyPositions();

     //               if (m_position.isNone())
					//{
					//	// NONEポジションの場合
					//	bool isOrder = await Trade.isExistsActiveOrders(m_authBitflyer, m_config.product_bitflyer);
					//	if (isOrder)
					//	{
					//		Console.WriteLine("Wait. No Positon. but Ordered.");
					//		System.Threading.Thread.Sleep(60000);
					//		continue;
					//	}
					//}
     
                    // Tickerを取得
                    Ticker ticker = await Ticker.GetTickerAsync(m_config.product_bitflyer);
                    if (ticker == null)
                    {
                        continue;
                    }
                    int tick_id = ticker.tick_id;
                    double cur_value = ticker.ltp;
                    double volume = ticker.volume;//_by_product;

                    if (pre_tick_id == tick_id)
                    {
                        continue;
                    }

                    DateTime dateTimeUtc = DateTime.Parse(ticker.timestamp);// 2018-04-10T10:34:16.677 UTCタイム
                    DateTime cur_timestamp = System.TimeZoneInfo.ConvertTimeFromUtc(dateTimeUtc, System.TimeZoneInfo.Local);

                    

                    double vol_diff = volume - pre_volume;

                    


                    bool isClose = false;
                    if (!isLastConnect)
                    {

                        // 過去取得した最後のキャンドルが閉じているかチェック
                        //  過去最後のTimestampが10:14:00なら、10:13:00～10:13:59のキャンドル
                        //  現時刻が10:14:00～なら過去最後のキャンドルは閉まっている
                        //  現時刻が10:13:00～なら過去最後のキャンドルは閉まっていない

                        TimeSpan span = cur_timestamp - nextCloseTime;
                        double elapsed_sec = span.TotalSeconds;

                        if (elapsed_sec < 0)
                        {
                            // 現時刻が過去最後のキャンドルの閉めより前だった場合、過去最後のキャンドルは閉まっていない
                            open_price = curCandle.open;
                            high_price = curCandle.high;
                            low_price = curCandle.low;

                            Console.WriteLine("prev is not closed. next={0}, cur={1}, elapsed={2}, open={3}, cur={4}, high={5}, low={6}"
                                , nextCloseTime
                                , cur_timestamp
                                , elapsed_sec
                                , open_price
                                , cur_value
                                , high_price
                                , low_price
                            );
                        }
                        else
                        {
                            // 現時刻が過去最後のキャンドルの閉めと一緒もしくはそれ以降だった場合、過去最後のキャンドルは閉まっている

                            nextCloseTime = nextCloseTime.AddSeconds(m_config.periods);

                            Console.WriteLine("prev is closed. next={0}, cur={1}, elapsed={2}", nextCloseTime, cur_timestamp, elapsed_sec);
                            isClose = true;
                            curCandle = null;
                        }
                        isLastConnect = true;
                    }
                    else
                    {
                        TimeSpan diff = nextCloseTime - cur_timestamp;
                        double diff_sec = diff.TotalSeconds;

                        // キャンドルを閉じるべきか判断
                        //Console.WriteLine("is close?. next={0}, cur={1}, diff={2}", nextCloseTime, cur_timestamp, diff_sec);
                        if (diff_sec <= 0.0)
                        {
                            // 次の更新時間になったらキャンドルを閉じる
                            //Console.WriteLine("need close. next={0}, cur={1}, diff={2}", nextCloseTime, cur_timestamp, diff_sec);

                            isClose = true;
                        }
                    }

                    // 足の最高値更新
                    if (high_price <= double.Epsilon)
                    {
                        high_price = cur_value;
                    }
                    else if (high_price < cur_value)
                    {
                        high_price = cur_value;
                    }

                    // 足の最低値更新
                    if (low_price <= double.Epsilon)
                    {
                        low_price = cur_value;
                    }
                    else if (low_price > cur_value)
                    {
                        low_price = cur_value;
                    }

                    if (isClose == true)
                    {
                        // キャンドルを閉じる
                        if (curCandle != null)
                        {
                            // CloseTimeは次の更新時間を使用する。
                            curCandle.timestamp = nextCloseTime.ToString();

                            // 現物(BTC_JPY)のTickerを取得
                            Ticker ticker_spot = await Ticker.GetTickerAsync("BTC_JPY");
                            double spot_last = 0.0;
                            if (ticker_spot != null)
                            {
                                // FXと現物の価格乖離率を算出
                                spot_last = ticker_spot.ltp;
                                double disparity_rate = (curCandle.last - spot_last) / spot_last * 100.0;
                                curCandle.disparity_rate = disparity_rate;
                            }

                            // ENTRY/EXITロジック
                            await tryEntryOrder(cur_value);
                            await tryExitOrder(cur_value);

                            if (m_position.isEntryCompleted())
                            {
								Console.WriteLine("closed candle. timestamp={0},    profit={1},last={2:0},frL={3:0},frS={4:0},emaS={5:0},sfd={6:0.00}"
                                                  , curCandle.timestamp
                                                  , m_position.calcProfit(curCandle.last)
                                                  , curCandle.last
                                                  , m_frontlineLong
                                                  , m_frontlineShort
								                  , curCandle.ema_sub
								                  , curCandle.disparity_rate
                                );
                            }
                            else
                            {
								Console.WriteLine("closed candle. timestamp={0},profit_sum={1},last={2:0},frL={3:0},frS={4:0},emaS={5:0},sfd={6:0.00}"
                                                  , curCandle.timestamp
                                                  , m_profitSum
                                                  , curCandle.last
                                                  , m_frontlineLong
                                                  , m_frontlineShort
								                  , curCandle.ema_sub
                                                  , curCandle.disparity_rate
                                );
                            }

                            if (Math.Abs(curCandle.disparity_rate) >= 4.9)
                            {
                                // SFD
                                if (!isSfd)
                                {
                                    isSfd = true;
                                    postSlack(string.Format("DispartyRate is Over. rate={0:0.00}. fx={1:0} btc={2:0}", curCandle.disparity_rate, curCandle.last, spot_last));
                                }
                            }
                            else
                            {
                                // no SFD
                                if (isSfd)
                                {
                                    isSfd = false;
                                    postSlack(string.Format("DispartyRate is Under. rate={0:0.00}. fx={1:0} btc={2:0}", curCandle.disparity_rate, curCandle.last, spot_last));
                                }
                            }
                        }
                        // 次の更新時間を更新
                        nextCloseTime = nextCloseTime.AddSeconds(m_config.periods);

                        // 新たなキャンドルを追加
                        open_price = cur_value;
                        curCandle = m_candleBuf.addCandle(high_price, low_price, open_price, cur_value, cur_timestamp.ToString());
                        if (curCandle == null)
                        {
                            Console.WriteLine("failed to addCandle.");
                            return;
                        }

                        // 最高値・最低値リセット
                        high_price = 0.0;
                        low_price = 0.0;
                    }
                    else
                    {
                        // 現在のキャンドルを更新
                        if (curCandle != null)
                        {
                            curCandle.high = high_price;
                            curCandle.low = low_price;
                            curCandle.open = open_price;
                            curCandle.last = cur_value;
                            curCandle.timestamp = cur_timestamp.ToString();
                            curCandle.volume = curCandle.volume + vol_diff;
                            //Console.WriteLine("diff={0:2} vol={1:2} pre={2:2}",vol_diff, volume, pre_volume);
                        }
                    }

                    // インジケータ更新

                    if (curCandle != null)
                    {
                        calcIndicator(m_candleBuf, ref curCandle);
                    }


                    // 注文状況確認ロジック
                    await checkEntry();

                    // Losscutロジック
                    //await tryLosscutOrder();

                    await checkExit();


                    ////await checkEntryCompleted();

                    // 表示を更新
                    if (UpdateViewDelegate != null)
                    {
                        UpdateViewDelegate();
                    }

                    pre_tick_id = tick_id;
                    pre_volume = volume;

                    if( System.IO.File.Exists(@"./StopCode.txt") )
                    {
                        m_stopFlag = true;
                        System.IO.File.Delete(@"./StopCode.txt");
                    }

                    if (m_stopFlag)
                    {
                        await tryCnacelOrder();
                        postSlack("recieved StopCode.");
                        System.Threading.Thread.Sleep(3000);

                        postSlack(string.Format("PROFIT_SUM={0:0}", m_profitSum));
                        System.Threading.Thread.Sleep(3000);

                        postSlack("====  END TRADE  ====");
                        System.Threading.Thread.Sleep(3000);

                        break;
                    }

                    cycle_cnt++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
            }
            return;
        }


        public bool isBackTest()
        {
            if (m_config == null)
            {
                return false;
            }

            if (m_config.backtest_flag > 0)
            {
                return true;
            }

            return false;
        }

        private async void sleepAsync(int sec)
        {
            await Task.Delay(sec * 1000);
        }

        public async void BackTest()
        {
            try
            {

                postSlack("==== START BACKTEST ====",true);
                //System.Threading.Thread.Sleep(3000);

                postSlack(string.Format("amount={0}",m_config.amount), true);
                postSlack(string.Format("periods={0}", m_config.periods), true);
                postSlack(string.Format("product={0}", m_config.product_bitflyer), true);
                postSlack(string.Format("ema_sample_num={0}", m_config.ema_sample_num), true);
                postSlack(string.Format("boll_sample_num={0}", m_config.boll_sample_num), true);
                postSlack(string.Format("boll_top_sample_num={0}", m_config.boll_top_sample_num), true);
                postSlack(string.Format("boll_over_candle_num={0}", m_config.boll_over_candle_num), true);
                postSlack(string.Format("ema_diff_far={0}", m_config.ema_diff_far), true);
                postSlack(string.Format("ema_diff_near={0}", m_config.ema_diff_near), true);
                postSlack(string.Format("losscut_value={0}", m_config.losscut_value), true);
                postSlack(string.Format("buffer_num={0}", m_config.buffer_num), true);
                postSlack(string.Format("backtest_hour={0}", m_config.backtest_hour), true);

                //System.Threading.Thread.Sleep(3000);


                // Cryptowatchから過去のデータを取得
				int test_num = (m_config.backtest_hour * 60 * 60) / m_config.periods;
                long after_secounds = (m_candleBuf.m_buffer_num + test_num) * m_config.periods;
                BitflyerOhlc ohlc = await BitflyerOhlc.GetOhlcAfterAsync(m_config.product_cryptowatch, m_config.periods, after_secounds);
				if (applyCandlestick(m_candleBuf, ref ohlc,m_config.periods, 0, m_candleBuf.m_buffer_num) != 0)
                {
                    Console.WriteLine("failed to applyCandlestick()");
                    return;
                }

                CandleBuffer testCandleBuf = CandleBuffer.createCandleBuffer(test_num);
                if (testCandleBuf == null)
                {
                    Console.WriteLine("failed to create test CandleBuffer");
                    return;
                }
				if (applyCandlestick(testCandleBuf, ref ohlc, m_config.periods, m_candleBuf.getCandleCount(), test_num, false) != 0)
                {
                    Console.WriteLine("failed to applyCandlestick()");
                    return;
                }


                int candle_cnt = testCandleBuf.getCandleCount();
                if (candle_cnt <= 0)
                {
                    Console.WriteLine("candle's count is 0");
                    return;
                }


				int test_num_top = (m_config.backtest_hour * 60 * 60) / m_config.periods_top;
				long after_secounds_top = (m_candleBuf.m_buffer_num + test_num_top) * m_config.periods_top;
				BitflyerOhlc ohlc_top = await BitflyerOhlc.GetOhlcAfterAsync(m_config.product_cryptowatch, m_config.periods_top, after_secounds_top);
                

				if (applyCandlestick(m_candleBufTop, ref ohlc_top, m_config.periods_top, 0, m_candleBuf.m_buffer_num) != 0)
                {
                    Console.WriteLine("failed to applyCandlestick()");
                    return;
                }

				CandleBuffer testCandleBuf_top = CandleBuffer.createCandleBuffer(test_num_top);
				if (testCandleBuf_top == null)
                {
                    Console.WriteLine("failed to create test CandleBufferTop");
                    return;
                }
				if (applyCandlestick(testCandleBuf_top, ref ohlc_top, m_config.periods_top, m_candleBufTop.getCandleCount(), test_num_top, false) != 0)
                {
                    Console.WriteLine("failed to applyCandlestick()");
                    return;
                }

                m_frontlineLong = m_frontlineShort = testCandleBuf.getCandle(0).last;

                int top_index = 0;

                int long_entry_cnt = 0;
                int short_entry_cnt = 0;
                int long_exit_cnt = 0;
                int short_exit_cnt = 0;
                int long_lc_cnt = 0;
                int short_lc_cnt = 0;
                int cycle_cnt = 0;
                for (int i = 0; i < candle_cnt; i++)
                {
                    Candlestick curCandle = testCandleBuf.getCandle(i);
                    if (curCandle == null)
                    {
                        continue;
                    }

                    // インジケータ更新

                    if (m_candleBuf.addCandle(curCandle) != 0)
                    {
                        Console.WriteLine("failed to addCandle for m_candleBuf.");
                        return;
                    }

                    calcIndicator(m_candleBuf, ref curCandle);

                    double next_open = 0.0;
                    Candlestick nextCandle = testCandleBuf.getCandle(i+1);
                    if (nextCandle == null)
                    {
                        next_open = curCandle.last;
                    }
                    else
                    {
                        next_open = nextCandle.open;
                    }
                    
					{
						if(top_index==0)
						{
							Candlestick topCandle = testCandleBuf_top.getCandle(top_index);
							DateTime curTime = DateTime.Parse(curCandle.timestamp);
							DateTime topTime = DateTime.Parse(topCandle.timestamp);
                            TimeSpan span = curTime - topTime;
                            double elapsed_sec = span.TotalSeconds;
							if(elapsed_sec>0)
							{
								if (m_candleBufTop.addCandle(topCandle) != 0)
                                {
                                    Console.WriteLine("failed to addCandle for candleBuf_top.");
                                    return;
                                }
								calcIndicator(m_candleBufTop, ref topCandle);
                                
                          //      Console.WriteLine("#### candleBufTop[{0}]. timestamp={1},last={2:0},trend={3}"
                          //                        , top_index
                          //                        , topCandle.timestamp
                          //                        , topCandle.last
								                  //, topCandle.isTrend()
                          //       );
								
								top_index++;
							}
						}

						{                     
							Candlestick topCandle = testCandleBuf_top.getCandle(top_index);
							if (topCandle != null)
							{                        
								if (curCandle.timestamp == topCandle.timestamp)
								{
									if (m_candleBufTop.addCandle(topCandle) != 0)
									{
										Console.WriteLine("failed to addCandle for candleBuf_top.");
										return;
									}

									calcIndicator(m_candleBufTop, ref topCandle);                           
                                    
									//Console.WriteLine("#### candleBufTop[{0}]. timestamp={1},last={2:0},trend={3}"
									//				  , top_index
									//				  , topCandle.timestamp
									//				  , topCandle.last
									//				  , topCandle.isTrend()
									// );
                                     top_index++;
								}
							}
						}
                    }

                    // ENTRYテスト
                    tryEntryOrderTest(ref long_entry_cnt, ref short_entry_cnt, next_open);
                    checkEntryTest(curCandle.last);

                    // EXIT/ロスカットテスト
                    if (tryExitOrderTest(ref long_exit_cnt, ref short_exit_cnt, ref long_lc_cnt, ref short_lc_cnt, next_open) == 0)
                    {
                        checkExitTest(curCandle.last);
                    }

                    //if (tryLosscutOrderTest(ref long_lc_cnt, ref short_lc_cnt) == 0)
                    //{
                    //    checkExitTest(curCandle.last);
                    //}

                    if (m_position.isEntryCompleted())
                    {
						Console.WriteLine("closed candle. timestamp={0},    profit={1},last={2:0},frL={3:0},frS={4:0},emaS={5:0},vola={6:0}"
                                          , curCandle.timestamp
                                          , m_position.calcProfit(curCandle.last)
                                          , curCandle.last
                                          , m_frontlineLong
                                          , m_frontlineShort
						                  , curCandle.ema_sub
						                  , curCandle.vola_ma
                        );
                    }
                    else
                    {
						Console.WriteLine("closed candle. timestamp={0},profit_sum={1},last={2:0},frL={3:0},frS={4:0},emaS={5:0},vola={6:0}"
                                          , curCandle.timestamp
                                          , m_profitSum
                                          , curCandle.last
                                          , m_frontlineLong
                                          , m_frontlineShort
						                  , curCandle.ema_sub
						                  , curCandle.vola_ma
                        );
                    }




                    System.Threading.Thread.Sleep(0);
                    cycle_cnt++;
                }

                // 表示を更新
                if (UpdateViewDelegate != null)
                {
                    UpdateViewDelegate();
                }

                foreach (Position position in m_posArray)
                {
                    if (position == null)
                    {
                        continue;
                    }

                    string state = position.getPositionStateStr();
                    double profit = position.getProfit();
                    double entry_price = position.entry_price;
                    double exit_price = position.exit_price;

                    //postSlack(string.Format("pos={0,5}, profit={1:0}, entry={2:0}, exit={3:0}", state, profit, entry_price, exit_price));
                    Console.WriteLine("pos={0,5},profit={1:0},entry={2:0},exit={3:0},from={4},to={5}", state, profit, entry_price, exit_price, position.entry_date, position.exit_date);
                }

                //System.Threading.Thread.Sleep(3000);
                postSlack(string.Format("PROFIT_SUM={0:0}, LONG={1}, SHORT={2}, LONG_LC={3}, SHORT_LC={4}", m_profitSum, long_entry_cnt, short_entry_cnt, long_lc_cnt, short_lc_cnt), true);

                //System.Threading.Thread.Sleep(3000);
                postSlack("====  END BACKTEST  ====", true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                m_stopFlag = true;
            }
            return;
        }


        public int loadAuthBitflyer(string filePath)
        {
            int result = 0;
            try
            {
                m_authBitflyer = AuthBitflyer.createAuthBitflyer(filePath);
                if (m_authBitflyer == null)
                {
                    result = -1;
                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = -1;
            }
            finally
            {
            }
            return result;
        }


        public int loadAuthSlack(string filePath)
        {
            int result = 0;
            try
            {
                m_authSlack = AuthSlack.createAuthSlack(filePath);
                if (m_authSlack == null)
                {
                    result = -1;
                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = -1;
            }
            finally
            {
            }
            return result;
        }



        public async Task<int> checkEntry()
        {
            int result = 0;
            try
            {
                if (m_position.isNone())
                {
                    result = 1;
                    return result;
                }
                // NONEポジションじゃない場合

                if (!m_position.isEntryActive())
                {
                    result = 1;
                    return result;
                }
                // ENTRYアクティブの場合

                GetChildOrderResponse responce = await SendChildOrder.getChildOrderAveragePrice(m_authBitflyer, m_config.product_bitflyer, m_position.entry_id);
                if(responce==null)
                {
                    //Console.WriteLine("Order is not completed.");
                    result = 1;
                    return result;
                }
                
                if (responce.child_order_state == "REJECTED")
                {
                    postSlack(string.Format("Order is rejected. entry_price={0}", responce.average_price));
                    result = -1;
                    return result;
                }

                if (responce.child_order_state != "COMPLETED")
                {
                    postSlack(string.Format("Order is not completed. entry_price={0}", responce.average_price));
                    result = 1;
                    return result;
                }

                // 注文確定
                postSlack(string.Format("Order is completed. entry_price={0} id={1}", responce.average_price, m_position.entry_id));
                m_position.entry(responce.average_price);

                if (m_position.isLong())
                {
                    m_frontlineLong = responce.average_price;// + m_config.losscut_value;
                }
                else if (m_position.isShort())
                {
                    m_frontlineShort = responce.average_price;// - m_config.losscut_value;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = -1;
            }
            finally
            {
            }
            return result;
        }



        public int checkEntryTest(double last_price)
        {
            int result = 0;
            try
            {
                if (m_position.isNone())
                {
                    result = 1;
                    return result;
                }
                // NONEポジションじゃない場合

                if (!m_position.isEntryActive())
                {
                    result = 1;
                    return result;
                }
                // ENTRYアクティブの場合

                // 注文確定
                postSlack(string.Format("Order is completed. entry_price={0} id={1}", last_price, m_position.entry_id), true);
                m_position.entry(last_price);

                if (m_position.isLong())
                {
                    m_frontlineLong = last_price;// + m_config.losscut_value;
                }
                else if (m_position.isShort())
                {
                    m_frontlineShort = last_price;// - m_config.losscut_value;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = -1;
            }
            finally
            {
            }
            return result;
        }

        public async Task<int> tryExitOrder(double next_open)
        {
            int result = 0;
            try
            {
                if (m_position.isNone())
                {
                    result = 1;
                    return result;
                }
                // NONEポジションじゃない場合

                if (!m_position.isEntryCompleted())
                {
                    result = 1;
                    return result;
                }
                // エントリーが完了している場合

                if (!m_position.isExitNone())
                {
                    result = 1;
                    return result;
                }
                // EXITが未だの場合

                Candlestick curCandle = m_candleBuf.getLastCandle();
                if (curCandle == null)
                {
                    result = -1;
                    return result;
                }


                if (m_position.isLong())
                {// LONGの場合

                    int long_lc_cnt = 0;
                    bool isCond = isConditionLongExitFL(ref long_lc_cnt, next_open, false);

                    if (isCond)
                    {
                        if (isMaintenanceBitflyer(curCandle.timestamp))
                        {
                            result = 1;
                            return result;
                        }

                        //Console.WriteLine("Try Long Exit Order.");
                        SendChildOrderResponse retObj = null;
                        int retry_cnt = 0;
                        while (true)
                        {
                            retry_cnt++;
                            retObj = await SendChildOrder.SellMarket(m_authBitflyer, m_config.product_bitflyer, m_position.amount);
                            if (retObj == null)
                            {
                                postSlack(string.Format("failed to Long Exit Order. retry_cnt={0}", retry_cnt));
                                System.Threading.Thread.Sleep(1000);
                                continue;
                            }
                            break;
                        }
                        // 注文成功
                        postSlack(string.Format("{0} Long Exit Order ID = {1}", curCandle.timestamp, retObj.child_order_acceptance_id));
                        m_position.exitOrder(retObj.child_order_acceptance_id, curCandle.timestamp);
                    }
                }
                else if (m_position.isShort())
                {// SHORTの場合
                    int short_lc_cnt = 0;
                    bool isCond = isConditionShortExitFL(ref short_lc_cnt, next_open, false);

                    if (isCond)
                    {
                        if (isMaintenanceBitflyer(curCandle.timestamp))
                        {
                            result = 1;
                            return result;
                        }

                        //Console.WriteLine("Try Short Exit Order.");
                        SendChildOrderResponse retObj = null;
                        int retry_cnt = 0;
                        while (true)
                        {
                            retry_cnt++;
                            retObj = await SendChildOrder.BuyMarket(m_authBitflyer, m_config.product_bitflyer, m_position.amount);
                            if (retObj == null)
                            {
                                postSlack(string.Format("failed to Short Exit Order. retry_cnt={0}", retry_cnt));
                                System.Threading.Thread.Sleep(1000);
                                continue;
                            }
                            break;
                        }
                        // 注文成功
                        postSlack(string.Format("{0} Short Exit Order ID = {1}", curCandle.timestamp, retObj.child_order_acceptance_id));
                        m_position.exitOrder(retObj.child_order_acceptance_id, curCandle.timestamp);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = -1;
            }
            finally
            {
            }
            return result;
        }

        public int tryExitOrderTest
        (
            ref int long_exit_cnt,
            ref int short_exit_cnt,
            ref int long_lc_cnt,
            ref int short_lc_cnt,
            double next_open
        )
        {
            int result = 0;
            try
            {
                if (m_position.isNone())
                {
                    result = 1;
                    return result;
                }
                // NONEポジションじゃない場合

                if (!m_position.isEntryCompleted())
                {
                    result = 1;
                    return result;
                }
                // エントリーが完了している場合

                if (!m_position.isExitNone())
                {
                    result = 1;
                    return result;
                }
                // EXITが未だの場合

                Candlestick curCandle = m_candleBuf.getLastCandle();
                if (curCandle == null)
                {
                    result = -1;
                    return result;
                }




                if (m_position.isLong())
                {// LONGの場合

                    bool isCond = isConditionLongExitFL(ref long_lc_cnt, next_open);

                    if (isCond)
                    {
                        if (isMaintenanceBitflyer(curCandle.timestamp))
                        {
                            result = 1;
                            return result;
                        }

                        // 注文成功
                        string long_id = string.Format("BT_LONG_EXIT_{0:D8}", long_exit_cnt);
                        postSlack(string.Format("{0} Long Exit Order ID = {1}", curCandle.timestamp, long_id), true);
                        m_position.exitOrder(long_id, curCandle.timestamp);
                        long_exit_cnt++;
                    }
                }
                else if (m_position.isShort())
                {// SHORTの場合
                    bool isCond = isConditionShortExitFL(ref short_lc_cnt, next_open);

                    if (isCond)
                    {
                        if (isMaintenanceBitflyer(curCandle.timestamp))
                        {
                            result = 1;
                            return result;
                        }

                        // 注文成功
                        string short_id = string.Format("BT_SHORT_EXIT_{0:D8}", short_exit_cnt);
                        postSlack(string.Format("{0} Short Exit Order ID = {1}", curCandle.timestamp, short_id),true);
                        m_position.exitOrder(short_id, curCandle.timestamp);
                        short_exit_cnt++;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = -1;
            }
            finally
            {
            }
            return result;
        }

        public async Task<int> tryLosscutOrder()
        {
            int result = 0;
            try
            {
                if (m_position.isNone())
                {
                    result = 1;
                    return result;
                }
                // NONEポジションじゃない場合

                if (!m_position.isEntryCompleted())
                {
                    result = 1;
                    return result;
                }
                // エントリーが完了している場合

                if (!m_position.isExitNone())
                {
                    result = 1;
                    return result;
                }
                // EXITが未だの場合

                Candlestick curCandle = m_candleBuf.getLastCandle();
                if (curCandle == null)
                {
                    result = -1;
                    return result;
                }


                if (m_position.isLong())
                {// LONGの場合

                    if (isConditionLongLosscut())
                    {
                        //Console.WriteLine("Try Long Losscut Order.");

                        SendChildOrderResponse retObj = await SendChildOrder.SellMarket(m_authBitflyer, m_config.product_bitflyer, m_position.amount);
                        if (retObj == null)
                        {
                            postSlack("failed to Long Losscut Order.");
                            result = -1;
                            return result;
                        }
                        // 注文成功
                        postSlack(string.Format("{0} Long Losscut Order ID = {1}", curCandle.timestamp, retObj.child_order_acceptance_id));
                        m_position.exitOrder(retObj.child_order_acceptance_id, curCandle.timestamp);
                    }
                }
                else if (m_position.isShort())
                {// SHORTの場合
                    if (isConditionShortLosscut())
                    {
                        //Console.WriteLine("Try Short Losscut Order.");

                        SendChildOrderResponse retObj = await SendChildOrder.BuyMarket(m_authBitflyer, m_config.product_bitflyer, m_position.amount);
                        if (retObj == null)
                        {
                            postSlack("failed to Short Losscut Order.");
                            result = -1;
                            return result;
                        }
                        // 注文成功
                        postSlack(string.Format("{0} Short Losscut Order ID = {1}", curCandle.timestamp, retObj.child_order_acceptance_id));
                        m_position.exitOrder(retObj.child_order_acceptance_id, curCandle.timestamp);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = -1;
            }
            finally
            {
            }
            return result;
        }

        public int tryLosscutOrderTest(ref int long_lc_cnt, ref int short_lc_cnt)
        {
            int result = 0;
            try
            {
                if (m_position.isNone())
                {
                    result = 1;
                    return result;
                }
                // NONEポジションじゃない場合

                if (!m_position.isEntryCompleted())
                {
                    result = 1;
                    return result;
                }
                // エントリーが完了している場合

                if (!m_position.isExitNone())
                {
                    result = 1;
                    return result;
                }
                // EXITが未だの場合

                Candlestick curCandle = m_candleBuf.getLastCandle();
                if (curCandle == null)
                {
                    result = -1;
                    return result;
                }


                if (m_position.isLong())
                {// LONGの場合

                    if (isConditionLongLosscut())
                    {
                        // 注文成功
                        string losscut_id = string.Format("BT_LONG_LC_{0:D8}", long_lc_cnt);
                        postSlack(string.Format("{0} Long Losscut Order ID = {1}", curCandle.timestamp, losscut_id), true);
                        m_position.exitOrder(losscut_id, curCandle.timestamp);
                        long_lc_cnt++;
                    }
                }
                else if (m_position.isShort())
                {// SHORTの場合
                    if (isConditionShortLosscut())
                    {
                        // 注文成功
                        string losscut_id = string.Format("BT_SHORT_LC_{0:D8}", short_lc_cnt);
                        postSlack(string.Format("{0} Short Losscut Order ID = {1}", curCandle.timestamp, losscut_id), true);
                        m_position.exitOrder(losscut_id, curCandle.timestamp);
                        short_lc_cnt++;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = -1;
            }
            finally
            {
            }
            return result;
        }

        public async Task<int> checkExit()
        {
            int result = 0;
            try
            {
                if (m_position.isNone())
                {
                    result = 1;
                    return result;
                }
                // NONEポジションじゃない場合

                if (!m_position.isExitActive())
                {
                    result = 1;
                    return result;
                }
                // EXIT注文アクティブの場合

                GetChildOrderResponse responce = await SendChildOrder.getChildOrderAveragePrice(m_authBitflyer, m_config.product_bitflyer, m_position.exit_id);
                if (responce == null)
                {
                    //Console.WriteLine("Order is not completed.");
                    result = 1;
                    return result;
                }

                if (responce.child_order_state == "REJECTED")
                {
                    postSlack(string.Format("Order is rejected. entry_price={0}", responce.average_price));
                    result = -1;
                    return result;
                }

                if (responce.child_order_state != "COMPLETED")
                {
                    postSlack(string.Format("Order is not completed. entry_price={0}", responce.average_price));
                    result = 1;
                    return result;
                }

                // 注文確定
                m_position.exit(responce.average_price);
                m_profitSum += m_position.getProfit();
                postSlack(string.Format("Order is completed. profit={0:0} sum={1:0} exit_price={2:0} id={3}"
                                        , m_position.getProfit()
                                        , m_profitSum
                                        , responce.average_price
                                        , m_position.exit_id)
                         );

                m_position = new Position();
                m_posArray.Add(m_position);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = -1;
            }
            finally
            {
            }
            return result;
        }

        public int checkExitTest(double exit_price)
        {
            int result = 0;
            try
            {
                if (m_position.isNone())
                {
                    result = 1;
                    return result;
                }
                // NONEポジションじゃない場合

                if (!m_position.isExitActive())
                {
                    result = 1;
                    return result;
                }
                // EXITアクティブの場合


                // 注文確定
                m_position.exit(exit_price);
                m_profitSum += m_position.getProfit();
                postSlack(string.Format("Order is completed. profit={0:0} sum={1:0} exit_price={2:0} id={3}"
                                        , m_position.getProfit()
                                        , m_profitSum
                                        , exit_price
                                        , m_position.exit_id), true
                         );

                m_position = new Position();
                m_posArray.Add(m_position);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = -1;
            }
            finally
            {
            }
            return result;
        }
        

        private bool isBadPosition()
        {
            bool result = false;
            try
            {
                if (m_candleBuf == null)
                {
                    result = false;
                    return result;
                }

                Candlestick curCandle = m_candleBuf.getLastCandle();
                if (curCandle == null)
                {
                    result = false;
                    return result;
                }
                
                double profit = m_position.calcProfit(curCandle.last);

                Candlestick minCandle = null;
                Candlestick maxCandle = null;
                Candlestick entryCandle = null;
                if (m_candleBuf.getMinMaxProfitCandle(out minCandle, out maxCandle, out entryCandle, m_position.isLong(), m_position.entry_date, m_position.entry_price) != 0)
                {
                    result = false;
                    return result;
                }

                double max_profit = m_position.calcProfit(maxCandle.last);
                double min_profit = m_position.calcProfit(minCandle.last);
                double ema_profit = m_position.calcProfit(maxCandle.ema);
                double profit_rate = max_profit / ema_profit * 100.0;
                if (profit_rate < 50.0)
                {
                    Console.WriteLine("## LOSSCUT ##. All-Profit is LOW. profit={0:0} max_profit={1:0} {2} ema_profit={3:0} {4:0.0}%", profit, max_profit, maxCandle.timestamp, ema_profit, profit_rate);
                    result = true;
                    return result;
                }
                else if ((profit_rate >= 50.0) && (maxCandle.timestamp != curCandle.timestamp))
                {
                    Console.WriteLine("## LOSSCUT ##. Max-Profit is NEAR EMA. profit={0:0} max_profit={1:0} {2} ema_profit={3:0} {4:0.0}%", profit, max_profit, maxCandle.timestamp, ema_profit, profit_rate);
                    result = true;
                    return result;
                }
                else
                {
                    Console.WriteLine("## CONTINUE ##. profit={0:0} max_profit={1:0} {2} ema_profit={3:0} {4:0.0}%", profit, max_profit, maxCandle.timestamp, ema_profit, profit_rate);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = false;
            }
            finally
            {
            }
            return result;
        }





        // Entryが約定したかチェック
        public async Task<int> checkEntryActive()
        {
            int result = 0;
            try
            {
                if (!m_position.isNone())
                {
                    result = 1;
                    return result;
                }
                // NONEポジションの場合

                if (!m_position.isEntryOrdered())
                {
                    result = 1;
                    return result;
                }
                // ENTRY注文済みの場合

                GetParentOrderResponse responce = await SendParentOrder.getParentOrderOCO(m_authBitflyer, m_config.product_bitflyer, m_position.entry_id);
                if (responce == null)
                {
                    //Console.WriteLine("Order is not completed.  Order ID = {0}", m_position.entry_id);
                    result = 1;
                    return result;
                }

                m_position.entry_page_id = responce.page_id;
                m_position.entry_parent_id = responce.parent_order_id;

                if (responce.children != null)
                {
                    if (m_position.entry_child_ids == null || m_position.entry_child_ids.Count() <= 0)
                    {
                        postSlack(string.Format("checkEntryActive. Parent Order ID = {0}, {1} state={2}", m_position.entry_id, m_position.entry_parent_id, responce.parent_order_state));
                        foreach (GetChildOrderResponse child in responce.children)
                        {
                            if (child == null)
                            {
                                continue;
                            }
                            if (m_position.entry_child_ids == null)
                            {
                                m_position.entry_child_ids = new List<string>();
                                if (m_position.entry_child_ids == null)
                                {
                                    result = -1;
                                    return result;
                                }
                            }

                            m_position.entry_child_ids.Add(child.child_order_acceptance_id);
                            postSlack(string.Format("checkEntryActive. Child Order ID = {0}", child.child_order_acceptance_id));
                        }
                    }
                }

                if (responce.parent_order_state == "NONE")
                {
                    //postSlack(string.Format("Order is nothing. Order ID = {0}", m_position.entry_id));
                    result = -1;
                    return result;
                }



                if (responce.parent_order_state == "ACTIVE")
                {
                    postSlack(string.Format("Order is active. Order ID = {0}, {1}", m_position.entry_id, m_position.entry_parent_id));

                    m_position.entryActive();
                    result = 0;
                    return result;
                }
                else if (responce.parent_order_state == "CANCELD")
                {
                    postSlack(string.Format("Order is canceld. Order ID = {0}, {1}", m_position.entry_id, m_position.entry_parent_id));
                    result = 1;
                    return result;
                }
                else if (responce.parent_order_state == "COMPLETED")
                {
                    // 注文確定
                    postSlack(string.Format("Order is completed. entry_price={0} id={1},{2}", responce.average_price, m_position.entry_id, m_position.entry_parent_id));
                    m_position.entry(responce.average_price, responce.side);
                    result = 0;
                    return result;
                }
                else if (responce.parent_order_state == "REJECTED")
                {
                    postSlack(string.Format("Order is rejected. Order ID = {0},{1}", m_position.entry_id, m_position.entry_parent_id));
                    m_position.entryRecject();
                    m_position = new Position();
                    m_posArray.Add(m_position);

                    result = -1;
                    return result;
                }
                else
                {
                    postSlack(string.Format("Order is not completed. Order ID = {0},{1}", m_position.entry_id, m_position.entry_parent_id));
                    result = 1;
                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = -1;
            }
            finally
            {
            }
            return result;
        }

        public async Task<int> checkEntryCompleted()
        {
            int result = 0;
            try
            {
                if (!m_position.isNone())
                {
                    result = 1;
                    return result;
                }
                // NONEポジションの場合

                if (!m_position.isEntryActive())
                {
                    result = 1;
                    return result;
                }
                // ENTRY注文がアクティブの場合

                GetParentOrderResponse responce = await SendParentOrder.getParentOrderOCO(m_authBitflyer, m_config.product_bitflyer, m_position.entry_id);
                if (responce == null)
                {
                    //Console.WriteLine("Order is not completed.  Order ID = {0}", m_position.entry_id);
                    result = 1;
                    return result;
                }

                if (responce.parent_order_state == "NONE")
                {
                    //postSlack(string.Format("Order is nothing. Order ID = {0}", m_position.entry_id));
                    result = -1;
                    return result;
                }

                if (responce.parent_order_state == "ACTIVE")
                {
                    //postSlack(string.Format("Order is active. Order ID = {0}", m_position.entry_id));
                    result = 1;
                    return result;
                }
                else if (responce.parent_order_state == "CANCELD")
                {
                    postSlack(string.Format("Order is canceld. Order ID = {0}", m_position.entry_id));
                    result = 1;
                    return result;
                }
                else if (responce.parent_order_state == "COMPLETED")
                {
                    // 注文確定
                    postSlack(string.Format("Order is completed. entry_price={0} id={1}", responce.average_price, m_position.entry_id));
                    m_position.entry(responce.average_price, responce.side);
                    result = 0;
                    return result;
                }
                else if (responce.parent_order_state == "REJECTED")
                {
                    postSlack(string.Format("Order is rejected. Order ID = {0}", m_position.entry_id));
                    result = -1;
                    return result;
                }
                else
                {
                    postSlack(string.Format("Order is not completed. Order ID = {0}", m_position.entry_id));
                    result = 1;
                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = -1;
            }
            finally
            {
            }
            return result;
        }

        public async Task<int> tryCnacelOrder()
        {
            int result = 0;
            try
            {
                if (!m_position.isNone())
                {
                    result = 1;
                    return result;
                }
                // NONEポジションの場合

                if (!m_position.isEntryOrdered())
                {
                    result = 1;
                    return result;
                }
                // ENTRY注文済みの場合

				bool isCancel = false;//isConditionCancelHige();
                if (!isCancel)
                {
                    result = 1;
                    return result;
                }

                int responce = await SendParentOrder.cancelParentOrderAcceptance(m_authBitflyer, m_config.product_bitflyer, m_position.entry_id);
                if (responce != 0)
                {
                    //Console.WriteLine("Order is not canceled.");
                    postSlack(string.Format("Cancel Order is failed. id={0}", m_position.entry_id));
                    result = 1;
                    return result;
                }

                string state = await SendParentOrder.getParentOrderState(m_authBitflyer, m_config.product_bitflyer, m_position.entry_page_id);
                if (state != "CANCELED")
                {
                    postSlack(string.Format("Cancel Commit is failed. id={0} state={1}", m_position.entry_id, state));
                    result = 1;
                    return result;
                }


                // 注文キャンセル確定
                postSlack(string.Format("Cancel Order is succeed. id={0}", m_position.entry_id));

                m_position.cancel();

                m_position = new Position();
                m_posArray.Add(m_position);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = -1;
            }
            finally
            {
            }
            return result;
        }

		public async Task<int> tryEntryOrder(double next_open)
        {
            int result = 0;
            try
            {
                if (!m_position.isNone())
                {
                    if (!m_position.isReserved())
                    {
                        result = 1;
                        return result;
                    }
                }

                Candlestick curCandle = m_candleBuf.getLastCandle();
                if (curCandle == null)
                {
                    result = -1;
                    return result;
                }


                bool isLong = isConditionLongEntryFL(next_open, false);
                bool isShort = isConditionShortEntryFL(next_open, false);
                //isLong = false;
                //isShort = false;

                const double ema_touch_play = 0.0;
                bool isGolden = false;
                bool isBeg = false;
                int back_cnt = 0;
                double cur_ema_length = 0.0;
                bool isTouchEma = false;
                bool isTouchEmaSub = false;
                if (m_candleBuf.getEMACrossState(out isGolden, out isBeg, out back_cnt, out cur_ema_length, out isTouchEma, out isTouchEmaSub, 0.6, ema_touch_play) != 0)
                {
                    result = -1;
                    return result;
                }

                const double disparity_border = 4.9;

                bool isTouch = isTouchEma;

                if (m_position.isNone())
                {
                    // NONEポジションの場合


                    if (isLong || isShort)
                    {
                        bool isActive = await Trade.isActive(m_authBitflyer, m_config.product_bitflyer);
                        if (isActive)
                        {
                            postSlack(string.Format("cant's Trade. Orders or Positions is exists. isLong={0} isShort={1}"
                                , isLong
                                , isShort
                            ));
                            result = -1;
                            return result;
                        }

                        if (isMaintenanceBitflyer(curCandle.timestamp))
                        {
                            result = 1;
                            return result;
                        }
                    }
                     
					if (isLong && isGolden && (curCandle.disparity_rate < disparity_border) )
                    {
                        //Console.WriteLine("Try Long Entry Order.");                  
                        
						if (isBeg && isTouch)
                        {
                            //SendChildOrderResponse retObj = null;
                            //int retry_cnt = 0;
                            //while (true)
                            //{
                            //    retry_cnt++;
                            //    retObj = await SendChildOrder.BuyMarket(m_authBitflyer, m_config.product_bitflyer, m_config.amount);
                            //    if (retObj == null)
                            //    {
                            //        //System.Threading.Thread.Sleep(1000);
                            //        //if (retry_cnt<=1)
                            //        //{
                            //        //    m_config.amount = m_config.amount - 0.01;
                            //        //    postSlack(string.Format("failed to Long Entry Order. Reduce amount. Retry. retry_cnt={0} amount={1}", retry_cnt, m_config.amount));
                            //        //    continue;
                            //        //}
                            //        postSlack(string.Format("failed to Long Entry Order. retry_cnt={0} amount={1}", retry_cnt, m_config.amount));
                            //        result = -1;
                            //        return result;
                            //    }
                            //    break;
                            //}
                            //// 注文成功

                            //m_position.entryLongOrder(retObj.child_order_acceptance_id, curCandle.timestamp, m_config.amount);

                            //postSlack(string.Format("{0} Long Entry Order ID = {1} isGold={2} bkCnt={3} isBeg={4}", curCandle.timestamp, retObj.child_order_acceptance_id, isGolden, back_cnt, isBeg));
                        }
						else if( isBeg || isTouch )
                        {
                            // LONG予約
                            m_position.reserveLongOrder();
                            postSlack(string.Format("{0} Long Reserved. ema={1:0} diff={2:0} isGold={3} bkCnt={4} isBeg={5} isEma={6}", curCandle.timestamp, curCandle.ema, curCandle.last - curCandle.ema, isGolden, back_cnt, isBeg, isTouch));
                        }
                    }
					else if (isShort && !isGolden && (curCandle.disparity_rate > -disparity_border) )
                    {
                        //Console.WriteLine("Try Short Entry Order.");
                        
						if (isBeg && isTouch)
                        {
                            //SendChildOrderResponse retObj = null;
                            //int retry_cnt = 0;
                            //while (true)
                            //{
                            //    retry_cnt++;
                            //    retObj = await SendChildOrder.SellMarket(m_authBitflyer, m_config.product_bitflyer, m_config.amount); ;
                            //    if (retObj == null)
                            //    {
                                    
                            //        //System.Threading.Thread.Sleep(1000);
                            //        //if (retry_cnt <= 1)
                            //        //{
                            //        //    m_config.amount = m_config.amount - 0.01;
                            //        //    postSlack(string.Format("failed to Short Entry Order. Reduce amount. Retry. retry_cnt={0} amount={1}", retry_cnt, m_config.amount));
                            //        //    continue;
                            //        //}
                            //        postSlack(string.Format("failed to Short Entry Order. retry_cnt={0} amount={1}", retry_cnt, m_config.amount));
                            //        result = -1;
                            //        return result;
                            //    }
                            //    break;
                            //}
                            //// 注文成功

                            //m_position.entryShortOrder(retObj.child_order_acceptance_id, curCandle.timestamp, m_config.amount);
                            //postSlack(string.Format("{0} Short Entry Order ID = {1} isDead={2} bkCnt={3} isBeg={4}", curCandle.timestamp, retObj.child_order_acceptance_id, !isGolden, back_cnt, isBeg));
                        }
						else if (isBeg || isTouch)
                        {
                            // SHORT予約
                            m_position.reserveShortOrder();
                            postSlack(string.Format("{0} Short Reserved. ema={1:0} diff={2:0} isDead={3} bkCnt={4} isBeg={5} isEma={6}", curCandle.timestamp, curCandle.ema, curCandle.last - curCandle.ema, !isGolden, back_cnt, isBeg, isTouch));
                        }
                    }
                }
                else
                {
                    if (m_position.isLongReserved())
                    {
                        if (isGolden)
                        {
							if (curCandle.disparity_rate >= disparity_border)
                            {
								// LONG予約キャンセル
                                m_position.cancelReserveOrder();
                                postSlack(string.Format("{0} Cancel Long Reserved. DispartyRate is Over. rate={1:0.00}.", curCandle.timestamp, curCandle.disparity_rate));

                                result = -1;
                                return result;
                            }

							if (isBeg && isTouch)
                            {
                                SendChildOrderResponse retObj = null;
                                int retry_cnt = 0;
                                while (true)
                                {
                                    retry_cnt++;
                                    retObj = await SendChildOrder.BuyMarket(m_authBitflyer, m_config.product_bitflyer, m_config.amount);
                                    if (retObj == null)
                                    {
                                        //System.Threading.Thread.Sleep(1000);
                                        //if (retry_cnt <= 1)
                                        //{
                                        //    m_config.amount = m_config.amount - 0.01;
                                        //    postSlack(string.Format("failed to Long(Reserved) Entry Order. Reduce amount. Retry. retry_cnt={0} amount={1}", retry_cnt, m_config.amount));
                                        //    continue;
                                        //}
                                        postSlack(string.Format("failed to Long(Reserved) Entry Order. retry_cnt={0} amount={1}", retry_cnt, m_config.amount));
                                        result = -1;
                                        return result;
                                    }
                                    break;
                                }
                                // 注文成功

                                m_position.entryLongOrder(retObj.child_order_acceptance_id, curCandle.timestamp, m_config.amount);

                                postSlack(string.Format("{0} Long(Reserved) Entry Order ID = {1} isGold={2} bkCnt={3} isBeg={4} isEma={5}", curCandle.timestamp, retObj.child_order_acceptance_id, isGolden, back_cnt, isBeg, isTouch));
                            }
                            else if (!isBeg && !isTouch)
                            {
                                // LONG予約キャンセル
                                m_position.cancelReserveOrder();
                                postSlack(string.Format("{0} Cancel Long Reserved. isLong={1} isGold={2} isBeg={3} isEma={4}", curCandle.timestamp, isLong, isGolden, isBeg, isTouch));
                            }
                        }
                        else
                        {
                            // LONG予約キャンセル
                            m_position.cancelReserveOrder();
                            postSlack(string.Format("{0} Cancel Long Reserved. isLong={1} isGold={2} isBeg={3} isEma={4}", curCandle.timestamp, isLong, isGolden, isBeg, isTouch));
                        }
                    }
                    else if (m_position.isShortReserved())
                    {
                        if (!isGolden)
                        {
							if (curCandle.disparity_rate <= -disparity_border)
                            {
                                // SHORT予約キャンセル
                                m_position.cancelReserveOrder();
                                postSlack(string.Format("{0} Cancel Short Reserved. DispartyRate is Over. rate={1:0.00}.", curCandle.timestamp, curCandle.disparity_rate));

                                result = -1;
                                return result;
                            }

							if (isBeg && isTouch)
                            {

                                SendChildOrderResponse retObj = null;
                                int retry_cnt = 0;
                                while (true)
                                {
                                    retry_cnt++;
                                    retObj = await SendChildOrder.SellMarket(m_authBitflyer, m_config.product_bitflyer, m_config.amount); ;
                                    if (retObj == null)
                                    {

                                        //System.Threading.Thread.Sleep(1000);
                                        //if (retry_cnt <= 1)
                                        //{
                                        //    m_config.amount = m_config.amount - 0.01;
                                        //    postSlack(string.Format("failed to Short(Reserved) Entry Order. Reduce amount. Retry. retry_cnt={0} amount={1}", retry_cnt, m_config.amount));
                                        //    continue;
                                        //}
                                        postSlack(string.Format("failed to Short(Reserved) Entry Order. retry_cnt={0} amount={1}", retry_cnt, m_config.amount));
                                        result = -1;
                                        return result;
                                    }
                                    break;
                                }
                                // 注文成功

                                m_position.entryShortOrder(retObj.child_order_acceptance_id, curCandle.timestamp, m_config.amount);


                                postSlack(string.Format("{0} Short(Reserved) Entry Order ID = {1} isDead={2} bkCnt={3} isBeg={4} isEma={5}", curCandle.timestamp, retObj.child_order_acceptance_id, !isGolden, back_cnt, isBeg, isTouch));
                            }
                            else if (!isBeg && !isTouch)
                            {
                                // SHORT予約キャンセル
                                m_position.cancelReserveOrder();
                                postSlack(string.Format("{0} Cancel Short Reserved. isShort={1} isDead={2} isBeg={3} isEma={4}", curCandle.timestamp, isShort, !isGolden, isBeg, isTouch));
                            }
                        }
                        else
                        {
                            // SHORT予約キャンセル
                            m_position.cancelReserveOrder();
                            postSlack(string.Format("{0} Cancel Short Reserved. isShort={1} isDead={2} isBeg={3} isEma={4}", curCandle.timestamp, isShort, !isGolden, isBeg, isTouch));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = -1;
            }
            finally
            {
                m_isDotenShort = false;
                m_isDotenLong = false;
            }
            return result;
        }

        private bool isMaintenanceBitflyer(string timestamp)
        {

            bool result = false;
            try
            {
                DateTime dateTime = DateTime.Parse(timestamp);
                int Hour = dateTime.Hour;
                int Minute = dateTime.Minute;

                if (Hour == 4 && (0 <= Minute && Minute <= 16))
                {
                    result = true;
                    return result;
                }

                if (Hour == 3 && Minute == 59)
                {
                    result = true;
                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = false;
            }
            finally
            {
            }
            return result;

        }


        private int tryEntryOrderTest
        (
            ref int long_entry_cnt, 
            ref int short_entry_cnt,
            double next_open
        )
        {
            int result = 0;
            try
            {
                if (!m_position.isNone())
                {
                    if (!m_position.isReserved())
                    {
                        result = 1;
                        return result;
                    }
                }

                Candlestick curCandle = m_candleBuf.getLastCandle();
                if (curCandle == null)
                {
                    result = -1;
                    return result;
                }



                bool isLong =  isConditionLongEntryFL(next_open);
                bool isShort = isConditionShortEntryFL(next_open);

                if (isLong || isShort)
                {
                    if (isMaintenanceBitflyer(curCandle.timestamp))
                    {
                        result = 1;
                        return result;
                    }
                }

                const double ema_touch_play = 0.0;
                bool isGolden = false;
                bool isBeg = false;
                int back_cnt = 0;
                double cur_ema_length = 0.0;
                bool isTouchEma = false;
                bool isTouchEmaSub = false;
                if (m_candleBuf.getEMACrossState(out isGolden, out isBeg, out back_cnt, out cur_ema_length, out isTouchEma, out isTouchEmaSub, 0.6, ema_touch_play) != 0)
                {
                    result = -1;
                    return result;
                }

                bool isTouch = isTouchEma/* && isTouchEmaSub*/;

                if (m_position.isNone())
                {
					// NONEポジションの場合


					if (isLong && isGolden)
					{
                        if (isBeg && isTouch)
                        {

                            //// 注文成功
                            //string long_id = string.Format("BT_LONG_ENTRY_{0:D8}", long_entry_cnt);

                            //m_position.entryLongOrder(long_id, curCandle.timestamp, m_config.amount);

                            //postSlack(string.Format("{0} Long Entry Order ID = {1} isGold={2} bkCnt={3} isBeg={4} isEma={5}", curCandle.timestamp, long_id, isGolden, back_cnt, isBeg, isTouch), true);

                            //long_entry_cnt++;

                        }
                        else if ( isBeg || isTouch)
                        {
							// LONG予約
							m_position.reserveLongOrder();
							postSlack(string.Format("{0} Long Reserved. ema={1:0} diff={2:0} isGold={3} bkCnt={4} isBeg={5} isEma={6}", curCandle.timestamp, curCandle.ema, curCandle.last - curCandle.ema, isGolden, back_cnt, isBeg, isTouch), true);
						}
					}
					else if (isShort && !isGolden)
					{
                        if (isBeg && isTouch)
                        {
                            //// 注文成功
                            //string short_id = string.Format("BT_SHORT_ENTRY_{0:D8}", short_entry_cnt);

                            //m_position.entryShortOrder(short_id, curCandle.timestamp, m_config.amount);

                            //postSlack(string.Format("{0} Short Entry Order ID = {1} isDead={2} bkCnt={3} isBeg={4} isEma={5}", curCandle.timestamp, short_id, !isGolden, back_cnt, isBeg, isTouch), true);

                            //short_entry_cnt++;

                        }
                        else if (isBeg || isTouch)
                        {
                            // SHORT予約
                            m_position.reserveShortOrder();
                            postSlack(string.Format("{0} Short Reserved. ema={1:0} diff={2:0} isDead={3} bkCnt={4} isBeg={5} isEma={6}", curCandle.timestamp, curCandle.ema, curCandle.last - curCandle.ema, !isGolden, back_cnt, isBeg, isTouch), true);

                        }
					}
                }
                else
                {
                    if (m_position.isLongReserved())
                    {
						if (isGolden)
                        {
                            if(isBeg && isTouch)
                            {

                                // 注文成功
                                string long_id = string.Format("BT_LONG_ENTRY_{0:D8}", long_entry_cnt);

                                m_position.entryLongOrder(long_id, curCandle.timestamp, m_config.amount);

                                postSlack(string.Format("{0} Long(Reserved) Entry Order ID = {1} isGold={2} bkCnt={3} isBeg={4} isEma={5}", curCandle.timestamp, long_id, isGolden, back_cnt, isBeg, isTouch), true);

                                long_entry_cnt++;
                            }
                            else if (!isBeg && !isTouch)
                            {
                                // LONG予約キャンセル
                                m_position.cancelReserveOrder();
                                postSlack(string.Format("{0} Cancel Long Reserved. isLong={1} isGold={2} isBeg={3} isEma={4}", curCandle.timestamp, isLong, isGolden, isBeg, isTouch), true);
                            }
                        }
                        else
                        {
                            // LONG予約キャンセル
                            m_position.cancelReserveOrder();
                            postSlack(string.Format("{0} Cancel Long Reserved. isLong={1} isGold={2} isBeg={3} isEma={4}", curCandle.timestamp, isLong, isGolden, isBeg, isTouch), true);

                        }
                    }
                    else if (m_position.isShortReserved())
                    {
						if (!isGolden)
                        {
                            if (isBeg && isTouch)
                            {
                                // 注文成功
                                string short_id = string.Format("BT_SHORT_ENTRY_{0:D8}", short_entry_cnt);

                                m_position.entryShortOrder(short_id, curCandle.timestamp, m_config.amount);

                                postSlack(string.Format("{0} Short(Reserved) Entry Order ID = {1} isDead={2} bkCnt={3} isBeg={4} isEma={5}", curCandle.timestamp, short_id, !isGolden, back_cnt, isBeg, isTouch), true);

                                short_entry_cnt++;
                            }
                            else if (!isBeg && !isTouch)
                            {
                                // SHORT予約キャンセル
                                m_position.cancelReserveOrder();
                                postSlack(string.Format("{0} Cancel Short Reserved. isShort={1} isDead={2} isBeg={3} isEma={4}", curCandle.timestamp, isShort, !isGolden, isBeg, isTouch), true);
                            }
                        }
                        else
                        {
                            // SHORT予約キャンセル
                            m_position.cancelReserveOrder();
                            postSlack(string.Format("{0} Cancel Short Reserved. isShort={1} isDead={2} isBeg={3}", curCandle.timestamp, isShort, !isGolden, isBeg), true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = -1;
            }
            finally
            {
                m_isDotenShort = false;
                m_isDotenLong = false;
                m_isDoten = false;
            }
            return result;
        }

		public bool isConditionLongLosscut()
        {
            bool result = false;
            try
            {
                if (m_candleBuf == null)
                {
                    result = false;
                    return result;
                }

                Candlestick curCandle = m_candleBuf.getLastCandle();
                if (curCandle == null)
                {
                    result = false;
                    return result;
                }
                
				{               
					double profit = curCandle.last - m_position.entry_price;
					if (profit <= m_config.losscut_value)
					{                  
                        if (!m_candleBuf.isHangAround(curCandle.last, Math.Abs(m_config.losscut_value), 3))
                        {
                            if ((curCandle.last - curCandle.ema) > 0.0)
                            {
                                result = true;
                                return result;
                            }
						}
					}

					if (profit <= -3000.0)
					{
						result = true;
                        return result;
					}
				}

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = false;
            }
            finally
            {
            }
            return result;
        }

        public bool isConditionShortLosscut()
        {
            bool result = false;
            try
            {
                if (m_candleBuf == null)
                {
                    result = false;
                    return result;
                }

                Candlestick curCandle = m_candleBuf.getLastCandle();
                if (curCandle == null)
                {
                    result = false;
                    return result;
                }
                
				{               
					double profit = m_position.entry_price - curCandle.last;
					if (profit <= m_config.losscut_value)
					{
                        if (!m_candleBuf.isHangAround(curCandle.last, Math.Abs(m_config.losscut_value), 3))
                        {
                            if ((curCandle.last - curCandle.ema) < 0.0)
                            {
                                result = true;
                                return result;
                            }
						}
					}
                    
					if (profit <= -3000.0)
                    {
                        result = true;
                        return result;
                    }
				}
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = false;
            }
            finally
            {
            }
            return result;
        }

        public bool isConditionShortEntryFL(double next_open, bool onlyConsole = true)
        {
            bool result = false;
            try
            {
                if (m_upBreakCnt > 0)
                {
                    m_upBreakCnt = m_upBreakCnt - 2;
                }

                if (m_candleBuf == null)
                {
                    result = false;
                    return result;
                }

                if (!m_candleBuf.isFullBuffer())
                {
                    result = false;
                    return result;
                }

                Candlestick curCandle = m_candleBuf.getLastCandle();
                if (curCandle == null)
                {
                    result = false;
                    return result;
                }

                double position = curCandle.last - m_frontlineShort;
                //double position = next_open - m_frontlineShort;


                //int posArrayNum = m_posArray.Count();
                //if (posArrayNum >= 2)
                //{
                //    Position lastPosition = m_posArray[posArrayNum - 2];

                //    DateTime ago = DateTime.Parse(lastPosition.exit_date);
                //    DateTime now = DateTime.Parse(curCandle.timestamp);
                //    TimeSpan span = now - ago;
                //    if (span.TotalMinutes < 5)
                //    {
                //        Console.WriteLine("not need short. last={0:0} span={1:0}", curCandle.last, span.Minutes);
                //        result = false;
                //        return result;
                //    }

                //}            

                double threshold = (Math.Abs(m_config.losscut_value) + curCandle.vola_ma)*1.1;
                const int past_num = 2;

                // フロントライン付近でウロウロしてる
                if (m_candleBuf.isHangAround(m_frontlineShort, threshold, past_num, 1))
                {
                    //postSlack(string.Format("** hang around Short-front-line **. pos={0:0} delta={1:0} top={2:0} dwn={3:0}", position, threshold, m_frontlineShort + threshold, m_frontlineShort - threshold), true);
                    result = false;
                    return result;
                }

                if (position <= 0)
                {
                    // 現在値がフロントラインより下

                    if (Math.Abs(position) <= threshold)
                    {
                        // ENTRYしない
                        result = false;
                        return result;
                    }

                    // ENTRYする
                    //postSlack(string.Format("!! DOWN-BREAK(Short) !!. pos={0:0}", position), onlyConsole);
                }
                else
                {
                    // 現在値がフロントラインより上

                    //postSlack(string.Format("!! UP-BREAK(Short) !!. pos={0:0} up_cnt={1}", position, m_upBreakCnt), true);    
                    if (Math.Abs(position) > threshold)
                    {
                        m_frontlineShort += (position * 0.5);
                        //postSlack(string.Format("** renewal Short-front-line **. pos={0:0} delta={1:0} top={2:0} dwn={3:0}", position, threshold, m_frontlineShort + threshold, m_frontlineShort - threshold), true);
                    }

                    result = false;
                    return result;

                }

                //const double ema_cross_play = 700.0;//1500.0;//1400.0;
                //if (curCandle.isCrossEMA(ema_cross_play))
                //{
                //    Console.WriteLine("NOT need short. last={0:0} pos={1:0} ema_diff={2:0} ", curCandle.last, position, curCandle.last - curCandle.ema);
                //    result = false;
                //    return result;
                //}

                // ENTRY
                //Console.WriteLine("need short. last={0:0} pos={1:0} front={2:0}", curCandle.last, position, m_frontlineShort);
                result = true;
                return result;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = false;
            }
            finally
            {
            }
            return result;
        }

        public bool isConditionLongEntryFL(double next_open, bool onlyConsole = true)
        {
            bool result = false;
            try
            {
				//Console.WriteLine("isConditionLongEntryFL");
                if (m_candleBuf == null)
                {
					//Console.WriteLine("isConditionLongEntryFL false0");
                    result = false;
                    return result;
                }

                if (!m_candleBuf.isFullBuffer())
                {
					//Console.WriteLine("isConditionLongEntryFL false1");
                    result = false;
                    return result;
                }

                Candlestick curCandle = m_candleBuf.getLastCandle();
                if (curCandle == null)
                {
					//Console.WriteLine("isConditionLongEntryFL false2");
                    result = false;
                    return result;
                }

                double position = curCandle.last - m_frontlineLong;
                //double position = next_open - m_frontlineLong;

                double threshold = (Math.Abs(m_config.losscut_value) + curCandle.vola_ma) * 1.1;
                const int past_num = 2;

                // フロントライン付近でウロウロしてる
                if (m_candleBuf.isHangAround(m_frontlineLong, threshold, past_num, 1))
                {
                    //postSlack(string.Format("** hang around Long-front-line **. pos={0:0} delta={1:0} top={2:0} dwn={3:0}", position, threshold, m_frontlineLong + threshold, m_frontlineLong - threshold), true);
					//Console.WriteLine("isConditionLongEntryFL false3");
					result = false;
                    return result;
                }

                if ( position <= 0)
                {
                    // 現在値がフロントラインより下
                    //postSlack(string.Format("!! DOWN-BREAK(Long) !!. pos={0:0}", position), true);

                    if (Math.Abs(position) > threshold)
                    {
                        m_frontlineLong += (position * 0.5);
                        //postSlack(string.Format("** renewal Long-front-line **. pos={0:0} delta={1:0} top={2:0} dwn={3:0}", position, threshold, m_frontlineLong + threshold, m_frontlineLong - threshold), true);

                    }
					//Console.WriteLine("isConditionLongEntryFL false4");
                    result = false;
                    return result;
                }
                else
                {
                    // 現在値がフロントラインより上

                    if (Math.Abs(position) <= threshold)
                    {
                        // ENTRYしない
						//Console.WriteLine("isConditionLongEntryFL false5");
                        result = false;
                        return result;
                    }

                    // ENTRYする
                    //postSlack(string.Format("!! UP-BREAK(Long) !!. pos={0:0}", position), onlyConsole);
                }

                //const double ema_cross_play = 700.0;//1500.0;//1400.0;
                //if (curCandle.isCrossEMA(ema_cross_play))
                //{
                //    Console.WriteLine("NOT need long. last={0:0} pos={1:0} ema_diff={2:0} ", curCandle.last, position, curCandle.last - curCandle.ema);
                //    result = false;
                //    return result;
                //}

                // ENTRY
                //Console.WriteLine("need long. last={0:0} pos={1:0} ", curCandle.last, position);
                result = true;
                return result;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = false;
            }
            finally
            {
            }
            return result;
        }

        public bool isConditionShortExitFL(ref int short_lc_cnt, double next_open, bool onlyConsole = true)
        {
            bool result = false;
            try
            {
                if (m_candleBuf == null)
                {
                    result = false;
                    return result;
                }

                Candlestick curCandle = m_candleBuf.getLastCandle();
                if (curCandle == null)
                {
                    result = false;
                    return result;
                }


                double profit = m_frontlineShort - curCandle.last;

                double forward_rate = 0.5;
                double forward_rate2 = 0.5;// 0.4 + rate;//  0.5 + 0.1 * rate;//

                double frontline_ahead = m_config.frontline_ahead; // / Math.Pow(2.0, -m_position.frontline_fwd_num);
                //double frontline_ahead = (m_config.frontline_ahead*0.5) + ( (m_config.frontline_ahead*0.5) / Math.Pow(2.0, -m_position.frontline_fwd_num));
                double frontline_ahead2 = frontline_ahead * 2.0;
                if (m_position.frontline_fwd_num <= 0)
                {
                    frontline_ahead2 = frontline_ahead;
                }

                bool isGolden = false;
                bool isBeg = false;
                int back_cnt = 0;
                double cur_ema_length = 0.0;
                bool isTouchEma = false;
                bool isTouchEmaSub = false;
                if (m_candleBuf.getEMACrossState(out isGolden, out isBeg, out back_cnt, out cur_ema_length, out isTouchEma, out isTouchEmaSub) != 0)
                {
                    result = false;
                    return result;
                }

                if (Math.Abs(m_frontlineShort - m_position.entry_price) <= double.Epsilon)
                {
                    // フロントラインがENTRY位置と同じ場合

                    if (isConditionShortLosscut() /*|| isGolden || (!isGolden && !isBeg)*/ )
                    {
                        // EXIT
                        postSlack(string.Format("## front-line is break ##. last={0:0} pos={1:0} front={2:0} ", curCandle.last, profit, m_frontlineShort), onlyConsole);
                        result = true;

                        // 最前線を後退
                        m_frontlineShort = curCandle.last;

                        short_lc_cnt++;
                    }
                    else if (profit >= frontline_ahead)
                    {
                        // 最前線を前進
                        double forward = Math.Round(profit * forward_rate);
                        m_frontlineShort = m_frontlineShort - forward;
                        m_position.frontline_fwd_num++;
                        // SHORT継続
                        postSlack(string.Format("## front-line is forward ##. last={0:0} pos={1:0} front={2:0} fwd={3:0} rate={4:0.00} ahead={5:0}", curCandle.last, profit, m_frontlineShort, forward, forward_rate, frontline_ahead), onlyConsole);
                        result = false;
                    }
                    else if (profit >= (frontline_ahead * 0.1))
                    {
                        // 最前線を前進
                        double forward = Math.Round(profit * 0.1);
                        m_frontlineShort = m_frontlineShort - forward;
                        // SHORT継続
                        postSlack(string.Format("## front-line is bit-forward ##. last={0:0} pos={1:0} front={2:0} fwd={3:0}", curCandle.last, profit, m_frontlineShort, forward), onlyConsole);
                        result = false;
                    }
                    else
                    {
                        //const int past_num = 10;
                        //if ((profit >= 0) && (m_candleBuf.isFullUpPast(m_position.entry_price, past_num, 1)))
                        //{
                        //	postSlack(string.Format("## front-line is escape ##. last={0:0} pos={1:0} front={2:0}", curCandle.last, profit, m_frontlineShort), onlyConsole);
                        //	result = true;

                        //	// 最前線を後退
                        //	m_frontlineShort = curCandle.last;
                        //}
                        //else
                        {
                            // 最前線を維持
                            // SHORT継続
                            //Console.WriteLine("## front-line is keep ##. last={0:0} pos={1:0} front={2:0} ", curCandle.last, profit, m_frontline);
                            result = false;
                        }
                    }
                }
                else
                {
                    // 何回か最前線を前進させている場合
                    if (profit <= 0 )
                    //if((m_position.entry_price - curCandle.last) <= 0)
                    {
                        // EXIT
                        postSlack(string.Format("## front-line is back ##. last={0:0} pos={1:0} front={2:0} ", curCandle.last, profit, m_frontlineShort), onlyConsole);
                        result = true;

                        // 最前線を後退
                        m_frontlineShort = curCandle.last;
                    }
                    else if (isGolden)
                    {
                        // EXIT
                        postSlack(string.Format("## golden-cross occurred  ##. last={0:0} pos={1:0} front={2:0}", curCandle.last, profit, m_frontlineLong), onlyConsole);
                        result = true;

                        // 最前線を後退
                        m_frontlineShort = curCandle.last;
                    }
                    else if (!isGolden && !isBeg)
                    {
                        postSlack(string.Format("## dead-cross is about to end ##. last={0:0} pos={1:0} front={2:0} bkCnt={3}", curCandle.last, profit, m_frontlineLong, back_cnt), onlyConsole);
                        result = true;

                        // 最前線を後退
                        m_frontlineShort = curCandle.last;
                    }
                    else if (profit >= frontline_ahead2)
                    {
                        // 最前線を前進
                        double forward = Math.Round(profit * forward_rate2);
                        m_frontlineShort = m_frontlineShort - forward;
                        m_position.frontline_fwd_num++;
                        // SHORT継続
                        postSlack(string.Format("## front-line is forward ##. last={0:0} pos={1:0} front={2:0} fwd={3:0} rate={4:0.00} ahead={5:0}", curCandle.last, profit, m_frontlineShort, forward, forward_rate2, frontline_ahead2), onlyConsole);
                        result = false;
                    }
                    else
                    {
                        // 最前線を維持
                        // SHORT継続
                        //Console.WriteLine("## front-line is keep ##. last={0:0} pos={1:0} front={2:0} ", curCandle.last, profit, m_frontline);
                        result = false;
                    }
                }


                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = false;
            }
            finally
            {
            }
            return result;
        }




        public bool isConditionLongExitFL(ref int long_lc_cnt, double next_open, bool onlyConsole = true)
        {
            bool result = false;
            try
            {
                if (m_candleBuf == null)
                {
                    result = false;
                    return result;
                }

                Candlestick curCandle = m_candleBuf.getLastCandle();
                if (curCandle == null)
                {
                    result = false;
                    return result;
                }

                double profit = curCandle.last - m_frontlineLong;


                double forward_rate = 0.5;
                double forward_rate2 = 0.5;


                double frontline_ahead = m_config.frontline_ahead;// / Math.Pow(2.0, -m_position.frontline_fwd_num);
                //double frontline_ahead = (m_config.frontline_ahead * 0.5) + ((m_config.frontline_ahead * 0.5) / Math.Pow(2.0, -m_position.frontline_fwd_num));
                double frontline_ahead2 = frontline_ahead * 2.0;
                if (m_position.frontline_fwd_num <= 0)
                {
                    frontline_ahead2 = frontline_ahead;
                }

                bool isGolden = false;
                bool isBeg = false;
                int back_cnt = 0;
                double cur_ema_length = 0.0;
                bool isTouchEma = false;
                bool isTouchEmaSub = false;
                if (m_candleBuf.getEMACrossState(out isGolden, out isBeg, out back_cnt, out cur_ema_length, out isTouchEma, out isTouchEmaSub) != 0)
                {
                    result = false;
                    return result;
                }

                if (Math.Abs(m_frontlineLong - m_position.entry_price) <= double.Epsilon)
                {
					// フロントラインがENTRY位置と同じ場合
					if (isConditionLongLosscut() /*|| !isGolden || (isGolden && !isBeg)*/)
                    {
						// EXIT
						postSlack(string.Format("## front-line is break ##. last={0:0} pos={1:0} front={2:0} ", curCandle.last, profit, m_frontlineLong), onlyConsole);
						result = true;

						// 最前線を後退
						m_frontlineLong = curCandle.last;

						long_lc_cnt++;
					}
					else if (profit >= frontline_ahead)
					{
						// 最前線を前進
						double forward = Math.Round(profit * forward_rate);
						m_frontlineLong = m_frontlineLong + forward;
                        m_position.frontline_fwd_num++;
                        // LONG継続
                        postSlack(string.Format("## front-line is forward ##. last={0:0} pos={1:0} front={2:0} fwd={3:0} rate={4:0.00} ahead={5:0}", curCandle.last, profit, m_frontlineLong, forward, forward_rate, frontline_ahead), onlyConsole);
						result = false;
					}
                    else if (profit >= (frontline_ahead * 0.1))
                    {
                        // 最前線を前進
                        double forward = Math.Round(profit * 0.1);
                        m_frontlineLong = m_frontlineLong + forward;
                        // LONG継続
                        postSlack(string.Format("## front-line is bit-forward ##. last={0:0} pos={1:0} front={2:0} fwd={3:0}", curCandle.last, profit, m_frontlineLong, forward), onlyConsole);
                        result = false;
                    }
                    else
					{
                        //const int past_num = 10;
                        //if ((profit >= 0) && (m_candleBuf.isFullDownPast(m_position.entry_price, past_num, 1)))
                        //{
                        //	postSlack(string.Format("## front-line is escape ##. last={0:0} pos={1:0} front={2:0}", curCandle.last, profit, m_frontlineShort), onlyConsole);
                        //	result = true;

                        //	// 最前線を後退
                        //	m_frontlineLong = curCandle.last;

                        //}
                        //else
                        {
                            // 最前線を維持
                            // LONG継続
                            //Console.WriteLine("## front-line is keep ##. last={0:0} pos={1:0} front={2:0} ", curCandle.last, profit, m_frontlineLong);
                            result = false;
					    }
                    }
                }
                else
                {
                    // 何回か最前線を前進させている場合
                    if (profit <= 0)
                    //if( (curCandle.last-m_position.entry_price) <= 0)
                    {
                        // EXIT
                        postSlack(string.Format("## front-line is back ##. last={0:0} pos={1:0} front={2:0} ", curCandle.last, profit, m_frontlineLong), onlyConsole);
                        result = true;

                        // 最前線を後退
                        m_frontlineLong = curCandle.last;
                    }
                    else if (!isGolden)
                    {
                        // EXIT
                        postSlack(string.Format("## dead-cross occurred  ##. last={0:0} pos={1:0} front={2:0}", curCandle.last, profit, m_frontlineLong), onlyConsole);
                        result = true;

                        // 最前線を後退
                        m_frontlineLong = curCandle.last;
                    }
                    else if (isGolden && !isBeg)
                    {
                        // EXIT
                        postSlack(string.Format("## golden-cross is about to end ##. last={0:0} pos={1:0} front={2:0} bkCnt={3}", curCandle.last, profit, m_frontlineLong, back_cnt), onlyConsole);
                        result = true;

                        // 最前線を後退
                        m_frontlineLong = curCandle.last;
                    }
                    else if (profit >= frontline_ahead2)
                    {
                        // 最前線を前進
                        double forward = Math.Round(profit * forward_rate2);
                        m_frontlineLong = m_frontlineLong + forward;
                        m_position.frontline_fwd_num++;
                        // LONG継続
                        postSlack(string.Format("## front-line is forward ##. last={0:0} pos={1:0} front={2:0} fwd={3:0} rate={4:0.00} ahead={5:0}", curCandle.last, profit, m_frontlineLong, forward, forward_rate2, frontline_ahead2), onlyConsole);
                        result = false;
                    }
                    else
                    {
                        // 最前線を維持
                        // LONG継続
                        //Console.WriteLine("## front-line is keep ##. last={0:0} pos={1:0} front={2:0} ", curCandle.last, profit, m_frontlineLong);
                        result = false;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = false;
            }
            finally
            {
            }
            return result;
        }


    }
}
