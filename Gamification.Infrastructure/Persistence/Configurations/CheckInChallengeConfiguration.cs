using Shared.Infrastructure.Persistence.Configurations;
using Gamification.Domain.Entities;
using Gamification.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Infrastructure.Persistence.Configurations
{
    public class CheckInChallengeConfiguration : BaseAuditableEntityConfiguration<CheckInChallenge>
    {
        public override void Configure(EntityTypeBuilder<CheckInChallenge> builder)
        {
            base.Configure(builder);

            builder.ToTable("CheckInChallenges", "gamification");

            // Query filter for soft deletion
            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.Property(e => e.ProofUrl)
                .IsRequired();

            builder.Property(e => e.ChallengeId)
                .IsRequired();

            builder.Property(e => e.CheckInId)
                .IsRequired();

            builder.Property(e => e.ExplorerId)
                .IsRequired();

            builder.Property(e => e.ValidationStatus)
                .HasDefaultValue(ChallengeValidationStatus.Pending)
                .IsRequired();

            // Relationships
            builder.HasOne(e => e.Challenge)
                .WithMany(c => c.CheckInChallenges)
                .HasForeignKey(e => e.ChallengeId)
                .OnDelete(DeleteBehavior.Cascade);


            // Audit Properties (inherited from BaseEntity)

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
