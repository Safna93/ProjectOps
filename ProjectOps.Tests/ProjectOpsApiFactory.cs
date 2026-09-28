using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ProjectOps.Api.Data;
using ProjectOps.Api.Models;

namespace ProjectOps.Tests;

public sealed class ProjectOpsApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"ProjectOpsIntegrationTests_{Guid.NewGuid():N}";

    public const string AdminPassword = "Admin-Integration-Test-123!";
    public const string UserPassword = "User-Integration-Test-123!";

    static ProjectOpsApiFactory()
    {
        Environment.SetEnvironmentVariable(
            "Jwt__Key",
            "ProjectOps-Integration-Test-Signing-Key-Only-123456789");
        Environment.SetEnvironmentVariable("Jwt__Issuer", "ProjectOps.Api");
        Environment.SetEnvironmentVariable("Jwt__Audience", "ProjectOps.Client");
        Environment.SetEnvironmentVariable("Jwt__ExpiryMinutes", "60");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<AppDbContext>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<DbContextOptions>();
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });
    }

    public async Task SeedTestUsersAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<AppUser>>();

        var admin = new AppUser { Username = "admin", Role = "Admin" };
        admin.PasswordHash = passwordHasher.HashPassword(admin, AdminPassword);

        var user = new AppUser { Username = "user", Role = "User" };
        user.PasswordHash = passwordHasher.HashPassword(user, UserPassword);

        dbContext.AppUsers.AddRange(admin, user);
        await dbContext.SaveChangesAsync();
    }
}
