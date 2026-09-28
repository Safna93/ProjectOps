using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ProjectOps.Tests;

public class ProjectApiIntegrationTests
{
    [Fact]
    public async Task Login_WithValidAdminCredentials_ReturnsJwtToken()
    {
        // Arrange
        using var factory = new ProjectOpsApiFactory();
        using var client = factory.CreateClient();
        await factory.SeedTestUsersAsync();

        // Act
        var response = await client.PostAsJsonAsync("/api/Auth/login", new
        {
            username = "admin",
            password = ProjectOpsApiFactory.AdminPassword
        });
        var login = await response.Content.ReadFromJsonAsync<LoginResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(login);
        Assert.False(string.IsNullOrWhiteSpace(login.Token));
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        // Arrange
        using var factory = new ProjectOpsApiFactory();
        using var client = factory.CreateClient();
        await factory.SeedTestUsersAsync();

        // Act
        var response = await client.PostAsJsonAsync("/api/Auth/login", new
        {
            username = "admin",
            password = "incorrect-test-password"
        });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetProjects_WithoutJwt_ReturnsUnauthorized()
    {
        // Arrange
        using var factory = new ProjectOpsApiFactory();
        using var client = factory.CreateClient();
        await factory.SeedTestUsersAsync();

        // Act
        var response = await client.GetAsync("/api/projects");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateProject_AsUser_ReturnsForbidden()
    {
        // Arrange
        using var factory = new ProjectOpsApiFactory();
        using var client = factory.CreateClient();
        await factory.SeedTestUsersAsync();
        var loginResponse = await client.PostAsJsonAsync("/api/Auth/login", new
        {
            username = "user",
            password = ProjectOpsApiFactory.UserPassword
        });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        Assert.NotNull(login);

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/projects")
        {
            Content = JsonContent.Create(new
            {
                projectCode = "INT001",
                projectName = "Integration Test Project",
                clientName = "Test Client",
                status = "Planning"
            })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private sealed record LoginResponse(string Token, string Role);
}
