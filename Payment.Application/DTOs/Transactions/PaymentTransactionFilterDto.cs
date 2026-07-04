using Payment.Domain.Enums;
using Shared.Application.Pagination;

namespace Payment.Application.DTOs.Transactions
{
    public class PaymentTransactionFilterDto
    {
        public string? ExplorerDisplayName { get; set; }

        public PaymentStatus? Status { get; set; }

        public Guid? TransactionId { get; set; }

        public string? Currency { get; set; }

        public DateOnly? FromDate { get; set; }

        public DateOnly? ToDate { get; set; }

        public OffsetPaginationRequest Pagination { get; set; } = new();
    }
}
