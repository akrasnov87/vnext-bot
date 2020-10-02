using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace vNextBot.Bots
{
    public static class BotExtension
    {
        public static BotChannelIdentity GetIdentity(this IMessageActivity activity)
        {
            return new BotChannelIdentity(activity);
        }
    }
}
