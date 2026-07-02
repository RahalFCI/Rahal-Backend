using Shared.Domain.Enums;

namespace Payment.Application.DTOs.Webhooks
{
    public record PaymentWebhookHandleResult(
        bool IsSuccess,
        ErrorCode ErrorCode,
        string? Message = null)
    {
        public static PaymentWebhookHandleResult Success(string? message = null) =>
            new(true, ErrorCode.None, message);

        public static PaymentWebhookHandleResult Failure(ErrorCode errorCode, string? message = null) =>
            new(false, errorCode, message);
    }
}
