using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rahal.Api.Controllers._Common;
using SocialMedia.Application.DTOs.Media;
using SocialMedia.Application.Interfaces;

namespace Rahal.Api.Controllers.SocialMedia
{
    public class MediaController : CustomControllerBase
    {
        private readonly IMediaService _mediaService;

        public MediaController(IMediaService mediaService)
        {
            _mediaService = mediaService;
        }

        /// <summary>
        /// Generates Cloudinary signed upload credentials for direct client-side uploads.
        /// The client uses these to upload files directly to Cloudinary — the file never
        /// touches our servers. The generated public_ids are registered in Redis so that
        /// post creation can verify the media was legitimately pre-signed.
        /// </summary>
        [HttpPost("signatures")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GenerateUploadSignaturesAsync(
            [FromBody] GenerateUploadSignaturesRequest request,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _mediaService.GenerateUploadSignaturesAsync(request, userId, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
