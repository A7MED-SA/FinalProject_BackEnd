using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using backend_project.Data;
using backend_project.DTOs.Dashboard;
using backend_project.Models;
using backend_project.Services.Implementations;
using CommerceTests.Helpers;

namespace CommerceTests;

public class StudentDashboardServiceTests
{
    private static StudentDashboardService CreateService(ApplicationDbContext context)
    {
        var logger = Mock.Of<ILogger<StudentDashboardService>>();
        return new StudentDashboardService(context, logger);
    }

    [Fact]
    public async Task GetDashboardAsync_WithEnrollments_ReturnsCorrectCounts()
    {
        var context = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        var student = TestDbContextFactory.CreateUser(userId);
        context.Users.Add(student);

        var creator = TestDbContextFactory.CreateUser();
        context.Users.Add(creator);

        var course1 = TestDbContextFactory.CreateCourse(creatorId: creator.Id);
        var course2 = TestDbContextFactory.CreateCourse(creatorId: creator.Id);
        var course3 = TestDbContextFactory.CreateCourse(creatorId: creator.Id);
        context.Courses.AddRange(course1, course2, course3);

        context.Enrollments.AddRange(
            new Enrollment { UserId = userId, CourseId = course1.Id, Status = EnrollmentStatus.Completed, ProgressPercentage = 100 },
            new Enrollment { UserId = userId, CourseId = course2.Id, Status = EnrollmentStatus.InProgress, ProgressPercentage = 50 },
            new Enrollment { UserId = userId, CourseId = course3.Id, Status = EnrollmentStatus.InProgress, ProgressPercentage = 10 }
        );
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var result = await service.GetDashboardAsync(userId);

        result.TotalEnrolledCourses.Should().Be(3);
        result.InProgressCourses.Should().Be(2);
        result.CompletedCourses.Should().Be(1);
        result.CertificatesEarned.Should().Be(0);
    }

    [Fact]
    public async Task GetDashboardAsync_NoEnrollments_ReturnsZeroValues()
    {
        var context = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();

        var service = CreateService(context);
        var result = await service.GetDashboardAsync(userId);

        result.TotalEnrolledCourses.Should().Be(0);
        result.InProgressCourses.Should().Be(0);
        result.CompletedCourses.Should().Be(0);
        result.TotalLearningHours.Should().Be(0);
        result.CertificatesEarned.Should().Be(0);
        result.RecentEnrollments.Should().BeEmpty();
        result.CertificateEligibleCourses.Should().BeEmpty();
    }

    [Fact]
    public async Task GetDashboardAsync_RefundedEnrollments_ExcludedFromCounts()
    {
        var context = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        var student = TestDbContextFactory.CreateUser(userId);
        context.Users.Add(student);

        var creator = TestDbContextFactory.CreateUser();
        context.Users.Add(creator);

        var course1 = TestDbContextFactory.CreateCourse(creatorId: creator.Id);
        var course2 = TestDbContextFactory.CreateCourse(creatorId: creator.Id);
        context.Courses.AddRange(course1, course2);

        context.Enrollments.AddRange(
            new Enrollment { UserId = userId, CourseId = course1.Id, Status = EnrollmentStatus.InProgress },
            new Enrollment { UserId = userId, CourseId = course2.Id, Status = EnrollmentStatus.Refunded }
        );
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var result = await service.GetDashboardAsync(userId);

        result.TotalEnrolledCourses.Should().Be(1);
        result.InProgressCourses.Should().Be(1);
    }

