using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace vNextBot.Bots
{
    public static class BotExtension
    {
        public static BotChannelIdentity GetIdentity(this IMessageActivity activity)
        {
            return new BotChannelIdentity(activity);
        }

        public static HttpResult Get(string uri, string token)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Headers.Add("Authorization", "TFS " + token);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    var httpResult = new HttpResult(response.StatusCode);
                    httpResult.Result = reader.ReadToEnd();
                    return httpResult;
                }
            } catch(System.Net.WebException e)
            {
                var response = (HttpWebResponse)e.Response;
                return new HttpResult(response.StatusCode);
            }
        }
    }
}
