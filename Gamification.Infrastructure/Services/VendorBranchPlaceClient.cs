using Gamification.Application.DTOs.VendorBranches;
using Gamification.Application.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;
using Shared.Application.DTOs;
using Shared.Application.Events.VendorBranches;
using Shared.Domain.Enums;

namespace Gamification.Infrastructure.Services
{
    public class VendorBranchPlaceClient : IVendorBranchPlaceClient
    {
        private readonly IRequestClient<CreateVendorBranchPlaceRequest> _createClient;
        private readonly IRequestClient<UpdateVendorBranchPlaceRequest> _updateClient;
        private readonly IRequestClient<GetVendorBranchPlaceRequest> _getClient;
        private readonly IRequestClient<GetVendorBranchPlacesRequest> _getManyClient;
        private readonly IRequestClient<DeleteVendorBranchPlaceRequest> _deleteClient;
        private readonly ResiliencePipeline _resiliencePipeline;
        private readonly ILogger<VendorBranchPlaceClient> _logger;

        public VendorBranchPlaceClient(
            IRequestClient<CreateVendorBranchPlaceRequest> createClient,
            IRequestClient<UpdateVendorBranchPlaceRequest> updateClient,
            IRequestClient<GetVendorBranchPlaceRequest> getClient,
            IRequestClient<GetVendorBranchPlacesRequest> getManyClient,
            IRequestClient<DeleteVendorBranchPlaceRequest> deleteClient,
            ResiliencePipelineProvider<string> resiliencePipelineProvider,
            ILogger<VendorBranchPlaceClient> logger)
        {
            _createClient = createClient;
            _updateClient = updateClient;
            _getClient = getClient;
            _getManyClient = getManyClient;
            _deleteClient = deleteClient;
            _resiliencePipeline = resiliencePipelineProvider.GetPipeline("rabbitmq");
            _logger = logger;
        }

        public async Task<ApiResponse<VendorBranchPlaceDto>> CreatePlaceAsync(CreateVendorBranchDto dto, CancellationToken cancellationToken = default)
        {
            return await ExecutePlaceRequestAsync(async ct =>
            {
                var operationId = Guid.NewGuid();
                var response = await _createClient.GetResponse<CreateVendorBranchPlaceResponse>(
                    new CreateVendorBranchPlaceRequest(
                        operationId,
                        dto.PlaceName,
                        dto.Description,
                        dto.Latitude,
                        dto.Longitude,
                        dto.GeoFenceRange,
                        dto.Address is null ? null : new VendorBranchPlaceAddressDto(
                            dto.Address.AddressLine,
                            dto.Address.Government,
                            dto.Address.City,
                            dto.Address.Country)),
                    ct);

                return response.Message.IsSuccess && response.Message.Place is not null
                    ? ApiResponse<VendorBranchPlaceDto>.Success(response.Message.Place)
                    : ApiResponse<VendorBranchPlaceDto>.Failure(response.Message.ErrorCode);
            }, cancellationToken);
        }

        public async Task<ApiResponse<VendorBranchPlaceDto>> UpdatePlaceAsync(Guid placeId, UpdateVendorBranchDto dto, CancellationToken cancellationToken = default)
        {
            return await ExecutePlaceRequestAsync(async ct =>
            {
                var operationId = Guid.NewGuid();
                var response = await _updateClient.GetResponse<UpdateVendorBranchPlaceResponse>(
                    new UpdateVendorBranchPlaceRequest(
                        operationId,
                        placeId,
                        dto.PlaceName,
                        dto.Description,
                        dto.Latitude,
                        dto.Longitude,
                        dto.GeoFenceRange,
                        dto.Address is null ? null : new VendorBranchPlaceAddressDto(
                            dto.Address.AddressLine,
                            dto.Address.Government,
                            dto.Address.City,
                            dto.Address.Country)),
                    ct);

                return response.Message.IsSuccess && response.Message.Place is not null
                    ? ApiResponse<VendorBranchPlaceDto>.Success(response.Message.Place)
                    : ApiResponse<VendorBranchPlaceDto>.Failure(response.Message.ErrorCode);
            }, cancellationToken);
        }

