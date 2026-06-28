using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notifications.Domain.Entities;

namespace Notifications.Infrastructure.Persistence.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notifications", "notifications");

            builder.HasKey(notification => notification.Id);

            builder.Property(notification => notification.Id)
                .ValueGeneratedNever();

            builder.Property(notification => notification.UserId)
                .IsRequired();

            builder.Property(notification => notification.ActorId);

            builder.Property(notification => notification.Type)
                .HasMaxLength(128)
                .IsRequired();

            builder.Property(notification => notification.TargetId)
                .HasMaxLength(256);

            builder.Property(notification => notification.IsRead)
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(notification => notification.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            builder.Property(notification => notification.UpdatedAt);

            builder.Property(notification => notification.DeletedAt);

            builder.Property(notification => notification.IsDeleted)
                .HasDefaultValue(false)
                .IsRequired();

            builder.HasQueryFilter(notification => !notification.IsDeleted);

            builder.Property(notification => notification.Metadata)
                .HasColumnType("jsonb")
                .HasDefaultValueSql("'{}'::jsonb")
                .IsRequired();

            builder.HasIndex(notification => new { notification.UserId, notification.CreatedAt })
                .HasDatabaseName("IX_Notifications_UserId_CreatedAt")
                .IsDescending(false, true);

            builder.HasIndex(notification => new { notification.UserId, notification.IsRead })
                .HasDatabaseName("IX_Notifications_UserId_IsRead");
        }
    }
}
