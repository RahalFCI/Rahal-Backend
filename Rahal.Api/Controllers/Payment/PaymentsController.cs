using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.DTOs.Transactions;
using Payment.Application.Interfaces;
using Payment.Domain.Enums;
using Shared.Application.Pagination;

namespace Rahal.Api.Controllers.Payment
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/payments")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentTransactionQueryService _paymentTransactionQueryService;

        public PaymentsController(IPaymentTransactionQueryService paymentTransactionQueryService)
        {
            _paymentTransactionQueryService = paymentTransactionQueryService;
        }

        [HttpGet("transactions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetTransactionsAsync(
            [FromQuery] string? explorerDisplayName,
            [FromQuery] PaymentStatus? status,
            [FromQuery] Guid? transactionId,
            [FromQuery] string? currency,
            [FromQuery] DateOnly? fromDate,
            [FromQuery] DateOnly? toDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var result = await _paymentTransactionQueryService.GetTransactionsAsync(
                new PaymentTransactionFilterDto
                {
                    ExplorerDisplayName = explorerDisplayName,
                    Status = status,
                    TransactionId = transactionId,
                    Currency = currency,
                    FromDate = fromDate,
                    ToDate = toDate,
                    Pagination = new OffsetPaginationRequest
                    {
                        Page = page,
                        PageSize = pageSize
                    }
                },
                cancellationToken);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
