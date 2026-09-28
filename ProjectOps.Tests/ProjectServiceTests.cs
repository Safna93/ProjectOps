using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using ProjectOps.Api.Data;
using ProjectOps.Api.Dtos;
using ProjectOps.Api.Models;
using ProjectOps.Api.Services;

namespace ProjectOps.Tests;

public class ProjectServiceTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsSavedProjects()
    {
        // Arrange
        using var context = CreateContext();
        context.Projects.Add(new Project
        {
            ProjectCode = "T001",
            ProjectName = "Test Project",
            ClientName = "Test Client",
            Status = "Planning",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "admin"
        });
        await context.SaveChangesAsync();
        var service = CreateService(context);

        // Act
        var projects = await service.GetAllAsync();

        // Assert
        var project = Assert.Single(projects.Items);
        Assert.Equal("T001", project.ProjectCode);
        Assert.Equal("Test Project", project.ProjectName);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsRequestedPageAndMetadata()
    {
        // Arrange
        using var context = CreateContext();
        for (var projectNumber = 1; projectNumber <= 12; projectNumber++)
        {
            context.Projects.Add(new Project
            {
                ProjectCode = $"T{projectNumber:000}",
                ProjectName = $"Test Project {projectNumber}",
                ClientName = "Test Client",
                Status = "Planning",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "admin"
            });
        }

        await context.SaveChangesAsync();
        var service = CreateService(context);

        // Act
        var page = await service.GetAllAsync(pageNumber: 2, pageSize: 5);

        // Assert
        Assert.Equal(5, page.Items.Count);
        Assert.Equal(["T006", "T007", "T008", "T009", "T010"],
            page.Items.Select(project => project.ProjectCode));
        Assert.Equal(12, page.TotalCount);
        Assert.Equal(3, page.TotalPages);
        Assert.Equal(2, page.PageNumber);
        Assert.Equal(5, page.PageSize);
    }

    [Fact]
    public async Task GetAllAsync_UsesDefaultsForInvalidPaginationValues()
    {
        // Arrange
        using var context = CreateContext();
        var service = CreateService(context);

        // Act
        var page = await service.GetAllAsync(pageNumber: 0, pageSize: 0);

        // Assert
        Assert.Equal(1, page.PageNumber);
        Assert.Equal(10, page.PageSize);
        Assert.Empty(page.Items);
        Assert.Equal(0, page.TotalPages);
    }

    [Fact]
    public async Task CreateAsync_SavesProjectAndAuditFields()
    {
        // Arrange
        using var context = CreateContext();
        var service = CreateService(context);
        var projectDto = new CreateProjectDto
        {
            ProjectCode = "T002",
            ProjectName = "New Project",
            ClientName = "Test Client",
            Status = "Planning"
        };
        var beforeCreate = DateTime.UtcNow;

        // Act
        var createdProject = await service.CreateAsync(projectDto, "admin");

        // Assert
        Assert.Equal("admin", createdProject.CreatedBy);
        Assert.True(createdProject.CreatedAt >= beforeCreate);
        Assert.Null(createdProject.UpdatedAt);
        Assert.Null(createdProject.UpdatedBy);
        Assert.Equal(1, await context.Projects.CountAsync());
    }

    [Fact]
    public async Task UpdateAsync_UpdatesProjectAndAuditFields()
    {
        // Arrange
        using var context = CreateContext();
        var originalCreatedAt = new DateTime(2026, 9, 28, 10, 0, 0, DateTimeKind.Utc);
        context.Projects.Add(new Project
        {
            ProjectCode = "T003",
            ProjectName = "Original Project",
            ClientName = "Original Client",
            Status = "Planning",
            CreatedAt = originalCreatedAt,
            CreatedBy = "first-admin"
        });
        await context.SaveChangesAsync();
        var projectId = await context.Projects.Select(project => project.Id).SingleAsync();
        var service = CreateService(context);
        var updateDto = new UpdateProjectDto
        {
            ProjectCode = "T003",
            ProjectName = "Updated Project",
            ClientName = "Updated Client",
            Status = "Completed"
        };

        // Act
        var updatedProject = await service.UpdateAsync(projectId, updateDto, "admin");

        // Assert
        Assert.NotNull(updatedProject);
        Assert.Equal("Updated Project", updatedProject.ProjectName);
        Assert.Equal("Completed", updatedProject.Status);
        Assert.Equal(originalCreatedAt, updatedProject.CreatedAt);
        Assert.Equal("first-admin", updatedProject.CreatedBy);
        Assert.Equal("admin", updatedProject.UpdatedBy);
        Assert.NotNull(updatedProject.UpdatedAt);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNullForMissingProject()
    {
        // Arrange
        using var context = CreateContext();
        var service = CreateService(context);

        // Act
        var project = await service.GetByIdAsync(12345);

        // Assert
        Assert.Null(project);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCachedProjectOnSecondRead()
    {
        // Arrange
        using var context = CreateContext();
        var projectEntity = new Project
        {
            ProjectCode = "T004",
            ProjectName = "Original Project",
            ClientName = "Test Client",
            Status = "Planning",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "admin"
        };
        context.Projects.Add(projectEntity);
        await context.SaveChangesAsync();
        var service = CreateService(context);
        var originalProject = await service.GetByIdAsync(projectEntity.Id);

        projectEntity.ProjectName = "Changed directly in database";
        await context.SaveChangesAsync();

        // Act
        var cachedProject = await service.GetByIdAsync(projectEntity.Id);

        // Assert
        Assert.NotNull(originalProject);
        Assert.NotNull(cachedProject);
        Assert.Equal("Original Project", cachedProject.ProjectName);
    }

    [Fact]
    public async Task UpdateAndDeleteAsync_InvalidateCachedProject()
    {
        // Arrange
        using var context = CreateContext();
        var projectEntity = new Project
        {
            ProjectCode = "T005",
            ProjectName = "Original Project",
            ClientName = "Test Client",
            Status = "Planning",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "admin"
        };
        context.Projects.Add(projectEntity);
        await context.SaveChangesAsync();
        var service = CreateService(context);
        var cachedProject = await service.GetByIdAsync(projectEntity.Id);
        Assert.NotNull(cachedProject);

        var updateDto = new UpdateProjectDto
        {
            ProjectCode = "T005",
            ProjectName = "Updated Project",
            ClientName = "Test Client",
            Status = "Completed",
            RowVersion = cachedProject.RowVersion
        };

        // Act
        var updatedProject = await service.UpdateAsync(projectEntity.Id, updateDto, "admin");
        var projectAfterUpdate = await service.GetByIdAsync(projectEntity.Id);
        var deleted = await service.DeleteAsync(projectEntity.Id);
        var projectAfterDelete = await service.GetByIdAsync(projectEntity.Id);

        // Assert
        Assert.NotNull(updatedProject);
        Assert.NotNull(projectAfterUpdate);
        Assert.Equal("Updated Project", projectAfterUpdate.ProjectName);
        Assert.True(deleted);
        Assert.Null(projectAfterDelete);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNullForMissingProject()
    {
        // Arrange
        using var context = CreateContext();
        var service = CreateService(context);
        var updateDto = new UpdateProjectDto
        {
            ProjectCode = "T999",
            ProjectName = "Missing Project",
            ClientName = "Test Client",
            Status = "Planning"
        };

        // Act
        var project = await service.UpdateAsync(12345, updateDto, "admin");

        // Assert
        Assert.Null(project);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static ProjectService CreateService(AppDbContext context)
    {
        var environment = new TestWebHostEnvironment();
        return new ProjectService(
            context,
            environment,
            new MemoryCache(new MemoryCacheOptions()),
            NullLogger<ProjectService>.Instance);
    }

    private sealed class TestWebHostEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "ProjectOps.Tests";

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();

        public string ContentRootPath { get; set; } = Path.GetTempPath();

        public string EnvironmentName { get; set; } = "Testing";

        public string WebRootPath { get; set; } = Path.GetTempPath();

        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    }
}