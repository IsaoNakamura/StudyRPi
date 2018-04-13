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
using CryptoBoxer;

namespace CryptoChart
{
    public partial class FormMain : Form
    {
        private ChartArea m_area = null;
        private Series m_series_ltp = null;
        private Series m_series_ema = null;
        private Series m_series_bollHigh = null;
        private Series m_series_bollLow = null;
        private Series m_series_min = null;
        private Series m_series_max = null;

        private Boxer m_boxer = null;

        public FormMain()
        {
            InitializeComponent();

            m_boxer = Boxer.createBoxer(updateChart);
            if (m_boxer == null)
            {
                return;
            }

            initChartArea();

            m_boxer.MainLoop();
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

                m_area.BackColor = Color.LightGray;

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
                m_series_ltp.Color = Color.LightSeaGreen;
                m_series_ltp.Name = "OHLC";

                if (m_series_ema != null)
                {
                    m_series_ema.Dispose();
                    m_series_ema = null;
                }

                m_series_ema = new Series();
                m_series_ema.ChartType = SeriesChartType.Line;
                m_series_ema.Color = Color.Aqua;
                m_series_ema.Name = "EMA";

                if (m_series_bollHigh != null)
                {
                    m_series_bollHigh.Dispose();
                    m_series_bollHigh = null;
                }

                m_series_bollHigh = new Series();
                m_series_bollHigh.ChartType = SeriesChartType.Line;
                m_series_bollHigh.Color = Color.CornflowerBlue;
                m_series_bollHigh.Name = "BOLL_HIGH";


                if (m_series_bollLow != null)
                {
                    m_series_bollLow.Dispose();
                    m_series_bollLow = null;
                }

                m_series_bollLow = new Series();
                m_series_bollLow.ChartType = SeriesChartType.Line;
                m_series_bollLow.Color = Color.MediumVioletRed;
                m_series_bollLow.Name = "BOLL_LOW";

                if (m_series_min != null)
                {
                    m_series_min.Dispose();
                    m_series_min = null;
                }

                m_series_min = new Series();
                m_series_min.ChartType = SeriesChartType.Line;
                m_series_min.Color = Color.Red;
                m_series_min.Name = "MIN";

                if (m_series_max != null)
                {
                    m_series_max.Dispose();
                    m_series_max = null;
                }

                m_series_max = new Series();
                m_series_max.ChartType = SeriesChartType.Line;
                m_series_max.Color = Color.Blue;
                m_series_max.Name = "MAX";

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
                m_series_ema.Points.Clear();
                m_series_bollHigh.Points.Clear();
                m_series_bollLow.Points.Clear();
                m_series_min.Points.Clear();
                m_series_max.Points.Clear();
                this.chart1.Series.Clear();

                CandleBuffer candleBuf = m_boxer.getCandleBuffer();
                if (candleBuf == null)
                {
                    result = 1;
                    return result;
                }

                double indicator_min = m_boxer.m_min;
                double indicator_max = m_boxer.m_max;

                if (!candleBuf.isFullBuffer())
                {
                    result = 1;
                    return result;
                }

                int candle_cnt = 0;
                double y_min = 0.0;
                double y_max = 0.0;
                foreach (Candlestick candle in candleBuf.getCandleList())
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

                    DataPoint dp_ema = new DataPoint(candle_cnt, candle.ema);
                    m_series_ema.Points.Add(dp_ema);

                    DataPoint dp_bollHigh = new DataPoint(candle_cnt, candle.boll_high);
                    m_series_bollHigh.Points.Add(dp_bollHigh);

                    DataPoint dp_bollLow = new DataPoint(candle_cnt, candle.boll_low);
                    m_series_bollLow.Points.Add(dp_bollLow);

                    DataPoint dp_min = new DataPoint(candle_cnt, indicator_min);
                    m_series_min.Points.Add(dp_min);

                    DataPoint dp_max = new DataPoint(candle_cnt, indicator_max);
                    m_series_max.Points.Add(dp_max);

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

                m_area.AxisY.Minimum = y_min - 100;
                m_area.AxisY.Maximum = y_max + 100;

                
                this.chart1.Series.Add(m_series_ltp);
                this.chart1.Series.Add(m_series_ema);
                this.chart1.Series.Add(m_series_bollHigh);
                this.chart1.Series.Add(m_series_bollLow);
                this.chart1.Series.Add(m_series_min);
                this.chart1.Series.Add(m_series_max);

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
    }
}
