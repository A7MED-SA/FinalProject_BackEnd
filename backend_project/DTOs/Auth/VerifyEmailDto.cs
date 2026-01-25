using System.ComponentModel.DataAnnotations;

namespace backend_project.DTOs.Auth;

public class VerifyEmailDto
{
    [Required(ErrorMessage = "Token is required")]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;
}