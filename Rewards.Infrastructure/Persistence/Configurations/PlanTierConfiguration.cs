using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rewards.Domain.Entities;
using Shared.Infrastructure.Persistence.Configurations;

namespace Rewards.Infrastructure.Persistence.Configurations
{
    public class PlanTierConfiguration : BaseAuditableEntityConfiguration<PlanTier>
    {
        public override void Configure(EntityTypeBuilder<PlanTier> builder)
        {
            base.Configure(builder);

            builder.ToTable("PlanTiers", "rewards");
            builder.HasQueryFilter(p => !p.IsDeleted);

            builder.Property(p => p.Name).IsRequired().HasMaxLength(80);
            builder.Property(p => p.Description).HasMaxLength(500);
            builder.Property(p => p.WeeklyPrice).HasPrecision(18, 2).IsRequired();
            builder.Property(p => p.WeeklyXpCost).IsRequired();
            builder.Property(p => p.XpMultiplier).HasPrecision(8, 2).IsRequired();
            builder.Property(p => p.MaxTravelPlans).IsRequired();
            builder.Property(p => p.IsActive).HasDefaultValue(true).IsRequired();
            builder.Property(p => p.UpdatedAt).ValueGeneratedOnUpdate();
            builder.Property(p => p.IsDeleted).HasDefaultValue(false);

            builder.HasIndex(p => p.Name).IsUnique().HasDatabaseName("IX_PlanTiers_Name");
            builder.HasIndex(p => p.IsActive).HasDatabaseName("IX_PlanTiers_IsActive");
        }
    }
}
