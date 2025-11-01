using System.ComponentModel.DataAnnotations;

namespace backend_project.DTOs.Auth;

public class RefreshTokenDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
