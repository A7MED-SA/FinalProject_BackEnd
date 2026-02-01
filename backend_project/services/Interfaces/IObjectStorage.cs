using backend_project.Models;

namespace backend_project.Services.Interfaces;

public interface IObjectStorage
{
    /// <summary>
    /// Generate a presigned URL for uploading a file
    /// </summary>
    Task<PresignedUploadResult> GenerateUploadUrlAsync(
        string bucket,
        string objectKey,
        string contentType,
        long maxSizeBytes,
        int expiryMinutes = 15);
    string GetPublicUrl(string bucket, string objectPath);

    /// <summary>
    /// Generate a presigned URL for downloading/viewing a file
    /// </summary>
    Task<string> GenerateViewUrlAsync(
        string bucket,
        string objectKey,
        int expiryMinutes = 60);

    /// <summary>
    /// Check if an object exists
    /// </summary>
    Task<bool> ObjectExistsAsync(string bucket, string objectKey);

    /// <summary>
    /// Get object metadata (size, content type, etc.)
    /// </summary>
    Task<ObjectMetadata?> GetObjectMetadataAsync(string bucket, string objectKey);

    /// <summary>
    /// Delete an object permanently
    /// </summary>
    Task DeleteObjectAsync(string bucket, string objectKey);

    /// <summary>
    /// Ensure bucket exists
    /// </summary>
    Task EnsureBucketExistsAsync(string bucket);

    /// <summary>
    /// Copy object to another location
    /// </summary>
    Task CopyObjectAsync(string sourceBucket, string sourceKey, string destBucket, string destKey);
}

public class PresignedUploadResult
{
    public string UploadUrl { get; set; } = string.Empty;
    public string ObjectKey { get; set; } = string.Empty;
    public string Bucket { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public Dictionary<string, string> RequiredHeaders { get; set; } = new();
}

public class ObjectMetadata
{
    public long SizeBytes { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTime LastModified { get; set; }
    public string ETag { get; set; } = string.Empty;
}
