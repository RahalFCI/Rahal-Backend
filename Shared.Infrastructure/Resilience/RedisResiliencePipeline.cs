using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Shared.Infrastructure.Resilience;
using StackExchange.Redis;

namespace Shared.Infrastructure.Resilience
{
    public class RedisResiliencePipelineFactory
    {
        private readonly ILogger<RedisResiliencePipelineFactory> _logger;
        private readonly RedisResilienceSettings _options;

        public RedisResiliencePipelineFactory(
            ILogger<RedisResiliencePipelineFactory> logger,
            IOptions<RedisResilienceSettings> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        public ResiliencePipeline CreatePipeline()
        {
            return new ResiliencePipelineBuilder()
                .AddTimeout(TimeSpan.FromSeconds(_options.TimeoutSeconds))
                .AddRetry(CreateRetryPolicy())
                .AddCircuitBreaker(CreateCircuitBreakerPolicy())
                .Build();
        }

        private RetryStrategyOptions CreateRetryPolicy() => new()
        {
            MaxRetryAttempts = _options.MaxRetries,
            Delay = TimeSpan.FromSeconds(_options.InitialDelaySeconds),
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
            ShouldHandle = new PredicateBuilder()
                .Handle<TimeoutException>()
                .Handle<RedisConnectionException>()
                .Handle<RedisTimeoutException>(),
            OnRetry = args =>
            {
                _logger.LogWarning(
                    "Redis retry attempt {Attempt} after {Delay}s. Reason: {Exception}",
                    args.AttemptNumber,
                    args.RetryDelay.TotalSeconds,
                    args.Outcome.Exception?.Message);
                return ValueTask.CompletedTask;
            }
        };

        private CircuitBreakerStrategyOptions CreateCircuitBreakerPolicy() => new()
        {
            FailureRatio = _options.CircuitBreakerFailureThreshold,
            BreakDuration = TimeSpan.FromSeconds(_options.CircuitBreakerOpenDurationSeconds),
            MinimumThroughput = 3,
            SamplingDuration = TimeSpan.FromSeconds(30),
            OnOpened = args =>
            {
                _logger.LogError(
                    "Redis circuit breaker OPENED for {Duration}s.",
                    args.BreakDuration.TotalSeconds);
                return ValueTask.CompletedTask;
            },
            OnClosed = args =>
            {
                _logger.LogInformation("Redis circuit breaker CLOSED. Cache recovered.");
                return ValueTask.CompletedTask;
            },
            OnHalfOpened = args =>
            {
                _logger.LogInformation("Redis circuit breaker HALF-OPEN. Testing.");
                return ValueTask.CompletedTask;
            }
        };
    }
}
