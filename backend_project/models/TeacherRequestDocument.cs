using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_project.Models;

[Table("teacher_request_documents")]
public class TeacherRequestDocument : BaseEntity
{
    [Required]
    [Column("request_id")]
    public Guid RequestId { get; set; }

    [Required]
    [Column("document_type")]
    public DocumentType DocumentType { get; set; }

    // Used when DocumentType = Cv or Certificate
    [Column("file_id")]
    public Guid? FileId { get; set; }

    // Used when DocumentType = PortfolioLink
    [Column("url_value")]
    [MaxLength(500)]
    public string? UrlValue { get; set; }

    [Column("uploaded_at")]
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(RequestId))]
    public TeacherRequest Request { get; set; } = null!;

    [ForeignKey(nameof(FileId))]
    public UploadedFile? File { get; set; }
}


public enum DocumentType
{
    CV,
    Certificate,
    IDCard,
    Degree,
    PortfolioLink,
    Transcript,
    Other
}