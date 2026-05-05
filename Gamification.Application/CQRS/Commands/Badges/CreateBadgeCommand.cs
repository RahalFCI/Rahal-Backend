using Gamification.Application.DTOs.Badge;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Commands.Badges
{
    public record CreateBadgeCommand(CreateBadgeDto Dto) : IRequest<string>;

}
