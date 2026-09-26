using ProjectOps.Api.Dtos;

namespace ProjectOps.Api.Services;

public interface IExternalProjectService
{
    Task<ExternalProjectStatusDto> GetProjectStatusAsync(int projectId, string mode);
}