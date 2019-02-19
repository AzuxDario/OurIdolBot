using Microsoft.EntityFrameworkCore;
using OurIdolBot.Database.Models.DynamicDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace OurIdolBot.Database.Models
{
    class DynamicDBContext : DbContext
    {
        public virtual DbSet<Server> Servers { get; set; }
        public virtual DbSet<AssignRole> AssignRoles { get; set; }
        public DynamicDBContext() : base(GetOptions("Data Source=DynamicDatabase.sqlite"))
        {

        }

        private static DbContextOptions GetOptions(string connectionString)
        {
            return SqliteDbContextOptionsBuilderExtensions.UseSqlite(new DbContextOptionsBuilder(), connectionString).Options;
        }

    }
}
