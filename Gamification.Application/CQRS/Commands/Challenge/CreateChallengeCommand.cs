using Gamification.Application.DTOs.Challenge;
using MediatR;
using Shared.Application.DTOs;

namespace Gamification.Application.CQRS.Commands.Challenge
{
    public record CreateChallengeCommand(CreateChallengeDto Dto) : IRequest<ApiResponse<GetChallengeDto>>;

}
