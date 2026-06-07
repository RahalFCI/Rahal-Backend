using MediatR;
using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Orchestrators.ExplorerProfiles
{
    public record DeleteExplorerProfileWithUserStatsOrchestrator(Guid Id) : IRequest<ApiResponse<string>>;

}
