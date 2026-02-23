using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rewards.Infrastructure.Persistence
{
    public class RewardsDbContext : DbContext
    {
        public RewardsDbContext(DbContextOptions<RewardsDbContext> options) : base(options)
        {
        }
        //TODO: DbSets to be added and make its configuration file

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("rewards");
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(RewardsDbContext).Assembly);
        }
    }
}
