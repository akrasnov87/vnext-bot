using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace vNextBot.Model
{
    [Table("cd_settings", Schema = "dbo")]
    public class Setting
    {
        [Key]
        public int id { get; set; }

        public string c_key { get; set; }
        public string c_value { get; set; }
        public int f_type { get; set; }
        public string c_summary { get; set; }
    }
}
