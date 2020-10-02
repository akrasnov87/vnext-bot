// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.10.3

using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using vNextBot.Model;

namespace vNextBot.Bots
{
    public class EchoBot : ActivityHandler
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            using(ApplicationContext db = new ApplicationContext())
            {
                BotChannelIdentity identity = turnContext.Activity.GetIdentity();
                string replyText = "";
                if (identity.IsAuthenticated)
                {
                    replyText = $"Echo: {turnContext.Activity.Text}";
                }
                else
                {
                    Regex regex = new Regex(@"^\d{6}$");
                    if (regex.IsMatch(turnContext.Activity.Text))
                    {
                        int pin = int.Parse(regex.Match(turnContext.Activity.Text).Value);
                        if(db.Registry(identity, pin))
                        {
                            replyText = "Спасибо! Ключ принят. Теперь можно выполнить авторизоваться на сервере TFS";
                        }
                    }
                    else
                    {
                        replyText = "Привет! Нам нужно познакомиться. Информация о " + turnContext.Activity.From.Name + " отсутствует в моей базе данных. Отправь мне ключ для своей идентификации.";
                    }
                }

                await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
            }
            
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome!";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }
    }
}
