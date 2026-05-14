using MediatR;
using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Commands.UserStats
{
    public record PermanentDeleteUserStatsCommand(Guid UserId) : IRequest<ApiResponse<string>>;

}
