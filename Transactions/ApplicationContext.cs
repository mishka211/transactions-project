using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace Transactions
{
    class ApplicationContext : DbContext
    {
        public DbSet<Trade> Trades { get; set; }
        public DbSet<Instrument> Instruments { get; set; }
        public DbSet<TradeResult> TradeResults { get; set; }
        public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<BrokerageAccount> BrokerageAccounts { get; set; }

        public ApplicationContext() : base("DefaultConnection") { }
    }
}
