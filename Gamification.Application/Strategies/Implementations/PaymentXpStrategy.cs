using Gamification.Domain.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.Strategies.Implementations
{
    public class PaymentXpStrategy : IXpCalculationStrategy
    {
        private readonly ILogger<PaymentXpStrategy> _logger;

        public XpSourceType SourceType => XpSourceType.Payment;

        public PaymentXpStrategy(ILogger<PaymentXpStrategy> logger)
        {
            _logger = logger;
        }

        public Task<int> CalculateXpAsync(Guid sourceId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Calculating XP for payment {PaymentId}", sourceId);

            int paymentAmount = 100;

            const int xpPerCurrencyUnit = 2;

            var totalXp = paymentAmount * xpPerCurrencyUnit;
            _logger.LogInformation(
                "Calculated payment XP: {TotalXp} for amount {Amount}",
                totalXp,
                paymentAmount);

            return Task.FromResult(totalXp);
        }
    }
}
