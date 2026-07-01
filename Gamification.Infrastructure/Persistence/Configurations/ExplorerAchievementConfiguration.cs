using Shared.Infrastructure.Persistence.Configurations;
using Gamification.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Infrastructure.Persistence.Configurations
{
    public class ExplorerAchievementConfiguration : BaseAuditableEntityConfiguration<ExplorerAchievement>
    {
        public override void Configure(EntityTypeBuilder<ExplorerAchievement> builder)
        {
            base.Configure(builder);

            builder.ToTable("ExplorerAchievement", "gamification");

            // Query filter for soft deletion
            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.Property(e => e.ExplorerId)
                .IsRequired();

            builder.Property(e => e.AchievementId)
                .IsRequired();


            // Relationships
            builder.HasOne(e => e.Achievement)
                .WithMany(c => c.ExplorerAchievements)
                .HasForeignKey(e => e.AchievementId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.ExplorerProfile)
                .WithMany(c => c.ExplorerAchievements)
                .HasForeignKey(e => e.ExplorerId)
                .OnDelete(DeleteBehavior.Cascade);


            // Audit Properties (inherited from BaseEntity)

            builder.Property(e => e.UpdatedAt)
                .ValueGeneratedOnUpdate();

            builder.Property(e => e.DeletedAt);

            builder.Property(e => e.IsDeleted)
                .HasDefaultValue(false);

            // Indexes
            builder.HasIndex(e => e.ExplorerId)
                .HasDatabaseName("IX_ExplorerAchievements_ExplorerProfileId");

            builder.HasIndex(e => e.AchievementId)
                .HasDatabaseName("IX_ExplorerAchievements_AchievementId");
        }
    }
}
