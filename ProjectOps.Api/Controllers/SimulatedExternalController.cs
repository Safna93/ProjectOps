using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectOps.Api.Dtos;

namespace ProjectOps.Api.Controllers;

[ApiController]
[Route("api/external")]
[AllowAnonymous]
public class SimulatedExternalController : ControllerBase
{
    private static readonly ConcurrentDictionary<int, int> TransientAttempts = new();
    private readonly ILogger<SimulatedExternalController> _logger;

    public SimulatedExternalController(ILogger<SimulatedExternalController> logger)
    {
        _logger = logger;
    }

    [HttpGet("project-status/{id:int}")]
    public async Task<ActionResult<ExternalProjectStatusDto>> GetProjectStatus(
        int id,
        [FromQuery] string mode = "success")
    {
        switch (mode.Trim().ToLowerInvariant())
        {
            case "success":
                return Ok(CreateStatus(id, "success"));

            case "transient":
                var attempt = TransientAttempts.AddOrUpdate(id, 1, (_, previous) => previous + 1);
                if (attempt % 2 == 1)
                {
                    _logger.LogWarning(
                        "Simulating a temporary downstream failure. ProjectId: {ProjectId}, Attempt: {Attempt}",
                        id,
                        attempt);
                    return StatusCode(StatusCodes.Status503ServiceUnavailable);
                }

                return Ok(CreateStatus(id, "transient-recovered"));

            case "slow":
                await Task.Delay(TimeSpan.FromSeconds(10), HttpContext.RequestAborted);
                return Ok(CreateStatus(id, "slow"));

            default:
                return BadRequest("Use mode=success, mode=transient, or mode=slow.");
        }
    }

    private static ExternalProjectStatusDto CreateStatus(int id, string scenario)
    {
        return new ExternalProjectStatusDto
        {
            ProjectId = id,
            Status = "Available",
            Scenario = scenario
        };
    }
}