using Application.UnitTests.Common;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Rewards.Application.DTOs.PlanTiers;
using Rewards.Application.Interfaces;
using Rewards.Application.Services;
using Rewards.Domain.Entities;
using Shared.Domain.Enums;

namespace Application.UnitTests.Rewards;

public class PlanTierServiceTests
{
    private readonly IRewardsRepository<PlanTier> _planTiers = Substitute.For<IRewardsRepository<PlanTier>>();

    [Fact]
    public async Task CreateAsync_ReturnsAlreadyExists_WhenNameIsDuplicate()
    {
        // Arrange: plan-tier names must be unique.
        _planTiers.GetTable().Returns(AsyncQueryable.From(new PlanTier { Id = Guid.NewGuid(), Name = "Gold" }));
        var service = CreateService();

        // Act: try to create another tier with the same name.
        var result = await service.CreateAsync(new CreatePlanTierDto { Name = "Gold" });

        // Assert: the duplicate is rejected before insert.
        result.IsSuccess.Should().BeFalse();
        result.errorCode.Should().Be(ErrorCode.AlreadyExists);
        _planTiers.DidNotReceiveWithAnyArgs().Add(default!);
        await _planTiers.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    [Fact]
    public async Task CreateAsync_AddsPlanTier_WhenNameIsUnique()
    {
        // Arrange: no existing tier has the requested name.
        PlanTier? addedTier = null;
        _planTiers.GetTable().Returns(AsyncQueryable.From<PlanTier>());
        _planTiers.When(repository => repository.Add(Arg.Any<PlanTier>()))
            .Do(call => addedTier = call.Arg<PlanTier>());

        var service = CreateService();

        // Act: create a new plan tier.
        var result = await service.CreateAsync(new CreatePlanTierDto
        {
            Name = "Premium",
            WeeklyPrice = 9.99m,
            WeeklyXpCost = 100,
            XpMultiplier = 2,
            MaxTravelPlans = 5
        });

        // Assert: the tier is mapped and saved.
        result.IsSuccess.Should().BeTrue();
        addedTier.Should().NotBeNull();
        addedTier!.Name.Should().Be("Premium");
        addedTier.WeeklyPrice.Should().Be(9.99m);
        await _planTiers.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesPlanTier_WhenItExists()
    {
        // Arrange: soft delete should mark the tier rather than removing the row.
        var tier = new PlanTier { Id = Guid.NewGuid(), Name = "Starter" };
        _planTiers.GetByIdAsync(tier.Id, Arg.Any<CancellationToken>()).Returns(tier);

        var service = CreateService();

        // Act: delete the tier.
        var result = await service.DeleteAsync(tier.Id);

        // Assert: the audit deletion fields are set and saved.
        result.IsSuccess.Should().BeTrue();
        tier.IsDeleted.Should().BeTrue();
        tier.DeletedAt.Should().NotBeNull();
        await _planTiers.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private PlanTierService CreateService()
    {
        return new PlanTierService(_planTiers, NullLogger<PlanTierService>.Instance);
    }
}
