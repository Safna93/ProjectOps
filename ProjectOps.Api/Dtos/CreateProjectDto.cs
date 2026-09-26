using System.ComponentModel.DataAnnotations;

namespace ProjectOps.Api.Dtos;

public class CreateProjectDto
{
    [Required]
    [StringLength(20)]
    public string ProjectCode { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string ProjectName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string ClientName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = string.Empty;
}