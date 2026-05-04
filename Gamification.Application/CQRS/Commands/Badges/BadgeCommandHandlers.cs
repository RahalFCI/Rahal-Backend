using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;
using Gamification.Application.DTOs.Badge;
using Gamification.Application.Mappers;
using Gamification.Domain.Entities;

namespace Gamification.Application.CQRS.Commands.Badges
{
    public class CreateBadgeCommandHandler : IRequestHandler<CreateBadgeCommand, string>
    {
        private readonly IGenericRepository<Badge> _repository;
        private readonly ILogger<CreateBadgeCommandHandler> _logger;

        public CreateBadgeCommandHandler(
            IGenericRepository<Badge> repository,
            ILogger<CreateBadgeCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<string> Handle(CreateBadgeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating badge {BadgeName}", request.Dto.Name);

            var badge = BadgeMapper.ToEntity(request.Dto);
            _repository.Add(badge);
            await _repository.SaveChangesAsync(cancellationToken);


            _logger.LogInformation("Badge {BadgeId} created successfully", badge.Id);

            return $"Badge created successfully. ID: {badge.Id}";
        }
    }

    public class UpdateBadgeCommandHandler : IRequestHandler<UpdateBadgeCommand, string>
    {
        private readonly IGenericRepository<Badge> _repository;
        private readonly ILogger<UpdateBadgeCommandHandler> _logger;

        public UpdateBadgeCommandHandler(
            IGenericRepository<Badge> repository,
            ILogger<UpdateBadgeCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<string> Handle(UpdateBadgeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating badge {BadgeId}", request.Id);

            var badge = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (badge is null)
            {
                _logger.LogWarning("Badge {BadgeId} not found", request.Id);
                return $"Badge not found";
            }

            BadgeMapper.UpdateEntity(badge, request.Dto);
            _repository.Update(badge);
            await _repository.SaveChangesAsync(cancellationToken);


            _logger.LogInformation("Badge {BadgeId} updated successfully", request.Id);

            return "Badge updated successfully";
        }
    }

    public class DeleteBadgeCommandHandler : IRequestHandler<DeleteBadgeCommand, string>
    {
        private readonly IGenericRepository<Badge> _repository;
        private readonly ILogger<DeleteBadgeCommandHandler> _logger;

        public DeleteBadgeCommandHandler(
            IGenericRepository<Badge> repository,
            ILogger<DeleteBadgeCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<string> Handle(DeleteBadgeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting badge {BadgeId}", request.Id);

            var badge = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (badge is null)
            {
                _logger.LogWarning("Badge {BadgeId} not found", request.Id);
                return $"Badge not found";
            }

            _repository.Delete(badge);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Badge {BadgeId} deleted successfully", request.Id);

            return "Badge deleted successfully";
        }
    }
}
