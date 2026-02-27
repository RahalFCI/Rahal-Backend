using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Infrastructure.Persistence.Configurations
{
    public class BaseAuditableEntityConfiguration<TEntity, TKey> : BaseEntityConfiguration<TEntity, TKey>
        where TEntity : BaseAuditableEntity<TKey>
        where TKey : IEquatable<TKey>
    {
        public override void Configure(EntityTypeBuilder<TEntity> builder)
        {
            base.Configure(builder);

            builder.Property(e => e.CreatedBy).HasMaxLength(100);
            builder.Property(e => e.CreatedAt).IsRequired();
            builder.Property(e => e.UpdatedBy).HasMaxLength(100);
        }
    }
}
