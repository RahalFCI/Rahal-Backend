using Gamification.Application.CQRS.Commands.UserStat;
using Gamification.Application.Interfaces;
using Gamification.Domain.Entities;
using Gamification.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Domain.Enums;

namespace Gamification.Application.CQRS.Handlers.UserStat.Commands
{
    public class SpendXpCommandHandler : IRequestHandler<SpendXpCommand, ApiResponse<string>>
    {
        private readonly IGamificationRepository<UserStats> _userStatsRepository;
        private readonly IGamificationRepository<XpTransaction> _xpTransactionRepository;
        private readonly ILogger<SpendXpCommandHandler> _logger;

        public SpendXpCommandHandler(
            IGamificationRepository<UserStats> userStatsRepository,
            IGamificationRepository<XpTransaction> xpTransactionRepository,
            ILogger<SpendXpCommandHandler> logger)
        {
            _userStatsRepository = userStatsRepository;
            _xpTransactionRepository = xpTransactionRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(SpendXpCommand request, CancellationToken cancellationToken)
        {
            if (request.Amount < 0)
                return ApiResponse<string>.Failure(ErrorCode.InvalidRequest);

            if (!Enum.TryParse<XpSourceType>(request.SourceType, out var sourceType))
                return ApiResponse<string>.Failure(ErrorCode.InvalidValue);

            var existingTransaction = await _xpTransactionRepository.GetTable()
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.ExplorerProfileId == request.ExplorerId
                    && t.Source == sourceType
                    && t.ReferenceId == request.ReferenceId,
                    cancellationToken);

            if (existingTransaction is not null)
                return existingTransaction.Amount == -request.Amount
                    ? ApiResponse<string>.Success("XP spend already processed")
                    : ApiResponse<string>.Failure(ErrorCode.Conflict);

            var userStats = await _userStatsRepository.GetTable()
                .FirstOrDefaultAsync(us => us.ExplorerProfileId == request.ExplorerId, cancellationToken);

            if (userStats is null)
                return ApiResponse<string>.Failure(ErrorCode.NotFound);

            if (userStats.AvailableXp < request.Amount)
                return ApiResponse<string>.Failure(ErrorCode.BusinessRuleViolation);

            userStats.AvailableXp -= request.Amount;
            userStats.UpdatedAt = DateTime.UtcNow;

            _xpTransactionRepository.Add(new XpTransaction
            {
                ExplorerProfileId = request.ExplorerId,
                Amount = -request.Amount,
                Source = sourceType,
                ReferenceId = request.ReferenceId
            });

            await _userStatsRepository.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Spent {Amount} XP for explorer {ExplorerId} from source {SourceType}",
                request.Amount, request.ExplorerId, sourceType);

            return ApiResponse<string>.Success("XP spent successfully");
        }
    }
}
