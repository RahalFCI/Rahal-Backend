using Microsoft.EntityFrameworkCore;
using Rewards.Domain.Entities;
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
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<UserCoupon> UserCoupons { get; set; }
        public DbSet<PlanTier> PlanTiers { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<TravelPlan> TravelPlans { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("rewards");
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(RewardsDbContext).Assembly);
        }
    }
}
