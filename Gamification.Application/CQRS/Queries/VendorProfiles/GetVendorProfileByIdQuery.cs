using Gamification.Application.DTOs.Vendor;
using MediatR;
using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.DTOs.Explorer;

namespace Gamification.Application.CQRS.Queries.VendorProfiles
{
    public record GetVendorProfileByIdQuery(Guid Id) : IRequest<ApiResponse<GetVendorDto>>;

}
