using Gamification.Application.DTOs.Explorer;
using Gamification.Application.DTOs.Vendor;
using MediatR;
using Shared.Application.DTOs;

namespace Gamification.Application.CQRS.Commands.VendorProfiles
{
    public record CreateVendorProfileCommand(AddVendorDto VendorProfileDto, string ProfilePictureUrl) : IRequest<ApiResponse<GetVendorDto>>;
}
