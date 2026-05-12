using Gamification.Application.DTOs.Explorer;
using Gamification.Application.DTOs.Vendor;
using MediatR;
using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.DTOs.Explorer;

namespace Gamification.Application.CQRS.Commands.VendorProfiles
{
    public record UpdateVendorProfileCommand(UpdateVendorDto UpdateVendorDto) : IRequest<ApiResponse<GetVendorDto>>;

}
