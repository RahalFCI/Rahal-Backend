using FluentAssertions;
using MassTransit;
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

public class PaymentWebhookServiceTests
{
    private readonly IPaymentGateway _gateway = Substitute.For<IPaymentGateway>();
    private readonly IGenericRepository<PaymentTransaction> _payments = Substitute.For<IGenericRepository<PaymentTransaction>>();
    private readonly IPublishEndpoint _publisher = Substitute.For<IPublishEndpoint>();

    [Fact]
    public async Task HandleGatewayWebhookAsync_IgnoresWebhook_WhenGatewayReturnsNoPaymentResult()
    {
        // Arrange: unsupported Stripe event types are represented as a null parsed result.
        _gateway.ParsePaymentWebhook("{}", "sig").Returns((GatewayWebhookPaymentResult?)null);
        var service = CreateService();

        // Act: handle the ignored webhook.
        var result = await service.HandleGatewayWebhookAsync("{}", "sig");

        // Assert: no database lookup or event publish is needed.
        result.IsSuccess.Should().BeTrue();
        await _payments.DidNotReceiveWithAnyArgs().GetByExpression(default!, default);
        await _publisher.DidNotReceiveWithAnyArgs().Publish<PaymentHandled>(default!, default);
    }

    [Fact]
    public async Task HandleGatewayWebhookAsync_ReturnsNotFound_WhenPaymentIntentIsUnknown()
    {
        // Arrange: the gateway event references a payment intent we do not have locally.
        _gateway.ParsePaymentWebhook("{}", "sig").Returns(new GatewayWebhookPaymentResult(
            "evt_1",
            "payment_intent.succeeded",
            "pi_missing",
            PaymentStatus.Succeeded,
            null));

        _payments.GetByExpression(Arg.Any<System.Linq.Expressions.Expression<Func<PaymentTransaction, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((PaymentTransaction?)null);

        var service = CreateService();

        // Act: handle the webhook.
        var result = await service.HandleGatewayWebhookAsync("{}", "sig");

        // Assert: the webhook fails cleanly and does not publish a domain event.
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.NotFound);
        await _publisher.DidNotReceiveWithAnyArgs().Publish<PaymentHandled>(default!, default);
    }

    [Fact]
    public async Task HandleGatewayWebhookAsync_UpdatesPayment_AndPublishesPaymentHandled()
    {
        // Arrange: an existing payment should be updated with the gateway outcome.
        var payment = new PaymentTransaction
        {
            Id = Guid.NewGuid(),
            OperationId = Guid.NewGuid(),
            ExplorerId = Guid.NewGuid(),
            ReferenceId = Guid.NewGuid(),
            Amount = 12.5m,
            Currency = "usd",
            GatewayPaymentIntentId = "pi_123",
            Status = PaymentStatus.Pending
        };

        _gateway.ParsePaymentWebhook("{}", "sig").Returns(new GatewayWebhookPaymentResult(
            "evt_1",
            "payment_intent.succeeded",
            "pi_123",
            PaymentStatus.Succeeded,
            null));

        _payments.GetByExpression(Arg.Any<System.Linq.Expressions.Expression<Func<PaymentTransaction, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(payment);

        var service = CreateService();

        // Act: handle the gateway event.
        var result = await service.HandleGatewayWebhookAsync("{}", "sig");

        // Assert: the local transaction changes and an internal payment event is emitted.
        result.IsSuccess.Should().BeTrue();
        payment.Status.Should().Be(PaymentStatus.Succeeded);
        payment.UpdatedAt.Should().NotBeNull();

        await _payments.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _publisher.Received(1).Publish(
            Arg.Is<PaymentHandled>(message =>
                message.PaymentId == payment.Id &&
                message.OperationId == payment.OperationId &&
                message.Status == PaymentStatus.Succeeded.ToString()),
            Arg.Any<CancellationToken>());
    }

    private PaymentWebhookService CreateService()
    {
        return new PaymentWebhookService(_gateway, _payments, _publisher, NullLogger<PaymentWebhookService>.Instance);
    }
}
