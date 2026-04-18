using System.ComponentModel.DataAnnotations;
using backend_project.Models;

namespace backend_project.DTOs.TeacherRequests.Requests;

public class UpdateTeacherRequestDto
{
    [Required(ErrorMessage = "الرسالة مطلوبة")]
    [StringLength(1000, ErrorMessage = "الرسالة يجب ألا تتجاوز 1000 حرف")]
    public string Message { get; set; } = string.Empty;

    [Required(ErrorMessage = "يجب إرفاق مستندات")]
    [MinLength(1, ErrorMessage = "يجب إرفاق مستند واحد على الأقل")]
    public List<TeacherRequestDocumentDto> Documents { get; set; } = new();
}