using System.ComponentModel.DataAnnotations;

namespace backend_project.DTOs.Auth;

public record OAuthLoginDto
{
    [Required]
    public string IdToken { get; init; } = string.Empty;

    [Required]
    public string Provider { get; init; } = string.Empty; // "google" or "microsoft"
}
