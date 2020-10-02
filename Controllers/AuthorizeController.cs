using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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

        // POST api/<CheckTokenController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }
    }
}
