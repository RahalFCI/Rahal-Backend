using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Domain.Entities;
using Users.Domain.Enums;

namespace Users.Infrastructure.Persistence.Configuration
{
    public class ExplorerConfiguration : IEntityTypeConfiguration<Explorer>
    {
        public void Configure(EntityTypeBuilder<Explorer> builder)
        {
            // Table configuration
            builder.ToTable("Explorers", "users");

            // Primary Key
            builder.HasKey(e => e.Id);

            // Required fields
            builder.Property(e => e.DisplayName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.CountryCode)
                .IsRequired()
                .HasMaxLength(2);

            builder.Property(e => e.Gender)
                .IsRequired();

            builder.Property(e => e.BirthDate)
                .IsRequired();

            builder.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(256);

            builder.HasIndex(u => u.Email).IsUnique();

            builder.Property(e => e.UserName)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(e => e.PhoneNumber)
                .HasMaxLength(20);

            // Optional fields with constraints
            builder.Property(e => e.Bio)
                .HasMaxLength(500)
                .HasDefaultValue(string.Empty);

            builder.Property(e => e.ProfilePictureURL)
                .HasMaxLength(500)
                .HasDefaultValue(string.Empty);

            // Default values for XP and level
            builder.Property(e => e.AvailableXp)
                .HasDefaultValue(0);

            builder.Property(e => e.CumaltiveXp)
                .HasDefaultValue(0);

            builder.Property(e => e.Level)
                .HasDefaultValue(1);

            builder.Property(e => e.IsPublic)
                .HasDefaultValue(true);

            builder.Property(e => e.IsPremium)
                .HasDefaultValue(false);

            builder.Property(e => e.Role)
                .HasConversion<int>()
                .HasDefaultValue(UserRoleEnum.Explorer);

            // Ignore computed property
            builder.Ignore(e => e.Age);
        }
    }
}
