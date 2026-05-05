using Gamification.Application.CQRS.Commands.Badges;
using Gamification.Application.Mappers;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Handlers.Badges.Commands
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
}
