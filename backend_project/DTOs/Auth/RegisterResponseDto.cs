namespace backend_project.DTOs.Auth;

public class RegisterResponseDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = null!;
    public string Message { get; set; } = null!;
}
