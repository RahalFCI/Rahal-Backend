using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace Shared.Infrastructure.Resilience
{
    /// <summary>
    /// Factory for creating configured resilience pipelines for external service integrations.
    /// Provides a reusable pipeline with retry, circuit breaker, and timeout policies.
    /// </summary>
    public class SearchResiliencePipelineFactory
    {
        private readonly ILogger<SearchResiliencePipelineFactory> _logger;
        private readonly ResilienceSettings _options;

        public SearchResiliencePipelineFactory(ILogger<SearchResiliencePipelineFactory> logger, ResilienceSettings options)
        {
            _logger = logger;
            _options = options;
        }

        public ResiliencePipeline CreatePipeline()
        {
            var pipelineBuilder = new ResiliencePipelineBuilder()
                .AddRetry(CreateRetryPolicy())
                .AddCircuitBreaker(CreateCircuitBreakerPolicy())
                .AddTimeout(TimeSpan.FromSeconds(_options.TimeoutSeconds));

            _logger.LogInformation(
                "Resilience pipeline created with configuration: " +
                "MaxRetries={MaxRetries}, " +
                "InitialDelaySeconds={InitialDelaySeconds}, " +
                "CircuitBreakerFailureThreshold={CircuitBreakerFailureThreshold}, " +
                "CircuitBreakerOpenDurationSeconds={CircuitBreakerOpenDurationSeconds}, " +
                "TimeoutSeconds={TimeoutSeconds}",
                _options.MaxRetries,
                _options.InitialDelaySeconds,
                _options.CircuitBreakerFailureThreshold,
                _options.CircuitBreakerOpenDurationSeconds,
                _options.TimeoutSeconds);

            return pipelineBuilder.Build();
        }

        private RetryStrategyOptions CreateRetryPolicy()
        {
            return new RetryStrategyOptions
            {
                ShouldHandle = args =>
                {
                    var exception = args.Outcome.Exception;
                    return ValueTask.FromResult(exception is HttpRequestException or OperationCanceledException);
                },
                MaxRetryAttempts = _options.MaxRetries,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromSeconds(_options.InitialDelaySeconds),
                UseJitter = true,
                OnRetry = args =>
                {
                    _logger.LogWarning(
                        "Resilience policy retry attempt {AttemptNumber} of {MaxRetries}. " +
                        "Exception: {ExceptionMessage}. " +
                        "Next retry in {DelayMilliseconds}ms.",
                        args.AttemptNumber,
                        _options.MaxRetries,
                        args.Outcome.Exception?.Message ?? "Unknown error",
                        args.RetryDelay.TotalMilliseconds);

                    return ValueTask.CompletedTask;
                }
            };
        }

        private CircuitBreakerStrategyOptions CreateCircuitBreakerPolicy()
        {
            return new CircuitBreakerStrategyOptions
            {
                ShouldHandle = args =>
                {
                    var exception = args.Outcome.Exception;
                    return ValueTask.FromResult(exception is HttpRequestException or OperationCanceledException);
                },
                FailureRatio = 1.0,
                MinimumThroughput = _options.CircuitBreakerFailureThreshold,
                SamplingDuration = TimeSpan.FromSeconds(30),
                BreakDuration = TimeSpan.FromSeconds(_options.CircuitBreakerOpenDurationSeconds)
            };
        }
    }
}
