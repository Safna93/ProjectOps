using ProjectOps.Api.Dtos;
using Microsoft.AspNetCore.Http;

namespace ProjectOps.Api.Services;

public interface IProjectService
{
    Task<PagedResult<ProjectDto>> GetAllAsync(
        string? search = null,
        string? status = null,
        string? sortBy = null,
        string? sortDirection = null,
        int pageNumber = 1,
        int pageSize = 10);

    Task<ProjectDto?> GetByIdAsync(int id);

    Task<ProjectDto> CreateAsync(CreateProjectDto projectDto, string username);

    Task<ProjectDto?> UpdateAsync(int id, UpdateProjectDto projectDto, string username);

    Task<bool> DeleteAsync(int id);

    Task<ProjectDto?> UploadDocumentAsync(int id, IFormFile file);

    Task<(string PhysicalPath, string FileName)?> GetDocumentAsync(int id);
}