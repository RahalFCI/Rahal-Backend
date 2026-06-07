using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Orchestrators.ExplorerProfiles
{
    public record UpdateExplorerProfilePictureOrchestrator(Guid UserId, IFormFile? ProfilePicture) : IRequest<ApiResponse<string>>;
}
