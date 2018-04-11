using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Windows.Forms.DataVisualization.Charting;

//using System.Net.Http;
//using System.Net.Http.Headers;

using System.Security.Cryptography;

//using Newtonsoft.Json;

using UtilityBitflyer;
using UtilityTrade;
using UtilityCryptowatch;

namespace CryptoChart
{
    public partial class FormMain : Form
    {
        private ChartArea m_area = null;
        private Series m_series_ltp = null;
        private CandleBuffer m_candleBuf = null;

        private int m_candleLength = 60; // チャート足、秒
        private string m_productCodeBitflyer = "FX_BTC_JPY";
        private string m_productCodeCryptowatch = "btcfxjpy";

        public FormMain()
        {
            InitializeComponent();

            m_candleBuf = new CandleBuffer();
            if (m_candleBuf == null)
            {
                return;
            }

            initChartArea();

            MainLoop();

        }

        private int initChartArea()
        {
            int result = 0;
            try
            {
                // グラフ領域の設定
                if (m_area != null)
                {
                    m_area.Dispose();
                    m_area = null;
                }
                m_area = new ChartArea();

                // 横軸（日付軸）の設定 
                // DateTimeのままでは使えないので
                //ToOADateメソッドでOLEオートメーション日付に変換
                m_area.AxisX.Title = "Number";
                m_area.AxisX.IntervalType = DateTimeIntervalType.Number;
                //m_area.AxisX.Minimum = new DateTime(2010, 1, 1).ToOADate();
                //m_area.AxisX.Maximum = new DateTime(2010, 1, 10).ToOADate();

                // 縦軸（株価軸）の設定
                m_area.AxisY.Title = "JPY";
                m_area.AxisY.Minimum = 718000;
                m_area.AxisY.Maximum = 726000;

                // 既定のグラフ領域の設定をクリアした後、設定する
                this.chart1.ChartAreas.Clear();
                this.chart1.ChartAreas.Add(m_area);

                // データ系列を作成する
                if (m_series_ltp != null)
                {
                    m_series_ltp.Dispose();
                    m_series_ltp = null;
                }

                m_series_ltp = new Series();
                m_series_ltp.ChartType = SeriesChartType.Candlestick;
                m_series_ltp.Color = Color.Green;
                m_series_ltp.Name = "LAST";


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

        private int updateChart()
        {
            int result = 0;
            try
            {
                m_series_ltp.Points.Clear();
                this.chart1.Series.Clear();

                if (!m_candleBuf.isFullBuffer())
                {
                    result = 1;
                    return result;
                }

                int candle_cnt = 0;
                double y_min = 0.0;
                double y_max = 0.0;
                foreach (Candlestick candle in m_candleBuf.getCandleList())
                {
                    if (candle == null)
                    {
                        continue;
                    }

                    // High Low Open Closeの順番で配列を作成
                    double[] values = new double[4]
                    {
                            candle.high,
                            candle.low,
                            candle.open,
                            candle.last
                    };

                    // 日付、四本値の配列からDataPointのインスタンスを作成
                    DataPoint dp = new DataPoint(candle_cnt, values);
                    m_series_ltp.Points.Add(dp);

                    // 表示範囲を算出
                    if (candle_cnt == 0)
                    {
                        y_max = candle.high;
                        y_min = candle.low;
                    }
                    else
                    {
                        if (y_max < candle.high)
                        {
                            y_max = candle.high;
                        }

                        if (y_min > candle.low)
                        {
                            y_min = candle.low;
                        }
                    }

                    candle_cnt++;
                }

                m_area.AxisY.Minimum = y_min - 1000;
                m_area.AxisY.Maximum = y_max + 1000;

                
                this.chart1.Series.Add(m_series_ltp);

                // Console.WriteLine("update Chart. y_min={0} y_max={1}", y_min, y_max);
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

        private async void MainLoop()
        {
            try
            {
                // Cryptowatchから過去のデータを取得
                BitflyerOhlc ohlc = await BitflyerOhlc.GetOhlcAfterAsync(m_productCodeCryptowatch, m_candleLength, 60);
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
                    Ticker ticker = await Ticker.GetTickerAsync(m_productCodeBitflyer);
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
                        if ( (prev_timestamp.Minute != cur_timestamp.Minute) && (elapsed_sec > 0.0) )
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

                    // チャートの表示を更新
                    updateChart();

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
    }
}
