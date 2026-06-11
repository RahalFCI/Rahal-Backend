using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rewards.Domain.Entities;

namespace Rewards.Infrastructure.Persistence.Configurations
{
    public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
    {
        public void Configure(EntityTypeBuilder<Subscription> builder)
        {
            builder.ToTable("Subscriptions", "rewards");
            builder.HasKey(s => s.Id);
            builder.HasQueryFilter(s => !s.IsDeleted);

            builder.Property(s => s.PaymentMethod).HasConversion<string>().IsRequired();
            builder.Property(s => s.Status).HasConversion<string>().IsRequired();
            builder.Property(s => s.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();
            builder.Property(s => s.UpdatedAt).ValueGeneratedOnUpdate();
            builder.Property(s => s.IsDeleted).HasDefaultValue(false);

            builder.HasOne(s => s.PlanTier)
                .WithMany(p => p.Subscriptions)
                .HasForeignKey(s => s.PlanTierId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(s => s.ExplorerId).HasDatabaseName("IX_Subscriptions_ExplorerId");
            builder.HasIndex(s => s.Status).HasDatabaseName("IX_Subscriptions_Status");
            builder.HasIndex(s => s.ExpiresAt).HasDatabaseName("IX_Subscriptions_ExpiresAt");
            builder.HasIndex(s => new { s.ExplorerId, s.Status }).HasDatabaseName("IX_Subscriptions_ExplorerId_Status");
        }
    }
}
