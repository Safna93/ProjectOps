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

    public async Task<PagedResult<ProjectDto>> GetAllAsync(
        string? search = null,
        string? status = null,
        string? sortBy = null,
        string? sortDirection = null,
        int pageNumber = 1,
        int pageSize = 10)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 10 : pageSize;

        IQueryable<Project> query = _dbContext.Projects
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim();
            query = query.Where(project =>
                project.ProjectCode.Contains(searchTerm) ||
                project.ProjectName.Contains(searchTerm) ||
                project.ClientName.Contains(searchTerm));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var statusFilter = status.Trim();
            query = query.Where(project => project.Status == statusFilter);
        }

        var sortField = sortBy?.Trim().ToLowerInvariant();
        var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        query = (sortField, descending) switch
        {
            ("projectname", false) => query.OrderBy(project => project.ProjectName),
            ("projectname", true) => query.OrderByDescending(project => project.ProjectName),
            ("clientname", false) => query.OrderBy(project => project.ClientName),
            ("clientname", true) => query.OrderByDescending(project => project.ClientName),
            ("status", false) => query.OrderBy(project => project.Status),
            ("status", true) => query.OrderByDescending(project => project.Status),
            ("createdat", false) => query.OrderBy(project => project.CreatedAt),
            ("createdat", true) => query.OrderByDescending(project => project.CreatedAt),
            (_, true) => query.OrderByDescending(project => project.ProjectCode),
            _ => query.OrderBy(project => project.ProjectCode)
        };

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        if (totalPages > 0 && pageNumber > totalPages)
        {
            pageNumber = totalPages;
        }

        var projects = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<ProjectDto>
        {
            Items = projects.Select(ToDto).ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
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