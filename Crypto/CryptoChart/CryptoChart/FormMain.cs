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
        private string m_productCode = "FX_BTC_JPY";
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

                    // 2018/04/10 19:21:00
                    DateTime timestamp = DateTimeOffset.FromUnixTimeSeconds((long)closeTime).LocalDateTime;
                    Console.WriteLine(timestamp);

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
                // 過去データから一番新しい(最後)タイムスタンプを取得
                List<double> candleLastFactor = ohlc.result.miniute.Last();
                double lastCloseTime = candleLastFactor[0];

                // CandleStick用
                double high_price = 0.0;
                double low_price = 0.0;

                int pre_tick_id = 0;
                DateTime prev_timestamp = DateTimeOffset.FromUnixTimeSeconds((long)lastCloseTime).LocalDateTime;
                int cycle_cnt = 0;

                Candlestick curCandle = null;
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

                    DateTime dateTimeUtc = DateTime.Parse(ticker.timestamp);// 2018-04-10T10:34:16.677 UTCタイム
                    DateTime cur_timestamp = System.TimeZoneInfo.ConvertTimeFromUtc(dateTimeUtc, System.TimeZoneInfo.Local);


                    // 過去取得したキャンドルデータと離れ過ぎていないかチェック
                    TimeSpan span = cur_timestamp - prev_timestamp;
                    double elapsed_time = span.TotalSeconds;
                    if (elapsed_time >= 61.0)
                    {
                        Console.WriteLine("Too Far!!. elapsed={0}, prev={1}, cur={2}", elapsed_time, prev_timestamp.ToString(), cur_timestamp.ToString());
                        break;
                    }

                    // 過去取得したキャンドルの方が新しくないかチェック
                    if (prev_timestamp.Minute > cur_timestamp.Minute)
                    {
                        Console.WriteLine("prev is new. prev={0}, cur={1}", prev_timestamp.ToString(), cur_timestamp.ToString());
                        continue;
                    }

                    // キャンドルを閉じるべきか判断
                    bool isClose = false;
                    if (prev_timestamp.Minute < cur_timestamp.Minute)
                    {
                        Console.WriteLine("Need Close. prev={0}, cur={1}", prev_timestamp.ToString(), cur_timestamp.ToString());
                        prev_timestamp = cur_timestamp;
                        isClose = true;
                    }

                    if (pre_tick_id == tick_id)
                    {
                        continue;
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

                    if (isClose == true || cycle_cnt==0)
                    {
                        // 新たなキャンドルを追加
                        Console.WriteLine("add Candle. timestamp={0}", cur_timestamp.ToString());
                        curCandle = m_candleBuf.addCandle(high_price, low_price, (high_price + low_price) / 2.0, cur_value, cur_timestamp.ToString());
                        if (curCandle == null)
                        {
                            Console.WriteLine("failed to addCandle.");
                            return;
                        }

                        // 最高値・最低値リセット
                        if (isClose)
                        {
                            high_price = 0.0;
                            low_price = 0.0;
                            Console.WriteLine("closed candle.");
                        }
                    }
                    else
                    {
                        // 現在のキャンドルを更新
                        if (curCandle != null)
                        {
                            
                            curCandle.high = high_price;
                            curCandle.low = low_price;
                            curCandle.open = (high_price + low_price) / 2.0;
                            curCandle.last = cur_value;
                            curCandle.timestamp = cur_timestamp.ToString();
                            // Console.WriteLine("update Candle. timestamp={0}", cur_timestamp);
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
