using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;

using System.Net.Http;
using System.Net.Http.Headers;


using System.Security.Cryptography;

using System.Collections.Specialized;

namespace UtilitySlack
{
    public class EndPoint
    {
        public static readonly Uri endpointUri = new Uri("https://slack.com");
    }

    public class RequestSlack
    {

        public static async Task<string> Request
        (
            string method,
            string path,
            string body
        )
        {
            string responce = null;
            try
            {
                StringContent content = null;
                if (body != null && body.Length > 0)
                {
                    content = new StringContent(body);
                }

                using (var client = new HttpClient())
                using (var request = new HttpRequestMessage(new HttpMethod(method), path))
                {
                    client.BaseAddress = EndPoint.endpointUri;

                    if (content != null)
                    {
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        content.Headers.ContentType.CharSet = "UTF-8";
                        request.Content = content;
                    }

                    var message = await client.SendAsync(request);
                    responce = await message.Content.ReadAsStringAsync();

                    if (message.IsSuccessStatusCode == false)
                    {
                        Console.WriteLine(message.RequestMessage);
                        Console.WriteLine(responce);
                        responce = null;
                        return responce;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                responce = null;
            }
            finally
            {
            }
            return responce;
        }
    }
}
