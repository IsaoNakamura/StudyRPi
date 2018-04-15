using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UtilityBitflyer;
using UtilityTrade;
using UtilityCryptowatch;

namespace CryptoBoxer
{
    public delegate int updateView();

    public class Boxer
    {
        /* === パラメタ用  === */

        // 売買量
        private double m_amount { get; set; }

        // チャート足(秒)
        private int m_periods { get; set; }

        // プロダクトコード
        private string m_product_bitflyer { get; set; }
        private string m_product_cryptowatch { get; set; }

        // EMAサンプリング数
        public int m_ema_sample_num { get; set; }

        // ボリンジャーバンドサンプリング数
        public int m_boll_sample_num { get; set; }
        /* =================== */

        // 状態用
        private CandleBuffer m_candleBuf { get; set; }
        public double m_min { get; set; }
        public double m_max { get; set; }
        public string m_position { get; set; }

        // デリゲートメソッド
        private updateView UpdateViewDelegate { get; set; }

        // 認証用
        private AuthBitflyer m_authBitflyer { get; set; }


        private Boxer()
        {
            m_amount = 0.001;
            m_periods = 60;
            m_product_bitflyer = null;
            m_product_cryptowatch = null;
            m_ema_sample_num = 20;
            m_boll_sample_num = 20;
            m_candleBuf = null;

            m_min = 0.0;
            m_max = 0.0;
            m_position = "NONE";

            m_authBitflyer = null;
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

        public static Boxer createBoxer
        (
            updateView UpdateViewDelegate,
            string  product_bitflyer    = "FX_BTC_JPY",
            string  product_cryptowatch = "btcfxjpy",
            int     periods             = 60,
            int     buffer_num          = 60,
            int     ema_sample_num      = 20,
            int     boll_sample_num     = 20
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

                CandleBuffer candleBuf = CandleBuffer.createCandleBuffer(buffer_num);
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

                boxer.UpdateViewDelegate    = _UpdateViewDelegate;
                boxer.m_candleBuf           = candleBuf;
                boxer.m_product_bitflyer    = product_bitflyer;
                boxer.m_product_cryptowatch = product_cryptowatch;
                boxer.m_periods             = periods;
                boxer.m_ema_sample_num      = ema_sample_num;
                boxer.m_boll_sample_num     = boll_sample_num;

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

        // 過去のキャンドル情報群をバッファに適用
        private int applyCandlestick(ref BitflyerOhlc ohlc)
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

                if (ohlc.result.miniute == null)
                {
                    Console.WriteLine("ohlc's miniute is null");
                    result = -1;
                    return result;
                }

                foreach (List<double> candleFactor in ohlc.result.miniute)
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

                    Candlestick candle = m_candleBuf.addCandle(highPrice, lowPrice, openPrice, closePrice, timestamp.ToString());
                    if (candle == null)
                    {
                        Console.WriteLine("failed to addCandle.");
                        continue;
                    }

                    calcIndicator(ref candle);

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

        public int calcIndicator(ref Candlestick candle)
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
                double ema = 0.0;
                if (m_candleBuf.calcEma(out ema, m_ema_sample_num) == 0)
                {
                    candle.ema = ema;
                }

                // 標準偏差、移動平均、ボリンジャーバンドを算出
                double stddev = 0.0;
                double ma = 0.0;
                if (m_candleBuf.calcStddevAndMA(out stddev, out ma, m_boll_sample_num) == 0)
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

        public async void MainLoop()
        {
            try
            {
                //double accept_price = await SendChildOrder.SellMarketAcceptance(m_authBitflyer, m_product_bitflyer, m_amount);
                //if (accept_price == 0.0)
                //{
                //    Console.WriteLine("failed to SellMarket()");
                //    return;
                //}

                // Cryptowatchから過去のデータを取得
                BitflyerOhlc ohlc = await BitflyerOhlc.GetOhlcAfterAsync(m_product_cryptowatch, m_periods, m_candleBuf.m_buffer_num);
                if (applyCandlestick(ref ohlc) != 0)
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
                DateTime prev_timestamp = DateTime.Parse(curCandle.timestamp);

                bool isLastConnect = false;
                int pre_tick_id = 0;
                int cycle_cnt = 0;
                while (true)
                {
                    // Tickerを取得
                    Ticker ticker = await Ticker.GetTickerAsync(m_product_bitflyer);
                    if (ticker == null)
                    {
                        continue;
                    }
                    int tick_id = ticker.tick_id;
                    double cur_value = ticker.ltp;

                    if (pre_tick_id == tick_id)
                    {
                        continue;
                    }

                    DateTime dateTimeUtc = DateTime.Parse(ticker.timestamp);// 2018-04-10T10:34:16.677 UTCタイム
                    DateTime cur_timestamp = System.TimeZoneInfo.ConvertTimeFromUtc(dateTimeUtc, System.TimeZoneInfo.Local);
                    TimeSpan span = cur_timestamp - prev_timestamp;
                    double elapsed_sec = span.TotalSeconds;

                    bool isClose = false;
                    if (!isLastConnect)
                    {
                        // 過去取得した最後のキャンドルが閉じているかチェック
                        //  過去最後のTimestampが10:14:00なら、10:13:00～10:13:59のキャンドル
                        //  現時刻が10:14:00～なら過去最後のキャンドルは閉まっている
                        //  現時刻が10:13:00～なら過去最後のキャンドルは閉まっていない

                        if (elapsed_sec < 0)
                        {
                            // 現時刻が過去最後のキャンドルの閉めより前だった場合、過去最後のキャンドルは閉まっていない
                            open_price = curCandle.open;
                            high_price = curCandle.high;
                            low_price = curCandle.low;

                            Console.WriteLine("prev is not closed. prev={0}, cur={1}, elapsed={2}, open={3}, cur={4}, high={5}, low={6}"
                                , prev_timestamp
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
                            Console.WriteLine("prev is closed. prev={0}, cur={1}, elapsed={2}", prev_timestamp.ToString(), cur_timestamp.ToString(), elapsed_sec);
                            isClose = true;
                            curCandle = null;
                        }
                        prev_timestamp = cur_timestamp;
                        isLastConnect = true;
                    }
                    else
                    {
                        // キャンドルを閉じるべきか判断
                        //Console.WriteLine("is close?. prev={0}, cur={1}, elapsed={2}", prev_timestamp, cur_timestamp, elapsed_sec);
                        if ((prev_timestamp.Minute != cur_timestamp.Minute) && (elapsed_sec > 0.0))
                        {
                            // 前回より分の値が変化したら、分足を閉じる
                            //Console.WriteLine("need close. prev={0}, cur={1}, elapsed={2}", prev_timestamp, cur_timestamp, elapsed_sec);
                            prev_timestamp = cur_timestamp;
                            isClose = true;
                        }
                        else
                        {
                            //Console.WriteLine("keep open. prev={0}, cur={1}, elapsed={2}", prev_timestamp, cur_timestamp, elapsed_sec);
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
                            // CloseTimeは現時刻を使用する。
                            curCandle.timestamp = cur_timestamp.ToString();
                            Console.WriteLine("closed candle. timestamp={0}, open={1}, close={2}, high={3}, low={4}"
                                , curCandle.timestamp
                                , curCandle.open
                                , curCandle.last
                                , curCandle.high
                                , curCandle.low
                            );
                        }

                        // 新たなキャンドルを追加
                        open_price = cur_value;
                        curCandle = m_candleBuf.addCandle(high_price, low_price, open_price, cur_value, cur_timestamp.ToString());
                        if (curCandle == null)
                        {
                            Console.WriteLine("failed to addCandle.");
                            return;
                        }

                        Console.WriteLine("add Candle. timestamp={0}, open={1}, close={2}, high={3}, low={4}"
                            , curCandle.timestamp
                            , curCandle.open
                            , curCandle.last
                            , curCandle.high
                            , curCandle.low
                        );

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
                            //Console.WriteLine("update Candle. timestamp={0}, open={1}, cur={2}, high={3}, low={4}", cur_timestamp, open_price, cur_value, high_price, low_price);

                        }
                    }

                    // インジケータ更新
                    if (curCandle != null)
                    {
                        calcIndicator(ref curCandle);
                    }

                    // TODO:トレードロジック

                    // 表示を更新
                    if (UpdateViewDelegate != null)
                    {
                        UpdateViewDelegate();
                    }

                    pre_tick_id = ticker.tick_id;

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

        public int hoge()
        {
            int result = 0;
            try
            {
                if(m_candleBuf==null)
                {
                    result = -1;
                    return result;
                }

                if (!m_candleBuf.isFullBuffer())
                {
                    result = -1;
                    return result;
                }

                int candle_cnt = m_candleBuf.getCandleCount();

                Candlestick curCandle = m_candleBuf.getLastCandle();
                int curIndex = candle_cnt - 1;

                Candlestick prevCandle = m_candleBuf.getCandle(curIndex - 1);
                if(prevCandle==null)
                {
                    result = -1;
                    return result;
                }

                if (prevCandle.isTouchBollHigh())
                {//前回がBOLL_HIGHにタッチしている場合
                    if(curCandle.isTouchBollHighLow())
                    {//現在がBOLLのどちらかにタッチ
                        // 何もしない
                    }
                    else
                    {//現在がBOLLにタッチしていない
                        int shortBollLv = prevCandle.getShortBollLevel();
                        if(shortBollLv>=0)
                        {
                            if(!curCandle.isTrend())
                            {// 下降キャンドルの場合
                                // NEED SHORT ENTRY
                            }
                            else
                            {// 上昇キャンドルの場合
                                // 何もしない
                            }

                        }
                        else
                        {
                            // 何もしない
                        }
                    }

                }
                else if (prevCandle.isTouchBollLow())
                {
                    //int longBollLv = prevCandle.getLongBollLevel();

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
