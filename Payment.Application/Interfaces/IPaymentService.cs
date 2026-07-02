using Shared.Application.Events.Payments;

namespace Payment.Application.Interfaces
{
    public interface IPaymentService
    {
        Task<ProcessPaymentResponse> ProcessPaymentAsync(
            ProcessPaymentRequest request,
            CancellationToken cancellationToken = default);
    }
}
