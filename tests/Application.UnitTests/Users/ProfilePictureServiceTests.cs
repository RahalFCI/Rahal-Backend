using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shared.Application.Interfaces;
using Users.Application.Services;

namespace Application.UnitTests.Users;

public class ProfilePictureServiceTests
{
    private readonly IFileStorageService _storage = Substitute.For<IFileStorageService>();

    [Fact]
    public async Task UploadProfilePictureAsync_ReturnsNull_WhenFileIsMissing()
    {
        // Arrange: users can update their profile without providing a picture.
        var service = CreateService();

        // Act: upload no file.
        var result = await service.UploadProfilePictureAsync(null);

        // Assert: storage is not called.
        result.Should().BeNull();
        await _storage.DidNotReceiveWithAnyArgs().UploadAsync(default!, default);
    }

    [Fact]
    public async Task UploadProfilePictureAsync_UploadsNonEmptyFile()
    {
        // Arrange: a non-empty form file should be delegated to storage.
        var file = Substitute.For<IFormFile>();
        file.Length.Returns(128);
        _storage.UploadAsync(file, Arg.Any<CancellationToken>()).Returns("https://cdn/profile.jpg");

        var service = CreateService();

        // Act: upload the picture.
        var result = await service.UploadProfilePictureAsync(file);

        // Assert: the storage URL is returned.
        result.Should().Be("https://cdn/profile.jpg");
        await _storage.Received(1).UploadAsync(file, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateProfilePictureAsync_KeepsOldUrl_WhenNoNewFileIsProvided()
    {
        // Arrange: no new file means the old URL should remain unchanged.
        var service = CreateService();

        // Act: update without a new picture.
        var result = await service.UpdateProfilePictureAsync(null, "https://cdn/old.jpg");

        // Assert: no upload or delete happens.
        result.Should().Be("https://cdn/old.jpg");
        await _storage.DidNotReceiveWithAnyArgs().UploadAsync(default!, default);
        await _storage.DidNotReceiveWithAnyArgs().DeleteAsync(default!, default);
    }

    [Fact]
    public async Task UpdateProfilePictureAsync_UploadsNewFile_AndDeletesOldUrl()
    {
        // Arrange: replacing a profile picture should clean up the previous object.
        var file = Substitute.For<IFormFile>();
        file.Length.Returns(256);
        _storage.UploadAsync(file, Arg.Any<CancellationToken>()).Returns("https://cdn/new.jpg");

        var service = CreateService();

        // Act: update with a new picture.
        var result = await service.UpdateProfilePictureAsync(file, "https://cdn/old.jpg");

        // Assert: the new URL is returned and the old URL is deleted.
        result.Should().Be("https://cdn/new.jpg");
        await _storage.Received(1).UploadAsync(file, Arg.Any<CancellationToken>());
        await _storage.Received(1).DeleteAsync("https://cdn/old.jpg", Arg.Any<CancellationToken>());
    }

    private ProfilePictureService CreateService()
    {
        return new ProfilePictureService(_storage, NullLogger<ProfilePictureService>.Instance);
    }
}
