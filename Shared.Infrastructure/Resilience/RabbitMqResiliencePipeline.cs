using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using Shared.Application.Settings.ReslilienceSettings;

namespace Shared.Infrastructure.Resilience
{
    public class RabbitMqResiliencePipelineFactory
    {
        private readonly ILogger<RabbitMqResiliencePipelineFactory> _logger;
        private readonly RabbitMqResilienceSettings _options;

        public RabbitMqResiliencePipelineFactory(
            ILogger<RabbitMqResiliencePipelineFactory> logger,
            IOptions<RabbitMqResilienceSettings> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        public ResiliencePipeline CreatePipeline()
        {
            return new ResiliencePipelineBuilder()
                .AddRetry(CreateRetryPolicy())
                .AddCircuitBreaker(CreateCircuitBreakerPolicy())
                .AddTimeout(TimeSpan.FromSeconds(_options.TimeoutSeconds))
                .Build();
        }

        private RetryStrategyOptions CreateRetryPolicy() => new()
        {
            MaxRetryAttempts = _options.MaxRetries,
            Delay = TimeSpan.FromSeconds(_options.InitialDelaySeconds),
            BackoffType = DelayBackoffType.Exponential,
            OnRetry = args =>
            {
                _logger.LogWarning(
                    "RabbitMQ request retry {Attempt} after {Delay}ms",
                    args.AttemptNumber,
                    args.RetryDelay.TotalMilliseconds);
                return ValueTask.CompletedTask;
            }
        };

        private CircuitBreakerStrategyOptions CreateCircuitBreakerPolicy() => new()
        {
            FailureRatio = _options.CircuitBreakerFailureThreshold,
            BreakDuration = TimeSpan.FromSeconds(_options.CircuitBreakerOpenDurationSeconds),
            MinimumThroughput = 5,
            SamplingDuration = TimeSpan.FromSeconds(30),
            OnOpened = args =>
            {
                _logger.LogError("RabbitMQ circuit opened for {BreakDuration}", args.BreakDuration);
                return ValueTask.CompletedTask;
            },
            OnClosed = _ =>
            {
                _logger.LogInformation("RabbitMQ circuit closed");
                return ValueTask.CompletedTask;
            },
            OnHalfOpened = _ =>
            {
                _logger.LogInformation("RabbitMQ circuit half-opened");
                return ValueTask.CompletedTask;
            }
        };
    }
}
