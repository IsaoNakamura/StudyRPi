using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using System.Net.Http;
using System.Net.Http.Headers;

using System.Security.Cryptography;

namespace UtilityBitflyer
{
    public class EndPoint
    {
        public static readonly Uri endpointUri = new Uri("https://api.bitflyer.jp");
    }

    class RequestBitflyer
    {
        public static async Task<string> Request
        (
            AuthBitflyer auth,
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
                        request.Content = content;
                    }

                    if (auth != null)
                    {
                        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                        var data = timestamp + method + path + body;
                        var hash = SignWithHMACSHA256(data, auth.m_access_secret);

                        request.Headers.Add("ACCESS-KEY", auth.m_access_key);
                        request.Headers.Add("ACCESS-TIMESTAMP", timestamp);
                        request.Headers.Add("ACCESS-SIGN", hash);
                    }

                    var message = await client.SendAsync(request);
                    responce = await message.Content.ReadAsStringAsync();

                    if (message.IsSuccessStatusCode == false)
                    {
                        Console.WriteLine(message.RequestMessage);
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

        static string SignWithHMACSHA256(string data, string secret)
        {
            using (var encoder = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
            {
                var hash = encoder.ComputeHash(Encoding.UTF8.GetBytes(data));
                return ToHexString(hash);
            }
        }

        static string ToHexString(byte[] bytes)
        {
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
