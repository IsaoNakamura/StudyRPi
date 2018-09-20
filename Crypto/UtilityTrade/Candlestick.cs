using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTrade
{
    public class Candlestick
    {
        public double high { get; set; }
        public double low { get; set; }
        public double open { get; set; }
        public double last { get; set; }
        public string timestamp { get; set; }

        public double volume { get; set; }

        public double disparity_rate { get; set; }

        // インジケータ
        public double ema { get; set; }
        public double ema_sub { get; set; }
        public double stddev { get; set; }
        public double ma { get; set; }
        public double boll_high { get; set; }
        public double boll_low { get; set; }
        public double vola_ma { get; set; }
        public double ema_angle { get; set; }

        public double boll_high_top { get; set; }
        public double boll_low_top { get; set; }
        public double ma_top { get; set; }

        public double volume_ma { get; set; }

        public double ma_top_increase { get; set; }
        public double ma_top_increase_rate { get; set; }

        public double hige_top_max { get; set; }
        public double hige_bottom_max { get; set; }

        public double range_min { get; set; }
        public double range_max { get; set; }
        public double body_min { get; set; }
        public double body_max { get; set; }

        public Candlestick()
        {
            high = 0.0;
            low = 0.0;
            open = 0.0;
            last = 0.0;
            timestamp = string.Empty;
            disparity_rate = 0.0;
            ema = 0.0;
            stddev = 0.0;
            ma = 0.0;
            ma_top = 0.0;
            boll_high = 0.0;
            boll_low = 0.0;
            boll_high_top = 0.0;
            boll_low_top = 0.0;
            vola_ma = 0.0;
            ema_angle = 0.0;
            ema_sub = 0.0;

            volume = 0.0;
            volume_ma = 0.0;

            ma_top_increase = 0.0;
            ma_top_increase_rate = 0.0;

            return;
        }

        public Candlestick
        (
            double _high,
            double _low,
            double _open,
            double _last,
            string _timestamp
        )
        {
            high = _high;
            low = _low;
            open = _open;
            last = _last;
            timestamp = _timestamp;
            return;
        }

        // トレンドを判断
        //  trueなら上昇トレンド
        //  falseなら下降
        public bool isTrend()
        {
            if (open <= last)
            {
                return true;
            }
            return false;
        }

        public double getVolatility()
        {
            return Math.Abs(last - open);
        }

		public double getDiff()
        {
            return (last - open);
        }

        public double getVolatilityRate()
        {
            if (vola_ma <= double.Epsilon)
            {
                return 0.0;
            }
            return ((getVolatility() / vola_ma) * 100.0);
        }

        public int getLastLevel()
        {
            return getValueLevel(last, high, low);
        }

        public int getOpenLevel()
        {
            return getValueLevel(open, high, low);
        }

        private static int getValueLevel(double price, double high, double low)
        {
            int result = 0;

            double length = high - low;
            double pos = price - low;
            double rate = pos / length * 100.0;

            int level = 0;
            if (rate >= 80.0)
            {
                level = 4;
            }
            else if (rate >= 60.0)
            {
                level = 3;
            }
            else if (rate >= 40.0)
            {
                level = 2;
            }
            else if (rate >= 20.0)
            {
                level = 1;
            }
            else
            {
                level = 0;
            }

            result = level;

            return result;
        }

        public int getUpCandleType()
        {
            int result = -1;

            int last_level = getValueLevel(last, high, low);
            int open_level = getValueLevel(open, high, low);

            if (!isTrend())
            {
                result = -1;
                return result;
            }

            // 上昇キャンドルの場合
            if (last_level == 4 || last_level == 3)
            {
                // 最高
                if (open_level == 0)
                {
                    // 丸坊主
                    // 上昇継続
                    result = 7;
                    return result;
                }
                else if (open_level == 1)
                {
                    // 大陽線
                    // 上昇継続
                    result = 6;
                    return result;
                }
                else if (open_level == 2)
                {
                    // 小陽線
                    // 上昇継続
                    result = 5;
                    return result;
                }
                else if (open_level == 3)
                {
                    // 小陽線/下髭陽線
                    // 上昇継続
                    result = 4;
                    return result;
                }
                else if (open_level == 4)
                {
                    // 下髭陽線
                    // 上昇継続
                    result = 3;
                    return result;
                }
            }
            else if (last_level == 2)
            {
                // 中間
                if (open_level == 0 || open_level == 1)
                {
                    // 上髭陽線
                    // 阻まれている
                    // だいぶ押された。そろそろ上昇が終わる
                    result = 2;
                    return result;
                }
                else if (open_level == 2)
                {
                    // 寄引同事線
                    // 転換
                    // 動いたが、結局終値と始値が同じ 方向転換の可能性
                    result = 1;
                    return result;
                }
            }
            else if (last_level == 0 || last_level == 1)
            {
                // 低
                if (open_level == 0 || open_level == 1)
                {
                    // 上髭陽線
                    // 阻まれている
                    result = 0;
                    return result;
                }
            }
            return result;
        }

        public int getDownCandleType()
        {
            int result = -1;

            int last_level = getValueLevel(last, high, low);
            int open_level = getValueLevel(open, high, low);

            if (isTrend())
            {
                result = -1;
                return result;
            }

            // 下降キャンドルの場合
            if (last_level == 0 || last_level == 1)
            {
                // 最高
                if (open_level == 4)
                {
                    // 丸坊主
                    // 下降継続
                    result = 7;
                    return result;
                }
                else if (open_level == 3)
                {
                    // 大陰線
                    // 下降継続
                    result = 6;
                    return result;
                }
                else if (open_level == 2)
                {
                    // 小陰線
                    // 下降継続
                    result = 5;
                    return result;
                }
                else if (open_level == 1)
                {
                    // 小陰線/上髭陰線
                    // 下降継続
                    result = 4;
                    return result;
                }
                else if (open_level == 0)
                {
                    // 上髭陰線
                    // 下降継続
                    result = 3;
                    return result;
                }
            }
            else if (last_level == 2)
            {
                // 中間
                if (open_level == 3 || open_level == 4)
                {
                    // 下髭陰線
                    // 阻まれている
                    // だいぶ押された。そろそろ下降が終わる
                    result = 2;
                    return result;
                }
                else if (open_level == 2)
                {
                    // 寄引同事線
                    // 転換
                    // 動いたが、結局終値と始値が同じ 方向転換の可能性
                    result = 1;
                    return result;
                }
            }
            else if (last_level == 3 || last_level == 4)
            {
                // 低
                if (open_level == 3 || open_level == 4)
                {
                    // 下髭陰線
                    // 阻まれている
                    result = 0;
                    return result;
                }
            }
            return result;
        }

        public int getShortLevel(double vola_big = 300.0, double vola_small = 20.0)
        {
            int result = -1;

            double vola_rate = getVolatilityRate();

            if (isTrend())
            {
                //上昇キャンドルの場合
                int up_type = getUpCandleType();
                if (up_type < 0)
                {
                    // error
                    result = -1;
                }
                else if (up_type <= 2)
                {
                    if (vola_rate >= vola_big)
                    {
                        // 大きいローソク
                        // 上昇継続
                        // SHORT危険
                        result = -2;
                    }
                    else
                    {
                        // トレンド転換
                        // 下降するかもSHORTできるかも
                        result = 0;
                    }
                }
                else
                {
                    if (vola_rate <= vola_small)
                    {
                        // 小さいローソク
                        // トレンド転換
                        // 下降するかもSHORTできるかも
                        result = 0;
                    }
                    else
                    {
                        // 上昇継続
                        // SHORT危険
                        result = -2;
                    }
                }
            }
            else
            {
                //下降トレンドの場合
                int down_type = getDownCandleType();
                if (down_type < 0)
                {
                    // error
                    result = -1;
                }
                else if (down_type <= 2)
                {
                    if (vola_rate >= vola_big)
                    {
                        // 大きいローソク
                        // 下降継続
                        // SHORT推奨
                        result = 2;
                    }
                    else
                    {
                        // トレンド転換
                        // 上昇するかもSHORTは注意
                        result = 1;
                    }
                }
                else
                {
                    if (vola_rate <= vola_small)
                    {
                        // 小さいローソク
                        // トレンド転換
                        // 上昇するかもSHORTは注意
                        result = 1;
                    }
                    else
                    {
                        // 下降継続
                        // SHORT推奨
                        result = 2;
                    }
                }
            }

            return result;
        }

        public int getLongLevel(double vola_big=300.0, double vola_small=50.0)
        {
            int result = -1;

            double vola_rate = getVolatilityRate();

            if (!isTrend())
            {
                //下降トレンドの場合
                int down_type = getDownCandleType();
                if (down_type < 0)
                {
                    // error
                    result = -1;
                }
                else if (down_type <= 2)
                {
                    if (vola_rate >= vola_big)
                    {
                        // 大きいローソク
                        // 下降継続
                        // LONG注意
                        result = -2;
                    }
                    else
                    {
                        // トレンド転換
                        // 上昇するかもLONGできるかも
                        result = 0;
                    }
                }
                else
                {
                    if (vola_rate <= vola_small)
                    {
                        // 小さいローソク
                        // トレンド転換
                        // 上昇するかもLONGできるかも
                        result = 0;
                    }
                    else
                    {
                        // 下降継続
                        // LONG注意
                        result = -2;
                    }
                }
            }
            else
            {
                //上昇キャンドルの場合
                int up_type = getUpCandleType();
                if (up_type < 0)
                {
                    // error
                    result = -1;
                }
                else if (up_type <= 2)
                {
                    if (vola_rate >= vola_big)
                    {
                        // 大きいローソク
                        // 上昇継続
                        // LONG推奨
                        result = 2;
                    }
                    else
                    {
                        // トレンド転換
                        // 下降するかもLONG注意
                        result = 1;
                    }
                }
                else
                {
                    if (vola_rate <= vola_small)
                    {
                        // 小さいローソク
                        // トレンド転換
                        // 下降するかもLONG注意
                        result = 1;
                    }
                    else
                    {
                        // 上昇継続
                        // LONG推奨
                        result = 2;
                    }
                }
            }

            return result;
        }

        public bool isCrossEMA(double play=0.0)
        {
            if (ema >= (low-play) && ema <= (high+play))
            {
                return true;
            }
            return false;
        }

        public bool isCrossMA(double play = 0.0)
        {
            if (ma >= (low - play) && ma <= (high + play))
            {
                return true;
            }
            return false;
        }

        public bool isCrossMATop(double play = 0.0)
        {
            if (ma_top >= (low - play) && ma_top <= (high + play))
            {
                return true;
            }
            return false;
        }

        public bool isCrossBBHigh(double play = 0.0)
        {
            if (boll_high >= (low - play) && boll_high <= (high + play))
            {
                return true;
            }
            return false;
        }

        public bool isCrossBBLow(double play = 0.0)
        {
            if (boll_low >= (low - play) && boll_low <= (high + play))
            {
                return true;
            }
            return false;
        }

        public bool isCrossBBHighTop(double play = 0.0)
        {
            if (boll_high_top >= (low - play) && boll_high_top <= (high + play))
            {
                return true;
            }
            return false;
        }

        public bool isCrossBBLowTop(double play = 0.0)
        {
            if (boll_low_top >= (low - play) && boll_low_top <= (high + play))
            {
                return true;
            }
            return false;
        }

        public bool isTouchBollHigh()
        {
            if (boll_high <= high)
            {
                return true;
            }

            if (boll_high <= last)
            {
                return true;
            }

            if (boll_high <= open)
            {
                return true;
            }

            if (boll_high <= low)
            {
                return true;
            }
            return false;
        }

        public bool isTouchBollLow()
        {
            if (boll_low >= low)
            {
                return true;
            }

            if (boll_low >= last)
            {
                return true;
            }

            if (boll_low >= open)
            {
                return true;
            }

            if (boll_low >= high)
            {
                return true;
            }

            return false;
        }

        public bool isTouchBollHighTop(double play = 0.0)
        {
            if (boll_high_top <= (high + play))
            {
                return true;
            }

            if (boll_high_top <= (last+play))
            {
                return true;
            }

            if (boll_high_top <= (open + play))
            {
                return true;
            }

            if (boll_high_top <= (low + play))
            {
                return true;
            }
            return false;
        }

        public bool isTouchBollLowTop(double play = 0.0)
        {
            if (boll_low_top >= (low - play))
            {
                return true;
            }

            if (boll_low_top >= (last-play))
            {
                return true;
            }

            if (boll_low_top >= (open - play))
            {
                return true;
            }

            if (boll_low_top >= (high - play))
            {
                return true;
            }

            return false;
        }

        public bool isTouchBollHighLow()
        {
            if(isTouchBollHigh())
            {
                return true;
            }

            if(isTouchBollLow())
            {
                return true;
            }

            return false;
        }
      
        // 値がボリンジャー高バンド以上か判断
        public bool isOverBBHigh(double value)
        {
			if (boll_high <= value)
            {
                return true;
            }
            return false;
        }
        
        
        // 値がボリンジャー低バンド以上か判断
        public bool isUnderBBLow(double value)
        {
            if (boll_low >= value)
            {
                return true;
            }
            return false;
        }
    }

    public class CandleBuffer
    {
        private List<Candlestick> m_candleList { get; set; }
        public int m_buffer_num { get; set; }

        public CandleBuffer()
        {
            m_candleList = null;
            m_buffer_num = 60;
            return;
        }

        public static CandleBuffer createCandleBuffer
        (
            int buffer_num = 60
        )
        {
            CandleBuffer result = null;
            try
            {
                if (buffer_num <= 0)
                {
                    result = null;
                    return result;
                }

                CandleBuffer buffer = new CandleBuffer();
                if (buffer == null)
                {
                    result = null;
                    return result;
                }

                buffer.m_buffer_num = buffer_num;

                result = buffer;
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

        public int getCandleCount()
        {
            if (m_candleList == null)
            {
                return 0;
            }

            return m_candleList.Count;
        }

        public bool isFullBuffer()
        {
            if (m_buffer_num <= getCandleCount())
            {
                return true;
            }
            return false;
        }

        public List<Candlestick> getCandleList()
        {
            return m_candleList;
        }

        public Candlestick getLastCandle()
        {
            if (getCandleCount() <= 0)
            {
                return null;
            }

            return m_candleList.Last();
        }

        public Candlestick getCandle(int index)
        {
            int candle_cnt = getCandleCount();
            if (candle_cnt <= 0)
            {
                return null;
            }

            if(index < 0)
            {
                return null;
            }

            if(index >= candle_cnt)
            {
                return null;
            }

            return m_candleList[index];
        }

        public Candlestick addCandle
        (
            double _high,
            double _low,
            double _open,
            double _last,
            string _timestamp
        )
        {
            Candlestick result = null;
            try
            {
                if (m_candleList == null)
                {
                    m_candleList = new List<Candlestick>();
                    if (m_candleList == null)
                    {
                        result = null;
                        return result;
                    }
                }

                Candlestick candle = new Candlestick(_high, _low, _open, _last, _timestamp);
                if (candle == null)
                {
                    result = null;
                    return result;
                }

                if (addCandle(candle) != 0)
                {
                    result = null;
                    return result;
                }
                result = candle;
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

        public int addCandle( Candlestick candle )
        {
            int result = 0;
            try
            {
                if (m_candleList == null)
                {
                    m_candleList = new List<Candlestick>();
                    if (m_candleList == null)
                    {
                        result = -1;
                        return result;
                    }
                }

                if (candle == null)
                {
                    result = -1;
                    return result;
                }

                m_candleList.Add(candle);

                // 保持数を超えた場合
                if (m_candleList.Count > m_buffer_num)
                {
                    // 何個消すか算出
                    int remove_cnt = m_candleList.Count - m_buffer_num;

                    // 古いCandlestickから削除
                    m_candleList.RemoveRange(0, remove_cnt);
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

        public int calcEma(out double ema, int sample_num)
        {
            int result = 0;
            ema = 0.0;
            try
            {
                int candle_cnt = getCandleCount();
                if (candle_cnt <= 0)
                {
                    result = -1;
                    return result;
                }

                int beg_idx = 0;
                if (candle_cnt >= sample_num)
                {
                    beg_idx = candle_cnt - sample_num;
                }

                double last_ema = 0.0;
                int elem_cnt = 0;
                for (int i= beg_idx; i< candle_cnt; i++)
                {
                    
                    Candlestick candle = m_candleList[i];
                    if (candle == null)
                    {
                        continue;
                    }
                    elem_cnt++;
                    double elem_value = candle.last;
                    double elem_ema = elem_value * 2.0 / (elem_cnt + 1) + last_ema * (elem_cnt + 1 - 2) / (elem_cnt + 1);

                    last_ema = elem_ema;
                }

                ema = last_ema;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = -1;
            }
            finally
            {
                if (result != 0)
                {
                    ema = 0.0;
                }
            }
            return result;
        }

        public int calcEma2(out double ema, int sample_num)
        {
            int result = 0;
            ema = 0.0;
            try
            {
                int candle_cnt = getCandleCount();
                if (candle_cnt <= 0)
                {
                    result = -1;
                    return result;
                }

                int beg_idx = 0;
                if (candle_cnt >= sample_num)
                {
                    beg_idx = candle_cnt - sample_num;
                }

                int last_idx = beg_idx - 1;
                if (last_idx < 0)
                {
                    result = -1;
                    return result;
                }

                int past_idx = last_idx - (sample_num - 1);
                if (past_idx < 0)
                {
                    result = -1;
                    return result;
                }

                double last_sum = 0.0;
                int last_cnt = 0;
                for (int i = past_idx; i <= last_idx; i++)
                {
                    Candlestick candle = m_candleList[i];
                    if (candle == null)
                    {
                        continue;
                    }
                    last_cnt++;
                    last_sum += candle.last;
                }


                double last_ema = last_sum / sample_num;
                int elem_cnt = 0;
                for (int i = beg_idx; i < candle_cnt; i++)
                {

                    Candlestick candle = m_candleList[i];
                    if (candle == null)
                    {
                        continue;
                    }
                    elem_cnt++;

                    double elem_value = candle.last;
                    double elem_ema = last_ema + (2.0 / (sample_num + 1)) * (elem_value - last_ema);

                    last_ema = elem_ema;
                }

                ema = last_ema;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = -1;
            }
            finally
            {
                if (result != 0)
                {
                    ema = 0.0;
                }
            }
            return result;
        }

        // 標準偏差と移動平均を算出
        public int calcStddevAndMA(out double stddev, out double ma, int sample_num)
        {
            int result = 0;
            stddev = 0.0;
            ma = 0.0;
            try
            {
                int candle_cnt = getCandleCount();
                if (candle_cnt <= 0)
                {
                    result = -1;
                    return result;
                }

                int beg_idx = 0;
                if (candle_cnt >= sample_num)
                {
                    beg_idx = candle_cnt - sample_num;
                }

                double value_sum = 0.0;
                double square_sum = 0.0;
                int elem_cnt = 0;
                for (int i = beg_idx; i < candle_cnt; i++)
                {
                    
                    Candlestick candle = m_candleList[i];
                    if (candle == null)
                    {
                        continue;
                    }
                    elem_cnt++;
                    double elem_value = candle.last;

                    value_sum += elem_value;
                    square_sum += (elem_value * elem_value);
                }

                if (elem_cnt > 0)
                {
                    stddev = Math.Sqrt(((elem_cnt * square_sum) - (value_sum * value_sum)) / (elem_cnt * (elem_cnt + 1)));
                    ma = value_sum / elem_cnt;
                }
                else
                {
                    stddev = 0.0;
                    ma = 0.0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = -1;
            }
            finally
            {
                if (result != 0)
                {
                    stddev = 0.0;
                    ma = 0.0;
                }
            }
            return result;
        }

        // ボラリティの移動平均を算出(最大～最小)
        public int calcHighLowVolatility(out double ma, int sample_num)
        {
            int result = 0;
            ma = 0.0;
            try
            {
                int candle_cnt = getCandleCount();
                if (candle_cnt <= 0)
                {
                    result = -1;
                    return result;
                }

                int beg_idx = 0;
                if (candle_cnt >= sample_num)
                {
                    beg_idx = candle_cnt - sample_num;
                }

                double value_sum = 0.0;
                double square_sum = 0.0;
                int elem_cnt = 0;
                for (int i = beg_idx; i < candle_cnt; i++)
                {

                    Candlestick candle = m_candleList[i];
                    if (candle == null)
                    {
                        continue;
                    }
                    elem_cnt++;
                    double elem_value = candle.high - candle.low;

                    value_sum += elem_value;
                    square_sum += (elem_value * elem_value);
                }

                if (elem_cnt > 0)
                {
                    ma = value_sum / elem_cnt;
                }
                else
                {
                    ma = 0.0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = -1;
            }
            finally
            {
                if (result != 0)
                {
                    ma = 0.0;
                }
            }
            return result;
        }

        // ボラリティの移動平均を算出(始値～終値)
        public int calcVolatilityMA(out double ma, int sample_num)
        {
            int result = 0;
            ma = 0.0;
            try
            {
                int candle_cnt = getCandleCount();
                if (candle_cnt <= 0)
                {
                    result = -1;
                    return result;
                }

                int beg_idx = 0;
                if (candle_cnt >= sample_num)
                {
                    beg_idx = candle_cnt - sample_num;
                }

                double value_sum = 0.0;
                double square_sum = 0.0;
                int elem_cnt = 0;
                for (int i = beg_idx; i < candle_cnt; i++)
                {

                    Candlestick candle = m_candleList[i];
                    if (candle == null)
                    {
                        continue;
                    }
                    elem_cnt++;
                    double elem_value = candle.getVolatility();

                    value_sum += elem_value;
                    square_sum += (elem_value * elem_value);
                }

                if (elem_cnt > 0)
                {
                    ma = value_sum / elem_cnt;
                }
                else
                {
                    ma = 0.0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = -1;
            }
            finally
            {
                if (result != 0)
                {
                    ma = 0.0;
                }
            }
            return result;
        }

        public int calcVolumeMA(out double ma, int sample_num)
        {
            int result = 0;
            ma = 0.0;
            try
            {
                int candle_cnt = getCandleCount();
                if (candle_cnt <= 0)
                {
                    result = -1;
                    return result;
                }

                int beg_idx = 0;
                if (candle_cnt >= sample_num)
                {
                    beg_idx = candle_cnt - sample_num;
                }

                double value_sum = 0.0;
                double square_sum = 0.0;
                int elem_cnt = 0;
                for (int i = beg_idx; i < candle_cnt; i++)
                {

                    Candlestick candle = m_candleList[i];
                    if (candle == null)
                    {
                        continue;
                    }
                    elem_cnt++;
                    double elem_value = candle.volume;

                    value_sum += elem_value;
                    square_sum += (elem_value * elem_value);
                }

                if (elem_cnt > 0)
                {
                    ma = value_sum / elem_cnt;
                }
                else
                {
                    ma = 0.0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = -1;
            }
            finally
            {
                if (result != 0)
                {
                    ma = 0.0;
                }
            }
            return result;
        }

        public int calcEmaAngleMA(out double ma, int sample_num)
        {
            int result = 0;
            ma = 0.0;
            try
            {
                int candle_cnt = getCandleCount();
                if (candle_cnt <= 0)
                {
                    result = -1;
                    return result;
                }

                int beg_idx = 0;
                if (candle_cnt >= sample_num)
                {
                    beg_idx = candle_cnt - sample_num;
                }


                double value_sum = 0.0;
                double square_sum = 0.0;
                int elem_cnt = 0;
                for (int i = beg_idx; i < candle_cnt; i++)
                {

                    Candlestick end_candle = m_candleList[i];
                    if (end_candle == null)
                    {
                        continue;
                    }

                    elem_cnt++;
                    double elem_value = 0.0;

                    if ((i - 5) >= 0)
                    {
                        Candlestick beg_candle = m_candleList[i-5];
                        if (beg_candle == null)
                        {
                            result = -1;
                            return result;
                        }
                        double ema_diff = (end_candle.ema - beg_candle.ema) / 5000.0 * 100.0;
                        double tan = ema_diff / sample_num;
                        double angle = Math.Atan(tan) * (180 / Math.PI);

                        elem_value = angle;
                    }
                    value_sum += elem_value;
                    square_sum += (elem_value * elem_value);
                }

                if (elem_cnt > 0)
                {
                    ma = value_sum / elem_cnt;
                }
                else
                {
                    ma = 0.0;
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = -1;
            }
            finally
            {
                if (result != 0)
                {
                    ma = 0.0;
                }
            }
            return result;
        }

        public int calcMATopIncrease(out double increase, int sample_num)
        {
            int result = 0;
            increase = 0.0;
            try
            {
                int candle_cnt = getCandleCount();
                if (candle_cnt <= 0)
                {
                    result = -1;
                    return result;
                }

                int beg_idx = candle_cnt - sample_num;
                if (beg_idx < 0 )
                {
                    result = -1;
                    return result;
                }

                Candlestick beg_candle = m_candleList[beg_idx];
                if (beg_candle == null)
                {
                    result = -1;
                    return result;
                }

                Candlestick end_candle = getLastCandle();
                if (end_candle == null)
                {
                    result = -1;
                    return result;
                }

                increase = end_candle.ma_top - beg_candle.ma_top;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = -1;
            }
            finally
            {
                if (result != 0)
                {
                    increase = 0.0;
                }
            }
            return result;
        }

        public int calcMATopIncreaseMA(out double ma, int sample_num)
        {
            int result = 0;
            ma = 0.0;
            try
            {
                int candle_cnt = getCandleCount();
                if (candle_cnt <= 0)
                {
                    result = -1;
                    return result;
                }

                int beg_idx = 0;
                if (candle_cnt >= sample_num)
                {
                    beg_idx = candle_cnt - sample_num;
                }

                double value_sum = 0.0;
                double square_sum = 0.0;
                int elem_cnt = 0;
                for (int i = beg_idx; i < candle_cnt; i++)
                {

                    Candlestick candle = m_candleList[i];
                    if (candle == null)
                    {
                        continue;
                    }
                    elem_cnt++;
                    double elem_value = candle.ma_top_increase;

                    value_sum += elem_value;
                    square_sum += (elem_value * elem_value);
                }

                if (elem_cnt > 0)
                {
                    ma = value_sum / elem_cnt;
                }
                else
                {
                    ma = 0.0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = -1;
            }
            finally
            {
                if (result != 0)
                {
                    ma = 0.0;
                }
            }
            return result;
        }

        public int calcHigeLength
        (
            out double top_length,
            out double bottom_length, 
            out double range_min, 
            out double range_max, 
            out double body_min,
            out double body_max,
            int sample_num
        )
        {
            int result = 0;
            top_length = 0.0;
            bottom_length = 0.0;
            range_min = 0.0;
            range_max = 0.0;
            body_min = 0.0;
            body_max = 0.0;
            try
            {
                int candle_cnt = getCandleCount();
                if (candle_cnt <= 0)
                {
                    result = -1;
                    return result;
                }

                int beg_idx = 0;
                if (candle_cnt >= sample_num)
                {
                    beg_idx = candle_cnt - sample_num;
                }

                double _range_max = 0.0;
                double _range_min = 0.0;
                double _body_max = 0.0;
                double _body_min = 0.0;
                double _top_length = 0.0;
                double _bottom_length = 0.0;
                bool isFirst = true;
                for (int i = beg_idx; i < candle_cnt; i++)
                {
                    Candlestick candle = m_candleList[i];
                    if (candle == null)
                    {
                        continue;
                    }


                    double top = 0.0;
                    double bottom = 0.0;

                    double body_high = 0.0;
                    double body_low = 0.0;

                    if (candle.isTrend())
                    {
                        body_high = candle.last;
                        body_low = candle.open;
                    }
                    else
                    {
                        body_high = candle.open;
                        body_low = candle.last;
                    }

                    top = candle.high - body_high;
                    bottom = body_low - candle.low;

                    if (isFirst)
                    {
                        isFirst = false;

                        _top_length = top;
                        _bottom_length = bottom;

                        _body_max = body_high;
                        _body_min = body_low;

                        _range_max = candle.high;
                        _range_min = candle.low;
                    }
                    else
                    {
                        _top_length = Math.Max(_top_length, top);
                        _bottom_length = Math.Max(_bottom_length, bottom);

                        _body_max = Math.Max(_body_max, body_high);
                        _body_min = Math.Min(_body_min, body_low);


                        _range_max = Math.Max(_range_max, candle.high);
                        _range_min = Math.Min(_range_min, candle.low);
                    }
                }

                top_length = _top_length;
                bottom_length = _bottom_length;

                range_min = _range_min;
                range_max = _range_max;

                body_min = _body_min;
                body_max = _body_max;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = -1;
            }
            finally
            {
                if (result != 0)
                {
                    top_length = 0.0;
                    bottom_length = 0.0;
                    range_min = 0.0;
                    range_max = 0.0;
                    body_min = 0.0;
                    body_max = 0.0;
                }
            }
            return result;
        }

        public bool isOverBBHigh(int past_num, int play)
        {
            bool result = false;
            try
            {
                Candlestick curCandle = getLastCandle();
                if (curCandle == null)
                {
                    result = false;
                    return result;
                }

                int candle_cnt = getCandleCount();
                if (candle_cnt <= 0)
                {
                    result = false;
                    return result;
                }

                int beg_idx = 0;
                if (candle_cnt >= (past_num + 1))
                {
                    beg_idx = candle_cnt - (past_num + 1);
                }

                for (int i = beg_idx; i < (candle_cnt-1); i++)
                {
                    Candlestick candle = m_candleList[i];
                    if (candle == null)
                    {
                        continue;
                    }
                    if (candle.isOverBBHigh(candle.last + play))
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

        public bool isUnderBBLow(int past_num, int play)
        {
            bool result = false;
            try
            {
                Candlestick curCandle = getLastCandle();
                if (curCandle == null)
                {
                    result = false;
                    return result;
                }

                int candle_cnt = getCandleCount();
                if (candle_cnt <= 0)
                {
                    result = false;
                    return result;
                }

                int beg_idx = 0;
                if (candle_cnt >= (past_num+1))
                {
                    beg_idx = candle_cnt - (past_num+1);
                }

                for (int i = beg_idx; i < (candle_cnt-1); i++)
                {
                    Candlestick candle = m_candleList[i];
                    if (candle == null)
                    {
                        continue;
                    }
                    if (candle.isUnderBBLow(candle.last - play))
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

        public bool isTurningHigh(int offset=2, double play=1000.0, int threshold=1)
        {
            bool result = false;
            try
            {
                int candle_cnt = getCandleCount();
                if (candle_cnt <= 0)
                {
                    result = false;
                    return result;
                }

                int beg_idx = (candle_cnt - 1) - offset;
                if(beg_idx < 0)
                {
                    result = false;
                    return result;  
                }

                Candlestick begCandle = m_candleList[beg_idx];
                if (begCandle == null)
                {
                    result = false;
                    return result;
                }
                Console.WriteLine("search-beg time={0} last={1}", begCandle.timestamp, begCandle.last);

                int crossCnt = 0;
                bool isCross = false;
                for (int i = beg_idx; i >= 0; i--)
                {
                    Candlestick candle = m_candleList[i];
                    if (candle == null)
                    {
                        continue;
                    }

                    if (candle.boll_high > candle.boll_high_top)
                    {
                        if(candle.isCrossMA(play))
                        {
                            if(!isCross)
                            {
                                crossCnt++;
                                isCross = true;
                                Console.WriteLine("search-cross time={0} last={1}", candle.timestamp, candle.last);
                            }
                        }
                        else
                        {
                            isCross = false;
                        }
                    }
                    else
                    {
                        isCross = false;
                    }

                    if(candle.isCrossMATop(play))
                    {
                        Console.WriteLine("search-end time={0} last={1}", candle.timestamp, candle.last);
                        break;
                    }
                }

                if(crossCnt>threshold)
                {
                    result = true;
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

        public bool isTurningLow(int offset = 2, double play = 1000.0, int threshold = 1)
        {
            bool result = false;
            try
            {
                int candle_cnt = getCandleCount();
                if (candle_cnt <= 0)
                {
                    result = false;
                    return result;
                }

                int beg_idx = (candle_cnt - 1) - offset;
                if (beg_idx < 0)
                {
                    result = false;
                    return result;
                }

                Candlestick begCandle = m_candleList[beg_idx];
                if(begCandle==null)
                {
                    result = false;
                    return result;
                }
                Console.WriteLine("search-beg time={0} last={1}", begCandle.timestamp, begCandle.last);

                int crossCnt = 0;
                bool isCross = false;
                for (int i = beg_idx; i >= 0; i--)
                {
                    Candlestick candle = m_candleList[i];
                    if (candle == null)
                    {
                        continue;
                    }

                    if (candle.boll_low < candle.boll_low_top)
                    {
                        if (candle.isCrossMA(play))
                        {
                            if (!isCross)
                            {
                                crossCnt++;
                                isCross = true;
                                Console.WriteLine("search-cross time={0} last={1}", candle.timestamp, candle.last);
                            }
                        }
                        else
                        {
                            isCross = false;
                        }
                    }
                    else
                    {
                        isCross = false;
                    }

                    if (candle.isCrossMATop(play))
                    {
                        Console.WriteLine("search-end time={0} last={1}", candle.timestamp, candle.last);
                        break;
                    }
                }

                if (crossCnt > threshold)
                {
                    result = true;
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


        public int searchLastOutsideBB(out int band_pos, out string outside_stamp, out string cross_stamp, out int back_cnt, out int matop_cross_cnt)
        {
            int result = -1;
            band_pos = 0;
            outside_stamp = "";
            cross_stamp = "";
            back_cnt = 0;
            matop_cross_cnt = 0;
            try
            {
                int candle_cnt = getCandleCount();
                if (candle_cnt <= 0)
                {
                    result = -1;
                    return result;
                }

                int beg_idx = (candle_cnt - 1) - 1;
                if (beg_idx < 0)
                {
                    result = -1;
                    return result;
                }

                bool isCross = false;
                for (int i = (beg_idx-1); i >= 0; i--)
                {
                    Candlestick candle = m_candleList[i];
                    if (candle == null)
                    {
                        continue;
                    }
                    back_cnt++;

                    if (candle.isCrossMATop())
                    {
                        matop_cross_cnt++;
                        cross_stamp = candle.timestamp;
                    }

                    if (candle.boll_low < candle.boll_low_top)
                    {
                        // BOLLが上位BOLLより下にはみ出した場合
                        if (candle.isCrossBBLowTop())
                        {
                            // 上位BOLLにキャンドルが交差した場合
                            band_pos = -1;
                            isCross = true;
                            outside_stamp = candle.timestamp;
                            //Console.WriteLine("Hit!! LastOutsideBB Low {0} LAST={1:0} CROSS={2}", candle.timestamp, candle.last, matop_cross_cnt);
                            break;
                        }
                    }

                    if (candle.boll_high > candle.boll_high_top)
                    {
                        // BOLLが上位BOLLより上にはみ出した場合
                        if (candle.isCrossBBHighTop())
                        {
                            // 上位BOLLにキャンドルが交差した場合
                            band_pos = 1;
                            isCross = true;
                            outside_stamp = candle.timestamp;
                            //Console.WriteLine("Hit!! LastOutsideBB High {0} LAST={1:0} CROSS={2}", candle.timestamp, candle.last, matop_cross_cnt);
                            break;
                        }
                    }
                }

                if (isCross)
                {
                    result = 0;
                    return result;
                }
                else
                {
                    Console.WriteLine("Miss!! search LastOutsideBB.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = -1;
            }
            finally
            {
                if (result != 0)
                {
                    band_pos = 0;
                    outside_stamp = "";
                    cross_stamp = "";
                    back_cnt = 0;
                    matop_cross_cnt = 0;
                }
            }
            return result;
        }

        public int getEMACrossState(out bool isGolden, out bool isFirst, out int back_cnt, out double cur_ema_length, double threshold_rate = 0.6)
        {
            int result = -1;
            isGolden = false;
            isFirst = false;
            back_cnt = 0;
            cur_ema_length = 0.0;
            try
            {
                int candle_cnt = getCandleCount();
                if (candle_cnt <= 0)
                {
                    result = -1;
                    return result;
                }

                int cur_idx = candle_cnt - 1;
                int beg_idx = cur_idx - 1;
                if (beg_idx < 0)
                {
                    result = -1;
                    return result;
                }

                Candlestick curCandle = m_candleList[cur_idx];
                if (curCandle == null)
                {
                    result = -1;
                    return result;
                }

                //double 
                cur_ema_length = Math.Abs(curCandle.ema - curCandle.ema_sub);
                int cur_cross_state = 0;
                if (curCandle.ema > curCandle.ema_sub)
                {
                    // GOLDEN
                    isGolden = true;
                    cur_cross_state = 1;
                }
                else if (curCandle.ema < curCandle.ema_sub)
                {
                    // DEAD
                    isGolden = false;
                    cur_cross_state = -1;
                }
                else
                {
                    // CROSS
                    cur_cross_state = 0;
                    isFirst = true;
                }
                

                double max_ema_length = 0.0;
                for (int i = beg_idx; i >= 0; i--)
                {
                    Candlestick candle = m_candleList[i];
                    if (candle == null)
                    {
                        continue;
                    }
                    back_cnt++;

                    double ema_length = Math.Abs(candle.ema - candle.ema_sub);
                    max_ema_length = Math.Max(ema_length, max_ema_length);

                    if (cur_cross_state == 1)
                    {
                        // GOLDEN
                        if (candle.ema <= candle.ema_sub)
                        {
							//Console.WriteLine("golden-cross from={0} now={1} cnt={2} ema={3:0} ema_sub={4:0}", candle.timestamp, curCandle.timestamp, back_cnt, curCandle.ema, curCandle.ema_sub);
                            // CROSS
                            break;
                        }
                    }
                    else if (cur_cross_state == -1)
                    {
                        // DEAD
                        if (candle.ema >= candle.ema_sub)
                        {
							//Console.WriteLine("dead-cross from={0} now={1} cnt={2} ema={3:0} ema_sub={4:0}", candle.timestamp, curCandle.timestamp, back_cnt, curCandle.ema, curCandle.ema_sub);
                            // CROSS
                            break;
                        }
                    }
                    else if (cur_cross_state == 0)
                    {
                        // CROSS
                        if (candle.ema < candle.ema_sub)
                        {
							//Console.WriteLine("golden-cross-now from={0} now={1} cnt={2} ema={3:0} ema_sub={4:0}", candle.timestamp, curCandle.timestamp, back_cnt, curCandle.ema, curCandle.ema_sub);
                            // GOLDEN
                            isGolden = true;
                            break;
                        }
                        else if (candle.ema > candle.ema_sub)
                        {
							//Console.WriteLine("dead-cross-now from={0} now={1} cnt={2} ema={3:0} ema_sub={4:0}", candle.timestamp, curCandle.timestamp, back_cnt, curCandle.ema, curCandle.ema_sub);
                            // DEAD
                            isGolden = false;
                            break;
                        }
                    }

                }

                if (cur_cross_state != 0)
                {
                    if (cur_ema_length < (max_ema_length*threshold_rate))
                    {
                        // 収束中
                        isFirst = false;
                    }
                    else
                    {
                        // 拡大中
                        isFirst = true;
                    }
                }

                result = 0;
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

        public int getNearEmaCandle(out Candlestick reverceCandle, bool entry_side, string entry_date, double boll_div=8.0)
        {
            int result = 0;
            reverceCandle = null;

            try
            {
                int candle_cnt = getCandleCount();
                if (candle_cnt <= 0)
                {
                    result = -1;
                    return result;
                }

                Candlestick curCandle = getLastCandle();
                if (curCandle == null)
                {
                    result = -1;
                    return result;
                }

                Candlestick entryCandle = null;
                double diff_min = double.MaxValue;
                for (int i = (candle_cnt - 1); i >= 0; i--)
                {
                    Candlestick candle = m_candleList[i];
                    if (candle == null)
                    {
                        continue;
                    }


                    if (candle.timestamp == entry_date)
                    {
                        entryCandle = candle;
                        break;
                    }

                    double diff = Math.Abs(candle.ema - candle.last);
                    //if (entry_side)
                    //{
                    //    // LONGの場合
                    //    diff = Math.Abs(candle.ema - candle.high);
                    //}
                    //else
                    //{
                    //    // SHORTの場合
                    //    diff = Math.Abs(candle.low - candle.ema);
                    //}

                    if (diff_min > diff)
                    {
                        diff_min = diff;
                        reverceCandle = candle;
                    }
                }

                if (entryCandle == null)
                {
                    result = -1;
                    return result;
                }

                if (reverceCandle == null)
                {
                    result = 1;
                    return result;
                }

                if (curCandle.timestamp == reverceCandle.timestamp)
                {
                    result = 2;
                    return result;
                }

                double high_length = reverceCandle.boll_high - reverceCandle.ema;
                double low_length = reverceCandle.ema - reverceCandle.boll_low;

                if (entry_side)
                {
                    // LONGの場合

                    if (diff_min > (low_length / boll_div))
                    {
                        result = 3;
                        return result;
                    }
                }
                else
                {
                    // SHORTの場合

                    if (diff_min > (high_length / boll_div))
                    {
                        result = 3;
                        return result;
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
                if (result != 0)
                {
                    reverceCandle = null;
                }
            }
            return result;
        }

        public int getMinMaxProfitCandle
        (
            out Candlestick minCandle,
            out Candlestick maxCandle,
            out Candlestick entryCandle,
            bool entry_side,
            string entry_date,
            double entry_price
        )
        {
            int result = 0;
            minCandle = null;
            maxCandle = null;
            entryCandle = null;

            try
            {
                int candle_cnt = getCandleCount();
                if (candle_cnt <= 0)
                {
                    result = -1;
                    return result;
                }

                Candlestick curCandle = getLastCandle();
                if (curCandle == null)
                {
                    result = -1;
                    return result;
                }

                double profit_min = double.MaxValue;
                double profit_max = double.MinValue;
                for (int i = (candle_cnt - 1); i >= 0; i--)
                {
                    Candlestick candle = m_candleList[i];
                    if (candle == null)
                    {
                        continue;
                    }

                    if (candle.timestamp == entry_date)
                    {
                        entryCandle = candle;
                        break;
                    }

                    double profit = 0.0;
                    if (entry_side)
                    {
                        // LONGの場合
                        profit = candle.last - entry_price;
                    }
                    else
                    {
                        // SHORTの場合
                        profit = entry_price - candle.last;
                    }

                    if (profit_min > profit)
                    {
                        profit_min = profit;
                        minCandle = candle;
                    }

                    if (profit_max < profit)
                    {
                        profit_max = profit;
                        maxCandle = candle;
                    }
                }

                if (entryCandle == null)
                {
                    result = -1;
                    return result;
                }

                if (minCandle == null)
                {
                    result = 1;
                    return result;
                }

                if (maxCandle == null)
                {
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
                if (result != 0)
                {
                    minCandle = null;
                    maxCandle = null;
                    entryCandle = null;
                }
            }
            return result;
        }

    }
}
