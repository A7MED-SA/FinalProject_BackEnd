using Athary.Application.DTOs.Communication;
using Athary.Application.Interfaces.Communication;
using Athary.Domain.Entities;
using Athary.Domain.Enums;
using Athary.Infrastructure.Services.Communication;
using FluentAssertions;
using MapsterMapper;

namespace Athary.Infrastructure.Tests.Communication;

public sealed class AnnouncementServiceTests : SqliteTestBase
{
    private readonly AnnouncementService _sut;
    private readonly User _admin;
    private readonly User _instructor;
    private readonly User _student;
    private readonly Course _course;

    public AnnouncementServiceTests()
    {
        _admin = new User { FirstName = "Admin", LastName = "A", Email = "a@a.com", UserName = "aa" };
        _instructor = new User { FirstName = "Inst", LastName = "R", Email = "i@r.com", UserName = "ir" };
        _student = new User { FirstName = "Stu", LastName = "D", Email = "s@d.com", UserName = "sd" };
        Context.Users.AddRange(_admin, _instructor, _student);

        var adminRole = new Role("Admin");
        var instructorRole = new Role("Instructor");
        var studentRole = new Role("Student");
        Context.Roles.AddRange(adminRole, instructorRole, studentRole);
        Context.SaveChanges();

        Context.UserRoles.AddRange(
            new UserRole { UserId = _admin.Id, RoleId = adminRole.Id },
            new UserRole { UserId = _instructor.Id, RoleId = instructorRole.Id },
            new UserRole { UserId = _student.Id, RoleId = studentRole.Id }
        );

        var cat = new Category { Name = "Cat", Slug = "cat" };
        Context.Categories.Add(cat);

        _course = new Course
        {
            Title = "Test Course", Slug = "test-course", Price = 50,
            CategoryId = cat.Id, CreatedBy = _instructor.Id,
            Status = CourseStatus.Published, IsPublished = true
        };
        Context.Courses.Add(_course);

        var enrollment = new Enrollment
        {
            UserId = _student.Id, CourseId = _course.Id,
            Status = EnrollmentStatus.InProgress
        };
        Context.Enrollments.Add(enrollment);

        Context.SaveChanges();
        _sut = new AnnouncementService(Context, Mapper);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateGlobalAnnouncement_WhenAdmin()
    {
        var result = await _sut.CreateAsync(_admin.Id, new CreateAnnouncementRequest
        {
            Title = "Global News",
            Content = "For everyone",
            Target = "All"
        });

        result.Title.Should().Be("Global News");
        result.Target.Should().Be("All");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateCourseAnnouncement_WhenInstructor()
    {
        var result = await _sut.CreateAsync(_instructor.Id, new CreateAnnouncementRequest
        {
            Title = "Course News",
            Content = "Update",
            Target = "SpecificCourse",
            CourseId = _course.Id
        });

        result.Title.Should().Be("Course News");
        result.Target.Should().Be("SpecificCourse");
        result.CourseId.Should().Be(_course.Id);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenStudentTriesToCreate()
    {
        await FluentActions.Invoking(() => _sut.CreateAsync(_student.Id, new CreateAnnouncementRequest
        {
            Title = "Hack",
            Content = "Bad",
            Target = "All"
        })).Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenInstructorCreatesGlobalAnnouncement()
    {
        await FluentActions.Invoking(() => _sut.CreateAsync(_instructor.Id, new CreateAnnouncementRequest
        {
            Title = "Global",
            Content = "Not allowed",
            Target = "All"
        })).Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GetFeedAsync_StudentShouldSeeRelevantAnnouncements()
    {
        await _sut.CreateAsync(_admin.Id, new CreateAnnouncementRequest
        {
            Title = "Global", Content = "All", Target = "All"
        });
        await _sut.CreateAsync(_instructor.Id, new CreateAnnouncementRequest
        {
            Title = "Course Specific", Content = "Course", Target = "SpecificCourse",
            CourseId = _course.Id
        });
        await _sut.CreateAsync(_admin.Id, new CreateAnnouncementRequest
        {
            Title = "Instructors Only", Content = "Instructors", Target = "Instructors"
        });

        var feed = await _sut.GetFeedAsync(_student.Id);

        feed.Items.Should().Contain(a => a.Title == "Global");
        feed.Items.Should().Contain(a => a.Title == "Course Specific");
        feed.Items.Should().NotContain(a => a.Title == "Instructors Only");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateOwnAnnouncement()
    {
        var created = await _sut.CreateAsync(_admin.Id, new CreateAnnouncementRequest
        {
            Title = "Original",
            Content = "Original content",
            Target = "All"
        });

        var updated = await _sut.UpdateAsync(created.Id, _admin.Id, new UpdateAnnouncementRequest
        {
            Title = "Updated",
            Content = "Updated content",
            Target = "Students",
            IsActive = true
        });

        updated.Title.Should().Be("Updated");
        updated.Content.Should().Be("Updated content");
        updated.Target.Should().Be("Students");
    }

    [Fact]
    public async Task DeactivateAsync_ShouldSetIsActiveFalse()
    {
        var created = await _sut.CreateAsync(_admin.Id, new CreateAnnouncementRequest
        {
            Title = "Temp", Content = "Temporary", Target = "All"
        });

        await _sut.DeactivateAsync(created.Id, _admin.Id);

        var feed = await _sut.GetFeedAsync(_admin.Id);
        feed.Items.Should().NotContain(a => a.Id == created.Id);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveAnnouncement()
    {
        var created = await _sut.CreateAsync(_admin.Id, new CreateAnnouncementRequest
        {
            Title = "Delete me", Content = "Bye", Target = "All"
        });

        await _sut.DeleteAsync(created.Id, _admin.Id);

        var db = await Context.Announcements.FindAsync(created.Id);
        db.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenNotOwner()
    {
        var created = await _sut.CreateAsync(_admin.Id, new CreateAnnouncementRequest
        {
            Title = "Not mine", Content = "Can't delete", Target = "All"
        });

        await FluentActions.Invoking(() => _sut.DeleteAsync(created.Id, _instructor.Id))
            .Should().ThrowAsync<InvalidOperationException>();
    }
}
