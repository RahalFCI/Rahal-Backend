namespace Shared.Infrastructure.Resilience
{
    public class EmailResilienceSettings
    {
        public const string SectionName = "ResilienceSettings:Email";
        public int MaxRetries { get; set; } = 3;
        public int InitialDelaySeconds { get; set; } = 2;
        public int CircuitBreakerFailureThreshold { get; set; } = 5;
        public int CircuitBreakerOpenDurationSeconds { get; set; } = 30;
        public int TimeoutSeconds { get; set; } = 10;
    }
}
