using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Infrastructure.Persistence
{
    public class GamificationDbContext : DbContext
    {
        public GamificationDbContext(DbContextOptions<GamificationDbContext> options) : base(options)
        {
        }
        //TODO: DbSets to be added and make its configuration file
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("gamification");
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(GamificationDbContext).Assembly);
        }
    }
}
