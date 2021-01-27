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

        public int range_min_keep { get; set; }
        public int range_max_keep { get; set; }

        public int range_min_cnt { get; set; }
        public int range_max_cnt { get; set; }

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

            range_min_keep = 0;
            range_max_keep = 0;

            range_min_cnt = 0;
            range_max_cnt = 0;

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

        public bool isCross(double value, double play = 0.0)
        {
            if (value >= (low - play) && value <= (high + play))
            {
                return true;
            }
            return false;
        }

        public bool isCrossEMA(double play=0.0)
        {
            return isCross(ema, play);
        }

        public bool isCrossEMAsub(double play = 0.0)
        {
            return isCross(ema_sub, play);
        }

        public bool isCrossMA(double play = 0.0)
        {
            return isCross(ma, play);
        }

        public bool isCrossMATop(double play = 0.0)
        {
            return isCross(ma_top, play);
        }

        public bool isCrossBBHigh(double play = 0.0)
        {
            return isCross(boll_high, play);
        }

        public bool isCrossBBLow(double play = 0.0)
        {
            return isCross(boll_low, play);
        }

        public bool isCrossBBHighTop(double play = 0.0)
        {
            return isCross(boll_high_top, play);
        }

        public bool isCrossBBLowTop(double play = 0.0)
        {
            return isCross(boll_low_top, play);
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
			Console.WriteLine("m_buffer_num={0} cnt={1}", m_buffer_num, getCandleCount());
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

        public int getLastCandleIndex()
        {
            if (getCandleCount() <= 0)
            {
                return -1;
            }

            return (getCandleCount() - 1);
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
            string _timestamp,
            bool isRemoveOverBuffer = true
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

                if (addCandle(candle, isRemoveOverBuffer) != 0)
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

        public Candlestick addCandle
        (
            double _high,
            double _low,
            double _open,
            double _last,
            double _volume,
            string _timestamp,
            int _periods,
            bool isRemoveOverBuffer = true
        )
        {
            Candlestick result = null;
            try
            {
                DateTime curTimestamp = DateTime.Parse(_timestamp);

                if (getCandleCount() > 0)
                {
                    Candlestick prevCandle = getLastCandle();
                    if (prevCandle == null)
                    {
                        return result;
                    }

                    DateTime prevTimestamp = DateTime.Parse(prevCandle.timestamp);

                    TimeSpan span = curTimestamp - prevTimestamp;
                    int elapsed_sec = (int)(span.TotalSeconds);
                    if (elapsed_sec > _periods)
                    {
                        int dummy_num = elapsed_sec / _periods - 1;
                        for (int j = 0; j < dummy_num; j++)
                        {
                            double add_minutes = (_periods / 60) * (j + 1);
                            DateTime dummyTimestamp = prevTimestamp.AddMinutes(add_minutes);
                            Candlestick dummyCandle = addCandle(prevCandle.high, prevCandle.low, prevCandle.open, prevCandle.last, dummyTimestamp.ToString(), isRemoveOverBuffer);
                            if (dummyCandle == null)
                            {
                                Console.WriteLine("failed to addCandle.");
                                continue;
                            }
                            dummyCandle.volume = prevCandle.volume;
                        }
                    }
                    else if (_last <= Double.Epsilon)
                    {

                        _last = prevCandle.last;
                        _open = prevCandle.open;
                        _high = prevCandle.high;
                        _low = prevCandle.low;
                        _volume = prevCandle.volume;
                    }
                }

                Candlestick candle = addCandle(_high, _low, _open, _last, _timestamp, isRemoveOverBuffer);
                if (candle == null)
                {
                    Console.WriteLine("failed to addCandle.");
                }
                candle.volume = _volume;

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

        public int addCandle( Candlestick candle , bool isRemoveOverBuffer = true)
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
                    if (isRemoveOverBuffer)
                    {
                        // 何個消すか算出
                        int remove_cnt = m_candleList.Count - m_buffer_num;

                        // 古いCandlestickから削除
                        m_candleList.RemoveRange(0, remove_cnt);
                    }
                    else
                    {
                        m_buffer_num = m_candleList.Count;
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

        public bool isOverBBHigh(out Candlestick overCandle, int past_num, double play)
        {
            bool result = false;
            overCandle = null;
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
                        overCandle = candle;
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
                if (!result)
                {
                    overCandle = null;
                }
            }
            return result;
        }

        public bool isUnderBBLow(out Candlestick underCandle, int past_num, double play)
        {
            bool result = false;
            underCandle = null;
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
                        underCandle = candle;
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
                if (!result)
                {
                    underCandle = null;
                }
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

        public int getEMACrossState
        (
            out bool isGolden, 
            out bool isBeg, 
            out int back_cnt, 
            out double cur_ema_length, 
            out bool isTouchEma,
            out bool isTouchEmaSub,
			out double high_max,
            out int high_max_idx,
            out double low_min,
            out int low_min_idx,
            double threshold_rate = 0.5,
            double ema_touch_play = 0.0,
			int cross_over_cnt = 65
        )
        {
            int result = -1;
            isGolden = false;
            isBeg = false;
            back_cnt = 0;
            cur_ema_length = 0.0;
            isTouchEma = false;
            isTouchEmaSub = false;
			high_max = 0.0;
			high_max_idx = 0;
            low_min = 0.0;
			low_min_idx = 0;
            
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

				high_max = curCandle.high;
                low_min = curCandle.low;

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
                    isBeg = true;
                }

                double max_ema_length = 0.0;
                int itr_idx = 0;
                for (itr_idx = beg_idx; itr_idx >= 0; itr_idx--)
                {
                    Candlestick candle = m_candleList[itr_idx];
                    if (candle == null)
                    {
                        continue;
                    }
                    back_cnt++;

                    double ema_length = Math.Abs(candle.ema - candle.ema_sub);
                    max_ema_length = Math.Max(ema_length, max_ema_length);

					//high_max = Math.Max(high_max,candle.high);
					//low_min = Math.Min(low_min, candle.low);

					if(candle.high>high_max)
					{
						high_max = candle.high;
						high_max_idx = itr_idx;
					}

					if (candle.low < low_min)
                    {
						low_min = candle.low;
						low_min_idx = itr_idx;
                    }

                    if (!isTouchEma)
                    {
                        if (candle.isCrossEMA())
                        {
                            isTouchEma = true;
                        }
                    }

                    if (!isTouchEmaSub)
                    {
                        if (candle.isCrossEMAsub())
                        {
                            isTouchEmaSub = true;
                        }
                    }

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



				if(itr_idx > cross_over_cnt)
				{
					for (int i = itr_idx - 1; i >= (itr_idx - cross_over_cnt); i--)
                    {
                        Candlestick candle = m_candleList[i];
                        if (candle == null)
                        {
                            continue;
                        }

                        if (cur_cross_state == 1)
                        {
                            // GOLDEN
							if (candle.low < low_min)
                            {
                                low_min = candle.low;
                                low_min_idx = i;
                            }
                        }
                        
						if (cur_cross_state == -1)
                        {
                            // DEAD
							if (candle.high > high_max)
                            {
                                high_max = candle.high;
                                high_max_idx = i;
                            }
                        }                  
                    }               
				}

                if (cur_cross_state != 0)
                {
                    //if (cur_ema_length < (max_ema_length * threshold_rate))
                    //{
                    //    // 収束中
                    //    isBeg = false;
                    //}
                    //else
                    //{
                    //    // 拡大中
                    //    isBeg = true;
                    //}

                    List<double> peakValueList = new List<double>();
                    List<int> peakIndexList = new List<int>();
                    if (calcPeakListEmaDiff(ref peakValueList, ref peakIndexList, cur_idx, cur_idx - back_cnt, isGolden, 5) != 0)
                    {
                        return result;
                    }

                    //Console.WriteLine("#MinMax ema");
                    //foreach (double value in peakList)
                    //for (int i = 0; i < peakValueList.Count(); i++)
                    //{
                    //    double value = peakValueList[i];
                    //    int index = peakIndexList[i];
                    //    Console.WriteLine("{0} {1}", index, (value - peakValueList.Min()) / (peakValueList.Max() - peakValueList.Min()));
                    //}
                    if (peakValueList.Count() > 2)
                    {
                        double valueA = peakValueList[peakValueList.Count() - 2];
                        double valueB = peakValueList[peakValueList.Count() - 1];
                        double normValA = (valueA - peakValueList.Min()) / (peakValueList.Max() - peakValueList.Min());
                        double normValB = (valueB - peakValueList.Min()) / (peakValueList.Max() - peakValueList.Min());

                        int indexA = peakIndexList[peakIndexList.Count() - 2];
                        int indexB = peakIndexList[peakIndexList.Count() - 1];
                        double normIdxA = (double)(indexA - peakIndexList.Min()) / (double)(peakIndexList.Max() - peakIndexList.Min());
                        double normIdxB = (double)(indexB - peakIndexList.Min()) / (double)(peakIndexList.Max() - peakIndexList.Min());

                        double tilt = (normValB - normValA) / (normIdxB - normIdxA);
                        //Console.WriteLine("{0} {1}", tilt, normValB);

                        if (tilt >= -1.5 && normValB >= threshold_rate)
                        {
                            // 拡大中
                            isBeg = true;
                        }
                        else
                        {
                            // 収束中
                            isBeg = false;
                        }
                    }
                }
                ////if (curCandle.timestamp == "2021/01/18 23:00:00")//
                ////if(curCandle.timestamp == "2021/01/18 19:29:00")
                //if (curCandle.timestamp == "2021/01/19 23:50:00")
                //{
                //    List<double> peakValueList = new List<double>();
                //    List<int> peakIndexList = new List<int>();
                //    int back_idx = 0;// cur_idx - 201;
                //    int sample_num = 240;//60;//15;
                //    if (calcPeakList(ref peakValueList, ref peakIndexList, cur_idx, back_idx, sample_num) != 0)
                //    {
                //        return result;
                //    }
                //}


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

        private int calcMinMaxListEmaDiff
        (
            ref SortedSet<int> minIndexList,
            ref SortedSet<int> maxIndexList,
            int cur_idx,
            int back_idx,
            bool isGolden,
            int stride_num
        )
        {
            int result = -1;

            try
            {
                if(minIndexList==null)
                {
                    return result;
                }

                if (maxIndexList == null)
                {
                    return result;
                }

                minIndexList.Clear();
                maxIndexList.Clear();
                
                double value_min = 0.0;
                double value_max = 0.0;
                int value_min_idx = -1;
                int value_max_idx = -1;
                if (getMinMaxEmaDiff(out value_min, out value_max, out value_min_idx, out value_max_idx, cur_idx, back_idx, isGolden) != 0)
                {
                    return result;
                }

                double peak_min = double.MaxValue;
                double peak_max = double.MinValue;
                int peak_min_idx = -1;
                int peak_max_idx = -1;
                int stride_idx = 0;
                for (int i = back_idx; i <= cur_idx; i++)
                {
                    Candlestick candle = m_candleList[i];
                    if (candle == null)
                    {
                        continue;
                    }

                    double value = 0.0;
                    if (isGolden)
                    {
                        value = candle.ema - candle.ema_sub;
                    }
                    else
                    {
                        value = candle.ema_sub - candle.ema;
                    }

                    if (value < peak_min)
                    {
                        // UPDATE(MIN)
                        peak_min = value;
                        peak_min_idx = i;
                    }

                    if (value > peak_max)
                    {
                        // UPDATE(MAX)
                        peak_max = value;
                        peak_max_idx = i;
                    }

                    stride_idx++;
                    if (stride_idx >= stride_num || ((stride_idx < stride_num) && (i == cur_idx)))
                    {
                        minIndexList.Add(peak_min_idx);
                        maxIndexList.Add(peak_max_idx);

                        // Reset
                        peak_min = double.MaxValue;
                        peak_max = double.MinValue;
                        peak_min_idx = -1;
                        peak_max_idx = -1;
                        stride_idx = 0;
                    }
                }

                if (maxIndexList.Count() != minIndexList.Count())
                {
                    return result;
                }
                result = 0;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                result = -1;
            }
            finally
            {
                if(result!=0)
                {
                    if (minIndexList != null)
                    {
                        minIndexList.Clear();
                    }

                    if (maxIndexList != null)
                    {
                        maxIndexList.Clear();
                    }
                }
            }
            return result;
        }

        public int calcPeakListEmaDiff
        (
            ref List<double> peakValueList,
            ref List<int> peakIndexList,
            int cur_idx,
            int back_idx,
            bool isGolden,
            int stride_num = 5
        )
        {
            int result = -1;

            try
            {
                if (peakValueList == null)
                {
                    return result;
                }
                peakValueList.Clear();

                if (peakIndexList == null)
                {
                    return result;
                }
                peakIndexList.Clear();


                SortedSet<int> minIndexList = new SortedSet<int>();
                SortedSet<int> maxIndexList = new SortedSet<int>();
                if (calcMinMaxListEmaDiff(ref minIndexList, ref maxIndexList, cur_idx, back_idx, isGolden, stride_num) != 0)
                {
                    return result;
                }

                bool isLastMax = false;
                bool isFirst = true;
                for (int i = 0; i < maxIndexList.Count(); i++)
                {
                    int min_idx = minIndexList.ElementAt(i);
                    int max_idx = maxIndexList.ElementAt(i);

                    Candlestick minCandle = m_candleList[min_idx];
                    if (minCandle == null)
                    {
                        continue;
                    }

                    Candlestick maxCandle = m_candleList[max_idx];
                    if (maxCandle == null)
                    {
                        continue;
                    }

                    double minValue = 0.0;
                    double maxValue = 0.0;
                    if (isGolden)
                    {
                        minValue = minCandle.ema - minCandle.ema_sub;
                        maxValue = maxCandle.ema - maxCandle.ema_sub;
                    }
                    else
                    {
                        minValue = minCandle.ema_sub - minCandle.ema;
                        maxValue = maxCandle.ema_sub - maxCandle.ema;
                    }

                    if (min_idx < max_idx)
                    {
                        // min max の順で格納
                        if (isFirst)
                        {
                            peakValueList.Add(minValue);
                            peakValueList.Add(maxValue);
                            isLastMax = true;
                            isFirst = false;
                            peakIndexList.Add(min_idx);
                            peakIndexList.Add(max_idx);
                        }
                        else
                        {
                            if (isLastMax)
                            {
                                // 前回もmin max の順

                                if (peakValueList.Last() < minValue)
                                {
                                    // 前回maxの方が今回minより小さい

                                    // 今回のminは入れない
                                    // 前回maxを今回maxに入れ替え
                                    peakValueList[peakValueList.Count() - 1] = maxValue;
                                    peakIndexList[peakIndexList.Count() - 1] = max_idx;
                                    isLastMax = true;
                                }
                                else if (peakValueList.Last() > minValue)
                                {
                                    // 前回maxの方が今回minより大きい

                                    // 同じ順序で格納

                                    if (peakIndexList.Last() != min_idx)
                                    {
                                        peakValueList.Add(minValue);
                                        peakIndexList.Add(min_idx);
                                    }
                                    if (peakIndexList.Last() != max_idx)
                                    {
                                        peakValueList.Add(maxValue);
                                        peakIndexList.Add(max_idx);
                                    }
                                    isLastMax = true;
                                }
                            }
                            else
                            {
                                // 前回はmax min の順

                                if (peakValueList.Last() > minValue)
                                {
                                    // 前回minより今回minの方が小さい
                                    // 前回minを今回minに入れ替え
                                    peakValueList[peakValueList.Count() - 1] = minValue;
                                    peakIndexList[peakIndexList.Count() - 1] = min_idx;
                                }
                                else if (peakValueList.Last() < minValue)
                                {
                                    // 前回minは今回minより小さい
                                    // 今回minは入れない
                                }

                                if (peakIndexList.Last() != max_idx)
                                {
                                    peakValueList.Add(maxValue);
                                    peakIndexList.Add(max_idx);

                                }
                                isLastMax = true;

                            }
                        }
                    }
                    else
                    {
                        // max min の順で格納
                        if (isFirst)
                        {
                            peakValueList.Add(maxValue);
                            peakValueList.Add(minValue);
                            isLastMax = false;
                            isFirst = false;
                            peakIndexList.Add(max_idx);
                            peakIndexList.Add(min_idx);
                        }
                        else
                        {
                            if (!isLastMax)
                            {
                                // 前回もmax min の順

                                if (peakValueList.Last() > maxValue)
                                {
                                    // 前回minの方が今回maxより大きい

                                    // 今回のmaxは入れない
                                    // 前回minの今回のminに入れ替え
                                    peakValueList[peakValueList.Count() - 1] = minValue;
                                    peakIndexList[peakIndexList.Count() - 1] = min_idx;
                                    isLastMax = false;
                                }
                                else if (peakValueList.Last() < maxValue)
                                {
                                    // 前回minの方が今回maxより小さい

                                    // 同じ順序で格納

                                    if (peakIndexList.Last() != max_idx)
                                    {
                                        peakValueList.Add(maxValue);
                                        peakIndexList.Add(max_idx);
                                    }

                                    if (peakIndexList.Last() != min_idx)
                                    {
                                        peakValueList.Add(minValue);
                                        peakIndexList.Add(min_idx);
                                    }
                                    isLastMax = false;
                                }
                            }
                            else
                            {
                                // 前回はmin max の順

                                if (peakValueList.Last() < maxValue)
                                {
                                    // 前回maxより今回maxの方が大きい
                                    // 前回maxを今回maxに入れ替え
                                    peakValueList[peakValueList.Count() - 1] = maxValue;
                                    peakIndexList[peakIndexList.Count() - 1] = max_idx;
                                }
                                else if (peakValueList.Last() > maxValue)
                                {
                                    // 前回maxは今回maxより大きい
                                    // 今回maxは入れない
                                }

                                if (peakIndexList.Last() != min_idx)
                                {
                                    peakValueList.Add(minValue);
                                    peakIndexList.Add(min_idx);
                                }
                                isLastMax = false;
                            }
                        }
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
                if (result != 0)
                {
                    if (peakValueList != null)
                    {
                        peakValueList.Clear();
                    }
                    
                    if (peakIndexList != null)
                    {
                        peakIndexList.Clear();
                    }
                }
            }
            return result;
        }

        public int getMinMaxEmaDiff
        (
            out double value_min,
            out double value_max,
            out int value_min_idx,
            out int value_max_idx,
            int cur_idx,
            int back_idx,
            bool isGolden
        )
        {
            int result = 0;
            value_min_idx = -1;
            value_max_idx = -1;
            value_min = double.MaxValue;
            value_max = double.MinValue;
            try
            {
                if (back_idx >= cur_idx)
                {
                    result = -1;
                    return result;
                }

                if (back_idx < 0)
                {
                    result = -1;
                    return result;
                }

                if (cur_idx >= m_candleList.Count())
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

                for (int i = back_idx; i <= cur_idx; i++)
                {
                    Candlestick candle = m_candleList[i];
                    if (candle == null)
                    {
                        continue;
                    }

                    double value = 0.0;
                    if (isGolden)
                    {
                        value = candle.ema - candle.ema_sub;
                    }
                    else
                    {
                        value = candle.ema_sub - candle.ema;
                    }

                    if (value < value_min)
                    {
                        value_min = value;
                        value_min_idx = i;
                    }

                    if (value > value_max)
                    {
                        value_max = value;
                        value_max_idx = i;
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
                    value_min_idx = -1;
                    value_max_idx = -1;
                    value_min = double.MaxValue;
                    value_max = double.MinValue;
                }
            }
            return result;
        }

        public int getMinMaxCandleValue
        (
            out double value_min,
            out double value_max
        )
        {
            int result = -1;
            value_min = double.MaxValue;
            value_max = double.MinValue;
            try
            {
                if(m_candleList.Count()<=0)
                {
                    return result;
                }
                value_min = m_candleList.Min(x => x.low);
                value_max = m_candleList.Max(x => x.high);

                result = 0;
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
                    value_min = double.MaxValue;
                    value_max = double.MinValue;
                }
            }
            return result;
        }

        public int getMinMaxCandle
        (
            out double value_min,
            out double value_max,
            out int value_min_idx,
            out int value_max_idx,
            int cur_idx,
            int back_idx
        )
        {
            int result = 0;
            value_min_idx = -1;
            value_max_idx = -1;
            value_min = double.MaxValue;
            value_max = double.MinValue;
            try
            {
                if(back_idx >= cur_idx)
                {
                    result = -1;
                    return result;
                }

                if (back_idx < 0)
                {
                    result = -1;
                    return result;
                }

                if (cur_idx >= m_candleList.Count())
                {
                    result = -1;
                    return result;
                }

                for (int i = back_idx; i <= cur_idx; i++)
                {
                    Candlestick candle = m_candleList[i];
                    if (candle == null)
                    {
                        continue;
                    }

                    if (candle.low < value_min)
                    {
                        value_min = candle.low;
                        value_min_idx = i;
                    }

                    if (candle.high > value_max)
                    {
                        value_max = candle.high;
                        value_max_idx = i;
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
                    value_min_idx = -1;
                    value_max_idx = -1;
                    value_min = double.MaxValue;
                    value_max = double.MinValue;
                }
            }
            return result;
        }

        private int calcMinMaxList
        (
           ref SortedSet<int> minIndexList,
           ref SortedSet<int> maxIndexList,
           int cur_idx,
           int back_idx,
           int stride_num
        )
        {
            int result = -1;

            try
            {
                if (minIndexList == null)
                {
                    return result;
                }

                if (maxIndexList == null)
                {
                    return result;
                }

                if(stride_num<=0)
                {
                    return result;
                }

                minIndexList.Clear();
                maxIndexList.Clear();

                double value_min = 0.0;
                double value_max = 0.0;
                int value_min_idx = -1;
                int value_max_idx = -1;
                if (getMinMaxCandle(out value_min, out value_max, out value_min_idx, out value_max_idx, cur_idx, back_idx) != 0)
                {
                    return result;
                }

                for (int i = back_idx; i <= cur_idx; i+=stride_num)
                {
                    int old_idx = i;
                    int now_idx = i + (stride_num-1);
                    if(now_idx>cur_idx)
                    {
                        now_idx = cur_idx;
                    }

                    if(old_idx < now_idx)
                    {
                        double scope_min = 0.0;
                        double scope_max = 0.0;
                        int scope_min_idx = -1;
                        int scope_max_idx = -1;
                        if (getMinMaxCandle(out scope_min, out scope_max, out scope_min_idx, out scope_max_idx, now_idx, old_idx) != 0)
                        {
                            return result;
                        }

                        minIndexList.Add(scope_min_idx);
                        maxIndexList.Add(scope_max_idx);
                    }
                    else if(old_idx==now_idx)
                    {
                        minIndexList.Add(old_idx);
                        maxIndexList.Add(old_idx);
                    }
                }

                if (maxIndexList.Count() != minIndexList.Count())
                {
                    return result;
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
                if (result != 0)
                {
                    if (minIndexList != null)
                    {
                        minIndexList.Clear();
                    }

                    if (maxIndexList != null)
                    {
                        maxIndexList.Clear();
                    }
                }
            }
            return result;
        }

        public int calcPeakList
        (
            ref List<double> peakValueList,
            ref List<int> peakIndexList,
            int cur_idx,
            int back_idx,
            int stride_num = 5
        )
        {
            int result = -1;

            try
            {
                if (peakValueList == null)
                {
                    return result;
                }
                peakValueList.Clear();

                if (peakIndexList == null)
                {
                    return result;
                }
                peakIndexList.Clear();


                SortedSet<int> minIndexList = new SortedSet<int>();
                SortedSet<int> maxIndexList = new SortedSet<int>();
                if (calcMinMaxList(ref minIndexList, ref maxIndexList, cur_idx, back_idx, stride_num) != 0)
                {
                    return result;
                }

                bool isLastMax = false;
                bool isFirst = true;
                for (int i = 0; i < maxIndexList.Count(); i++)
                {
                    int min_idx = minIndexList.ElementAt(i);
                    int max_idx = maxIndexList.ElementAt(i);

                    Candlestick minCandle = m_candleList[min_idx];
                    if (minCandle == null)
                    {
                        continue;
                    }

                    Candlestick maxCandle = m_candleList[max_idx];
                    if (maxCandle == null)
                    {
                        continue;
                    }

                    double min_value = minCandle.low;
                    double max_value = maxCandle.high;

                    if (min_idx < max_idx)
                    {
                        // min max の順で格納
                        if (isFirst)
                        {
                            peakValueList.Add(min_value);
                            peakValueList.Add(max_value);
                            isLastMax = true;
                            isFirst = false;
                            peakIndexList.Add(min_idx);
                            peakIndexList.Add(max_idx);
                        }
                        else
                        {
                            if (isLastMax)
                            {
                                // 前回もmin max の順

                                if (peakValueList.Last() < min_value)
                                {
                                    // 前回maxの方が今回minより小さい

                                    // 今回のminは入れない
                                    // 前回maxを今回maxに入れ替え
                                    peakValueList[peakValueList.Count() - 1] = max_value;
                                    peakIndexList[peakIndexList.Count() - 1] = max_idx;
                                    isLastMax = true;
                                }
                                else if (peakValueList.Last() > min_value)
                                {
                                    // 前回maxの方が今回minより大きい

                                    // 今回minから前回maxの間で最も小さい位置を算出
                                    double scope_min_value = min_value;
                                    double scope_max_value = 0.0;
                                    int scope_min_idx = min_idx;
                                    int scope_max_idx = -1;
                                    int last_idx = peakIndexList.Last();// +1;
                                    if (min_idx > last_idx)
                                    {
                                        if (getMinMaxCandle(out scope_min_value, out scope_max_value, out scope_min_idx, out scope_max_idx, min_idx, last_idx) != 0)
                                        {
                                            return result;
                                        }
                                    }

                                    // 同じ順序で格納

                                    //if (peakIndexList.Last() != scope_min_idx)
                                    {
                                        peakValueList.Add(scope_min_value);
                                        peakIndexList.Add(scope_min_idx);
                                    }
                                    //if (peakIndexList.Last() != max_idx)
                                    {
                                        peakValueList.Add(max_value);
                                        peakIndexList.Add(max_idx);
                                    }
                                    isLastMax = true;
                                }
                            }
                            else
                            {
                                // 前回はmax min の順

                                if (peakValueList.Last() > min_value)
                                {
                                    // 前回minより今回minの方が小さい
                                    // 前回minを今回minに入れ替え
                                    peakValueList[peakValueList.Count() - 1] = min_value;
                                    peakIndexList[peakIndexList.Count() - 1] = min_idx;
                                }
                                else if (peakValueList.Last() < min_value)
                                {
                                    // 前回minは今回minより小さい
                                    // 今回minは入れない
                                }

                                //if (peakIndexList.Last() != max_idx)
                                {
                                    peakValueList.Add(max_value);
                                    peakIndexList.Add(max_idx);

                                }
                                isLastMax = true;

                            }
                        }
                    }
                    else
                    {
                        // max min の順で格納
                        if (isFirst)
                        {
                            peakValueList.Add(max_value);
                            peakValueList.Add(min_value);
                            isLastMax = false;
                            isFirst = false;
                            peakIndexList.Add(max_idx);
                            peakIndexList.Add(min_idx);
                        }
                        else
                        {
                            if (!isLastMax)
                            {
                                // 前回もmax min の順

                                if (peakValueList.Last() > max_value)
                                {
                                    // 前回minの方が今回maxより大きい

                                    // 今回のmaxは入れない
                                    // 前回minの今回のminに入れ替え
                                    peakValueList[peakValueList.Count() - 1] = min_value;
                                    peakIndexList[peakIndexList.Count() - 1] = min_idx;
                                    isLastMax = false;
                                }
                                else if (peakValueList.Last() < max_value)
                                {
                                    // 前回minの方が今回maxより小さい

                                    // 今回maxから前回minの間で最も小さい位置を算出
                                    double scope_min_value = 0.0;
                                    double scope_max_value = max_value;
                                    int scope_min_idx = -1;
                                    int scope_max_idx = max_idx;
                                    int last_idx = peakIndexList.Last();// +1;
                                    if (max_idx > last_idx)
                                    {
                                        if (getMinMaxCandle(out scope_min_value, out scope_max_value, out scope_min_idx, out scope_max_idx, max_idx, last_idx) != 0)
                                        {
                                            return result;
                                        }
                                    }

                                    // 同じ順序で格納

                                    //if (peakIndexList.Last() != scope_max_idx)
                                    {
                                        peakValueList.Add(scope_max_value);
                                        peakIndexList.Add(scope_max_idx);
                                    }

                                    //if (peakIndexList.Last() != min_idx)
                                    {
                                        peakValueList.Add(min_value);
                                        peakIndexList.Add(min_idx);
                                    }
                                    isLastMax = false;
                                }
                            }
                            else
                            {
                                // 前回はmin max の順

                                if (peakValueList.Last() < max_value)
                                {
                                    // 前回maxより今回maxの方が大きい
                                    // 前回maxを今回maxに入れ替え
                                    peakValueList[peakValueList.Count() - 1] = max_value;
                                    peakIndexList[peakIndexList.Count() - 1] = max_idx;
                                }
                                else if (peakValueList.Last() > max_value)
                                {
                                    // 前回maxは今回maxより大きい
                                    // 今回maxは入れない
                                }

                                //if (peakIndexList.Last() != min_idx)
                                {
                                    peakValueList.Add(min_value);
                                    peakIndexList.Add(min_idx);
                                }
                                isLastMax = false;
                            }
                        }
                    }
                }

                Console.WriteLine("#Peak");
                for (int i = back_idx; i <= cur_idx; i++)
                {
                    Candlestick candle = m_candleList[i];
                    if (candle == null)
                    {
                        continue;
                    }
                    double high = (candle.high - peakValueList.Min()) / (peakValueList.Max() - peakValueList.Min());
                    double open = (candle.open - peakValueList.Min()) / (peakValueList.Max() - peakValueList.Min());
                    double last = (candle.last - peakValueList.Min()) / (peakValueList.Max() - peakValueList.Min());
                    double low = (candle.low - peakValueList.Min()) / (peakValueList.Max() - peakValueList.Min());
                    int peakIndex = peakIndexList.IndexOf(i);

                    if(minIndexList.Contains(i))
                    {
                        if (peakIndex >= 0)
                        {
                            double peak = (peakValueList[peakIndex] - peakValueList.Min()) / (peakValueList.Max() - peakValueList.Min());
                            Console.WriteLine("{0},'{1},{2},{3},,{4},{5}", i, candle.timestamp, high, low, low, peak);
                        }
                        else
                        {
                            Console.WriteLine("{0},'{1},{2},{3},,{4},", i, candle.timestamp, high, low, low);
                        }
                    }
                    else if(maxIndexList.Contains(i))
                    {
                        if (peakIndex >= 0)
                        {
                            double peak = (peakValueList[peakIndex] - peakValueList.Min()) / (peakValueList.Max() - peakValueList.Min());
                            Console.WriteLine("{0},'{1},{2},{3},{4},,{5}", i, candle.timestamp, high, low, high, peak);
                        }
                        else
                        {
                            Console.WriteLine("{0},'{1},{2},{3},{4},,", i, candle.timestamp, high, low, high);
                        }
                    }
                    else
                    {
                        if (peakIndex >= 0)
                        {
                            double peak = (peakValueList[peakIndex] - peakValueList.Min()) / (peakValueList.Max() - peakValueList.Min());
                            Console.WriteLine("{0},'{1},{2},{3},,,{4}", i, candle.timestamp, high, low, peak);
                        }
                        else
                        {
                            Console.WriteLine("{0},'{1},{2},{3},,,", i, candle.timestamp, high, low);
                        }
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
                if (result != 0)
                {
                    if (peakValueList != null)
                    {
                        peakValueList.Clear();
                    }

                    if (peakIndexList != null)
                    {
                        peakIndexList.Clear();
                    }
                }
            }
            return result;
        }

        public int calcPeakList
        (
            ref List<double> peakMinValueList,
            ref List<double> peakMaxValueList,
            ref List<int> peakMinIndexList,
            ref List<int> peakMaxIndexList,
            int cur_idx,
            int back_idx,
            int stride_num = 5
        )
        {
            int result = -1;

            try
            {
                if (peakMinValueList == null)
                {
                    return result;
                }
                peakMinValueList.Clear();

                if (peakMaxValueList == null)
                {
                    return result;
                }
                peakMaxValueList.Clear();

                if (peakMinIndexList == null)
                {
                    return result;
                }
                peakMinIndexList.Clear();

                if (peakMaxIndexList == null)
                {
                    return result;
                }
                peakMaxIndexList.Clear();

                SortedSet<int> minIndexList = new SortedSet<int>();
                SortedSet<int> maxIndexList = new SortedSet<int>();
                if (calcMinMaxList(ref minIndexList, ref maxIndexList, cur_idx, back_idx, stride_num) != 0)
                {
                    return result;
                }

                bool isLastMax = false;
                bool isFirst = true;
                for (int i = 0; i < maxIndexList.Count(); i++)
                {
                    int min_idx = minIndexList.ElementAt(i);
                    int max_idx = maxIndexList.ElementAt(i);

                    Candlestick minCandle = m_candleList[min_idx];
                    if (minCandle == null)
                    {
                        continue;
                    }

                    Candlestick maxCandle = m_candleList[max_idx];
                    if (maxCandle == null)
                    {
                        continue;
                    }

                    double min_value = minCandle.low;
                    double max_value = maxCandle.high;

                    if (min_idx < max_idx)
                    {
                        // min max の順で格納
                        if (isFirst)
                        {
                            peakMinValueList.Add(min_value);
                            peakMaxValueList.Add(max_value);
                            isLastMax = true;
                            isFirst = false;
                            peakMinIndexList.Add(min_idx);
                            peakMaxIndexList.Add(max_idx);
                        }
                        else
                        {
                            if (isLastMax)
                            {
                                // 前回もmin max の順

                                if (peakMaxValueList.Last() < min_value)
                                {
                                    // 前回maxの方が今回minより小さい

                                    // 今回のminは入れない
                                    // 前回maxを今回maxに入れ替え
                                    peakMaxValueList[peakMaxValueList.Count() - 1] = max_value;
                                    peakMaxIndexList[peakMaxIndexList.Count() - 1] = max_idx;
                                    isLastMax = true;
                                }
                                else if (peakMaxValueList.Last() > min_value)
                                {
                                    // 前回maxの方が今回minより大きい

                                    // 今回minから前回maxの間で最も小さい位置を算出
                                    double scope_min_value = min_value;
                                    double scope_max_value = 0.0;
                                    int scope_min_idx = min_idx;
                                    int scope_max_idx = -1;
                                    int last_idx = peakMaxIndexList.Last();// +1;
                                    if (min_idx > last_idx)
                                    {
                                        if (getMinMaxCandle(out scope_min_value, out scope_max_value, out scope_min_idx, out scope_max_idx, min_idx, last_idx) != 0)
                                        {
                                            return result;
                                        }
                                    }

                                    // 同じ順序で格納
                                    peakMinValueList.Add(scope_min_value);
                                    peakMinIndexList.Add(scope_min_idx);

                                    peakMaxValueList.Add(max_value);
                                    peakMaxIndexList.Add(max_idx);
                                    isLastMax = true;
                                }
                            }
                            else
                            {
                                // 前回はmax min の順

                                if (peakMinValueList.Last() > min_value)
                                {
                                    // 前回minより今回minの方が小さい
                                    // 前回minを今回minに入れ替え
                                    peakMinValueList[peakMinValueList.Count() - 1] = min_value;
                                    peakMinIndexList[peakMinIndexList.Count() - 1] = min_idx;
                                }
                                else if (peakMinValueList.Last() < min_value)
                                {
                                    // 前回minは今回minより小さい
                                    // 今回minは入れない
                                }

                                peakMaxValueList.Add(max_value);
                                peakMaxIndexList.Add(max_idx);
                                isLastMax = true;
                            }
                        }
                    }
                    else
                    {
                        // max min の順で格納
                        if (isFirst)
                        {
                            peakMaxValueList.Add(max_value);
                            peakMinValueList.Add(min_value);
                            isLastMax = false;
                            isFirst = false;
                            peakMaxIndexList.Add(max_idx);
                            peakMinIndexList.Add(min_idx);
                        }
                        else
                        {
                            if (!isLastMax)
                            {
                                // 前回もmax min の順

                                if (peakMinValueList.Last() > max_value)
                                {
                                    // 前回minの方が今回maxより大きい

                                    // 今回のmaxは入れない
                                    // 前回minの今回のminに入れ替え
                                    peakMinValueList[peakMinValueList.Count() - 1] = min_value;
                                    peakMinIndexList[peakMinIndexList.Count() - 1] = min_idx;
                                    isLastMax = false;
                                }
                                else if (peakMinValueList.Last() < max_value)
                                {
                                    // 前回minの方が今回maxより小さい

                                    // 今回maxから前回minの間で最も小さい位置を算出
                                    double scope_min_value = 0.0;
                                    double scope_max_value = max_value;
                                    int scope_min_idx = -1;
                                    int scope_max_idx = max_idx;
                                    int last_idx = peakMinIndexList.Last();// +1;
                                    if (max_idx > last_idx)
                                    {
                                        if (getMinMaxCandle(out scope_min_value, out scope_max_value, out scope_min_idx, out scope_max_idx, max_idx, last_idx) != 0)
                                        {
                                            return result;
                                        }
                                    }

                                    // 同じ順序で格納
                                    peakMaxValueList.Add(scope_max_value);
                                    peakMaxIndexList.Add(scope_max_idx);

                                    peakMinValueList.Add(min_value);
                                    peakMinIndexList.Add(min_idx);
                                    isLastMax = false;
                                }
                            }
                            else
                            {
                                // 前回はmin max の順

                                if (peakMaxValueList.Last() < max_value)
                                {
                                    // 前回maxより今回maxの方が大きい
                                    // 前回maxを今回maxに入れ替え
                                    peakMaxValueList[peakMaxValueList.Count() - 1] = max_value;
                                    peakMaxIndexList[peakMaxIndexList.Count() - 1] = max_idx;
                                }
                                else if (peakMaxValueList.Last() > max_value)
                                {
                                    // 前回maxは今回maxより大きい
                                    // 今回maxは入れない
                                }

                                peakMinValueList.Add(min_value);
                                peakMinIndexList.Add(min_idx);
                                isLastMax = false;
                            }
                        }
                    }
                }

                if (peakMinValueList.Count() > 0)
                {
                    if (peakMinIndexList.Last() == cur_idx)
                    {
                        peakMinIndexList.RemoveAt(peakMinIndexList.Count() - 1);
                        peakMinValueList.RemoveAt(peakMinValueList.Count() - 1);
                    }
                }

                if (peakMaxValueList.Count() > 0)
                {
                    if (peakMaxIndexList.Last() == cur_idx)
                    {
                        peakMaxIndexList.RemoveAt(peakMaxIndexList.Count() - 1);
                        peakMaxValueList.RemoveAt(peakMaxValueList.Count() - 1);
                    }
                }

                //Console.WriteLine("#Peak");
                //for (int i = back_idx; i <= cur_idx; i++)
                //{
                //    Candlestick candle = m_candleList[i];
                //    if (candle == null)
                //    {
                //        continue;
                //    }
                //    double high = (candle.high - peakMinValueList.Min()) / (peakMaxValueList.Max() - peakMinValueList.Min());
                //    double open = (candle.open - peakMinValueList.Min()) / (peakMaxValueList.Max() - peakMinValueList.Min());
                //    double last = (candle.last - peakMinValueList.Min()) / (peakMaxValueList.Max() - peakMinValueList.Min());
                //    double low = (candle.low - peakMinValueList.Min()) / (peakMaxValueList.Max() - peakMinValueList.Min());
                //    int peakMinIndex = peakMinIndexList.IndexOf(i);
                //    int peakMaxIndex = peakMaxIndexList.IndexOf(i);

                //    if (peakMinIndex >= 0 && peakMaxIndex>=0)
                //    {
                //        double peakMin = (peakMinValueList[peakMinIndex] - peakMinValueList.Min()) / (peakMaxValueList.Max() - peakMinValueList.Min());
                //        double peakMax = (peakMaxValueList[peakMaxIndex] - peakMinValueList.Min()) / (peakMaxValueList.Max() - peakMinValueList.Min());
                //        Console.WriteLine("{0},'{1},{2},{3},{4},{5}", i, candle.timestamp, high, low, peakMin, peakMax);
                //    }
                //    else if(peakMinIndex >= 0)
                //    {
                //        double peakMin = (peakMinValueList[peakMinIndex] - peakMinValueList.Min()) / (peakMaxValueList.Max() - peakMinValueList.Min());
                //        Console.WriteLine("{0},'{1},{2},{3},{4},", i, candle.timestamp, high, low, peakMin);
                //    }
                //    else if(peakMaxIndex >= 0)
                //    {
                //        double peakMax = (peakMaxValueList[peakMaxIndex] - peakMinValueList.Min()) / (peakMaxValueList.Max() - peakMinValueList.Min());
                //        Console.WriteLine("{0},'{1},{2},{3},,{4}", i, candle.timestamp, high, low, peakMax);
                //    }
                //    else
                //    {
                //        Console.WriteLine("{0},'{1},{2},{3},,", i, candle.timestamp, high, low);
                //    }
                //}
                result = 0;
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
                    if (peakMinValueList != null)
                    {
                        peakMinValueList.Clear();
                    }

                    if (peakMinIndexList != null)
                    {
                        peakMinIndexList.Clear();
                    }

                    if (peakMaxValueList != null)
                    {
                        peakMaxValueList.Clear();
                    }

                    if (peakMaxIndexList != null)
                    {
                        peakMaxIndexList.Clear();
                    }
                }
            }
            return result;
        }

        public int calcBigTrendTilt
        (
            ref List<double> tiltList,
            out double basePeakValue,
            out int basePeakIdx,
            List<double> peakValueList,
            List<int> peakIndexList,
            int periods,
            bool isTop
        )
        {
            int result = -1;


            basePeakValue = 0.0;
            basePeakIdx = -1;

            try
            {
                if(tiltList==null)
                {
                    return result;
                }

                if (peakValueList == null)
                {
                    return result;
                }

                if (peakValueList == null)
                {
                    return result;
                }


                int mostIdx = -1;
                if (isTop)
                {
                    mostIdx = peakValueList.IndexOf(peakValueList.Max());
                }
                else
                {
                    mostIdx = peakValueList.IndexOf(peakValueList.Min());
                }

                if (mostIdx < 0)
                {
                    return result;
                }

                int mostPeakIdx = peakIndexList[mostIdx];
                double mostPeakValue = peakValueList[mostIdx];

                Candlestick mostPeakCandle = getCandle(mostPeakIdx);
                if (mostPeakCandle == null)
                {
                    return result;
                }

                DateTime mostPeakTime = DateTime.Parse(mostPeakCandle.timestamp);

                int fwdIdx = -1;
                double fwdTilt = 0.0;
                for (int i = mostIdx + 1; i < peakIndexList.Count(); i++)
                {
                    int nextPeakIdx = peakIndexList[i];
                    double nextPeakValue = peakValueList[i];

                    int nextElapsed_sec = (nextPeakIdx - mostPeakIdx) * periods;
                    double nextTilt = (nextPeakValue - mostPeakValue) / (double)(nextElapsed_sec);

                    bool isBreak = false;
                    for (int j = i + 1; j < peakIndexList.Count(); j++)
                    {
                        int subPeakIdx = peakIndexList[j];
                        double subPeakValue = peakValueList[j];
                        Candlestick subCandle = getCandle(subPeakIdx);
                        if (subCandle == null)
                        {
                            return result;
                        }

                        DateTime subPeakTime = DateTime.Parse(subCandle.timestamp);
                        TimeSpan subPeakSpan = subPeakTime - mostPeakTime;
                        int subElapsed_sec = (int)(subPeakSpan.TotalSeconds);

                        double trendValue = nextTilt * subElapsed_sec + mostPeakValue;
                        if(isTop)
                        {
                            if ((trendValue - subPeakValue) < 0.0)
                            {
                                // 割っている
                                isBreak = true;
                                break;
                            }
                        }
                        else
                        {
                            if ((subPeakValue - trendValue) < 0.0)
                            {
                                // 割っている
                                isBreak = true;
                                break;
                            }
                        }

                    }

                    if (!isBreak)
                    {
                        // 以降は1つも割っていない
                        fwdIdx = i;
                        fwdTilt = nextTilt;
                        break;
                    }
                }

                int bwdIdx = -1;
                double bwdTilt = 0.0;
                for (int i = mostIdx - 1; i >= 0; i--)
                {
                    int nextPeakIdx = peakIndexList[i];
                    double nextPeakValue = peakValueList[i];

                    int nextElapsed_sec = (mostPeakIdx - nextPeakIdx) * periods;
                    double nextTilt = (mostPeakValue - nextPeakValue) / (double)(nextElapsed_sec);

                    bool isBreak = false;
                    for (int j = i - 1; j >= 0; j--)
                    {
                        int subPeakIdx = peakIndexList[j];
                        double subPeakValue = peakValueList[j];
                        Candlestick subCandle = getCandle(subPeakIdx);
                        if (subCandle == null)
                        {
                            return result;
                        }

                        DateTime subPeakTime = DateTime.Parse(subCandle.timestamp);
                        TimeSpan subPeakSpan = subPeakTime - mostPeakTime;
                        int subElapsed_sec = (int)(subPeakSpan.TotalSeconds);
                        double trendValue = nextTilt * subElapsed_sec + mostPeakValue;

                        if (isTop)
                        {
                            if ((trendValue - subPeakValue) < 0.0)
                            {
                                // 割っている
                                isBreak = true;
                                break;
                            }
                        }
                        else
                        {
                            if ((subPeakValue - trendValue) < 0.0)
                            {
                                // 割っている
                                isBreak = true;
                                break;
                            }
                        }

                    }

                    if (!isBreak)
                    {
                        // 以降は1つも割っていない
                        bwdIdx = i;
                        bwdTilt = nextTilt;
                        break;
                    }
                }


                if (fwdIdx<0 && bwdIdx<0)
                {
                    return result;
                }

                if (fwdIdx >= 0)
                {
                    tiltList.Add(fwdTilt);
                }

                if (bwdIdx >= 0)
                {
                    tiltList.Add(bwdTilt);
                }

                basePeakValue = mostPeakValue;
                basePeakIdx = mostPeakIdx;

                result = 0;
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
                    tiltList.Clear();
                    basePeakValue = 0.0;
                    basePeakIdx = -1;
                }
            }
            return result;
        }


        public int calcBreakBigTrendLine
        (
            out bool isBreakTop,
            out bool isBreakBottom,
            Candlestick curCandle,
            List<double> peakMinValueList,
            List<double> peakMaxValueList,
            List<int> peakMinIndexList,
            List<int> peakMaxIndexList,
            int periods
        )
        {
            int result = -1;

            isBreakTop = false;
            isBreakBottom = false;

            try
            {
                if (curCandle == null)
                {
                    return result;
                }

                if (peakMinValueList == null)
                {
                    return result;
                }

                if (peakMaxValueList == null)
                {
                    return result;
                }

                if (peakMinIndexList == null)
                {
                    return result;
                }

                if (peakMaxIndexList == null)
                {
                    return result;
                }

                List<double> topTiltList = new List<double>();
                double topPeakValue = 0.0;
                int topPeakIdx = -1;
                if (calcBigTrendTilt(ref topTiltList, out topPeakValue, out topPeakIdx, peakMaxValueList, peakMaxIndexList, periods, true)!=0)
                {
                    return result;
                }
                
                List<double> bottomTiltList = new List<double>();
                double bottomPeakValue = 0.0;
                int bottomPeakIdx = -1;
                if (calcBigTrendTilt(ref bottomTiltList, out bottomPeakValue, out bottomPeakIdx, peakMinValueList, peakMinIndexList, periods, false) != 0)
                {
                    return result;
                }

                DateTime curTime = DateTime.Parse(curCandle.timestamp);
                {
                    Candlestick topCandle = getCandle(topPeakIdx);
                    if (topCandle == null)
                    {
                        return result;
                    }
                    DateTime topTime = DateTime.Parse(topCandle.timestamp);
                    TimeSpan topSpan = curTime - topTime;
                    int topElapsed_sec = (int)(topSpan.TotalSeconds);
                    if(topElapsed_sec==0)
                    {
                        Console.WriteLine();
                    }

                    foreach (double topTilt in topTiltList)
                    {
                        double topTrendValue = topTilt * topElapsed_sec + topPeakValue;
                        //double diff = topTrendValue - curCandle.last;
                        double diff = topTrendValue - curCandle.low;
                        if (diff<0.0)
                        {
                            isBreakTop = true;
                            break;
                        }
                    }
                }
                {
                    Candlestick bottomCandle = getCandle(bottomPeakIdx);
                    if (bottomCandle == null)
                    {
                        return result;
                    }
                    DateTime bottomTime = DateTime.Parse(bottomCandle.timestamp);
                    TimeSpan bottomSpan = curTime - bottomTime;
                    int bottomElapsed_sec = (int)(bottomSpan.TotalSeconds);
                    if (bottomElapsed_sec == 0)
                    {
                        Console.WriteLine();
                    }

                    foreach (double bottomTilt in bottomTiltList)
                    {
                        double bottomTrendValue = bottomTilt * bottomElapsed_sec + bottomPeakValue;
                        //double diff = curCandle.last - bottomTrendValue;
                        double diff = curCandle.high - bottomTrendValue;
                        if (diff<0.0)
                        {
                            isBreakBottom = true;
                            break;
                        }
                    }
                }

                //bool isTouchedTop = false;
                //bool isTouchedBottom = false;
                //{
                //    if (curCandle.timestamp == "2021/01/26 23:50:00")
                //    {
                //        Console.WriteLine();
                //    }

                //    int touchedTopIdx = -1;
                //    int touchedBtmIdx = -1;
                //    //foreach (Candlestick candle in m_candleList.Reverse<Candlestick>() )
                //    for (int i = (m_candleList.Count - 1); i >= 0; i--)
                //    {
                //        Candlestick candle = m_candleList[i];
                //        if (candle == null)
                //        {
                //            continue;
                //        }

                //        DateTime time = DateTime.Parse(candle.timestamp);
                //        TimeSpan span = curTime - time;
                //        int elapsed_sec = (int)(span.TotalSeconds);

                //        if (!isTouchedTop)
                //        {
                //            foreach (double topTilt in topTiltList)
                //            {
                //                double topTrendValue = topTilt * elapsed_sec + topPeakValue;
                //                double diff = topTrendValue - candle.high;
                //                if (diff < 0.0)
                //                {
                //                    touchedTopIdx = i;
                //                    break;
                //                }
                //            }
                //        }

                //        if (!isTouchedBottom)
                //        {
                //            foreach (double bottomTilt in bottomTiltList)
                //            {
                //                double bottomTrendValue = bottomTilt * elapsed_sec + bottomPeakValue;
                //                double diff = candle.low - bottomTrendValue;
                //                if (diff < 0.0)
                //                {
                //                    touchedBtmIdx = i;
                //                    break;
                //                }
                //            }
                //        }
                //    }
                //    if(touchedTopIdx>=0)
                //    {
                //        int candle_cnt = (m_candleList.Count - 1) - touchedTopIdx;
                //        if (candle_cnt < 10)
                //        {
                //            isTouchedTop = true;
                //        }
                //    }

                //    if (touchedBtmIdx >= 0)
                //    {
                //        int candle_cnt = (m_candleList.Count - 1) - touchedBtmIdx;
                //        if (candle_cnt < 10)
                //        {
                //            isTouchedBottom = true;
                //        }
                //    }
                //}

                result = 0;
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
                    isBreakTop = false;
                    isBreakBottom = false;
                }
            }
            return result;
        }


        public int printPeakList
        (
            List<double> peakMinValueList,
            List<double> peakMaxValueList,
            List<int> peakMinIndexList,
            List<int> peakMaxIndexList,
            int cur_idx,
            int back_idx
        )
        {
            int result = -1;

            try
            {
                if (peakMinValueList == null)
                {
                    return result;
                }

                if (peakMaxValueList == null)
                {
                    return result;
                }

                if (peakMinIndexList == null)
                {
                    return result;
                }

                if (peakMaxIndexList == null)
                {
                    return result;
                }

                Console.WriteLine("#Peak");
                for (int i = back_idx; i <= cur_idx; i++)
                {
                    Candlestick candle = m_candleList[i];
                    if (candle == null)
                    {
                        continue;
                    }
                    double high = (candle.high - peakMinValueList.Min()) / (peakMaxValueList.Max() - peakMinValueList.Min());
                    double open = (candle.open - peakMinValueList.Min()) / (peakMaxValueList.Max() - peakMinValueList.Min());
                    double last = (candle.last - peakMinValueList.Min()) / (peakMaxValueList.Max() - peakMinValueList.Min());
                    double low = (candle.low - peakMinValueList.Min()) / (peakMaxValueList.Max() - peakMinValueList.Min());
                    int peakMinIndex = peakMinIndexList.IndexOf(i);
                    int peakMaxIndex = peakMaxIndexList.IndexOf(i);

                    if (peakMinIndex >= 0 && peakMaxIndex >= 0)
                    {
                        double peakMin = (peakMinValueList[peakMinIndex] - peakMinValueList.Min()) / (peakMaxValueList.Max() - peakMinValueList.Min());
                        double peakMax = (peakMaxValueList[peakMaxIndex] - peakMinValueList.Min()) / (peakMaxValueList.Max() - peakMinValueList.Min());
                        Console.WriteLine("{0},'{1},{2},{3},{4},{5}", i, candle.timestamp, high, low, peakMin, peakMax);
                    }
                    else if (peakMinIndex >= 0)
                    {
                        double peakMin = (peakMinValueList[peakMinIndex] - peakMinValueList.Min()) / (peakMaxValueList.Max() - peakMinValueList.Min());
                        Console.WriteLine("{0},'{1},{2},{3},{4},", i, candle.timestamp, high, low, peakMin);
                    }
                    else if (peakMaxIndex >= 0)
                    {
                        double peakMax = (peakMaxValueList[peakMaxIndex] - peakMinValueList.Min()) / (peakMaxValueList.Max() - peakMinValueList.Min());
                        Console.WriteLine("{0},'{1},{2},{3},,{4}", i, candle.timestamp, high, low, peakMax);
                    }
                    else
                    {
                        Console.WriteLine("{0},'{1},{2},{3},,", i, candle.timestamp, high, low);
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
                if (result != 0)
                {
                }
            }
            return result;
        }



        public int calcZigzagList
        (
            ref List<double> peakValueList,
            ref List<int> peakIndexList,
            int cur_idx,
            int back_idx,
            double deviation // 反転率
        )
        {
            int result = -1;

            try
            {
                if (peakValueList == null)
                {
                    return result;
                }
                peakValueList.Clear();

                if (peakIndexList == null)
                {
                    return result;
                }
                peakIndexList.Clear();

                double value_min = 0.0;
                double value_max = 0.0;
                int value_min_idx = -1;
                int value_max_idx = -1;
                if (getMinMaxCandle(out value_min, out value_max, out value_min_idx, out value_max_idx, cur_idx, back_idx) != 0)
                {
                    return result;
                }


                double last_low = double.MaxValue;
                double last_high = double.MinValue;

                int peak_idx = back_idx;
                int last_low_idx = -1;
                int last_high_idx = -1;

                bool isUp = true;
                bool isFirst = true;
                for (int i = back_idx; i <= cur_idx; i++)
                {
                    Candlestick candle = m_candleList[i];
                    if (candle == null)
                    {
                        continue;
                    }

                    double low = (candle.low - value_min) / (value_max - value_min);
                    double high = (candle.high - value_min) / (value_max - value_min);

                    // 暫定最安値を更新
                    if (last_low >= low)
                    {
                        last_low_idx = i;
                        last_low = low;
                    }

                    // 暫定最高値を更新
                    if (last_high <= high)
                    {
                        last_high_idx = i;
                        last_high = high;
                    }

                    Candlestick peakCandle = m_candleList[peak_idx];
                    if (peakCandle == null)
                    {
                        return result;
                    }
                    double peak_low = (peakCandle.low - value_min) / (value_max - value_min);
                    double peak_high = (peakCandle.high - value_min) / (value_max - value_min);

                    if (isFirst)
                    {
                        if (peak_idx != last_high_idx && peak_low < last_high)
                        {
                            isUp = true;
                            isFirst = false;
                        }
                        else if (peak_idx!= last_low_idx && peak_high > last_low)
                        {
                            isUp = false;
                            isFirst = false;
                        }
                    }

                    if(isFirst)
                    {
                        continue;
                    }

                    if(isUp)
                    {
                        // 上昇中
                        // 折り返し度算出
                        double down_rate = 0.0;
                        if (peak_idx < last_high_idx && last_high_idx < i)
                        {
                            down_rate = (last_high - low) / (last_high - peak_low);
                        }

                        if (down_rate >= deviation && last_high > peak_low)
                        {
                            // 下に折り返した場合
                            // 頂点の更新
                            if (peak_idx != last_high_idx)
                            {
                                Candlestick highCandle = m_candleList[last_high_idx];
                                if (highCandle == null)
                                {
                                    continue;
                                }

                                // Update
                                peakIndexList.Add(last_high_idx);
                                peakValueList.Add(highCandle.high);
                                peak_idx = last_high_idx;

                                // Reset
                                last_low_idx = -1;
                                last_low = double.MaxValue;
                                last_high_idx = -1;
                                last_high = double.MinValue;

                                isUp = false;
                            }
                        }
                    }
                    else
                    {
                        // 下降中
                        // 折り返し度算出
                        double up_rate = 0.0;
                        if (peak_idx < last_low_idx && last_low_idx < i)
                        {
                            up_rate = (high - last_low) / (peak_high - last_low);
                        }

                        if (up_rate >= deviation && peak_high > last_low)
                        {
                            // 上に折り返した場合
                            // 頂点の更新
                            if (peak_idx != last_low_idx)
                            {
                                Candlestick lowCandle = m_candleList[last_low_idx];
                                if (lowCandle == null)
                                {
                                    continue;
                                }

                                // Update
                                peakIndexList.Add(last_low_idx);
                                peakValueList.Add(lowCandle.low);
                                peak_idx = last_low_idx;

                                // Reset
                                last_low_idx = -1;
                                last_low = double.MaxValue;
                                last_high_idx = -1;
                                last_high = double.MinValue;

                                isUp = true;
                            }
                        }
                    }
                }

                Console.WriteLine("#Zigzag");
                for (int i = back_idx; i <= cur_idx; i++)
                {
                    Candlestick candle = m_candleList[i];
                    if (candle == null)
                    {
                        continue;
                    }
                    double high = (candle.high - value_min) / (value_max - value_min);
                    double open = (candle.open - value_min) / (value_max - value_min);
                    double last = (candle.last - value_min) / (value_max - value_min);
                    double low = (candle.low - value_min) / (value_max - value_min);
                    //double high = (candle.high - peakValueList.Min()) / (peakValueList.Max() - peakValueList.Min());
                    //double open = (candle.open - peakValueList.Min()) / (peakValueList.Max() - peakValueList.Min());
                    //double last = (candle.last - peakValueList.Min()) / (peakValueList.Max() - peakValueList.Min());
                    //double low = (candle.low - peakValueList.Min()) / (peakValueList.Max() - peakValueList.Min());
                    int peakIndex = peakIndexList.IndexOf(i);
                    if (peakIndex >= 0)
                    {
                        //double peak = (peakValueList[peakIndex] - peakValueList.Min()) / (peakValueList.Max() - peakValueList.Min());
                        double peak = (peakValueList[peakIndex] - value_min) / (value_max - value_min);
                        Console.WriteLine("{0},'{1},{2},{3},{4}", i, candle.timestamp, high, low, peak);
                    }
                    else
                    {
                        Console.WriteLine("{0},'{1},{2},{3},", i, candle.timestamp, high, low);
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
                if (result != 0)
                {
                    if (peakValueList != null)
                    {
                        peakValueList.Clear();
                    }

                    if (peakIndexList != null)
                    {
                        peakIndexList.Clear();
                    }
                }
            }
            return result;
        }

        public int calcZigzagList
        (
            ref List<double> peakValueList,
            ref List<int> peakIndexList,
            int cur_idx,
            int back_idx,
            double depth,    // 参照する値幅の上限値
            int backstep,    // 参照するローソク本数
            double deviation // 反転率
        )
        {
            int result = -1;

            try
            {
                if (peakValueList == null)
                {
                    return result;
                }
                peakValueList.Clear();

                if (peakIndexList == null)
                {
                    return result;
                }
                peakIndexList.Clear();

                double value_min = 0.0;
                double value_max = 0.0;
                int value_min_idx = -1;
                int value_max_idx = -1;
                if (getMinMaxCandle(out value_min, out value_max, out value_min_idx, out value_max_idx, cur_idx, back_idx) != 0)
                {
                    return result;
                }


                double candle_min = 0.0;
                double candle_max = 0.0;
                if (getMinMaxCandleValue(out candle_min, out candle_max) != 0)
                {
                    return result;
                }

                int peak_idx = back_idx;
                Candlestick begCandle = m_candleList[back_idx];
                if (begCandle == null)
                {
                    return result;
                }

                int last_low_idx = back_idx + (backstep-1);
                int last_high_idx = last_low_idx;
                Candlestick lastCandle = m_candleList[last_low_idx];
                if (lastCandle == null)
                {
                    return result;
                }
                double last_low = (lastCandle.low - value_min) / (value_max - value_min);
                double last_high = (lastCandle.high - value_min) / (value_max - value_min);


                for (int i = last_low_idx + 1; i <= cur_idx; i++)
                {
                    Candlestick candle = m_candleList[i];
                    if (candle == null)
                    {
                        continue;
                    }

                    double low = (candle.low - value_min) / (value_max - value_min);
                    double high = (candle.high - value_min) / (value_max - value_min);

                    // 暫定最安値を更新
                    if (last_low >= low)
                    {
                        last_low_idx = i;
                        last_low = low;
                    }

                    // 暫定最高値を更新
                    if (last_high <= high)
                    {
                        last_high_idx = i;
                        last_high = high;
                    }

                    if ((i - peak_idx) < backstep)
                    {
                        // 頂点からローソク数がbackstep本以上離れていない場合
                        // SKIP
                        continue;
                    }

                    Candlestick peakCandle = m_candleList[peak_idx];
                    if (peakCandle == null)
                    {
                        return result;
                    }
                    double peak_low = (peakCandle.low - value_min) / (value_max - value_min);
                    double peak_high = (peakCandle.high - value_min) / (value_max - value_min);

                    if (Math.Abs(low - peak_high) < depth && Math.Abs(high - peak_low) < depth)
                    {
                        // 頂点から価格がdepth以上離れていない場合
                        // SKIP
                        continue;
                    }

                    //// 暫定最安値を更新
                    //if (last_low >= low)
                    //{
                    //    last_low_idx = i;
                    //    last_low = low;
                    //}

                    //// 暫定最高値を更新
                    //if (last_high <= high)
                    //{
                    //    last_high_idx = i;
                    //    last_high = high;
                    //}

                    // 折り返し度算出
                    double up_rate = (high - last_low) / (peak_high - last_low);
                    double down_rate = (last_high - low) / (last_high - peak_low);

                    if (up_rate >= deviation && up_rate > down_rate)
                    {
                        // 上に折り返した場合
                        // 頂点の更新
                        if (peak_idx != last_low_idx)
                        {
                            Candlestick lowCandle = m_candleList[last_low_idx];
                            if (lowCandle == null)
                            {
                                continue;
                            }

                            // Update
                            peakIndexList.Add(last_low_idx);
                            peakValueList.Add(lowCandle.low);
                            peak_idx = last_low_idx;

                            // Reset
                            last_low_idx = -1;
                            last_low = double.MaxValue;
                            last_high_idx = -1;
                            last_high = double.MinValue;
                        }
                    }

                    if (down_rate >= deviation && down_rate > up_rate)
                    {
                        // 下に折り返した場合
                        // 頂点の更新
                        if (peak_idx != last_high_idx)
                        {
                            Candlestick highCandle = m_candleList[last_high_idx];
                            if (highCandle == null)
                            {
                                continue;
                            }

                            // Update
                            peakIndexList.Add(last_high_idx);
                            peakValueList.Add(highCandle.high);
                            peak_idx = last_high_idx;

                            // Reset
                            last_low_idx = -1;
                            last_low = double.MaxValue;
                            last_high_idx = -1;
                            last_high = double.MinValue;
                        }
                    }
                }

                Console.WriteLine("#Zigzag");
                for (int i = back_idx; i <= cur_idx; i++)
                {
                    Candlestick candle = m_candleList[i];
                    if (candle == null)
                    {
                        continue;
                    }
                    double high = (candle.high - value_min) / (value_max - value_min);
                    double open = (candle.open - value_min) / (value_max - value_min);
                    double last = (candle.last - value_min) / (value_max - value_min);
                    double low = (candle.low - value_min) / (value_max - value_min);
                    //double high = (candle.high - peakValueList.Min()) / (peakValueList.Max() - peakValueList.Min());
                    //double open = (candle.open - peakValueList.Min()) / (peakValueList.Max() - peakValueList.Min());
                    //double last = (candle.last - peakValueList.Min()) / (peakValueList.Max() - peakValueList.Min());
                    //double low = (candle.low - peakValueList.Min()) / (peakValueList.Max() - peakValueList.Min());
                    int peakIndex = peakIndexList.IndexOf(i);
                    if(peakIndex>=0)
                    {
                        //double peak = (peakValueList[peakIndex] - peakValueList.Min()) / (peakValueList.Max() - peakValueList.Min());
                        double peak = (peakValueList[peakIndex] - value_min) / (value_max - value_min);
                        Console.WriteLine("{0},'{1},{2},{3},{4}", i, candle.timestamp, high, low, peak);
                    }
                    else
                    {
                        Console.WriteLine("{0},'{1},{2},{3},", i, candle.timestamp, high, low);
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
                if (result != 0)
                {
                    if (peakValueList != null)
                    {
                        peakValueList.Clear();
                    }

                    if (peakIndexList != null)
                    {
                        peakIndexList.Clear();
                    }
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

        public int getNearEmaCandle(out Candlestick reverceCandle, bool entry_side, string entry_date, double boll_div = 8.0)
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

        public bool isPassBBtoMATop(out int band_pos)
        {
            bool result = false;
            band_pos = 0;
            try
            {
                string outside_stamp = "";
                string cross_stamp = "";
                int back_cnt = 0;
                int matop_cross_cnt = 0;

                if (searchLastOutsideBB(out band_pos, out outside_stamp, out cross_stamp, out back_cnt, out matop_cross_cnt) != 0)
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
                string outside_stamp = "";
                string cross_stamp = "";
                int back_cnt = 0;
                int matop_cross_cnt = 0;

                if (searchLastOutsideBB(out band_pos, out outside_stamp, out cross_stamp, out back_cnt, out matop_cross_cnt) != 0)
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

		public bool isFullUpPast(double price, int past_num, int past_pos = 0)
        {
            bool result = false;
            try
            {
                int candle_cnt = getCandleCount();
                if (candle_cnt > past_num)
                {
					int last_index = (candle_cnt - 1) - past_pos;
                    int past_index = last_index - past_num;
                    result = true;
                    for (int i = last_index; i > past_index; i--)
                    {
                        Candlestick candle = m_candleList[i];
                        if (candle == null)
                        {
                            continue;
                        }

                        if (candle.last <= price)
                        {
                            result = false;
                            break;
                        }
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

		public bool isFullDownPast(double price, int past_num, int past_pos = 0)
        {
            bool result = false;
            try
            {
                int candle_cnt = getCandleCount();
                if (candle_cnt > past_num)
                {
					int last_index = (candle_cnt - 1) - past_pos;
                    int past_index = last_index - past_num;
                    result = true;
                    for (int i = last_index; i > past_index; i--)
                    {
                        Candlestick candle = m_candleList[i];
                        if (candle == null)
                        {
                            continue;
                        }

                        if (candle.last >= price)
                        {
                            result = false;
                            break;
                        }
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

        public bool isHangAround(double price, double threshold, int past_num, int past_pos=0)
        {
            bool result = false;
            try
            {
                int candle_cnt = getCandleCount();
                if (candle_cnt > past_num)
                {
                    int last_index = (candle_cnt - 1) - past_pos;
                    int past_index = last_index - past_num;
                    result = true;
                    for (int i = last_index; i > past_index; i--)
                    {
                        Candlestick candle = m_candleList[i];
                        if (candle == null)
                        {
                            continue;
                        }

                        double diff = Math.Abs(candle.last - price);

                        if (diff > threshold)
                        {
                            result = false;
                            break;
                        }
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

    }
}
