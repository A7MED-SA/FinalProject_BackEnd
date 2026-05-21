using System;

namespace backend_project.DTOs.Document;

public class CreateDocumentDto
{
    public Guid SectionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid FileId { get; set; }
    public bool IsDownloadable { get; set; } = true;
    public bool RequiresSubscription { get; set; } = true;
}
