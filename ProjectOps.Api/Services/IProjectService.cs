using ProjectOps.Api.Dtos;

namespace ProjectOps.Api.Services;

public interface IProjectService
{
    Task<IReadOnlyList<ProjectDto>> GetAllAsync();

    Task<ProjectDto?> GetByIdAsync(int id);

    Task<ProjectDto> CreateAsync(CreateProjectDto projectDto, string username);

    Task<ProjectDto?> UpdateAsync(int id, UpdateProjectDto projectDto, string username);

    Task<bool> DeleteAsync(int id);
}