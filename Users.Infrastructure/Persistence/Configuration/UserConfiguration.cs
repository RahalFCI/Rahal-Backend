using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.Domain.Entities._Common;
using Users.Domain.Enums;

namespace Users.Infrastructure.Persistence.Configuration
{

    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("AspNetUsers", "users");

            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.Property(e => e.DisplayName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.UserType)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(e => e.RefreshToken)
                .HasMaxLength(500);

            builder.Property(e => e.DeletedAt);

            builder.Property(e => e.IsDeleted)
                .HasDefaultValue(false);

            builder.HasIndex(e => e.UserType)
                .HasDatabaseName("IX_UserType");
        }
    }
}
