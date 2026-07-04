using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Payment.Application.DTOs.Gateway;
using Payment.Application.Interfaces;
using Payment.Application.Services;
using Payment.Domain.Entities;
using Payment.Domain.Enums;
using Shared.Application.Events.Payments;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;

namespace Application.UnitTests.Payment;

public class PaymentServiceTests
{
    private readonly IPaymentGateway _gateway = Substitute.For<IPaymentGateway>();
    private readonly IGenericRepository<PaymentTransaction> _payments = Substitute.For<IGenericRepository<PaymentTransaction>>();

    [Fact]
    public async Task ProcessPaymentAsync_ReturnsValidationError_WhenAmountIsInvalid()
    {
        // Arrange: an invalid amount should be rejected before touching Stripe or the database.
        var service = CreateService();
        var request = ValidRequest() with { Amount = 0 };

        // Act: call the application service directly.
        var result = await service.ProcessPaymentAsync(request);

        // Assert: the service fails fast and avoids side effects.
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.ValidationError);
        _payments.DidNotReceiveWithAnyArgs().Add(default!);
        await _gateway.DidNotReceiveWithAnyArgs().CreatePaymentIntentAsync(default!, default);
    }

    [Fact]
    public async Task ProcessPaymentAsync_CreatesPaymentIntent_AndPersistsGatewayData()
    {
        // Arrange: the gateway returns the data the mobile client needs to open PaymentSheet.
        PaymentTransaction? addedPayment = null;
        _payments.When(repository => repository.Add(Arg.Any<PaymentTransaction>()))
            .Do(call => addedPayment = call.Arg<PaymentTransaction>());

        _gateway.PublishableKey.Returns("pk_test_123");
        _gateway.CreatePaymentIntentAsync(Arg.Any<CreatePaymentIntentGatewayRequest>(), Arg.Any<CancellationToken>())
            .Returns(new CreatePaymentIntentGatewayResult(
                "pi_123",
                "pi_secret_123",
                "cus_123",
                "ephkey_123",
                PaymentStatus.RequiresPaymentMethod));

        var service = CreateService();
        var request = ValidRequest() with { Amount = 10.50m, Currency = " USD " };

        // Act: process the payment request through the application service.
        var result = await service.ProcessPaymentAsync(request);

        // Assert: the persisted transaction and response both reflect the gateway result.
        result.IsSuccess.Should().BeTrue();
        result.PaymentIntentClientSecret.Should().Be("pi_secret_123");
        result.CustomerId.Should().Be("cus_123");
        result.EphemeralKeySecret.Should().Be("ephkey_123");
        result.PublishableKey.Should().Be("pk_test_123");

        addedPayment.Should().NotBeNull();
        addedPayment!.AmountMinor.Should().Be(1050);
        addedPayment.Currency.Should().Be("usd");
        addedPayment.GatewayPaymentIntentId.Should().Be("pi_123");
        addedPayment.GatewayCustomerId.Should().Be("cus_123");
        addedPayment.Status.Should().Be(PaymentStatus.RequiresPaymentMethod);

        await _payments.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessPaymentAsync_UsesOneMinorUnitMultiplier_ForZeroDecimalCurrencies()
    {
        // Arrange: JPY should not be multiplied by 100.
        PaymentTransaction? addedPayment = null;
        _payments.When(repository => repository.Add(Arg.Any<PaymentTransaction>()))
            .Do(call => addedPayment = call.Arg<PaymentTransaction>());

        _gateway.CreatePaymentIntentAsync(Arg.Any<CreatePaymentIntentGatewayRequest>(), Arg.Any<CancellationToken>())
            .Returns(new CreatePaymentIntentGatewayResult("pi_jpy", "secret", null, null, PaymentStatus.RequiresPaymentMethod));

        var service = CreateService();

        // Act: process a zero-decimal currency amount.
        await service.ProcessPaymentAsync(ValidRequest() with { Amount = 1050m, Currency = "JPY" });

        // Assert: the gateway amount uses the same whole-unit value.
        addedPayment.Should().NotBeNull();
        addedPayment!.AmountMinor.Should().Be(1050);
        addedPayment.Currency.Should().Be("jpy");
    }

    [Fact]
    public async Task ProcessPaymentAsync_MarksPaymentFailed_WhenGatewayThrows()
    {
        // Arrange: gateway failure should still leave a failed transaction record for reconciliation.
        PaymentTransaction? addedPayment = null;
        _payments.When(repository => repository.Add(Arg.Any<PaymentTransaction>()))
            .Do(call => addedPayment = call.Arg<PaymentTransaction>());

        _gateway.CreatePaymentIntentAsync(Arg.Any<CreatePaymentIntentGatewayRequest>(), Arg.Any<CancellationToken>())
            .Returns<Task<CreatePaymentIntentGatewayResult>>(_ => throw new InvalidOperationException("stripe unavailable"));

        var service = CreateService();

        // Act: process a request while the gateway is unavailable.
        var result = await service.ProcessPaymentAsync(ValidRequest());

        // Assert: the caller receives an external-service failure and the transaction is marked failed.
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.ExternalServiceError);
        addedPayment.Should().NotBeNull();
        addedPayment!.Status.Should().Be(PaymentStatus.Failed);
        addedPayment.FailureMessage.Should().Be("stripe unavailable");
        await _payments.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private PaymentService CreateService()
    {
        return new PaymentService(_gateway, _payments, NullLogger<PaymentService>.Instance);
    }

    private static ProcessPaymentRequest ValidRequest()
    {
        return new ProcessPaymentRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            20m,
            "usd",
            "card",
            Guid.NewGuid());
    }
}
