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
using System;

namespace vNextBot.Bots
{
    public class EchoBot : ActivityHandler
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(new Activity { Type = ActivityTypes.Typing }, cancellationToken);

            using (ApplicationContext db = new ApplicationContext())
            {
                var urlSetting = db.Settings.FirstOrDefault(t => t.c_key == "C_URL");

                BotChannelIdentity identity = turnContext.Activity.GetIdentity();
                string replyText = "Результат известен, но обработчик не найден.";
                var setting = db.Settings.FirstOrDefault(t => t.c_key == "C_BOT_URL");
                if (identity.IsAuthenticated)
                {
                    if (identity.DbUser.team_id.HasValue)
                    {
                        bool isFTS = false;
                        string searchText = turnContext.Activity.Text.NameWithOut();
                        var answer = db.Search(searchText, 0);
                        if (answer == null)
                        {
                            answer = db.Search(searchText, 1);

                            if(answer != null)
                            {
                                isFTS = true;
                            }
                        }

                        if (answer == null)
                        {
                            replyText = "Поиск ответа не дал результат, попробуйте сформулировать запрос иначе.";
                        }
                        else
                        {
                            HttpResult httpResult = null;
                            // получение информации от TFS
                            if (answer.Action == "TFS_API")
                            {
                                string name = turnContext.Activity.Text.GetName();
                                string tfsUrl = urlSetting.c_value + answer.Url;
                                if (!string.IsNullOrEmpty(name))
                                {
                                    tfsUrl = tfsUrl.Replace("@Me", "");
                                    tfsUrl += "'" + name + "'";
                                }
                                httpResult = BotExtension.Get(tfsUrl, identity.TfsToken, "text/plain");
                            }

                            //получение информации от внешнего API
                            if (answer.Action == "API")
                            {
                                string url = answer.Url;
                                httpResult = BotExtension.Get(url, "text/plain");
                            }

                            if(answer.Action == "DOWNLOAD")
                            {
                                string url = answer.Url;
                                string title = answer.Title;
                                httpResult = new HttpResult(System.Net.HttpStatusCode.OK);
                                httpResult.Result = "Требуемую информацию можно скачать по ссылке «<a href=\"" + url + "\">_*" + title.Replace("@Me", identity.Name) + "*_</a>»";
                            }

                            if (answer.Action == "LINK")
                            {
                                string url = answer.Url;
                                string title = answer.Title;
                                httpResult = new HttpResult(System.Net.HttpStatusCode.OK);
                                httpResult.Result = "Информацию можно получить по ссылке «<a href=\"" + url + "\">_*" + title.Replace("@Me", identity.Name) + "*_</a>»";
                            }

                            if (answer.Action == "TEXT")
                            {
                                string title = answer.Title;
                                httpResult = new HttpResult(System.Net.HttpStatusCode.OK);
                                httpResult.Result = title.Replace("@Me", identity.Name);
                            }

                            if(httpResult == null)
                            {
                                httpResult = new HttpResult(System.Net.HttpStatusCode.OK);
                                httpResult.Result = "Информация найден, но команда *" + answer.Action + "* неизвестна.<br />Сообщите разработчику для обработки команды.";
                            }

                            if (isFTS)
                            {
                                httpResult.SetFTS();
                            }

                            if (httpResult.Status == System.Net.HttpStatusCode.Unauthorized)
                            {
                                replyText = "Для продолжения работы требуется перейти по <a href=\"" + setting.c_value + "?token=" + identity.AuthorizeToken + "\">_*ссылке*_</a> и повторить авторизацию на сервере TFS.";
                            }
                            else
                            {
                                replyText = httpResult.Result;
                            }
                        }
                    }
                    else
                    {
                        string teamName = turnContext.Activity.Text;
                        var httpResult = BotExtension.Get(string.Format("{0}/v1/teams/" + teamName, urlSetting.c_value), identity.TfsToken, "application/json");
                        if (httpResult.Result != "null")
                        {
                            var team = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(httpResult.Result);
                            identity.DbUser.c_team = team.name;
                            identity.DbUser.team_id = Guid.Parse((string)team.id);
                            db.Update(identity.DbUser);
                            db.SaveChanges();
                            replyText = "Спасибо, регистрация завершена!<br />Вы в проекте *" + identity.DbUser.c_project + "* и Ваша команде *" + identity.DbUser.c_team + "*.";
                        }
                        else
                        {
                            replyText = string.Format("Команда {0} не найден. Повторите запрос еще раз.", teamName);
                        }
                    }
                }
                else
                {
                    if (identity.IsActive)
                    {        
                        replyText = "Для продолжения работы нужно перейти по <a href=\"" + setting.c_value + "?token=" + identity.AuthorizeToken + "\">_*ссылке*_</a> и выполнить авторизацию.";
                    } else
                    {
                        Regex regex = new Regex(@"^\d{6}$");
                        if (regex.IsMatch(turnContext.Activity.Text))
                        {
                            int pin = int.Parse(regex.Match(turnContext.Activity.Text).Value);
                            if (db.Registry(identity, pin, turnContext.Activity.ServiceUrl))
                            {
                                identity.UpdateIdentity(turnContext.Activity);

                                replyText = "Спасибо! Ключ принят.<br />Теперь нужно выполнить авторизоваться на сервере TFS и для этого требуется перейти по <a href=\"" + setting.c_value + "?token=" + identity.AuthorizeToken + "\">_*ссылке*_</a>.";
                            }
                        } else {
                            replyText = "Здравствуйте! Информация о *" + turnContext.Activity.From.Name + "* отсутствует в базе данных.<br />Для начала регистрации, требуется отправь ключ.";
                        }
                    }
                }

                await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
            }
            
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Приветствую! Я чат-бот команды *vNext*.<br />Моя основная задача оптимизировать работу с TFS.";
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
