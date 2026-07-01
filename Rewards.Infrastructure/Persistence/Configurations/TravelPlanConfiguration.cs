using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rewards.Domain.Entities;
using Shared.Infrastructure.Persistence.Configurations;

namespace Rewards.Infrastructure.Persistence.Configurations
{
    public class TravelPlanConfiguration : BaseAuditableEntityConfiguration<TravelPlan>
    {
        public override void Configure(EntityTypeBuilder<TravelPlan> builder)
        {
            base.Configure(builder);

            builder.ToTable("TravelPlans", "rewards");
            builder.HasQueryFilter(t => !t.IsDeleted);

            builder.Property(t => t.StayDurationDays).IsRequired();
            builder.Property(t => t.Prompt).IsRequired().HasMaxLength(2000);
            builder.Property(t => t.GeneratedPlanJson).HasColumnType("jsonb").IsRequired();
            builder.Property(t => t.UpdatedAt).ValueGeneratedOnUpdate();
            builder.Property(t => t.IsDeleted).HasDefaultValue(false);

            builder.HasOne(t => t.Subscription)
                .WithMany()
                .HasForeignKey(t => t.SubscriptionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(t => t.ExplorerId).HasDatabaseName("IX_TravelPlans_ExplorerId");
            builder.HasIndex(t => t.SubscriptionId).HasDatabaseName("IX_TravelPlans_SubscriptionId");
        }
    }
}
