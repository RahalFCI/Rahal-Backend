using Gamification.Application.DTOs.Explorer;
using MediatR;
using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Queries.ExplorerProfiles
{
    public record GetAllExplorerProfilesQuery : IRequest<ApiResponse<IEnumerable<GetExplorerDto>>>;
}
