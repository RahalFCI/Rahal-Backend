using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Commands.ProfilePictures
{
    public record UpdateProfilePictureCommand(IFormFile? ProfilePicture, string? OldProfilePictureUrl) : IRequest<ApiResponse<string?>>;
}
