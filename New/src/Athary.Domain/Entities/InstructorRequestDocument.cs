using Athary.Domain.Enums;

namespace Athary.Domain.Entities;

public sealed class InstructorRequestDocument : BaseEntity
{
    public Guid RequestId { get; set; }
    public DocumentType DocumentType { get; set; }
    public Guid? FileId { get; set; }
    public string? UrlValue { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public InstructorRequest Request { get; set; } = null!;
    public UploadedFile? File { get; set; }
}
