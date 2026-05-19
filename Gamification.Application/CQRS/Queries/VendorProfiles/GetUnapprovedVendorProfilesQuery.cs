using Gamification.Application.DTOs.Vendor;
using MediatR;
using Shared.Application.DTOs;
using Shared.Application.Pagination;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Queries.VendorProfiles
{
    public record GetUnapprovedVendorProfilesQuery(OffsetPaginationRequest PaginationRequest) : IRequest<ApiResponse<PagedResult<GetVendorDto>>>;

}
