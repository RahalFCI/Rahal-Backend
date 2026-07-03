using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain.Entities;
using Payment.Domain.Enums;

namespace Payment.Infrastructure.Persistence.Configurations
{
    public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
    {
        public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
        {
            builder.ToTable("Payments", "payment");

            builder.HasKey(payment => payment.Id);

            builder.Property(payment => payment.Id)
                .ValueGeneratedNever();

            builder.Property(payment => payment.OperationId)
                .IsRequired();

            builder.Property(payment => payment.ExplorerId)
                .IsRequired();

            builder.Property(payment => payment.ReferenceId)
                .IsRequired();

            builder.Property(payment => payment.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(payment => payment.AmountMinor)
                .IsRequired();

            builder.Property(payment => payment.Currency)
                .HasMaxLength(3)
                .IsRequired();

            builder.Property(payment => payment.Status)
                .HasConversion<string>()
                .HasMaxLength(64)
                .IsRequired();

            builder.Property(payment => payment.Gateway)
                .HasConversion<string>()
                .HasMaxLength(64)
                .IsRequired();

            builder.Property(payment => payment.GatewayPaymentIntentId)
                .HasMaxLength(256);

            builder.Property(payment => payment.GatewayCustomerId)
                .HasMaxLength(256);

            builder.Property(payment => payment.FailureMessage)
                .HasColumnType("text");

            builder.Property(payment => payment.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            builder.Property(payment => payment.UpdatedAt);

            builder.Property(payment => payment.DeletedAt);

            builder.Property(payment => payment.IsDeleted)
                .HasDefaultValue(false)
                .IsRequired();

            builder.HasQueryFilter(payment => !payment.IsDeleted);

            builder.HasIndex(payment => payment.ReferenceId)
                .HasDatabaseName("IX_Payments_ReferenceId");

            builder.HasIndex(payment => payment.OperationId)
                .HasDatabaseName("IX_Payments_OperationId");

            builder.HasIndex(payment => payment.GatewayPaymentIntentId)
                .HasDatabaseName("IX_Payments_GatewayPaymentIntentId")
                .IsUnique()
                .HasFilter("\"GatewayPaymentIntentId\" IS NOT NULL");
        }
    }
}
