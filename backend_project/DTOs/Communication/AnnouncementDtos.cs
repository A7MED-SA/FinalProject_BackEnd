namespace backend_project.DTOs.Communication;

public class CreateAnnouncementRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Target { get; set; } = "All";
    public Guid? CourseId { get; set; }
}

public class UpdateAnnouncementRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Target { get; set; } = "All";
    public Guid? CourseId { get; set; }
    public bool IsActive { get; set; } = true;
}

public class AnnouncementResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public Guid? CourseId { get; set; }
    public Guid CreatedBy { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? PublishedAt { get; set; }
}

public class AnnouncementListResponse
{
    public IEnumerable<AnnouncementResponse> Items { get; set; } = new List<AnnouncementResponse>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}
