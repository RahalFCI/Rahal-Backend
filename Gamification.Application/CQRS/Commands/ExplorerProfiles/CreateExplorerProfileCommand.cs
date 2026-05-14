using Gamification.Application.DTOs.Explorer;
using Gamification.Application.DTOs.Vendor;
using MediatR;
using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Commands.ExplorerProfiles
{
    public record CreateExplorerProfileCommand(AddExplorerDto ExplorerProfileDto, string ProfilePictureUrl) : IRequest<ApiResponse<Guid>>;
}
