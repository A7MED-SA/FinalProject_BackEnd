using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using MassTransit;

namespace backend_project.Models;

[Table("users")]
public class User : IdentityUser<Guid>
{
    public User()
    {
        // Generate Sequential GUID for new users
        Id = NewId.NextSequentialGuid();
        SecurityStamp = Guid.NewGuid().ToString();

        // ÊÚííä ÇáÍÇáÉ ÇáÇÝÊÑÇÖíÉ Åáì ÛíÑ ãÝÚá
        IsActive = false;
    }

    [Column("name")]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [Column("profile_picture_url")]
    [MaxLength(500)]
    public string? ProfilePictureUrl { get; set; }

    [Column("bio")]
    public string? Bio { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = false;

    [Column("date_of_birth")]
    public DateOnly? DateOfBirth { get; set; }

    [Column("gender")]
    [MaxLength(50)]
    public Gender? Gender { get; set; }

    [Column("nationality")]
    [MaxLength(100)]
    public string? Nationality { get; set; }

    // Hide PhoneNumber from Identity - we use UserPhone table instead
    [NotMapped]
    public override string? PhoneNumber { get => base.PhoneNumber; set => base.PhoneNumber = value; }

    [NotMapped]
    public override bool PhoneNumberConfirmed { get => base.PhoneNumberConfirmed; set => base.PhoneNumberConfirmed = value; }

    [Column("last_login")]
    public DateTime? LastLogin { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    // Navigation Properties
    public virtual ICollection<UserPhone> UserPhones { get; set; } = new List<UserPhone>();
    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
    public virtual ICollection<TeacherRequest> SubmittedTeacherRequests { get; set; } = new List<TeacherRequest>();
    public virtual ICollection<TeacherRequest> ProcessedTeacherRequests { get; set; } = new List<TeacherRequest>();
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<Course> CreatedCourses { get; set; } = new List<Course>();
    public virtual ICollection<Course> ApprovedCourses { get; set; } = new List<Course>();
}

public enum Gender
{
    Male,
    Female,
    Other,
    PreferNotToSay
}