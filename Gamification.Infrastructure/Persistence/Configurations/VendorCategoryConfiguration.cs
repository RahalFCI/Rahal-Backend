using Shared.Infrastructure.Persistence.Configurations;
using Gamification.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Infrastructure.Persistence.Configurations
{
    public class VendorCategoryConfiguration : BaseAuditableEntityConfiguration<VendorCategory>
    {
        public override void Configure(EntityTypeBuilder<VendorCategory> builder)
        {
            base.Configure(builder);

            builder.ToTable("VendorCategories", "gamification");

            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.Property(e => e.CategoryName)
                .IsRequired();

            builder.Property(e => e.UpdatedAt)
                .ValueGeneratedOnUpdate();

            builder.Property(e => e.DeletedAt);

            builder.Property(e => e.IsDeleted)
                .HasDefaultValue(false);

            builder.HasIndex(e => e.CategoryName)
                .IsUnique()
                .HasDatabaseName("IX_VendorCategory_CategoryName");

            builder.HasIndex(e => e.IsDeleted)
                .HasDatabaseName("IX_VendorCategory_IsDeleted");
        }
    }
}
