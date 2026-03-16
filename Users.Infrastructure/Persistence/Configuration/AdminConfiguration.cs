using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Domain.Entities;
using Users.Domain.Enums;

namespace Users.Infrastructure.Persistence.Configuration
{
    public class AdminConfiguration : IEntityTypeConfiguration<Admin>
    {
        public void Configure(EntityTypeBuilder<Admin> builder)
        {
            // Table configuration
            builder.ToTable("Admins", "users");

            // Primary Key
            builder.HasKey(a => a.Id);

            // Required fields
            builder.Property(a => a.DisplayName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(a => a.Email)
                .IsRequired()
                .HasMaxLength(256);

            builder.HasIndex(u => u.Email).IsUnique();

            builder.Property(a => a.UserName)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(a => a.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);

            // Optional fields
            builder.Property(a => a.ProfilePictureURL)
                .HasMaxLength(500)
                .HasDefaultValue(string.Empty);

            // Default values
            builder.Property(a => a.Role)
                .HasConversion<int>()
                .HasDefaultValue(UserRoleEnum.Admin);

            // Audit fields
            builder.Property(a => a.RefreshToken)
                .HasMaxLength(500);
        }
    }
}
