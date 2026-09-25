namespace ProjectOps.Api.Models;

public class Project
{
    public int Id { get; set; }

    public string ProjectCode { get; set; } = string.Empty;

    public string ProjectName { get; set; } = string.Empty;

    public string ClientName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}