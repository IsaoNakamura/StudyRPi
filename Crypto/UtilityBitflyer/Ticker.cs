using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using System.Net.Http;
using System.Net.Http.Headers;

namespace UtilityBitflyer
{
    public class EndPoint
    {
        public static readonly Uri endpointUri = new Uri("https://api.bitflyer.jp");
    }

    public class Ticker
    {
        [JsonProperty]
        public int tick_id { get; set; }

        [JsonProperty]
        public string product_code { get; set; }

        [JsonProperty]
        public string timestamp { get; set; }

        [JsonProperty]
        public double ltp { get; set; }

        [JsonProperty]
        public double best_ask { get; set; }

        [JsonProperty]
        public double best_bid { get; set; }

        [JsonProperty]
        public double total_bid_depth { get; set; }

        [JsonProperty]
        public double volume { get; set; }

        [JsonProperty]
        public double volume_by_product { get; set; }

        [JsonProperty]
        public double best_bid_size { get; set; }

        [JsonProperty]
        public double best_ask_size { get; set; }

        [JsonProperty]
        public double total_ask_depth { get; set; }

        public Ticker()
        {
            tick_id = 0;
            product_code = null;
            timestamp = null;
            ltp = 0.0;
            best_ask = 0.0;
            best_bid = 0.0;
            total_bid_depth = 0.0;
            volume = 0.0;
            volume_by_product = 0.0;
            best_bid_size = 0.0;
            best_ask_size = 0.0;
            total_ask_depth = 0.0;
            return;
        }

        public static  async Task<Ticker> GetTickerAsync()
        {
            Ticker retObj = null;
            try
            {
                var method = "GET";
                var path = "/v1/getticker";
                var query = "";
                using (var client = new HttpClient())
                using (var request = new HttpRequestMessage(new HttpMethod(method), path + query))
                {
                    client.BaseAddress = EndPoint.endpointUri;
                    var message = await client.SendAsync(request);
                    var resJson = await message.Content.ReadAsStringAsync();


                    retObj = JsonConvert.DeserializeObject<Ticker>(resJson);
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
