using Application.UnitTests.Common;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Places.Application.DTOs.PlaceCategory;
using Places.Application.Interfaces;
using Places.Application.Services;
using Places.Domain.Entities;
using Shared.Domain.Enums;

namespace Application.UnitTests.Places;

public class PlaceCategoryServiceTests
{
    private readonly IPlacesRepository<PlaceCategory> _categories = Substitute.For<IPlacesRepository<PlaceCategory>>();
    private readonly IPlacesRepository<Place> _places = Substitute.For<IPlacesRepository<Place>>();

    [Fact]
    public async Task DeletePlaceCategoryAsync_ReturnsNotFound_WhenCategoryDoesNotExist()
    {
        // Arrange: the category table has no matching category.
        _categories.GetTable().Returns(AsyncQueryable.From<PlaceCategory>());
        _places.GetTable().Returns(AsyncQueryable.From<Place>());
        var service = CreateService();

        // Act: delete a missing category.
        var result = await service.DeletePlaceCategoryAsync(Guid.NewGuid());

        // Assert: no soft-delete write is attempted.
        result.IsSuccess.Should().BeFalse();
        result.errorCode.Should().Be(ErrorCode.NotFound);
        _categories.DidNotReceiveWithAnyArgs().SaveInclude(default!);
        await _categories.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    [Fact]
    public async Task DeletePlaceCategoryAsync_ReturnsValidationError_WhenPlacesStillUseCategory()
    {
        // Arrange: the category exists and still has places attached to it.
        var categoryId = Guid.NewGuid();
        _categories.GetTable().Returns(AsyncQueryable.From(new PlaceCategory { Id = categoryId, Name = "Museums" }));
        _places.GetTable().Returns(AsyncQueryable.From(new Place { Id = Guid.NewGuid(), PlaceCategoryId = categoryId }));

        var service = CreateService();

        // Act: try to delete an in-use category.
        var result = await service.DeletePlaceCategoryAsync(categoryId);

        // Assert: the business guard prevents orphaning places.
        result.IsSuccess.Should().BeFalse();
        result.errorCode.Should().Be(ErrorCode.ValidationError);
        _categories.DidNotReceiveWithAnyArgs().SaveInclude(default!);
    }

    [Fact]
    public async Task CreatePlaceCategoryAsync_AddsCategory_AndSaves()
    {
        // Arrange: category creation is a simple mapper plus repository write.
        PlaceCategory? addedCategory = null;
        _categories.When(repository => repository.Add(Arg.Any<PlaceCategory>()))
            .Do(call => addedCategory = call.Arg<PlaceCategory>());

        var service = CreateService();

        // Act: create a category.
        var result = await service.CreatePlaceCategoryAsync(new CreatePlaceCategoryDto
        {
            Name = "Parks",
            Description = "Outdoor places"
        });

        // Assert: the mapped category is persisted.
        result.IsSuccess.Should().BeTrue();
        addedCategory.Should().NotBeNull();
        addedCategory!.Name.Should().Be("Parks");
        addedCategory.Description.Should().Be("Outdoor places");
        await _categories.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private PlaceCategoryService CreateService()
    {
        return new PlaceCategoryService(
            _categories,
            _places,
            NullLogger<PlaceCategoryService>.Instance);
    }
}
