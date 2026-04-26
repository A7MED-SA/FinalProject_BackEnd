using System;
using System.Collections.Generic;
using backend_project.Models;
using System.ComponentModel.DataAnnotations;

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
    
    // 👇 أضف الخصائص دي
    public string? ThumbnailUrl { get; set; }
    public string? CategoryName { get; set; }
}
public class CourseFilterDto
{
    // 🔍 بحث نصي
    public string? SearchTerm { get; set; }
    
    // 📂 تصفية حسب الفئة
    public Guid? CategoryId { get; set; }
    
    // 💰 نطاق السعر
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    
    // 📊 الترتيب
    /// <summary>
    /// خيارات الترتيب: "price_asc", "price_desc", "rating", "newest"
    /// </summary>
    public string? SortBy { get; set; }
    
    // 📄 Pagination (اختياري)
    [Range(1, 100)]
    public int PageSize { get; set; } = 20;
    
    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;
}

// في: DTOs/Course/CourseDetailsDto.cs

public class CourseDetailsDto : CourseSummaryDto
{
    public Guid CategoryId { get; set; }
    // public string CategoryName { get; set; } = string.Empty; // ← موجود بالفعل في الـ base class
    
    public Guid CreatedBy { get; set; }
    public string CreatorName { get; set; } = string.Empty;
    public List<CourseRequirementDto> Requirements { get; set; } = new();
    public List<CourseLearningOutcomeDto> LearningOutcomes { get; set; } = new();
    
    // 👇 أضف الخصائص دي
    public string? ImageUrl { get; set; }
    public Guid? CourseImageFileId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
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

// DTOs/Course/CourseDtos.cs
public class UpdateCourseDto
{
    public string? Title { get; set; }
    public string? Slug { get; set; }
    public string? Description { get; set; }
    
    public Guid? CategoryId { get; set; }          
    public CourseLevel? Level { get; set; }        
    public CourseLanguage? Language { get; set; }  
    public decimal? Price { get; set; }            
}
public class SetCourseImageRequest
{
    [Required(ErrorMessage = "File ID is required")]
    public Guid FileId { get; set; }
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
