using Gamification.Application.DTOs.UserStats;
using MediatR;
using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Commands.UserStat
{
    public record CreateUserStatsCommand(CreateUserStatsDto Dto) : IRequest<ApiResponse<string>>;

}
