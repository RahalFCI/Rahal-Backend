using MediatR;
using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Commands.Badges
{
    public record PermenantDeleteBadgeCommand(Guid Id) : IRequest<ApiResponse<string>>;

}
