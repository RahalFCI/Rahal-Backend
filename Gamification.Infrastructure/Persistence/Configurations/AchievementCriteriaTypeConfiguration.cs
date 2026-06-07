using Gamification.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Infrastructure.Persistence.Configurations
{
    public class AchievementCriteriaTypeConfiguration : IEntityTypeConfiguration<AchievementCriteriaType>
    {
        public void Configure(EntityTypeBuilder<AchievementCriteriaType> builder)
        {
            builder.ToTable("AchievementCriteriaTypes", "gamification");

            // Primary Key (composite)
            builder.HasKey(e => e.Id);

            // Query filter for soft deletion
            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.Property(e => e.Name)
                .IsRequired();

            builder.Property(e => e.Code)
                .IsRequired();

            // Large string mapped to TEXT type in database
            builder.Property(e => e.Description)
                .IsRequired()
                .HasColumnType("text");

            // Audit Properties (inherited from BaseEntity)
            builder.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.UpdatedAt)
                .ValueGeneratedOnUpdate();

            builder.Property(e => e.DeletedAt);

            builder.Property(e => e.IsDeleted)
                .HasDefaultValue(false);

            //Indexes
            builder.HasIndex(e => e.Name)
                .IsUnique();




        }
    }
}
