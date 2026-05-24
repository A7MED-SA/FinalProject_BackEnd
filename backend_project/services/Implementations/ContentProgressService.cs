using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend_project.Data;
using backend_project.DTOs.ContentProgress;
using backend_project.Models;
using backend_project.Services.Interfaces;

namespace backend_project.Services.Implementations;

public class ContentProgressService : IContentProgressService
{
    private readonly ApplicationDbContext _context;
    private readonly ICertificateService _certificateService;

    public ContentProgressService(ApplicationDbContext context, ICertificateService certificateService)
    {
        _context = context;
        _certificateService = certificateService;
    }

    public async Task<ContentProgressDto> UpdateProgressAsync(Guid enrollmentId, UpdateProgressDto updateDto)
    {
        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.Id == enrollmentId);

        if (enrollment == null)
            throw new KeyNotFoundException("Enrollment not found.");

        enrollment.LastAccessedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new ContentProgressDto
        {
            EnrollmentId = enrollmentId,
            CompletionPercentage = updateDto.CompletionPercentage ?? 0,
            WatchTimeSeconds = updateDto.WatchTimeSeconds,
            IsCompleted = updateDto.MarkAsCompleted,
            LastAccessedAt = DateTime.UtcNow
        };
    }

    public async Task<ContentProgressDto> MarkCompletedAsync(Guid enrollmentId, Guid contentId, ContentType contentType)
    {
        var progress = await _context.ContentProgresses
            .FirstOrDefaultAsync(cp => cp.EnrollmentId == enrollmentId
                && cp.ContentId == contentId
                && cp.ContentType == contentType);

        if (progress == null)
        {
            progress = new ContentProgress
            {
                Id = Guid.NewGuid(),
                EnrollmentId = enrollmentId,
                ContentId = contentId,
                ContentType = contentType,
                IsCompleted = true,
                CompletionPercentage = 100,
                CompletedAt = DateTime.UtcNow,
                LastAccessedAt = DateTime.UtcNow
            };
            _context.ContentProgresses.Add(progress);
        }
        else
        {
            progress.IsCompleted = true;
            progress.CompletionPercentage = 100;
            progress.CompletedAt = DateTime.UtcNow;
            progress.LastAccessedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        await RecalculateEnrollmentProgressAsync(enrollmentId);

        return MapToDto(progress);
    }

    public async Task<IEnumerable<ContentProgressDto>> GetProgressForEnrollmentAsync(Guid enrollmentId)
    {
        return await _context.ContentProgresses
            .AsNoTracking()
            .Where(cp => cp.EnrollmentId == enrollmentId)
            .OrderByDescending(cp => cp.LastAccessedAt)
            .Select(cp => MapToDto(cp))
            .ToListAsync();
    }

    public async Task RecalculateEnrollmentProgressAsync(Guid enrollmentId)
    {
        var enrollment = await _context.Enrollments
            .Include(e => e.Course)
                .ThenInclude(c => c.Sections)
                    .ThenInclude(s => s.SectionItems)
            .FirstOrDefaultAsync(e => e.Id == enrollmentId);

        if (enrollment == null)
            return;

        var mandatoryItems = enrollment.Course.Sections
            .SelectMany(s => s.SectionItems)
            .Where(si => si.IsMandatory)
            .ToList();

        var totalMandatory = mandatoryItems.Count;

        if (totalMandatory == 0)
        {
            enrollment.ProgressPercentage = 0;
            await _context.SaveChangesAsync();
            return;
        }

        var completedMandatory = await _context.ContentProgresses
            .CountAsync(cp => cp.EnrollmentId == enrollmentId
                && cp.IsCompleted
                && mandatoryItems.Any(mi => mi.ItemId == cp.ContentId
                    && mi.ItemType.ToString() == cp.ContentType.ToString()));

        enrollment.ProgressPercentage = Math.Round((decimal)completedMandatory / totalMandatory * 100, 2);

        if (enrollment.ProgressPercentage >= 100)
        {
            enrollment.Status = EnrollmentStatus.Completed;
            enrollment.CompletedAt ??= DateTime.UtcNow;

            try
            {
                await _certificateService.GenerateCertificateAsync(enrollmentId);
            }
            catch
            {
                // Certificate generation failure should not block progress update
            }
        }

        await _context.SaveChangesAsync();
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
