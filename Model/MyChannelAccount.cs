using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace vNextBot.Model
{
    [Table("sd_channel_accounts", Schema = "dbo")]
    public class MyChannelAccount
    {
        [Key]
        public int id { get; set; }

        public string account_id { get; set; }
        public string account_name { get; set; }
        public string c_type { get; set; }
        public int f_user { get; set; }
    }
}
