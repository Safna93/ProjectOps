using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using ProjectOps.Api.Data;
using ProjectOps.Api.Models;

namespace ProjectOps.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<ProjectsController> _logger;

    public ProjectsController(AppDbContext dbContext, ILogger<ProjectsController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Project>>> GetProjects()
    {
        _logger.LogInformation("Retrieving projects.");

        var projects = await _dbContext.Projects
            .AsNoTracking()
            .ToListAsync();

        return Ok(projects);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Project>> CreateProject(Project project)
    {
        _dbContext.Projects.Add(project);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "Project created successfully. ProjectId: {ProjectId}, ProjectCode: {ProjectCode}",
            project.Id,
            project.ProjectCode);

        return CreatedAtAction(nameof(GetProjects), null, project);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateProject(int id, Project project)
    {
        var existingProject = await _dbContext.Projects.FindAsync(id);

        if (existingProject is null)
        {
            _logger.LogWarning("Project not found for update. ProjectId: {ProjectId}", id);
            return NotFound();
        }

        existingProject.ProjectCode = project.ProjectCode;
        existingProject.ProjectName = project.ProjectName;
        existingProject.ClientName = project.ClientName;
        existingProject.Status = project.Status;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Project updated successfully. ProjectId: {ProjectId}", id);

        return NoContent();
    }
     [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteProject(int id)
    {
        var existingProject = await _dbContext.Projects.FindAsync(id);

        if (existingProject is null)
        {
            _logger.LogWarning("Project not found for deletion. ProjectId: {ProjectId}", id);
            return NotFound();
        }

        _dbContext.Projects.Remove(existingProject);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Project deleted successfully. ProjectId: {ProjectId}", id);

        return NoContent();
    }

    [HttpGet("test-error")]
    public IActionResult TestError()
    {
        throw new Exception("Test exception for Stage 10 verification.");
    }
}
