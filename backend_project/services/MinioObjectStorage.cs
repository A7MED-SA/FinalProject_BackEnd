using backend_project.Configuration;
using backend_project.Services.Interfaces;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace backend_project.Services;

public class MinioObjectStorage : IObjectStorage
{
    private readonly IMinioClient _minioClient;
    private readonly MinioSettings _settings;
    private readonly ILogger<MinioObjectStorage> _logger;

    public MinioObjectStorage(
        IMinioClient minioClient,
        IOptions<MinioSettings> settings,
        ILogger<MinioObjectStorage> logger)
    {
        _minioClient = minioClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<PresignedUploadResult> GenerateUploadUrlAsync(
        string bucket,
        string objectKey,
        string contentType,
        long maxSizeBytes,
        int expiryMinutes = 15)
    {
        await EnsureBucketExistsAsync(bucket);

        var args = new PresignedPutObjectArgs()
            .WithBucket(bucket)
            .WithObject(objectKey)
            .WithExpiry(expiryMinutes * 60);

        var uploadUrl = await _minioClient.PresignedPutObjectAsync(args);

        return new PresignedUploadResult
        {
            UploadUrl = uploadUrl,
            ObjectKey = objectKey,
            Bucket = bucket,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes),
            RequiredHeaders = new Dictionary<string, string>
            {
                ["Content-Type"] = contentType
            }
        };
    }

    // public string GetPublicUrl(string bucket, string objectPath)
    // {
    //     // لو شغالين HTTPS على السيرفر: استخدم https://
    //     // لو HTTP: استخدم http://
    //     string scheme = _settings.UseSSL ? "https" : "http";
        
    //     // ملاحظة: في MinIO لازم نستخدم /minio/ في المسار
    //     return $"{scheme}://{_settings.Endpoint}/minio/{bucket}/{objectPath}";
    // }
    public string GetPublicUrl(string bucket, string objectPath)
    {
        return $"{_settings.Endpoint}/{bucket}/{objectPath}";
    }

    public async Task<string> GenerateViewUrlAsync(
        string bucket,
        string objectKey,
        int expiryMinutes = 60)
    {
        var args = new PresignedGetObjectArgs()
            .WithBucket(bucket)
            .WithObject(objectKey)
            .WithExpiry(expiryMinutes * 60);

        return await _minioClient.PresignedGetObjectAsync(args);
    }

    public async Task<bool> ObjectExistsAsync(string bucket, string objectKey)
    {
        try
        {
            var args = new StatObjectArgs()
                .WithBucket(bucket)
                .WithObject(objectKey);

            await _minioClient.StatObjectAsync(args);
            return true;
        }
        catch (ObjectNotFoundException)
        {
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking object existence: {Bucket}/{Key}", bucket, objectKey);
            throw;
        }
    }

    public async Task<ObjectMetadata?> GetObjectMetadataAsync(string bucket, string objectKey)
    {
        try
        {
            var args = new StatObjectArgs()
                .WithBucket(bucket)
                .WithObject(objectKey);

            var stat = await _minioClient.StatObjectAsync(args);

            return new ObjectMetadata
            {
                SizeBytes = stat.Size,
                ContentType = stat.ContentType,
                LastModified = stat.LastModified,
                ETag = stat.ETag
            };
        }
        catch (ObjectNotFoundException)
        {
            return null;
        }
    }

    public async Task DeleteObjectAsync(string bucket, string objectKey)
    {
        var args = new RemoveObjectArgs()
            .WithBucket(bucket)
            .WithObject(objectKey);

        await _minioClient.RemoveObjectAsync(args);
        _logger.LogInformation("Deleted object: {Bucket}/{Key}", bucket, objectKey);
    }

    public async Task EnsureBucketExistsAsync(string bucket)
    {
        var existsArgs = new BucketExistsArgs().WithBucket(bucket);
        bool exists = await _minioClient.BucketExistsAsync(existsArgs);

        if (!exists)
        {
            var makeArgs = new MakeBucketArgs().WithBucket(bucket);
            await _minioClient.MakeBucketAsync(makeArgs);
            _logger.LogInformation("Created bucket: {Bucket}", bucket);
        }
    }

    public async Task CopyObjectAsync(
        string sourceBucket, 
        string sourceKey, 
        string destBucket, 
        string destKey)
    {
        await EnsureBucketExistsAsync(destBucket);

        var copySource = new CopySourceObjectArgs()
            .WithBucket(sourceBucket)
            .WithObject(sourceKey);

        var args = new CopyObjectArgs()
            .WithBucket(destBucket)
            .WithObject(destKey)
            .WithCopyObjectSource(copySource);

        await _minioClient.CopyObjectAsync(args);
        _logger.LogInformation("Copied {Source}/{SourceKey} to {Dest}/{DestKey}",
            sourceBucket, sourceKey, destBucket, destKey);
    }
}
