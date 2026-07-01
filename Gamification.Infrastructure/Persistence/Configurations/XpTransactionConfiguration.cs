using Shared.Infrastructure.Persistence.Configurations;
using Gamification.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Infrastructure.Persistence.Configurations
{
    public class XpTransactionConfiguration : BaseAuditableEntityConfiguration<XpTransaction>
    {
        public override void Configure(EntityTypeBuilder<XpTransaction> builder)
        {
            base.Configure(builder);

            builder.ToTable("XpTransactions", "gamification");

            // Query filter for soft deletion
            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.Property(e => e.ExplorerProfileId)
                .IsRequired();

            builder.Property(e => e.Source)
                .IsRequired();

            builder.Property(e => e.Amount)
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(e => e.ReferenceId)
                .IsRequired();


            // Relationships

            builder.HasOne(e => e.ExplorerProfile)
                .WithMany(c => c.XpTransactions)
                .HasForeignKey(e => e.ExplorerProfileId)
                .OnDelete(DeleteBehavior.Cascade);


            // Audit Properties (inherited from BaseEntity)

            builder.Property(e => e.UpdatedAt)
                .ValueGeneratedOnUpdate();

            builder.Property(e => e.DeletedAt);

            builder.Property(e => e.IsDeleted)
                .HasDefaultValue(false);

            // Indexes
            builder.HasIndex(e => e.ExplorerProfileId)
                .HasDatabaseName("IX_XpTransactions_ExplorerProfileId");

            builder.HasIndex(x => new
            {
                x.ExplorerProfileId,
                x.Source,
                x.ReferenceId
            }).IsUnique();

        }
    }
}
