using System.ComponentModel.DataAnnotations;

namespace ProjectOps.Api.Models;

public class Project
{
    public int Id { get; set; }

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

    public DateTime CreatedAt { get; set; }

    [Required]
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public string? DocumentFileName { get; set; }

    public string? DocumentStoredName { get; set; }
}