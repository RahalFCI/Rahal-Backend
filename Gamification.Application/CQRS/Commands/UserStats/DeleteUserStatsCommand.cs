using MediatR;
using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Commands.UserStats
{
    public record DeleteUserStatsCommand(Guid UserId) : IRequest<ApiResponse<string>>;

}
