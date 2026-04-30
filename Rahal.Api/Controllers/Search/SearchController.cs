using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Places.Infrastructure.Search;
using Rahal.Api.Controllers._Common;
using Shared.Application.DTOs;
using Shared.Application.DTOs.Search;
using Shared.Application.Interfaces;
using Shared.Application.Search;
using Users.Infrastructure.Search;

namespace Rahal.Api.Controllers.Search
{

    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : CustomControllerBase
    {
        private readonly ISearchService<UserSearchDocument> _userSearchService;
        private readonly ISearchService<PlaceSearchDocument> _placesSearchService;
        private readonly ILogger<SearchController> _logger;

        public SearchController(
            ISearchService<UserSearchDocument> userSearchService,
            ISearchService<PlaceSearchDocument> placesSearchService,
            ILogger<SearchController> logger)
        {
            _userSearchService = userSearchService;
            _placesSearchService = placesSearchService;
            _logger = logger;
        }

        [HttpGet("users")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SearchUsers(
            [FromQuery] SearchRequestDto searchRequest,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("User search initiated: query='{Query}', page={Page}, pageSize={PageSize}",
                searchRequest.Query, searchRequest.Page, searchRequest.PageSize);

            try
            {
                // Create search options from request
                var options = new SearchOptions
                {
                    Query = searchRequest.Query,
                    Page = searchRequest.Page,
                    PageSize = searchRequest.PageSize,
                    Filter = searchRequest.Filter,
                    SortBy = searchRequest.SortBy
                };

                // Perform search
                var result = await _userSearchService.SearchAsync(
                    searchRequest.Query, 
                    options, 
                    cancellationToken);

                _logger.LogInformation(
                    "User search completed: query='{Query}', found={HitCount} results out of {TotalHits}",
                    searchRequest.Query, result.Hits.Count(), result.TotalHits);

                // Return success response with pagination metadata
                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        results = result.Hits,
                        pagination = new
                        {
                            currentPage = result.Page,
                            pageSize = result.PageSize,
                            totalPages = result.TotalPages,
                            totalResults = result.TotalHits,
                            hasMore = result.HasMore
                        }
                    }
                });
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogError(ex, "User search was cancelled");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    success = false,
                    message = "Search service is temporarily unavailable"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during user search: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "An error occurred while processing your search"
                });
            }
        }

        [HttpGet("places")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SearchPlaces(
            [FromQuery] SearchRequestDto searchRequest,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Place search initiated: query='{Query}', page={Page}, pageSize={PageSize}",
                searchRequest.Query, searchRequest.Page, searchRequest.PageSize);

            try
            {
                // Create search options from request
                var options = new SearchOptions
                {
                    Query = searchRequest.Query,
                    Page = searchRequest.Page,
                    PageSize = searchRequest.PageSize,
                    Filter = searchRequest.Filter,
                    SortBy = searchRequest.SortBy
                };

                // Perform search
                var result = await _userSearchService.SearchAsync(
                    searchRequest.Query,
                    options,
                    cancellationToken);

                _logger.LogInformation(
                    "Place search completed: query='{Query}', found={HitCount} results out of {TotalHits}",
                    searchRequest.Query, result.Hits.Count(), result.TotalHits);

                // Return success response with pagination metadata
                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        results = result.Hits,
                        pagination = new
                        {
                            currentPage = result.Page,
                            pageSize = result.PageSize,
                            totalPages = result.TotalPages,
                            totalResults = result.TotalHits,
                            hasMore = result.HasMore
                        }
                    }
                });
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogError(ex, "User search was cancelled");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    success = false,
                    message = "Search service is temporarily unavailable"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during user search: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "An error occurred while processing your search"
                });
            }
        }


        [HttpGet("health")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> SearchHealth(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Search health check initiated");

                // Perform a simple empty search to check service availability
                var result = await _userSearchService.SearchAsync(string.Empty, 
                    new SearchOptions { Page = 1, PageSize = 1 }, 
                    cancellationToken);

                _logger.LogInformation("Search service is healthy");

                return Ok(new
                {
                    success = true,
                    status = "healthy",
                    message = "Search service is available and responding"
                });
            }
            catch (OperationCanceledException)
            {
                _logger.LogError("Search service health check timed out");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    success = false,
                    status = "unhealthy",
                    message = "Search service is not responding"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Search service health check failed: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    success = false,
                    status = "unhealthy",
                    message = "Search service is unavailable"
                });
            }
        }
    }
}
