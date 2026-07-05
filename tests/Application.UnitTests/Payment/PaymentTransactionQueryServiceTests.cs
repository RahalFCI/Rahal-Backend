using Application.UnitTests.Common;
using FluentAssertions;
using MassTransit;
using NSubstitute;
using NSubstitute.Core;
using Payment.Application.DTOs.Transactions;
using Payment.Application.Services;
using Payment.Domain.Entities;
using Payment.Domain.Enums;
using Shared.Application.Events.Gamification;
using Shared.Application.Interfaces;
using Shared.Application.Pagination;
using Shared.Domain.Enums;

namespace Application.UnitTests.Payment;

public class PaymentTransactionQueryServiceTests
{
    private readonly IGenericRepository<PaymentTransaction> _payments = Substitute.For<IGenericRepository<PaymentTransaction>>();
    private readonly IRequestClient<GetExplorerPaymentProfilesRequest> _explorersClient = Substitute.For<IRequestClient<GetExplorerPaymentProfilesRequest>>();

    [Fact]
    public async Task GetTransactionsAsync_ReturnsEmptyPage_WhenDisplayNameMatchesNoExplorers()
    {
        // Arrange: a display-name filter is resolved before querying payments.
        _payments.GetTable().Returns(AsyncQueryable.From(CreatePayments().AsEnumerable()));
        StubExplorerResponse(new GetExplorerPaymentProfilesResponse(true, ErrorCode.None, Array.Empty<ExplorerPaymentProfileDto>()));

        var service = CreateService();

        // Act: ask for a name that Gamification cannot resolve.
        var result = await service.GetTransactionsAsync(new PaymentTransactionFilterDto
        {
            ExplorerDisplayName = "missing",
            Pagination = new OffsetPaginationRequest { Page = 2, PageSize = 5 }
        });

        // Assert: payment data is not scanned when the name filter has no matching users.
        result.IsSuccess.Should().BeTrue();
        result.Data.Items.Should().BeEmpty();
        result.Data.TotalCount.Should().Be(0);
        result.Data.Page.Should().Be(2);
        result.Data.PageSize.Should().Be(5);
    }

