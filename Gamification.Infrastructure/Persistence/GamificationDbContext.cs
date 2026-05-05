using Gamification.Domain.Entities;
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
        
        public DbSet<Achievement> Achievement { get; set; }
        public DbSet<AchievementCriteriaType> AchievementCriteriaType { get; set; }
        public DbSet<Badge> Badge { get; set; }
        public DbSet<Challenge> Challenge { get; set; }
        public DbSet<CheckInChallenge> CheckInChallenge { get; set; }
        public DbSet<ExplorerAchievement> ExplorerAchievement { get; set; }
        public DbSet<UserStats> UserStats { get; set; }
        public DbSet<XpTransaction> XpTransaction { get; set; }
        public DbSet<ExplorerProfile> ExplorerProfiles { get; set; }
        public DbSet<VendorProfile> VendorProfiles { get; set; }
        public DbSet<AdminProfile> AdminProfiles { get; set; }
        public DbSet<VendorCategory> VendorCategories { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("gamification");
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(GamificationDbContext).Assembly);
        }
    }
}
