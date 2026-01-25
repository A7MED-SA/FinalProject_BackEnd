using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_project.Models;

[Table("teacher_request_documents")]
public class TeacherRequestDocument : BaseEntity
{

    [Required]
    [Column("request_id")]
    public Guid RequestId { get; set; }

    [Column("document_type")]
    [MaxLength(50)]
    public DocumentType DocumentType { get; set; }

    [Required]
    [Column("value")]
    [MaxLength(500)]
    public string Value { get; set; } = string.Empty;

    [Column("uploaded_at")]
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("RequestId")]
    public virtual TeacherRequest Request { get; set; } = null!;
}

public enum DocumentType
{
    Cv,
    Certificate,
    PortfolioLink
}
