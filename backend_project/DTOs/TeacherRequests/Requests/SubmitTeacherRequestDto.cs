using System.ComponentModel.DataAnnotations;
using backend_project.Models;

namespace backend_project.DTOs.TeacherRequests.Requests;

public class SubmitTeacherRequestDto
{
    [Required(ErrorMessage = "الرسالة مطلوبة")]
    [StringLength(1000, ErrorMessage = "الرسالة يجب ألا تتجاوز 1000 حرف")]
    public string Message { get; set; } = string.Empty;

    [Required(ErrorMessage = "يجب إرفاق مستندات")]
    [MinLength(1, ErrorMessage = "يجب إرفاق مستند واحد على الأقل")]
    public List<TeacherRequestDocumentDto> Documents { get; set; } = new();
}


public class TeacherRequestDocumentDto
{
    [Required(ErrorMessage = "نوع المستند مطلوب")]
    public DocumentType DocumentType { get; set; }

    // مطلوب إذا كان نوع المستند Cv أو Certificate
    public Guid? FileId { get; set; }

    // مطلوب إذا كان نوع المستند PortfolioLink
    [Url(ErrorMessage = "رابط غير صالح")]
    [StringLength(500, ErrorMessage = "الرابط يجب ألا يتجاوز 500 حرف")]
    public string? UrlValue { get; set; }
}