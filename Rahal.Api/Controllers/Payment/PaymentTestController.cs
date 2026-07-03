using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.Interfaces;
using Shared.Application.Events.Payments;

namespace Rahal.Api.Controllers.Payment
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/payments")]
    public class PaymentTestController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentTestController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("test-intent")]
        [ProducesResponseType(typeof(ProcessPaymentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProcessPaymentResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateTestIntentAsync(
            [FromBody] CreateTestPaymentIntentRequest request,
            CancellationToken cancellationToken)
        {
            var processPaymentRequest = new ProcessPaymentRequest(
                request.OperationId ?? Guid.NewGuid(),
                request.UserId,
                request.Amount,
                request.Currency,
                request.PaymentMethod ?? "PaymentSheet",
                request.ReferenceId ?? Guid.NewGuid());

            var result = await _paymentService.ProcessPaymentAsync(
                processPaymentRequest,
                cancellationToken);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }

    public sealed record CreateTestPaymentIntentRequest(
        Guid UserId,
        decimal Amount,
        string Currency,
        Guid? ReferenceId = null,
        Guid? OperationId = null,
        string? PaymentMethod = null);
}
