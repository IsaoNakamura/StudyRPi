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
    public class SendParentOrderBody
    {
        [JsonProperty]
        public string order_method { get; set; }

        [JsonProperty]
        public int minute_to_expire { get; set; }

        [JsonProperty]
        public string time_in_force { get; set; }

        [JsonProperty]
        public List<SendParentOrderParameter> parameters { get; set; }
    }

    public class SendParentOrderParameter
    {
        [JsonProperty]
        public string product_code { get; set; }

        [JsonProperty]
        public string condition_type { get; set; } // LIMIT MARKET STOP STOP_LIMIT TRAIL

        [JsonProperty]
        public string side { get; set; }

        [JsonProperty]
        public double price { get; set; }

        [JsonProperty]
        public double size { get; set; }

        [JsonProperty]
        public double trigger_price { get; set; }

        //[JsonProperty]
        //public int offset { get; set; }   // TRAIL必須項目
    }

    public class SendParentOrderResponse
    {
        [JsonProperty]
        public string parent_order_acceptance_id { get; private set; }
    }

    public class GetParentOrderResponse
    {
        public int page_id { get; set; }
        public string parent_order_id { get; set; }
        public string side { get; set; }
        public double average_price { get; set; }
        public string parent_order_state { get; set; }
        public List<GetChildOrderResponse> children { get; set; }
    }

    public class CancelParentOrderAcceptanceBody
    {
        [JsonProperty]
        public string product_code { get; set; }

        [JsonProperty]
        public string parent_order_acceptance_id { get; set; }
    }

    public class SendParentOrder
    {
        private static async Task<SendParentOrderResponse> PostSendParentOrder
        (
            AuthBitflyer auth,
            SendParentOrderBody bodyObj
        )
        {
            SendParentOrderResponse retObj = null;
            try
            {
                if (bodyObj == null)
                {
                    Console.WriteLine("SendParentOrderBody is null.");
                    return null;
                }

                string method = "POST";
                string path = "/v1/me/sendparentorder";

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

                retObj = JsonConvert.DeserializeObject<SendParentOrderResponse>(resJson);
                if (retObj == null)
                {
                    Console.WriteLine("SendParentOrder's DeserializeObject is null.");
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

        public static async Task<SendParentOrderResponse> SendStopLimitOCO
        (
            AuthBitflyer auth,
            string product_code,
            double size,
            double buy_price,
            double sell_price
        )
        {
            SendParentOrderResponse retObj = null;
            try
            {
                SendParentOrderBody bodyObj = new SendParentOrderBody();
                if (bodyObj == null)
                {
                    Console.WriteLine("failed to create SendParentOrderBody.");
                    return null;
                }
                bodyObj.order_method = "OCO";
                bodyObj.minute_to_expire = 10000;
                bodyObj.time_in_force = "GTC";

                List<SendParentOrderParameter> parameters = new List<SendParentOrderParameter>();
                if (parameters == null)
                {
                    Console.WriteLine("failed to create SendParentOrderBody's parameters.");
                    return null;
                }
                bodyObj.parameters = parameters;

                {
                    SendParentOrderParameter buyParam = new SendParentOrderParameter();
                    buyParam.condition_type = "STOP_LIMIT";
                    buyParam.trigger_price = buy_price;
                    buyParam.price = buy_price;
                    buyParam.product_code = product_code;
                    buyParam.side = "BUY";
                    buyParam.size = size;

                    bodyObj.parameters.Add(buyParam);
                }

                {
                    SendParentOrderParameter sellParam = new SendParentOrderParameter();
                    sellParam.condition_type = "STOP_LIMIT";
                    sellParam.trigger_price = sell_price;
                    sellParam.price = sell_price;
                    sellParam.product_code = product_code;
                    sellParam.side = "SELL";
                    sellParam.size = size;

                    bodyObj.parameters.Add(sellParam);
                }

                retObj = await PostSendParentOrder(auth, bodyObj);
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

        public static async Task<SendParentOrderResponse> SendStopLimitIFD
        (
            AuthBitflyer auth,
            string product_code,
            string first_side,
            double first_size,
            double first_price,
            double first_trigger_price,
            string second_side,
            double second_size,
            double second_price,
            double second_trigger_price
        )
        {
            SendParentOrderResponse retObj = null;
            try
            {
                SendParentOrderBody bodyObj = new SendParentOrderBody();
                if (bodyObj == null)
                {
                    Console.WriteLine("failed to create SendParentOrderBody.");
                    return null;
                }
                bodyObj.order_method = "OCO";
                bodyObj.minute_to_expire = 10000;
                bodyObj.time_in_force = "GTC";

                List<SendParentOrderParameter> parameters = new List<SendParentOrderParameter>();
                if (parameters == null)
                {
                    Console.WriteLine("failed to create SendParentOrderBody's parameters.");
                    return null;
                }
                bodyObj.parameters = parameters;

                {
                    SendParentOrderParameter firstParam = new SendParentOrderParameter();
                    firstParam.condition_type = "STOP_LIMIT";
                    firstParam.trigger_price = first_trigger_price;
                    firstParam.price = first_price;
                    firstParam.product_code = product_code;
                    firstParam.side = first_side;
                    firstParam.size = first_size;

                    bodyObj.parameters.Add(firstParam);
                }

                {
                    SendParentOrderParameter secondParam = new SendParentOrderParameter();
                    secondParam.condition_type = "STOP_LIMIT";
                    secondParam.trigger_price = second_trigger_price;
                    secondParam.price = second_price;
                    secondParam.product_code = product_code;
                    secondParam.side = second_side;
                    secondParam.size = second_size;

                    bodyObj.parameters.Add(secondParam);
                }

                retObj = await PostSendParentOrder(auth, bodyObj);
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

        private static async Task<SendParentOrderResponse>SendStopLimitSimple
        (
            AuthBitflyer auth,
            string product_code,
            string side,
            double size,
            double price,
            double trigger_price
        )
        {
            SendParentOrderResponse retObj = null;
            try
            {
                if (side == null)
                {
                    return null;
                }

                if (side.Length <= 0)
                {
                    return null;
                }

                if (side != "BUY" && side != "SELL")
                {
                    return null;
                }

                SendParentOrderBody bodyObj = new SendParentOrderBody();
                if (bodyObj == null)
                {
                    Console.WriteLine("failed to create SendParentOrderBody.");
                    return null;
                }
                bodyObj.order_method = "SIMPLE";
                bodyObj.minute_to_expire = 10000;
                bodyObj.time_in_force = "GTC";

                List<SendParentOrderParameter> parameters = new List<SendParentOrderParameter>();
                if (parameters == null)
                {
                    Console.WriteLine("failed to create SendParentOrderBody's parameters.");
                    return null;
                }
                bodyObj.parameters = parameters;

                {
                    SendParentOrderParameter buyParam = new SendParentOrderParameter();
                    buyParam.condition_type = "STOP_LIMIT";
                    buyParam.trigger_price = trigger_price;
                    buyParam.price = price;
                    buyParam.product_code = product_code;
                    buyParam.side = side;
                    buyParam.size = size;

                    bodyObj.parameters.Add(buyParam);
                }


                retObj = await PostSendParentOrder(auth, bodyObj);
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

        public static async Task<SendParentOrderResponse> BuyStopLimitSimple
        (
            AuthBitflyer auth,
            string product_code,
            double size,
            double price,
            double trigger_price
        )
        {
            SendParentOrderResponse retObj = null;
            try
            {
                retObj = await SendStopLimitSimple(auth, product_code, "BUY", size, price, trigger_price);
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

        public static async Task<SendParentOrderResponse> SellStopLimitSimple
        (
            AuthBitflyer auth,
            string product_code,
            double size,
            double price,
            double trigger_price
        )
        {
            SendParentOrderResponse retObj = null;
            try
            {
                retObj = await SendStopLimitSimple(auth, product_code, "SELL", size, price, trigger_price);
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

        public static async Task<int> cancelParentOrderAcceptance
        (
            AuthBitflyer auth,
            string product_code,
            string acceptance_id
        )
        {
            int result = 0;
            try
            {
                CancelParentOrderAcceptanceBody bodyObj = new CancelParentOrderAcceptanceBody();
                if (bodyObj == null)
                {
                    result = -1;
                    return result;
                }
                bodyObj.parent_order_acceptance_id = acceptance_id;
                bodyObj.product_code = product_code;

                string body = JsonConvert.SerializeObject(bodyObj, Formatting.None);
                if (body == null || body.Length <= 0)
                {
                    result = -1;
                    return result;
                }

                string method = "POST";
                string path = "/v1/me/cancelparentorder";

                int resCode = await RequestBitflyer.RequestChkSuccessCode(auth, method, path, body);
                if (resCode != 0)
                {
                    Console.WriteLine("failed to RequestChkSuccessCode for cancel.");
                    result = -1;
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
            }
            return result;
        }

        public static async Task<JArray> getParentOrders
        (
            AuthBitflyer auth,
            string product_code
        )
        {
            JArray retArray = null;
            try
            {
                string method = "GET";
                string path = "/v1/me/getparentorders";
                string query = string.Format(
                        "?product_code={0}"
                        , product_code
                    );
                string body = "";

                path = path + query;

                string resJson = await RequestBitflyer.Request(auth, method, path, body);
                if (resJson == null)
                {
                    Console.WriteLine("failed to RequestBitflyer.");
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

        public static async Task<JArray> getParentOrders
        (
            AuthBitflyer auth,
            string product_code,
            string parent_order_state
        )
        {
            JArray retArray = null;
            try
            {
                string method = "GET";
                string path = "/v1/me/getparentorders";
                string query = string.Format(
                        "?product_code={0}&parent_order_state={1}"
                        , product_code
                        , parent_order_state
                    );
                string body = "";

                path = path + query;

                string resJson = await RequestBitflyer.Request(auth, method, path, body);
                if (resJson == null)
                {
                    Console.WriteLine("failed to RequestBitflyer.");
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

        private static async Task<JArray> getParentOrders
        (
            AuthBitflyer auth,
            string product_code,
            int after_page_id,
            int before_page_id
        )
        {
            JArray retArray = null;
            try
            {
                string method = "GET";
                string path = "/v1/me/getparentorders";
                string query = string.Format(
                        "?product_code={0}&after={1}&before={2}"
                        , product_code
                        , after_page_id
                        , before_page_id
                    );
                string body = "";

                path = path + query;

                string resJson = await RequestBitflyer.Request(auth, method, path, body);
                if (resJson == null)
                {
                    Console.WriteLine("failed to RequestBitflyer.");
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

        public static async Task<string> getParentOrderState
        (
            AuthBitflyer auth,
            string product_code,
            int page_id
        )
        {
            string result = null;
            try
            {
                JArray retArray = await getParentOrders(auth, product_code, page_id - 1, page_id + 1);

                if (retArray == null || retArray.Count<=0 || retArray.Count>1)
                {
                    result = null;
                    return result;
                }

                JObject jobj = (JObject)retArray[0];
                if (jobj == null)
                {
                    result = null;
                    return result;
                }

                string state = (string)jobj["parent_order_state"];
                if (state == null || state.Length <= 0)
                {
                    result = null;
                    return result;
                }

                result = state;

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

        private static async Task<JObject> getParentOrderAcceptance
        (
            AuthBitflyer auth,
            string product_code,
            string acceptance_id
        )
        {
            JObject retObj = null;
            try
            {
                string method = "GET";
                string path = "/v1/me/getparentorder";
                string query = string.Format(
                        "?product_code={0}&parent_order_acceptance_id={1}"
                        , product_code
                        , acceptance_id
                    );
                string body = "";

                path = path + query;

                string resJson = await RequestBitflyer.Request(auth, method, path, body);
                if (resJson == null)
                {
                    Console.WriteLine("failed to RequestBitflyer.");
                    return null;
                }

                retObj = (JObject)JsonConvert.DeserializeObject(resJson);
                if (retObj == null)
                {
                    Console.WriteLine("Ticker's DeserializeObject is null.");
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

        private static async Task<JArray> getChildOrdersParent
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
                        "?product_code={0}&parent_order_id={1}"
                        , product_code
                        , acceptance_id
                    );
                string body = "";

                path = path + query;

                string resJson = await RequestBitflyer.Request(auth, method, path, body);
                if (resJson == null)
                {
                    Console.WriteLine("failed to RequestBitflyer.");
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
        public static async Task<GetParentOrderResponse> getParentOrderOCO
        (
            AuthBitflyer auth,
            string product_code,
            string acceptance_id
        )
        {
            GetParentOrderResponse result = null;
            try
            {
                JObject retObj = await getParentOrderAcceptance(auth, product_code, acceptance_id);
                if (retObj == null)
                {
                    result = null;
                    return result;
                }

                int page_id = (int)retObj["id"];
                string parent_order_id = (string)retObj["parent_order_id"];
                if (parent_order_id == null || parent_order_id.Length <= 0)
                {
                    result = null;
                    return result;
                }

                JArray retArray = await getChildOrdersParent(auth, product_code, parent_order_id);
                if (retArray == null || retArray.Count <= 0)
                {
                    result = new GetParentOrderResponse();
                    if (result == null)
                    {
                        return result;
                    }
                    result.page_id = page_id;
                    result.parent_order_id = parent_order_id;
                    result.side = "";
                    result.average_price = 0.0;
                    result.parent_order_state = "NONE";
                    
                    return result;
                }

                JObject compObj = null;

                int cntReject = 0;
                int cntExpired = 0;
                int cntCanceld = 0;
                int cntActive = 0;

                List<GetChildOrderResponse> children = new List<GetChildOrderResponse>();
                if (children == null)
                {
                    result = null;
                    return result;
                }

                foreach (JObject jobj in retArray)
                {
                    if (jobj == null)
                    {
                        continue;
                    }

                    GetChildOrderResponse child = new GetChildOrderResponse();
                    if (child == null)
                    {
                        continue;
                    }

                    child.child_order_state = (string)jobj["child_order_state"];
                    child.average_price = (double)jobj["average_price"];
                    child.price = (double)jobj["price"];
                    child.child_order_id = (string)jobj["child_order_id"];
                    child.child_order_acceptance_id = (string)jobj["child_order_acceptance_id"];
                    child.side = (string)jobj["side"];

                    children.Add(child);


                    if (child.child_order_state == "COMPLETED")
                    {
                        compObj = jobj;
                    }
                    else if (child.child_order_state == "REJECTED")
                    {
                        cntReject++;
                    }
                    else if (child.child_order_state == "EXPIRED")
                    {
                        cntExpired++;
                    }
                    else if (child.child_order_state == "CANCELED")
                    {
                        cntCanceld++;
                    }
                    else if (child.child_order_state == "ACTIVE")
                    {
                        cntActive++;
                    }
                }

                if (compObj != null)
                {
                    result = new GetParentOrderResponse();
                    if (result == null)
                    {
                        return result;
                    }
                    result.page_id = page_id;
                    result.parent_order_id = parent_order_id;
                    result.side = (string)compObj["side"]; ;
                    result.average_price = (double)compObj["average_price"];
                    result.parent_order_state = (string)compObj["child_order_state"];
                    result.children = children;
                    Console.WriteLine("COMPLETED. side={0} state={1} accept_id={2} parent_id={3}", result.side, result.parent_order_state, acceptance_id, parent_order_id);
                }
                else if (cntActive == retArray.Count)
                {
                    result = new GetParentOrderResponse();
                    if (result == null)
                    {
                        return result;
                    }
                    result.page_id = page_id;
                    result.parent_order_id = parent_order_id;
                    result.side = "";
                    result.average_price = 0.0;
                    result.parent_order_state = "ACTIVE";
                    result.children = children;
                    //Console.WriteLine("ACTIVE. side={0} state={1} accept_id={2} parent_id={3}", result.side, result.parent_order_state, acceptance_id, parent_order_id);
                }
                else if (cntCanceld == retArray.Count)
                {
                    result = new GetParentOrderResponse();
                    if (result == null)
                    {
                        return result;
                    }
                    result.page_id = page_id;
                    result.parent_order_id = parent_order_id;
                    result.side = "";
                    result.average_price = 0.0;
                    result.parent_order_state = "CANCELED";
                    result.children = children;
                    Console.WriteLine("CANCELED. side={0} state={1} accept_id={2} parent_id={3}", result.side, result.parent_order_state, acceptance_id, parent_order_id);
                }
                else if ((cntReject + cntExpired) == retArray.Count)
                {
                    result = new GetParentOrderResponse();
                    if (result == null)
                    {
                        return result;
                    }
                    result.page_id = page_id;
                    result.parent_order_id = parent_order_id;
                    result.side = "";
                    result.average_price = 0.0;
                    result.parent_order_state = "REJECTED";
                    result.children = children;
                    Console.WriteLine("REJECTED. side={0} state={1} accept_id={2} parent_id={3}", result.side, result.parent_order_state, acceptance_id, parent_order_id);
                }
                else
                {
                    //Console.WriteLine("Entry Order is NULL. accept_id={0} parent_id={1}", acceptance_id, parent_order_id);
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
    }

}
