using Gamification.Application.DTOs.Explorer;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Orchestrators.ExplorerProfiles
{
    public record CreateExplorerProfileWithUserStatsOrchestrator(AddExplorerDto explorerDto, IFormFile? ProfilePicture) : IRequest<ApiResponse<GetExplorerDto>>;
}
