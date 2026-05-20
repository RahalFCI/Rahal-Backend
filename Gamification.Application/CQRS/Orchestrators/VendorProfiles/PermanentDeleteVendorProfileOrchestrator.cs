using MediatR;
using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Orchestrators.VendorProfiles
{
    public record PermanentDeleteVendorProfileOrchestrator(Guid Id) : IRequest<ApiResponse<string>>;

}
