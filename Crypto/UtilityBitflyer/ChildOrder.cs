using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.Net.Http;
using System.Net.Http.Headers;


namespace UtilityBitflyer
{
    public class SendChildOrderResponse
    {
        [JsonProperty]
        public string child_order_acceptance_id { get; private set; }
    }

    public class SendChildOrderBody
    {
        [JsonProperty]
        public string product_code { get; set; }

        [JsonProperty]
        public string child_order_type { get; set; }

        [JsonProperty]
        public string side { get; set; }

        [JsonProperty]
        public double price { get; set; }

        [JsonProperty]
        public double size { get; set; }

        [JsonProperty]
        public int minute_to_expire { get; set; }

        [JsonProperty]
        public string time_in_force { get; set; }
    }


    public class GetChildOrderResponse
    {
        public double average_price { get; set; }
        public string child_order_state { get; set; }
        public string child_order_id { get; set; }
        public string child_order_acceptance_id { get; set; }
        public string side { get; set; }
        public double price { get; set; }
    }

    public class SendChildOrder
    {
        public static async Task<SendChildOrderResponse> PostSendChildOrder
        (
            AuthBitflyer auth,
            SendChildOrderBody bodyObj
        )
        {
            SendChildOrderResponse retObj = null;
            try
            {
                if (bodyObj == null)
                {
                    Console.WriteLine("SendChildOrderBody is null.");
                    return null;
                }

                string method = "POST";
                string path = "/v1/me/sendchildorder";

                string body = JsonConvert.SerializeObject(bodyObj, Formatting.None);
                if (body == null || body.Length <= 0)
                {
                    Console.WriteLine("failed to Serialize bodyObj.");
                    return null;
                }

                string resJson = await RequestBitflyer.Request(auth, method, path, body);
                if (resJson == null)
                {
                    //Console.WriteLine("failed to RequestBitflyer.");
                    return null;
                }

                retObj = JsonConvert.DeserializeObject<SendChildOrderResponse>(resJson);
                if (retObj == null)
                {
                    Console.WriteLine("SendChildOrder's DeserializeObject is null.");
                    return null;
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

        public static async Task<SendChildOrderResponse> PostSendChildOrder
        (
            AuthBitflyer auth,
            string product_code,
            string child_order_type,
            string side,
            double price,
            double size,
            int minute_to_expire,
            string time_in_force
        )
        {
            SendChildOrderResponse retObj = null;
            try
            {
                SendChildOrderBody bodyObj = new SendChildOrderBody();
                if (bodyObj == null)
                {
                    Console.WriteLine("failed to create SendChildOrderBody.");
                    return null;
                }
                bodyObj.product_code = product_code;
                bodyObj.child_order_type = child_order_type;
                bodyObj.side = side;
                bodyObj.price = price;
                bodyObj.size = size;
                bodyObj.minute_to_expire = minute_to_expire;
                bodyObj.time_in_force = time_in_force;

                retObj = await PostSendChildOrder(auth, bodyObj);
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

        public static async Task<SendChildOrderResponse> BuyMarket
        (
            AuthBitflyer auth,
            string product_code,
            double size
        )
        {
            SendChildOrderResponse retObj = null;
            try
            {
                retObj = await PostSendChildOrder(
                                   auth,
                                   product_code,
                                   "MARKET",
                                   "BUY",
                                   0.0,
                                   size,
                                   10000,
                                   "GTC"
                               );
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

        public static async Task<SendChildOrderResponse> SellMarket
        (
            AuthBitflyer auth,
            string product_code,
            double size
        )
        {
            SendChildOrderResponse retObj = null;
            try
            {
                retObj = await PostSendChildOrder(
                                   auth,
                                   product_code,
                                   "MARKET",
                                   "SELL",
                                   0.0,
                                   size,
                                   10000,
                                   "GTC"
                               );
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

        // average_priceを返す　失敗すれば0.0
        private static async Task<double> SendMarketAcceptance
        (
            AuthBitflyer auth,
            string side,
            string product_code,
            double size
        )
        {
            double result = 0.0;
            try
            {
                SendChildOrderResponse retObj = await PostSendChildOrder(
                                                           auth,
                                                           product_code,
                                                           "MARKET",
                                                           side,
                                                           0.0,
                                                           size,
                                                           10000,
                                                           "GTC"
                                                       );
                if (retObj == null)
                {
                    Console.WriteLine("faile to SellMarketAcceptance.");
                    return result;
                }

                Console.WriteLine("child_order_acceptance_id={0}", retObj.child_order_acceptance_id);

                int retry_cnt = 0;
                while (true)
                {
                    System.Threading.Thread.Sleep(1000);

                    JArray retArray = await getChildOrdersAcceptance(auth, product_code, retObj.child_order_acceptance_id);
                    if (retArray != null && retArray.Count > 0)
                    {
                        JObject jobj = (JObject)retArray[0];
                        JValue jvalue = (JValue)jobj["average_price"];
                        result = (double)jvalue.Value;
                        break;
                    }
                    else
                    {
                        Console.WriteLine("retry={0}. faile to getChildOrdersAcceptance.", retry_cnt);
                    }


                    retry_cnt++;
                    if (retry_cnt > 180)
                    {
                        Console.WriteLine("timeout. faile to getChildOrdersAcceptance.");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = 0.0;
            }
            finally
            {
            }
            return result;
        }

        private static async Task<double> BuyMarketAcceptance
        (
            AuthBitflyer auth,
            string product_code,
            double size
        )
        {
            double result = 0.0;
            try
            {
                double retObj = await SendMarketAcceptance(
                                        auth,
                                        "BUY",
                                        product_code,
                                        size
                                      );
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = 0.0;
            }
            finally
            {
            }
            return result;
        }

        private static async Task<double> SellMarketAcceptance
        (
            AuthBitflyer auth,
            string product_code,
            double size
        )
        {
            double result = 0.0;
            try
            {
                double retObj = await SendMarketAcceptance(
                                        auth,
                                        "SELL",
                                        product_code,
                                        size
                                      );
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = 0.0;
            }
            finally
            {
            }
            return result;
        }



        public static async Task<JArray> getChildOrdersAcceptance
        (
            AuthBitflyer auth,
            string product_code,
            string acceptance_id
        )
        {
            JArray retArray = null;
            try
            {
                string method = "GET";
                string path = "/v1/me/getchildorders";
                string query = string.Format(
                        "?product_code={0}&child_order_acceptance_id={1}"
                        , product_code
                        , acceptance_id
                    );
                string body = "";

                path = path + query;

                string resJson = await RequestBitflyer.Request(auth, method, path, body);
                if (resJson == null)
                {
					Console.WriteLine("failed to getChildOrdersAcceptance-Request.");
                    return null;
                }

                retArray = (JArray)JsonConvert.DeserializeObject(resJson);
                if (retArray == null)
                {
                    Console.WriteLine("Ticker's DeserializeObject is null.");
                    return null;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                retArray = null;
            }
            finally
            {
            }
            return retArray;
        }

        public static async Task<JArray> getChildOrders
        (
            AuthBitflyer auth,
            string product_code,
            string child_order_state
        )
        {
            JArray retArray = null;
            try
            {
                string method = "GET";
                string path = "/v1/me/getchildorders";
                string query = string.Format(
                        "?product_code={0}&child_order_state={1}"
                        , product_code
                        , child_order_state
                    );
                string body = "";

                path = path + query;

                string resJson = await RequestBitflyer.Request(auth, method, path, body);
                if (resJson == null)
                {
					Console.WriteLine("failed to getChildOrders-Request.");
                    return null;
                }

                retArray = (JArray)JsonConvert.DeserializeObject(resJson);
                if (retArray == null)
                {
                    Console.WriteLine("Ticker's DeserializeObject is null.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                retArray = null;
            }
            finally
            {
            }
            return retArray;
        }

        // average_priceを返す　失敗すれば0.0
        public static async Task<GetChildOrderResponse> getChildOrderAveragePrice
        (
            AuthBitflyer auth,
            string product_code,
            string acceptance_id
        )
        {
            GetChildOrderResponse result = null;
            try
            {
                JArray retArray = await getChildOrdersAcceptance(auth, product_code, acceptance_id);
                if (retArray != null && retArray.Count > 0)
                {
                    bool isReject = false;
                    bool isFullComp = true;
                    double price = 0.0;
                    string state = "";
                    bool isFirst = true;
                    foreach (JObject jobj in retArray)
                    {
                        JValue average_price = (JValue)jobj["average_price"];
                        JValue child_order_state = (JValue)jobj["child_order_state"];
                        if ((string)child_order_state != "COMPLETED")
                        {
                            isFullComp = false;
                        }
                        if ((string)child_order_state == "REJECTED")
                        {
                            isReject = true;
                        }
                        if (isFirst)
                        {
                            price = (double)average_price;
                            state = (string)child_order_state;
                            isFirst = false;
                        }
                    }

                    if (isFullComp)
                    {
                        result = new GetChildOrderResponse();
                        if (result == null)
                        {
                            return result;
                        }
                        result.average_price = price;
                        result.child_order_state = state;
                    }
                    else if (isReject)
                    {
                        result = new GetChildOrderResponse();
                        if (result == null)
                        {
                            return result;
                        }
                        result.average_price = 0.0;
                        result.child_order_state = "REJECTED";
                    }
                }
                else
                {
                    //Console.WriteLine("failed to getChildOrderAveragePrice.");
                }
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

        public static async Task<bool> isCanceldChildOrders
        (
            AuthBitflyer auth,
            string product_code,
            List<string> acceptance_ids
        )
        {
            bool result = false;
            try
            {
                if (acceptance_ids == null || acceptance_ids.Count <= 0)
                {
                    result = false;
                    return result;
                }

                foreach (string acceptance_id in acceptance_ids)
                {
                    if (acceptance_id == null || acceptance_id.Length <= 0)
                    {
                        result = false;
                        return result;
                    }

                    bool isExistOrders = false;
                    {
                        JArray retArray = await getChildOrdersAcceptance(auth, product_code, acceptance_id);
                        if (retArray != null && retArray.Count > 0)
                        {
                            isExistOrders = true;
                            foreach (JObject jobj in retArray)
                            {
                                if (jobj == null)
                                {
                                    continue;
                                }

                                string child_order_state = (string)jobj["child_order_state"];
                                if (child_order_state != "COMPLETED")
                                {
                                    result = false;
                                    return result;
                                }
                            }
                        }
                        else
                        {
                            isExistOrders = false;
                        }
                    }

                    if (!isExistOrders)
                    {
                        JArray retArray = await Trade.getExecutionsAcceptance(auth, product_code, acceptance_id);
                        if (retArray != null && retArray.Count > 0)
                        {
                            result = false;
                            return result;
                        }
                        // 建玉にキャンセルした注文が入ってなければキャンセル済みとする
                        result = true;
                        return result;
                    }
                    else
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

    }
}
