using System;

namespace backend_project.DTOs.Document;

public class UpdateDocumentDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsDownloadable { get; set; }
    public bool RequiresSubscription { get; set; }
}
