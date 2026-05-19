using Gamification.Application.CQRS.Queries.VendorProfiles;
using Gamification.Application.DTOs.Vendor;
using Gamification.Application.Mappers;
using Gamification.Domain.Entities;
using MediatR;
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
    public class GetUnapprovedVendorProfilesQueryHandler : IRequestHandler<GetUnapprovedVendorProfilesQuery, ApiResponse<PagedResult<GetVendorDto>>>
    {
        private readonly IGenericRepository<VendorProfile> _repository;
        private readonly ILogger<GetUnapprovedVendorProfilesQueryHandler> _logger;

        public GetUnapprovedVendorProfilesQueryHandler(
            IGenericRepository<VendorProfile> repository,
            ILogger<GetUnapprovedVendorProfilesQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<PagedResult<GetVendorDto>>> Handle(GetUnapprovedVendorProfilesQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching all unapproved vendors - page {Page}, pageSize {PageSize}", request.PaginationRequest.Page, request.PaginationRequest.PageSize);

            var result = await _repository.GetTable()
                .Where(v => !v.IsApproved)
                .Select(v => VendorProfileMapper.ToGetDto(v))
                .ToPagedResultAsync(request.PaginationRequest, cancellationToken);

            _logger.LogInformation("Retrieved {Count} vendor profiles out of {TotalCount}", result.Items.Count(), result.TotalCount);

            return ApiResponse<PagedResult<GetVendorDto>>.Success(result);
        }
    }
}
