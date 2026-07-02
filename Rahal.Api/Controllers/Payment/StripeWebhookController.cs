using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.Interfaces;
using Shared.Domain.Enums;

namespace Rahal.Api.Controllers.Payment
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/payments/webhooks/stripe")]
    public class StripeWebhookController : ControllerBase
    {
        private readonly IPaymentWebhookService _paymentWebhookService;
        private readonly ILogger<StripeWebhookController> _logger;

        public StripeWebhookController(
            IPaymentWebhookService paymentWebhookService,
            ILogger<StripeWebhookController> logger)
        {
            _paymentWebhookService = paymentWebhookService;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> HandleAsync(CancellationToken cancellationToken)
        {
            var signatureHeader = Request.Headers["Stripe-Signature"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(signatureHeader))
            {
                return BadRequest("Missing Stripe-Signature header.");
            }

            string payload;
            using (var reader = new StreamReader(Request.Body))
            {
                payload = await reader.ReadToEndAsync(cancellationToken);
            }

            try
            {
                var result = await _paymentWebhookService.HandleGatewayWebhookAsync(
                    payload,
                    signatureHeader,
                    cancellationToken);

                if (result.IsSuccess)
                {
                    return Ok(result);
                }

                return result.ErrorCode switch
                {
                    ErrorCode.NotFound => NotFound(result),
                    ErrorCode.ValidationError => BadRequest(result),
                    _ => BadRequest(result)
                };
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Stripe webhook cannot be handled because payment configuration is invalid.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Payment gateway is not configured.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Stripe webhook signature verification or parsing failed.");
                return BadRequest("Invalid Stripe webhook payload or signature.");
            }
        }
    }
}
