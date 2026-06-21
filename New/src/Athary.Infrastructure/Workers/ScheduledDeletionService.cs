using Athary.Application.Interfaces.Notification;
using Athary.Domain.Entities;
using Athary.Domain.Enums;
using Athary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Athary.Infrastructure.Workers;

public class ScheduledDeletionService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ScheduledDeletionService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(1);

    public ScheduledDeletionService(IServiceProvider serviceProvider, ILogger<ScheduledDeletionService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ScheduledDeletionService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessScheduledDeletionsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing scheduled deletions");
            }

            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation("ScheduledDeletionService stopped");
    }

    private async Task ProcessScheduledDeletionsAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var now = DateTime.UtcNow;

        var dueCourses = await context.Courses
            .Where(c => c.ScheduledDeletionAt != null
                     && c.ScheduledDeletionAt <= now
                     && !c.IsReadOnlyForStudents
                     && c.DeletedAt == null)
            .ToListAsync(stoppingToken);

        if (dueCourses.Count == 0) return;

        _logger.LogInformation("Processing {Count} scheduled course deletions", dueCourses.Count);

        foreach (var course in dueCourses)
        {
            try
            {
                course.IsReadOnlyForStudents = true;

                _logger.LogInformation("Course {CourseId} '{Title}' is now read-only (scheduled deletion)", course.Id, course.Title);

                var enrolledStudents = await context.Enrollments
                    .Where(e => e.CourseId == course.Id
                             && (e.Status == EnrollmentStatus.InProgress || e.Status == EnrollmentStatus.Completed))
                    .Select(e => e.UserId)
                    .ToListAsync(stoppingToken);

                foreach (var studentId in enrolledStudents)
                {
                    await notificationService.CreateAndSendNotificationAsync(
                        studentId,
                        "Course is now Read-Only",
                        $"The course \"{course.Title}\" has been set to read-only mode. You can still view content, but cannot submit quizzes, update progress, join live sessions, or add comments.",
                        NotificationType.Course,
                        $"/courses/{course.Id}",
                        cancellationToken: stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process scheduled deletion for course {CourseId}", course.Id);
            }
        }

        await context.SaveChangesAsync(stoppingToken);
    }
}
