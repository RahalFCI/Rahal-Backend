using Gamification.Application.CQRS.Commands.ExplorerProfiles;
using Gamification.Application.Interfaces;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Application.DTOs;
using Shared.Domain.Enums;

namespace Gamification.Application.CQRS.Handlers.ExplorerProfiles.Commands
{
    public class SetExplorerPremiumCommandHandler : IRequestHandler<SetExplorerPremiumCommand, ApiResponse<string>>
    {
        private readonly IGamificationRepository<ExplorerProfile> _repository;

        public SetExplorerPremiumCommandHandler(IGamificationRepository<ExplorerProfile> repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<string>> Handle(SetExplorerPremiumCommand request, CancellationToken cancellationToken)
        {
            var explorerProfile = await _repository.GetTable()
                .FirstOrDefaultAsync(p => p.UserId == request.ExplorerId, cancellationToken);

            if (explorerProfile is null)
                return ApiResponse<string>.Failure(ErrorCode.NotFound);

            explorerProfile.IsPremium = request.IsPremium;
            explorerProfile.PlanTierId = request.PlanTierId;
            explorerProfile.UpdatedAt = DateTime.UtcNow;
            await _repository.SaveChangesAsync(cancellationToken);

            return ApiResponse<string>.Success("Explorer premium state updated successfully");
        }
    }
}
