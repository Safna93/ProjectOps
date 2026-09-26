using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectOps.Api.Dtos;
using ProjectOps.Api.Services;

namespace ProjectOps.Api.Controllers;

[ApiController]
[Route("api/projects")]
[Authorize]
public class ExternalProjectController : ControllerBase
{
    private readonly IExternalProjectService _externalProjectService;

    public ExternalProjectController(IExternalProjectService externalProjectService)
    {
        _externalProjectService = externalProjectService;
    }

    [HttpGet("{id:int}/external-status")]
    [ProducesResponseType<ExternalProjectStatusDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status504GatewayTimeout)]
    public async Task<ActionResult<ExternalProjectStatusDto>> GetExternalProjectStatus(
        int id,
        [FromQuery] string mode = "success")
    {
        try
        {
            return Ok(await _externalProjectService.GetProjectStatusAsync(id, mode));
        }
        catch (TimeoutException)
        {
            return Problem(
                statusCode: StatusCodes.Status504GatewayTimeout,
                title: "The downstream service timed out.");
        }
        catch (HttpRequestException)
        {
            return Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "The downstream service is unavailable.");
        }
    }
}