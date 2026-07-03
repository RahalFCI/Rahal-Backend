using MassTransit;
using Microsoft.Extensions.Logging;
using Payment.Application.Interfaces;
using Shared.Application.Events.Payments;
using Shared.Domain.Enums;

namespace Payment.Application.EventConsumers
{
    public class ProcessPaymentRequestConsumer : IConsumer<ProcessPaymentRequest>
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<ProcessPaymentRequestConsumer> _logger;

        public ProcessPaymentRequestConsumer(
            IPaymentService paymentService,
            ILogger<ProcessPaymentRequestConsumer> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ProcessPaymentRequest> context)
        {
            try
            {
                var response = await _paymentService.ProcessPaymentAsync(
                    context.Message,
                    context.CancellationToken);

                await context.RespondAsync(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unhandled payment request failure for operation {OperationId}",
                    context.Message.OperationId);

                await context.RespondAsync(new ProcessPaymentResponse(
                    context.Message.OperationId,
                    false,
                    ErrorCode.UnknownError,
                    null,
                    "Unhandled payment processing error."));
            }
        }
    }
}
