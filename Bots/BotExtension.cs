using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using vNextBot.Model;

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
                if (e.Response != null)
                {
                    var response = (HttpWebResponse)e.Response;
                    return new HttpResult(response.StatusCode);
                } else
                {
                    return new HttpResult(HttpStatusCode.BadRequest);
                }
            }
        }

        public static string GetBearerToken()
        {
            using (var httpClient = new HttpClient())
            {
                List<KeyValuePair<string, string>> values = new List<KeyValuePair<string, string>>();
                values.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
                values.Add(new KeyValuePair<string, string>("client_id", Startup.MicrosoftAppId));
                values.Add(new KeyValuePair<string, string>("client_secret", Startup.MicrosoftAppPassword));
                values.Add(new KeyValuePair<string, string>("scope", "https://api.botframework.com/.default"));

                using (var content = new FormUrlEncodedContent(values))
                {
                    content.Headers.Clear();
                    content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

                    HttpResponseMessage response = httpClient.PostAsync("https://login.microsoftonline.com/botframework.com/oauth2/v2.0/token", content).ConfigureAwait(false).GetAwaiter().GetResult();

                    dynamic res = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult());
                    return res.access_token;
                }
            }
        }

        static ConnectorClient GetConnectorClient(string serviceUrl, string botClientId, string botSecret)
        {

            MicrosoftAppCredentials appCredentials =
               new MicrosoftAppCredentials(botClientId, botSecret);
            MicrosoftAppCredentials.TrustServiceUrl(serviceUrl);

            Uri uri = new Uri(serviceUrl);
            ConnectorClient connectorClient =
                new ConnectorClient(uri, appCredentials);

            return connectorClient;
        }

        public static async Task SendMessageAsync(
            string serviceUrl,
            string conversationId,
            string recipientId,
            string fromId,
            string message)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                ConnectorClient connectorClient = GetConnectorClient(serviceUrl, Startup.MicrosoftAppId, Startup.MicrosoftAppPassword);

                Activity messageActivity =
                new Activity();
                messageActivity.Type = ActivityTypes.Message;
                messageActivity.Text = message;
                messageActivity.ChannelId = Channels.Webchat;
                messageActivity.ServiceUrl = serviceUrl;
                messageActivity.Conversation = new ConversationAccount()
                {
                    Id = conversationId
                };
                messageActivity.Recipient = new ChannelAccount()
                {
                    Id = recipientId
                };
                messageActivity.From = new ChannelAccount()
                {
                    Id = fromId
                };

                await connectorClient
                    .Conversations
                    .SendToConversationAsync(messageActivity);

            }
        }
    }
}
