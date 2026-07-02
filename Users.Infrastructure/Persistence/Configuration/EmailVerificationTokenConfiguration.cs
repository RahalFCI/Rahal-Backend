using Shared.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.Domain.Entities;

namespace Users.Infrastructure.Persistence.Configuration
{
    public class EmailVerificationTokenConfiguration : BaseAuditableEntityConfiguration<EmailVerificationToken>
    {
        public override void Configure(EntityTypeBuilder<EmailVerificationToken> builder)
        {
            base.Configure(builder);

            builder.ToTable("EmailVerificationTokens", "users");

            // Properties
            builder.Property(e => e.UserId)
                .IsRequired();

            builder.Property(e => e.CodeHash)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(e => e.ExpiresAt)
                .IsRequired();

            builder.Property(e => e.IsUsed)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(e => e.AttemptCount)
                .IsRequired()
                .HasDefaultValue(0);

            // Audit Properties (inherited from BaseEntity)

            builder.Property(e => e.UpdatedAt)
                .ValueGeneratedOnUpdate();

            builder.Property(e => e.DeletedAt);

            builder.Property(e => e.IsDeleted)
                .HasDefaultValue(false);

            // Foreign Key
            builder.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_EmailVerificationToken_UserId");

            builder.HasIndex(e => e.ExpiresAt)
                .HasDatabaseName("IX_EmailVerificationToken_ExpiresAt");

            // Audit properties
            builder.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("now() at time zone 'UTC'");

            builder.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now() at time zone 'UTC'");
        }
    }
}
