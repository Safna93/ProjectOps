using System.ComponentModel.DataAnnotations;

namespace ProjectOps.Api.Models;

public class AppUser
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Role { get; set; } = string.Empty;
}