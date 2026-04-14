namespace Shared.Infrastructure.Resilience
{
    /// <summary>
    /// Configuration options for resilience policies applied to external service integrations.
    /// These settings are read from appsettings under the "Search:Resilience" section.
    /// </summary>
    public class ResilienceSettings
    {
        public const string SectionName = "Search:Resilience";
        public int MaxRetries { get; set; } = 3;
        public int InitialDelaySeconds { get; set; } = 2;
        public int CircuitBreakerFailureThreshold { get; set; } = 5;
        public int CircuitBreakerOpenDurationSeconds { get; set; } = 30;
        public int TimeoutSeconds { get; set; } = 10;
    }
}
