using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectOps.Api.Data;
using ProjectOps.Api.Models;
using ProjectOps.Api.Services;

namespace ProjectOps.Tests;

public class AuthServiceTests
{
    [Fact]
    public async Task AuthenticateAsync_ReturnsUser_WhenPasswordIsCorrect()
    {
        // Arrange
        using var context = CreateContext();
        var hasher = new PasswordHasher<AppUser>();
        var user = AddUser(context, hasher, "admin", "test-password", "Admin");
        var service = new AuthService(context, hasher);

        // Act
        var authenticatedUser = await service.AuthenticateAsync("admin", "test-password");

        // Assert
        Assert.NotNull(authenticatedUser);
        Assert.Equal(user.Id, authenticatedUser.Id);
    }

    [Fact]
    public async Task AuthenticateAsync_ReturnsNull_WhenPasswordIsWrong()
    {
        // Arrange
        using var context = CreateContext();
        var hasher = new PasswordHasher<AppUser>();
        AddUser(context, hasher, "admin", "correct-password", "Admin");
        var service = new AuthService(context, hasher);

        // Act
        var authenticatedUser = await service.AuthenticateAsync("admin", "wrong-password");

        // Assert
        Assert.Null(authenticatedUser);
    }

    [Fact]
    public async Task AuthenticateAsync_ReturnsNull_WhenUsernameIsUnknown()
    {
        // Arrange
        using var context = CreateContext();
        var hasher = new PasswordHasher<AppUser>();
        var service = new AuthService(context, hasher);

        // Act
        var authenticatedUser = await service.AuthenticateAsync("missing-user", "any-password");

        // Assert
        Assert.Null(authenticatedUser);
    }

    [Fact]
    public async Task AuthenticateAsync_ReturnsDatabaseRole_WhenCredentialsAreCorrect()
    {
        // Arrange
        using var context = CreateContext();
        var hasher = new PasswordHasher<AppUser>();
        AddUser(context, hasher, "user", "test-password", "User");
        var service = new AuthService(context, hasher);

        // Act
        var authenticatedUser = await service.AuthenticateAsync("user", "test-password");

        // Assert
        Assert.NotNull(authenticatedUser);
        Assert.Equal("User", authenticatedUser.Role);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static AppUser AddUser(
        AppDbContext context,
        IPasswordHasher<AppUser> hasher,
        string username,
        string password,
        string role)
    {
        var user = new AppUser { Username = username, Role = role };
        user.PasswordHash = hasher.HashPassword(user, password);
        context.AppUsers.Add(user);
        context.SaveChanges();
        return user;
    }
}