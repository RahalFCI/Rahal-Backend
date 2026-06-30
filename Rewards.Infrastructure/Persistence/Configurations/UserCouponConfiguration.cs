using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rewards.Domain.Entities;
using Shared.Infrastructure.Persistence.Configurations;

namespace Rewards.Infrastructure.Persistence.Configurations
{
    public class UserCouponConfiguration : BaseAuditableEntityConfiguration<UserCoupon>
    {
        public override void Configure(EntityTypeBuilder<UserCoupon> builder)
        {
            base.Configure(builder);

            builder.ToTable("UserCoupons", "rewards");
            builder.HasQueryFilter(c => !c.IsDeleted);

            builder.Property(c => c.Code).IsRequired().HasMaxLength(64);
            builder.Property(c => c.Status).HasConversion<string>().IsRequired();
            builder.Property(c => c.ClaimedAt).IsRequired();
            builder.Property(c => c.ExpiresAt).IsRequired();
            builder.Property(c => c.IsRedeemed).IsRequired().HasDefaultValue(false);
            builder.Property(c => c.UpdatedAt).ValueGeneratedOnUpdate();
            builder.Property(c => c.IsDeleted).HasDefaultValue(false);

            builder.HasOne(c => c.Coupon)
                .WithMany(c => c.UserCoupons)
                .HasForeignKey(c => c.CouponId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(c => c.Code).IsUnique().HasDatabaseName("IX_UserCoupons_Code");
            builder.HasIndex(c => new { c.ExplorerId, c.CouponId }).IsUnique().HasDatabaseName("IX_UserCoupons_ExplorerId_CouponId");
            builder.HasIndex(c => c.Status).HasDatabaseName("IX_UserCoupons_Status");
        }
    }
}
