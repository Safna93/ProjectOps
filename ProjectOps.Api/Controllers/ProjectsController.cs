using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Text;
using ProjectOps.Api.Dtos;
using ProjectOps.Api.Services;

namespace ProjectOps.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private const long MaxDocumentSize = 5 * 1024 * 1024;
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

    [HttpPost("{id:int}/document")]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxDocumentSize + 64 * 1024)]
    public async Task<ActionResult<ProjectDto>> UploadDocument(int id, [FromForm] IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("Select a non-empty PDF file.");
        }

        if (file.Length > MaxDocumentSize)
        {
            return StatusCode(StatusCodes.Status413PayloadTooLarge, "The PDF must be 5 MB or smaller.");
        }

        if (!string.Equals(Path.GetExtension(file.FileName), ".pdf", StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(file.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase) ||
            !await HasPdfSignatureAsync(file))
        {
            return BadRequest("Only PDF files are allowed.");
        }

        var project = await _projectService.UploadDocumentAsync(id, file);
        if (project is null)
        {
            return NotFound();
        }

        _logger.LogInformation(
            "Project document uploaded. ProjectId: {ProjectId}, FileName: {FileName}",
            id,
            project.DocumentFileName);

        return Ok(project);
    }

    [HttpGet("{id:int}/document")]
    public async Task<IActionResult> DownloadDocument(int id)
    {
        var document = await _projectService.GetDocumentAsync(id);
        if (document is null)
        {
            return NotFound();
        }

        return PhysicalFile(document.Value.PhysicalPath, "application/pdf", document.Value.FileName);
    }

    private static async Task<bool> HasPdfSignatureAsync(IFormFile file)
    {
        await using var stream = file.OpenReadStream();
        var signature = new byte[5];
        var bytesRead = await stream.ReadAsync(signature.AsMemory());

        return bytesRead == signature.Length && Encoding.ASCII.GetString(signature) == "%PDF-";
    }

    [HttpGet("test-error")]
    public IActionResult TestError()
    {
        throw new Exception("Test exception for Stage 10 verification.");
    }
}
