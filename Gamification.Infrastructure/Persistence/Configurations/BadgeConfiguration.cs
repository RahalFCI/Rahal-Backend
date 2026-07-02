using Shared.Infrastructure.Persistence.Configurations;
using Gamification.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gamification.Infrastructure.Persistence.Configurations
{
    public class BadgeConfiguration : BaseAuditableEntityConfiguration<Badge>
    {
        public override void Configure(EntityTypeBuilder<Badge> builder)
        {
            base.Configure(builder);

            builder.ToTable("Badges", "gamification");

            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(e => e.ImageUrl)
                .IsRequired();

            // Large string mapped to TEXT type in database
            builder.Property(e => e.Description)
                .HasColumnType("text");

            // Audit Properties (inherited from BaseEntity)

            builder.Property(e => e.UpdatedAt)
                .ValueGeneratedOnUpdate();

            builder.Property(e => e.DeletedAt);

            builder.Property(e => e.IsDeleted)
                .HasDefaultValue(false);

            builder.HasIndex(e => e.Name)
                .IsUnique()
                .HasDatabaseName("IX_Badges_Name");

            builder.HasIndex(e => e.IsDeleted)
                .HasDatabaseName("IX_Badges_IsDeleted");
        }
    }
}
