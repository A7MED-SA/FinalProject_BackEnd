namespace Athary.Application.DTOs.Email;

public sealed record EmailAttachmentDto
{
    public string FileName { get; init; } = string.Empty;
    public byte[] Content { get; init; } = Array.Empty<byte>();
    public string ContentType { get; init; } = string.Empty;
}
