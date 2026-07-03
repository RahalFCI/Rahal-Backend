using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Rewards.Application.Interfaces;
using Shared.Application.DTOs;
using Shared.Domain.Enums;
using System.Net.Http.Json;
using System.Text.Json;

namespace Rewards.Infrastructure.Services
{
    public class RagTravelPlanService : IRagTravelPlanService
    {
        private const string ProjectId = "1";
        private const int Limit = 10;
        private const string GenerateTravelPlanEndpoint = $"/api/v1/nlp/ask/{ProjectId}";

        private readonly HttpClient _httpClient;
        private readonly ILogger<RagTravelPlanService> _logger;

        public RagTravelPlanService(HttpClient httpClient, ILogger<RagTravelPlanService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> GenerateTravelPlanAsync(string prompt, CancellationToken cancellationToken = default)
        {
            try
            {
                var url = QueryHelpers.AddQueryString(
                    GenerateTravelPlanEndpoint,
                    new Dictionary<string, string?>
                    {
                        ["query"] = prompt,
                        ["limit"] = Limit.ToString()
                    });

                using var response = await _httpClient.PostAsync(url, null, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("RAG travel plan request failed with status code {StatusCode}", response.StatusCode);
                    return ApiResponse<string>.Failure(ErrorCode.ExternalServiceError);
                }

                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var generatedPlan = ExtractGeneratedPlan(responseContent);

                if (string.IsNullOrWhiteSpace(generatedPlan))
                {
                    _logger.LogWarning("RAG travel plan response did not include generated content");
                    return ApiResponse<string>.Failure(ErrorCode.ExternalServiceError);
                }

                return ApiResponse<string>.Success(generatedPlan);
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "RAG travel plan request timed out");
                return ApiResponse<string>.Failure(ErrorCode.Timeout);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "RAG travel plan request failed");
                return ApiResponse<string>.Failure(ErrorCode.ExternalServiceError);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "RAG travel plan client is not configured correctly");
                return ApiResponse<string>.Failure(ErrorCode.ExternalServiceError);
            }
        }

        private static string? ExtractGeneratedPlan(string responseContent)
        {
            if (string.IsNullOrWhiteSpace(responseContent))
                return null;

            try
            {
                using var document = JsonDocument.Parse(responseContent);
                var root = document.RootElement;

                if (root.ValueKind == JsonValueKind.String)
                    return root.GetString();

                if (root.ValueKind == JsonValueKind.Object
                    && root.TryGetProperty("answer", out var result)
                    && result.ValueKind == JsonValueKind.String)
                {
                    return result.GetString();
                }
            }
            catch (JsonException)
            {
                return responseContent;
            }

            return null;
        }
    }
}
