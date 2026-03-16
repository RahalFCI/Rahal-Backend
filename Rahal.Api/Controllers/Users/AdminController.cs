using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rahal.Api.Controllers._Common;
using Shared.Application.DTOs;
using Shared.Domain.Enums;
using Users.Application.DTOs;
using Users.Application.DTOs.Admin;
using Users.Application.DTOs.Auth;
using Users.Application.DTOs.Register;
using Users.Application.Factory;
using Users.Application.Interfaces;
using Users.Application.Services;
using Users.Domain.Entities;
using Users.Domain.Enums;

namespace Rahal.Api.Controllers.Users
{
    public class AdminController : CustomControllerBase
    {
        private readonly IAuthService<Admin> _authService;
        private readonly IUserService<Admin, AdminDto, AdminSummaryDto> _userService;
        private readonly IUserFactory<RegisterAdminDto, Admin> _userFactory;

        public AdminController(
            IAuthService<Admin> authService,
            IUserService<Admin, AdminDto, AdminSummaryDto> userService,
            IUserFactory<RegisterAdminDto, Admin> userFactory)
        {
            _authService = authService;
            _userService = userService;
            _userFactory = userFactory;
        }

        /// <summary>
        /// Register a new admin user
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterAdminDto registerAdminDto, CancellationToken cancellationToken)
        {
            var user = _userFactory.CreateUser(registerAdminDto);

            if (user is null)
                return BadRequest(ApiResponse<string>.Failure(ErrorCode.InvalidRequest));

            var result = await _authService.RegisterAsync(user, registerAdminDto.Password, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result);

            return CreatedAtAction(nameof(RegisterAsync), result);
        }

        /// <summary>
        /// Login admin user
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
        /// Logout admin user
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
        /// Get admin by ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
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
        /// Get all admins (Admin only)
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
        /// Update admin profile
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateAsync([FromRoute] Guid id, [FromBody] AdminDto adminDto, CancellationToken cancellationToken)
        {
            var result = await _userService.UpdateUser(adminDto, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Update admin password
        /// </summary>
        [HttpPut("password/{id}")]
        [Authorize(Roles = "Admin")]
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
        /// Delete admin user (Admin only)
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
    }
}