    [Fact]
    public async Task GetTransactionsAsync_FiltersByResolvedExplorerDisplayName_AndReusesNameResult()
    {
        // Arrange: Gamification translates the display-name filter to an explorer id.
        var alphaId = Guid.NewGuid();
        var betaId = Guid.NewGuid();
        _payments.GetTable().Returns(AsyncQueryable.From(CreatePayments(alphaId, betaId).AsEnumerable()));

        StubExplorerResponse(new GetExplorerPaymentProfilesResponse(
            true,
            ErrorCode.None,
            new[] { new ExplorerPaymentProfileDto(alphaId, "Alpha Explorer") }));

        var service = CreateService();

        // Act: filter by display name and status.
        var result = await service.GetTransactionsAsync(new PaymentTransactionFilterDto
        {
            ExplorerDisplayName = "Alpha",
            Status = PaymentStatus.Succeeded,
            Pagination = new OffsetPaginationRequest { Page = 1, PageSize = 10 }
        });

        // Assert: only Alpha's succeeded transaction is returned, with the already resolved display name.
        result.IsSuccess.Should().BeTrue();
        result.Data.TotalCount.Should().Be(1);
        result.Data.Items.Should().ContainSingle()
            .Which.ExplorerDisplayName.Should().Be("Alpha Explorer");

        await _explorersClient.Received(1).GetResponse<GetExplorerPaymentProfilesResponse>(
            Arg.Any<GetExplorerPaymentProfilesRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetTransactionsAsync_ResolvesNamesForCurrentPage_WhenNoDisplayNameFilterIsProvided()
    {
        // Arrange: without a display-name filter, names are fetched after the payment page is selected.
        var alphaId = Guid.NewGuid();
        var betaId = Guid.NewGuid();
        _payments.GetTable().Returns(AsyncQueryable.From(CreatePayments(alphaId, betaId).AsEnumerable()));

        StubExplorerResponse(call =>
        {
            var request = call.Arg<GetExplorerPaymentProfilesRequest>();
            request.DisplayName.Should().BeNull();
            request.ExplorerIds.Should().NotBeNull();

            return new GetExplorerPaymentProfilesResponse(
                true,
                ErrorCode.None,
                new[]
                {
                    new ExplorerPaymentProfileDto(alphaId, "Alpha Explorer"),
                    new ExplorerPaymentProfileDto(betaId, "Beta Voyager")
                });
        });

        var service = CreateService();

        // Act: query succeeded USD payments without a display-name filter.
        var result = await service.GetTransactionsAsync(new PaymentTransactionFilterDto
        {
            Status = PaymentStatus.Succeeded,
            Currency = " USD ",
            Pagination = new OffsetPaginationRequest { Page = 1, PageSize = 10 }
        });

        // Assert: the payments are filtered locally and decorated with names from Gamification.
        result.IsSuccess.Should().BeTrue();
        result.Data.TotalCount.Should().Be(2);
        result.Data.Items.Select(item => item.ExplorerDisplayName)
            .Should().BeEquivalentTo("Alpha Explorer", "Beta Voyager");
    }

    [Fact]
    public async Task GetTransactionsAsync_UsesUnknownExplorer_WhenNameLookupFails()
    {
        // Arrange: payment rows can still be returned even if the remote name lookup fails.
        var explorerId = Guid.NewGuid();
        _payments.GetTable().Returns(AsyncQueryable.From(CreatePayments(explorerId, Guid.NewGuid()).AsEnumerable()));

        StubExplorerResponse(new GetExplorerPaymentProfilesResponse(false, ErrorCode.ExternalServiceError, Array.Empty<ExplorerPaymentProfileDto>()));

        var service = CreateService();

        // Act: request a page without filtering by display name.
        var result = await service.GetTransactionsAsync(new PaymentTransactionFilterDto
        {
            TransactionId = CreatePayments(explorerId, Guid.NewGuid())[0].Id
        });

        // Assert: transaction data remains available with a safe placeholder name.
        result.IsSuccess.Should().BeTrue();
        result.Data.Items.Should().ContainSingle()
            .Which.ExplorerDisplayName.Should().Be("Unknown Explorer");
    }

    private PaymentTransactionQueryService CreateService()
    {
        return new PaymentTransactionQueryService(_payments, _explorersClient);
    }

    private void StubExplorerResponse(GetExplorerPaymentProfilesResponse message)
    {
        var response = Substitute.For<Response<GetExplorerPaymentProfilesResponse>>();
        response.Message.Returns(message);

        _explorersClient.GetResponse<GetExplorerPaymentProfilesResponse>(
                Arg.Any<GetExplorerPaymentProfilesRequest>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));
    }

    private void StubExplorerResponse(Func<CallInfo, GetExplorerPaymentProfilesResponse> factory)
    {
        _explorersClient.GetResponse<GetExplorerPaymentProfilesResponse>(
                Arg.Any<GetExplorerPaymentProfilesRequest>(),
                Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var response = Substitute.For<Response<GetExplorerPaymentProfilesResponse>>();
                response.Message.Returns(factory(call));
                return Task.FromResult(response);
            });
    }

    private static List<PaymentTransaction> CreatePayments(Guid? alphaId = null, Guid? betaId = null)
    {
        var alpha = alphaId ?? Guid.NewGuid();
        var beta = betaId ?? Guid.NewGuid();

        return new List<PaymentTransaction>
        {
            new()
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                ExplorerId = alpha,
                OperationId = Guid.NewGuid(),
                ReferenceId = Guid.NewGuid(),
                Amount = 10,
                Currency = "usd",
                Status = PaymentStatus.Succeeded,
                Gateway = PaymentGatewayType.Stripe,
                CreatedAt = new DateTime(2026, 7, 3, 10, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                ExplorerId = alpha,
                OperationId = Guid.NewGuid(),
                ReferenceId = Guid.NewGuid(),
                Amount = 15,
                Currency = "eur",
                Status = PaymentStatus.Failed,
                Gateway = PaymentGatewayType.Stripe,
                CreatedAt = new DateTime(2026, 7, 2, 10, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                ExplorerId = beta,
                OperationId = Guid.NewGuid(),
                ReferenceId = Guid.NewGuid(),
                Amount = 20,
                Currency = "usd",
                Status = PaymentStatus.Succeeded,
                Gateway = PaymentGatewayType.Stripe,
                CreatedAt = new DateTime(2026, 7, 1, 10, 0, 0, DateTimeKind.Utc)
            }
        };
    }
}
