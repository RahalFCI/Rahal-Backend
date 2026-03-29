using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.Domain.Entities;
using Users.Domain.Enums;

namespace Users.Infrastructure.Persistence.Configuration
{
    public class ExplorerProfileConfiguration : IEntityTypeConfiguration<ExplorerProfile>
    {
        public void Configure(EntityTypeBuilder<ExplorerProfile> builder)
        {
            builder.ToTable("ExplorerProfiles", "users");

            builder.HasKey(e => e.Id);

            builder.HasQueryFilter(e => !e.IsDeleted);

            // Foreign key to User
            builder.HasOne(e => e.User)
                .WithOne(u => u.ExplorerProfile)
                .HasForeignKey<ExplorerProfile>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Audit Properties (inherited from BaseAuditableEntity)
            builder.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.UpdatedAt)
                .ValueGeneratedOnUpdate();

            builder.Property(e => e.DeletedAt);

            builder.Property(e => e.IsDeleted)
                .HasDefaultValue(false);

            // Domain Properties
            builder.Property(e => e.CountryCode)
                .IsRequired()
                .HasMaxLength(2);

            builder.Property(e => e.Gender)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(e => e.BirthDate)
                .IsRequired();

            builder.Property(e => e.Bio)
                .HasMaxLength(500);

            builder.Property(e => e.AvailableXp)
                .HasDefaultValue(0);

            builder.Property(e => e.CumulativeXp)
                .HasDefaultValue(0);

            builder.Property(e => e.Level)
                .HasDefaultValue(1);

            builder.Property(e => e.IsPublic)
                .HasDefaultValue(true);

            builder.Property(e => e.IsPremium)
                .HasDefaultValue(false);

            // Indexes
            builder.HasIndex(e => e.UserId)
                .IsUnique();

            builder.HasIndex(e => e.IsDeleted)
                .HasDatabaseName("IX_ExplorerProfiles_IsDeleted");
        }
    }
}
