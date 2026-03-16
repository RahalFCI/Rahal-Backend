using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Domain.Entities;
using Users.Domain.Enums;

namespace Users.Infrastructure.Persistence.Configuration
{
    public class VendorConfiguration : IEntityTypeConfiguration<Vendor>
    {
        public void Configure(EntityTypeBuilder<Vendor> builder)
        {
            // Table configuration
            builder.ToTable("Vendors", "users");

            // Primary Key
            builder.HasKey(v => v.Id);

            // Required fields
            builder.Property(v => v.DisplayName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(v => v.CountryCode)
                .IsRequired()
                .HasMaxLength(2);

            builder.Property(v => v.Address)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(v => v.AddressUrl)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(v => v.WorkingHours)
                .IsRequired()
                .HasColumnType("jsonb");

            builder.Property(v => v.CategoryId)
                .IsRequired();

            builder.Property(v => v.Email)
                .IsRequired()
                .HasMaxLength(256);
                
            builder.HasIndex(u => u.Email).IsUnique();

            builder.Property(v => v.UserName)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(v => v.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);

            // Optional fields
            builder.Property(v => v.ProfilePictureURL)
                .HasMaxLength(500)
                .HasDefaultValue(string.Empty);

            // Default values
            builder.Property(v => v.IsApproved)
                .HasDefaultValue(false);

            builder.Property(v => v.Role)
                .HasConversion<int>()
                .HasDefaultValue(UserRoleEnum.Vendor);

            // Relationships
            builder.HasOne(v => v.Category)
                .WithMany()
                .HasForeignKey(v => v.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
