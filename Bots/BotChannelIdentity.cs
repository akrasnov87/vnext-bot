using Microsoft.Bot.Schema;
using System.Security.Principal;
using vNextBot.Model;

namespace vNextBot.Bots
{
    public class BotChannelIdentity : IIdentity
    {
        public string Id { get; }
        public BotChannelIdentity(IMessageActivity activity)
        {
            using(ApplicationContext db = new ApplicationContext())
            {
                Id = activity.From.Id;
                IsAuthenticated = db.IsUserExists(activity.From.Id);
                Name = activity.From.Name;
                AuthenticationType = activity.ChannelId;
            }
        }

        public string AuthenticationType
        {
            get;
        }

        public bool IsAuthenticated
        {
            get;
        }

        public string Name { get; }
    }
}
