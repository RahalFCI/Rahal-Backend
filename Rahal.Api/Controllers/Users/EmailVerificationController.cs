using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Shared.Application.DTOs;
using Shared.Domain.Enums;
using Users.Application.DTOs.EmailVerification;
using Users.Application.Interfaces;

namespace Rahal.Api.Controllers.Users
{

    [ApiController]
    [Route("api/[controller]")]
    public class EmailVerificationController : ControllerBase
    {
        private readonly IEmailVerificationService _emailVerificationService;
        private readonly ILogger<EmailVerificationController> _logger;

        public EmailVerificationController(
            IEmailVerificationService emailVerificationService,
            ILogger<EmailVerificationController> logger)
        {
            _emailVerificationService = emailVerificationService;
            _logger = logger;
        }

        [HttpPost("verify-email")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> VerifyEmailAsync(
            [FromBody] VerifyOtpRequest request,
            CancellationToken cancellationToken = default)
        {

            var result = await _emailVerificationService.VerifyOtpAsync(
                    request.Email,
                    request.Otp,
                    cancellationToken);

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return StatusCode(GetStatusCodeFromErrorCode(result.errorCode), result);
        }


        [HttpPost("resend-verification")]
        [AllowAnonymous]
        [EnableRateLimiting("otp-resend")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> ResendVerificationAsync(
            [FromBody] ResendOtpRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _emailVerificationService.ResendOtpAsync(
                    request.Email,
                    cancellationToken);

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return StatusCode(GetStatusCodeFromErrorCode(result.errorCode), result);
        
        }


        private static int GetStatusCodeFromErrorCode(ErrorCode errorCode)
        {
            return errorCode switch
            {
                ErrorCode.NotFound => StatusCodes.Status404NotFound,
                ErrorCode.InvalidRequest => StatusCodes.Status400BadRequest,
                ErrorCode.InvalidOperation => StatusCodes.Status400BadRequest,
                ErrorCode.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorCode.Forbidden => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status500InternalServerError
            };
        }
    }
}
