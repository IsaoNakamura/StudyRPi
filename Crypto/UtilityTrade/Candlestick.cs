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

        // インジケータ
        public double ema { get; set; }
        public double stddev { get; set; }
        public double ma { get; set; }
        public double boll_high { get; set; }
        public double boll_low { get; set; }

        public Candlestick()
        {
            high = 0.0;
            low = 0.0;
            open = 0.0;
            last = 0.0;
            timestamp = string.Empty;
            ema = 0.0;
            stddev = 0.0;
            ma = 0.0;
            boll_high = 0.0;
            boll_low = 0.0;
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

        public int getShortBollLevel()
        {
            int result = -6;

            if(isTouchBollLow())
            {//BOLL_LOWにタッチしている場合はSHORTすべきでない
                result = -6;
                return result;
            }

            if (!isTouchBollHigh())
            {//BOLL_HIGHにタッチしてない場合はSHORTすべきでない
                result = -5;
                return result;
            }
            // ここからはBOLL_HIGHにタッチしている

            int lastLv = getLastLevel();

            if (isTrend())
            {//上昇キャンドルの場合
                if (boll_high <= low)
                {//完全にキャンドルがBOLL_HIGHをOVERしてる
                    // まだ上がる可能性が高い、SHORT超危険
                    result = -4;
                    return result;
                }

                if (boll_high <= open)
                {//下髭以外はBOLL_HIGHをOVERしてる
                    // まだ上がる可能性が高い、SHORT危険
                    result = -3;
                    return result;
                }

                if (boll_high <= last)
                {//HIGHTとLASTがBOLL_HIGHをOVERしてる
                    if (lastLv >= 2)
                    {// 大陽線もしくは小陽線
                        result = -2;
                        return result;
                    }
                    else
                    {// 上髭
                        result = 0;
                        return result;
                    }
                }

                if (boll_high <= high)
                {//上髭だけがBOLL_HIGHをOVERしてる
                    if (lastLv >= 4)
                    {// 大陽線
                        result = -1;
                        return result;
                    }
                    else
                    {// 上髭
                        result = 1;
                        return result;
                    }
                }
            }
            else
            {//下降トレンドの場合
                if (boll_high <= low)
                {
                    result = 2;
                    return result;
                }

                if (boll_high <= last)
                {
                    result = 3;
                    return result;
                }

                if (boll_high <= open)
                {
                    result = 4;
                    return result;
                }

                if (boll_high <= high)
                {
                    result = 5;
                    return result;
                }
            }

            return result;
        }

        public int getLongBollLevel()
        {
            int result = -6;

            if (isTouchBollHigh())
            {//BOLL_HIGHにタッチしている場合はLONGすべきでない
                result = -6;
                return result;
            }

            if (!isTouchBollLow())
            {//BOLL_LOWにタッチしてない場合はLONGすべきでない
                result = -5;
                return result;
            }
            // ここからはBOLL_LOWにタッチしている
            int lastLv = getLastLevel();

            if (!isTrend())
            {//下降キャンドルの場合
                if (boll_low >= high)
                {//完全にキャンドルがBOLL_LOWをUNDERしてる
                    // まだ下がる可能性が高い、LONG超危険
                    result = -4;
                    return result;
                }

                if (boll_low >= open)
                {
                    // まだ下がる可能性が高い、LONG危険
                    result = -3;
                    return result;
                }

                if (boll_low >= last)
                {
                    if (lastLv <= 2)
                    {// 大陰線もしくは小陰線
                        result = -2;
                        return result;
                    }
                    else
                    {// 下髭
                        result = 0;
                        return result;
                    }
                }

                if (boll_low >= low)
                {
                    if (lastLv <= 0)
                    {// 大陰線
                        result = -1;
                        return result;
                    }
                    else
                    {// 下髭
                        result = 1;
                        return result;
                    }
                }
            }
            else
            {//上昇キャンドルの場合
                if (boll_low >= high)
                {
                    result = 2;
                    return result;
                }

                if (boll_low >= last)
                {
                    result = 3;
                    return result;
                }

                if (boll_low >= open)
                {
                    result = 4;
                    return result;
                }

                if (boll_low >= low)
                {
                    result = 5;
                    return result;
                }
            }

            return result;
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

        // キャンドル終値がボリンジャー高バンド以上か判断
        public bool isOverBBLast()
        {
            if (boll_high <= last)
            {
                return true;
            }
            return false;
        }

        // キャンドル上値がボリンジャー高バンド以上か判断
        public bool isOverBBHigh()
        {
            if (boll_high <= high)
            {
                return true;
            }
            return false;
        }

        // キャンドル終値がボリンジャー低バンド以下か判断
        public bool isUnderBBLast()
        {
            if (boll_low >= last)
            {
                return true;
            }
            return false;
        }

        // キャンドル上値がボリンジャー高バンド以上か判断
        public bool isUnderBBLow()
        {
            if (boll_low <= low)
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
                m_candleList.Add(candle);

                // 保持数を超えた場合
                if (m_candleList.Count > m_buffer_num)
                {
                    // 何個消すか算出
                    int remove_cnt = m_candleList.Count - m_buffer_num;

                    // 古いCandlestickから削除
                    m_candleList.RemoveRange(0, remove_cnt);
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


    }
}
