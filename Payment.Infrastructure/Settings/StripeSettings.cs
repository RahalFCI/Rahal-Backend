namespace Payment.Infrastructure.Settings
{
    public class StripeSettings
    {
        public const string SectionName = "Stripe";
        public const string DefaultEphemeralKeyApiVersion = "2024-06-20";

        public string SecretKey { get; set; } = string.Empty;

        public string PublishableKey { get; set; } = string.Empty;

        public string WebhookSecret { get; set; } = string.Empty;

        public string EphemeralKeyApiVersion { get; set; } = DefaultEphemeralKeyApiVersion;
    }
}
