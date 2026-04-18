using System.ComponentModel.DataAnnotations;
using backend_project.Models;

namespace backend_project.DTOs.TeacherRequests.Requests;

public class ProcessTeacherRequestDto
{
    [Required(ErrorMessage = "الحالة مطلوبة")]
    public TeacherRequestStatus Status { get; set; }

    [StringLength(500, ErrorMessage = "الملاحظات يجب ألا تتجاوز 500 حرف")]
    public string? AdminNotes { get; set; }

    [StringLength(500, ErrorMessage = "سبب الرفض يجب ألا يتجاوز 500 حرف")]
    public string? RejectionReason { get; set; }
}