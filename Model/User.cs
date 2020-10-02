using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace vNextBot.Model
{
    [Table("pd_users", Schema = "dbo")]
    public class User
    {
        [Key]
        public int id { get; set; }

        public string c_login { get; set; }
        public string c_password { get; set; }
        public string c_fio { get; set; }
        public string c_description { get; set; }
        public bool b_disabled { get; set; }
        public string c_project { get; set; }
        public Guid? project_id { get; set; }
        public string c_domain { get; set; }
        public int n_pin { get; set; }
    }
}
