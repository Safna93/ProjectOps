using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using ProjectOps.Api.Data;
using ProjectOps.Api.Models;

namespace ProjectOps.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public ProjectsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Project>>> GetProjects()
    {
        var projects = await _dbContext.Projects
            .AsNoTracking()
            .ToListAsync();

        return Ok(projects);
    }

    [HttpPost]
    public async Task<ActionResult<Project>> CreateProject(Project project)
    {
        _dbContext.Projects.Add(project);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProjects), null, project);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProject(int id, Project project)
    {
        var existingProject = await _dbContext.Projects.FindAsync(id);

        if (existingProject is null)
        {
            return NotFound();
        }

        existingProject.ProjectCode = project.ProjectCode;
        existingProject.ProjectName = project.ProjectName;
        existingProject.ClientName = project.ClientName;
        existingProject.Status = project.Status;

        await _dbContext.SaveChangesAsync();

        return NoContent();
    }
     [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProject(int id)
    {
        var existingProject = await _dbContext.Projects.FindAsync(id);

        if (existingProject is null)
        {
            return NotFound();
        }

        _dbContext.Projects.Remove(existingProject);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }
      
    }
