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
using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.DTOs.Explorer;

namespace Gamification.Application.CQRS.Handlers.VendorProfiles.Queries
{
    public class GetAllVendorsProfilesQueryHandler : IRequestHandler<GetAllVendorsProfilesQuery, ApiResponse<IEnumerable<GetVendorDto>>>
    {
        private readonly IGenericRepository<VendorProfile> _repository;
        private readonly ILogger<GetAllVendorsProfilesQueryHandler> _logger;

        public GetAllVendorsProfilesQueryHandler(
            IGenericRepository<VendorProfile> repository,
            ILogger<GetAllVendorsProfilesQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<IEnumerable<GetVendorDto>>> Handle(GetAllVendorsProfilesQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching all vendors");

            var vendorProfiles = await _repository.GetAllAsync(cancellationToken: cancellationToken);
            var dtos = VendorProfileMapper.ToGetDtos(vendorProfiles);

            _logger.LogInformation("Retrieved {Count} vendor profiles", vendorProfiles.Count());

            return ApiResponse<IEnumerable<GetVendorDto>>.Success(dtos);
        }
    }
}
