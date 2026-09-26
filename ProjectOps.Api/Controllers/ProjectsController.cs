using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectOps.Api.Dtos;
using ProjectOps.Api.Services;

namespace ProjectOps.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly ILogger<ProjectsController> _logger;

    public ProjectsController(IProjectService projectService, ILogger<ProjectsController> logger)
    {
        _projectService = projectService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjectDto>>> GetProjects()
    {
        _logger.LogInformation("Retrieving projects.");
        var projects = await _projectService.GetAllAsync();
        return Ok(projects);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProjectDto>> CreateProject(CreateProjectDto projectDto)
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(username))
        {
            return Unauthorized();
        }

        var project = await _projectService.CreateAsync(projectDto, username);

        _logger.LogInformation(
            "Project created successfully. ProjectId: {ProjectId}, ProjectCode: {ProjectCode}",
            project.Id,
            project.ProjectCode);

        return CreatedAtAction(nameof(GetProjects), null, project);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateProject(int id, UpdateProjectDto projectDto)
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(username))
        {
            return Unauthorized();
        }

        var project = await _projectService.UpdateAsync(id, projectDto, username);
        if (project is null)
        {
            _logger.LogWarning("Project not found for update. ProjectId: {ProjectId}", id);
            return NotFound();
        }

        _logger.LogInformation("Project updated successfully. ProjectId: {ProjectId}", id);

        return NoContent();
    }
     [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteProject(int id)
    {
        var deleted = await _projectService.DeleteAsync(id);
        if (!deleted)
        {
            _logger.LogWarning("Project not found for deletion. ProjectId: {ProjectId}", id);
            return NotFound();
        }

        _logger.LogInformation("Project deleted successfully. ProjectId: {ProjectId}", id);

        return NoContent();
    }

    [HttpGet("test-error")]
    public IActionResult TestError()
    {
        throw new Exception("Test exception for Stage 10 verification.");
    }
}
