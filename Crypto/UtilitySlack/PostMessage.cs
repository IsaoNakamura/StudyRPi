using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UtilitySlack
{
    public class PostMessage
    {
        public PostMessage()
        {
        }

        public static async Task<int> Request
        (
            AuthSlack auth,
            string message
        )
        {
            int result = 0;
            try
            {
                if(auth == null)
                {
                    Console.WriteLine("auth is null.");
                    result = -1;
                    return result;
                }

                if (message == null)
                {
                    Console.WriteLine("message is null.");
                    result = -1;
                    return result;
                }

                if(message.Length <= 0)
                {
                    Console.WriteLine("message is 0.");
                    result = -1;
                    return result;
                }

                string method = "POST";
                string path = "/api/chat.postMessage";

                string query = string.Format("?token={0}&channel={1}&text={2}&username={3}&icon_url={4}"
                               , auth.token
                               , auth.channel
                               , message
                               , auth.username
                               , auth.icon_url
                              );
                path = path + query;

                string resJson = await RequestSlack.Request(method, path, null);
                if (resJson == null)
                {
                    Console.WriteLine("failed to RequestSlack.");
                    result = -1;
                    return result;
                }
                //Console.WriteLine(resJson);
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
    }
}
