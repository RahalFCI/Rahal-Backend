using Gamification.Application.DTOs.Explorer;
using MediatR;
using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Queries.ExplorerProfiles
{
    public record GetExplorerProfilesIncludingDeletedQuery : IRequest<ApiResponse<IEnumerable<GetExplorerDto>>>;
}
