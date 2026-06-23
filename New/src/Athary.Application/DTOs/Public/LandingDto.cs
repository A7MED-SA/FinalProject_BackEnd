using Athary.Application.DTOs.Category;
using Athary.Application.DTOs.Courses;
using Athary.Application.DTOs.LiveSession;

namespace Athary.Application.DTOs.Public;

public sealed record LandingDto
{
    public LandingStatsDto Stats { get; init; } = new();
    public List<CategoryResponseDto> Categories { get; init; } = new();
    public List<PublicCourseDto> FeaturedCourses { get; init; } = new();
    public List<LiveSessionResponseDto> UpcomingLiveSessions { get; init; } = new();
    public List<TestimonialDto> Testimonials { get; init; } = new();
}

public sealed record LandingStatsDto
{
    public int TotalStudents { get; init; }
    public int TotalCourses { get; init; }
    public int TotalInstructors { get; init; }
    public double SatisfactionRate { get; init; }
    public long TotalVideoHours { get; init; }
    public int TotalCertificatesIssued { get; init; }
}
