using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using vNextBot.Bots;
using vNextBot.Model;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace vNextBot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizeController : ControllerBase
    {
        [HttpGet]
        public string Get(string token)
        {
            var base64EncodedBytes = Convert.FromBase64String(token);
            string data = Encoding.UTF8.GetString(base64EncodedBytes);
            string[] parts = data.Split("|");
            using(ApplicationContext db = new ApplicationContext())
            {
                if(db.Users.Any(t=>t.id == int.Parse(parts[0]) && !t.b_disabled) &&
                    db.ChannelAccounts.Any(t => t.account_id == parts[1] && t.c_type == parts[2])) {
                    return "SUCCESS";
                } else
                {
                    return "FAIL";
                }
            }
        }

        [HttpPost]
        public RedirectResult Post()
        {
            if(HttpContext.Request.Form.Count() == 3)
            {
                string login = HttpContext.Request.Form["login"];
                string password = HttpContext.Request.Form["password"];
                string token = HttpContext.Request.Form["token"];

                using (ApplicationContext db = new ApplicationContext())
                {
                    var setting = db.Settings.FirstOrDefault(t => t.c_key == "C_URL");
                    var tfsUrl = db.Settings.FirstOrDefault(t => t.c_key == "C_TFS_URL").c_value;

                    User user;

                    var base64EncodedBytes = Convert.FromBase64String(token);
                    string data = Encoding.UTF8.GetString(base64EncodedBytes);
                    string[] parts = data.Split("|");

                    if (db.Users.Any(t => t.id == int.Parse(parts[0]) && !t.b_disabled) &&
                    db.ChannelAccounts.Any(t => t.account_id == parts[1] && t.c_type == parts[2]))
                    {
                        user = db.Users.FirstOrDefault(t => t.id == int.Parse(parts[0]));
                    }
                    else
                    {
                        return Redirect("~/?authorize=FAIL&txt=No matches found in the database");
                    }

                    var tfsToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format("{0}|{1}|{2}|{3}|{4}", tfsUrl, user.c_project, user.c_domain, login, password)));

                    var httpResult = BotExtension.Get(string.Format("{0}/v1/projects", setting.c_value), tfsToken);
                    if(httpResult.IsAuthorize)
                    {
                        user.c_login = login;
                        user.c_password = Convert.ToBase64String(Encoding.UTF8.GetBytes(password));
                        db.Users.Update(user);
                        db.SaveChanges();
                    } else
                    {
                        return Redirect("~/?token=" + token + "&authorize=FAIL&txt=The user was not logged in to the server");
                    }
                } 

                return Redirect("~/?authorize=SUCCESS");
            }

            return Redirect("~/?authorize=FAIL&txt=Token, Username, or password not passed");
        }
    }
}
