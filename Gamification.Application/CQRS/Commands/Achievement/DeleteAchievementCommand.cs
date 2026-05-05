using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Commands.Achievement
{
    public record DeleteAchievementCommand(Guid Id) : IRequest<string>;

}
