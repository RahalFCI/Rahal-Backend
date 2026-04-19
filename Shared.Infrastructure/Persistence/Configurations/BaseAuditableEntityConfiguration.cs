using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Infrastructure.Persistence.Configurations
{
    public class BaseAuditableEntityConfiguration<TEntity> : BaseEntityConfiguration<TEntity>
        where TEntity : BaseEntity
    {
        public override void Configure(EntityTypeBuilder<TEntity> builder)
        {
            base.Configure(builder);

            builder.Property(e => e.CreatedAt).IsRequired();
        }
    }
}
