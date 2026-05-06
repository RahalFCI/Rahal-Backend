using Gamification.Application.CQRS.Commands.Badges;
using Gamification.Application.CQRS.Queries.Badge;
using Gamification.Application.Mappers;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Handlers.Badges.Commands
{
    public class CreateBadgeCommandHandler : IRequestHandler<CreateBadgeCommand, ApiResponse<string>>
    {
        private readonly IGenericRepository<Badge> _repository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IMediator _mediator;
        private readonly ILogger<CreateBadgeCommandHandler> _logger;

        public CreateBadgeCommandHandler(
            IGenericRepository<Badge> repository,
            IFileStorageService fileStorageService,
            IMediator mediator,
            ILogger<CreateBadgeCommandHandler> logger)
        {
            _repository = repository;
            _fileStorageService = fileStorageService;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(CreateBadgeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating badge {BadgeName}", request.Dto.Name);

            var existingBadge = await _mediator.Send(new GetBadgeByNameQuery(request.Dto.Name));

            if (existingBadge.IsSuccess)
                return ApiResponse<string>.Failure(ErrorCode.AlreadyExists);

            var badge = BadgeMapper.ToEntity(request.Dto);

            if(request.Dto.Image is not null) {
                badge.ImageUrl = await _fileStorageService.UploadAsync(request.Dto.Image, cancellationToken);
                _logger.LogInformation("Badge {BadgeId} Image uploaded successfully", badge.Id);

            }

            _repository.Add(badge);
            await _repository.SaveChangesAsync(cancellationToken);


            _logger.LogInformation("Badge {BadgeId} created successfully", badge.Id);

            return ApiResponse<string>.Success($"Badge created successfully. ID: {badge.Id}");
        }
    }
}
