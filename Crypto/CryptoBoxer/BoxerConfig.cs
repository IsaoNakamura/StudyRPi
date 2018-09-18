using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using UtilityBasic;

namespace CryptoBoxer
{
    public class BoxerConfig
    {
        [JsonProperty]
        public double amount { get; set; }

        [JsonProperty]
        public int periods { get; set; }

        [JsonProperty]
        public string product_bitflyer { get; set; }

        [JsonProperty]
        public string product_cryptowatch { get; set; }

        [JsonProperty]
        public int ema_sample_num { get; set; }

        [JsonProperty]
        public int ema_sub_sample_num { get; set; }

        [JsonProperty]
        public int boll_sample_num { get; set; }

        [JsonProperty]
        public int boll_top_sample_num { get; set; }

        [JsonProperty]
        public int boll_over_candle_num { get; set; }

        [JsonProperty]
        public double ema_diff_far { get; set; }

        [JsonProperty]
        public double ema_diff_near { get; set; }

        [JsonProperty]
        public double ema_cross_near { get; set; }

        [JsonProperty]
        public double losscut_value { get; set; }

        [JsonProperty]
        public int buffer_num { get; set; }

        [JsonProperty]
        public int backtest_hour { get; set; }

        [JsonProperty]
        public int backtest_flag { get; set; }

        [JsonProperty]
        public int boll_diff_play { get; set; }

        [JsonProperty]
        public double next_open_diff { get; set; }

        [JsonProperty]
        public int back_cnt { get; set; }

        [JsonProperty]
        public int expiration_cnt { get; set; }

        [JsonProperty]
        public double expiration_ema_diff { get; set; }

        [JsonProperty]
        public double entry_offset { get; set; }

        [JsonProperty]
        public int boll_outside_check { get; set; }

        [JsonProperty]
        public double frontline_ahead { get; set; }

        [JsonProperty]
        public int boll_chk_past_num { get; set; }

        [JsonProperty]
        public double ema_reverce_play { get; set; }

        [JsonProperty]
        public double whale_vola_rate { get; set; }

        [JsonProperty]
        public double vola_big { get; set; }

        [JsonProperty]
        public double vola_small { get; set; }

        [JsonProperty]
        public double fixed_profit { get; set; }

        public BoxerConfig()
        {
            amount = 0.0;
            periods = 60;
            product_bitflyer = null;
            product_cryptowatch = null;
            ema_sample_num = 20;
            ema_sub_sample_num = 20;
            boll_sample_num = 20;
            boll_top_sample_num = 100;
            boll_over_candle_num = 6;
            ema_diff_far = 1000;
            ema_diff_near = 100;
            ema_cross_near = 2900.0;
            losscut_value = -5000;
            buffer_num = 60;
            backtest_hour = 72;
            backtest_flag = 0;
            boll_diff_play = 1000;
            next_open_diff = 500.0;
            back_cnt = 2;
            expiration_cnt = 11;
            expiration_ema_diff = 5000.0;
            entry_offset = 800.0;
            boll_outside_check = 1;
            frontline_ahead = 1000.0;
            boll_chk_past_num = 4;
            ema_reverce_play = 0.0;
            whale_vola_rate = 300.0;
            vola_big = 500.0;
            vola_small = 20.0;
            fixed_profit = 300.0;
            return;
        }

        public static BoxerConfig loadBoxerConfig(string filePath)
        {
            BoxerConfig result = null;
            try
            {
                if (filePath == null || filePath.Length <= 0)
                {
                    result = null;
                    return result;
                }

                BoxerConfig config = null;
                if (UtilityJson.loadFromJson<BoxerConfig>(out config, filePath) != 0)
                {
                    result = null;
                    return result;
                }

                result = config;
            }
            catch (Exception ex)
            {
                Console.WriteLine("exception: {0}", ex.Message);
                result = null;
            }
            finally
            {
            }
            return result;
        }

        public static int saveBoxerConfig(BoxerConfig config, string dirPath, string fileName)
        {
            int result = 0;
            try
            {
                if (dirPath == null || dirPath.Length <= 0)
                {
                    result = -1;
                    return result;
                }

                if (fileName == null || fileName.Length <= 0)
                {
                    result = -1;
                    return result;
                }

                if (UtilityJson.saveToJson<BoxerConfig>(config, dirPath, fileName) != 0)
                {
                    result = -1;
                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("exception: {0}", ex.Message);
                result = -1;
            }
            finally
            {
            }
            return result;
        }
    }
}
