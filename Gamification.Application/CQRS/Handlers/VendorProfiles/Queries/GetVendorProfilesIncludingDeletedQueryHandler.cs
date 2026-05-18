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
using Shared.Application.Pagination;
using Shared.Infrastructure.Pagination;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Handlers.VendorProfiles.Queries
{
    internal class GetVendorProfilesIncludingDeletedQueryHandler : IRequestHandler<GetVendorProfilesIncludingDeletedQuery, ApiResponse<PagedResult<GetVendorDto>>>
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

        public async Task<ApiResponse<PagedResult<GetVendorDto>>> Handle(GetVendorProfilesIncludingDeletedQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching all vendors including deleted - page {Page}, pageSize {PageSize}", request.PaginationRequest.Page, request.PaginationRequest.PageSize);

            var result = await _repository.GetTable()
                .IgnoreQueryFilters()
                .Select(v => VendorProfileMapper.ToGetDto(v))
                .ToPagedResultAsync(request.PaginationRequest, cancellationToken);

            _logger.LogInformation("Retrieved {Count} vendor profiles out of {TotalCount}", result.Items.Count(), result.TotalCount);

            return ApiResponse<PagedResult<GetVendorDto>>.Success(result);
        }
    }
}
