using System.Net.Http.Json;
using ProjectOps.Api.Dtos;

namespace ProjectOps.Api.Services;

public class ExternalProjectService : IExternalProjectService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExternalProjectService> _logger;

    public ExternalProjectService(HttpClient httpClient, ILogger<ExternalProjectService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ExternalProjectStatusDto> GetProjectStatusAsync(int projectId, string mode)
    {
        _logger.LogInformation(
            "Requesting downstream project status. ProjectId: {ProjectId}, Mode: {Mode}",
            projectId,
            mode);

        try
        {
            var requestUri = $"api/external/project-status/{projectId}?mode={Uri.EscapeDataString(mode)}";
            var result = await _httpClient.GetFromJsonAsync<ExternalProjectStatusDto>(requestUri);

            return result ?? throw new HttpRequestException("The downstream service returned an empty response.");
        }
        catch (TaskCanceledException exception)
        {
            _logger.LogError(exception, "Downstream project status request timed out. ProjectId: {ProjectId}", projectId);
            throw new TimeoutException("The downstream service did not respond before the timeout.", exception);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception, "Downstream project status request failed. ProjectId: {ProjectId}", projectId);
            throw;
        }
    }
}