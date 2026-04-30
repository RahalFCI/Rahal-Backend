using Shared.Application.DTOs;
using Shared.Domain.Enums;

namespace Places.Application.DTOs.CheckIn
{
    public class CheckInValidationResult
    {
        public bool IsValid { get; set; }
        public int RiskScore { get; set; }
        public List<string> FailedSignals { get; set; } = new();
        public ErrorCode? ErrorCode { get; set; }
        public string? Reason { get; set; }

        public static CheckInValidationResult HardFailure(ErrorCode errorCode, string reason)
        {
            return new CheckInValidationResult
            {
                IsValid = false,
                ErrorCode = errorCode,
                Reason = reason
            };
        }

        public static CheckInValidationResult FromScore(int score, List<string> signals)
        {
            var isValid = score <= 60;
            var result = new CheckInValidationResult
            {
                RiskScore = score,
                IsValid = isValid,
                FailedSignals = signals
            };

            if (!isValid)
            {
                result.ErrorCode = Shared.Domain.Enums.ErrorCode.LocationSpoofingDetected;
                result.Reason = $"Risk score {score} exceeds threshold. Signals: {string.Join(", ", signals)}";
            }

            return result;
        }
    }
}
