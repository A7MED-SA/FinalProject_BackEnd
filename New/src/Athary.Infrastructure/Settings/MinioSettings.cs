namespace Athary.Infrastructure.Settings;

public sealed class MinioSettings
{
    public string Endpoint { get; set; } = "localhost:9000";
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public bool UseSsl { get; set; } = false;
    public string PrivateBucket { get; set; } = "private";
    public string ImagesBucket { get; set; } = "images";
    public string VideosBucket { get; set; } = "videos";
    public string DocumentsBucket { get; set; } = "documents";
    public string RecordingsBucket { get; set; } = "recordings";
    public string CertificatesBucket { get; set; } = "certificates";
    public int PresignedUrlExpiryMinutes { get; set; } = 60;
    public string Region { get; set; } = "us-east-1";
}
