namespace ProjectOps.Api.Dtos;

public class ExternalProjectStatusDto
{
    public int ProjectId { get; set; }

    public string Status { get; set; } = string.Empty;

    public string Scenario { get; set; } = string.Empty;
}