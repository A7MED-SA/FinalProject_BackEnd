namespace Athary.Application.DTOs.Email;

public sealed record EmailMessageDto
{
    public List<string> ToEmails { get; init; } = new();
    public List<string> CcEmails { get; init; } = new();
    public List<string> BccEmails { get; init; } = new();
    public string Subject { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public bool IsHtml { get; init; } = true;
    public List<EmailAttachmentDto> Attachments { get; init; } = new();
}
