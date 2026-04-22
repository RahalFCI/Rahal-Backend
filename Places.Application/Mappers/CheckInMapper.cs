using Places.Application.DTOs.CheckIn;
using Places.Domain.Entities;
using Places.Domain.Enums;

namespace Places.Application.Mappers
{
    public static class CheckInMapper
    {
        public static GetCheckInDto ToGetDto(CheckIn checkIn)
        {
            return new GetCheckInDto
            {
                ExplorerId = checkIn.ExplorerId,
                PlaceId = checkIn.PlaceId,
                ValidationStatus = checkIn.ValidationStatus,
                PlaceName = checkIn.Place?.Name ?? string.Empty,
                ValidationStatusName = checkIn.ValidationStatus.ToString()
            };
        }

        public static CheckIn ToEntity(CreateCheckInDto dto)
        {
            return new CheckIn
            {
                ExplorerId = dto.ExplorerId,
                PlaceId = dto.PlaceId,
                ValidationStatus = CheckInValidationStatus.Pending
            };
        }

        public static void UpdateEntity(CheckIn checkIn, UpdateCheckInDto dto)
        {
            checkIn.ValidationStatus = dto.ValidationStatus;
        }

        public static IEnumerable<GetCheckInDto> ToGetDtos(IEnumerable<CheckIn> checkIns)
        {
            return checkIns.Select(ToGetDto);
        }
    }
}
