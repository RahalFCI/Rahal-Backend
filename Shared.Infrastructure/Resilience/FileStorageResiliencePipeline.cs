using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using System.IdentityModel;
using System.Collections.Generic;
using System.Text;
using Shared.Application.Settings.ReslilienceSettings;

namespace Shared.Infrastructure.Resilience
{
    public class FileStorageResiliencePipelineFactory
    {
        private readonly ILogger<FileStorageResiliencePipelineFactory> _logger;
        private readonly FileStorageResilienceSettings _options;

        public FileStorageResiliencePipelineFactory(
            ILogger<FileStorageResiliencePipelineFactory> logger,
            IOptions<FileStorageResilienceSettings> options)
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
                .Handle<IOException>(),
            OnRetry = args =>
            {
                _logger.LogWarning(
                    "File storage retry attempt {Attempt} after {Delay}s. Reason: {Exception}",
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
            SamplingDuration = TimeSpan.FromSeconds(60),
            OnOpened = args =>
            {
                _logger.LogError(
                    "File storage circuit breaker OPENED for {Duration}s.",
                    args.BreakDuration.TotalSeconds);
                return ValueTask.CompletedTask;
            },
            OnClosed = args =>
            {
                _logger.LogInformation("File storage circuit breaker CLOSED. Storage recovered.");
                return ValueTask.CompletedTask;
            },
            OnHalfOpened = args =>
            {
                _logger.LogInformation("File storage circuit breaker HALF-OPEN. Testing.");
                return ValueTask.CompletedTask;
            }
        };
    }
}
