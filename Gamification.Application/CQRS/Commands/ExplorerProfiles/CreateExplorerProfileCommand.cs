using Gamification.Application.DTOs.Explorer;
using MediatR;
using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Commands.ExplorerProfiles
{
    public record CreateExplorerProfileCommand(AddExplorerDto ExplorerProfileDto) : IRequest<ApiResponse<Guid>>
    {
    }
}
