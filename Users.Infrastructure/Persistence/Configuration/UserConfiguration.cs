using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.Domain.Entities;
using Users.Domain.Entities._Common;
using Users.Domain.Enums;

namespace Users.Infrastructure.Persistence.Configuration
{

    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("AspNetUsers", "users");

            // Properties
            builder.Property(e => e.DisplayName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.ProfilePictureURL)
                .HasMaxLength(500);

            builder.Property(e => e.UserType)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(e => e.RefreshToken)
                .HasMaxLength(500);

            // Indexes
            builder.HasIndex(e => e.UserType)
                .HasDatabaseName("IX_UserType");

            // Navigation properties
            builder.HasOne(e => e.ExplorerProfile)
                .WithOne(p => p.User)
                .HasForeignKey<ExplorerProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.VendorProfile)
                .WithOne(p => p.User)
                .HasForeignKey<VendorProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.AdminProfile)
                .WithOne(p => p.User)
                .HasForeignKey<AdminProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
