using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.Domain.Entities;

namespace Users.Infrastructure.Persistence.Configuration
{

    public class VendorProfileConfiguration : IEntityTypeConfiguration<VendorProfile>
    {
        public void Configure(EntityTypeBuilder<VendorProfile> builder)
        {
            builder.ToTable("VendorProfiles", "users");

            builder.HasKey(e => e.Id);

            // Foreign key to User
            builder.HasOne(e => e.User)
                .WithOne(u => u.VendorProfile)
                .HasForeignKey<VendorProfile>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Audit Properties (inherited from BaseAuditableEntity)
            builder.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.UpdatedAt)
                .ValueGeneratedOnUpdate();

            builder.Property(e => e.DeletedAt);

            builder.Property(e => e.IsDeleted)
                .HasDefaultValue(false);

            // Domain Properties
            builder.Property(e => e.CountryCode)
                .IsRequired()
                .HasMaxLength(2);

            builder.Property(e => e.Address)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.AddressUrl)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(e => e.WorkingHours)
                .IsRequired()
                .HasColumnType("jsonb")
                .HasComment("JSON format: {\"Monday\": \"09:00-17:00\", ...}");

            builder.Property(e => e.IsApproved)
                .HasDefaultValue(false);

            // Foreign key to VendorCategory
            builder.HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(e => e.UserId)
                .IsUnique();

            builder.HasIndex(e => e.CategoryId);

            builder.HasIndex(e => e.IsApproved);

            builder.HasIndex(e => e.IsDeleted)
                .HasDatabaseName("IX_VendorProfiles_IsDeleted");
        }
    }
}
