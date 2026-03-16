using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rahal.Api.Controllers._Common;
using Shared.Application.DTOs;
using Shared.Domain.Enums;
using Users.Application.DTOs;
using Users.Application.DTOs.Auth;
using Users.Application.DTOs.Register;
using Users.Application.DTOs.Vendor;
using Users.Application.Factory;
using Users.Application.Interfaces;
using Users.Application.Services;
using Users.Domain.Entities;
using Users.Domain.Enums;

namespace Rahal.Api.Controllers.Users
{
    public class VendorController : CustomControllerBase
    {
        private readonly IAuthService<Vendor> _authService;
        private readonly IUserService<Vendor, VendorDto, VendorSummaryDto> _userService;
        private readonly IUserFactory<RegisterVendorDto, Vendor> _userFactory;

        public VendorController(
            IAuthService<Vendor> authService,
            IUserService<Vendor, VendorDto, VendorSummaryDto> userService,
            IUserFactory<RegisterVendorDto, Vendor> userFactory)
        {
            _authService = authService;
            _userService = userService;
            _userFactory = userFactory;
        }

        /// <summary>
        /// Register a new vendor user
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterVendorDto registerVendorDto)
        {
            var user = _userFactory.CreateUser(registerVendorDto);

            if (user is null)
                return BadRequest(ApiResponse<string>.Failure(ErrorCode.InvalidRequest));

            var result = await _authService.RegisterAsync(user, registerVendorDto.Password);

            if (!result.IsSuccess)
                return BadRequest(result);

            return CreatedAtAction(nameof(RegisterAsync), result);
        }

        /// <summary>
        /// Login vendor user
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> LoginAsync([FromBody] AuthRequestDto authRequestDto)
        {
            var result = await _authService.LoginAsync(authRequestDto);

            if (!result.IsSuccess)
                return Unauthorized(result);

            return Ok(result);
        }

        /// <summary>
        /// Logout vendor user
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> LogoutAsync()
        {
            try
            {
                await _authService.LogoutAsync();
                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }

        /// <summary>
        /// Get vendor by ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Vendor,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetByIdAsync([FromRoute] Guid id)
        {
            var result = await _userService.GetById(id);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Get all vendors (Admin only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAllAsync()
        {
            var result = await _userService.GetAllUsers();
            return Ok(result);
        }

        /// <summary>
        /// Update vendor profile
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Vendor,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateAsync([FromRoute] Guid id, [FromBody] VendorDto vendorDto)
        {
            var result = await _userService.UpdateUser(vendorDto);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Update vendor password
        /// </summary>
        [HttpPut("password/{id}")]
        [Authorize(Roles = "Vendor,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdatePasswordAsync([FromRoute] Guid id, [FromBody] UpdatePasswordDto updatePasswordDto)
        {
            var result = await _userService.UpdatePassword(id, updatePasswordDto);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Delete vendor user (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteAsync([FromRoute] Guid id)
        {
            var result = await _userService.DeleteUser(id);

            if (!result.IsSuccess)
                return NotFound(result);

            return NoContent();
        }
    }
}
