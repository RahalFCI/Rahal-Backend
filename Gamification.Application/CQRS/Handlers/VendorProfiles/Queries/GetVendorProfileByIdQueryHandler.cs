using Gamification.Application.CQRS.Handlers.ExplorerProfiles.Queries;
using Gamification.Application.CQRS.Queries.ExplorerProfiles;
using Gamification.Application.CQRS.Queries.VendorProfiles;
using Gamification.Application.DTOs.Vendor;
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

namespace Gamification.Application.CQRS.Handlers.VendorProfiles.Queries
{
    internal class GetVendorProfileByIdQueryHandler : IRequestHandler <GetVendorProfileByIdQuery, ApiResponse<GetVendorDto>>
    {
        private readonly IGenericRepository<VendorProfile> _repository;
        private readonly ILogger<GetVendorProfileByIdQueryHandler> _logger;

        public GetVendorProfileByIdQueryHandler(
            IGenericRepository<VendorProfile> repository,
            ILogger<GetVendorProfileByIdQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }
        public async Task<ApiResponse<GetVendorDto>> Handle(GetVendorProfileByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching vendorProfile {VendorId}", request.Id);

            var vendorProfile = await _repository.GetByIdAsync(request.Id, cancellationToken: cancellationToken);
            if (vendorProfile is null)
            {
                _logger.LogInformation("Failed to find vendorProfile {VendorId}", request.Id);
                return ApiResponse<GetVendorDto>.Failure(ErrorCode.NotFound);
            }
            var dto = VendorProfileMapper.ToGetDto(vendorProfile);

            _logger.LogInformation("Retrieved vendorProfile {VendorId}", request.Id);
            return ApiResponse<GetVendorDto>.Success(dto);
        }
    }
}
