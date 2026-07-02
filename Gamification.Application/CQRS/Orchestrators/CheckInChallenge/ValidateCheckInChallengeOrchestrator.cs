using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.Application.DTOs;

namespace Gamification.Application.CQRS.Orchestrators.CheckInChallenge
{
    public record ValidateCheckInChallengeOrchestrator(Guid Id, IFormFile Image) : IRequest<ApiResponse<bool>>;
}
