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
using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.DTOs.Explorer;

namespace Gamification.Application.CQRS.Handlers.VendorProfiles.Queries
{
    internal class GetVendorProfilesIncludingDeletedQueryHandler : IRequestHandler<GetVendorProfilesIncludingDeletedQuery, ApiResponse<IEnumerable<GetVendorDto>>>
    {
        private readonly IGenericRepository<VendorProfile> _repository;
        private readonly ILogger<GetVendorProfilesIncludingDeletedQueryHandler> _logger;

        public GetVendorProfilesIncludingDeletedQueryHandler(
            IGenericRepository<VendorProfile> repository,
            ILogger<GetVendorProfilesIncludingDeletedQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<IEnumerable<GetVendorDto>>> Handle(GetVendorProfilesIncludingDeletedQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching all vendors");

            var vendorProfiles = await _repository.GetTable().IgnoreQueryFilters().ToListAsync(cancellationToken: cancellationToken);
            var dtos = VendorProfileMapper.ToGetDtos(vendorProfiles);

            _logger.LogInformation("Retrieved {Count} Profiles", vendorProfiles.Count());

            return ApiResponse<IEnumerable<GetVendorDto>>.Success(dtos);
        }
    }
}
