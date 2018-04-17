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
        public int boll_sample_num { get; set; }

        [JsonProperty]
        public double ema_diff_far { get; set; }

        [JsonProperty]
        public double ema_diff_near { get; set; }

        [JsonProperty]
        public int buffer_num { get; set; }

        public BoxerConfig()
        {
            amount = 0.0;
            periods = 60;
            product_bitflyer = null;
            product_cryptowatch = null;
            ema_sample_num = 20;
            boll_sample_num = 20;
            ema_diff_far = 1000;
            ema_diff_near = 100;
            buffer_num = 60;
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
