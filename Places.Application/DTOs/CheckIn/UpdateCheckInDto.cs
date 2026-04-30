using Places.Domain.Enums;

namespace Places.Application.DTOs.CheckIn
{
    public class UpdateCheckInDto
    {
        public CheckInValidationStatus ValidationStatus { get; set; }
    }
}
