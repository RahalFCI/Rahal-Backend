using Gamification.Application.DTOs.Achievement;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Commands.Achievement
{
    public record CreateAchievementCommand(CreateAchievementDto Dto) : IRequest<string>;

}
