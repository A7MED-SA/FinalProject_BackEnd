namespace backend_project.Configuration;

public class MinioSettings
{
    public string Endpoint { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public bool UseSSL { get; set; } = false;
    public int PresignedUrlExpiryMinutes { get; set; } = 60;

    // Bucket names
    public string VideosBucket { get; set; } = "videos";
    public string DocumentsBucket { get; set; } = "documents";
    public string ImagesBucket { get; set; } = "images";
    public string RecordingsBucket { get; set; } = "recordings";
    public string CertificatesBucket { get; set; } = "certificates";
    public string PrivateBucket { get; set; } = "private";
}
