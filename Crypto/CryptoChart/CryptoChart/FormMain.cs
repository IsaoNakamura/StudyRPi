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

namespace CryptoChart
{
    public partial class FormMain : Form
    {
        private ChartArea m_area = null;
        private Series m_series_ltp = null;
        private CandleBuffer m_candleBuf = null;

        public FormMain()
        {
            InitializeComponent();

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
                m_series_ltp.Color = Color.Red;
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

        private async void MainLoop()
        {
            try
            {
                //Task<int> task = Task.Run<int>(new Func<int>(getTicker));
                //int retTask = await task;

                // CandleStick用
                double high_price = 0.0;
                double low_price = 0.0;

                int pre_tick_id = 0;
                DateTime prev = DateTime.Now;
                int cycle_cnt = 0;
                
                while (true)
                {
                    // 一定時間経ったか算出
                    bool isMinit = false;
                    DateTime now = DateTime.Now;
                    TimeSpan span = now - prev;
                    double elapsed_time = span.TotalSeconds;
                    if (elapsed_time >= 5.0) {
                        Console.WriteLine(elapsed_time);
                        prev = now;
                        isMinit = true;
                    }

                    // Tickerを取得
                    Ticker ticker = await Ticker.GetTickerAsync();
                    if (ticker == null)
                    {
                        continue;
                    }
                    int tick_id = ticker.tick_id;
                    double cur_value = ticker.ltp;
                    string timestamp = ticker.timestamp;


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

                    if (isMinit == true)
                    {
                        if (m_candleBuf == null)
                        {
                            m_candleBuf = new CandleBuffer();
                            if (m_candleBuf == null)
                            {
                                continue;
                            }
                        }

                        if (m_candleBuf.addCandle(high_price, low_price, (high_price + low_price) / 2.0, cur_value, timestamp) != 0)
                        {
                            continue;
                        }

                        if (m_candleBuf.isFullBuffer())
                        {
                            m_series_ltp.Points.Clear();

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

                            this.chart1.Series.Clear();
                            this.chart1.Series.Add(m_series_ltp);
                        }

                        // 最高値・最低値リセット
                        high_price = 0.0;
                        low_price = 0.0;
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
    }
}
