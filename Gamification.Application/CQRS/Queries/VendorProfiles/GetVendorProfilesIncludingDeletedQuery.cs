using Gamification.Application.DTOs.Vendor;
using MediatR;
using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Queries.VendorProfiles
{
    public record GetVendorProfilesIncludingDeletedQuery : IRequest<ApiResponse<IEnumerable<GetVendorDto>>>;

}
