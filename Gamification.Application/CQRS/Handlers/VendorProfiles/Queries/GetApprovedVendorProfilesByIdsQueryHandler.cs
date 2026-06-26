using Gamification.Application.CQRS.Queries.VendorProfiles;
using Gamification.Application.DTOs.Vendor;
using Gamification.Application.Interfaces;
using Gamification.Application.Mappers;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Application.DTOs;
using Shared.Application.Pagination;
using Shared.Infrastructure.Pagination;

namespace Gamification.Application.CQRS.Handlers.VendorProfiles.Queries
{
    public class GetApprovedVendorProfilesByIdsQueryHandler
        : IRequestHandler<GetApprovedVendorProfilesByIdsQuery, ApiResponse<PagedResult<GetVendorDto>>>
    {
        private readonly IGamificationRepository<VendorProfile> _repository;

        public GetApprovedVendorProfilesByIdsQueryHandler(IGamificationRepository<VendorProfile> repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<PagedResult<GetVendorDto>>> Handle(GetApprovedVendorProfilesByIdsQuery request, CancellationToken cancellationToken)
        {
            var vendorIds = request.VendorIds.Distinct().ToList();
            var vendors =  _repository.GetTable()
                .AsNoTracking()
                .Where(v => vendorIds.Contains(v.UserId) && v.IsApproved)
                .Select(v => VendorProfileMapper.ToGetDto(v));

            var response = await PaginationExtensions.ToPagedResultAsync(vendors, request.PaginationRequest, cancellationToken);

            return ApiResponse<PagedResult<GetVendorDto>>.Success(response);
        }
    }
}
