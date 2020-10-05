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
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace vNextBot.Bots
{
    public class EchoBot : ActivityHandler
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            using(ApplicationContext db = new ApplicationContext())
            {
                var urlSetting = db.Settings.FirstOrDefault(t => t.c_key == "C_URL");

                BotChannelIdentity identity = turnContext.Activity.GetIdentity();
                string replyText = "Результат известен, но обработчик не найден.";
                if (identity.IsAuthenticated)
                {
                    var answer = db.Search(turnContext.Activity.Text);
                    if (answer == null)
                    {
                        replyText = "Нет информации по Вашему запрос, попробуйте перестроить и повторить заново.";
                    }
                    else
                    {
                        if (answer.Action == "API") {
                            replyText = BotExtension.Get(urlSetting.c_value + answer.Url, identity.TfsToken).Result;
                        }
                    }
                }
                else
                {
                    var setting = db.Settings.FirstOrDefault(t => t.c_key == "C_BOT_URL");
                    if (identity.IsActive)
                    {        
                        replyText = "Для завершения регистрации требуется перейти по <a href=\"" + setting.c_value + "?token=" + identity.AuthorizeToken + "\">ссылке</a>.";
                    } else
                    {
                        Regex regex = new Regex(@"^\d{6}$");
                        if (regex.IsMatch(turnContext.Activity.Text))
                        {
                            int pin = int.Parse(regex.Match(turnContext.Activity.Text).Value);
                            if (db.Registry(identity, pin, turnContext.Activity.ServiceUrl))
                            {
                                identity.UpdateIdentity(turnContext.Activity);

                                //1|29:1dXWZFADh0YyLw6U0BhFV-EjyRYBbzBkK2246Lr117Mg|skype
                                replyText = "Спасибо! Ключ принят.<br />Теперь можно выполнить авторизоваться на сервере TFS. Для этого требуется перейти по <a href=\"" + setting.c_value + "?token=" + identity.AuthorizeToken + "\">ссылке</a>.";
                            }
                        }
                        else
                        {
                            replyText = "Привет! Нам нужно познакомиться.<br />Информация о <b>" + turnContext.Activity.From.Name + "</b> отсутствует в моей базе данных.<br />Отправь мне ключ для своей идентификации.";
                        }
                    }
                }
                Activity activity = MessageFactory.Text(replyText);
                activity.SetLocale("ru-RU");
                
                await turnContext.SendActivityAsync(activity, cancellationToken);
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
