using System.Text.Json;
using Gamification.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Domain.Enums;

namespace Gamification.Infrastructure.Services
{
    public class CheckInChallengeAiValidationService : ICheckInChallengeAiValidationService
    {
        private const string VerifyCheckInEndpoint = "/verify";

        private readonly HttpClient _httpClient;
        private readonly ILogger<CheckInChallengeAiValidationService> _logger;

        public CheckInChallengeAiValidationService(
            HttpClient httpClient,
            ILogger<CheckInChallengeAiValidationService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<ApiResponse<bool>> ValidateCheckInChallengeAsync(
            IFormFile image,
            string description,
            CancellationToken cancellationToken = default)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                await using var imageStream = image.OpenReadStream();
                using var imageContent = new StreamContent(imageStream);

                if (!string.IsNullOrWhiteSpace(image.ContentType))
                    imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(image.ContentType);

                content.Add(imageContent, "image", image.FileName);
                content.Add(new StringContent(description), "description");

                using var response = await _httpClient.PostAsync(VerifyCheckInEndpoint, content, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("AI check-in challenge validation request failed with status code {StatusCode}", response.StatusCode);
                    return ApiResponse<bool>.Failure(ErrorCode.ExternalServiceError);
                }

                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var isValid = ExtractValidationResult(responseContent);

                return isValid.HasValue
                    ? ApiResponse<bool>.Success(isValid.Value)
                    : ApiResponse<bool>.Failure(ErrorCode.ExternalServiceError);
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "AI check-in challenge validation request timed out");
                return ApiResponse<bool>.Failure(ErrorCode.Timeout);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "AI check-in challenge validation request failed");
                return ApiResponse<bool>.Failure(ErrorCode.ExternalServiceError);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "AI check-in challenge validation client is not configured correctly");
                return ApiResponse<bool>.Failure(ErrorCode.ExternalServiceError);
            }
        }

        private static bool? ExtractValidationResult(string responseContent)
        {
            if (string.IsNullOrWhiteSpace(responseContent))
                return null;

            try
            {
                using var document = JsonDocument.Parse(responseContent);
                var root = document.RootElement;

                if (root.ValueKind is JsonValueKind.True or JsonValueKind.False)
                    return root.GetBoolean();

                if (root.ValueKind == JsonValueKind.Object
                    && root.TryGetProperty("verification_result", out var isValid)
                    && isValid.ValueKind is JsonValueKind.True or JsonValueKind.False)
                {
                    return isValid.GetBoolean();
                }
            }
            catch (JsonException)
            {
                return null;
            }

            return null;
        }
    }
}
