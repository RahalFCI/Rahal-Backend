using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rahal.Api.Controllers._Common;
using Shared.Application.DTOs;
using Shared.Domain.Enums;
using Users.Application.DTOs.Auth;
using Users.Application.Interfaces;
using Users.Application.Services;

namespace Rahal.Api.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : CustomControllerBase
    {
        private readonly IPasswordResetService _passwordResetService;
        private readonly ITokenService _tokenService;

        public AuthController(IPasswordResetService passwordResetService, ITokenService tokenService)
        {
            _passwordResetService = passwordResetService;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Request password reset and receive reset link via email
        /// </summary>
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ForgotPasswordAsync([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
        {
            try
            {
                await _passwordResetService.ForgotPasswordAsync(request.Email, cancellationToken);

                // Always return success for security (prevent email enumeration attacks)
                return NoContent();
            }
            catch (Exception)
            {
                return NoContent();
            }
        }

        /// <summary>
        /// Reset password using otp and new password
        /// </summary>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
        {
            var result = await _passwordResetService.ResetPasswordAsync(request, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [Authorize]
        [HttpPost("generate")]
        public async Task<IActionResult> GenerateNewAccessToken(TokenDto tokenDto)
        {

            var response = await _tokenService.RefreshExpiredToken(tokenDto);

            if (response.IsSuccess)
            {
                return Ok(response);
            }
            else
            {
                return Unauthorized(response);
            }
        }
    }
}
