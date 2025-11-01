namespace backend_project.DTOs.Email;

public class EmailMessageDto
{
    public List<string> ToEmails { get; set; } = new List<string>();
    public List<string> CcEmails { get; set; } = new List<string>();
    public List<string> BccEmails { get; set; } = new List<string>();
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; } = true;
    public List<EmailAttachmentDto> Attachments { get; set; } = new List<EmailAttachmentDto>();
}