using System.ComponentModel.DataAnnotations;

namespace ProjectOps.Web.Models;

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
}