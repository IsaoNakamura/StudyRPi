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
        private Series m_series_entry = null;

        //private Series m_series_bollHigh_top = null;
        //private Series m_series_bollLow_top = null;


        private ChartArea m_indicatorArea = null;
        private Series m_series_indi = null;

        private Boxer m_boxer = null;

        public FormMain()
        {
            InitializeComponent();

            m_boxer = Boxer.createBoxer(updateView, @".\boxerConfig.json");
            if (m_boxer == null)
            {
                Console.WriteLine("failed to createBoxer.");
                return;
            }

            if (m_boxer.loadAuthBitflyer(@".\AuthBitflyer.json") != 0)
            {
                Console.WriteLine("failed to loadAuthBitflyer.");
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
                m_area = new ChartArea("MainChart");

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

                m_area.AxisY.Interval = 4000;

                m_area.BackColor = Color.LightGray;

                // グラフ領域の設定
                if (m_indicatorArea != null)
                {
                    m_indicatorArea.Dispose();
                    m_indicatorArea = null;
                }
                m_indicatorArea = new ChartArea("IndicatorChart");

                // 横軸（日付軸）の設定 
                // DateTimeのままでは使えないので
                //ToOADateメソッドでOLEオートメーション日付に変換
                m_indicatorArea.AxisX.Title = "Number";
                m_indicatorArea.AxisX.IntervalType = DateTimeIntervalType.Number;

                // 縦軸（株価軸）の設定
                m_indicatorArea.AxisY.Title = "Value";
                m_indicatorArea.AxisY.Minimum = 0.0;
                m_indicatorArea.AxisY.Maximum = 100.0;

                m_indicatorArea.BackColor = Color.LightGray;


                // 既定のグラフ領域の設定をクリアした後、設定する
                this.chart1.ChartAreas.Clear();
                this.chart1.ChartAreas.Add(m_area);
                this.chart1.ChartAreas.Add(m_indicatorArea);

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

                //if (m_series_bollHigh_top != null)
                //{
                //    m_series_bollHigh_top.Dispose();
                //    m_series_bollHigh_top = null;
                //}
                //m_series_bollHigh_top = new Series();
                //m_series_bollHigh_top.ChartType = SeriesChartType.Line;
                //m_series_bollHigh_top.Color = Color.DarkBlue;
                //m_series_bollHigh_top.Name = "BOLL_HIGH_TOP";


                //if (m_series_bollLow_top != null)
                //{
                //    m_series_bollLow_top.Dispose();
                //    m_series_bollLow_top = null;
                //}
                //m_series_bollLow_top = new Series();
                //m_series_bollLow_top.ChartType = SeriesChartType.Line;
                //m_series_bollLow_top.Color = Color.DarkRed;
                //m_series_bollLow_top.Name = "BOLL_LOW_TOP";

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

                
                if (m_series_entry != null)
                {
                    m_series_entry.Dispose();
                    m_series_entry = null;
                }

                m_series_entry = new Series();
                m_series_entry.ChartType = SeriesChartType.Line;
                m_series_entry.Color = Color.GreenYellow;
                m_series_entry.Name = "ENTRY";



                if (m_series_indi != null)
                {
                    m_series_indi.Dispose();
                    m_series_indi = null;
                }

                m_series_indi = new Series();
                m_series_indi.ChartType = SeriesChartType.Line;
                m_series_indi.Color = Color.Purple;
                m_series_indi.Name = "Indicator";

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
        private int updateView()
        {
            int result = 0;
            try
            {
                updateCurrentInfoGrid();
                updateChart();
                updatePositionHistoryGrid();

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

                this.chart1.Series.Clear();

                updateCandleChart();
                updateIndicatorChart();
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


        private int updateCandleChart()
        {
            int result = 0;
            try
            {
                m_series_ltp.Points.Clear();
                m_series_ema.Points.Clear();
                m_series_bollHigh.Points.Clear();
                m_series_bollLow.Points.Clear();
                //m_series_bollHigh_top.Points.Clear();
                //m_series_bollLow_top.Points.Clear();
                m_series_min.Points.Clear();
                m_series_max.Points.Clear();
                m_series_entry.Points.Clear();

                if (m_boxer == null)
                {
                    result = 1;
                    return result;
                }

                CandleBuffer candleBuf = m_boxer.getCandleBuffer();
                if (candleBuf == null)
                {
                    result = 1;
                    return result;
                }

                double indicator_min = m_boxer.m_min;
                double indicator_max = m_boxer.m_max;
                double entry_price = m_boxer.getEntryPrice();

                if (!candleBuf.isFullBuffer())
                {
                    result = 1;
                    return result;
                }

                int candle_full_cnt = candleBuf.getCandleCount();
                if (candle_full_cnt <= 0)
                {
                    result = -1;
                    return result;
                }

                int beg_idx = 0;
                if (candle_full_cnt >= 100)
                {
                    beg_idx = candle_full_cnt - 100;
                }


                int candle_cnt = 0;
                double y_min = 0.0;
                double y_max = 0.0;
                //foreach (Candlestick candle in candleBuf.getCandleList())
                for(int i=beg_idx; i< candle_full_cnt; i++)
                {
                    Candlestick candle = candleBuf.getCandle(i);
                    if (candle == null)
                    {
                        continue;
                    }

                    // High Low Open Closeの順番で配列を作成
                    double[] values = new double[4]
                    {
                            Math.Round(candle.high,0),
                            Math.Round(candle.low,0),
                            Math.Round(candle.open,0),
                            Math.Round(candle.last,0)
                    };

                    // 日付、四本値の配列からDataPointのインスタンスを作成
                    DataPoint dp = new DataPoint(candle_cnt, values);
                    m_series_ltp.Points.Add(dp);

                    DataPoint dp_ema = new DataPoint(candle_cnt, Math.Round(candle.ema,0));
                    m_series_ema.Points.Add(dp_ema);

                    DataPoint dp_bollHigh = new DataPoint(candle_cnt, Math.Round(candle.boll_high,0));
                    m_series_bollHigh.Points.Add(dp_bollHigh);

                    DataPoint dp_bollLow = new DataPoint(candle_cnt, Math.Round(candle.boll_low,0));
                    m_series_bollLow.Points.Add(dp_bollLow);

                    //DataPoint dp_bollHigh_top = new DataPoint(candle_cnt, Math.Round(candle.boll_high_top,0));
                    //m_series_bollHigh_top.Points.Add(dp_bollHigh_top);

                    //DataPoint dp_bollLow_top = new DataPoint(candle_cnt, Math.Round(candle.boll_low_top,0));
                    //m_series_bollLow_top.Points.Add(dp_bollLow_top);

                    DataPoint dp_min = new DataPoint(candle_cnt, Math.Round(indicator_min,0));
                    m_series_min.Points.Add(dp_min);

                    DataPoint dp_max = new DataPoint(candle_cnt, Math.Round(indicator_max,0));
                    m_series_max.Points.Add(dp_max);

                    if (entry_price > double.Epsilon)
                    {

                        DataPoint dp_entry = new DataPoint(candle_cnt, Math.Round(entry_price,0));
                        m_series_entry.Points.Add(dp_entry);
                    }

                    double elem_max = candle.boll_high;
                    double elem_min = candle.boll_low;
                    //if (candle.boll_high_top > candle.boll_high)
                    //{
                    //    elem_max = candle.boll_high_top;
                    //}
                    //if (candle.boll_low_top < candle.boll_low)
                    //{
                    //    elem_min = candle.boll_low_top;
                    //}


                    // 表示範囲を算出
                    if (candle_cnt == 0)
                    {
                        y_max = elem_max;
                        y_min = elem_min;
                    }
                    else
                    {
                        if (y_max < elem_max)
                        {
                            y_max = elem_max;
                        }

                        if (y_min > elem_min)
                        {
                            y_min = elem_min;
                        }
                    }

                    candle_cnt++;
                }

                m_area.AxisY.Minimum = Math.Round(Math.Floor  ( (y_min - 1000 ) / 1000) * 1000);
                m_area.AxisY.Maximum = Math.Round(Math.Ceiling( (y_max + 1000 ) / 1000) * 1000);


                this.chart1.Series.Add(m_series_ltp);
                this.chart1.Series.Add(m_series_ema);
                this.chart1.Series.Add(m_series_bollHigh);
                this.chart1.Series.Add(m_series_bollLow);
                //this.chart1.Series.Add(m_series_bollHigh_top);
                //this.chart1.Series.Add(m_series_bollLow_top);
                this.chart1.Series.Add(m_series_min);
                this.chart1.Series.Add(m_series_max);
                if (entry_price > double.Epsilon)
                {
                    this.chart1.Series.Add(m_series_entry);
                }
                    

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

        private int updateIndicatorChart()
        {
            int result = 0;
            try
            {
                m_series_indi.Points.Clear();

                if (m_boxer == null)
                {
                    result = 1;
                    return result;
                }

                CandleBuffer candleBuf = m_boxer.getCandleBuffer();
                if (candleBuf == null)
                {
                    result = 1;
                    return result;
                }

                if (!candleBuf.isFullBuffer())
                {
                    result = 1;
                    return result;
                }

                int candle_full_cnt = candleBuf.getCandleCount();
                if (candle_full_cnt <= 0)
                {
                    result = -1;
                    return result;
                }

                int beg_idx = 0;
                if (candle_full_cnt >= 100)
                {
                    beg_idx = candle_full_cnt - 100;
                }

                int candle_cnt = 0;
                double y_min = 0.0;
                double y_max = 0.0;
                //foreach (Candlestick candle in candleBuf.getCandleList())
                for (int i = beg_idx; i < candle_full_cnt; i++)
                {
                    Candlestick candle = candleBuf.getCandle(i);
                    if (candle == null)
                    {
                        continue;
                    }

                    double value = Math.Floor(candle.ema_angle);

                    DataPoint dp_angle = new DataPoint(candle_cnt, value);
                    m_series_indi.Points.Add(dp_angle);


                    // 表示範囲を算出
                    if (candle_cnt == 0)
                    {
                        y_max = value;
                        y_min = 0.0;
                    }
                    else
                    {
                        if (y_max < value)
                        {
                            y_max = value;
                        }

                        if (y_min > value)
                        {
                            y_min = value;
                        }
                    }

                    candle_cnt++;
                }

                m_series_indi.ChartArea = "IndicatorChart";

                m_indicatorArea.AxisY.Minimum = Math.Floor(y_min);
                m_indicatorArea.AxisY.Maximum = Math.Floor(y_max);


                this.chart1.Series.Add(m_series_indi);

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


        private int updateCurrentInfoGrid()
        {
            int result = 0;
            try
            {
                this.CurrentInfoGrid.Rows.Clear();

                if (m_boxer == null)
                {
                    result = 1;
                    return result;
                }

                CandleBuffer candleBuf = m_boxer.getCandleBuffer();
                if (candleBuf == null)
                {
                    result = 1;
                    return result;
                }

                Candlestick curCandle = candleBuf.getLastCandle();
                if (curCandle == null)
                {
                    result = 1;
                    return result;
                }

                Position pos = m_boxer.getPosition();
                if (pos == null)
                {
                    result = 1;
                    return result;
                }

                {
                    // ポジション
                    int idx = this.CurrentInfoGrid.Rows.Add();
                    this.CurrentInfoGrid.Rows[idx].Cells[0].Value = "POS";
                    this.CurrentInfoGrid.Rows[idx].Cells[1].Value = m_boxer.getPositionName();
                }

                {
                    // Entry値
                    int idx = this.CurrentInfoGrid.Rows.Add();
                    this.CurrentInfoGrid.Rows[idx].Cells[0].Value = "ENTRY";
                    this.CurrentInfoGrid.Rows[idx].Cells[1].Value = string.Format("{0:0}", m_boxer.getEntryPrice());
                }

                {
                    // 利益
                    int idx = this.CurrentInfoGrid.Rows.Add();
                    this.CurrentInfoGrid.Rows[idx].Cells[0].Value = "PROFIT";
                    this.CurrentInfoGrid.Rows[idx].Cells[1].Value = string.Format("{0:0}", m_boxer.calcProfit());
                }

                {
                    // 終値(現在値)
                    int idx = this.CurrentInfoGrid.Rows.Add();
                    this.CurrentInfoGrid.Rows[idx].Cells[0].Value = "LAST";
                    
                    this.CurrentInfoGrid.Rows[idx].Cells[1].Value = string.Format("{0:0}", curCandle.last);
                }

                //{
                //    // EMA
                //    int idx = this.CurrentInfoGrid.Rows.Add();
                //    this.CurrentInfoGrid.Rows[idx].Cells[0].Value = "EMA";
                //    this.CurrentInfoGrid.Rows[idx].Cells[1].Value = string.Format("{0:0}", curCandle.ema);
                //}

                //{
                //    // BOLL_H
                //    int idx = this.CurrentInfoGrid.Rows.Add();
                //    this.CurrentInfoGrid.Rows[idx].Cells[0].Value = "BOLL_H";
                //    this.CurrentInfoGrid.Rows[idx].Cells[1].Value = string.Format("{0:0}", curCandle.boll_high);
                //}

                //{
                //    // BOLL_L
                //    int idx = this.CurrentInfoGrid.Rows.Add();
                //    this.CurrentInfoGrid.Rows[idx].Cells[0].Value = "BOLL_L";
                //    this.CurrentInfoGrid.Rows[idx].Cells[1].Value = string.Format("{0:0}", curCandle.boll_low);
                //}

                {
                    // LASTとEMAとの差
                    int idx = this.CurrentInfoGrid.Rows.Add();
                    this.CurrentInfoGrid.Rows[idx].Cells[0].Value = "EMA_DIFF";
                    this.CurrentInfoGrid.Rows[idx].Cells[1].Value = string.Format("{0:0}", curCandle.last - curCandle.ema);
                }

                {
                    // EMAの角度
                    int idx = this.CurrentInfoGrid.Rows.Add();
                    this.CurrentInfoGrid.Rows[idx].Cells[0].Value = "EMA_ANGLE";
                    this.CurrentInfoGrid.Rows[idx].Cells[1].Value = string.Format("{0:0.0}", curCandle.ema_angle);
                }


                int curLongBollLv = 0;
                int prevLongBollLv = 0;
                int curShortBollLv = 0;
                int prevShortBollLv = 0;
                bool isLong = m_boxer.isConditionLongEntry(ref curLongBollLv, ref prevLongBollLv);
                bool isShort = m_boxer.isConditionShortEntry(ref curShortBollLv, ref prevShortBollLv);


                {
                    int idx = this.CurrentInfoGrid.Rows.Add();
                    this.CurrentInfoGrid.Rows[idx].Cells[0].Value = "PRE_LONG_LV";
                    this.CurrentInfoGrid.Rows[idx].Cells[1].Value = prevLongBollLv;
                }
                {
                    int idx = this.CurrentInfoGrid.Rows.Add();
                    this.CurrentInfoGrid.Rows[idx].Cells[0].Value = "CUR_LONG_LV";
                    this.CurrentInfoGrid.Rows[idx].Cells[1].Value = curLongBollLv;
                }
                {
                    int idx = this.CurrentInfoGrid.Rows.Add();
                    this.CurrentInfoGrid.Rows[idx].Cells[0].Value = "PRE_SHORT_LV";
                    this.CurrentInfoGrid.Rows[idx].Cells[1].Value = prevShortBollLv;
                }
                {
                    int idx = this.CurrentInfoGrid.Rows.Add();
                    this.CurrentInfoGrid.Rows[idx].Cells[0].Value = "CUR_SHORT_LV";
                    this.CurrentInfoGrid.Rows[idx].Cells[1].Value = curShortBollLv;
                }
                {
                    double vola = curCandle.getVolatility();
                    int idx = this.CurrentInfoGrid.Rows.Add();
                    this.CurrentInfoGrid.Rows[idx].Cells[0].Value = "VOLA";
                    this.CurrentInfoGrid.Rows[idx].Cells[1].Value = string.Format("{0:0}", vola);
                }

                {
                    int idx = this.CurrentInfoGrid.Rows.Add();
                    this.CurrentInfoGrid.Rows[idx].Cells[0].Value = "VOLA_MA";
                    this.CurrentInfoGrid.Rows[idx].Cells[1].Value = string.Format("{0:0}", curCandle.vola_ma);
                }

                {
                    double vola_rate = curCandle.getVolatilityRate();
                    int idx = this.CurrentInfoGrid.Rows.Add();
                    this.CurrentInfoGrid.Rows[idx].Cells[0].Value = "VOLA_RATE";
                    this.CurrentInfoGrid.Rows[idx].Cells[1].Value = string.Format("{0:0}%", vola_rate);
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

        private int updatePositionHistoryGrid()
        {
            int result = 0;
            try
            {
                this.PositionHistoryGrid.Rows.Clear();

                if (m_boxer == null)
                {
                    result = 1;
                    return result;
                }

                List<Position> posArray = m_boxer.getPositionList();
                if (posArray == null)
                {
                    result = 1;
                    return result;
                }

                foreach (Position pos in posArray.Reverse<Position>())
                {
                    if (pos == null)
                    {
                        continue;
                    }
                    int idx = this.PositionHistoryGrid.Rows.Add();

                    // POS
                    this.PositionHistoryGrid.Rows[idx].Cells[0].Value = pos.getPositionStateStr();

                    // ORDER
                    this.PositionHistoryGrid.Rows[idx].Cells[1].Value = pos.getOrderStateStr();

                    // PROFIT
                    this.PositionHistoryGrid.Rows[idx].Cells[2].Value = string.Format("{0:0}", pos.getProfit());


                    // ENTRY
                    this.PositionHistoryGrid.Rows[idx].Cells[3].Value = string.Format("{0:0}", pos.entry_price);

                    // EXIT
                    this.PositionHistoryGrid.Rows[idx].Cells[4].Value = string.Format("{0:0}", pos.exit_price);
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

        private void StopBoxer()
        {
            if (m_boxer != null)
            {
                m_boxer.setStopFlag(true);
            }
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopBoxer();
            System.Threading.Thread.Sleep(3000);
        }

        private void StopBoxerButton_Click(object sender, EventArgs e)
        {
            StopBoxer();
        }
    }
}
