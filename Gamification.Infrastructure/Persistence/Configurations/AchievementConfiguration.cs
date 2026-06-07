using Gamification.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Infrastructure.Persistence.Configurations
{
    public class AchievementConfiguration : IEntityTypeConfiguration<Achievement>
    {
        public void Configure(EntityTypeBuilder<Achievement> builder)
        {
            builder.ToTable("Achievements", "gamification");

            // Primary Key (composite)
            builder.HasKey(e => e.Id);

            // Query filter for soft deletion
            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.Property(e => e.AchievementCriteriaTypeId)
                .IsRequired();

            builder.Property(e => e.CriteriaThreshold)
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(e => e.Title)
                .IsRequired();

            builder.Property(e => e.Xp)
                .HasDefaultValue(0)
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


            //Foreign Keys

            builder.HasOne(e => e.Badge)
                .WithMany()
                .HasForeignKey(e => e.BadgeId)
                .OnDelete(DeleteBehavior.Cascade);


            //Indexes

            builder.HasIndex(e => e.AchievementCriteriaTypeId)
                .HasDatabaseName("IX_Achievements_AchievementCriteriaTypeId");

            builder.HasIndex(e => e.Title)
                .IsUnique();
        }
    }
}
