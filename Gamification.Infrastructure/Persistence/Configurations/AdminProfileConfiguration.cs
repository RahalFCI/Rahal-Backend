using Gamification.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gamification.Infrastructure.Persistence.Configurations
{
    public class AdminProfileConfiguration : IEntityTypeConfiguration<AdminProfile>
    {
        public void Configure(EntityTypeBuilder<AdminProfile> builder)
        {
            builder.ToTable("AdminProfiles", "gamification");

            builder.HasKey(e => e.UserId);

            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.Property(e => e.UserId)
                .IsRequired();

            builder.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.UpdatedAt)
                .ValueGeneratedOnUpdate();

            builder.Property(e => e.DeletedAt);

            builder.Property(e => e.IsDeleted)
                .HasDefaultValue(false);

            builder.HasIndex(e => e.UserId)
                .IsUnique()
                .HasDatabaseName("IX_AdminProfiles_UserId");

            builder.HasIndex(e => e.IsDeleted)
                .HasDatabaseName("IX_AdminProfiles_IsDeleted");
        }
    }
}
