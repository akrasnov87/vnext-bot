using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace vNextBot.Bots
{
    public class HttpResult
    {
        public HttpResult(HttpStatusCode status)
        {
            Status = status;
            Result = "Сервис не вернул результат.";
        }
        public string Result { get; set; }
        public HttpStatusCode Status { get; set; }

        public void SetFTS()
        {
            Result += "<br />_Ответ найден с помощью полнотекстового поиска_";
        }

        public bool IsAuthorize
        {
            get
            {
                return Status != HttpStatusCode.Unauthorized;
            }
        }
    }
}
