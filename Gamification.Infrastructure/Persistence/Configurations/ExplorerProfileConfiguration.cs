using Shared.Infrastructure.Persistence.Configurations;
using Gamification.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gamification.Infrastructure.Persistence.Configurations
{
    public class ExplorerProfileConfiguration : BaseAuditableEntityConfiguration<ExplorerProfile>
    {
        public override void Configure(EntityTypeBuilder<ExplorerProfile> builder)
        {
            base.Configure(builder);

            builder.ToTable("ExplorerProfiles", "gamification");

            builder.HasKey(e => e.UserId);

            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.Property(e => e.DisplayName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.ProfilePictureURL)
                .HasMaxLength(500);

            builder.Property(e => e.UserId)
                .IsRequired();

            builder.Property(e => e.Gender)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(e => e.BirthDate)
                .IsRequired();

            builder.Property(e => e.Level)
                .HasDefaultValue(1)
                .IsRequired();

            builder.Property(e => e.IsPremium)
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(e => e.IsPublic)
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(e => e.Bio)
                .IsRequired(false)
                .HasMaxLength(500);

            builder.Property(e => e.CountryCode)
                .IsRequired()
                .HasMaxLength(2);

            builder.Property(e => e.UpdatedAt)
                .ValueGeneratedOnUpdate();

            builder.Property(e => e.DeletedAt);

            builder.Property(e => e.IsDeleted)
                .HasDefaultValue(false);

            builder.HasIndex(e => e.UserId)
                .IsUnique()
                .HasDatabaseName("IX_ExplorerProfiles_UserId");

            builder.HasIndex(e => e.IsDeleted)
                .HasDatabaseName("IX_ExplorerProfiles_IsDeleted");
        }
    }
}
