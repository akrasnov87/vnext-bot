using Microsoft.Bot.Schema;
using System;
using System.Linq;
using System.Security.Principal;
using System.Text;
using vNextBot.Model;

namespace vNextBot.Bots
{
    public class BotChannelIdentity : IIdentity
    {
        public User DbUser { get; private set; }

        public string Id { get; }

        public string AuthorizeToken { get; private set; }
        public string TfsToken { get; private set; }

        public BotChannelIdentity(IMessageActivity activity)
        {
            Id = activity.From.Id;
            Name = activity.From.Name;
            AuthenticationType = activity.ChannelId;

            UpdateIdentity(activity);
        }

        public void UpdateIdentity(IMessageActivity activity)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                IsActive = db.IsUserExists(activity.From.Id);

                if (IsActive)
                {
                    var channelAccount = db.ChannelAccounts.FirstOrDefault(t => t.account_id == Id && t.c_type == AuthenticationType);
                    if (channelAccount != null)
                    {
                        DbUser = db.Users.FirstOrDefault(t => t.id == channelAccount.f_user);
                        IsAuthenticated = DbUser.b_authorize;

                        // генерация токен 1|29:1dXWZFADh0YyLw6U0BhFV-EjyRYBbzBkK2246Lr117Mg|skype
                        AuthorizeToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(DbUser.id + "|" + Id + "|" + AuthenticationType + "|" + activity.ServiceUrl + "|" + activity.Conversation.Id + "|" + activity.From.Id + "|" + activity.Recipient.Id));
                        var setting = db.Settings.FirstOrDefault(t => t.c_key == "C_TFS_URL");

                        if (!string.IsNullOrEmpty(DbUser.c_password))
                        {
                            var base64EncodedBytes = Convert.FromBase64String(DbUser.c_password);
                            string password = Encoding.UTF8.GetString(base64EncodedBytes);

                            TfsToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format("{0}|{1}|{2}|{3}|{4}", setting.c_value, DbUser.c_project, DbUser.c_domain, DbUser.c_login, password)));
                        }
                    }
                }
            }
        }

        public string AuthenticationType
        {
            get;
        }

        public bool IsAuthenticated
        {
            get;
            private set;
        }
        public bool IsActive
        {
            get;
            private set;
        }

        public string Name { get; }
    }
}
