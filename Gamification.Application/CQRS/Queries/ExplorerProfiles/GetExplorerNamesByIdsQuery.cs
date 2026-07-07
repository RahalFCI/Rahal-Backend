using Gamification.Application.DTOs.Explorer;
using MediatR;
using Shared.Application.DTOs;
using System;
using System.Collections.Generic;

namespace Gamification.Application.CQRS.Queries.ExplorerProfiles
{
    public record GetExplorerNamesByIdsQuery(IReadOnlyCollection<Guid> Ids) : IRequest<ApiResponse<List<GetExplorerNameDto>>>;
}
