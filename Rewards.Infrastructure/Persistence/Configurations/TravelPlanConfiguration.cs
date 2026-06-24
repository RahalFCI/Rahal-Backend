using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rewards.Domain.Entities;

namespace Rewards.Infrastructure.Persistence.Configurations
{
    public class TravelPlanConfiguration : IEntityTypeConfiguration<TravelPlan>
    {
        public void Configure(EntityTypeBuilder<TravelPlan> builder)
        {
            builder.ToTable("TravelPlans", "rewards");
            builder.HasKey(t => t.Id);
            builder.HasQueryFilter(t => !t.IsDeleted);

            builder.Property(t => t.BudgetLimit).HasPrecision(18, 2).IsRequired();
            builder.Property(t => t.StayDurationDays).IsRequired();
            builder.Property(t => t.Prompt).IsRequired().HasMaxLength(2000);
            builder.Property(t => t.GeneratedPlanJson).HasColumnType("jsonb").IsRequired();
            builder.Property(t => t.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();
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
