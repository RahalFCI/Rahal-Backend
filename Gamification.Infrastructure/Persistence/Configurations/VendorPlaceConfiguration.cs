using Gamification.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gamification.Infrastructure.Persistence.Configurations
{
    public class VendorPlaceConfiguration : IEntityTypeConfiguration<VendorPlace>
    {
        public void Configure(EntityTypeBuilder<VendorPlace> builder)
        {
            builder.ToTable("VendorPlaces", "gamification");

            builder.HasKey(e => e.Id);

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

            builder.Property(e => e.IsPrimary)
                .HasDefaultValue(false);

            builder.Property(e => e.IsActive)
                .HasDefaultValue(true);

            builder.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.UpdatedAt)
                .ValueGeneratedOnUpdate();

            builder.Property(e => e.DeletedAt);

            builder.Property(e => e.IsDeleted)
                .HasDefaultValue(false);

            builder.HasIndex(e => e.VendorId)
                .HasDatabaseName("IX_VendorPlaces_VendorId");

            builder.HasIndex(e => e.PlaceId)
                .IsUnique()
                .HasDatabaseName("IX_VendorPlaces_PlaceId");

            builder.HasIndex(e => new { e.VendorId, e.IsPrimary })
                .IsUnique()
                .HasFilter("\"IsPrimary\" = true AND \"IsDeleted\" = false")
                .HasDatabaseName("IX_VendorPlaces_OnePrimaryPerVendor");
        }
    }
}
