using Gamification.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Infrastructure.Persistence.Configurations
{
    public class XpTransactionConfiguration : IEntityTypeConfiguration<XpTransaction>
    {
        public void Configure(EntityTypeBuilder<XpTransaction> builder)
        {
            builder.ToTable("XpTransactions", "gamification");
            // Primary Key (composite)
            builder.HasKey(e => e.Id);

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
                .HasDatabaseName("IX_XpTransactions_ExplorerProfileId");

        }
    }
}
