using System;
using System.Collections.Generic;
using backend_project.Models;

namespace backend_project.DTOs.Course;

public class CourseSummaryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public CourseLevel Level { get; set; }
    public CourseLanguage Language { get; set; }
    public CourseStatus Status { get; set; }
    public int TotalDurationMinutes { get; set; }
    public int EnrollmentCount { get; set; }
    public decimal AverageRating { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CourseDetailsDto : CourseSummaryDto
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public Guid CreatedBy { get; set; }
    public string CreatorName { get; set; } = string.Empty;
    public List<CourseRequirementDto> Requirements { get; set; } = new();
    public List<CourseLearningOutcomeDto> LearningOutcomes { get; set; } = new();
    // Sections will be added later or fetched separately depending on UI needs
}

public class CreateCourseDto
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid CategoryId { get; set; }
    public CourseLevel Level { get; set; } = CourseLevel.Beginner;
    public CourseLanguage Language { get; set; } = CourseLanguage.Ar;
    public decimal Price { get; set; }
}

public class UpdateCourseDto
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid CategoryId { get; set; }
    public CourseLevel Level { get; set; }
    public CourseLanguage Language { get; set; }
    public decimal Price { get; set; }
}

public class CourseRequirementDto
{
    public Guid Id { get; set; }
    public string RequirementText { get; set; } = string.Empty;
}

public class CourseLearningOutcomeDto
{
    public Guid Id { get; set; }
    public string OutcomeText { get; set; } = string.Empty;
}

public class AddRequirementDto
{
    public string RequirementText { get; set; } = string.Empty;
}

public class AddLearningOutcomeDto
{
    public string OutcomeText { get; set; } = string.Empty;
}
