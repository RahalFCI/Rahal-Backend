using Gamification.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Infrastructure.Persistence.Configurations
{
    public class CheckInChallengeConfiguration : IEntityTypeConfiguration<CheckInChallenge>
    {
        public void Configure(EntityTypeBuilder<CheckInChallenge> builder)
        {
            builder.ToTable("CheckInChallenges", "gamification");
            // Primary Key (composite)
            builder.HasKey(e => e.Id);

            // Query filter for soft deletion
            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.Property(e => e.ProofUrl)
                .IsRequired();

            builder.Property(e => e.ChallengeId)
                .IsRequired();

            builder.Property(e => e.CheckInId)
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(e => e.ValidationStatus)
                .HasDefaultValue(0)
                .IsRequired();

            // Relationships
            builder.HasOne(e => e.Challenge)
                .WithMany(c => c.CheckInChallenges)
                .HasForeignKey(e => e.ChallengeId)
                .OnDelete(DeleteBehavior.Cascade);


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
            builder.HasIndex(e => e.ValidationStatus)
                .HasDatabaseName("IX_CheckInChallenges_ValidationStatus");

            builder.HasIndex(e => e.CheckInId)
                .HasDatabaseName("IX_CheckInChallenges_CheckInId");

            builder.HasIndex(e => e.ChallengeId)
                .HasDatabaseName("IX_CheckInChallenges_ChallengeId");
        }
    }
}
