using Gamification.Application.DTOs.Explorer;
using Gamification.Domain.Entities;
using MediatR;
using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Commands.ExplorerProfiles
{
    public record UpdateExplorerProfileCommand(UpdateExplorerDto UpdateExplorerDto) : IRequest<ApiResponse<GetExplorerDto>>;
}
