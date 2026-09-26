using Microsoft.EntityFrameworkCore;
using ProjectOps.Api.Data;
using ProjectOps.Api.Dtos;
using ProjectOps.Api.Models;

namespace ProjectOps.Api.Services;

public class ProjectService : IProjectService
{
    private readonly AppDbContext _dbContext;

    public ProjectService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ProjectDto>> GetAllAsync()
    {
        var projects = await _dbContext.Projects
            .AsNoTracking()
            .ToListAsync();

        return projects.Select(ToDto).ToList();
    }

    public async Task<ProjectDto?> GetByIdAsync(int id)
    {
        var project = await _dbContext.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(project => project.Id == id);

        return project is null ? null : ToDto(project);
    }

    public async Task<ProjectDto> CreateAsync(CreateProjectDto projectDto, string username)
    {
        var project = new Project
        {
            ProjectCode = projectDto.ProjectCode,
            ProjectName = projectDto.ProjectName,
            ClientName = projectDto.ClientName,
            Status = projectDto.Status,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = username
        };

        _dbContext.Projects.Add(project);
        await _dbContext.SaveChangesAsync();

        return ToDto(project);
    }

    public async Task<ProjectDto?> UpdateAsync(int id, UpdateProjectDto projectDto, string username)
    {
        var project = await _dbContext.Projects.FindAsync(id);
        if (project is null)
        {
            return null;
        }

        project.ProjectCode = projectDto.ProjectCode;
        project.ProjectName = projectDto.ProjectName;
        project.ClientName = projectDto.ClientName;
        project.Status = projectDto.Status;
        project.UpdatedAt = DateTime.UtcNow;
        project.UpdatedBy = username;

        await _dbContext.SaveChangesAsync();

        return ToDto(project);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var project = await _dbContext.Projects.FindAsync(id);
        if (project is null)
        {
            return false;
        }

        _dbContext.Projects.Remove(project);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    private static ProjectDto ToDto(Project project)
    {
        return new ProjectDto
        {
            Id = project.Id,
            ProjectCode = project.ProjectCode,
            ProjectName = project.ProjectName,
            ClientName = project.ClientName,
            Status = project.Status,
            CreatedAt = project.CreatedAt,
            CreatedBy = project.CreatedBy,
            UpdatedAt = project.UpdatedAt,
            UpdatedBy = project.UpdatedBy
        };
    }
}