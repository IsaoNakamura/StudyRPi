using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using System.Net.Http;
using System.Net.Http.Headers;

namespace UtilityCryptowatch
{
    public class EndPoint
    {
        public static readonly Uri endpointUri = new Uri("https://api.cryptowat.ch");
    }

    public class Allowance
    {
        [JsonProperty]
        public double cost { get; set; }

        [JsonProperty]
        public double remaining { get; set; }

        public Allowance()
        {
            cost = 0.0;
            remaining = 0.0;
            return;
        }
    }

    public class CandleFactor
    {
        [JsonProperty]
        public List<double> factorList { get; set; }

        public CandleFactor()
        {
            factorList = null;
            return;
        }

        public double getClosePrice()
        {
            if (factorList == null)
            {
                return 0.0;
            }
            return factorList[0];
        }

        public double getOpenPrice()
        {
            if (factorList == null)
            {
                return 0.0;
            }
            return factorList[1];
        }

    }

    public class Result
    {
        [JsonProperty("60")]
        //public List<CandleFactor> miniute { get; set; }
        public List<List<double>> miniute { get; set; }

        [JsonProperty("900")]
        //public List<CandleFactor> miniute { get; set; }
        public List<List<double>> fifteen_miniute { get; set; }

        //List<double> is CandleFactor
        //        [0],       [1],       [2],      [3],        [4],    [5]
        //[ CloseTime, OpenPrice, HighPrice, LowPrice, ClosePrice, Volume ]

        public Result()
        {
            miniute = null;
            return;
        }

        public List<List<double>> getResult(int periods)
        {
            List<List<double>> result = null;
            if (periods == 60)
            {
                result = miniute;
            } else if (periods == 900)
            {
                result = fifteen_miniute;
            }
            return result;
        }
    }

    public class BitflyerOhlc
    {
        [JsonProperty]
        public Allowance allowance { get; set; }

        [JsonProperty]
        public Result result { get; set; }

        public BitflyerOhlc()
        {
            allowance = null;
            result = null;
            return;
        }

        public static async Task<BitflyerOhlc> GetOhlcAfterAsync
        (
            string  symbol,         // 通貨シンボル、 FX_BTC_JPYならbtcfxjpyを指定する
            int     periods,        // 足指定、1分足なら60を指定する
            long    after_seconds  // 現在から何分前の情報を取得するか
        )
        {
            BitflyerOhlc retObj = null;
            try
            {
                // 現在からafter_minitue分前のUnixTimeを算出する
                DateTime now = DateTime.Now;
                var dto = new DateTimeOffset(now.Ticks, new TimeSpan(+09, 00, 00));
                long after_length = dto.ToUnixTimeSeconds() - after_seconds;

                var method = "GET";
                var path = "/markets/bitflyer/";
                var query = "?periods=" + periods + "&after=" + after_length;
                using (var client = new HttpClient())
                using (var request = new HttpRequestMessage(new HttpMethod(method), path + symbol + "/ohlc" + query))
                {
                    client.BaseAddress = EndPoint.endpointUri;
                    var message = await client.SendAsync(request);
                    var resJson = await message.Content.ReadAsStringAsync();

                    if (message.IsSuccessStatusCode==false)
                    {
                        Console.WriteLine(message.RequestMessage);
                        return null;
                    }

                    retObj = JsonConvert.DeserializeObject<BitflyerOhlc>(resJson);
                    if (retObj == null)
                    {
                        Console.WriteLine("Ticker's DeserializeObject is null.");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                retObj = null;
            }
            finally
            {
            }
            return retObj;
        }
    }


}
