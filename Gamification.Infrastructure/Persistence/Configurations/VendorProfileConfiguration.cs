using Gamification.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gamification.Infrastructure.Persistence.Configurations
{
    public class VendorProfileConfiguration : IEntityTypeConfiguration<VendorProfile>
    {
        public void Configure(EntityTypeBuilder<VendorProfile> builder)
        {
            builder.ToTable("VendorProfiles", "gamification");

            builder.Property(e => e.DisplayName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.ProfilePictureURL)
                .HasMaxLength(500);

            builder.HasKey(e => e.UserId);

            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.Property(e => e.UserId)
                .IsRequired();

            builder.Property(e => e.CountryCode)
                .IsRequired()
                .HasMaxLength(2);

            builder.Property(e => e.Address)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(e => e.AddressUrl)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(e => e.WorkingHours)
                .IsRequired()
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<DayOfWeek, string>>(v) ?? new());

            builder.Property(e => e.CategoryId)
                .IsRequired();

            builder.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.UpdatedAt)
                .ValueGeneratedOnUpdate();

            builder.Property(e => e.DeletedAt);

            builder.Property(e => e.IsDeleted)
                .HasDefaultValue(false);

            builder.HasIndex(e => e.UserId)
                .IsUnique()
                .HasDatabaseName("IX_VendorProfiles_UserId");

            builder.HasIndex(e => e.CategoryId)
                .HasDatabaseName("IX_VendorProfiles_CategoryId");

            builder.HasIndex(e => e.IsDeleted)
                .HasDatabaseName("IX_VendorProfiles_IsDeleted");
        }
    }
}
