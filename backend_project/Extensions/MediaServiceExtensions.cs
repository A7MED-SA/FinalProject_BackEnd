using backend_project.Configuration;
using backend_project.Services;
using backend_project.Services.Interfaces;
using backend_project.Workers;
using Minio;

namespace backend_project.Extensions;

public static class MediaServiceExtensions
{
    public static IServiceCollection AddMediaServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure MinIO settings
        services.Configure<MinioSettings>(configuration.GetSection("MinioSettings"));
        var minioSettings = configuration.GetSection("MinioSettings").Get<MinioSettings>()
            ?? throw new InvalidOperationException("MinioSettings not configured");

        // Register MinIO client
        services.AddSingleton<IMinioClient>(sp =>
        {
            var client = new MinioClient()
                .WithEndpoint(minioSettings.Endpoint)
                .WithCredentials(minioSettings.AccessKey, minioSettings.SecretKey);

            if (minioSettings.UseSSL)
                client.WithSSL();

            return client.Build();
        });

        // Register storage abstraction
        services.AddScoped<IObjectStorage, MinioObjectStorage>();

        // Register media services
        services.AddScoped<IMediaService, MediaService>();
        services.AddScoped<IAdminMediaService, AdminMediaService>();
        services.AddScoped<IVideoProcessingService, VideoProcessingService>();

        // Register background worker
        services.AddHostedService<VideoProcessingWorker>();

        return services;
    }
}
