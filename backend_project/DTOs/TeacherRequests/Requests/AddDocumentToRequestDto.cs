using System.ComponentModel.DataAnnotations;
using backend_project.Models;

namespace backend_project.DTOs.TeacherRequests.Requests;

public class AddDocumentToRequestDto
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