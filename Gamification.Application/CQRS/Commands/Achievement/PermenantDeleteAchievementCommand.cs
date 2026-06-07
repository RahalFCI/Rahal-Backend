using MediatR;
using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Commands.Achievement
{
    public record PermenantDeleteAchievementCommand(Guid Id) : IRequest<ApiResponse<string>>;
}
