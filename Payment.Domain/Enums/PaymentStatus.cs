namespace Payment.Domain.Enums
{
    public enum PaymentStatus
    {
        Pending = 1,
        RequiresPaymentMethod = 2,
        RequiresAction = 3,
        Processing = 4,
        Succeeded = 5,
        Failed = 6,
        Canceled = 7
    }
}
