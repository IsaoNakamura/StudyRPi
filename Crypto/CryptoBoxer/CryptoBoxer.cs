using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public double m_frontline { get; set; }


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

            m_posArray = null;

            m_min = 0.0;
            m_max = 0.0;
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

            m_frontline = 0.0;

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
        private int applyCandlestick(CandleBuffer candleBuf, ref BitflyerOhlc ohlc)
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

                List<List<double>> candles = ohlc.result.getResult(m_config.periods);

                if (candles == null)
                {
                    Console.WriteLine("ohlc's candle is null. periods={0}", m_config.periods);
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

        private int applyCandlestick(CandleBuffer candleBuf, ref BitflyerOhlc ohlc, int begIdx, int count, bool isCalcIndicator=true)
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

                List<List<double>> candles = ohlc.result.getResult(m_config.periods);

                if (candles == null)
                {
                    Console.WriteLine("ohlc's candle is null. periods={0}", m_config.periods);
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

                        // MAX更新
                        if (candle.boll_high < candle.last)
                        {
                            if (m_max < candle.last)
                            {
                                m_max = candle.last;
                            }
                        }
                        else
                        {
                            m_max = candle.boll_high;
                        }

                        // MIN更新
                        if (candle.boll_low > candle.last)
                        {
                            if (m_min > candle.last)
                            {
                                m_min = candle.last;
                            }
                        }
                        else
                        {
                            m_min = candle.boll_low;
                        }
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
                if (applyCandlestick(m_candleBuf, ref ohlc, 0, m_candleBuf.m_buffer_num) != 0)
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
                while (true)
                {
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
                            if (ticker_spot != null)
                            {
                                // FXと現物の価格乖離率を算出
                                double spot_last = ticker_spot.ltp;
                                double disparity_rate = (curCandle.last - spot_last) / spot_last * 100.0;
                                curCandle.disparity_rate = disparity_rate;
                            }

                            // ENTRY/EXITロジック
                            await tryEntryOrder(cur_value);
                            await tryExitOrder();

                            if (m_position.isEntryCompleted())
                            {
                                Console.WriteLine("closed candle. timestamp={0},profit={1},last={2:0},trend={3},ema={4:0},vola={5:0},vola_rate={6:0},sfd={7:0.00}"
                                                  , curCandle.timestamp
                                                  , m_position.calcProfit(curCandle.last)
                                                  , curCandle.last
                                                  , curCandle.isTrend()
                                                  , curCandle.ema
                                                  , curCandle.getVolatility()
                                                  , curCandle.getVolatilityRate()
                                                  , curCandle.disparity_rate
                                );
                            }
                            else
                            {
                                Console.WriteLine("closed candle. timestamp={0},profit_sum={1},last={2:0},trend={3},ema={4:0},vola={5:0},vola_rate={6:0},sfd={7:0.00}"
                                                  , curCandle.timestamp
                                                  , m_profitSum
                                                  , curCandle.last
                                                  , curCandle.isTrend()
                                                  , curCandle.ema
                                                  , curCandle.getVolatility()
                                                  , curCandle.getVolatilityRate()
                                                  , curCandle.disparity_rate
                                );
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

                    // ENTRYロジック
                    //await tryEntryOrder(cur_value);

                    // 注文状況確認ロジック
                    await checkEntry();

                    // EXITロジック
                    //await tryExitOrder();

                    


                    // Losscutロジック
                    await tryLosscutOrder();

                    await checkExit();





                    //await checkEntryCompleted();

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

                    System.Threading.Thread.Sleep(0);
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
                if (applyCandlestick(m_candleBuf, ref ohlc, 0, m_candleBuf.m_buffer_num) != 0)
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
				if (applyCandlestick(testCandleBuf, ref ohlc, m_candleBuf.getCandleCount(), test_num, false) != 0)
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

				{
					// periods
					//  1m= 60sec
					// 15m=900sec
				}


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

                    // ENTRYテスト
                    tryEntryOrderTest(ref long_entry_cnt, ref short_entry_cnt, next_open);
                    checkEntryTest(curCandle.last);

                    // EXIT/ロスカットテスト
                    if (tryExitOrderTest(ref long_exit_cnt, ref short_exit_cnt) == 0)
                    {
                        checkExitTest(curCandle.last);
                    }

                    if (tryLosscutOrderTest(ref long_lc_cnt, ref short_lc_cnt) == 0)
                    {
                        checkExitTest(curCandle.last);
                    }

                    if (m_position.isEntryCompleted())
                    {
                        Console.WriteLine("closed candle. timestamp={0},profit={1},last={2:0},trend={3},ema={4:0},vola={5:0},vola_rate={6:0},sfd={7:0.00}"
                                          , curCandle.timestamp
                                          , m_position.calcProfit(curCandle.last)
                                          , curCandle.last
                                          , curCandle.isTrend()
                                          , curCandle.ema
                                          , curCandle.getVolatility()
                                          , curCandle.getVolatilityRate()
                                          , curCandle.disparity_rate
                        );
                    }
                    else
                    {
                        Console.WriteLine("closed candle. timestamp={0},profit_sum={1},last={2:0},trend={3},ema={4:0},vola={5:0},vola_rate={6:0},sfd={7:0.00}"
                                          , curCandle.timestamp
                                          , m_profitSum
                                          , curCandle.last
                                          , curCandle.isTrend()
                                          , curCandle.ema
                                          , curCandle.getVolatility()
                                          , curCandle.getVolatilityRate()
                                          , curCandle.disparity_rate
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

        public async Task<int> tryEntryOrder(double next_open)
        {
            int result = 0;
            try
            {
                if (!m_position.isNone())
                {
                    result = 1;
                    return result;
                }

                Candlestick curCandle = m_candleBuf.getLastCandle();
                if (curCandle == null)
                {
                    result = -1;
                    return result;
                }



                // NONEポジションの場合

                //bool isLongSub = isConditionLongEntryOverEma();
                //bool isShortSub = isConditionShortEntryOverEma();

                bool isLongSub = isConditionLongEntryScam(next_open);
                bool isShortSub = isConditionShortEntryScam(next_open);


                bool isLong = false;// isConditionLongEntryCrossEma();// || m_isDotenLong;
                bool isShort = false;// isConditionShortEntryCrossEma();// || m_isDotenShort;

                if (isLongSub || isShortSub || isLong || isShort)
                {
                    bool isActive = await Trade.isActive(m_authBitflyer, m_config.product_bitflyer);
                    if (isActive)
                    {
                        postSlack(string.Format("cant's Trade. Orders or Positions is exists. isLongSub={0} isShortSub={1} isLong={2} isShort={3}"
                            , isLongSub
                            , isShortSub
                            , isLong
                            , isShort
                        ));
                        result = -1;
                        return result;
                    }
                }

                if (isLongSub)
                {
                    //Console.WriteLine("Try Long Entry Order.");

                    if (curCandle.disparity_rate >= 5.0)
                    {
                        postSlack(string.Format("cancel Long Entry Order. DispartyRate is Over. rate={0:0.00}.", curCandle.disparity_rate));
                        result = -1;
                        return result;
                    }

                    SendChildOrderResponse retObj = await SendChildOrder.BuyMarket(m_authBitflyer, m_config.product_bitflyer, m_config.amount);
                    if (retObj == null)
                    {
                        postSlack("failed to Long Entry Order");
                        result = -1;
                        return result;
                    }
                    // 注文成功

                    m_position.entryLongOrder(retObj.child_order_acceptance_id, curCandle.timestamp);
                    postSlack(string.Format("{0} Long(Sub) Entry Order ID = {1}", curCandle.timestamp, retObj.child_order_acceptance_id));
                    m_position.strategy_type = Position.StrategyType.SCAM;
                }
                else if (isShortSub)
                {
                    //Console.WriteLine("Try Short Entry Order.");

                    SendChildOrderResponse retObj = await SendChildOrder.SellMarket(m_authBitflyer, m_config.product_bitflyer, m_config.amount);
                    if (retObj == null)
                    {
                        postSlack("failed to Short Entry Order");
                        result = -1;
                        return result;
                    }
                    // 注文成功

                    m_position.entryShortOrder(retObj.child_order_acceptance_id, curCandle.timestamp);


                    postSlack(string.Format("{0} Short(Sub) Entry Order ID = {1}", curCandle.timestamp, retObj.child_order_acceptance_id));
                    m_position.strategy_type = Position.StrategyType.SCAM;

                }
                else if (isLong)
                {
                    //Console.WriteLine("Try Long Entry Order.");

                    if (curCandle.disparity_rate >= 5.0)
                    {
                        postSlack(string.Format("cancel Long Entry Order. DispartyRate is Over. rate={0:0.00}.", curCandle.disparity_rate));
                        result = -1;
                        return result;
                    }

                    SendChildOrderResponse retObj = await SendChildOrder.BuyMarket(m_authBitflyer, m_config.product_bitflyer, m_config.amount);
                    if (retObj == null)
                    {
                        postSlack("failed to Long Entry Order");
                        result = -1;
                        return result;
                    }
                    // 注文成功

                    m_position.entryLongOrder(retObj.child_order_acceptance_id, curCandle.timestamp);
                    if (m_isDotenLong)
                    {
                        m_position.strategy_type = Position.StrategyType.DOTEN;
                    }
                    else
                    {
                        m_position.strategy_type = Position.StrategyType.CROSS_EMA;
                    }

                    postSlack(string.Format("{0} Long Entry Order ID = {1}", curCandle.timestamp, retObj.child_order_acceptance_id));
                }
                else if(isShort)
                {
                    //Console.WriteLine("Try Short Entry Order.");

                    SendChildOrderResponse retObj = await SendChildOrder.SellMarket(m_authBitflyer, m_config.product_bitflyer, m_config.amount);
                    if (retObj == null)
                    {
                        postSlack("failed to Short Entry Order");
                        result = -1;
                        return result;
                    }
                    // 注文成功

                    m_position.entryShortOrder(retObj.child_order_acceptance_id, curCandle.timestamp);
                    if (m_isDotenShort)
                    {
                        m_position.strategy_type = Position.StrategyType.DOTEN;
                    }
                    else
                    {
                        m_position.strategy_type = Position.StrategyType.CROSS_EMA;
                    }

                    postSlack(string.Format("{0} Short Entry Order ID = {1}", curCandle.timestamp, retObj.child_order_acceptance_id));
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

        public async Task<int> tryEntryOrderFrontLine()
        {
            int result = 0;
            try
            {
                if (!m_position.isNone())
                {
                    result = 1;
                    return result;
                }

                Candlestick curCandle = m_candleBuf.getLastCandle();
                if (curCandle == null)
                {
                    result = -1;
                    return result;
                }

                // NONEポジションの場合


                bool isLong = isConditionLongEntryFL();
                bool isShort = isConditionShortEntryFL();

                if (isLong)
                {
                    //Console.WriteLine("Try Long Entry Order.");

                    if (curCandle.disparity_rate >= 5.0)
                    {
                        postSlack(string.Format("cancel Long Entry Order. DispartyRate is Over. rate={0:0.00}.", curCandle.disparity_rate));
                        result = -1;
                        return result;
                    }

                    SendChildOrderResponse retObj = await SendChildOrder.BuyMarket(m_authBitflyer, m_config.product_bitflyer, m_config.amount);
                    if (retObj == null)
                    {
                        postSlack("failed to Long Entry Order");
                        result = -1;
                        return result;
                    }
                    // 注文成功

                    m_position.entryLongOrder(retObj.child_order_acceptance_id, curCandle.timestamp);
                    m_position.strategy_type = Position.StrategyType.FLONT_LINE;

                    postSlack(string.Format("{0} Long Entry Order ID = {1}", curCandle.timestamp, retObj.child_order_acceptance_id));
                }
                else if (isShort)
                {
                    //Console.WriteLine("Try Short Entry Order.");

                    SendChildOrderResponse retObj = await SendChildOrder.SellMarket(m_authBitflyer, m_config.product_bitflyer, m_config.amount);
                    if (retObj == null)
                    {
                        postSlack("failed to Short Entry Order");
                        result = -1;
                        return result;
                    }
                    // 注文成功

                    m_position.entryShortOrder(retObj.child_order_acceptance_id, curCandle.timestamp);
                    m_position.strategy_type = Position.StrategyType.FLONT_LINE;

                    postSlack(string.Format("{0} Short Entry Order ID = {1}", curCandle.timestamp, retObj.child_order_acceptance_id));
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

        public bool isConditionShortEntryFL()
        {
            bool result = false;
            try
            {
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

                int candle_cnt = m_candleBuf.getCandleCount();

                Candlestick curCandle = m_candleBuf.getLastCandle();
                if (curCandle == null)
                {
                    result = false;
                    return result;
                }

                int curIndex = candle_cnt - 1;

                Candlestick prevCandle = m_candleBuf.getCandle(curIndex - 1);
                if (prevCandle == null)
                {
                    result = false;
                    return result;
                }

                double offset = (prevCandle.last - m_config.entry_offset);
                double diff = curCandle.last - offset;

                if (diff > 0.0)
                {
                    Console.WriteLine("not need short. inside entry_offset. last={0:0} diff={1:0} offset={2:0} ", curCandle.last, diff, offset);
                    result = false;
                    return result;
                }

                // ENTRY
                Console.WriteLine("need short. over entry_offset. last={0:0} diff={1:0} offset={2:0} ", curCandle.last, diff, offset);
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

        public bool isConditionLongEntryFL()
        {
            bool result = false;
            try
            {
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

                int candle_cnt = m_candleBuf.getCandleCount();

                Candlestick curCandle = m_candleBuf.getLastCandle();
                if (curCandle == null)
                {
                    result = false;
                    return result;
                }

                int curIndex = candle_cnt - 1;

                Candlestick prevCandle = m_candleBuf.getCandle(curIndex - 1);
                if (prevCandle == null)
                {
                    result = false;
                    return result;
                }

                double offset = (prevCandle.last + m_config.entry_offset);
                double diff = offset - curCandle.last;

                if (diff > 0.0)
                {
                    Console.WriteLine("not need long. inside entry_offset. last={0:0} diff={1:0} offset={2:0} ", curCandle.last, diff, offset);
                    result = false;
                    return result;
                }

                // ENTRY
                Console.WriteLine("need long. over entry_offset. last={0:0} diff={1:0} offset={2:0} ", curCandle.last, diff, offset);
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

        public bool isConditionShortExitFL()
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


                double position = m_frontline - curCandle.last;
                if (position < 0.0)
                {
                    // 最前線が後退
                    // EXIT
                    postSlack(string.Format("## front-line is back ##. last={0:0} pos={1:0} front={2:0} ", curCandle.last, position, m_frontline), true);
                    result = true;
                }
                else if (position >= m_config.frontline_ahead)
                {
                    // 最前線を前進
                    m_frontline = m_frontline - Math.Round(position * 1.0); ;
                    // SHORT継続
                    postSlack(string.Format("## front-line is forward ##. last={0:0} pos={1:0} front={2:0} ", curCandle.last, position, m_frontline), true);
                    result = false;
                }
                else
                {
                    // 最前線を維持
                    // SHORT継続
                    Console.WriteLine("## front-line is keep ##. last={0:0} pos={1:0} front={2:0} ", curCandle.last, position, m_frontline);
                    result = false;
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


        public bool isConditionLongExitFL()
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

                double position = curCandle.last - m_frontline;
                if (position < 0.0)
                {
                    // 最前線が後退
                    // EXIT
                    postSlack(string.Format("## front-line is back ##. last={0:0} pos={1:0} front={2:0} ", curCandle.last, position, m_frontline),true);
                    result = true;
                }
                else if (position >= m_config.frontline_ahead)
                {
                    // 最前線を前進
                    m_frontline = m_frontline + Math.Round(position*1.0);
                    // LONG継続
                    postSlack(string.Format("## front-line is forward ##. last={0:0} pos={1:0} front={2:0} ", curCandle.last, position, m_frontline), true);
                    result = false;
                }
                else
                {
                    // 最前線を維持
                    // LONG継続
                    Console.WriteLine("## front-line is keep ##. last={0:0} pos={1:0} front={2:0} ", curCandle.last, position, m_frontline);
                    result = false;
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

        private int tryEntryOrderTest(ref int long_entry_cnt, ref int short_entry_cnt, double next_open)
        {
            int result = 0;
            try
            {
                if (!m_position.isNone())
                {
                    result = 1;
                    return result;
                }

                Candlestick curCandle = m_candleBuf.getLastCandle();
                if (curCandle == null)
                {
                    result = -1;
                    return result;
                }

                // NONEポジションの場合

                //bool isLongSub = isConditionLongEntryOverEma();
                //bool isShortSub = isConditionShortEntryOverEma();

                bool isLongSub = isConditionLongEntryScam(next_open); 
                bool isShortSub = isConditionShortEntryScam(next_open);

                //bool isLongSub = isConditionLongEntrySwing(next_open);
                //bool isShortSub = isConditionShortEntrySwing(next_open);

                bool isLong = false;// isConditionLongEntryCrossEma();// || m_isDotenLong;
                bool isShort = false;// isConditionShortEntryCrossEma();// || m_isDotenShort;

                //bool isLong = isConditionLongEntryReboundEMA(next_open) && m_isDoten;
                //bool isShort = isConditionShortEntryReboundEMA(next_open) && m_isDoten;



                if (isLongSub)
                {
                    // 注文成功
                    string long_id = string.Format("BT_LONG_ENTRY_{0:D8}", long_entry_cnt);

                    m_position.entryLongOrder(long_id, curCandle.timestamp);


                    postSlack(string.Format("{0} Long(Sub) Entry Order ID = {1}", curCandle.timestamp, long_id), true);
                    //m_position.strategy_type = Position.StrategyType.OVER_EMA;
                    //m_position.strategy_type = Position.StrategyType.SWING;
                    m_position.strategy_type = Position.StrategyType.SCAM;
                    long_entry_cnt++;
                }
                else if (isShortSub)
                {
                    // 注文成功
                    string short_id = string.Format("BT_SHORT_ENTRY_{0:D8}", short_entry_cnt);

                    m_position.entryShortOrder(short_id, curCandle.timestamp);

                    postSlack(string.Format("{0} Short(Sub) Entry Order ID = {1}", curCandle.timestamp, short_id), true);
                    //m_position.strategy_type = Position.StrategyType.OVER_EMA;
                    //m_position.strategy_type = Position.StrategyType.SWING;
                    m_position.strategy_type = Position.StrategyType.SCAM;

                    short_entry_cnt++;
                }
                else if (isLong)
                {
                    // 注文成功
                    string long_id = string.Format("BT_LONG_ENTRY_{0:D8}", long_entry_cnt);

                    m_position.entryLongOrder(long_id, curCandle.timestamp);
                    if (m_isDotenLong)
                    {
                        m_position.strategy_type = Position.StrategyType.DOTEN;
                    }
                    else
                    {
                        m_position.strategy_type = Position.StrategyType.CROSS_EMA;
                        //m_position.strategy_type = Position.StrategyType.REBOUND_EMA;
                    }

                    postSlack(string.Format("{0} Long Entry Order ID = {1}", curCandle.timestamp, long_id), true);
                    

                    long_entry_cnt++;
                }
                else if (isShort)
                {
                    // 注文成功
                    string short_id = string.Format("BT_SHORT_ENTRY_{0:D8}", short_entry_cnt);

                    m_position.entryShortOrder(short_id, curCandle.timestamp);
                    if (m_isDotenShort)
                    {
                        m_position.strategy_type = Position.StrategyType.DOTEN;
                    }
                    else
                    {
                        m_position.strategy_type = Position.StrategyType.CROSS_EMA;
                        //m_position.strategy_type = Position.StrategyType.REBOUND_EMA;
                    }

                    postSlack(string.Format("{0} Short Entry Order ID = {1}", curCandle.timestamp, short_id), true);

                    short_entry_cnt++;
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
                    m_frontline = responce.average_price;// + m_config.losscut_value;
                }
                else if (m_position.isShort())
                {
                    m_frontline = responce.average_price;// - m_config.losscut_value;
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
                    m_frontline = last_price;// + m_config.losscut_value;
                }
                else if (m_position.isShort())
                {
                    m_frontline = last_price;// - m_config.losscut_value;
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

        public async Task<int> tryExitOrder()
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

                    bool isCond = false;
                    if (m_position.strategy_type == Position.StrategyType.CROSS_EMA || m_position.strategy_type == Position.StrategyType.DOTEN)
                    {
                        isCond = isConditionLongExitCrossEma();
                    }
                    else if (m_position.strategy_type == Position.StrategyType.FLONT_LINE)
                    {
                        isCond = isConditionLongExitFL();
                    }
                    else
                    {
                        isCond = isConditionLongExit();
                    }

                    if (isCond)
                    {
                        //Console.WriteLine("Try Long Exit Order.");
                        SendChildOrderResponse retObj = null;
                        int retry_cnt = 0;
                        while (true)
                        {
                            retry_cnt++;
                            retObj = await SendChildOrder.SellMarket(m_authBitflyer, m_config.product_bitflyer, m_config.amount);
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

                    bool isCond = false;
                    if (m_position.strategy_type == Position.StrategyType.CROSS_EMA || m_position.strategy_type == Position.StrategyType.DOTEN)
                    {
                        isCond = isConditionShortExitCrossEma();
                    }
                    else if (m_position.strategy_type == Position.StrategyType.FLONT_LINE)
                    {
                        isCond = isConditionShortExitFL();
                    }
                    else
                    {
                        isCond = isConditionShortExit();
                    }

                    if (isCond)
                    {
                        //Console.WriteLine("Try Short Exit Order.");
                        SendChildOrderResponse retObj = null;
                        int retry_cnt = 0;
                        while (true)
                        {
                            retry_cnt++;
                            retObj = await SendChildOrder.BuyMarket(m_authBitflyer, m_config.product_bitflyer, m_config.amount);
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

        public int tryExitOrderTest(ref int long_exit_cnt, ref int short_exit_cnt)
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

                    bool isCond = false;
                    if (   m_position.strategy_type == Position.StrategyType.CROSS_EMA
                        || m_position.strategy_type == Position.StrategyType.DOTEN
                        || m_position.strategy_type == Position.StrategyType.REBOUND_EMA
                    )
                    {
                        isCond = isConditionLongExitCrossEma();
                    }
                    else
                    {
                        isCond = isConditionLongExit();
                    }

                    if (isCond)
                    {
                        // 注文成功
                        string long_id = string.Format("BT_LONG_EXIT_{0:D8}", long_exit_cnt);
                        postSlack(string.Format("{0} Long Exit Order ID = {1}", curCandle.timestamp, long_id), true);
                        m_position.exitOrder(long_id, curCandle.timestamp);
                        long_exit_cnt++;
                    }
                }
                else if (m_position.isShort())
                {// SHORTの場合
                    bool isCond = false;
                    if (   m_position.strategy_type == Position.StrategyType.CROSS_EMA 
                        || m_position.strategy_type == Position.StrategyType.DOTEN
                        || m_position.strategy_type == Position.StrategyType.REBOUND_EMA
                    )
                    {
                        isCond = isConditionShortExitCrossEma();
                    }
                    else
                    {
                        isCond = isConditionShortExit();
                    }

                    if (isCond)
                    {
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

                        SendChildOrderResponse retObj = await SendChildOrder.SellMarket(m_authBitflyer, m_config.product_bitflyer, m_config.amount);
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

                        SendChildOrderResponse retObj = await SendChildOrder.BuyMarket(m_authBitflyer, m_config.product_bitflyer, m_config.amount);
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



        public bool isConditionLongExit()
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

                double profit = curCandle.last - m_position.entry_price;

                if (curCandle.ema <= curCandle.last)
                {
                    if (profit > 0.0)
                    {
                        result = true;
                        return result;
                    }
                }

                double ema_diff = curCandle.ema - curCandle.last;
                if (ema_diff <= m_config.ema_diff_near)
                {
                    if (profit > 0.0)
                    {
                        result = true;
                        return result;
                    }
                }

				if (curCandle.high >= curCandle.ema)
                {
                    if (profit > 0.0)
                    {
                        result = true;
                        return result;
                    }
                }

                if (m_config.fixed_profit >= 100)
                {
                    if (profit >= m_config.fixed_profit)
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
                if (result)
                {
                    m_isDoten = true;
                }
            }
            return result;
        }

        public bool isConditionShortExit()
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

                double profit = m_position.entry_price - curCandle.last;

                if (curCandle.ema >= curCandle.last)
                {
                    if (profit > 0.0)
                    {
                        result = true;
                        return result;
                    }
                }

                double ema_diff = curCandle.last - curCandle.ema;
                if (ema_diff <= m_config.ema_diff_near)
                {
                    if (profit > 0.0)
                    {
                        result = true;
                        return result;
                    }
                }

				if( curCandle.low <= curCandle.ema )
				{
					if (profit > 0.0)
                    {
                        result = true;
                        return result;
                    }
				}

                if (m_config.fixed_profit >= 100)
                {
                    if (profit >= m_config.fixed_profit)
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
                if (result)
                {
                    m_isDoten = true;
                }
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

                double profit = curCandle.last - m_position.entry_price;
                if (profit <= m_config.losscut_value)
                {
                    result = true;
                    return result;
                }

                if (!curCandle.isTrend())
                {
                    double vola = curCandle.getVolatilityRate();
                    if (vola >= m_config.whale_vola_rate)
                    {
                        Console.WriteLine("## LOSSCUT ## volaRate is Limit-Over. profit={0:0} vola={1} limit={2:0}", profit, vola, m_config.whale_vola_rate);
                        result = true;
                        return result;
                    }
                }

                switch (m_position.strategy_type)
                {
                    case Position.StrategyType.SCAM:
                    case Position.StrategyType.SWING:
                        if (profit <= 0.0)
                        {
                            //if (m_position.entry_price >= curCandle.ema)
                            if (curCandle.last >= curCandle.ema)
                            {
                                if (isBadPosition())
                                {
                                    result = true;
                                    return result;
                                }
                            }

                            if (m_config.boll_outside_check > 0)
                            {
                                if ((curCandle.boll_low - m_config.boll_diff_play) < curCandle.boll_low_top)
                                {
                                    if (isBadPosition())
                                    {
                                        Console.WriteLine("## LOSSCUT ## boll_low is outside. profit={0:0} boll_low={1:0} boll_low_top={2:0}", profit, curCandle.boll_low, curCandle.boll_low_top);
                                        result = true;
                                        return result;
                                    }
                                }
                            }

                            if (m_config.lc_boll_outside_check > 0)
                            {
                                if (curCandle.range_min_keep <= 0)
                                {
                                    Console.WriteLine("## LOSSCUT ## RANGE-KEEP is end. profit={0:0} min_cnt={1}", profit, curCandle.range_min_cnt);
                                    result = true;
                                    return result;
                                }
                            }
                        }
                        break;
                    case Position.StrategyType.CROSS_EMA:
                    case Position.StrategyType.REBOUND_EMA:
                        if (m_config.expiration_cnt >= 0)
                        {
                            bool isGolden = false;
                            bool isFirst = false;
                            int back_cnt = 0;
                            double cur_ema_length = 0.0;
                            if (m_candleBuf.getEMACrossState(out isGolden, out isFirst, out back_cnt, out cur_ema_length, 1.0) == 0)
                            {
                                if ((!isGolden) && (back_cnt >= m_config.expiration_cnt))
                                {
                                    if ((cur_ema_length <= m_config.expiration_ema_diff))
                                    {
                                        //m_isDotenShort = true;
                                    }
                                    result = true;
                                    return result;
                                }
                            }
                        }
                        break;
                    case Position.StrategyType.DOTEN:
                        break;
                    default:
                        break;
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

                double profit = m_position.entry_price - curCandle.last;
                if (profit <= m_config.losscut_value)
                {
                    result = true;
                    return result;
                }

                if (curCandle.isTrend())
                {
                    double vola = curCandle.getVolatilityRate();
                    if (vola >= m_config.whale_vola_rate)
                    {
                        Console.WriteLine("## LOSSCUT ## volaRate is Limit-Over. profit={0:0} vola={1} limit={2:0}", profit, vola, m_config.whale_vola_rate);
                        result = true;
                        return result;
                    }
                }

                switch (m_position.strategy_type)
                {
                    case Position.StrategyType.SCAM:
                    case Position.StrategyType.SWING:
                        if (profit <= 0.0)
                        {
                            //if (m_position.entry_price <= curCandle.ema)                  
                            if (curCandle.last <= curCandle.ema)
                            {
                                if (isBadPosition())
                                {
                                    result = true;
                                    return result;
                                }
                            }

                            if (m_config.lc_boll_outside_check > 0)
                            {
                                if ((curCandle.boll_high + m_config.boll_diff_play) > curCandle.boll_high_top)
                                {
                                    if (isBadPosition())
                                    {
                                        Console.WriteLine("## LOSSCUT ## boll_high is outside. profit={0:0} boll_high={1:0} boll_high_top={2:0}", profit, curCandle.boll_high, curCandle.boll_high_top);
                                        result = true;
                                        return result;
                                    }
                                }
                            }

                            if (m_config.lc_boll_outside_check > 0)
                            {
                                if (curCandle.range_max_keep <= 0)
                                {
                                    Console.WriteLine("## LOSSCUT ## RANGE-KEEP is end. profit={0:0} max_cnt={1}", profit, curCandle.range_max_cnt);
                                    result = true;
                                    return result;
                                }
                            }
                        }
                        break;
                    case Position.StrategyType.CROSS_EMA:
                    case Position.StrategyType.REBOUND_EMA:
                        if (m_config.expiration_cnt >= 0)
                        {
                            bool isGolden = false;
                            bool isFirst = false;
                            int back_cnt = 0;
                            double cur_ema_length = 0.0;
                            if (m_candleBuf.getEMACrossState(out isGolden, out isFirst, out back_cnt, out cur_ema_length, 1.0) == 0)
                            {
                                if ((isGolden) && (back_cnt >= m_config.expiration_cnt))
                                {
                                    if ((cur_ema_length <= m_config.expiration_ema_diff))
                                    {
                                        //m_isDotenLong = true;
                                    }
                                    result = true;
                                    return result;
                                }
                            }
                        }
                        break;
                    case Position.StrategyType.DOTEN:
                        break;
                    default:
                        break;
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

        public bool isConditionShortEntrySwing(double next_open)
        {
            bool result = false;

            try
            {
                m_curShortBollLv = -5;
                m_preShortBollLv = -5;

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

                int candle_cnt = m_candleBuf.getCandleCount();

                Candlestick curCandle = m_candleBuf.getLastCandle();
                if (curCandle == null)
                {
                    result = false;
                    return result;
                }

                int curIndex = candle_cnt - 1;

                Candlestick prevCandle = m_candleBuf.getCandle(curIndex - 1);
                if (prevCandle == null)
                {
                    result = false;
                    return result;
                }

                Candlestick pastCandle = m_candleBuf.getCandle(curIndex - 2);
                if (pastCandle == null)
                {
                    result = false;
                    return result;
                }

                m_curShortBollLv = curCandle.getShortLevel();
                m_preShortBollLv = prevCandle.getShortLevel();

                if (m_config.boll_outside_check > 0)
                {
                    if ((curCandle.boll_high + m_config.boll_diff_play) > curCandle.boll_high_top)
                    {
                        Console.WriteLine("not need short. boll_high is outside.");
                        result = false;
                        return result;
                    }
                }

                if (!prevCandle.isOverBBHigh(prevCandle.last))
                {
                    if (!pastCandle.isOverBBHigh(pastCandle.last))
                    {
                        // 一つ前のキャンドルと過去のキャンドルの終値がBollHighをOVERしてない
                        // SHORTすべきでない
                        Console.WriteLine("not need short. prevCandle and pastCandle'last is not over BB_HIGH");
                        result = false;
                        return result;
                    }
                    else
                    {
                        // 過去キャンドルの終値がBollHighをOVERしている
                        Console.WriteLine("pastCandle'last is over BB_HIGH");
                    }
                }
                else
                {
                    // 一つ前のキャンドルの終値がBollHighをOVERしている
                    Console.WriteLine("prevCandle'last is over BB_HIGH");
                }

                double ema_diff = curCandle.last - curCandle.ema;
                if (ema_diff < m_config.ema_diff_far)
                {
                    Console.WriteLine("not need short. ema_diff is LOW. diff={0:0}", ema_diff);
                    result = false;
                    return result;
                }

                if (m_preShortBollLv < 0)
                {
                    //前回のSHORTレベルが低い
                    Console.WriteLine("not need short. m_preShortBollLv is LOW. Lv={0}", m_preShortBollLv);
                    // 何もしない
                    result = false;
                    return result;
                }
                else
                {
                    if (m_preShortBollLv <= 0)
                    {
                        //前回のSHORTレベルが0以下
                        if (prevCandle.isTrend())
                        {
                            // 前回が上昇キャンドルの場合

                            Console.WriteLine("not need short. m_preShortBollLv is LOW but change soon. Lv={0}", m_preShortBollLv);
                            // 何もしない
                            result = false;
                            return result;
                        }
                        else
                        {
                            // 前回が下降キャンドルの場合

                            if (prevCandle.isOverBBHigh(prevCandle.last))
                            {
                                // キャンドル終値がボリンジャー高バンド以上にある
                                // まだ上昇の可能性がある
                                Console.WriteLine("not need short. prevCandle's last is Over BB_HIGHLOW. Lv={0}", m_preShortBollLv);
                                // 何もしない
                                result = false;
                                return result;
                            }
                        }
                    }

                    //前回のSHORTレベルが0より大きい

                    if (m_curShortBollLv <= 0)
                    {
                        // 現在のSHORTレベルが0以下
                        Console.WriteLine("not need short. m_curShortBollLv is LOW. Lv={0}", m_curShortBollLv);
                        // 何もしない
                        result = false;
                        return result;
                    }
                    else
                    {
                        // 現在のSHORTレベルが0より高い

                        if (curCandle.last < next_open)
                        {
                            double diff = next_open - curCandle.last;
                            if (diff >= m_config.next_open_diff)
                            {
                                Console.WriteLine("not need short. next_open is HIGH. Lv={0} Diff={1:0} last={2:0} next={3:0}", m_curLongBollLv, diff, curCandle.last, next_open);
                                // 何もしない
                                result = false;
                                return result;
                            }
                        }

                        int band_pos = 0;
                        if (isPassBBtoMATop(out band_pos))
                        {
                            // 上位ボリンジャーバンドをはみ出てMAにタッチしていた場合
                            // ENTRY
                            Console.WriteLine("need short. m_curShortBollLv is HIGH. Lv={0}", m_curShortBollLv);
                            result = true;
                            return result;
                        }
                        else
                        {
                            // 上位ボリンジャーバンドをはみ出てMAにタッチしていない場合
                            if (band_pos == -1)
                            {
                                // 上位BBバンドの下側を超えていた場合
                                // MA側に向かう上への力が強いはず
                                // 現在値より上位MAが下にあればENTRY
                                if (curCandle.last > curCandle.ma_top)
                                {

                                    bool isGolden = false;
                                    bool isFirst = false;
                                    int back_cnt = 0;
                                    double cur_ema_length = 0.0;
                                    if (m_candleBuf.getEMACrossState(out isGolden, out isFirst, out back_cnt, out cur_ema_length) != 0)
                                    {
                                        // 何もしない
                                        result = false;
                                        return result;
                                    }

                                    if ((!isGolden) && isFirst)
                                    {
                                        // ENTRY
                                        Console.WriteLine("need short. Touch BB_LOW. MA_TOP is OVER. Lv={0}", m_curShortBollLv);
                                        result = true;
                                        return result;
                                    }
                                    else
                                    {
                                        Console.WriteLine("not need short. not DEAD-CROSS. Lv={0}", m_curLongBollLv);
                                        // 何もしない
                                        result = false;
                                        return result;
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("not need short. not pass BB to MATop. Lv={0}", m_curShortBollLv);
                                    // 何もしない
                                    result = false;
                                    return result;
                                }
                            }
                            else if (band_pos == 1)
                            {
                                // 上位BBバンドの上側を超えていた場合
                                // MA側に向かう下への力が強いはず
                                // 現在値より上位MAが下にあればENTRY
                                if (curCandle.last > curCandle.ma_top)
                                {
                                    bool isGolden = false;
                                    bool isFirst = false;
                                    int back_cnt = 0;
                                    double cur_ema_length = 0.0;
                                    if (m_candleBuf.getEMACrossState(out isGolden, out isFirst, out back_cnt, out cur_ema_length) != 0)
                                    {
                                        // 何もしない
                                        result = false;
                                        return result;
                                    }

                                    if ((!isGolden) && isFirst)
                                    {
                                        // ENTRY
                                        Console.WriteLine("need short. Touch BB_LOW. MA_TOP is OVER. Lv={0}", m_curShortBollLv);
                                        result = true;
                                        return result;
                                    }
                                    else
                                    {
                                        Console.WriteLine("not short long. not DEAD-CROSS. Lv={0}", m_curLongBollLv);
                                        // 何もしない
                                        result = false;
                                        return result;
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("not need short. not pass BB to MATop. Lv={0}", m_curShortBollLv);
                                    // 何もしない
                                    result = false;
                                    return result;
                                }
                            }
                            else
                            {
                                Console.WriteLine("not need short. not pass BB to MATop. Lv={0}", m_curShortBollLv);
                                // 何もしない
                                result = false;
                                return result;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = false;
                m_curShortBollLv = -5;
                m_preShortBollLv = -5;
            }
            finally
            {
            }
            return result;
        }

        public bool isConditionLongEntrySwing(double next_open)
        {
            bool result = false;
            try
            {
                m_curLongBollLv = -5;
                m_preLongBollLv = -5;

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

                int candle_cnt = m_candleBuf.getCandleCount();

                Candlestick curCandle = m_candleBuf.getLastCandle();
                if (curCandle == null)
                {
                    result = false;
                    return result;
                }

                int curIndex = candle_cnt - 1;

                Candlestick prevCandle = m_candleBuf.getCandle(curIndex - 1);
                if (prevCandle == null)
                {
                    result = false;
                    return result;
                }

                Candlestick pastCandle = m_candleBuf.getCandle(curIndex - 2);
                if (pastCandle == null)
                {
                    result = false;
                    return result;
                }

                m_curLongBollLv = curCandle.getLongLevel();
                m_preLongBollLv = prevCandle.getLongLevel();



                if (!prevCandle.isUnderBBLow(prevCandle.last))
                {
                    if (!pastCandle.isUnderBBLow(pastCandle.last))
                    {
                        Console.WriteLine("not need long. pastCandle and prevCandle'last is not under BB_LOW");
                        result = false;
                        return result;
                    }
                    else
                    {
                        Console.WriteLine("pastCandle'last is under BB_LOW");
                    }
                }
                else
                {
                    Console.WriteLine("prevCandle'last is under BB_LOW");
                }

                if (m_config.boll_outside_check > 0)
                {
                    if ((curCandle.boll_low - m_config.boll_diff_play) < curCandle.boll_low_top)
                    {
                        Console.WriteLine("not need long. boll_low is outside.");
                        result = false;
                        return result;
                    }
                }


                double ema_diff = curCandle.ema - curCandle.last;
                if (ema_diff < m_config.ema_diff_far)
                {
                    Console.WriteLine("not need long. ema_diff is LOW. diff={0:0}", ema_diff);
                    result = false;
                    return result;
                }


                if (m_preLongBollLv < 0)
                {
                    //前回のLONGレベルが低い
                    Console.WriteLine("not need long. m_preLongBollLv is LOW. Lv={0}", m_preLongBollLv);
                    // 何もしない
                    result = false;
                    return result;
                }
                else
                {
                    if (m_preLongBollLv <= 0)
                    {
                        //前回のLONGレベルが0以下
                        if (prevCandle.isTrend())
                        {
                            // 前回が上昇キャンドルの場合
                            if (prevCandle.isUnderBBLow(prevCandle.last))
                            {
                                // 何もしない
                                Console.WriteLine("not need long. prevCandle's last is Under BB_LOW. Lv={0}", m_preLongBollLv);
                                result = false;
                                return result;
                            }
                        }
                        else
                        {
                            // 前回が下降キャンドルの場合
                            Console.WriteLine("not need long. m_preLongBollLv is LOW but change soon. Lv={0}", m_preLongBollLv);
                            // 何もしない
                            result = false;
                            return result;
                        }
                    }

                    //前回のLONGレベルが0より大きい

                    if (m_curLongBollLv <= 0)
                    {
                        // 現在のLONGレベルが0以下
                        Console.WriteLine("not need long. m_curLongBollLv is LOW. Lv={0}", m_curLongBollLv);
                        // 何もしない
                        result = false;
                        return result;
                    }
                    else
                    {
                        // 現在のLONGレベルが0より高い

                        if (curCandle.last > next_open)
                        {
                            double diff = curCandle.last - next_open;
                            if (diff >= m_config.next_open_diff)
                            {
                                Console.WriteLine("not need long. next_open is LOW. Lv={0} Diff={1:0} last={2:0} next={3:0}", m_curLongBollLv, diff, curCandle.last, next_open);
                                // 何もしない
                                result = false;
                                return result;
                            }
                        }


                        int band_pos = 0;
                        if (isPassBBtoMATop(out band_pos))
                        {
                            // 上位ボリンジャーバンドをはみ出てから一度はMAにタッチしていた場合
                            // ENTRY
                            Console.WriteLine("need long. m_curLongBollLv is HIGH. Lv={0}", m_curLongBollLv);
                            result = true;
                            return result;
                        }
                        else
                        {
                            // 上位ボリンジャーバンドをはみ出てから一度もMAにタッチしていない場合
                            if (band_pos == -1)
                            {
                                // 上位BBバンドの下側を超えていた場合
                                // MAに向かう上への力が強いはず
                                // 現在値より上位MAが上にあればENTRY
                                if (curCandle.last < curCandle.ma_top)
                                {
                                    bool isGolden = false;
                                    bool isFirst = false;
                                    int back_cnt = 0;
                                    double cur_ema_length = 0.0;
                                    if (m_candleBuf.getEMACrossState(out isGolden, out isFirst, out back_cnt, out cur_ema_length) != 0)
                                    {
                                        // 何もしない
                                        result = false;
                                        return result;
                                    }

                                    if (isGolden && isFirst)
                                    {
                                        // GOLDENクロスの初動の場合

                                        // ENTRY
                                        Console.WriteLine("need long. Touch BB_LOW. MA_TOP is OVER. Lv={0}", m_curLongBollLv);
                                        result = true;
                                        return result;
                                    }
                                    else
                                    {
                                        Console.WriteLine("not need long. not GOLDEN-CROSS. Lv={0}", m_curLongBollLv);
                                        // 何もしない
                                        result = false;
                                        return result;
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("not need long. not pass BB to MATop. Lv={0}", m_curLongBollLv);
                                    // 何もしない
                                    result = false;
                                    return result;
                                }
                            }
                            else if (band_pos == 1)
                            {
                                // 上位BBバンドの上側を超えていた場合
                                // MAに向かう下への力が強いはず
                                // 現在値より上位MAが上にあればENTRY
                                if (curCandle.last < curCandle.ma_top)
                                {
                                    bool isGolden = false;
                                    bool isFirst = false;
                                    int back_cnt = 0;
                                    double cur_ema_length = 0.0;
                                    if (m_candleBuf.getEMACrossState(out isGolden, out isFirst, out back_cnt, out cur_ema_length) != 0)
                                    {
                                        // 何もしない
                                        result = false;
                                        return result;
                                    }

                                    if (isGolden && isFirst)
                                    {
                                        // GOLDENクロスの初動の場合

                                        // ENTRY
                                        Console.WriteLine("need long. Touch BB_LOW. MA_TOP is OVER. Lv={0}", m_curLongBollLv);
                                        result = true;
                                        return result;
                                    }
                                    else
                                    {
                                        Console.WriteLine("not need long. not GOLDEN-CROSS. Lv={0}", m_curLongBollLv);
                                        // 何もしない
                                        result = false;
                                        return result;
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("not need long. not pass BB to MATop. Lv={0}", m_curLongBollLv);
                                    // 何もしない
                                    result = false;
                                    return result;
                                }
                            }
                            else
                            {
                                Console.WriteLine("not need long. not pass BB to MATop. Lv={0}", m_curLongBollLv);
                                // 何もしない
                                result = false;
                                return result;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = false;
                m_curLongBollLv = -5;
                m_preLongBollLv = -5;
            }
            finally
            {
            }
            return result;
        }

        public bool isConditionShortEntry()
        {
            bool result = false;

            try
            {
                m_curShortBollLv = -5;
                m_preShortBollLv = -5;

                if (m_candleBuf==null)
                {
                    result = false;
                    return result;
                }

                if (!m_candleBuf.isFullBuffer())
                {
                    result = false;
                    return result;
                }

                int candle_cnt = m_candleBuf.getCandleCount();

                Candlestick curCandle = m_candleBuf.getLastCandle();
                if (curCandle == null)
                {
                    result = false;
                    return result;
                }

                int curIndex = candle_cnt - 1;

                Candlestick prevCandle = m_candleBuf.getCandle(curIndex - 1);
                if(prevCandle==null)
                {
                    result = false;
                    return result;
                }

                Candlestick pastCandle = m_candleBuf.getCandle(curIndex - 2);
                if (pastCandle == null)
                {
                    result = false;
                    return result;
                }

                m_curShortBollLv = curCandle.getShortLevel();
                m_preShortBollLv = prevCandle.getShortLevel();



				if (!prevCandle.isOverBBHigh(prevCandle.last))
                {
					if (!pastCandle.isOverBBHigh(pastCandle.last))
                    {
                        // 一つ前のキャンドルと過去のキャンドルの終値がBollHighをOVERしてない
                        // SHORTすべきでない
                        result = false;
                        return result;
                    }
                    else
                    {
                        // 過去キャンドルの終値がBollHighをOVERしている
                        Console.WriteLine("pastCandle is over BB_HIGH");
                    }
                }
                else
                {
					// 一つ前のキャンドルがBollHighにタッチしていない
                    Console.WriteLine("prevCandle is Touch BB_HIGH");
                }

				//if (curCandle.boll_high < (curCandle.boll_high_top+5000))
				if (curCandle.boll_high < curCandle.boll_high_top )
                {
                    Console.WriteLine("not need short. boll_high is inside.");
                    result = false;
                    return result;
                }
                
                /*
				if(curCandle.isCrossBBHighTop(1000))
				{
					Console.WriteLine("not need short. cross top's boll_high.");
                    result = false;
                    return result;
				}
				*/
                
                /*
                if(!m_candleBuf.isOverTopBB(m_config.boll_over_candle_num))
                {
                    Console.WriteLine("not need short. boll_high is not over the top.");
                    result = false;
                    return result;
                }
                */

                double ema_diff = curCandle.last - curCandle.ema;
                if (ema_diff < m_config.ema_diff_far)
                {
                    Console.WriteLine("not need short. ema_diff is LOW. diff={0:0}",ema_diff);
                    result = false;
                    return result;
                }

                /*
                double diff = 0.0;
                int back_cnt = 0;
                if (m_candleBuf.searchMACross(ref diff, ref back_cnt) == 0)
                {
                    if ( (back_cnt <= 0) || (back_cnt >=90) )
                    {
                        Console.WriteLine("not need short. back_cnt is over. cnt={0} diff={1:0}", back_cnt, diff);
                        result = false;
                        return result;
                    }
                }
                */

                /*
                double ma_diff = curCandle.last - curCandle.ma;
                if(ma_diff < 2000.0)
                {
                    Console.WriteLine("not need short. ma_diff is LOW. diff={0:0}", ma_diff);
                    result = false;
                    return result;
                }


                if (curCandle.isCrossBBHighTop() || (curCandle.boll_high_top > curCandle.high))
                {
                    Console.WriteLine("not need short. Cross BBHighTop");
                    result = false;
                    return result;
                }
                */

                if (m_preShortBollLv < 0)
                {
                    //前回のSHORTレベルが低い
                    Console.WriteLine("not need short. m_preShortBollLv is LOW. Lv={0}", m_preShortBollLv);
                    // 何もしない
                    result = false;
                    return result;
                }
                else
                {
                    if (m_preShortBollLv <= 0)
                    {
                        //前回のSHORTレベルが0以下
                        if (prevCandle.isTrend())
                        {
                            // 前回が上昇キャンドルの場合
                            /*
                            if (prevCandle.isOverBBLast())
                            {
                                // キャンドル始値がボリンジャー高バンド以上にある
                                // まだ上昇の可能性がある
                                Console.WriteLine("not need short. prevCandle's last is Over BB_HIGH. Lv={0}", m_preShortBollLv);
                                // 何もしない
                                result = false;
                                return result;
                            }
                            */
                            Console.WriteLine("not need short. m_preShortBollLv is LOW but change soon. Lv={0}", m_preShortBollLv);
                            // 何もしない
                            result = false;
                            return result;
                        }
                        else
                        {
                            // 前回が下降キャンドルの場合
							if (prevCandle.isOverBBHigh(prevCandle.last))
                            {
                                // キャンドル終値がボリンジャー高バンド以上にある
                                // まだ上昇の可能性がある
                                Console.WriteLine("not need short. prevCandle's last is Over BB_HIGHLOW. Lv={0}", m_preShortBollLv);
                                // 何もしない
                                result = false;
                                return result;
                            }
                        }
                    }

                    //前回のSHORTレベルが0より大きい

                    if (m_curShortBollLv <= 0)
                    {
                        // 現在のSHORTレベルが0以下
                        Console.WriteLine("not need short. m_curShortBollLv is LOW. Lv={0}", m_curShortBollLv);
                        // 何もしない
                        result = false;
                        return result;
                    }
                    else
                    {
						// 現在のSHORTレベルが0より高い

						double curDiff = curCandle.getDiff();
						double preDiff = prevCandle.getDiff();
						if(curDiff <= preDiff)
                        {
                            Console.WriteLine("need short. m_curShortBollLv is HIGH. Lv={0}", m_curShortBollLv);
                            // ENTRY
                            result = true;
                            return result;
                        }
                        else
                        {
							Console.WriteLine("not need short. curDiff is big than preDiff. cur={0} pre={1}", curDiff, preDiff);
                            // 何もしない
                            result = false;
                            return result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = false;
                m_curShortBollLv = -5;
                m_preShortBollLv = -5;
            }
            finally
            {
            }
            return result;
        }

        public bool isConditionLongEntry()
        {
            bool result = false;
            try
            {
                m_curLongBollLv = -5;
                m_preLongBollLv = -5;

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

                int candle_cnt = m_candleBuf.getCandleCount();

                Candlestick curCandle = m_candleBuf.getLastCandle();
                if (curCandle == null)
                {
                    result = false;
                    return result;
                }

                int curIndex = candle_cnt - 1;

                Candlestick prevCandle = m_candleBuf.getCandle(curIndex - 1);
                if (prevCandle == null)
                {
                    result = false;
                    return result;
                }

                Candlestick pastCandle = m_candleBuf.getCandle(curIndex - 2);
                if (pastCandle == null)
                {
                    result = false;
                    return result;
                }

                m_curLongBollLv = curCandle.getLongLevel();
                m_preLongBollLv = prevCandle.getLongLevel();



				if (!prevCandle.isUnderBBLow(prevCandle.last))
                {
					if (!pastCandle.isUnderBBLow(pastCandle.last))
                    {
                        //if (!curCandle.isTouchBollLowTop())
                        //{
                            result = false;
                            return result;
                        //}
                        //else
                        //{
                        //    Console.WriteLine("curCandle is Touch BB_LOW_TOP");
                        //}
                    }
                    else
                    {
                        Console.WriteLine("pastCandle is Touch BB_LOW");
                    }
                }
                else
                {
                    Console.WriteLine("prevCandle is Touch BB_LOW");
                }

				//if (curCandle.boll_low > (curCandle.boll_low_top-5000))
				if (curCandle.boll_low > curCandle.boll_low_top )
                {
                    Console.WriteLine("not need long. boll_low is inside.");
                    result = false;
                    return result;
                }
                
                /*
				if (curCandle.isCrossBBLowTop(1000))
                {
                    Console.WriteLine("not need long. cross top's boll_low.");
                    result = false;
                    return result;
                }
                */
                
                /*
                if (!m_candleBuf.isUnderTopBB(m_config.boll_over_candle_num))
                {
                    Console.WriteLine("not need long. boll_high is not under the top.");
                    result = false;
                    return result;
                }
                */
                            
                double ema_diff = curCandle.ema - curCandle.last;
                if (ema_diff < m_config.ema_diff_far)
                {
                    Console.WriteLine("not need long. ema_diff is LOW. diff={0:0}", ema_diff);
                    result = false;
                    return result;
                }

                /*
                double diff = 0.0;
                int back_cnt = 0;
                if (m_candleBuf.searchMACross(ref diff, ref back_cnt) == 0)
                {
                    if ((back_cnt <= 0) || (back_cnt >= 90))
                    {
                        Console.WriteLine("not need long. back_cnt is over. cnt={0} diff={1:0}", back_cnt, diff);
                        result = false;
                        return result;
                    }
                }
                */

                /*
                double ma_diff = curCandle.ma - curCandle.last;
                if (ma_diff < 2000.0)
                {
                    Console.WriteLine("not need long. ma_diff is LOW. diff={0:0}", ma_diff);
                    result = false;
                    return result;
                }


                if(curCandle.isCrossBBLowTop() || (curCandle.boll_low_top < curCandle.low) )
                {
                    Console.WriteLine("not need long. Cross BBLowTop");
                    result = false;
                    return result;
                }
                */

                if (m_preLongBollLv < 0)
                {
                    //前回のLONGレベルが低い
                    Console.WriteLine("not need long. m_preLongBollLv is LOW. Lv={0}", m_preLongBollLv);
                    // 何もしない
                    result = false;
                    return result;
                }
                else
                {
                    if (m_preLongBollLv <= 0)
                    {
                        //前回のLONGレベルが0以下
                        if (prevCandle.isTrend())
                        {
                            // 前回が上昇キャンドルの場合
							if (prevCandle.isUnderBBLow(prevCandle.last))
                            {
                                // 何もしない
                                Console.WriteLine("not need long. prevCandle's last is Under BB_LOW. Lv={0}", m_preLongBollLv);
                                result = false;
                                return result;
                            }
                        }
                        else
                        {
                            // 前回が下降キャンドルの場合
                            Console.WriteLine("not need long. m_preLongBollLv is LOW but change soon. Lv={0}", m_preLongBollLv);
                            // 何もしない
                            result = false;
                            return result;
                        }
                    }

                    //前回のLONGレベルが0より大きい

                    if (m_curLongBollLv <= 0)
                    {
                        // 現在のLONGレベルが0以下
                        Console.WriteLine("not need long. m_curLongBollLv is LOW. Lv={0}", m_curLongBollLv);
                        // 何もしない
                        result = false;
                        return result;
                    }
                    else
                    {
                        // 現在のLONGレベルが0より高い
						double curDiff = curCandle.getDiff();
                        double preDiff = prevCandle.getDiff();
                        if (curDiff >= preDiff)
                        {
                            Console.WriteLine("need long. m_curLongBollLv is HIGH. Lv={0}", m_curLongBollLv);
                            // ENTRY
                            result = true;
                            return result;
                        }
                        else
                        {
							Console.WriteLine("not need long. curDiff is small than preDiff. cur={0} pre={1}", curDiff, preDiff);
                            // 何もしない
                            result = false;
                            return result;
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = false;
                m_curLongBollLv = -5;
                m_preLongBollLv = -5;
            }
            finally
            {
            }
            return result;
        }

		public bool isConditionShortEntryScam(double next_open)
        {
            bool result = false;

            try
            {
                m_curShortBollLv = -5;
                m_preShortBollLv = -5;

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

                int candle_cnt = m_candleBuf.getCandleCount();

                Candlestick curCandle = m_candleBuf.getLastCandle();
                if (curCandle == null)
                {
                    result = false;
                    return result;
                }

                int curIndex = candle_cnt - 1;

                Candlestick prevCandle = m_candleBuf.getCandle(curIndex - 1);
                if (prevCandle == null)
                {
                    result = false;
                    return result;
                }

                m_curShortBollLv = curCandle.getShortLevel(m_config.vola_big,m_config.vola_small);
                m_preShortBollLv = prevCandle.getShortLevel(m_config.vola_big, m_config.vola_small);

                if (curCandle.isTrend())
                {
                    // 現キャンドルが上昇
                    // SHORTすべきでない
                    Console.WriteLine("not need short. candle is Up-Trend.");
                    result = false;
                    return result;
                }

                if (m_config.boll_outside_check > 0)
                {
                    if ((curCandle.boll_high + m_config.boll_diff_play) > curCandle.boll_high_top)
                    {
                        // BBが上位BBをはみ出た場合
                        // SHORTすべきでない
                        Console.WriteLine("not need short. boll_high is outside.");
                        result = false;
                        return result;
                    }
                }

                Candlestick overCandle = null;
                if (!m_candleBuf.isOverBBHigh(out overCandle, m_config.boll_chk_past_num, m_config.boll_chk_play))
                {
                    // N個前のキャンドルの終値がBollHighをOVERしてない
                    // SHORTすべきでない
                    Console.WriteLine("not need short. pastCandle'last is not over BB_HIGH");
                    result = false;
                    return result;
                }

                double ema_diff = curCandle.last - curCandle.ema;
                if (ema_diff < m_config.ema_diff_far)
                {
                    // 現在の値がEMAに近い
                    // SHORTすべきでない
                    Console.WriteLine("not need short. ema_diff is LOW. diff={0:0}", ema_diff);
                    result = false;
                    return result;
                }
                //Console.WriteLine("isConditonShortEntry. ema_diff is LOW. diff={0:0} last={1:0} ema={2:0}", ema_diff, curCandle.last, curCandle.ema);

                if ((curCandle.boll_high + m_config.boll_diff_play) > curCandle.boll_high_top)
                {
                    // BBが上位BBをはみ出た場合
                    if (m_curShortBollLv <= 1)
                    {
                        // 現在のSHORTレベルが1以下
                        Console.WriteLine("not need short. m_curShortBollLv is LOW. Lv={0}", m_curShortBollLv);
                        // 何もしない
                        result = false;
                        return result;
                    }
                }
                else
                {
                    // BBが上位BBをはみ出ない場合
                    if (m_curShortBollLv <= 0)
                    {
                        // 現在のSHORTレベルが0以下
                        Console.WriteLine("not need short. m_curShortBollLv is LOW. Lv={0}", m_curShortBollLv);
                        // 何もしない
                        result = false;
                        return result;
                    }
                }

                // 現在のSHORTレベルが高い

                //if (prevCandle.isTrend())
                //{
                //    int prevCandleType = prevCandle.getUpCandleType();
                //    if (prevCandleType  > 0)
                //    {
                //        Console.WriteLine("not need short. prevCandle is not over hair. prevType={0}", prevCandleType);
                //        // 何もしない
                //        result = false;
                //        return result;
                //    }
                //}



                //double prevVolaRate = prevCandle.getVolatilityRate();
                //if (prevCandle.isTrend() && (prevVolaRate >= m_config.vola_big))
                //{
                //    Console.WriteLine("not need short. prevCandle's Vola is Big. Lv={0} VolaRate={1:0.0}", m_preShortBollLv, prevVolaRate);
                //    // 何もしない
                //    result = false;
                //    return result;
                //}


                //double curVolaRate = curCandle.getVolatilityRate();
                //if (prevCandle.isTrend() && (prevVolaRate > curVolaRate) && (m_preShortBollLv < 0))
                //{
                //    Console.WriteLine("not need short. prevCandle's Vola is Big than Cur. prev={0:0.0} cur={1:0.0}", prevVolaRate, curVolaRate);
                //    // 何もしない
                //    result = false;
                //    return result;
                //}


                //if (m_preShortBollLv <= -2)
                //{
                //    // ひとつ前のSHORTレベルが-2以下
                //    Console.WriteLine("not need short. m_preShortBollLv is LOW. Lv={0}", m_preShortBollLv);
                //    // 何もしない
                //    result = false;
                //    return result;
                //}

                //if (
                //        (curCandle.boll_high < curCandle.boll_high_top) &&
                //        ((curCandle.high >= curCandle.boll_high_top) || (prevCandle.high >= prevCandle.boll_high_top))
                //)
                //{
                //    // 何もしない
                //    result = false;
                //    return result;
                //}

                //if (curCandle.boll_high > curCandle.boll_high_top)
                //{
                //    if (curCandle.last > curCandle.boll_high)
                //    {
                //        // 何もしない
                //        result = false;
                //        return result;
                //    }
                //}

                if (curCandle.last < next_open)
                {
                    double diff = next_open - curCandle.last;
                    if (diff >= m_config.next_open_diff)
                    {
                        // 次のキャンドルのOPEN値がキャンドルの終値より大きい
                        // (SHORTしたいのに上がろうとしている)
                        Console.WriteLine("not need short. next_open is HIGH. Lv={0} Diff={1:0} last={2:0} next={3:0}", m_curLongBollLv, diff, curCandle.last, next_open);
                        // 何もしない
                        result = false;
                        return result;
                    }
                }

                bool isGolden = false;
                bool isFirst = false;
                int back_cnt = 0;
                double cur_ema_length = 0.0;
                if (m_candleBuf.getEMACrossState(out isGolden, out isFirst, out back_cnt, out cur_ema_length) != 0)
                {
                    // 何もしない
                    result = false;
                    return result;
                }

                Console.WriteLine("## getEMACrossState ## isGolden={0} isFirst={1} back={2} ema_length={3:0}", isGolden, isFirst, back_cnt, cur_ema_length);

                bool isPass = false;
                int band_pos = 0;
                if (isPassBBtoMATop(out isPass, out band_pos) != 0)
                {
                    // 何もしない
                    result = false;
                    return result;
                }

                if(isPass)
                {
					// 上位ボリンジャーバンドをはみ出てMAにタッチしていた場合
					// (上位の収束状態は終了しているので影響を受けにくい状態)
					// ENTRY
                    Console.WriteLine("need short. touched MA. Lv={0} Type={1} VolaRate={2:0}", m_curShortBollLv, curCandle.getDownCandleType(), curCandle.getVolatilityRate());
                    result = true;
                    return result;
                }

                // 上位ボリンジャーバンドをはみ出てMAにタッチしていない場合
                if ( /*(band_pos == -1) ||*/ (band_pos == 1) )
                {
                    // 上位BBバンドの上側を超えていた場合
                    // MA側に向かう下への力が強いはず
                    // 現在値より上位MAが下にあればENTRY
                    if (curCandle.last > curCandle.ma_top)
                    {
                        if (!isGolden)
                        {
                            // DEAD-CROSS
                            // ENTRY
                            Console.WriteLine("need short. over ma_top and DEAD-CROSS. last={0:0} ma_top={1:0} back_cnt={2}", curCandle.last, curCandle.ma_top, back_cnt);
                            result = true;
                            return result;
                        }
                        else
                        {
                            // GOLDEN-CROSS
                            if (isFirst)
                            {
                                Console.WriteLine("not need short.  GOLDEN-CROSS is begin. back_cnt={0}", back_cnt);
                                // 何もしない
                                result = false;
                                return result;
                            }
                            else
                            {
                                // ENTRY
                                Console.WriteLine("need short. GOLDEN-CROSS is end. back_cnt={0}", back_cnt);
                                result = true;
                                return result;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("not need short. not pass BB to MATop. Lv={0}", m_curShortBollLv);
                        // 何もしない
                        result = false;
                        return result;
                    }
                }
                else
                {
                    Console.WriteLine("not need short. not pass BB to MATop. Lv={0}", m_curShortBollLv);
                    // 何もしない
                    result = false;
                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = false;
                m_curShortBollLv = -5;
                m_preShortBollLv = -5;
            }
            finally
            {
            }
            return result;
        }

		public bool isConditionLongEntryScam(double next_open)
        {
            bool result = false;
            try
            {
                m_curLongBollLv = -5;
                m_preLongBollLv = -5;

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

                int candle_cnt = m_candleBuf.getCandleCount();

                Candlestick curCandle = m_candleBuf.getLastCandle();
                if (curCandle == null)
                {
                    result = false;
                    return result;
                }

                int curIndex = candle_cnt - 1;

                Candlestick prevCandle = m_candleBuf.getCandle(curIndex - 1);
                if (prevCandle == null)
                {
                    result = false;
                    return result;
                }

                m_curLongBollLv = curCandle.getLongLevel(m_config.vola_big, m_config.vola_small);
                m_preLongBollLv = prevCandle.getLongLevel(m_config.vola_big, m_config.vola_small);

                if (!curCandle.isTrend())
                {
                    // 現キャンドルが下降
                    // LONGすべきでない
                    Console.WriteLine("not need long. candle is Down-Trend.");
                    result = false;
                    return result;
                }

                Candlestick underCandle = null;
                if (!m_candleBuf.isUnderBBLow(out underCandle, m_config.boll_chk_past_num, m_config.boll_chk_play))
                {
                    Console.WriteLine("not need long. pastCandle's last is not under BB_LOW");
                    result = false;
                    return result;
                }


                if (m_config.boll_outside_check > 0)
                {
                    if ((curCandle.boll_low - m_config.boll_diff_play) < curCandle.boll_low_top)
                    {
                        Console.WriteLine("not need long. boll_low is outside.");
                        result = false;
                        return result;
                    }
                }

                double ema_diff = curCandle.ema - curCandle.last;
                if (ema_diff < m_config.ema_diff_far)
                {
                    Console.WriteLine("not need long. ema_diff is LOW. diff={0:0}", ema_diff);
                    result = false;
                    return result;
                }

                if ((curCandle.boll_low - m_config.boll_diff_play) < curCandle.boll_low_top)
                {
                    // BBが上位BBをはみ出た場合
                    if (m_curLongBollLv <= 1)
                    {
                        // 現在のLONGレベルが1以下
                        Console.WriteLine("not need long. m_curLongBollLv is LOW. Lv={0}", m_curLongBollLv);
                        // 何もしない
                        result = false;
                        return result;
                    }
                }
                else
                {
                    // BBが上位BBをはみ出ない場合
                    if (m_curLongBollLv <= 0)
                    {
                        // 現在のLONGレベルが0以下
                        Console.WriteLine("not need long. m_curLongBollLv is LOW. Lv={0}", m_curLongBollLv);
                        // 何もしない
                        result = false;
                        return result;
                    }
                }

                // 現在のLONGレベルが0より高い

                //if (!prevCandle.isTrend())
                //{
                //    int prevCandleType = prevCandle.getDownCandleType();
                //    if (prevCandleType > 0 )
                //    {
                //        Console.WriteLine("not need long. prevCandle is not under hair. prevType={0}", prevCandleType);
                //        // 何もしない
                //        result = false;
                //        return result;
                //    }
                //}

                //double prevVolaRate = prevCandle.getVolatilityRate();
                //if ((!prevCandle.isTrend()) && (prevVolaRate >= m_config.vola_big))
                //{
                //    Console.WriteLine("not need long. prevCandle's Vola is Big. Lv={0} VolaRate={1:0.0}", m_preLongBollLv, prevVolaRate);
                //    // 何もしない
                //    result = false;
                //    return result;
                //}

                //double curVolaRate = curCandle.getVolatilityRate();
                //if ( (!prevCandle.isTrend()) && (prevVolaRate > curVolaRate) && (m_preLongBollLv < 0) )
                //{
                //    Console.WriteLine("not need long. prevCandle's Vola is Big than Cur. prev={0:0.0} cur={1:0.0}", prevVolaRate, curVolaRate);
                //    // 何もしない
                //    result = false;
                //    return result;
                //}

                //if (m_preLongBollLv <= -2)
                //{
                //    // ひとつ前のLONGレベルが-2以下
                //    Console.WriteLine("not need long. m_preLongBollLv is LOW. Lv={0}", m_preLongBollLv);
                //    // 何もしない
                //    result = false;
                //    return result;
                //}

                //if (
                //        (curCandle.boll_low > curCandle.boll_low_top) &&
                //        ((curCandle.low <= curCandle.boll_low_top) || (prevCandle.low <= prevCandle.boll_low_top))
                //)
                //{
                //    // 何もしない
                //    result = false;
                //    return result;
                //}

                //if (curCandle.boll_low < curCandle.boll_low_top)
                //{
                //    if (curCandle.last < curCandle.boll_low)
                //    {
                //        // 何もしない
                //        result = false;
                //        return result;
                //    }
                //}

                //if (curCandle.last > next_open)
                //{
                //    double diff = curCandle.last - next_open;
                //    if (diff >= m_config.next_open_diff)
                //    {
                //        Console.WriteLine("not need long. next_open is LOW. Lv={0} Diff={1:0} last={2:0} next={3:0}", m_curLongBollLv, diff, curCandle.last, next_open);
                //        // 何もしない
                //        result = false;
                //        return result;
                //    }
                //}

                bool isGolden = false;
                bool isFirst = false;
                int back_cnt = 0;
                double cur_ema_length = 0.0;
                if (m_candleBuf.getEMACrossState(out isGolden, out isFirst, out back_cnt, out cur_ema_length) != 0)
                {
                    // 何もしない
                    result = false;
                    return result;
                }

                Console.WriteLine("## getEMACrossState ## isGolden={0} isFirst={1} back={2} ema_length={3:0}", isGolden, isFirst, back_cnt, cur_ema_length);

                bool isPass = false;
                int band_pos = 0;
                if (isPassBBtoMATop(out isPass, out band_pos) != 0)
                {
                    // 何もしない
                    result = false;
                    return result;
                }

                if (isPass)
                {
                    // 上位ボリンジャーバンドをはみ出てMAにタッチしていた場合
					// ENTRY
                    Console.WriteLine("need long. touched MA. Lv={0} Type={1} VolaRate={2:0}", m_curLongBollLv, curCandle.getUpCandleType(), curCandle.getVolatilityRate());
                    result = true;
                    return result;

                }

                // 上位ボリンジャーバンドをはみ出てMAにタッチしていない場合
                if ( (band_pos == -1) /*|| (band_pos == 1)*/)
                {
                    // 上位BBバンドの下側を超えていた場合
                    // MAに向かう上への力が強いはず
                    // 現在値より上位MAが上にあればENTRY
                    if (curCandle.last < curCandle.ma_top)
                    {
                        if (isGolden)
                        {
                            // GOLDEN-CROSS

                            // ENTRY
                            Console.WriteLine("need long. under ma_top and GOLDEN-CROSS. last={0:0} ma_top={1:0} back_cnt={2}", curCandle.last, curCandle.ma_top, back_cnt);
                            result = true;
                            return result;
                        }
                        else
                        {
                            // DEAD-CROSS
                            if (isFirst)
                            {
                                Console.WriteLine("not need long. DEAD-CROSS is begin. back_cnt={0}", back_cnt);
                                // 何もしない
                                result = false;
                                return result;
                            }
                            else
                            {
                                // ENTRY
                                Console.WriteLine("need long. DEAD-CROSS is end. back_cnt={0}", back_cnt);
                                result = true;
                                return result;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("not need long. not pass BB to MATop. Lv={0}", m_curLongBollLv);
                        // 何もしない
                        result = false;
                        return result;
                    }
                }
                else
                {
                    Console.WriteLine("not need long. not pass BB to MATop. Lv={0}", m_curLongBollLv);
                    // 何もしない
                    result = false;
                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = false;
                m_curLongBollLv = -5;
                m_preLongBollLv = -5;
            }
            finally
            {
            }
            return result;
        }

        public bool isConditionShortEntryReboundEMA(double next_open)
        {
            bool result = false;

            try
            {

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

                int candle_cnt = m_candleBuf.getCandleCount();

                Candlestick curCandle = m_candleBuf.getLastCandle();
                if (curCandle == null)
                {
                    result = false;
                    return result;
                }

                int curIndex = candle_cnt - 1;

                Candlestick prevCandle = m_candleBuf.getCandle(curIndex - 1);
                if (prevCandle == null)
                {
                    result = false;
                    return result;
                }

                int curShortBollLv = curCandle.getShortLevel();
                int preShortBollLv = prevCandle.getShortLevel();


                if (curCandle.isTouchBollLow())
                {
                    Console.WriteLine("not need short. touch BollLow. cur_emaS={0:0} cur_emaL={1:0} prev_emaS={2:0} prev_emaL={3:0}", curCandle.ema, curCandle.ema_sub, prevCandle.ema, prevCandle.ema_sub);
                    result = false;
                    return result;
                }

                if (curCandle.isTouchBollLowTop())
                {
                    Console.WriteLine("not need short. touch BollLowTop. cur_emaS={0:0} cur_emaL={1:0} prev_emaS={2:0} prev_emaL={3:0}", curCandle.ema, curCandle.ema_sub, prevCandle.ema, prevCandle.ema_sub);
                    result = false;
                    return result;
                }

                if (curCandle.isTouchBollHigh())
                {
                    Console.WriteLine("not need short. touch BollHigh. cur_emaS={0:0} cur_emaL={1:0} prev_emaS={2:0} prev_emaL={3:0}", curCandle.ema, curCandle.ema_sub, prevCandle.ema, prevCandle.ema_sub);
                    result = false;
                    return result;
                }

                if (curCandle.isTouchBollHighTop())
                {
                    Console.WriteLine("not need short. touch BollHighTop. cur_emaS={0:0} cur_emaL={1:0} prev_emaS={2:0} prev_emaL={3:0}", curCandle.ema, curCandle.ema_sub, prevCandle.ema, prevCandle.ema_sub);
                    result = false;
                    return result;
                }


                double ema_diff = Math.Abs(curCandle.ema - curCandle.last);
                if (ema_diff > m_config.ema_cross_near)
                {
                    Console.WriteLine("not need short. ema_diff is OVER. diff={0}", ema_diff);
                    // 何もしない
                    result = false;
                    return result;
                }


                if (curShortBollLv <= 0)
                {
                    // 現在のSHORTレベルが0以下
                    Console.WriteLine("not need short. curShortBollLv is LOW. Lv={0}", curShortBollLv);
                    // 何もしない
                    result = false;
                    return result;
                }
                // 現在のSHORTレベルが0より高い

                if (curCandle.last < next_open)
                {
                    double diff = next_open - curCandle.last;
                    if (diff >= m_config.next_open_diff)
                    {
                        Console.WriteLine("not need short. next_open is HIGH. Lv={0} Diff={1:0} last={2:0} next={3:0}", curShortBollLv, diff, curCandle.last, next_open);
                        // 何もしない
                        result = false;
                        return result;
                    }
                }

                if (preShortBollLv < 0)
                {
                    //前回のSHORTレベルが低い
                    Console.WriteLine("not need short. preShortBollLv is LOW. Lv={0}", preShortBollLv);
                    // 何もしない
                    result = false;
                    return result;
                }

                bool isGolden = false;
                bool isFirst = false;
                int back_cnt = 0;
                double cur_ema_length = 0.0;
                if (m_candleBuf.getEMACrossState(out isGolden, out isFirst, out back_cnt, out cur_ema_length) != 0)
                {
                    // 何もしない
                    Console.WriteLine("not need shrot. failed to getEMACrossState() cur_emaS={0:0} cur_emaL={1:0} prev_emaS={2:0} prev_emaL={3:0}", curCandle.ema, curCandle.ema_sub, prevCandle.ema, prevCandle.ema_sub);
                    result = false;
                    return result;
                }

                if (isGolden)
                {
                    //GOLDEN-CROSS
                    Console.WriteLine("not need shrot. Golden Cross cnt={0}", back_cnt);
                    // 何もしない
                    result = false;
                    return result;
                }

                if (!isGolden && !isFirst)
                {
                    //DEAD-CROSS
                    Console.WriteLine("not need shrot. Dead Cross is End. cnt={0}", back_cnt);
                    // 何もしない
                    result = false;
                    return result;
                }

                int band_pos = 0;
                if (isPassBBtoMATop(out band_pos))
                {
                    // 上位ボリンジャーバンドをはみ出てMAにタッチ済みの場合
                    Console.WriteLine("not need short. touched MA. band_pos={0}", band_pos);
                    result = false;
                    return result;
                }

                if (band_pos != 1)
                {
                    Console.WriteLine("not need short.  not-touched BBHighTop band_pos={0}", band_pos);
                    result = false;
                    return result;
                }

                // ENTRY
                Console.WriteLine("need short. curLv={0} preLv{1}", curShortBollLv, preShortBollLv);
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

        public bool isConditionLongEntryReboundEMA(double next_open)
        {
            bool result = false;

            try
            {
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

                int candle_cnt = m_candleBuf.getCandleCount();

                Candlestick curCandle = m_candleBuf.getLastCandle();
                if (curCandle == null)
                {
                    result = false;
                    return result;
                }

                int curIndex = candle_cnt - 1;

                Candlestick prevCandle = m_candleBuf.getCandle(curIndex - 1);
                if (prevCandle == null)
                {
                    result = false;
                    return result;
                }

                int curLongBollLv = curCandle.getLongLevel();
                int preLongBollLv = prevCandle.getLongLevel();


                if (curCandle.isTouchBollHigh())
                {
                    Console.WriteLine("not need long. touch BollHigh. cur_emaS={0:0} cur_emaL={1:0} prev_emaS={2:0} prev_emaL={3:0}", curCandle.ema, curCandle.ema_sub, prevCandle.ema, prevCandle.ema_sub);
                    result = false;
                    return result;
                }

                if (curCandle.isTouchBollHighTop())
                {
                    Console.WriteLine("not need long. touch BollHighTop. cur_emaS={0:0} cur_emaL={1:0} prev_emaS={2:0} prev_emaL={3:0}", curCandle.ema, curCandle.ema_sub, prevCandle.ema, prevCandle.ema_sub);
                    result = false;
                    return result;
                }

                if (curCandle.isTouchBollLow())
                {
                    Console.WriteLine("not need long. touch BollLow. cur_emaS={0:0} cur_emaL={1:0} prev_emaS={2:0} prev_emaL={3:0}", curCandle.ema, curCandle.ema_sub, prevCandle.ema, prevCandle.ema_sub);
                    result = false;
                    return result;
                }

                if (curCandle.isTouchBollLowTop())
                {
                    Console.WriteLine("not need long. touch BollLowTop. cur_emaS={0:0} cur_emaL={1:0} prev_emaS={2:0} prev_emaL={3:0}", curCandle.ema, curCandle.ema_sub, prevCandle.ema, prevCandle.ema_sub);
                    result = false;
                    return result;
                }


                double ema_diff = Math.Abs(curCandle.ema - curCandle.last);
                if (ema_diff > m_config.ema_cross_near)
                {
                    Console.WriteLine("not need long. ema_diff is OVER. diff={0}", ema_diff);
                    // 何もしない
                    result = false;
                    return result;
                }


                if (curLongBollLv <= 0)
                {
                    // 現在のLongレベルが0以下
                    Console.WriteLine("not need long. curLongBollLv is LOW. Lv={0}", curLongBollLv);
                    // 何もしない
                    result = false;
                    return result;
                }
                // 現在のLONGレベルが0より高い

                if (curCandle.last > next_open)
                {
                    double diff = next_open - curCandle.last;
                    if (diff >= m_config.next_open_diff)
                    {
                        Console.WriteLine("not need long. next_open is HIGH. Lv={0} Diff={1:0} last={2:0} next={3:0}", curLongBollLv, diff, curCandle.last, next_open);
                        // 何もしない
                        result = false;
                        return result;
                    }
                }

                if (preLongBollLv < 0)
                {
                    //前回のLONGレベルが低い
                    Console.WriteLine("not need long. preLongBollLv is LOW. Lv={0}", preLongBollLv);
                    // 何もしない
                    result = false;
                    return result;
                }

                bool isGolden = false;
                bool isFirst = false;
                int back_cnt = 0;
                double cur_ema_length = 0.0;
                if (m_candleBuf.getEMACrossState(out isGolden, out isFirst, out back_cnt, out cur_ema_length) != 0)
                {
                    // 何もしない
                    Console.WriteLine("not need long. failed to getEMACrossState() cur_emaS={0:0} cur_emaL={1:0} prev_emaS={2:0} prev_emaL={3:0}", curCandle.ema, curCandle.ema_sub, prevCandle.ema, prevCandle.ema_sub);
                    result = false;
                    return result;
                }

                if (!isGolden)
                {
                    //DEAD-CROSS
                    Console.WriteLine("not need long. Dead Cross cnt={0}", back_cnt);
                    // 何もしない
                    result = false;
                    return result;
                }

                if (!isGolden && !isFirst)
                {
                    //GOLDEN-CROSSだけど収束状態
                    Console.WriteLine("not need long. Golden Cross is End. cnt={0}", back_cnt);
                    // 何もしない
                    result = false;
                    return result;
                }

                int band_pos = 0;
                if (isPassBBtoMATop(out band_pos))
                {
                    // 上位ボリンジャーバンドをはみ出てMAにタッチ済みの場合
                    Console.WriteLine("not need long. touched MA. band_pos={0}", band_pos);
                    result = false;
                    return result;
                }

                if (band_pos != -1)
                {
                    Console.WriteLine("not need long.  not-touched BBLowTop band_pos={0}", band_pos);
                    result = false;
                    return result;
                }

                // ENTRY
                Console.WriteLine("need long. curLv={0} preLv{1}", curLongBollLv, preLongBollLv);
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


        public bool isPassBBtoMATop(out int band_pos)
        {
            bool result = false;
            band_pos = 0;
            try
            {
                if (m_candleBuf == null)
                {
                    result = false;
                    return result;
                }

                string outside_stamp = "";
                string cross_stamp = "";
                int back_cnt = 0;
                int matop_cross_cnt = 0;

                if (m_candleBuf.searchLastOutsideBB(out band_pos, out outside_stamp, out cross_stamp, out back_cnt, out matop_cross_cnt) != 0)
                {
                    result = false;
                    return result;
                }

                if (matop_cross_cnt > 0)
                {
                    Console.WriteLine("## Pass BB to MATop ##. OUTSIDE={0} CROSS={1} BACK={2} CNT={3}", outside_stamp, cross_stamp, back_cnt, matop_cross_cnt);
                    result = true;
                }
                else
                {
                    Console.WriteLine("## Not Pass BB to MATop ##. OUTSIDE={0} CROSS={1} BACK={2} CNT={3}", outside_stamp, cross_stamp, back_cnt, matop_cross_cnt);
                    result = false;
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

        public int isPassBBtoMATop(out bool isPass, out int band_pos)
        {
            int result = 0;
            isPass = false;
            band_pos = 0;
            try
            {
                if (m_candleBuf == null)
                {
                    result = -1;
                    return result;
                }

                string outside_stamp = "";
                string cross_stamp = "";
                int back_cnt = 0;
                int matop_cross_cnt = 0;

                if (m_candleBuf.searchLastOutsideBB(out band_pos, out outside_stamp, out cross_stamp, out back_cnt, out matop_cross_cnt) != 0)
                {
                    result = -1;
                    return result;
                }

                if (matop_cross_cnt > 0)
                {
                    Console.WriteLine("## Pass BB to MATop ##. OUTSIDE={0} CROSS={1} BACK={2} CNT={3}", outside_stamp, cross_stamp, back_cnt, matop_cross_cnt);
                    isPass = true;
                }
                else
                {
                    Console.WriteLine("## Not Pass BB to MATop ##. OUTSIDE={0} CROSS={1} BACK={2} CNT={3}", outside_stamp, cross_stamp, back_cnt, matop_cross_cnt);
                    isPass = false;
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

        public bool isConditionShortEntryCrossEma()
        {
            bool result = false;

            try
            {
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

                int candle_cnt = m_candleBuf.getCandleCount();

                Candlestick curCandle = m_candleBuf.getLastCandle();
                if (curCandle == null)
                {
                    result = false;
                    return result;
                }


                int curIndex = candle_cnt - 1;

                Candlestick prevCandle = m_candleBuf.getCandle(curIndex - 1);
                if (prevCandle == null)
                {
                    result = false;
                    return result;
                }

                //if (curCandle.isTouchBollLow())
                //{
                //    Console.WriteLine("not need short. touch BollLow. cur_emaS={0:0} cur_emaL={1:0} prev_emaS={2:0} prev_emaL={3:0}", curCandle.ema, curCandle.ema_sub, prevCandle.ema, prevCandle.ema_sub);
                //    result = false;
                //    return result;
                //}

                if (curCandle.isTouchBollLowTop())
                {
                    Console.WriteLine("not need short. touch BollLowTop. cur_emaS={0:0} cur_emaL={1:0} prev_emaS={2:0} prev_emaL={3:0}", curCandle.ema, curCandle.ema_sub, prevCandle.ema, prevCandle.ema_sub);
                    result = false;
                    return result;
                }


                bool isGolden = false;
                bool isFirst = false;
                int back_cnt = 0;
                double cur_ema_length = 0.0;
                if (m_candleBuf.getEMACrossState(out isGolden, out isFirst, out back_cnt, out cur_ema_length) != 0)
                {
                    // 何もしない
                    Console.WriteLine("not need short. failed to getEMACrossState() cur_emaS={0:0} cur_emaL={1:0} prev_emaS={2:0} prev_emaL={3:0}", curCandle.ema, curCandle.ema_sub, prevCandle.ema, prevCandle.ema_sub);
                    result = false;
                    return result;
                }

                if ( (!isGolden) && (back_cnt <= m_config.back_cnt))
                {
                    double ema_diff = Math.Abs(curCandle.ema - curCandle.last);
                    if ((ema_diff <= m_config.ema_cross_near) || (curCandle.ema <= curCandle.last))
                    {
                        // ENTRY
                        Console.WriteLine("need short. Dead Cross. cnt={0} diff={1:0} ema={2:0} last={3:0}", back_cnt, ema_diff, curCandle.ema, curCandle.last);
                        result = true;
                        return result;
                    }
                    else
                    {
                        // 何もしない
                        Console.WriteLine("not need short. Dead Cross. not near ema. cnt={0} diff={1:0} ema={2:0} last={3:0}", back_cnt, ema_diff, curCandle.ema, curCandle.last);
                        result = false;
                        return result;
                    }
                }
                else
                {
                    // 何もしない
					if (isGolden)
                    {
                        Console.WriteLine("not need short. Golden Cross BackCnt Over. cnt={0} ema={1:0} last={2:0}", back_cnt, curCandle.ema, curCandle.last);
                    }
                    else
                    {
                        Console.WriteLine("not need short. Dead Cross BackCnt Over. cnt={0} ema={1:0} last={2:0}", back_cnt, curCandle.ema, curCandle.last);
                    }
                    result = false;
                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = false;
                m_curShortBollLv = -5;
                m_preShortBollLv = -5;
            }
            finally
            {
            }
            return result;
        }

        public bool isConditionLongEntryCrossEma()
        {
            bool result = false;

            try
            {
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

                int candle_cnt = m_candleBuf.getCandleCount();

                Candlestick curCandle = m_candleBuf.getLastCandle();
                if (curCandle == null)
                {
                    result = false;
                    return result;
                }


                int curIndex = candle_cnt - 1;

                Candlestick prevCandle = m_candleBuf.getCandle(curIndex - 1);
                if (prevCandle == null)
                {
                    result = false;
                    return result;
                }

                //if (curCandle.isTouchBollHigh())
                //{
                //    Console.WriteLine("not need long. touch BollHigh. cur_emaS={0:0} cur_emaL={1:0} prev_emaS={2:0} prev_emaL={3:0}", curCandle.ema, curCandle.ema_sub, prevCandle.ema, prevCandle.ema_sub);
                //    result = false;
                //    return result;
                //}

                if (curCandle.isTouchBollHighTop())
                {
                    Console.WriteLine("not need long. touch BollHighTop. cur_emaS={0:0} cur_emaL={1:0} prev_emaS={2:0} prev_emaL={3:0}", curCandle.ema, curCandle.ema_sub, prevCandle.ema, prevCandle.ema_sub);
                    result = false;
                    return result;
                }

                bool isGolden = false;
                bool isFirst = false;
                int back_cnt = 0;
                double cur_ema_length = 0.0;
                if (m_candleBuf.getEMACrossState(out isGolden, out isFirst, out back_cnt, out cur_ema_length) != 0)
                {
                    // 何もしない
                    Console.WriteLine("not need long. failed to getEMACrossState() cur_emaS={0:0} cur_emaL={1:0} prev_emaS={2:0} prev_emaL={3:0}", curCandle.ema, curCandle.ema_sub, prevCandle.ema, prevCandle.ema_sub);
                    result = false;
                    return result;
                }

                if (isGolden && (back_cnt<=m_config.back_cnt) )
                {
                    double ema_diff = Math.Abs(curCandle.ema - curCandle.last);
                    if ((ema_diff <= m_config.ema_cross_near) || (curCandle.ema >= curCandle.last))
                    {
                        // ENTRY
                        Console.WriteLine("need long. Golden Cross. cnt={0} diff={1:0} ema={2:0} last={3:0}", back_cnt, ema_diff, curCandle.ema, curCandle.last);
                        result = true;
                        return result;
                    }
                    else
                    {
                        // 何もしない
                        Console.WriteLine("not need long. Golden Cross. not near ema. cnt={0} diff={1:0} ema={2:0} last={3:0}", back_cnt, ema_diff, curCandle.ema, curCandle.last);
                        result = false;
                        return result;
                    }
                }
                else
                {
                    // 何もしない
					if(isGolden)
					{
						Console.WriteLine("not need long. Golden Cross BackCnt Over. cnt={0} ema={1:0} last={2:0}", back_cnt, curCandle.ema, curCandle.last);
					}
					else
					{
						Console.WriteLine("not need long. Dead Cross BackCnt Over. cnt={0} ema={1:0} last={2:0}", back_cnt, curCandle.ema, curCandle.last);	
					}

                    result = false;
                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = false;
                m_curShortBollLv = -5;
                m_preShortBollLv = -5;
            }
            finally
            {
            }
            return result;
        }

        public bool isConditionShortEntryOverEma()
        {
            bool result = false;

            try
            {
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

                int candle_cnt = m_candleBuf.getCandleCount();

                Candlestick curCandle = m_candleBuf.getLastCandle();
                if (curCandle == null)
                {
                    result = false;
                    return result;
                }

                int curIndex = candle_cnt - 1;

                Candlestick prevCandle = m_candleBuf.getCandle(curIndex - 1);
                if (prevCandle == null)
                {
                    result = false;
                    return result;
                }

                if (curCandle.ema >= curCandle.last)
                {
                    // EMAが現在値より上にある場合はSHORTしない
                    Console.WriteLine("## OVER EMA ## not need short. ema is over. last={0:0} ema={1:0}", curCandle.last, curCandle.ema);
                    result = false;
                    return result;
                }

                double ema_diff = Math.Abs(curCandle.ema - curCandle.last);
                if (ema_diff <= m_config.ema_cross_near)
                {
                    // EMAが現在値近くにある場合はSHORTしない
                    Console.WriteLine("## OVER EMA ## not need short. ema is near. last={0:0} ema={1:0} diff={2:0}", curCandle.last, curCandle.ema, ema_diff);
                    result = false;
                    return result;
                }

                bool isGolden = false;
                bool isFirst = false;
                int back_cnt = 0;
                double cur_ema_length = 0.0;
                if (m_candleBuf.getEMACrossState(out isGolden, out isFirst, out back_cnt, out cur_ema_length) != 0)
                {
                    // 何もしない
                    Console.WriteLine("## OVER EMA ## not need short. failed to getEMACrossState() cur_emaS={0:0} cur_emaL={1:0} prev_emaS={2:0} prev_emaL={3:0}", curCandle.ema, curCandle.ema_sub, prevCandle.ema, prevCandle.ema_sub);
                    result = false;
                    return result;
                }

                if (!isGolden)
                {
                    // デッドクロス中の場合
                    // 何もしない
                    Console.WriteLine("## OVER EMA ## not need short. Dead Cross. cnt={0} ema={1:0} last={2:0}", back_cnt, curCandle.ema, curCandle.last);
                    result = false;
                    return result;
                }

                if (m_config.boll_outside_check > 0)
                {
                    if ((curCandle.boll_high + m_config.boll_diff_play) >= curCandle.boll_high_top)
                    {
                        Console.WriteLine("## OVER EMA ## not need short. boll_high is outside.");
                        result = false;
                        return result;
                    }
                }

                if (!curCandle.isTouchBollHigh())
                {
                    if (!prevCandle.isTouchBollHigh())
                    {
                        // BBバンドの上部にタッチしていない場合
                        // 何もしない
                        Console.WriteLine("## OVER EMA ## not need short. not touch Boll-High. cnt={0} ema={1:0} last={2:0}", back_cnt, curCandle.ema, curCandle.last);
                        result = false;
                        return result;
                    }
                }

                int shortLv = curCandle.getShortLevel();
                if (shortLv < 0)
                {
                    // SHORTレベルが低い
                    // 何もしない
                    Console.WriteLine("## OVER EMA ## not need short. Short-Level is LOW. cnt={0} ema={1:0} last={2:0}", back_cnt, curCandle.ema, curCandle.last);
                    result = false;
                    return result;
                }

                int prevShortLv = prevCandle.getShortLevel();
                if (prevShortLv < 0)
                {
                    // SHORTレベルが低い
                    // 何もしない
                    Console.WriteLine("## OVER EMA ## not need short. PrevShort-Level is LOW. cnt={0} ema={1:0} last={2:0}", back_cnt, curCandle.ema, curCandle.last);
                    result = false;
                    return result;
                }


                // ENTRY
                Console.WriteLine("## OVER EMA ## need short. Golden Cross's Effect is end. cnt={0} diff={1:0} ema={2:0} last={3:0}", back_cnt, ema_diff, curCandle.ema, curCandle.last);
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

        public bool isConditionLongEntryOverEma()
        {
            bool result = false;

            try
            {
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

                int candle_cnt = m_candleBuf.getCandleCount();

                Candlestick curCandle = m_candleBuf.getLastCandle();
                if (curCandle == null)
                {
                    result = false;
                    return result;
                }

                int curIndex = candle_cnt - 1;

                Candlestick prevCandle = m_candleBuf.getCandle(curIndex - 1);
                if (prevCandle == null)
                {
                    result = false;
                    return result;
                }

                if (curCandle.ema <= curCandle.last)
                {
                    // EMAが現在値より下にある場合はLONGしない
                    Console.WriteLine("## OVER EMA ## not need long. ema is under. last={0:0} ema={1:0}", curCandle.last, curCandle.ema);
                    result = false;
                    return result;
                }

                double ema_diff = Math.Abs(curCandle.ema - curCandle.last);
                if (ema_diff <= m_config.ema_cross_near)
                {
                    // EMAが現在値近くにある場合はLONGしない
                    Console.WriteLine("## OVER EMA ## not need long. ema is near. last={0:0} ema={1:0} diff={2:0}", curCandle.last, curCandle.ema, ema_diff);
                    result = false;
                    return result;
                }

                bool isGolden = false;
                bool isFirst = false;
                int back_cnt = 0;
                double cur_ema_length = 0.0;
                if (m_candleBuf.getEMACrossState(out isGolden, out isFirst, out back_cnt, out cur_ema_length) != 0)
                {
                    // 何もしない
                    Console.WriteLine("## OVER EMA ## not need long. failed to getEMACrossState() cur_emaS={0:0} cur_emaL={1:0} prev_emaS={2:0} prev_emaL={3:0}", curCandle.ema, curCandle.ema_sub, prevCandle.ema, prevCandle.ema_sub);
                    result = false;
                    return result;
                }

                if (isGolden)
                {
                    // ゴールデンクロス中の場合
                    // 何もしない
                    Console.WriteLine("## OVER EMA ## not need long. Golden Cross. cnt={0} ema={1:0} last={2:0}", back_cnt, curCandle.ema, curCandle.last);
                    result = false;
                    return result;
                }

                if (m_config.boll_outside_check > 0)
                {
                    if ((curCandle.boll_low - m_config.boll_diff_play) <= curCandle.boll_low_top)
                    {
                        Console.WriteLine("## OVER EMA ## not need long. boll_low is outside.");
                        result = false;
                        return result;
                    }
                }


                if (!curCandle.isTouchBollLow())
                {
                    if (!prevCandle.isTouchBollLow())
                    {
                        // BBバンドの下部にタッチしていない場合
                        // 何もしない
                        Console.WriteLine("## OVER EMA ## not need long. not touch Boll-Low. cnt={0} ema={1:0} last={2:0}", back_cnt, curCandle.ema, curCandle.last);
                        result = false;
                        return result;
                    }
                }

                int longLv = curCandle.getLongLevel();
                if (longLv < 0)
                {
                    // LONGレベルが低い
                    // 何もしない
                    Console.WriteLine("## OVER EMA ## not need long. Long-Level is LOW. cnt={0} ema={1:0} last={2:0}", back_cnt, curCandle.ema, curCandle.last);
                    result = false;
                    return result;
                }

                int prevLongLv = prevCandle.getLongLevel();
                if (prevLongLv < 0)
                {
                    // LONGレベルが低い
                    // 何もしない
                    Console.WriteLine("## OVER EMA ## not need long. PrevLong-Level is LOW. cnt={0} ema={1:0} last={2:0}", back_cnt, curCandle.ema, curCandle.last);
                    result = false;
                    return result;
                }

                // ENTRY
                Console.WriteLine("## OVER EMA ## need long. Dead Cross's Effect is end. cnt={0} diff={1:0} ema={2:0} last={3:0}", back_cnt, ema_diff, curCandle.ema, curCandle.last);
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



        public bool isConditionShortExitCrossEma()
        {
            bool result = false;
            try
            {
                if (m_candleBuf == null)
                {
                    result = false;
                    return result;
                }

                int candle_cnt = m_candleBuf.getCandleCount();

                Candlestick curCandle = m_candleBuf.getLastCandle();
                if (curCandle == null)
                {
                    result = false;
                    return result;
                }
                int curIndex = candle_cnt - 1;

                Candlestick prevCandle = m_candleBuf.getCandle(curIndex - 1);
                if (prevCandle == null)
                {
                    result = false;
                    return result;
                }

                double profit = curCandle.last - m_position.entry_price;

                bool isGolden = false;
                bool isFirst = false;
                int back_cnt = 0;
                double cur_ema_length = 0.0;
                if (m_candleBuf.getEMACrossState(out isGolden, out isFirst, out back_cnt, out cur_ema_length, 1.0) != 0)
                {
                    result = false;
                    return result;
                }

                //if (!curCandle.isTouchBollLow())
                if (!curCandle.isTouchBollLowTop())
                {
                    //Console.WriteLine("not exit short. inside BOLL_LOW_TOP. bollLtop={0:0} last={1:0}", curCandle.boll_low_top, curCandle.last);
                    result = false;
                    return result;
                }

                //if ((!isGolden) && isFirst)
                //{
                //    // DEADクロスが初動の場合

                //    int shortLv = curCandle.getShortLevel();
                //    if (shortLv <= 0)
                //    {
                //        // SHORTレベルが低くなった場合
                //        Console.WriteLine("exit short. DeadCrossFirst. shortLv is Low. Lv={0} bollLtop={1:0} last={2:0}", shortLv, curCandle.boll_low_top, curCandle.last);
                //        // SHORT-EXIT
                //        result = true;
                //        return result;
                //    }
                //    else
                //    {
                //        // SHORTレベルまだ高い場合
                //        // SHORT継続
                //        //Console.WriteLine("not exit short. DeadCrossFirst. shortLv is High. Lv={0} bollLtop={1:0} last={2:0}", shortLv, curCandle.boll_low_top, curCandle.last);
                //        result = false;
                //        return result;
                //    }
                //}

                //if (profit > 0.0)
                {
                    Console.WriteLine("exit short. cnt={0} bollLtop={1:0} last={2:0}", back_cnt, curCandle.boll_low_top, curCandle.last);
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

        public bool isConditionLongExitCrossEma()
        {
            bool result = false;
            try
            {
                if (m_candleBuf == null)
                {
                    result = false;
                    return result;
                }

                int candle_cnt = m_candleBuf.getCandleCount();

                Candlestick curCandle = m_candleBuf.getLastCandle();
                if (curCandle == null)
                {
                    result = false;
                    return result;
                }

                int curIndex = candle_cnt - 1;

                Candlestick prevCandle = m_candleBuf.getCandle(curIndex - 1);
                if (prevCandle == null)
                {
                    result = false;
                    return result;
                }

                double profit = curCandle.last - m_position.entry_price;

                bool isGolden = false;
                bool isFirst = false;
                int back_cnt = 0;
                double cur_ema_length = 0.0;
                if (m_candleBuf.getEMACrossState(out isGolden, out isFirst, out back_cnt, out cur_ema_length, 1.0) != 0)
                {
                    result = false;
                    return result;
                }


                //if (!curCandle.isTouchBollHigh())
                if (!curCandle.isTouchBollHighTop())
                {
                    //Console.WriteLine("not exit long. GoldCrossFirst. inside BOLL_HIGH_TOP. bollHtop={0:0} last={1:0}", curCandle.boll_high_top, curCandle.last);
                    result = false;
                    return result;
                }

                //if (isGolden && isFirst)
                //{
                //    // GOLDENクロスが初動の場合

                //    int longLv = curCandle.getLongLevel();
                //    if (longLv <= 0)
                //    {
                //        // LONGレベルが低くなった場合
                //        Console.WriteLine("exit long. GoldCrossFirst. longLv is Low. Lv={0} bollLtop={1:0} last={2:0}", longLv, curCandle.boll_low_top, curCandle.last);
                //        // LONG-EXIT
                //        result = true;
                //        return result;
                //    }
                //    else
                //    {
                //        // LONGレベルまだ高い場合
                //        // LONG継続
                //        //Console.WriteLine("not exit long. GoldenCrossFirst. longLv is High. Lv={0} bollLtop={1:0} last={2:0}", longLv, curCandle.boll_low_top, curCandle.last);
                //        result = false;
                //        return result;
                //    }
                //}

                //if (profit > 0.0)
                {
                    Console.WriteLine("exit long. cnt={0} bollLtop={1:0} last={2:0}", back_cnt, curCandle.boll_low_top, curCandle.last);
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

        public bool isConditionEntryHige()
        {
            bool result = false;

            try
            {
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

                if (curCandle.last >= curCandle.body_max)
                {
                    result = false;
                    return result;
                }

                if (curCandle.last <= curCandle.body_min)
                {
                    result = false;
                    return result;
                }

                // ENTRY
                Console.WriteLine("need short. boll_high is inside boll_high_top ");
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

        public bool isConditionShortEntryHige()
        {
            bool result = false;

            try
            {
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

                // 小ボリンジャーが大ボリンジャーをはみ出た場合はEntryしない
                if (curCandle.boll_high > curCandle.boll_high_top)
                {
                    Console.WriteLine("not need short. boll_high is outside boll_high_top.");
                    result = false;
                    return result;
                }

                // ENTRY
                Console.WriteLine("need short. boll_high is inside boll_high_top ");
                result = true;
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = false;            }
            finally
            {
            }
            return result;
        }

        public bool isConditionLongEntryHige()
        {
            bool result = false;

            try
            {
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

                // 小ボリンジャーが大ボリンジャーをはみ出た場合はEntryしない
                if (curCandle.boll_low < curCandle.boll_low_top)
                {
                    Console.WriteLine("not need long. boll_low is outside boll_low_top.");
                    result = false;
                    return result;
                }

                // ENTRY
                Console.WriteLine("need long. boll_low is inside boll_low_top ");
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

        public bool isConditionCancelHige()
        {
            bool result = false;

            try
            {
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

                //if ((curCandle.last > curCandle.body_min) && (curCandle.last < curCandle.body_max))
                //{
                //    result = false;
                //    return result;
                //}gaz
                //// CANCEL
                //Console.WriteLine("need cancel. last={0:0} body_min={1:0} body_max={2:0} under={3:0} over={4:0}"
                //    , curCandle.last
                //    , curCandle.body_min
                //    , curCandle.body_max
                //    , curCandle.body_min - curCandle.last
                //    , curCandle.last - curCandle.body_min
                //);

                double buy_diff = Math.Abs(m_position.limit_buy_price - curCandle.last);
                double sell_diff = Math.Abs(m_position.limit_sell_price - curCandle.last);


                double buy_thr = 2500;//curCandle.hige_bottom_max * 0.3;// 0.3;
                double sell_thr = 2500;//curCandle.hige_top_max * 0.3; // 0.3;

                if ( (buy_diff >= buy_thr) && (sell_diff >= sell_thr))
                {
                    //Console.WriteLine("not need cancel. buy_diff={0:0} sell_diff={1:0} buy_thr={2:0} sell_thr={3:0}", buy_diff, sell_diff, buy_thr, sell_thr);
                    result = false;
                    return result;
                }



                // CANCEL
                Console.WriteLine("need cancel. buy_diff={0:0} sell_diff={1:0} buy_thr={2:0} sell_thr={3:0}", buy_diff, sell_diff, buy_thr, sell_thr);



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

        public bool isConditionExitHige()
        {
            bool result = false;

            try
            {
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

                double profit = m_position.calcProfit(curCandle.last);

                if ((curCandle.last > curCandle.body_min) && (curCandle.last < curCandle.body_max))
                {
                    // EXIT
                    Console.WriteLine("need exit. last={0:0} body_min={1:0} body_max={2:0} under={3:0} over={4:0}"
                        , curCandle.last
                        , curCandle.body_min
                        , curCandle.body_max
                        , curCandle.body_min - curCandle.last
                        , curCandle.last - curCandle.body_min
                    );
                    result = true;
                    return result;
                }

                //if (m_position.entry_date != curCandle.timestamp)
                //{
                //    // Entry時とキャンドルが変わった場合
                //    // EXIT
                //    Console.WriteLine("need exit. changed candle.  profit={0:0}  entry={1} cur={2}", profit, m_position.entry_date, curCandle.timestamp);

                //    result = true;
                //    return result;
                //}

                //double hige_length = 0.0;
                //if (m_position.isLong())
                //{
                //    hige_length = curCandle.hige_bottom_max;
                //}
                //else if (m_position.isShort())
                //{
                //    hige_length = curCandle.hige_top_max;
                //}
                //else
                //{
                //    result = false;
                //    return result;
                //}

                //if ( (profit > 0) && (profit > hige_length) )
                //{
                //    // プラス利益かつヒゲより長い場合
                //    // Exit
                //    Console.WriteLine("need exit. profit is longer than hige. profit={0:0} entry={1:0} last={2:0}", profit, m_position.entry_price, curCandle.last);
                //    result = true;
                //    return result;
                //}

                //if ((profit <= 0) && (-profit > hige_length))
                //{
                //    // マイナス利益かつヒゲより長い場合
                //    // Losscut
                //    Console.WriteLine("need exit(losscut). profit={0:0} entry={1:0} last={2:0}", profit, m_position.entry_price, curCandle.last);

                //    result = true;
                //    return result;
                //}
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

        public async Task<int> tryEntryOrderHige()
        {
            int result = 0;
            try
            {
                if (!m_position.isNone())
                {
                    result = 1;
                    return result;
                }
                // PositionがNONEの場合

                if (!m_position.isEntryNone())
                {
                    result = 1;
                    return result;
                }
                // Entry注文がNONEの場合

                if (!m_position.isExitNone())
                {
                    result = 1;
                    return result;
                }
                // Exit注文していない場合

                Candlestick curCandle = m_candleBuf.getLastCandle();
                if (curCandle == null)
                {
                    result = -1;
                    return result;
                }

                // NONEポジションの場合


                bool isEntry = true;// isConditionEntryHige();

                if (isEntry)
                {
                    if (curCandle.disparity_rate >= 5.0)
                    {
                        postSlack(string.Format("cancel Long Entry Order. DispartyRate is Over. rate={0:0.00}.", curCandle.disparity_rate));
                        result = -1;
                        return result;
                    }


                    double buy_price = curCandle.last + 3000;//curCandle.range_min;//Math.Round( curCandle.last - curCandle.hige_bottom_max);
                    double sell_price = curCandle.last - 3000;//curCandle.range_max;//Math.Round(curCandle.last + curCandle.hige_top_max);


                    SendParentOrderResponse retObj = await SendParentOrder.SendStopLimitOCO(m_authBitflyer, m_config.product_bitflyer, m_config.amount, buy_price, sell_price);
                    if (retObj == null)
                    {
                        postSlack("failed to OCO Entry Order");
                        result = -1;
                        return result;
                    }
                    // 注文成功
                    m_position.entryOrder(retObj.parent_order_acceptance_id, curCandle.timestamp);
                    m_position.limit_buy_price = buy_price;
                    m_position.limit_sell_price = sell_price;
                    m_position.strategy_type = Position.StrategyType.HIGE;

                    postSlack(
                        string.Format("{0} OCO Entry Order ID = {1} buy={2:0} sell={3:0} body_min={4:0} body_max={5:0}"
                            , curCandle.timestamp
                            , retObj.parent_order_acceptance_id
                            , buy_price
                            , sell_price
                            , curCandle.body_min
                            , curCandle.body_max
                        )
                    );
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

                bool isCancel = isConditionCancelHige();
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

        //public async Task<int> checkCancelOrder()
        //{
        //    int result = 0;
        //    try
        //    {
        //        if (!m_position.isNone())
        //        {
        //            result = 1;
        //            return result;
        //        }
        //        // NONEポジションの場合

        //        if (!m_position.isEntryActive())
        //        {
        //            result = 1;
        //            return result;
        //        }
        //        // ENTRYアクティブの場合


        //        bool isCanceled = await SendChildOrder.isCanceldChildOrders(m_authBitflyer, m_config.product_bitflyer, m_position.entry_child_ids);
        //        if (!isCanceled)
        //        {
        //            result = 1;
        //            return result;
        //        }

        //        postSlack(string.Format("Order is canceld. Order ID = {0}", m_position.entry_id));

        //        m_position.cancel();

        //        m_position = new Position();
        //        m_posArray.Add(m_position);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex);
        //        result = -1;
        //    }
        //    finally
        //    {
        //    }
        //    return result;
        //}

        public async Task<int> tryExitOrderHige()
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

                bool isExit = isConditionExitHige();
                if (!isExit)
                {
                    result = 1;
                    return result;
                }

                if (m_position.isLong())
                {// LONGの場合

                    bool isCond = true;
                    if (isCond)
                    {
                        //Console.WriteLine("Try Long Exit Order.");
                        SendChildOrderResponse retObj = null;
                        int retry_cnt = 0;
                        while (true)
                        {
                            retry_cnt++;
                            retObj = await SendChildOrder.SellMarket(m_authBitflyer, m_config.product_bitflyer, m_config.amount);
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

                    bool isCond = true;
                    if (isCond)
                    {
                        //Console.WriteLine("Try Short Exit Order.");
                        SendChildOrderResponse retObj = null;
                        int retry_cnt = 0;
                        while (true)
                        {
                            retry_cnt++;
                            retObj = await SendChildOrder.BuyMarket(m_authBitflyer, m_config.product_bitflyer, m_config.amount);
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


    }
}
