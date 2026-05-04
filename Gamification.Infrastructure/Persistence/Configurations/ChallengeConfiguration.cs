using Gamification.Domain.Entities;
using Gamification.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Infrastructure.Persistence.Configurations
{
    public class ChallengeConfiguration : IEntityTypeConfiguration<Challenge>
    {
        public void Configure(EntityTypeBuilder<Challenge> builder)
        {
            builder.ToTable("Challenges", "gamification");
            // Primary Key (composite)
            builder.HasKey(e => e.Id);

            // Query filter for soft deletion
            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.Property(e => e.PlaceId)
                .IsRequired();

            builder.Property(e => e.Name)
                .IsRequired();

            builder.Property(e => e.XpReward)
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(e => e.MinimumLevelRequired)
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(e => e.Difficulty)
                .IsRequired();

            builder.Property(e => e.Type)
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

            // Indexes
            builder.HasIndex(e => e.Difficulty)
                .HasDatabaseName("IX_Challenges_Difficulty");

            builder.HasIndex(e => e.Type)
                .HasDatabaseName("IX_Challenges_Type");

            builder.HasIndex(e => e.Name)
                .HasDatabaseName("IX_Challenges_Name");
        }
    }
}
