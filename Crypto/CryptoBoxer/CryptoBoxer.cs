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

        private int applyCandlestick(CandleBuffer candleBuf, ref BitflyerOhlc ohlc, int begIdx, int count)
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

                    // Cryptowatchでとれるohlcは閉じてないキャンドルの値も取得される。
                    //  1回目 2018/04/11 10:14:00, open=743093, close=743172, high=743200, low=743093
                    //  2回目 2018/04/11 10:14:00, open=743093, close=743194, high=743200, low=743020
                    // Timestampが10:14:00なら、10:13:00～10:13:59のキャンドル


                    // 2018/04/10 19:21:00
                    DateTime timestamp = DateTimeOffset.FromUnixTimeSeconds((long)closeTime).LocalDateTime;
                    Console.WriteLine("{0}, open={1}, close={2}, high={3}, low={4}, vol={5}", timestamp.ToString(), openPrice, closePrice, highPrice, lowPrice, volume);

                    Candlestick candle = candleBuf.addCandle(highPrice, lowPrice, openPrice, closePrice, timestamp.ToString());
                    if (candle == null)
                    {
                        Console.WriteLine("failed to addCandle.");
                        continue;
                    }
                    candle.volume = volume;

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
                    if (candleBuf.calcEma(out ema, m_config.ema_sample_num) == 0)
                    {
                        candle.ema = ema;
                    }
                }

                {
                    double ema = 0.0;
                    if (candleBuf.calcEma(out ema, 20) == 0)
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

                //{
                //    double ma_top_increase_ma = 0.0;
                //    if (candleBuf.calcMATopIncreaseMA(out ma_top_increase_ma, 20) == 0)
                //    {
                //        candle.ma_top_increase_ma = ma_top_increase_ma;
                //    }
                //}
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
                System.Threading.Thread.Sleep(3000);

                postSlack(string.Format("amount={0}", m_config.amount));
                postSlack(string.Format("periods={0}", m_config.periods));
                postSlack(string.Format("product={0}", m_config.product_bitflyer));
                postSlack(string.Format("ema_sample_num={0}", m_config.ema_sample_num));
                postSlack(string.Format("boll_sample_num={0}", m_config.boll_sample_num));
                postSlack(string.Format("boll_top_sample_num={0}", m_config.boll_top_sample_num));
                postSlack(string.Format("boll_over_candle_num={0}", m_config.boll_over_candle_num));
                postSlack(string.Format("ema_diff_far={0}", m_config.ema_diff_far));
                postSlack(string.Format("ema_diff_near={0}", m_config.ema_diff_near));
                postSlack(string.Format("losscut_value={0}", m_config.losscut_value));
                postSlack(string.Format("buffer_num={0}", m_config.buffer_num));
                postSlack(string.Format("backtest_hour={0}", m_config.backtest_hour));

                System.Threading.Thread.Sleep(3000);

                // Cryptowatchから過去のデータを取得
                BitflyerOhlc ohlc = await BitflyerOhlc.GetOhlcAfterAsync(m_config.product_cryptowatch, m_config.periods, (m_candleBuf.m_buffer_num) * m_config.periods);
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
                        if (diff_sec<=0.0)
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
                            await tryEntryOrder();
                            await tryExitOrder();

                            // Losscutロジック
                            await tryLosscutOrder();

                            Console.WriteLine("closed candle. timestamp={0},last={1},ema={2:0},B_H={3:0},B_L={4:0},vol={5:0},volma={6:0},curL={7},preL={8},curS={9},preS={10},ema={11:0},sfd={12:0.00}"
                                              , curCandle.timestamp
                                              , curCandle.last
                                              , curCandle.last - curCandle.ema
                                              , curCandle.boll_high - curCandle.last
                                              , curCandle.last - curCandle.boll_low
                                              , curCandle.volume
                                              , curCandle.volume_ma
                                              , m_curLongBollLv
                                              , m_preLongBollLv
                                              , m_curShortBollLv
                                              , m_preShortBollLv
                                              , curCandle.ema
                                              , curCandle.disparity_rate
                            );
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

                    // ENTRY/EXITロジック
                    //await tryEntryOrder();
                    //await tryExitOrder();

                    // Losscutロジック
                    //await tryLosscutOrder();

                    // 注文状況確認ロジック
                    await checkEntry();
                    await checkExit();

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
                int test_num = (m_config.backtest_hour * 60);
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
                if (applyCandlestick(testCandleBuf, ref ohlc, m_candleBuf.getCandleCount(), test_num) != 0)
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

                    if (m_candleBuf.addCandle(curCandle) != 0)
                    {
                        Console.WriteLine("failed to addCandle for m_candleBuf.");
                        return;
                    }

                    // ENTRYテスト
                    tryEntryOrderTest(ref long_entry_cnt, ref short_entry_cnt);
                    checkEntryTest(curCandle.last);

                    // EXIT/ロスカットテスト
                    tryExitOrderTest(ref long_exit_cnt, ref short_exit_cnt);
                    tryLosscutOrderTest(ref long_lc_cnt, ref short_lc_cnt);
                    checkExitTest(curCandle.last);


                    Console.WriteLine("closed candle. timestamp={0},profit_sum={1},last={2:0},ema={3:0},B_H={4:0},B_L={5:0},B_HT={6:0},B_LT={7:0}"
                                      , curCandle.timestamp
                                      , m_profitSum
                                      , curCandle.last
                                      , curCandle.ema
                                      , curCandle.boll_high
                                      , curCandle.boll_low
                                      , curCandle.boll_high_top
                                      , curCandle.boll_low_top
                    );


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

        public async Task<int> tryEntryOrder()
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
                bool isLong = isConditionLongEntryScam();
                bool isShort= isConditionShortEntryScam();



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
                    postSlack(string.Format("{0} Long Entry Order ID = {1}", curCandle.timestamp, retObj.child_order_acceptance_id));
                    m_position.entryLongOrder(retObj.child_order_acceptance_id, curCandle.timestamp);
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
                    postSlack(string.Format("{0} Short Entry Order ID = {1}", curCandle.timestamp, retObj.child_order_acceptance_id));
                    m_position.entryShortOrder(retObj.child_order_acceptance_id, curCandle.timestamp);
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

        private int tryEntryOrderTest(ref int long_entry_cnt, ref int short_entry_cnt)
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
                bool isLong = isConditionLongEntryScam();
                bool isShort = isConditionShortEntryScam();

                if (isLong)
                {
                    // 注文成功
                    string long_id = string.Format("BT_LONG_ENTRY_{0:D8}", long_entry_cnt);
                    postSlack(string.Format("{0} Long Entry Order ID = {1}", curCandle.timestamp, long_id), true);
                    m_position.entryLongOrder(long_id, curCandle.timestamp);
                    //double diff_rate = 0.0;
                    //int cnt = 0;
                    //if (m_candleBuf.searchMACross(ref diff_rate, ref cnt) == 0)
                    //{
                    //    m_position.entry_increase = cnt;//curCandle.ema_angle;
                    //}
                    long_entry_cnt++;
                }
                else if (isShort)
                {
                    // 注文成功
                    string short_id = string.Format("BT_SHORT_ENTRY_{0:D8}", short_entry_cnt);
                    postSlack(string.Format("{0} Short Entry Order ID = {1}", curCandle.timestamp, short_id),true);
                    m_position.entryShortOrder(short_id, curCandle.timestamp);
                    //double diff_rate = 0.0;
                    //int cnt = 0;
                    //if (m_candleBuf.searchMACross(ref diff_rate, ref cnt) == 0)
                    //{
                    //    m_position.entry_increase = cnt;//curCandle.ema_angle;
                    //}
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

                GetchildorderResponse responce = await SendChildOrder.getChildOrderAveragePrice(m_authBitflyer, m_config.product_bitflyer, m_position.entry_id);
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

                    if (isConditionLongExit())
                    {
                        //Console.WriteLine("Try Long Exit Order.");

                        SendChildOrderResponse retObj = await SendChildOrder.SellMarket(m_authBitflyer, m_config.product_bitflyer, m_config.amount);
                        if (retObj == null)
                        {
                            postSlack("failed to Long Exit Order.");
                            result = -1;
                            return result;
                        }
                        // 注文成功
                        postSlack(string.Format("{0} Long Exit Order ID = {1}", curCandle.timestamp, retObj.child_order_acceptance_id));
                        m_position.exitOrder(retObj.child_order_acceptance_id, curCandle.timestamp);
                    }
                }
                else if (m_position.isShort())
                {// SHORTの場合
                    if (isConditionShortExit())
                    {
                        //Console.WriteLine("Try Short Exit Order.");

                        SendChildOrderResponse retObj = await SendChildOrder.BuyMarket(m_authBitflyer, m_config.product_bitflyer, m_config.amount);
                        if (retObj == null)
                        {
                            postSlack("failed to Short Exit Order.");
                            result = -1;
                            return result;
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

                    if (isConditionLongExit())
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
                    if (isConditionShortExit())
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
                // EXITアクティブの場合

                GetchildorderResponse responce = await SendChildOrder.getChildOrderAveragePrice(m_authBitflyer, m_config.product_bitflyer, m_position.exit_id);
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

        public int checkExitTest(double last_price)
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
                m_position.exit(last_price);
                m_profitSum += m_position.getProfit();
                postSlack(string.Format("Order is completed. profit={0:0} sum={1:0} exit_price={2:0} id={3}"
                                        , m_position.getProfit()
                                        , m_profitSum
                                        , last_price
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

                //bool isOverEma = false;
                //if (curCandle.ema <= curCandle.last)
                //{
                //    isOverEma = true;
                //}

                //bool isNearEma = false;
                double ema_diff = curCandle.ema - curCandle.last;
                if (ema_diff <= m_config.ema_diff_near)
                {
                    //isNearEma = true;
                    result = true;
                    return result;
                }

                //if (isNearEma || isOverEma)
                //{
                //    int curLastLv = curCandle.getLastLevel();
                //    int curOpenLv = curCandle.getOpenLevel();
                //    if (curCandle.isTrend())
                //    {//上昇キャンドルなら
                //        if (curLastLv == 4 && curOpenLv == 0)
                //        {// 大陽線
                //            // SKIP
                //            Console.WriteLine("skip LONG-EXIT. curLastLv={0} curOpenLv={1}", curLastLv, curOpenLv);
                //            result = false;
                //            return result;
                //        }
                //        else
                //        {// 上髭
                //            // EXIT
                //            result = true;
                //            return result;
                //        }
                //    }
                //    else
                //    {//下降キャンドルなら
                //        // EXIT
                //        result = true;
                //        return result;
                //    }
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

                //bool isUnderEma = false;
                if (curCandle.ema >= curCandle.last)
                {
                    //isUnderEma = true;
                    result = true;
                    return result;
                }

                //bool isNearEma = false;
                //double ema_diff = curCandle.last - curCandle.ema;
                //if (ema_diff <= m_config.ema_diff_near)
                //{
                //    isNearEma = true;
                //}

                //if (isNearEma || isUnderEma)
                //{
                //    int curLastLv = curCandle.getLastLevel();
                //    int curOpenLv = curCandle.getOpenLevel();
                //    if (!curCandle.isTrend())
                //    {//下降キャンドルなら
                //        if (curLastLv == 0 && curOpenLv==4)
                //        {// 大陰線
                //            // SKIP
                //            Console.WriteLine("skip LONG-EXIT. curLastLv={0} curOpenLv={1}", curLastLv, curOpenLv);
                //            result = false;
                //            return result;
                //        }
                //        else
                //        {//下髭
                //            // EXIT
                //            result = true;
                //            return result;
                //        }
                //    }
                //    else
                //    {//上昇キャンドルなら
                //        // EXIT
                //        result = true;
                //        return result;
                //    }
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
						// 一つ前のキャンドルと過去のキャンドルがBollHighにタッチしていない
                        // SHORTすべきでない
                        result = false;
                        return result;
                    }
                    else
                    {
						// 過去のキャンドルがBollHighにタッチ
                        Console.WriteLine("pastCandle is Touch BB_HIGH");
                    }
                }
                else
                {
					// 一つ前のキャンドルがBollHighにタッチしていない
                    Console.WriteLine("prevCandle is Touch BB_HIGH");
                }

				if (curCandle.boll_high < (curCandle.boll_high_top+5000))
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
                
                if(!m_candleBuf.isOverTopBB(m_config.boll_over_candle_num))
                {
                    Console.WriteLine("not need short. boll_high is not over the top.");
                    result = false;
                    return result;
                }            

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

				if (curCandle.boll_low > (curCandle.boll_low_top-5000))
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
                
                if (!m_candleBuf.isUnderTopBB(m_config.boll_over_candle_num))
                {
                    Console.WriteLine("not need long. boll_high is not under the top.");
                    result = false;
                    return result;
                }
                
                            
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

		public bool isConditionShortEntryScam()
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

                m_curShortBollLv = curCandle.getShortLevel();
                m_preShortBollLv = prevCandle.getShortLevel();

				if ((curCandle.boll_high+ m_config.boll_diff_play) > curCandle.boll_high_top)
                {
                    Console.WriteLine("not need short. boll_high is outside.");
                    result = false;
                    return result;
                }

                if (!prevCandle.isOverBBHigh(prevCandle.last))
                {
					// 一つ前のキャンドルの終値がBollHighをOVERしてない
                    // SHORTすべきでない
					Console.WriteLine("not need short. prevCandle'last is not over BB_HIGH");
                    result = false;
                    return result;
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

                    bool isCond = true;
                    double curVola = curCandle.getVolatility();
                    double preVola = prevCandle.getVolatility();
                    if (!curCandle.isTrend() && prevCandle.isTrend())
                    {
                        double rate = curVola / preVola * 100.0;
                        if (rate < m_config.vola_rate)
                        {
                            isCond = false;
                        }
                    }

                    if (isCond)
                    {

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
                                    // ENTRY
                                    Console.WriteLine("need short. Touch BB_LOW. MA_TOP is OVER. Lv={0}", m_curShortBollLv);
                                    result = true;
                                    return result;
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
                                    // ENTRY
                                    Console.WriteLine("need short. Touch BB_HIGH. MA_TOP is UNDER. Lv={0}", m_curShortBollLv);
                                    result = true;
                                    return result;
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
                    else
                    {
                        Console.WriteLine("not need long. preDiff is small. Lv={0}", m_curLongBollLv);
                        // 何もしない
                        result = false;
                        return result;
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

		public bool isConditionLongEntryScam()
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

                m_curLongBollLv = curCandle.getLongLevel();
                m_preLongBollLv = prevCandle.getLongLevel();

                if (!prevCandle.isUnderBBLow(prevCandle.last))
                {
					Console.WriteLine("not need long. prevCandle'last is not under BB_LOW");
					result = false;
                    return result;
                }
                else
                {
                    Console.WriteLine("prevCandle'last is under BB_LOW");
                }

				if ((curCandle.boll_low-m_config.boll_diff_play) < curCandle.boll_low_top)
                {
                    Console.WriteLine("not need long. boll_low is outside.");
                    result = false;
                    return result;
                }            

                double ema_diff = curCandle.ema - curCandle.last;
                if (ema_diff < m_config.ema_diff_far)
                {
                    Console.WriteLine("not need long. ema_diff is LOW. diff={0:0}", ema_diff);
                    result = false;
                    return result;
                }
                
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

                    bool isCond = true;
                    double curVola = curCandle.getVolatility();
                    double preVola = prevCandle.getVolatility();
                    if (curCandle.isTrend() && !prevCandle.isTrend())
                    {
                        double rate = curVola / preVola * 100.0;
                        if (rate < m_config.vola_rate)
                        {
                            isCond = false;
                        }
                    }

                    if (isCond)
                    {
                        int band_pos = 0;
                        if (isPassBBtoMATop(out band_pos))
                        {
                            // 上位ボリンジャーバンドをはみ出てMAにタッチしていた場合
                            // ENTRY
                            Console.WriteLine("need long. m_curLongBollLv is HIGH. Lv={0}", m_curLongBollLv);
                            result = true;
                            return result;
                        }
                        else
                        {
                            // 上位ボリンジャーバンドをはみ出てMAにタッチしていない場合
                            if (band_pos == -1)
                            {
                                // 上位BBバンドの下側を超えていた場合
                                // MAに向かう上への力が強いはず
                                // 現在値より上位MAが上にあればENTRY
                                if (curCandle.last < curCandle.ma_top)
                                {
                                    // ENTRY
                                    Console.WriteLine("need long. Touch BB_LOW. MA_TOP is OVER. Lv={0}", m_curLongBollLv);
                                    result = true;
                                    return result;
                                }
                                else
                                {
                                    Console.WriteLine("not need short. not pass BB to MATop. Lv={0}", m_curLongBollLv);
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
                                    // ENTRY
                                    Console.WriteLine("need long. Touch BB_HIGH. MA_TOP is OVER. Lv={0}", m_curLongBollLv);
                                    result = true;
                                    return result;
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
                    else
                    {
                        Console.WriteLine("not need long. preDiff is big. Lv={0}", m_curLongBollLv);
                        // 何もしない
                        result = false;
                        return result;
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
                    Console.WriteLine("Pass BB to MATop. OUTSIDE={0} CROSS={1} BACK={2} CNT={3}", outside_stamp, cross_stamp, back_cnt, matop_cross_cnt);
                    result = true;
                }
                else
                {
                    Console.WriteLine("Not Pass BB to MATop. OUTSIDE={0} CROSS={1} BACK={2} CNT={3}", outside_stamp, cross_stamp, back_cnt, matop_cross_cnt);
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
    }
}
