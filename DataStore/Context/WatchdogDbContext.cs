using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataStore.Entity;

namespace DataStore.Context
{
    public class WatchdogDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Server> Servers { get; set; }
        public DbSet<Deal> Deals { get; set; }
        public DbSet<Symbol> Symbols { get; set; }
        public DbSet<DealGroup> DealGroups { get; set; }
    }
}
