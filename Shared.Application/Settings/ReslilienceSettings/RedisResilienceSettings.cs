namespace Shared.Infrastructure.Resilience
{
    public class RedisResilienceSettings
    {
        public const string SectionName = "ResilienceSettings:Redis";
        public int MaxRetries { get; set; } = 3;
        public int InitialDelaySeconds { get; set; } = 1;
        public double CircuitBreakerFailureThreshold { get; set; } = 0.5;
        public int CircuitBreakerOpenDurationSeconds { get; set; } = 15;
        public int TimeoutSeconds { get; set; } = 5;
    }
}
