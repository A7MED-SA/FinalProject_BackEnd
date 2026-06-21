namespace Athary.Application.Interfaces.Media;

public interface IObjectStorage
{
    Task<PresignedUploadResult> GenerateUploadUrlAsync(
        string bucket,
        string objectKey,
        string contentType,
        long maxSizeBytes,
        int expiryMinutes = 15);
    string GetPublicUrl(string bucket, string objectPath);

    Task<string> GenerateViewUrlAsync(
        string bucket,
        string objectKey,
        int expiryMinutes = 60);

    Task<bool> ObjectExistsAsync(string bucket, string objectKey);

    Task<ObjectMetadata?> GetObjectMetadataAsync(string bucket, string objectKey);

    Task DeleteObjectAsync(string bucket, string objectKey);

    Task EnsureBucketExistsAsync(string bucket);

    Task CopyObjectAsync(string sourceBucket, string sourceKey, string destBucket, string destKey);
}

public sealed record PresignedUploadResult
{
    public string UploadUrl { get; init; } = string.Empty;
    public string ObjectKey { get; init; } = string.Empty;
    public string Bucket { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public Dictionary<string, string> RequiredHeaders { get; init; } = new();
}

public sealed record ObjectMetadata
{
    public long SizeBytes { get; init; }
    public string ContentType { get; init; } = string.Empty;
    public DateTime LastModified { get; init; }
    public string ETag { get; init; } = string.Empty;
}
