using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Shared.Application.Settings.ReslilienceSettings;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;

namespace Shared.Infrastructure.Resilience
{
    public class EmailResiliencePipelineFactory
    {
        private readonly ILogger<EmailResiliencePipelineFactory> _logger;
        private readonly EmailResilienceSettings _options;

        public EmailResiliencePipelineFactory(
            ILogger<EmailResiliencePipelineFactory> logger,
            IOptions<EmailResilienceSettings> options)
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
                .Handle<SmtpException>()
                .Handle<TimeoutException>()
                .Handle<SocketException>(),
            OnRetry = args =>
            {
                _logger.LogWarning(
                    "Email retry attempt {Attempt} after {Delay}s. Reason: {Exception}",
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
            MinimumThroughput = 5,
            SamplingDuration = TimeSpan.FromSeconds(30),
            OnOpened = args =>
            {
                _logger.LogError(
                    "Email circuit breaker OPENED for {Duration}s.",
                    args.BreakDuration.TotalSeconds);
                return ValueTask.CompletedTask;
            },
            OnClosed = args =>
            {
                _logger.LogInformation("Email circuit breaker CLOSED. Service recovered.");
                return ValueTask.CompletedTask;
            },
            OnHalfOpened = args =>
            {
                _logger.LogInformation("Email circuit breaker HALF-OPEN. Testing.");
                return ValueTask.CompletedTask;
            }
        };
    }
}
