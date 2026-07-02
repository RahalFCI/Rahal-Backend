using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rewards.Domain.Entities;
using Shared.Infrastructure.Persistence.Configurations;

namespace Rewards.Infrastructure.Persistence.Configurations
{
    public class CouponConfiguration : BaseAuditableEntityConfiguration<Coupon>
    {
        public override void Configure(EntityTypeBuilder<Coupon> builder)
        {
            base.Configure(builder);

            builder.ToTable("Coupons", "rewards");
            builder.HasQueryFilter(c => !c.IsDeleted);

            builder.Property(c => c.Title).IsRequired().HasMaxLength(150);
            builder.Property(c => c.Description).HasMaxLength(1000);
            builder.Property(c => c.XpCost).IsRequired();
            builder.Property(c => c.DiscountType).HasConversion<string>().IsRequired();
            builder.Property(c => c.DiscountValue).HasPrecision(18, 2).IsRequired();
            builder.Property(c => c.MaxDiscountValue).HasPrecision(18, 2);
            builder.Property(c => c.MinimumCharge).HasPrecision(18, 2).IsRequired();
            builder.Property(c => c.MaxClaims).IsRequired();
            builder.Property(c => c.CurrentClaims).IsRequired().HasDefaultValue(0);
            builder.Property(c => c.ExpiresAt).IsRequired();
            builder.Property(c => c.IsActive).IsRequired().HasDefaultValue(true);
            builder.Property(c => c.UpdatedAt).ValueGeneratedOnUpdate();
            builder.Property(c => c.IsDeleted).HasDefaultValue(false);

            builder.HasIndex(c => c.VendorId).HasDatabaseName("IX_Coupons_VendorId");
            builder.HasIndex(c => c.ExpiresAt).HasDatabaseName("IX_Coupons_ExpiresAt");
            builder.HasIndex(c => c.IsActive).HasDatabaseName("IX_Coupons_IsActive");
            builder.HasIndex(c => c.DiscountType).HasDatabaseName("IX_Coupons_DiscountType");
        }
    }
}
