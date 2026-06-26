using Gamification.Application.DTOs.VendorBranches;
using Gamification.Domain.Entities;

namespace Gamification.Application.Mappers
{
    public static class VendorPlaceMapper
    {
        public static VendorPlace ToEntity(Guid vendorId, Guid placeId, CreateVendorBranchDto dto)
        {
            return new VendorPlace
            {
                VendorId = vendorId,
                PlaceId = placeId,
                BranchName = dto.BranchName,
                PhoneNumber = dto.PhoneNumber,
                Notes = dto.Notes,
                IsPrimary = dto.IsPrimary,
                IsActive = true
            };
        }

        public static VendorBranchDto ToDto(VendorPlace vendorPlace)
        {
            return new VendorBranchDto
            {
                Id = vendorPlace.Id,
                VendorId = vendorPlace.VendorId,
                PlaceId = vendorPlace.PlaceId,
                BranchName = vendorPlace.BranchName,
                PhoneNumber = vendorPlace.PhoneNumber,
                Notes = vendorPlace.Notes,
                IsPrimary = vendorPlace.IsPrimary,
                IsActive = vendorPlace.IsActive
            };
        }
    }
}
