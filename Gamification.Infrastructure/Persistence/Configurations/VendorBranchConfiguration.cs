using Gamification.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations;

namespace Gamification.Infrastructure.Persistence.Configurations
{
    public class VendorBranchConfiguration : BaseAuditableEntityConfiguration<VendorBranch>
    {
        public override void Configure(EntityTypeBuilder<VendorBranch> builder)
        {
            base.Configure(builder);

            builder.ToTable("VendorBranches", "gamification");

            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.Property(e => e.VendorId)
                .IsRequired();

            builder.Property(e => e.PlaceId)
                .IsRequired();

            builder.Property(e => e.BranchName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.PhoneNumber)
                .HasMaxLength(30);

            builder.Property(e => e.Notes)
                .HasMaxLength(500);

            builder.Property(e => e.IsActive)
                .HasDefaultValue(true);

            builder.Property(e => e.UpdatedAt)
                .ValueGeneratedOnUpdate();

            builder.Property(e => e.DeletedAt);

            builder.Property(e => e.IsDeleted)
                .HasDefaultValue(false);

            builder.HasIndex(e => e.VendorId)
                .HasDatabaseName("IX_VendorBranches_VendorId");

            builder.HasIndex(e => e.PlaceId)
                .IsUnique()
                .HasDatabaseName("IX_VendorBranches_PlaceId");
        }
    }
}
