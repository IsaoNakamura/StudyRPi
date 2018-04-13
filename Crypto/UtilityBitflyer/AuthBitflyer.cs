using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using UtilityBasic;

namespace UtilityBitflyer
{
    public class AuthBitflyer
    {
        [JsonProperty("ACCESS-KEY")]
        public string m_access_key { get; private set; }

        [JsonProperty("ACCESS-SECRET")]
        public string m_access_secret { get; private set; }

        private AuthBitflyer()
        {
            m_access_key = null;
            m_access_secret = null;
            return;
        }

        public static AuthBitflyer createAuthBitflyer(string filePath)
        {
            AuthBitflyer result = null;
            try
            {
                if (filePath == null || filePath.Length <= 0)
                {
                    result = null;
                    return result;
                }

                AuthBitflyer auth = null;
                if (UtilityJson.loadFromJson<AuthBitflyer>(out auth, filePath) != 0)
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
