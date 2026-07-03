using Payment.Application.DTOs.Transactions;
using Shared.Application.DTOs;
using Shared.Application.Pagination;

namespace Payment.Application.Interfaces
{
    public interface IPaymentTransactionQueryService
    {
        Task<ApiResponse<PagedResult<PaymentTransactionResponseDto>>> GetTransactionsAsync(
            PaymentTransactionFilterDto filter,
            CancellationToken cancellationToken = default);
    }
}
