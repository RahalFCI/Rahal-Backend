using MediatR;
using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.DTOs.Explorer;

namespace Gamification.Application.CQRS.Queries.ExplorerProfiles
{
    public record GetAllExplorerProfilesQuery : IRequest<ApiResponse<IEnumerable<GetExplorerDto>>>;
}
