using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rahal.Api.Controllers._Common;
using Shared.Application.DTOs;
using Shared.Application.Pagination;
using Shared.Domain.Enums;
using Users.Application.DTOs._Common;
using Users.Application.DTOs.Auth;
using Users.Application.DTOs.OAuth;
using Users.Application.DTOs.Register;
using Users.Application.Interfaces;
using Users.Application.Services;
using Users.Domain.Entities._Common;
using Users.Domain.Enums;

namespace Rahal.Api.Controllers.Users
{
    public class UserController : CustomControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly IOAuthGoogleFacade _googleOAuthFacade;

        public UserController(
            IAuthService authService,
            IUserService userService,
            IOAuthGoogleFacade googleOAuthFacade)
        {
            _authService = authService;
            _userService = userService;
            _googleOAuthFacade = googleOAuthFacade;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterAsync([FromBody] BaseRegisterDto registerRequestDto, CancellationToken cancellationToken = default)
        {
            var result = await _authService.RegisterAsync(registerRequestDto, registerRequestDto.Password, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("register-admin")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterAdminAsync([FromBody] BaseRegisterDto registerRequestDto, CancellationToken cancellationToken = default)
        {
            var result = await _authService.CreateAdminAsync(registerRequestDto, registerRequestDto.Password, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }



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

 
        [HttpGet("{id}")]
        [Authorize]
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

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAllAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var request = new OffsetPaginationRequest { Page = page, PageSize = pageSize };
            var result = await _userService.GetAllUsers(request, cancellationToken);
            return Ok(result);
        }

        [HttpGet("explorers")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAllExplorersAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var request = new OffsetPaginationRequest { Page = page, PageSize = pageSize };
            var result = await _userService.GetAllUsers(request, cancellationToken);
            return Ok(result);
        }

        [HttpGet("vendors")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAllVendorsAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var request = new OffsetPaginationRequest { Page = page, PageSize = pageSize };
            var result = await _userService.GetAllUsers(request, cancellationToken);
            return Ok(result);
        }

        [HttpGet("admins")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAllAdminsAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var request = new OffsetPaginationRequest { Page = page, PageSize = pageSize };
            var result = await _userService.GetAllUsers(request, cancellationToken);
            return Ok(result);
        }


        [HttpGet("include-deleted")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAllIncludingDeletedAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var request = new OffsetPaginationRequest { Page = page, PageSize = pageSize };
            var result = await _userService.GetAllUsersIncludingDeleted(request, cancellationToken);
            return Ok(result);
        }

        [HttpGet("explorers-include-deleted")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAllExplorersIncludingDeletedAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var request = new OffsetPaginationRequest { Page = page, PageSize = pageSize };
            var result = await _userService.GetAllExplorersIncludingDeleted(request, cancellationToken);
            return Ok(result);
        }

        [HttpGet("vendors-include-deleted")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAllVendorsIncludingDeletedAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var request = new OffsetPaginationRequest { Page = page, PageSize = pageSize };
            var result = await _userService.GetAllVendorsIncludingDeleted(request, cancellationToken);
            return Ok(result);
        }

        [HttpGet("admins-include-deleted")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAllAdminsIncludingDeletedAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var request = new OffsetPaginationRequest { Page = page, PageSize = pageSize };
            var result = await _userService.GetAllAdminsIncludingDeleted(request, cancellationToken);
            return Ok(result);
        }


        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateAsync([FromBody] BaseUserDto explorerDto, CancellationToken cancellationToken = default)
        {
            var result = await _userService.UpdateUser(explorerDto, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

 
        [HttpPut("password/{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdatePasswordAsync([FromBody] UpdatePasswordDto updatePasswordDto, CancellationToken cancellationToken)
        {
            var id = GetCurrentUserId();

            var result = await _userService.UpdatePassword(id, updatePasswordDto, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }


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


        [HttpDelete("permanent/{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeletePermanentlyAsync([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            if (GetCurrentUserId() != id)
                return Forbid();

            var result = await _userService.DeleteUserPermanently(id, cancellationToken);

            if (!result.IsSuccess)
                return NotFound(result);

            return NoContent();
        }


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

