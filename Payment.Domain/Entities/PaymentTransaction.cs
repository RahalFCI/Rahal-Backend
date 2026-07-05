using Payment.Domain.Enums;
using Shared.Domain.Entities;

namespace Payment.Domain.Entities
{
    public class PaymentTransaction : BaseEntity
    {
        public Guid OperationId { get; set; }

        public Guid ExplorerId { get; set; }

        public Guid ReferenceId { get; set; }

        public decimal Amount { get; set; }

        public long AmountMinor { get; set; }

        public string Currency { get; set; } = string.Empty;

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        public PaymentGatewayType Gateway { get; set; } = PaymentGatewayType.Stripe;

        public string? GatewayPaymentIntentId { get; set; }

        public string? GatewayCustomerId { get; set; }

        public string? FailureMessage { get; set; }
    }
}
