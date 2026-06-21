using Athary.Application.DTOs.Courses;
using Athary.Application.Interfaces.Courses;
using Athary.Domain.Entities;
using Athary.Domain.Enums;
using Athary.Domain.Interfaces;
using Athary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Athary.Infrastructure.Services.Courses;

public sealed class ContentProgressService : IContentProgressService
{
    private readonly ApplicationDbContext _context;
    private readonly IRepository<ContentProgress> _progressRepo;
    private readonly IRepository<Enrollment> _enrollmentRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ContentProgressService> _logger;

    public ContentProgressService(
        ApplicationDbContext context,
        IRepository<ContentProgress> progressRepo,
        IRepository<Enrollment> enrollmentRepo,
        IUnitOfWork unitOfWork,
        ILogger<ContentProgressService> logger)
    {
        _context = context;
        _progressRepo = progressRepo;
        _enrollmentRepo = enrollmentRepo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ContentProgressDto> UpdateProgressAsync(Guid enrollmentId, UpdateProgressDto updateDto, CancellationToken cancellationToken = default)
    {
        var enrollment = await _enrollmentRepo.FirstOrDefaultAsync(
            e => e.Id == enrollmentId,
            cancellationToken: cancellationToken)
            ?? throw new KeyNotFoundException("Enrollment not found.");

        enrollment.LastAccessedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ContentProgressDto
        {
            EnrollmentId = enrollmentId,
            CompletionPercentage = updateDto.CompletionPercentage ?? 0,
            WatchTimeSeconds = updateDto.WatchTimeSeconds,
            IsCompleted = updateDto.MarkAsCompleted,
            LastAccessedAt = DateTime.UtcNow
        };
    }

    public async Task<ContentProgressDto> MarkCompletedAsync(Guid enrollmentId, Guid contentId, ContentType contentType, CancellationToken cancellationToken = default)
    {
        var progress = await _progressRepo.FirstOrDefaultAsync(
            cp => cp.EnrollmentId == enrollmentId && cp.ContentId == contentId && cp.ContentType == contentType,
            cancellationToken: cancellationToken);

        if (progress is null)
        {
            progress = new ContentProgress
            {
                EnrollmentId = enrollmentId,
                ContentId = contentId,
                ContentType = contentType,
                IsCompleted = true,
                CompletionPercentage = 100,
                CompletedAt = DateTime.UtcNow,
                LastAccessedAt = DateTime.UtcNow
            };

            await _progressRepo.AddAsync(progress, cancellationToken);
        }
        else
        {
            progress.IsCompleted = true;
            progress.CompletionPercentage = 100;
            progress.CompletedAt = DateTime.UtcNow;
            progress.LastAccessedAt = DateTime.UtcNow;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await RecalculateEnrollmentProgressAsync(enrollmentId, cancellationToken);

        return MapToDto(progress);
    }

    public async Task<IEnumerable<ContentProgressDto>> GetProgressForEnrollmentAsync(Guid enrollmentId, CancellationToken cancellationToken = default)
    {
        return await _context.ContentProgresses
            .AsNoTracking()
            .Where(cp => cp.EnrollmentId == enrollmentId)
            .OrderByDescending(cp => cp.LastAccessedAt)
            .Select(cp => MapToDto(cp))
            .ToListAsync(cancellationToken);
    }

    public async Task RecalculateEnrollmentProgressAsync(Guid enrollmentId, CancellationToken cancellationToken = default)
    {
        var enrollment = await _context.Enrollments
            .Include(e => e.Course)
                .ThenInclude(c => c.Sections)
                    .ThenInclude(s => s.SectionItems)
            .FirstOrDefaultAsync(e => e.Id == enrollmentId, cancellationToken);

        if (enrollment is null)
            return;

        var mandatoryItems = enrollment.Course.Sections
            .SelectMany(s => s.SectionItems)
            .Where(si => si.IsMandatory)
            .ToList();

        var totalMandatory = mandatoryItems.Count;

        if (totalMandatory == 0)
        {
            enrollment.ProgressPercentage = 0;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        var mandatoryItemIds = mandatoryItems.Select(mi => mi.ItemId).ToHashSet();
        var completedMandatory = await _context.ContentProgresses
            .CountAsync(cp => cp.EnrollmentId == enrollmentId
                && cp.IsCompleted
                && mandatoryItemIds.Contains(cp.ContentId), cancellationToken);

        enrollment.ProgressPercentage = Math.Round((decimal)completedMandatory / totalMandatory * 100, 2);

        if (enrollment.ProgressPercentage >= 100)
        {
            enrollment.Status = EnrollmentStatus.Completed;
            enrollment.CompletedAt ??= DateTime.UtcNow;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static ContentProgressDto MapToDto(ContentProgress cp)
    {
        return new ContentProgressDto
        {
            Id = cp.Id,
            EnrollmentId = cp.EnrollmentId,
            ContentType = cp.ContentType,
            ContentId = cp.ContentId,
            IsCompleted = cp.IsCompleted,
            WatchTimeSeconds = cp.WatchTimeSeconds,
            AttemptsCount = cp.AttemptsCount,
            CompletionPercentage = cp.CompletionPercentage,
            Metadata = cp.Metadata,
            LastAccessedAt = cp.LastAccessedAt,
            CompletedAt = cp.CompletedAt
        };
    }
}
