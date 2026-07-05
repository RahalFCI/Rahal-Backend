using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notifications.Domain.Entities;

namespace Notifications.Infrastructure.Persistence.Configurations
{
    public class UserNotificationTokenConfiguration : IEntityTypeConfiguration<UserNotificationToken>
    {
        public void Configure(EntityTypeBuilder<UserNotificationToken> builder)
        {
            builder.ToTable("users_tokens", "notifications");

            builder.HasKey(token => token.Id);

            builder.Property(token => token.Id)
                .ValueGeneratedNever();

            builder.Property(token => token.UserId)
                .IsRequired();

            builder.Property(token => token.FcmToken)
                .HasColumnType("text")
                .IsRequired();

            builder.Property(token => token.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            builder.Property(token => token.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            builder.Property(token => token.DeletedAt);

            builder.Property(token => token.IsDeleted)
                .HasDefaultValue(false)
                .IsRequired();

            builder.HasQueryFilter(token => !token.IsDeleted);

            builder.HasIndex(token => token.UserId)
                .HasDatabaseName("IX_users_tokens_UserId")
                .IsUnique();
        }
    }
}
