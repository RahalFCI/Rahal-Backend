using Places.Application.DTOs.CheckIn;
using Places.Domain.Entities;

namespace Places.Application.Interfaces
{
    public interface ICheckInValidatorService
    {
        Task<CheckInValidationResult> ValidateAsync(
            CheckInRequestDto request,
            Guid explorerId,
            Place place,
            CancellationToken ct = default);
    }
}
