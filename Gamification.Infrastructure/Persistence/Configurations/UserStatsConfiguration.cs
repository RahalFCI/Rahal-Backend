using Gamification.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Infrastructure.Persistence.Configurations
{
    public class UserStatsConfiguration : IEntityTypeConfiguration<UserStats>
    {
        public void Configure(EntityTypeBuilder<UserStats> builder)
        {
            builder.ToTable("UserStats", "gamification");
            // Primary Key
            builder.HasKey(e => e.Id);

            // Query filter for soft deletion
            builder.HasQueryFilter(e => !e.IsDeleted);


            builder.Property(e => e.TotalBadgeCount)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(e => e.TotalAchievementCount)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(e => e.TotalChallengeCount)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(e => e.TotalCheckInCount)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(e => e.LongestStreak)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(e => e.ExplorerProfileId)
                .IsRequired();

            // Audit Properties (inherited from BaseEntity)
            builder.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.UpdatedAt)
                .ValueGeneratedOnUpdate();

            builder.Property(e => e.DeletedAt);

            builder.Property(e => e.IsDeleted)
                .HasDefaultValue(false);

            // Indexes
            builder.HasIndex(e => e.ExplorerProfileId)
                .HasDatabaseName("IX_ExplorerAchievements_ExplorerProfileId");
        }
    }
}