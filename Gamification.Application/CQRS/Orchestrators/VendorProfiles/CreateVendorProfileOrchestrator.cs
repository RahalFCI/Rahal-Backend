using Gamification.Application.DTOs.Vendor;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Orchestrators.VendorProfiles
{
    public record CreateVendorProfileOrchestrator(AddVendorDto addVendorDto, IFormFile? profilePicture) : IRequest<ApiResponse<Guid>>
    {
    }
}
