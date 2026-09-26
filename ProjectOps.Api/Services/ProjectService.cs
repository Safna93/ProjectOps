using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using ProjectOps.Api.Data;
using ProjectOps.Api.Dtos;
using ProjectOps.Api.Models;

namespace ProjectOps.Api.Services;

public class ProjectService : IProjectService
{
    private readonly AppDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;

    public ProjectService(AppDbContext dbContext, IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _environment = environment;
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

    public async Task<ProjectDto?> UploadDocumentAsync(int id, IFormFile file)
    {
        var project = await _dbContext.Projects.FindAsync(id);
        if (project is null)
        {
            return null;
        }

        var uploadDirectory = Path.Combine(_environment.ContentRootPath, "Uploads", "Projects");
        Directory.CreateDirectory(uploadDirectory);

        var storedName = $"{Guid.NewGuid():N}.pdf";
        var storedPath = Path.Combine(uploadDirectory, storedName);
        var previousStoredName = project.DocumentStoredName;

        await using (var fileStream = new FileStream(
            storedPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None))
        {
            await file.CopyToAsync(fileStream);
        }

        project.DocumentFileName = Path.GetFileName(file.FileName);
        project.DocumentStoredName = storedName;

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch
        {
            File.Delete(storedPath);
            throw;
        }

        if (!string.IsNullOrWhiteSpace(previousStoredName))
        {
            var previousPath = Path.Combine(uploadDirectory, Path.GetFileName(previousStoredName));
            if (File.Exists(previousPath))
            {
                File.Delete(previousPath);
            }
        }

        return ToDto(project);
    }

    public async Task<(string PhysicalPath, string FileName)?> GetDocumentAsync(int id)
    {
        var project = await _dbContext.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(project => project.Id == id);

        if (project is null ||
            string.IsNullOrWhiteSpace(project.DocumentStoredName) ||
            string.IsNullOrWhiteSpace(project.DocumentFileName))
        {
            return null;
        }

        var uploadDirectory = Path.Combine(_environment.ContentRootPath, "Uploads", "Projects");
        var physicalPath = Path.Combine(uploadDirectory, Path.GetFileName(project.DocumentStoredName));

        return File.Exists(physicalPath)
            ? (physicalPath, project.DocumentFileName)
            : null;
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
            UpdatedBy = project.UpdatedBy,
            DocumentFileName = project.DocumentFileName
        };
    }
}