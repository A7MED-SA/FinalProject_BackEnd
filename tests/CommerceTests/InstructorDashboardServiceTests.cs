using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using backend_project.Data;
using backend_project.Models;
using backend_project.Services.Implementations;
using CommerceTests.Helpers;

namespace CommerceTests;

public class InstructorDashboardServiceTests
{
    private static InstructorDashboardService CreateService(ApplicationDbContext context)
    {
        var logger = Mock.Of<ILogger<InstructorDashboardService>>();
        return new InstructorDashboardService(context, logger);
    }

    [Fact]
    public async Task GetDashboardAsync_WithCourses_ReturnsCorrectCounts()
    {
        var context = TestDbContextFactory.Create();
        var instructorId = Guid.NewGuid();
        var instructor = TestDbContextFactory.CreateUser(instructorId);
        context.Users.Add(instructor);

        var student1 = TestDbContextFactory.CreateUser();
        var student2 = TestDbContextFactory.CreateUser();
        context.Users.AddRange(student1, student2);

        var course1 = TestDbContextFactory.CreateCourse(creatorId: instructorId);
        var course2 = TestDbContextFactory.CreateCourse(creatorId: instructorId);
        var course3 = TestDbContextFactory.CreateCourse(creatorId: instructorId);
        context.Courses.AddRange(course1, course2, course3);

        context.Enrollments.AddRange(
            new Enrollment { UserId = student1.Id, CourseId = course1.Id, Status = EnrollmentStatus.InProgress },
            new Enrollment { UserId = student2.Id, CourseId = course1.Id, Status = EnrollmentStatus.InProgress },
            new Enrollment { UserId = student1.Id, CourseId = course2.Id, Status = EnrollmentStatus.Completed }
        );
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var result = await service.GetDashboardAsync(instructorId);

        result.PublishedCourses.Should().Be(0); // courses are not published
        result.TotalStudents.Should().Be(2);
        result.Courses.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetDashboardAsync_NoCourses_ReturnsZeroValues()
    {
        var context = TestDbContextFactory.Create();
        var instructorId = Guid.NewGuid();

        var service = CreateService(context);
        var result = await service.GetDashboardAsync(instructorId);

        result.PublishedCourses.Should().Be(0);
        result.TotalStudents.Should().Be(0);
        result.TotalTeachingHours.Should().Be(0);
        result.AverageRating.Should().Be(0);
        result.TotalRevenue.Should().Be(0);
        result.Courses.Should().BeEmpty();
        result.PendingEditRequests.Should().BeEmpty();
    }

    [Fact]
    public async Task GetDashboardAsync_RefundedEnrollments_ExcludedFromStudentCount()
    {
        var context = TestDbContextFactory.Create();
        var instructorId = Guid.NewGuid();
        var instructor = TestDbContextFactory.CreateUser(instructorId);
        context.Users.Add(instructor);

        var student = TestDbContextFactory.CreateUser();
        context.Users.Add(student);

        var course = TestDbContextFactory.CreateCourse(creatorId: instructorId);
        context.Courses.Add(course);

        context.Enrollments.AddRange(
            new Enrollment { UserId = student.Id, CourseId = course.Id, Status = EnrollmentStatus.InProgress },
            new Enrollment { UserId = student.Id, CourseId = course.Id, Status = EnrollmentStatus.Refunded }
        );
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var result = await service.GetDashboardAsync(instructorId);

        // Only non-refunded enrollments count
        result.TotalStudents.Should().Be(1);
    }

    [Fact]
    public async Task GetCoursesAsync_ReturnsPaginatedResults()
    {
        var context = TestDbContextFactory.Create();
        var instructorId = Guid.NewGuid();
        var instructor = TestDbContextFactory.CreateUser(instructorId);
        context.Users.Add(instructor);

        for (int i = 0; i < 5; i++)
        {
            context.Courses.Add(TestDbContextFactory.CreateCourse(creatorId: instructorId));
        }
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var result = await service.GetCoursesAsync(instructorId, 1, 2);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(5);
        result.TotalPages.Should().Be(3);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(2);
    }
}
