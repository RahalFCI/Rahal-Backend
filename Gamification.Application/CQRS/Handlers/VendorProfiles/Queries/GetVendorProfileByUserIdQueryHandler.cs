using Gamification.Application.CQRS.Handlers.ExplorerProfiles.Queries;
using Gamification.Application.CQRS.Queries.ExplorerProfiles;
using Gamification.Application.CQRS.Queries.VendorProfiles;
using Gamification.Application.DTOs.Vendor;
using Gamification.Application.Mappers;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Handlers.VendorProfiles.Queries
{
    internal class GetVendorProfileByUserIdQueryHandler : IRequestHandler<GetVendorProfileByUserIdQuery, ApiResponse<GetVendorDto>>
    {
        private readonly IGenericRepository<VendorProfile> _repository;
        private readonly ILogger<GetVendorProfileByUserIdQueryHandler> _logger;

        public GetVendorProfileByUserIdQueryHandler(
            IGenericRepository<VendorProfile> repository,
            ILogger<GetVendorProfileByUserIdQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<GetVendorDto>> Handle(GetVendorProfileByUserIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching vendor profile for user {UserId}", request.Id);

            var profile = await _repository.GetTable()
                .FirstOrDefaultAsync(p => p.UserId == request.Id, cancellationToken);

            if (profile is null)
            {
                _logger.LogWarning("Vendor profile not found for user {UserId}", request.Id);
                return ApiResponse<GetVendorDto>.Failure(ErrorCode.NotFound);
            }

            var profileDto = VendorProfileMapper.ToGetDto(profile);


            return ApiResponse<GetVendorDto>.Success(profileDto);
        }
    }
}
