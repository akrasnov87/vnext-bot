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
            await turnContext.SendActivityAsync(new Activity { Type = ActivityTypes.Typing }, cancellationToken);

            using (ApplicationContext db = new ApplicationContext())
            {
                var urlSetting = db.Settings.FirstOrDefault(t => t.c_key == "C_URL");

                BotChannelIdentity identity = turnContext.Activity.GetIdentity();
                string replyText = "��������� ��������, �� ���������� �� ������.";
                var setting = db.Settings.FirstOrDefault(t => t.c_key == "C_BOT_URL");
                if (identity.IsAuthenticated)
                {
                    var answer = db.Search(turnContext.Activity.Text);
                    if (answer == null)
                    {
                        replyText = "��� ����������, ���������� �������������� ������ �����.";
                    }
                    else
                    {
                        HttpResult httpResult = null;
                        if (answer.Action == "API") {
                            httpResult = BotExtension.Get(urlSetting.c_value + answer.Url, identity.TfsToken);
                        }

                        if (httpResult.Status == System.Net.HttpStatusCode.Unauthorized)
                        {
                            replyText = "��� ����������� ������ ��������� ������� �� <a href=\"" + setting.c_value + "?token=" + identity.AuthorizeToken + "\">������</a> � ��������� ����������� �� ������� TFS.";
                        }
                        else
                        {
                            replyText = httpResult.Result;
                        }
                    }
                }
                else
                {
                    if (identity.IsActive)
                    {        
                        replyText = "��� ����������� ������ ����� ������� �� <a href=\"" + setting.c_value + "?token=" + identity.AuthorizeToken + "\">������</a> � ��������� �����������.";
                    } else
                    {
                        Regex regex = new Regex(@"^\d{6}$");
                        if (regex.IsMatch(turnContext.Activity.Text))
                        {
                            int pin = int.Parse(regex.Match(turnContext.Activity.Text).Value);
                            if (db.Registry(identity, pin, turnContext.Activity.ServiceUrl))
                            {
                                identity.UpdateIdentity(turnContext.Activity);

                                replyText = "�������! ���� ������.<br />������ ����� ��������� �������������� �� ������� TFS � ��� ����� ��������� ������� �� <a href=\"" + setting.c_value + "?token=" + identity.AuthorizeToken + "\">������</a>.";
                            }
                        } else {
                            replyText = "������������! ���������� � <b>" + turnContext.Activity.From.Name + "</b> ����������� � ���� ������.<br />��� ������ ����������� ��������� ������� ����.";
                        }
                    }
                }

                await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
            }
            
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "�����������! � ���-��� ������� <b>vNext</b>.<br />��� �������� ������ �������������� ������ � TFS.";
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
