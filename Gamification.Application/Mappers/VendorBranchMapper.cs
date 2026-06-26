using Gamification.Application.DTOs.VendorBranches;
using Gamification.Domain.Entities;
using Shared.Application.Events.VendorBranches;

namespace Gamification.Application.Mappers
{
    public static class VendorBranchMapper
    {
        public static VendorBranch ToEntity(Guid placeId, CreateVendorBranchDto dto)
        {
            return new VendorBranch
            {
                VendorId = dto.VendorId,
                PlaceId = placeId,
                BranchName = dto.BranchName,
                PhoneNumber = dto.PhoneNumber,
                Notes = dto.Notes,
                IsActive = true
            };
        }

        public static GetVendorBranchDto ToGetDto(VendorBranch branch, VendorBranchPlaceDto place)
        {
            return new GetVendorBranchDto
            {
                Id = branch.Id,
                VendorId = branch.VendorId,
                PlaceId = branch.PlaceId,
                BranchName = branch.BranchName,
                PhoneNumber = branch.PhoneNumber,
                Notes = branch.Notes,
                IsActive = branch.IsActive,
                PlaceName = place.Name,
                Description = place.Description,
                Latitude = place.Latitude,
                Longitude = place.Longitude,
                GeoFenceRange = place.GeoFenceRange,
                Address = place.Address is null
                    ? null
                    : new GetVendorBranchAddressDto
                    {
                        AddressLine = place.Address.AddressLine,
                        Government = place.Address.Government,
                        City = place.Address.City,
                        Country = place.Address.Country
                    }
            };
        }
    }
}