    [Fact]
    public async Task GetDashboardAsync_WithCertificate_CountsCertificate()
    {
        var context = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        var student = TestDbContextFactory.CreateUser(userId);
        context.Users.Add(student);

        var creator = TestDbContextFactory.CreateUser();
        context.Users.Add(creator);

        var course = TestDbContextFactory.CreateCourse(creatorId: creator.Id);
        context.Courses.Add(course);

        context.Enrollments.Add(new Enrollment
        {
            UserId = userId,
            CourseId = course.Id,
            Status = EnrollmentStatus.Completed,
            ProgressPercentage = 100,
            CertificateId = Guid.NewGuid()
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var result = await service.GetDashboardAsync(userId);

        result.CertificatesEarned.Should().Be(1);
        result.CompletedCourses.Should().Be(1);
    }

    [Fact]
    public async Task GetDashboardAsync_CompletedCourseWithoutCertificate_ShowsAsEligible()
    {
        var context = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        var student = TestDbContextFactory.CreateUser(userId);
        context.Users.Add(student);

        var creator = TestDbContextFactory.CreateUser();
        context.Users.Add(creator);

        var course = TestDbContextFactory.CreateCourse(creatorId: creator.Id);
        context.Courses.Add(course);

        context.Enrollments.Add(new Enrollment
        {
            UserId = userId,
            CourseId = course.Id,
            Status = EnrollmentStatus.Completed,
            ProgressPercentage = 100
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var result = await service.GetDashboardAsync(userId);

        result.CertificateEligibleCourses.Should().HaveCount(1);
        result.CertificateEligibleCourses[0].CourseId.Should().Be(course.Id);
    }

    [Fact]
    public async Task GetDashboardAsync_ComputesLearningHoursFromVideoProgress()
    {
        var context = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        var student = TestDbContextFactory.CreateUser(userId);
        context.Users.Add(student);

        var creator = TestDbContextFactory.CreateUser();
        context.Users.Add(creator);

        var course = TestDbContextFactory.CreateCourse(creatorId: creator.Id);
        context.Courses.Add(course);

        var enrollment = new Enrollment
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CourseId = course.Id,
            Status = EnrollmentStatus.InProgress
        };
        context.Enrollments.Add(enrollment);
        context.ContentProgresses.Add(new ContentProgress
        {
            EnrollmentId = enrollment.Id,
            ContentType = ContentType.Video,
            ContentId = Guid.NewGuid(),
            WatchTimeSeconds = 7200
        });
        context.ContentProgresses.Add(new ContentProgress
        {
            EnrollmentId = enrollment.Id,
            ContentType = ContentType.Quiz,
            ContentId = Guid.NewGuid(),
            IsCompleted = true
        });
        context.ContentProgresses.Add(new ContentProgress
        {
            EnrollmentId = enrollment.Id,
            ContentType = ContentType.Document,
            ContentId = Guid.NewGuid(),
            IsCompleted = true
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var result = await service.GetDashboardAsync(userId);

        // 7200 seconds = 2 hours video + 0.5 quiz + 0.25 doc = 2.75
        result.TotalLearningHours.Should().BeApproximately(2.75m, 0.01m);
    }

    [Fact]
    public async Task GetDashboardAsync_RecentEnrollmentsOrderedByLastAccess()
    {
        var context = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        var student = TestDbContextFactory.CreateUser(userId);
        context.Users.Add(student);

        var creator = TestDbContextFactory.CreateUser();
        context.Users.Add(creator);

        var course1 = TestDbContextFactory.CreateCourse(creatorId: creator.Id);
        var course2 = TestDbContextFactory.CreateCourse(creatorId: creator.Id);
        context.Courses.AddRange(course1, course2);

        context.Enrollments.AddRange(
            new Enrollment { UserId = userId, CourseId = course1.Id, Status = EnrollmentStatus.InProgress, LastAccessedAt = DateTime.UtcNow.AddDays(-1) },
            new Enrollment { UserId = userId, CourseId = course2.Id, Status = EnrollmentStatus.InProgress, LastAccessedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var result = await service.GetDashboardAsync(userId);

        result.RecentEnrollments.Should().HaveCount(2);
        result.RecentEnrollments[0].CourseId.Should().Be(course2.Id);
        result.RecentEnrollments[1].CourseId.Should().Be(course1.Id);
    }
}
