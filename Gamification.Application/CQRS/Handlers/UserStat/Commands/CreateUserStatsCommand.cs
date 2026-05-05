using Gamification.Application.DTOs.UserStats;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Handlers.UserStat.Commands
{
    public record CreateUserStatsCommand(CreateUserStatsDto Dto) : IRequest<string>;

}
