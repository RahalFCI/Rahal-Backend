using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rahal.Api.Controllers._Common;
using Shared.Application.DTOs;
using Shared.Domain.Enums;
using Users.Application.DTOs.Auth;
using Users.Application.DTOs.Explorer;
using Users.Application.DTOs.OAuth;
using Users.Application.DTOs.Register;
using Users.Application.Factory;
using Users.Application.Interfaces;
using Users.Application.Services;
using Users.Domain.Entities._Common;
using Users.Domain.Enums;

namespace Rahal.Api.Controllers.Users
{
    public class ExplorerController : CustomControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService<ExplorerDto, ExplorerSummaryDto> _userService;
        private readonly IUserFactory<RegisterExplorerDto, User> _userFactory;
        private readonly IOAuthGoogleFacade _googleOAuthFacade;

        public ExplorerController(
            IAuthService authService,
            IUserService<ExplorerDto, ExplorerSummaryDto> userService,
            IUserFactory<RegisterExplorerDto, User> userFactory,
            IOAuthGoogleFacade googleOAuthFacade)
        {
            _authService = authService;
            _userService = userService;
            _userFactory = userFactory;
            _googleOAuthFacade = googleOAuthFacade;
        }

        /// <summary>
        /// Register a new explorer user
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterAsync([FromForm] RegisterExplorerDto registerRequestDto, [FromForm] IFormFile? profilePicture = null, CancellationToken cancellationToken = default)
        {
            var user = _userFactory.CreateUser(registerRequestDto);

            if (user is null)
                return BadRequest(ApiResponse<string>.Failure(ErrorCode.InvalidRequest));

            var result = await _authService.RegisterAsync(user, registerRequestDto.Password, profilePicture, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result);

            return CreatedAtAction(nameof(RegisterAsync), result);
        }

        /// <summary>
        /// Login explorer user
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> LoginAsync([FromBody] AuthRequestDto authRequestDto, CancellationToken cancellationToken)
        {
            var result = await _authService.LoginAsync(authRequestDto, cancellationToken);

            if (!result.IsSuccess)
                return Unauthorized(result);

            return Ok(result);
        }

        /// <summary>
        /// Authenticate explorer via Google OAuth
        /// </summary>
        [HttpPost("google-signin")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GoogleSignInAsync(
            [FromBody] GoogleSignInRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _googleOAuthFacade.AuthenticateAsync(
                request.IdToken,
                UserRoleEnum.Explorer,
                cancellationToken);

            if (!result.IsSuccess)
                return Unauthorized(result);

            return Ok(result);
        }

        /// <summary>
        /// Logout explorer user
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> LogoutAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _authService.LogoutAsync(cancellationToken);
                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }

        /// <summary>
        /// Get explorer by ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Explorer,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetByIdAsync([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await _userService.GetById(id, cancellationToken);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Get all explorers (Admin only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken)
        {
            var result = await _userService.GetAllUsers(cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Get all explorers including deleted (Admin only)
        /// </summary>
        [HttpGet("include-deleted")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAllIncludingDeletedAsync(CancellationToken cancellationToken)
        {
            var result = await _userService.GetAllUsersIncludingDeleted(cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Update explorer profile
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Explorer,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateAsync([FromRoute] Guid id, [FromForm] ExplorerDto explorerDto, [FromForm] IFormFile? profilePicture = null, CancellationToken cancellationToken = default)
        {
            var result = await _userService.UpdateUser(explorerDto, profilePicture, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Update explorer password
        /// </summary>
        [HttpPut("password/{id}")]
        [Authorize(Roles = "Explorer,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdatePasswordAsync([FromRoute] Guid id, [FromBody] UpdatePasswordDto updatePasswordDto, CancellationToken cancellationToken)
        {
            var result = await _userService.UpdatePassword(id, updatePasswordDto, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Delete explorer user (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteAsync([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await _userService.DeleteUser(id, cancellationToken);

            if (!result.IsSuccess)
                return NotFound(result);

            return NoContent();
        }

        /// <summary>
        /// Restore deleted explorer user (Admin only)
        /// </summary>
        [HttpPut("restore/{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> RestoreAsync([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await _userService.RestoreDeletedUser(id, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }
    }
}

