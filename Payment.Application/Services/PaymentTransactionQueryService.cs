using MassTransit;
using Microsoft.EntityFrameworkCore;
using Payment.Application.DTOs.Transactions;
using Payment.Application.Interfaces;
using Payment.Domain.Entities;
using Shared.Application.DTOs;
using Shared.Application.Events.Gamification;
using Shared.Application.Interfaces;
using Shared.Application.Pagination;
using Shared.Domain.Enums;

namespace Payment.Application.Services
{
    public class PaymentTransactionQueryService : IPaymentTransactionQueryService
    {
        private const string UnknownExplorerDisplayName = "Unknown Explorer";

        private readonly IGenericRepository<PaymentTransaction> _paymentRepository;
        private readonly IRequestClient<GetExplorerPaymentProfilesRequest> _explorerProfilesClient;

        public PaymentTransactionQueryService(
            IGenericRepository<PaymentTransaction> paymentRepository,
            IRequestClient<GetExplorerPaymentProfilesRequest> explorerProfilesClient)
        {
            _paymentRepository = paymentRepository;
            _explorerProfilesClient = explorerProfilesClient;
        }

        public async Task<ApiResponse<PagedResult<PaymentTransactionResponseDto>>> GetTransactionsAsync(
            PaymentTransactionFilterDto filter,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<ExplorerPaymentProfileDto>? explorerDisplayNameMatches = null;

            if (!string.IsNullOrWhiteSpace(filter.ExplorerDisplayName))
            {
                var explorersResponse = await GetExplorerProfilesAsync(
                    filter.ExplorerDisplayName,
                    null,
                    cancellationToken);

                if (!explorersResponse.IsSuccess)
                {
                    return ApiResponse<PagedResult<PaymentTransactionResponseDto>>.Failure(
                        explorersResponse.ErrorCode);
                }

                explorerDisplayNameMatches = explorersResponse.Explorers;
                if (explorerDisplayNameMatches.Count == 0)
                {
                    return ApiResponse<PagedResult<PaymentTransactionResponseDto>>.Success(
                        EmptyResult(filter.Pagination));
                }
            }

            var query = _paymentRepository
                .GetTable()
                .AsNoTracking();

            if (explorerDisplayNameMatches is not null)
            {
                var explorerIds = explorerDisplayNameMatches
                    .Select(explorer => explorer.UserId)
                    .Distinct()
                    .ToArray();

                query = query.Where(payment => explorerIds.Contains(payment.ExplorerId));
            }

            if (filter.TransactionId is not null)
            {
                query = query.Where(payment => payment.Id == filter.TransactionId.Value);
            }

            if (filter.Status is not null)
            {
                query = query.Where(payment => payment.Status == filter.Status.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Currency))
            {
                var currency = filter.Currency.Trim().ToLowerInvariant();
                query = query.Where(payment => payment.Currency == currency);
            }

            if (filter.FromDate is not null)
            {
                var fromDate = filter.FromDate.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
                query = query.Where(payment => payment.CreatedAt >= fromDate);
            }

            if (filter.ToDate is not null)
            {
                var toDateExclusive = filter.ToDate.Value
                    .AddDays(1)
                    .ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

                query = query.Where(payment => payment.CreatedAt < toDateExclusive);
            }

            var page = filter.Pagination.Page < 1 ? 1 : filter.Pagination.Page;
            var pageSize = filter.Pagination.PageSize < 1 ? 10 : filter.Pagination.PageSize;

            var totalCount = await query.CountAsync(cancellationToken);
            var payments = await query
                .OrderByDescending(payment => payment.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var explorerNames = await GetExplorerNamesForPaymentsAsync(
                payments,
                explorerDisplayNameMatches,
                cancellationToken);

            var items = payments
                .Select(payment => ToResponse(payment, explorerNames))
                .ToList();

            return ApiResponse<PagedResult<PaymentTransactionResponseDto>>.Success(
                new PagedResult<PaymentTransactionResponseDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                });
        }

        private async Task<IReadOnlyDictionary<Guid, string>> GetExplorerNamesForPaymentsAsync(
            IReadOnlyCollection<PaymentTransaction> payments,
            IReadOnlyList<ExplorerPaymentProfileDto>? alreadyResolvedExplorers,
            CancellationToken cancellationToken)
        {
            if (payments.Count == 0)
            {
                return new Dictionary<Guid, string>();
            }

            if (alreadyResolvedExplorers is not null)
            {
                return alreadyResolvedExplorers
                    .GroupBy(explorer => explorer.UserId)
                    .ToDictionary(
                        group => group.Key,
                        group => group.First().DisplayName);
            }

            var explorerIds = payments
                .Select(payment => payment.ExplorerId)
                .Distinct()
                .ToArray();

            var explorersResponse = await GetExplorerProfilesAsync(
                null,
                explorerIds,
                cancellationToken);

            if (!explorersResponse.IsSuccess)
            {
                return new Dictionary<Guid, string>();
            }

            return explorersResponse.Explorers
                .GroupBy(explorer => explorer.UserId)
                .ToDictionary(
                    group => group.Key,
                    group => group.First().DisplayName);
        }

        private async Task<GetExplorerPaymentProfilesResponse> GetExplorerProfilesAsync(
            string? displayName,
            IReadOnlyCollection<Guid>? explorerIds,
            CancellationToken cancellationToken)
        {
            var response = await _explorerProfilesClient.GetResponse<GetExplorerPaymentProfilesResponse>(
                new GetExplorerPaymentProfilesRequest(displayName, explorerIds),
                cancellationToken);

            return response.Message;
        }

        private static PaymentTransactionResponseDto ToResponse(
            PaymentTransaction payment,
            IReadOnlyDictionary<Guid, string> explorerNames) =>
            new(
                payment.Id,
                payment.ExplorerId,
                explorerNames.GetValueOrDefault(payment.ExplorerId, UnknownExplorerDisplayName),
                payment.OperationId,
                payment.ReferenceId,
                payment.Amount,
                payment.Currency,
                payment.Status,
                payment.Gateway,
                payment.GatewayPaymentIntentId,
                payment.FailureMessage,
                payment.CreatedAt,
                payment.UpdatedAt);

        private static PagedResult<PaymentTransactionResponseDto> EmptyResult(
            OffsetPaginationRequest pagination)
        {
            var page = pagination.Page < 1 ? 1 : pagination.Page;
            var pageSize = pagination.PageSize < 1 ? 10 : pagination.PageSize;

            return new PagedResult<PaymentTransactionResponseDto>
            {
                Items = Array.Empty<PaymentTransactionResponseDto>(),
                TotalCount = 0,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}
