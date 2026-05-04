using Gamification.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Infrastructure.Persistence.Configurations
{
    public class ExplorerAchievementConfiguration : IEntityTypeConfiguration<ExplorerAchievement>
    {
        public void Configure(EntityTypeBuilder<ExplorerAchievement> builder)
        {
            builder.ToTable("ExplorerAchievement", "gamification");
            // Primary Key (composite)
            builder.HasKey(e => e.Id);

            // Query filter for soft deletion
            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.Property(e => e.ExplorerProfileId)
                .IsRequired();

            builder.Property(e => e.AchievementId)
                .IsRequired();


            // Relationships
            builder.HasOne(e => e.Achievement)
                .WithMany(c => c.ExplorerAchievements)
                .HasForeignKey(e => e.AchievementId)
                .OnDelete(DeleteBehavior.Cascade);

            //TODO: add relationship with ExplorerProfile


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

            builder.HasIndex(e => e.AchievementId)
                .HasDatabaseName("IX_ExplorerAchievements_AchievementId");
        }
    }
}
