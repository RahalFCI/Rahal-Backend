using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Places.Application.DTOs.Place;
using Places.Application.Interfaces;
using Places.Application.Mappers;
using Places.Domain.Entities;
using Shared.Application.Events.VendorBranches;
using Shared.Domain.Enums;

namespace Places.Application.EventConsumers
{
    public class VendorBranchPlaceRequestConsumer :
        IConsumer<CreateVendorBranchPlaceRequest>,
        IConsumer<UpdateVendorBranchPlaceRequest>,
        IConsumer<GetVendorBranchPlaceRequest>,
        IConsumer<GetVendorBranchPlacesRequest>,
        IConsumer<DeleteVendorBranchPlaceRequest>
    {
        private static readonly Guid VendorPlaceCategoryId = Guid.Parse("c6666666-6666-6666-6666-666666666666");
        private readonly IPlaceService _placeService;
        private readonly IPlacesRepository<Place> _placeRepository;
        private readonly ILogger<VendorBranchPlaceRequestConsumer> _logger;

        public VendorBranchPlaceRequestConsumer(
            IPlaceService placeService,
            IPlacesRepository<Place> placeRepository,
            ILogger<VendorBranchPlaceRequestConsumer> logger)
        {
            _placeService = placeService;
            _placeRepository = placeRepository;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<CreateVendorBranchPlaceRequest> context)
        {
            var request = context.Message;
            var result = await _placeService.CreatePlaceAsync(new CreatePlaceDto
            {
                Name = request.Name,
                Description = request.Description,
                PlaceCategoryId = VendorPlaceCategoryId,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                GeoFenceRange = request.GeoFenceRange,
                Address = MapAddress(request.Address)
            }, context.CancellationToken);

            if (!result.IsSuccess)
            {
                await context.RespondAsync(new CreateVendorBranchPlaceResponse(request.OperationId, false, result.errorCode, null));
                return;
            }

            var placeResult = await _placeService.GetPlaceByIdAsync(result.Data, context.CancellationToken);
            await context.RespondAsync(new CreateVendorBranchPlaceResponse(
                request.OperationId,
                placeResult.IsSuccess,
                placeResult.errorCode,
                placeResult.IsSuccess ? MapPlace(placeResult.Data) : null));
        }

        public async Task Consume(ConsumeContext<UpdateVendorBranchPlaceRequest> context)
        {
            var request = context.Message;
            var existing = await _placeService.GetPlaceByIdAsync(request.PlaceId, context.CancellationToken);
            if (!existing.IsSuccess || existing.Data.PlaceCategoryId != VendorPlaceCategoryId)
            {
                await context.RespondAsync(new UpdateVendorBranchPlaceResponse(request.OperationId, false, ErrorCode.NotFound, null));
                return;
            }

            var result = await _placeService.UpdatePlaceAsync(request.PlaceId, new UpdatePlaceDto
            {
                Name = request.Name,
                Description = request.Description,
                PlaceCategoryId = VendorPlaceCategoryId,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                GeoFenceRange = request.GeoFenceRange,
                Address = MapAddress(request.Address)
            }, context.CancellationToken);

            if (!result.IsSuccess)
            {
                await context.RespondAsync(new UpdateVendorBranchPlaceResponse(request.OperationId, false, result.errorCode, null));
                return;
            }

            var placeResult = await _placeService.GetPlaceByIdAsync(request.PlaceId, context.CancellationToken);
            await context.RespondAsync(new UpdateVendorBranchPlaceResponse(
                request.OperationId,
                placeResult.IsSuccess,
                placeResult.errorCode,
                placeResult.IsSuccess ? MapPlace(placeResult.Data) : null));
        }

        public async Task Consume(ConsumeContext<GetVendorBranchPlaceRequest> context)
        {
            var request = context.Message;
            var result = await _placeService.GetPlaceByIdAsync(request.PlaceId, context.CancellationToken);
            if (!result.IsSuccess || result.Data.PlaceCategoryId != VendorPlaceCategoryId)
            {
                await context.RespondAsync(new GetVendorBranchPlaceResponse(request.OperationId, false, ErrorCode.NotFound, null));
                return;
            }

            await context.RespondAsync(new GetVendorBranchPlaceResponse(request.OperationId, true, ErrorCode.None, MapPlace(result.Data)));
        }

        public async Task Consume(ConsumeContext<GetVendorBranchPlacesRequest> context)
        {
            var request = context.Message;
            var placeIds = request.PlaceIds.Distinct().ToList();
            var places = await _placeRepository.GetTable()
                .AsNoTracking()
                .Where(p => placeIds.Contains(p.Id) && p.PlaceCategoryId == VendorPlaceCategoryId)
                .ToListAsync(context.CancellationToken);

            await context.RespondAsync(new GetVendorBranchPlacesResponse(
                request.OperationId,
                true,
                ErrorCode.None,
                places.Select(p => MapPlace(PlaceMapper.ToGetDto(p)))));
        }

        public async Task Consume(ConsumeContext<DeleteVendorBranchPlaceRequest> context)
        {
            var request = context.Message;
            var existing = await _placeService.GetPlaceByIdAsync(request.PlaceId, context.CancellationToken);
            if (!existing.IsSuccess || existing.Data.PlaceCategoryId != VendorPlaceCategoryId)
            {
                await context.RespondAsync(new DeleteVendorBranchPlaceResponse(request.OperationId, false, ErrorCode.NotFound, null));
                return;
            }

            var result = await _placeService.DeletePlaceAsync(request.PlaceId, context.CancellationToken);
            await context.RespondAsync(new DeleteVendorBranchPlaceResponse(
                request.OperationId,
                result.IsSuccess,
                result.errorCode,
                result.IsSuccess ? "Vendor place deleted successfully" : null));

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed to delete vendor place {PlaceId}: {ErrorCode}", request.PlaceId, result.errorCode);
            }
        }

        private static AddressDto? MapAddress(VendorBranchPlaceAddressDto? address)
        {
            return address is null
                ? null
                : new AddressDto
                {
                    AddressLine = address.AddressLine,
                    Government = address.Government,
                    City = address.City,
                    Country = address.Country
                };
        }

        private static VendorBranchPlaceDto MapPlace(GetPlaceDto place)
        {
            return new VendorBranchPlaceDto(
                place.Id,
                place.Name,
                place.Description,
                place.Latitude,
                place.Longitude,
                place.GeoFenceRange,
                place.Address is null
                    ? null
                    : new VendorBranchPlaceAddressDto(
                        place.Address.AddressLine,
                        place.Address.Government,
                        place.Address.City,
                        place.Address.Country));
        }
    }
}
