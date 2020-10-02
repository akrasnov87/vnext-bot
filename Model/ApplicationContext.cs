using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using vNextBot.Bots;

namespace vNextBot.Model
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Server=192.168.1.53;Port=5432;Database=vnext-bot;Username=mobnius;Password=mobnius-0");
        }

        public DbSet<MyChannelAccount> ChannelAccounts { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Setting> Settings { get; set; }

        public bool IsUserExists(string channel_account_id)
        {
            using (var command = Database.GetDbConnection().CreateCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "dbo.cf_ui_user_exists";
                command.Parameters.Add(new Npgsql.NpgsqlParameter("_channel_id", NpgsqlTypes.NpgsqlDbType.Text)
                { Value = channel_account_id });
                if (command.Connection.State == ConnectionState.Closed)
                    command.Connection.Open();
                var res = (int)command.ExecuteScalar();
                return res == 0;
            }
        }

        public bool Registry(BotChannelIdentity identity, int pin)
        {
            if(Users.Any(t=>t.n_pin == pin && t.b_disabled))
            {
                // пин-код совпал и запись отключена
                User user = Users.FirstOrDefault(t => t.n_pin == pin && t.b_disabled);
                if (user != null)
                {
                    user.c_fio = identity.Name;
                    user.b_disabled = false;
                    Users.Update(user);

                    ChannelAccounts.Add(new MyChannelAccount()
                    {
                        f_user = user.id,
                        c_type = identity.AuthenticationType,
                        account_id = identity.Id,
                        account_name = identity.Name
                    });

                    SaveChanges();
                    return true;
                }
            }

            return false;
        }
    }
}