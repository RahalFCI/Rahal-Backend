using Gamification.Application.DTOs.Badge;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Commands.Badges
{
    public record UpdateBadgeCommand(Guid Id, UpdateBadgeDto Dto) : IRequest<string>;

}
