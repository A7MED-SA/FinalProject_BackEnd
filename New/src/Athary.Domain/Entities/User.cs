using Athary.Domain.Enums;
using MassTransit;
using Microsoft.AspNetCore.Identity;

namespace Athary.Domain.Entities;

public class User : IdentityUser<Guid>
{
    public User()
    {
        Id = NewId.NextSequentialGuid();
        SecurityStamp = Guid.NewGuid().ToString();
        IsActive = false;
    }

    public string FullName => $"{FirstName} {LastName}".Trim();

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public Guid? ProfileImageFileId { get; set; }

    public UploadedFile? ProfileImageFile { get; set; }

    public List<UploadedFile> UploadedFiles { get; set; }
        = new List<UploadedFile>();

    public string? Bio { get; set; }

    public bool IsActive { get; set; } = false;

    public DateOnly? DateOfBirth { get; set; }

    public Gender? Gender { get; set; }

    public string? Nationality { get; set; }

    public override string? PhoneNumber { get => base.PhoneNumber; set => base.PhoneNumber = value; }

    public override bool PhoneNumberConfirmed { get => base.PhoneNumberConfirmed; set => base.PhoneNumberConfirmed = value; }

    public DateTime? LastLogin { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public decimal RevenueSharePercentage { get; set; } = 50.00m;

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public List<UserPhone> UserPhones { get; set; } = new List<UserPhone>();

    public List<Session> Sessions { get; set; } = new List<Session>();

    public List<Address> Addresses { get; set; } = new List<Address>();

    public List<InstructorRequest> SubmittedInstructorRequests { get; set; } = new List<InstructorRequest>();

    public List<InstructorRequest> ProcessedInstructorRequests { get; set; } = new List<InstructorRequest>();

    public List<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public List<Course> CreatedCourses { get; set; } = new List<Course>();

    public List<Course> ApprovedCourses { get; set; } = new List<Course>();
}