        public async Task<ApiResponse<VendorBranchPlaceDto>> GetPlaceAsync(Guid placeId, CancellationToken cancellationToken = default)
        {
            return await ExecutePlaceRequestAsync(async ct =>
            {
                var operationId = Guid.NewGuid();
                var response = await _getClient.GetResponse<GetVendorBranchPlaceResponse>(
                    new GetVendorBranchPlaceRequest(operationId, placeId),
                    ct);

                return response.Message.IsSuccess && response.Message.Place is not null
                    ? ApiResponse<VendorBranchPlaceDto>.Success(response.Message.Place)
                    : ApiResponse<VendorBranchPlaceDto>.Failure(response.Message.ErrorCode);
            }, cancellationToken);
        }

        public async Task<ApiResponse<IEnumerable<VendorBranchPlaceDto>>> GetPlacesAsync(IEnumerable<Guid> placeIds, CancellationToken cancellationToken = default)
        {
            return await ExecuteManyPlacesRequestAsync(async ct =>
            {
                var operationId = Guid.NewGuid();
                var response = await _getManyClient.GetResponse<GetVendorBranchPlacesResponse>(
                    new GetVendorBranchPlacesRequest(operationId, placeIds.Distinct().ToList()),
                    ct);

                return response.Message.IsSuccess
                    ? ApiResponse<IEnumerable<VendorBranchPlaceDto>>.Success(response.Message.Places)
                    : ApiResponse<IEnumerable<VendorBranchPlaceDto>>.Failure(response.Message.ErrorCode);
            }, cancellationToken);
        }

        public async Task<ApiResponse<string>> DeletePlaceAsync(Guid placeId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _resiliencePipeline.ExecuteAsync<ApiResponse<string>>(async ct =>
                {
                    var operationId = Guid.NewGuid();
                    var response = await _deleteClient.GetResponse<DeleteVendorBranchPlaceResponse>(
                        new DeleteVendorBranchPlaceRequest(operationId, placeId),
                        ct);

                    return response.Message.IsSuccess
                        ? ApiResponse<string>.Success(response.Message.Message ?? "Vendor place deleted successfully")
                        : ApiResponse<string>.Failure(response.Message.ErrorCode);
                });
            }
            catch (RequestTimeoutException ex)
            {
                _logger.LogError(ex, "Deleting vendor branch place {PlaceId} timed out", placeId);
                return ApiResponse<string>.Failure(ErrorCode.Timeout);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Deleting vendor branch place {PlaceId} failed", placeId);
                return ApiResponse<string>.Failure(ErrorCode.ExternalServiceError);
            }
        }

        private async Task<ApiResponse<VendorBranchPlaceDto>> ExecutePlaceRequestAsync(
            Func<CancellationToken, Task<ApiResponse<VendorBranchPlaceDto>>> action,
            CancellationToken cancellationToken)
        {
            try
            {
                return await _resiliencePipeline.ExecuteAsync<ApiResponse<VendorBranchPlaceDto>>(async ct =>
                    await action(ct));
            }
            catch (RequestTimeoutException ex)
            {
                _logger.LogError(ex, "Vendor branch place request timed out");
                return ApiResponse<VendorBranchPlaceDto>.Failure(ErrorCode.Timeout);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Vendor branch place request failed");
                return ApiResponse<VendorBranchPlaceDto>.Failure(ErrorCode.ExternalServiceError);
            }
        }

        private async Task<ApiResponse<IEnumerable<VendorBranchPlaceDto>>> ExecuteManyPlacesRequestAsync(
            Func<CancellationToken, Task<ApiResponse<IEnumerable<VendorBranchPlaceDto>>>> action,
            CancellationToken cancellationToken)
        {
            try
            {
                return await _resiliencePipeline.ExecuteAsync<ApiResponse<IEnumerable<VendorBranchPlaceDto>>>(async ct =>
                    await action(ct));
            }
            catch (RequestTimeoutException ex)
            {
                _logger.LogError(ex, "Vendor branch places request timed out");
                return ApiResponse<IEnumerable<VendorBranchPlaceDto>>.Failure(ErrorCode.Timeout);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Vendor branch places request failed");
                return ApiResponse<IEnumerable<VendorBranchPlaceDto>>.Failure(ErrorCode.ExternalServiceError);
            }
        }
    }
}
