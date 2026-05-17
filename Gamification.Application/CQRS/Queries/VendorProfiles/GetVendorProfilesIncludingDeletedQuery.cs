using Gamification.Application.DTOs.Vendor;
using MediatR;
using Shared.Application.DTOs;
using Shared.Application.Pagination;
using Shared.Infrastructure.Pagination;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Queries.VendorProfiles
{
    public record GetVendorProfilesIncludingDeletedQuery(OffsetPaginationRequest PaginationRequest) : IRequest<ApiResponse<PagedResult<GetVendorDto>>>;

}
