using System.ComponentModel.DataAnnotations;
using backend_project.Models;

namespace backend_project.DTOs.Auth;

public class RegisterDto
{
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "������ ���������� �����")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare("Password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public Gender? Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }

    // --- ������ ������ ---
    [Phone]
    public string? PhoneNumber { get; set; }

    public string? Country { get; set; }
    public string? City { get; set; }
    public string? StreetLine1 { get; set; } // ��� Street ������
    public string? PostalCode { get; set; }  // ��� ���� ������ �� �������
}