namespace Shared.Application.Settings.ReslilienceSettings
{
    public class SearchResilienceSettings
    {
        public const string SectionName = "ResilienceSettings:Search";
        public int MaxRetries { get; set; } = 3;
        public int InitialDelaySeconds { get; set; } = 2;
        public int CircuitBreakerFailureThreshold { get; set; } = 5;
        public int CircuitBreakerOpenDurationSeconds { get; set; } = 30;
        public int TimeoutSeconds { get; set; } = 10;
    }
}
