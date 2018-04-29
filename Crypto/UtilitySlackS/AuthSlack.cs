using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using UtilityBasic;

namespace UtilitySlack
{
    public class AuthSlack
    {
        [JsonProperty]
        public string channel { get; private set; }

        [JsonProperty]
        public string token { get; private set; }

        [JsonProperty]
        public string icon_url { get; private set; }

        [JsonProperty]
        public string username { get; private set; }

        public AuthSlack()
        {
            channel = null;
            token = null;
            icon_url = null;
            username = null;
            return;
        }

        public static AuthSlack createAuthSlack(string filePath)
        {
            AuthSlack result = null;
            try
            {
                if (filePath == null || filePath.Length <= 0)
                {
                    result = null;
                    return result;
                }

                AuthSlack auth = null;
                if (UtilityJson.loadFromJson<AuthSlack>(out auth, filePath) != 0)
                {
                    result = null;
                    return result;
                }

                result = auth;
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
    }
}
