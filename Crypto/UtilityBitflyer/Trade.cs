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
    public class Trade
    {
        public static async Task<JArray> getExecutionsAcceptance
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
                string path = "/v1/me/getexecutions";
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

        public static async Task<JArray> getPositions
        (
            AuthBitflyer auth,
            string product_code
        )
        {
            JArray retArray = null;
            try
            {
                string method = "GET";
                string path = "/v1/me/getpositions";
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

        public static async Task<bool> isExistsPosition
        (
            AuthBitflyer auth,
            string product_code
        )
        {
            bool result = false;
            try
            {
                JArray retArray = await getPositions(auth, product_code);
                if (retArray != null && retArray.Count > 0)
                {
                    result = true;
                    return result;
                }
                result = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
            }
            return result;
        }

        public static async Task<bool> isExistsActiveOrders
        (
            AuthBitflyer auth,
            string product_code
        )
        {
            bool result = false;
            try
            {
                bool isParentOrder = false;
                JArray parentArray = await SendParentOrder.getParentOrders(auth, product_code, "ACTIVE");
                if (parentArray != null && parentArray.Count > 0)
                {
                    isParentOrder = true;
                }

                bool isChildOrder = false;
                JArray childArray = await SendChildOrder.getChildOrders(auth, product_code, "ACTIVE");
                if (childArray != null && childArray.Count > 0)
                {
                    isChildOrder = true;
                }

                result = (isParentOrder || isChildOrder);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
            }
            return result;
        }

        public static async Task<bool> isActive
        (
            AuthBitflyer auth,
            string product_code
        )
        {
            bool result = false;
            try
            {
                bool isOrder = await isExistsActiveOrders(auth, product_code);


                bool isPosition = await isExistsPosition(auth, product_code);

                result = (isOrder || isPosition);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
            }
            return result;
        }

    }
}
