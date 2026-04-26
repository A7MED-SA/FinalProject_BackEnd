using backend_project.Services.Interfaces;

namespace backend_project.Services.Background;

/// <summary>
/// Background service that periodically cleans up expired edit requests.
/// Runs daily at midnight by default.
/// </summary>
public class EditRequestCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EditRequestCleanupService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(24);

    public EditRequestCleanupService(
        IServiceProvider serviceProvider,
        ILogger<EditRequestCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EditRequestCleanupService is starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Running edit request cleanup at {Time}", DateTime.UtcNow);

                using var scope = _serviceProvider.CreateScope();
                var approvalService = scope.ServiceProvider
                    .GetRequiredService<ICourseEditApprovalService>();

                var count = await approvalService.CleanupExpiredRequestsAsync();
                _logger.LogInformation("Cleaned up {Count} expired edit requests", count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during edit request cleanup");
            }

            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation("EditRequestCleanupService is stopping");
    }
}
