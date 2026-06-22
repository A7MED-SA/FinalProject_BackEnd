# 📋 دليل التعديلات الشامل — منصة آثاري
## Comprehensive Modification Guide — Athary Platform

**تاريخ الإعداد:** 23 يونيو 2026  
**الغرض:** توثيق كل التعديلات المطلوبة لربط Frontend بالـ Backend  
**الحالة:** خطة تنفيذية كاملة

---

## 📑 فهرس المحتويات

1. [نظرة عامة](#1-نظرة-عامة)
2. [ملخص الفجوات الحرجة](#2-ملخص-الفجوات-الحرجة)
3. [تعديلات Backend](#3-تعديلات-backend)
4. [تعديلات Frontend](#4-تعديلات-frontend)
5. [خطة التنفيذ](#5-خطة-التنفيذ)
6. [Time Estimates واقعية](#6-time-estimates-واقعية)
7. [Checklist نهائي](#7-checklist-نهائي)

---

## 1. نظرة عامة

### 📊 الحالة الحالية

| الجانب | الحالة | النسبة |
|---|---|---|
| **Backend** | ✅ شبه جاهز | 95% |
| **Frontend** | ⚠️ Prototype (Mocked) | 40% |
| **الربط بينهما** | ❌ غير موجود | 0% |

### 🎯 الهدف النهائي

- ✅ ربط كل صفحات Frontend بالـ Backend الحقيقي
- ✅ إضافة الـ endpoints الناقصة في Backend
- ✅ توحيد الـ Types مع الـ DTOs
- ✅ تفعيل OAuth + Refresh Token + SignalR
- ✅ تطبيق Presigned URL flow للـ uploads

---

## 2. ملخص الفجوات الحرجة

### 🔴 الفجوة #1: Endpoints مختلفة تماماً

| Frontend يستخدم | Backend فعلي | الحالة |
|---|---|---|
| `GET /api/courses` | `GET /api/public/courses` | ❌ |
| `GET /api/courses/:id` | `GET /api/public/courses/:id` | ❌ |
| `POST /api/auth/login` → `{token, user}` | `POST /api/auth/login` → `{accessToken, refreshToken, sessionId, expiresAt, user}` | ❌ |
| `POST /api/auth/resend-otp` | `POST /api/auth/resend-verification` | ❌ |
| `POST /api/auth/verify-email` (OTP) | `POST /api/auth/verify-email` (Token) | ❌ |
| `GET /api/auth/me` | `GET /api/profile/me` | ❌ |
| `POST /api/cart/add` | `POST /api/cart/items` | ❌ |
| `POST /api/checkout` | `POST /api/orders` + `POST /api/payments/process` | ❌ |
| `GET /api/student/courses` | `GET /api/enrollments` | ❌ |
| `GET /api/student/favorites` | `GET /api/wishlist` | ❌ |
| `GET /api/instructor/courses` | `GET /api/management/courses` | ❌ |
| `GET /api/admin/teacher-requests` | `GET /api/instructor-requests/pending` | ❌ |
| `POST /api/media/upload` (FormData) | **Presigned URL Flow** (3 خطوات) | ❌ |
| `GET /api/quiz/:quizId` | `GET /api/enrollments/:id/quizzes/:id/attempts` | ❌ |
| `GET /api/instructors/:name` | ❌ غير موجود | ❌ |

### 🔴 الفجوة #2: Types غير مطابقة

| Frontend Type | Backend DTO | الفرق |
|---|---|---|
| `Course.category: string` | `PublicCourseDto.categoryName: string` | ❌ اسم مختلف |
| `Course.rating: number` | `PublicCourseDto.averageRating: decimal` | ❌ |
| `Course.duration: string` | `PublicCourseDto.totalDurationMinutes: int` | ❌ |
| `Course.lessonsCount: number` | `PublicCourseDto.lessonCount: int` | ❌ |
| `Course.thumbnail: string` | `PublicCourseDto.courseImageUrl: string` | ❌ |

### 🔴 الفجوة #3: Services ناقصة

- ❌ `payment.service.ts`
- ❌ `coupon.service.ts`
- ❌ `refund.service.ts`
- ❌ `review.service.ts`
- ❌ `liveSession.service.ts`
- ❌ `announcement.service.ts`
- ❌ `instructorRequest.service.ts`
- ❌ `admin.service.ts`

### 🔴 الفجوة #4: OAuth Flow غلط

```ts
// ❌ Frontend الحالي
loginWithGoogle: () => {
  window.location.href = `${import.meta.env.VITE_API_URL}/oauth/google`;
}

// ✅ Backend الفعلي
POST /api/oauth/google
Body: { idToken: string, provider: "google" }
```

### 🔴 الفجوة #5: SignalR غير مفعّل

- ❌ مفيش SignalR service
- ❌ مفيش hook
- ❌ مفيش integration مع notificationStore
- ❌ مفيش reconnection logic

---

## 3. تعديلات Backend

### 3.1 🔴 Instructor Slug — إضافة Slug للمستخدم

**الوصف:** السماح بالوصول للملف الشخصي عبر `/api/public/instructors/{slug}` بدلاً من GUID فقط.

#### 3.1.1 Domain Layer

**الملف:** `Domain/Entities/User.cs`

```csharp
// ✅ المطلوب إضافته
[StringLength(100, MinimumLength = 3)]
[RegularExpression(@"^[a-z0-9-]+$", 
    ErrorMessage = "Slug must be lowercase letters, numbers, and hyphens only")]
public string? Slug { get; set; }
```

#### 3.1.2 Application Layer

**الملف:** `Application/DTOs/Profile/PublicProfileDto.cs`

```csharp
public class PublicProfileDto
{
    public Guid Id { get; set; }
    public string? FullName { get; set; }
    public string? Slug { get; set; }  // ✅ إضافة
    public string? Bio { get; set; }
    public string? Nationality { get; set; }
    public string? ProfileImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

**الملف:** `Application/Interfaces/IProfileService.cs`

```csharp
public interface IProfileService
{
    Task<PublicProfileDto?> GetBySlugAsync(string slug);
    Task<bool> IsSlugAvailableAsync(string slug);
    Task<string> GenerateSlugAsync(string fullName);
    Task<List<PublicProfileDto>> SearchBySlugAsync(string query);
}
```

**الملف:** `Application/Services/ProfileService.cs`

```csharp
public async Task<PublicProfileDto?> GetBySlugAsync(string slug)
{
    var user = await _context.Users
        .Where(u => u.Slug == slug && u.Role == UserRole.Instructor && u.DeletedAt == null)
        .Select(u => new PublicProfileDto
        {
            Id = u.Id,
            FullName = $"{u.FirstName} {u.LastName}",
            Slug = u.Slug,
            Bio = u.Bio,
            Nationality = u.Nationality,
            ProfileImageUrl = u.ProfileImageUrl,
            CreatedAt = u.CreatedAt
        })
        .FirstOrDefaultAsync();

    return user;
}

public async Task<bool> IsSlugAvailableAsync(string slug)
{
    return !await _context.Users.AnyAsync(u => u.Slug == slug);
}

public async Task<string> GenerateSlugAsync(string fullName)
{
    var baseSlug = fullName.ToLower()
        .Replace(" ", "-")
        .Replace(".", "")
        .Replace("'", "");
    
    baseSlug = Regex.Replace(baseSlug, @"[^a-z0-9-]", "");
    
    var slug = baseSlug;
    var counter = 1;
    
    while (!await IsSlugAvailableAsync(slug))
    {
        slug = $"{baseSlug}-{counter}";
        counter++;
    }
    
    return slug;
}

public async Task<List<PublicProfileDto>> SearchBySlugAsync(string query)
{
    return await _context.Users
        .Where(u => u.Role == UserRole.Instructor && 
                    u.DeletedAt == null &&
                    (u.Slug.Contains(query) || 
                     ($"{u.FirstName} {u.LastName}").Contains(query)))
        .Select(u => new PublicProfileDto
        {
            Id = u.Id,
            FullName = $"{u.FirstName} {u.LastName}",
            Slug = u.Slug,
            Bio = u.Bio,
            ProfileImageUrl = u.ProfileImageUrl
        })
        .Take(10)
        .ToListAsync();
}
```

#### 3.1.3 API Layer

**الملف:** `Controllers/PublicInstructorController.cs` (جديد)

```csharp
[ApiController]
[Route("api/public/instructors")]
public class PublicInstructorController : ControllerBase
{
    private readonly IProfileService _profileService;

    public PublicInstructorController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet("{slug}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var instructor = await _profileService.GetBySlugAsync(slug);
        
        if (instructor == null)
            return NotFound(ApiResponse<PublicProfileDto>.Error("Instructor not found"));
        
        return Ok(ApiResponse<PublicProfileDto>.Success(instructor));
    }

    [HttpGet("{slug}/courses")]
    [AllowAnonymous]
    public async Task<IActionResult> GetInstructorCourses(string slug)
    {
        var instructor = await _profileService.GetBySlugAsync(slug);
        
        if (instructor == null)
            return NotFound();
        
        var courses = await _courseRepository.GetByInstructorIdAsync(instructor.Id);
        return Ok(ApiResponse<List<PublicCourseDto>>.Success(courses));
    }

    [HttpGet("check-slug")]
    [Authorize]
    public async Task<IActionResult> CheckSlugAvailability([FromQuery] string slug)
    {
        var isAvailable = await _profileService.IsSlugAvailableAsync(slug);
        return Ok(ApiResponse<bool>.Success(isAvailable));
    }

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return Ok(ApiResponse<List<PublicProfileDto>>.Success(new List<PublicProfileDto>()));
        
        var instructors = await _profileService.SearchBySlugAsync(q);
        return Ok(ApiResponse<List<PublicProfileDto>>.Success(instructors));
    }
}
```

#### 3.1.4 Infrastructure Layer

```bash
dotnet ef migrations add AddUserSlug
dotnet ef database update
```

**الملف:** `Infrastructure/Data/AppDbContext.cs`

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<User>(entity =>
    {
        entity.Property(e => e.Slug)
            .HasMaxLength(100)
            .IsRequired(false);
        
        entity.HasIndex(e => e.Slug)
            .IsUnique()
            .HasFilter("[Slug] IS NOT NULL");
    });
}
```

---

### 3.2 🔴 Landing Aggregated Endpoint

**الوصف:** Endpoint واحد يعيد كل بيانات الصفحة الرئيسية لتقليل عدد الـ Requests.

#### 3.2.1 Application Layer

**الملف:** `Application/DTOs/Public/LandingDto.cs` (جديد)

```csharp
public class LandingDto
{
    public LandingStatsDto Stats { get; set; }
    public List<CategoryResponseDto> Categories { get; set; }
    public List<PublicCourseDto> FeaturedCourses { get; set; }
    public List<LiveSessionResponseDto> UpcomingLiveSessions { get; set; }
    public List<TestimonialDto> Testimonials { get; set; }
}

public class LandingStatsDto
{
    public int TotalStudents { get; set; }
    public int TotalCourses { get; set; }
    public int TotalInstructors { get; set; }
    public double SatisfactionRate { get; set; }
    public long TotalVideoHours { get; set; }
    public int TotalCertificatesIssued { get; set; }
}
```

#### 3.2.2 Application Layer — Interface & Service

**الملف:** `Application/Interfaces/IPublicService.cs` (جديد)

```csharp
public interface IPublicService
{
    Task<LandingDto> GetLandingDataAsync();
}
```

**الملف:** `Application/Services/PublicService.cs` (جديد)

```csharp
public class PublicService : IPublicService
{
    private readonly ICourseRepository _courseRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILiveSessionRepository _liveSessionRepository;
    private readonly ITestimonialRepository _testimonialRepository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<PublicService> _logger;

    public async Task<LandingDto> GetLandingDataAsync()
    {
        var cacheKey = "landing_data_v1";
        
        if (_cache.TryGetValue(cacheKey, out LandingDto cachedData))
            return cachedData;

        try
        {
            var statsTask = _courseRepository.GetLandingStatsAsync();
            var categoriesTask = _categoryRepository.GetAllAsync();
            var featuredTask = _courseRepository.GetFeaturedCoursesAsync(6);
            var liveTask = _liveSessionRepository.GetUpcomingAsync(4);
            var testimonialsTask = _testimonialRepository.GetApprovedAsync(10);

            await Task.WhenAll(statsTask, categoriesTask, featuredTask, liveTask, testimonialsTask);

            var result = new LandingDto
            {
                Stats = statsTask.Result,
                Categories = categoriesTask.Result,
                FeaturedCourses = featuredTask.Result,
                UpcomingLiveSessions = liveTask.Result,
                Testimonials = testimonialsTask.Result
            };

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get landing data");
            return GetDefaultLandingData();
        }
    }

    private LandingDto GetDefaultLandingData()
    {
        return new LandingDto
        {
            Stats = new LandingStatsDto(),
            Categories = new List<CategoryResponseDto>(),
            FeaturedCourses = new List<PublicCourseDto>(),
            UpcomingLiveSessions = new List<LiveSessionResponseDto>(),
            Testimonials = new List<TestimonialDto>()
        };
    }
}
```

#### 3.2.3 Infrastructure Layer

**الملف:** `Infrastructure/Repositories/CourseRepository.cs`

```csharp
public async Task<LandingStatsDto> GetLandingStatsAsync()
{
    var totalStudents = await _context.Users
        .Where(u => u.Role == UserRole.Student && u.DeletedAt == null)
        .CountAsync();

    var totalCourses = await _context.Courses
        .Where(c => c.Status == CourseStatus.Published && c.DeletedAt == null)
        .CountAsync();

    var totalInstructors = await _context.Users
        .Where(u => u.Role == UserRole.Instructor && u.DeletedAt == null)
        .CountAsync();

    var totalVideoHours = await _context.VideoContents
        .Where(v => v.DeletedAt == null)
        .SumAsync(v => (double?)v.DurationSeconds) ?? 0;

    var totalCertificates = await _context.Certificates
        .Where(c => c.DeletedAt == null)
        .CountAsync();

    var satisfactionRate = await _context.Reviews
        .Where(r => r.DeletedAt == null)
        .AverageAsync(r => (double?)r.Rating) ?? 0;

    return new LandingStatsDto
    {
        TotalStudents = totalStudents,
        TotalCourses = totalCourses,
        TotalInstructors = totalInstructors,
        SatisfactionRate = Math.Round(satisfactionRate / 5.0 * 100, 1),
        TotalVideoHours = (long)(totalVideoHours / 3600),
        TotalCertificatesIssued = totalCertificates
    };
}

public async Task<List<PublicCourseDto>> GetFeaturedCoursesAsync(int count)
{
    return await _context.Courses
        .Where(c => c.IsFeatured && c.Status == CourseStatus.Published && c.DeletedAt == null)
        .OrderByDescending(c => c.EnrollmentCount)
        .Take(count)
        .Select(c => new PublicCourseDto
        {
            Id = c.Id,
            Title = c.Title,
            Slug = c.Slug,
            Description = c.Description,
            CourseImageUrl = c.ImageUrl,
            Price = c.Price,
            IsFree = c.Price == 0,
            Level = c.Level,
            Language = c.Language,
            CategoryName = c.Category.Name,
            CategoryId = c.CategoryId,
            InstructorName = $"{c.CreatedByUser.FirstName} {c.CreatedByUser.LastName}",
            AverageRating = c.Reviews.Any() ? c.Reviews.Average(r => r.Rating) : 0,
            EnrollmentCount = c.Enrollments.Count,
            TotalDurationMinutes = c.TotalDurationMinutes,
            SectionCount = c.Sections.Count,
            LessonCount = c.Sections.Sum(s => s.Items.Count),
            PublishedAt = c.PublishedAt
        })
        .ToListAsync();
}
```

#### 3.2.4 API Layer

**الملف:** `Controllers/PublicController.cs` (جديد)

```csharp
[ApiController]
[Route("api/public")]
public class PublicController : ControllerBase
{
    private readonly IPublicService _publicService;

    public PublicController(IPublicService publicService)
    {
        _publicService = publicService;
    }

    [HttpGet("landing")]
    [AllowAnonymous]
    [ResponseCache(Duration = 300)]
    public async Task<IActionResult> GetLanding()
    {
        var data = await _publicService.GetLandingDataAsync();
        return Ok(ApiResponse<LandingDto>.Success(data));
    }

    [HttpGet("stats")]
    [AllowAnonymous]
    [ResponseCache(Duration = 300)]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _publicService.GetLandingDataAsync();
        return Ok(ApiResponse<LandingStatsDto>.Success(stats.Stats));
    }
}
```

---

### 3.3 🟡 Testimonials API

**الوصف:** إضافة كيان Testimonial لإدارة توصيات الطلاب.

#### 3.3.1 Domain Layer

**الملف:** `Domain/Entities/Testimonial.cs` (جديد)

```csharp
public class Testimonial : BaseEntity
{
    [Required]
    [StringLength(1000)]
    public string Content { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; }

    public bool IsApproved { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;
    
    public bool IsFlagged { get; set; } = false;
    public string? FlagReason { get; set; }
}
```

#### 3.3.2 Application Layer

**الملف:** `Application/DTOs/Public/TestimonialDto.cs` (جديد)

```csharp
public class TestimonialDto
{
    public Guid Id { get; set; }
    public string Content { get; set; }
    public int Rating { get; set; }
    public string UserName { get; set; }
    public string? UserAvatar { get; set; }
    public string? UserRole { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateTestimonialDto
{
    [Required]
    [StringLength(1000, MinimumLength = 10)]
    public string Content { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }
}

public class UpdateTestimonialDto
{
    public string? Content { get; set; }
    public int? Rating { get; set; }
    public bool? IsApproved { get; set; }
    public int? DisplayOrder { get; set; }
}
```

#### 3.3.3 API Layer

**الملف:** `Controllers/PublicTestimonialsController.cs` (جديد)

```csharp
[ApiController]
[Route("api/public/testimonials")]
public class PublicTestimonialsController : ControllerBase
{
    private readonly ITestimonialService _testimonialService;

    [HttpGet]
    [AllowAnonymous]
    [ResponseCache(Duration = 600)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? minRating = null)
    {
        var testimonials = await _testimonialService.GetApprovedAsync(page, pageSize, minRating);
        return Ok(ApiResponse<List<TestimonialDto>>.Success(testimonials));
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateTestimonialDto dto)
    {
        var userId = User.GetUserId();
        var testimonial = await _testimonialService.CreateAsync(userId, dto);
        return CreatedAtAction(nameof(GetAll), ApiResponse<TestimonialDto>.Success(testimonial));
    }
}
```

**الملف:** `Controllers/AdminTestimonialsController.cs` (جديد)

```csharp
[ApiController]
[Route("api/admin/testimonials")]
[Authorize(Roles = "Admin")]
public class AdminTestimonialsController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? isApproved = null)
    {
        var testimonials = await _testimonialService.GetAllAsync(isApproved);
        return Ok(ApiResponse<List<TestimonialDto>>.Success(testimonials));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTestimonialDto dto)
    {
        var testimonial = await _testimonialService.UpdateAsync(id, dto);
        return Ok(ApiResponse<TestimonialDto>.Success(testimonial));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _testimonialService.DeleteAsync(id);
        return NoContent();
    }

    [HttpPatch("{id}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        await _testimonialService.ApproveAsync(id);
        return Ok(ApiResponse<null>.Success(null, "Testimonial approved"));
    }

    [HttpPatch("{id}/flag")]
    public async Task<IActionResult> Flag(Guid id, [FromBody] string reason)
    {
        await _testimonialService.FlagAsync(id, reason);
        return Ok(ApiResponse<null>.Success(null, "Testimonial flagged"));
    }

    [HttpPut("reorder")]
    public async Task<IActionResult> Reorder([FromBody] List<ReorderItemDto> items)
    {
        await _testimonialService.ReorderAsync(items);
        return Ok(ApiResponse<null>.Success(null, "Testimonials reordered"));
    }
}
```

#### 3.3.4 Seed Data

**الملف:** `Infrastructure/Data/Seeders/TestimonialSeeder.cs` (جديد)

```csharp
public static class TestimonialSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Testimonials.AnyAsync())
            return;

        var testimonials = new List<Testimonial>
        {
            new Testimonial
            {
                Content = "منصة رائعة، تعلمت الكثير عن التاريخ الإسلامي بطريقة ممتعة وتفاعلية.",
                Rating = 5,
                UserId = Guid.Parse("user-guid-1"),
                IsApproved = true,
                DisplayOrder = 1
            },
            new Testimonial
            {
                Content = "المدربون محترفون والمحتوى عالي الجودة. أنصح بها بشدة.",
                Rating = 5,
                UserId = Guid.Parse("user-guid-2"),
                IsApproved = true,
                DisplayOrder = 2
            }
        };

        context.Testimonials.AddRange(testimonials);
        await context.SaveChangesAsync();
    }
}
```

---

### 3.4 🟡 Notification Preferences API

**الوصف:** السماح للمستخدم بتخصيص إعدادات الإشعارات.

#### 3.4.1 Domain Layer

**الملف:** `Domain/Entities/NotificationPreference.cs` (جديد)

```csharp
public class NotificationPreference : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; }

    public bool EmailNotifications { get; set; } = true;
    public bool PushNotifications { get; set; } = true;
    public bool CourseUpdates { get; set; } = true;
    public bool MarketingEmails { get; set; } = false;
    public bool NewMessageAlerts { get; set; } = true;
    public bool LiveSessionReminders { get; set; } = true;
    public bool QuizReminders { get; set; } = true;
    public bool CertificateAchievements { get; set; } = true;
    public bool AnnouncementAlerts { get; set; } = true;
}
```

#### 3.4.2 Application Layer

**الملف:** `Application/DTOs/Notification/NotificationPreferencesDto.cs` (جديد)

```csharp
public class NotificationPreferencesDto
{
    public bool EmailNotifications { get; set; }
    public bool PushNotifications { get; set; }
    public bool CourseUpdates { get; set; }
    public bool MarketingEmails { get; set; }
    public bool NewMessageAlerts { get; set; }
    public bool LiveSessionReminders { get; set; }
    public bool QuizReminders { get; set; }
    public bool CertificateAchievements { get; set; }
    public bool AnnouncementAlerts { get; set; }
}

public class UpdateNotificationPreferencesDto
{
    public bool? EmailNotifications { get; set; }
    public bool? PushNotifications { get; set; }
    public bool? CourseUpdates { get; set; }
    public bool? MarketingEmails { get; set; }
    public bool? NewMessageAlerts { get; set; }
    public bool? LiveSessionReminders { get; set; }
    public bool? QuizReminders { get; set; }
    public bool? CertificateAchievements { get; set; }
    public bool? AnnouncementAlerts { get; set; }
}
```

#### 3.4.3 API Layer

**الملف:** `Controllers/NotificationPreferencesController.cs` (جديد)

```csharp
[ApiController]
[Route("api/notifications/preferences")]
[Authorize]
public class NotificationPreferencesController : ControllerBase
{
    private readonly INotificationPreferenceService _service;

    [HttpGet]
    public async Task<IActionResult> GetPreferences()
    {
        var userId = User.GetUserId();
        var preferences = await _service.GetByUserIdAsync(userId);
        return Ok(ApiResponse<NotificationPreferencesDto>.Success(preferences));
    }

    [HttpPut]
    public async Task<IActionResult> UpdatePreferences([FromBody] UpdateNotificationPreferencesDto dto)
    {
        var userId = User.GetUserId();
        var preferences = await _service.UpdateAsync(userId, dto);
        return Ok(ApiResponse<NotificationPreferencesDto>.Success(preferences));
    }
}
```

#### 3.4.4 Migration Strategy

```csharp
public override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.CreateTable(
        name: "NotificationPreferences",
        columns: table => new
        {
            Id = table.Column<Guid>(type: "uuid", nullable: false),
            UserId = table.Column<Guid>(type: "uuid", nullable: false),
            EmailNotifications = table.Column<bool>(nullable: false, defaultValue: true),
            PushNotifications = table.Column<bool>(nullable: false, defaultValue: true),
            CourseUpdates = table.Column<bool>(nullable: false, defaultValue: true),
            MarketingEmails = table.Column<bool>(nullable: false, defaultValue: false),
            NewMessageAlerts = table.Column<bool>(nullable: false, defaultValue: true),
            LiveSessionReminders = table.Column<bool>(nullable: false, defaultValue: true),
            QuizReminders = table.Column<bool>(nullable: false, defaultValue: true),
            CertificateAchievements = table.Column<bool>(nullable: false, defaultValue: true),
            AnnouncementAlerts = table.Column<bool>(nullable: false, defaultValue: true),
            CreatedAt = table.Column<DateTime>(nullable: false),
            UpdatedAt = table.Column<DateTime>(nullable: true)
        },
        constraints: table =>
        {
            table.PrimaryKey("PK_NotificationPreferences", x => x.Id);
            table.ForeignKey(
                name: "FK_NotificationPreferences_Users_UserId",
                column: x => x.UserId,
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        });

    migrationBuilder.CreateIndex(
        name: "IX_NotificationPreferences_UserId",
        table: "NotificationPreferences",
        column: "UserId",
        unique: true);

    migrationBuilder.Sql(@"
        INSERT INTO ""NotificationPreferences"" (""Id"", ""UserId"", ""EmailNotifications"", 
            ""PushNotifications"", ""CourseUpdates"", ""MarketingEmails"", ""NewMessageAlerts"", 
            ""LiveSessionReminders"", ""QuizReminders"", ""CertificateAchievements"", 
            ""AnnouncementAlerts"", ""CreatedAt"")
        SELECT 
            gen_random_uuid(),
            u.""Id"",
            true, true, true, false, true, true, true, true, true,
            NOW()
        FROM ""Users"" u
        WHERE NOT EXISTS (
            SELECT 1 FROM ""NotificationPreferences"" np 
            WHERE np.""UserId"" = u.""Id""
        )
        AND u.""DeletedAt"" IS NULL
    ");
}
```

---

### 3.5 🟡 Contact Form API

**الوصف:** تخزين رسائل التواصل من صفحة "اتصل بنا".

#### 3.5.1 Domain Layer

**الملف:** `Domain/Entities/ContactMessage.cs` (جديد)

```csharp
public class ContactMessage : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string FullName { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string Email { get; set; }

    [Phone]
    [StringLength(20)]
    public string? Phone { get; set; }

    [Required]
    [StringLength(200)]
    public string Subject { get; set; }

    [Required]
    [StringLength(2000)]
    public string Message { get; set; }

    public bool IsRead { get; set; } = false;
    public Guid? AssignedToId { get; set; }
    public User? AssignedTo { get; set; }
}
```

#### 3.5.2 API Layer

**الملف:** `Controllers/PublicContactController.cs` (جديد)

```csharp
[ApiController]
[Route("api/public/contact")]
public class PublicContactController : ControllerBase
{
    private readonly IContactService _contactService;

    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("Contact")]
    public async Task<IActionResult> Send([FromBody] ContactMessageDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<null>.Error("Invalid data"));

        if (ContainsSpamKeywords(dto.Message))
            return BadRequest(ApiResponse<null>.Error("Message contains spam content"));

        await _contactService.CreateAsync(dto);
        return Ok(ApiResponse<null>.Success(null, "Message sent successfully"));
    }

    private bool ContainsSpamKeywords(string content)
    {
        var spamKeywords = new[] { "viagra", "casino", "lottery" };
        return spamKeywords.Any(keyword => content.ToLower().Contains(keyword));
    }
}
```

**الملف:** `Controllers/AdminContactController.cs` (جديد)

```csharp
[ApiController]
[Route("api/admin/contact")]
[Authorize(Roles = "Admin")]
public class AdminContactController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? isRead = null)
    {
        var messages = await _contactService.GetAllAsync(isRead);
        return Ok(ApiResponse<List<ContactMessageDto>>.Success(messages));
    }

    [HttpPut("{id}/mark-read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        await _contactService.MarkAsReadAsync(id);
        return Ok(ApiResponse<null>.Success(null, "Message marked as read"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _contactService.DeleteAsync(id);
        return NoContent();
    }
}
```

#### 3.5.3 Rate Limiting Configuration

**الملف:** `Program.cs`

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("Contact", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1)
            }));
});
```

---

### 3.6 🟢 About Page API

**الملف:** `Controllers/PublicAboutController.cs` (جديد)

```csharp
[ApiController]
[Route("api/public/about")]
public class PublicAboutController : ControllerBase
{
    private readonly ISystemSettingsService _settingsService;

    [HttpGet]
    [AllowAnonymous]
    [ResponseCache(Duration = 3600)]
    public async Task<IActionResult> GetAboutInfo()
    {
        var aboutData = new
        {
            Title = await _settingsService.GetByKeyAsync("About.Title"),
            Description = await _settingsService.GetByKeyAsync("About.Description"),
            Mission = await _settingsService.GetByKeyAsync("About.Mission"),
            Vision = await _settingsService.GetByKeyAsync("About.Vision"),
            Stats = new
            {
                ManuscriptsCount = 200,
                LearnersCount = 10000,
                YearsOfExperience = 14
            }
        };

        return Ok(ApiResponse<object>.Success(aboutData));
    }
}
```

---

### 3.7 🟢 Legal Pages API

**الملف:** `Domain/Entities/LegalPage.cs` (جديد)

```csharp
public class LegalPage : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string Type { get; set; } // "privacy", "terms", "refund"

    [Required]
    [StringLength(200)]
    public string Title { get; set; }

    [Required]
    public string Content { get; set; }

    public DateTime LastUpdatedAt { get; set; }
    
    [StringLength(20)]
    public string? Version { get; set; }
    
    public bool IsPublished { get; set; } = true;
    public Guid? LastUpdatedById { get; set; }
    public User? LastUpdatedBy { get; set; }
}
```

**الملف:** `Controllers/PublicLegalController.cs` (جديد)

```csharp
[ApiController]
[Route("api/public/legal")]
public class PublicLegalController : ControllerBase
{
    private readonly ILegalPageService _legalPageService;

    [HttpGet("{type}")]
    [AllowAnonymous]
    [ResponseCache(Duration = 3600)]
    public async Task<IActionResult> GetLegalContent(string type)
    {
        var validTypes = new[] { "privacy", "terms", "refund" };
        
        if (!validTypes.Contains(type.ToLower()))
            return BadRequest(ApiResponse<null>.Error("Invalid legal page type"));

        var content = await _legalPageService.GetByTypeAsync(type.ToLower());
        
        if (content == null)
            return NotFound(ApiResponse<null>.Error("Legal page not found"));

        return Ok(ApiResponse<LegalPageDto>.Success(content));
    }
}
```

---

## 4. تعديلات Frontend

### 4.1 🔴 تحديث `src/lib/api.ts` — Refresh Token Logic

**الملف:** `src/lib/api.ts`

```ts
import axios, { AxiosError, InternalAxiosRequestConfig } from 'axios';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'https://localhost:7001/api',
  timeout: 15000,
  headers: { 'Content-Type': 'application/json' },
});

// Request interceptor
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('auth-token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor — Refresh Token + Error Handling
let isRefreshing = false;
let failedQueue: Array<{
  resolve: (value?: unknown) => void;
  reject: (reason?: unknown) => void;
}> = [];

const processQueue = (error: unknown, token: string | null = null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error);
    } else {
      prom.resolve(token);
    }
  });
  failedQueue = [];
};

api.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean };

    if (error.response?.status === 401 && !originalRequest._retry) {
      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        }).then((token) => {
          originalRequest.headers.Authorization = `Bearer ${token}`;
          return api(originalRequest);
        });
      }

      originalRequest._retry = true;
      isRefreshing = true;

      try {
        const refreshToken = localStorage.getItem('refresh-token');
        
        if (!refreshToken) {
          throw new Error('No refresh token');
        }

        const { data } = await axios.post(
          `${import.meta.env.VITE_API_URL}/auth/refresh`,
          { refreshToken }
        );

        const newAccessToken = data.data.accessToken;
        const newRefreshToken = data.data.refreshToken;

        localStorage.setItem('auth-token', newAccessToken);
        localStorage.setItem('refresh-token', newRefreshToken);

        processQueue(null, newAccessToken);
        
        originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;
        return api(originalRequest);
      } catch (refreshError) {
        processQueue(refreshError, null);
        localStorage.removeItem('auth-token');
        localStorage.removeItem('refresh-token');
        window.location.href = '/auth';
        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    }

    if (error.response) {
      const { status, data } = error.response;
      
      switch (status) {
        case 400:
          console.error('Bad Request:', data?.message || data?.errors);
          break;
        case 403:
          console.error('Forbidden:', data?.message);
          break;
        case 404:
          console.error('Not Found:', data?.message);
          break;
        case 429:
          console.error('Rate Limited:', data?.message);
          break;
        case 500:
          console.error('Server Error:', data?.message);
          break;
      }
    }

    return Promise.reject(error);
  }
);

export default api;
```

---

### 4.2 🔴 إنشاء `src/types/api.ts` — كل الـ DTOs

**الملف:** `src/types/api.ts` (جديد — كبير جداً)

```ts
// ============================
// 1. API Envelope
// ============================
export interface ApiResponse<T> {
  success: boolean;
  message?: string;
  data: T;
  errors?: string[];
}

export interface PagedList<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

// ============================
// 2. Auth DTOs
// ============================
export interface LoginRequest {
  email: string;
  password: string;
  rememberMe?: boolean;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  confirmPassword: string;
  gender?: Gender;
  dateOfBirth?: string;
  phoneNumber?: string;
  country?: string;
  city?: string;
  streetLine1?: string;
  postalCode?: string;
}

export interface RegisterResponse {
  userId: string;
  email: string;
  message: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  sessionId: string;
  expiresAt: string;
  user: UserInfoDto;
}

export interface UserInfoDto {
  id: string;
  email: string;
  fullName: string;
  profilePictureUrl?: string;
  isActive: boolean;
  emailConfirmed: boolean;
  roles: string[];
}

export interface VerifyEmailRequest {
  token: string;
  email: string;
}

export interface ResetPasswordRequest {
  token: string;
  email: string;
  newPassword: string;
  confirmPassword: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

export interface OAuthLoginRequest {
  idToken: string;
  provider: 'google' | 'microsoft';
}

export interface SessionDto {
  id: string;
  ipAddress: string;
  userAgent: string;
  createdAt: string;
  lastUsed?: string;
  isActive: boolean;
}

export type Gender = 'Male' | 'Female' | 'Other' | 'PreferNotToSay';

// ============================
// 3. Profile DTOs
// ============================
export interface ProfileDto {
  id: string;
  fullName: string;
  email: string;
  bio?: string;
  gender?: Gender;
  dateOfBirth?: string;
  nationality?: string;
  profileImageUrl?: string;
  createdAt: string;
  phones: PhoneDto[];
  addresses: AddressDto[];
}

export interface PublicProfileDto {
  id: string;
  fullName?: string;
  slug?: string;
  bio?: string;
  nationality?: string;
  profileImageUrl?: string;
  createdAt: string;
}

export interface UpdateProfileRequest {
  firstName?: string;
  lastName?: string;
  bio?: string;
  gender?: Gender;
  dateOfBirth?: string;
  nationality?: string;
}

export interface PhoneDto {
  id: string;
  phoneNumber: string;
  type: PhoneType;
  isVerified: boolean;
  isDefault: boolean;
}

export type PhoneType = 'Primary' | 'Secondary';

export interface AddressDto {
  id: string;
  type: string;
  streetLine1: string;
  streetLine2?: string;
  city: string;
  stateProvince?: string;
  postalCode: string;
  country: string;
  contactPhone?: string;
  isDefault: boolean;
}

export interface AddPhoneRequest {
  phoneNumber: string;
  type: PhoneType;
  isDefault: boolean;
}

export interface AddAddressRequest {
  type: string;
  streetLine1: string;
  streetLine2?: string;
  city: string;
  stateProvince?: string;
  postalCode: string;
  country: string;
  contactPhone?: string;
  isDefault: boolean;
}

// ============================
// 4. Category DTOs
// ============================
export interface CategoryResponseDto {
  id: string;
  name: string;
  description?: string;
  parentId?: string;
  imageUrl?: string;
  slug?: string;
  position: number;
  children: CategoryResponseDto[];
}

// ============================
// 5. Course DTOs
// ============================
export type CourseLevel = 'Beginner' | 'Intermediate' | 'Advanced';
export type CourseLanguage = 'Ar' | 'En';
export type CourseStatus = 'Draft' | 'PendingReview' | 'Published' | 'Archived';
export type PublicCourseSortBy = 'PublishedAt' | 'Price' | 'AverageRating' | 'EnrollmentCount' | 'Title';

export interface PublicCourseDto {
  id: string;
  title: string;
  slug: string;
  description?: string;
  courseImageUrl?: string;
  price: number;
  isFree: boolean;
  level: CourseLevel;
  language: CourseLanguage;
  categoryName: string;
  categoryId: string;
  instructorName: string;
  averageRating: number;
  enrollmentCount: number;
  totalDurationMinutes: number;
  sectionCount: number;
  lessonCount: number;
  publishedAt?: string;
}

export interface PublicCourseDetailDto {
  id: string;
  title: string;
  slug: string;
  description?: string;
  courseImageUrl?: string;
  introVideoUrl?: string;
  price: number;
  isFree: boolean;
  level: CourseLevel;
  language: CourseLanguage;
  categoryName: string;
  categoryId: string;
  instructor: PublicInstructorDto;
  averageRating: number;
  enrollmentCount: number;
  totalDurationMinutes: number;
  requirements: string[];
  learningOutcomes: string[];
  sections: PublicSectionDto[];
  version: number;
  publishedAt?: string;
  lastContentUpdateAt?: string;
}

export interface PublicInstructorDto {
  id: string;
  fullName: string;
  bio?: string;
  profileImageUrl?: string;
}

export interface PublicSectionDto {
  id: string;
  title: string;
  description?: string;
  position: number;
  items: PublicSectionItemDto[];
}

export interface PublicSectionItemDto {
  id: string;
  itemType: SectionItemType;
  position: number;
  isPreviewAllowed: boolean;
}

export type SectionItemType = 'Video' | 'Quiz' | 'Document' | 'LiveSession';

export interface PublicCourseFilterDto {
  searchQuery?: string;
  categoryId?: string;
  level?: CourseLevel;
  language?: CourseLanguage;
  minPrice?: number;
  maxPrice?: number;
  isFreeOnly?: boolean;
  minRating?: number;
  sortBy?: PublicCourseSortBy;
  sortDescending?: boolean;
  page?: number;
  pageSize?: number;
}

export interface PlatformStatsDto {
  totalCourses: number;
  totalStudents: number;
  totalInstructors: number;
  totalCategories: number;
}

export interface FilterOptionsDto {
  categories: FilterOptionItem[];
  levels: CourseLevel[];
  languages: CourseLanguage[];
  minPrice: number;
  maxPrice: number;
}

export interface FilterOptionItem {
  id: string;
  name: string;
  courseCount: number;
}

export interface CourseSuggestionDto {
  id: string;
  title: string;
  slug: string;
  categoryName: string;
}

// ============================
// 6. Enrollment DTOs
// ============================
export type EnrollmentStatus = 'InProgress' | 'Completed' | 'Expired' | 'Refunded';
export type EnrollmentSource = 'Purchase' | 'Gift' | 'AdminGrant' | 'Coupon';

export interface EnrollmentResponseDto {
  id: string;
  userId: string;
  courseId: string;
  courseTitle: string;
  enrolledAt: string;
  status: EnrollmentStatus;
  progressPercentage: number;
  completedAt?: string;
  lastAccessedAt?: string;
  source: EnrollmentSource;
  accessExpiresAt?: string;
  isRefunded: boolean;
}

export interface EnrollmentDetailDto extends EnrollmentResponseDto {
  progresses: ContentProgressDto[];
}

export interface ContentProgressDto {
  id: string;
  enrollmentId: string;
  contentType: ContentType;
  contentId: string;
  isCompleted: boolean;
  watchTimeSeconds: number;
  attemptsCount: number;
  completionPercentage: number;
  metadata?: string;
  lastAccessedAt?: string;
  completedAt?: string;
}

export type ContentType = 'Video' | 'Quiz' | 'Document' | 'LiveSession';

export interface UpdateProgressRequest {
  watchTimeSeconds: number;
  completionPercentage?: number;
  metadata?: string;
  markAsCompleted: boolean;
}

// ============================
// 7. Cart & Order DTOs
// ============================
export interface CartResponseDto {
  id: string;
  items: CartItemDto[];
  subtotal: number;
  couponCode?: string;
  discountAmount: number;
  finalAmount: number;
}

export interface CartItemDto {
  id: string;
  courseId: string;
  courseTitle: string;
  courseImageUrl?: string;
  instructorName?: string;
  priceSnapshot: number;
  currentPrice: number;
  addedAt: string;
}

export interface ApplyCouponResponse {
  code: string;
  discountAmount: number;
  finalAmount: number;
  message?: string;
}

export interface OrderResponseDto {
  id: string;
  orderNumber: string;
  subtotal: number;
  discountAmount: number;
  finalAmount: number;
  status: string;
  couponCode?: string;
  itemCount: number;
  createdAt: string;
}

export interface OrderDetailDto {
  id: string;
  orderNumber: string;
  subtotal: number;
  discountAmount: number;
  finalAmount: number;
  status: string;
  couponCode?: string;
  items: OrderItemDto[];
  payments: PaymentHistoryDto[];
  createdAt: string;
}

export interface OrderItemDto {
  id: string;
  courseId: string;
  courseTitle: string;
  priceAtPurchase: number;
}

export interface PaymentHistoryDto {
  id: string;
  amount: number;
  status: string;
  gatewayResponse?: string;
  createdAt: string;
}

// ============================
// 8. Payment DTOs
// ============================
export interface PaymentResponseDto {
  id: string;
  orderId: string;
  amount: number;
  status: string;
  gatewayTransactionId?: string;
  gatewayResponse?: string;
  paymentMethodName?: string;
  createdAt: string;
}

export interface PaymentMethodResponse {
  id: string;
  name: string;
  provider: string;
  type: string;
  isActive: boolean;
  configuration?: string;
}

// ============================
// 9. Quiz DTOs
// ============================
export type QuestionType = 'MultipleChoice' | 'TrueFalse' | 'ShortAnswer';
export type QuizAttemptStatus = 'InProgress' | 'Submitted' | 'Graded';

export interface QuizResponseDto {
  id: string;
  title: string;
  description?: string;
  durationMinutes?: number;
  passingScorePercent: number;
  maxAttempts?: number;
  shuffleQuestions: boolean;
  shuffleOptions: boolean;
  showResultsImmediately: boolean;
  allowReview: boolean;
  totalPoints: number;
  questionCount: number;
  createdAt: string;
  questions: QuestionResponseDto[];
}

export interface QuestionResponseDto {
  id: string;
  questionText: string;
  type: QuestionType;
  points: number;
  explanation?: string;
  position: number;
  options: OptionResponseDto[];
}

export interface OptionResponseDto {
  id: string;
  optionText: string;
  isCorrect?: boolean;
  position: number;
}

export interface SubmitAnswerDto {
  questionId: string;
  selectedOptionId?: string;
  answerText?: string;
}

export interface SubmitAttemptRequest {
  answers: SubmitAnswerDto[];
}

export interface QuizAttemptResponseDto {
  id: string;
  enrollmentId: string;
  quizId: string;
  attemptNumber: number;
  startedAt: string;
  submittedAt?: string;
  scorePercentage: number;
  isPassed: boolean;
  status: string;
}

export interface QuizResultDto extends QuizAttemptResponseDto {
  isAutoSubmitted: boolean;
  answers: AnswerResultDto[];
}

export interface AnswerResultDto {
  questionId: string;
  selectedOptionId?: string;
  answerText?: string;
  isCorrect: boolean;
  earnedPoints: number;
}

// ============================
// 10. Certificate DTOs
// ============================
export interface CertificateResponse {
  id: string;
  userId: string;
  userFullName: string;
  courseId: string;
  courseTitle: string;
  verificationCode: string;
  status: string;
  issuedAt: string;
  completedAt: string;
  certificateFileId?: string;
  revokedAt?: string;
}

export interface CertificateVerificationResponse {
  isValid: boolean;
  fullName: string;
  courseTitle: string;
  issuedAt: string;
  completedAt: string;
  verificationCode: string;
  status: string;
}

// ============================
// 11. Notification DTOs
// ============================
export type NotificationType = 'Course' | 'Payment' | 'System' | 'Message' | 'InstructorRequest' | 'Enrollment' | 'Assignment';

export interface NotificationDto {
  id: string;
  title: string;
  message?: string;
  type: NotificationType;
  linkUrl?: string;
  icon?: string;
  isRead: boolean;
  createdAt: string;
  readAt?: string;
}

export interface NotificationListDto {
  notifications: NotificationDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface NotificationPreferencesDto {
  emailNotifications: boolean;
  pushNotifications: boolean;
  courseUpdates: boolean;
  marketingEmails: boolean;
  newMessageAlerts: boolean;
  liveSessionReminders: boolean;
  quizReminders: boolean;
  certificateAchievements: boolean;
  announcementAlerts: boolean;
}

// ============================
// 12. Message DTOs
// ============================
export interface MessageResponse {
  id: string;
  senderId: string;
  receiverId: string;
  content: string;
  sentAt: string;
  isRead: boolean;
  readAt?: string;
}

export interface ConversationResponse {
  otherUserId: string;
  otherUserName: string;
  lastMessage: string;
  lastMessageAt: string;
  unreadCount: number;
}

export interface ConversationListResponse {
  items: ConversationResponse[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface ConversationMessagesResponse {
  items: MessageResponse[];
  page: number;
  pageSize: number;
  totalCount: number;
}

// ============================
// 13. Review DTOs
// ============================
export interface ReviewResponse {
  id: string;
  userId: string;
  userFullName: string;
  courseId: string;
  courseTitle: string;
  rating: number;
  comment?: string;
  status: string;
  isVerified: boolean;
  helpfulCount: number;
  notHelpfulCount: number;
  isFlagged: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface ReviewDetailResponse extends ReviewResponse {
  moderatedAt?: string;
  moderatedBy?: string;
  moderatorName?: string;
  deletedAt?: string;
  flaggedBy?: string;
  flaggedAt?: string;
}

// ============================
// 14. Live Session DTOs
// ============================
export type LiveSessionStatus = 'Scheduled' | 'Live' | 'Finished' | 'Cancelled';

export interface LiveSessionResponseDto {
  id: string;
  title: string;
  description?: string;
  scheduledStart: string;
  scheduledEnd: string;
  status: LiveSessionStatus;
  meetingUrl: string;
  password?: string;
  maxAttendees?: number;
  actualStartAt?: string;
  actualEndAt?: string;
  recordingFileId?: string;
  currentAttendeesCount: number;
}

// ============================
// 15. Wishlist DTOs
// ============================
export interface WishlistItemDto {
  id: string;
  courseId: string;
  courseTitle: string;
  courseImageUrl?: string;
  instructorName?: string;
  price: number;
  addedAt: string;
}

export interface WishlistResponseDto {
  items: WishlistItemDto[];
  count: number;
}

// ============================
// 16. Announcement DTOs
// ============================
export interface AnnouncementResponse {
  id: string;
  title: string;
  content: string;
  target: string;
  courseId?: string;
  createdBy: string;
  createdByName: string;
  isActive: boolean;
  publishedAt?: string;
}

export interface AnnouncementListResponse {
  items: AnnouncementResponse[];
  page: number;
  pageSize: number;
  totalCount: number;
}

// ============================
// 17. Dashboard DTOs
// ============================
export interface DashboardMetricDto {
  label: string;
  value: string;
  change?: number;
  trend: string;
  icon?: string;
  color?: string;
}

export interface ChartSeriesDto {
  labels: string[];
  series: SeriesItemDto[];
}

export interface SeriesItemDto {
  name: string;
  data: number[];
}

export interface DistributionItemDto {
  label: string;
  value: number;
  color?: string;
  percentage: number;
}

export interface StudentOverviewDto {
  metrics: DashboardMetricDto[];
  recentCourses: StudentCourseDto[];
  weeklyActivity: ChartSeriesDto;
  recentCertificates: StudentCertificateDto[];
}

export interface StudentCourseDto {
  id: string;
  enrollmentId: string;
  courseId: string;
  courseTitle: string;
  thumbnailUrl?: string;
  instructorName: string;
  progressPercentage: number;
  status: string;
  lastAccessedAt?: string;
}

export interface StudentCertificateDto {
  id: string;
  courseId: string;
  courseTitle: string;
  verificationCode: string;
  issuedAt: string;
  status: string;
}

export interface InstructorOverviewDto {
  metrics: DashboardMetricDto[];
  courses: InstructorCourseDto[];
  revenueTrend: ChartSeriesDto;
  enrollmentTrend: ChartSeriesDto;
  studentLevelDistribution: DistributionItemDto[];
  pendingEditRequests: number;
}

export interface InstructorCourseDto {
  id: string;
  title: string;
  slug: string;
  thumbnailUrl?: string;
  price: number;
  status: string;
  enrollmentCount: number;
  averageRating: number;
  totalDurationMinutes: number;
  revenue: number;
  progressPercentage: number;
  createdAt: string;
  publishedAt?: string;
}

export interface AdminOverviewDto {
  metrics: DashboardMetricDto[];
  revenueTrend: ChartSeriesDto;
  enrollmentTrend: ChartSeriesDto;
  userGrowth: ChartSeriesDto;
  courseDistribution: DistributionItemDto[];
  topCourses: TopCourseDto[];
  pendingItems: PendingItemsDto;
}

export interface TopCourseDto {
  id: string;
  title: string;
  instructorName?: string;
  price: number;
  enrollmentCount: number;
  averageRating: number;
  revenue: number;
}

export interface PendingItemsDto {
  pendingCourses: number;
  pendingEditRequests: number;
  pendingTeacherRequests: number;
  flaggedReviews: number;
}

// ============================
// 18. Landing DTOs
// ============================
export interface LandingDto {
  stats: LandingStatsDto;
  categories: CategoryResponseDto[];
  featuredCourses: PublicCourseDto[];
  upcomingLiveSessions: LiveSessionResponseDto[];
  testimonials: TestimonialDto[];
}

export interface LandingStatsDto {
  totalStudents: number;
  totalCourses: number;
  totalInstructors: number;
  satisfactionRate: number;
  totalVideoHours: number;
  totalCertificatesIssued: number;
}

export interface TestimonialDto {
  id: string;
  content: string;
  rating: number;
  userName: string;
  userAvatar?: string;
  userRole?: string;
  displayOrder: number;
  createdAt: string;
}

// ============================
// 19. Media DTOs
// ============================
export type StoredFileType = 'Image' | 'Video' | 'Document' | 'Recording' | 'Certificate';
export type FileVisibility = 'Public' | 'EnrolledOnly' | 'Private';
export type FileStatus = 'Uploading' | 'Ready' | 'Failed' | 'Deleted';

export interface UploadUrlRequestDto {
  fileType: StoredFileType;
  fileName: string;
  contentType: string;
  fileSizeBytes: number;
  visibility: FileVisibility;
  relatedEntityId?: string;
  relatedEntityType?: string;
}

export interface UploadUrlResponseDto {
  fileId: string;
  uploadUrl: string;
  objectKey: string;
  bucket: string;
  expiresAt: string;
  requiredHeaders: Record<string, string>;
}

export interface ConfirmUploadDto {
  fileId: string;
  objectKey: string;
  bucket: string;
}

export interface ViewUrlResponseDto {
  fileId: string;
  viewUrl: string;
  expiresAt: string;
  contentType: string;
  sizeBytes: number;
}

export interface MediaFileDto {
  id: string;
  originalName: string;
  filePath: string;
  bucket: string;
  fileType: StoredFileType;
  visibility: FileVisibility;
  status: FileStatus;
  mimeType?: string;
  sizeBytes: number;
  uploadedAt: string;
  uploadedBy: string;
  uploaderName?: string;
}

// ============================
// 20. Contact DTOs
// ============================
export interface ContactMessageDto {
  fullName: string;
  email: string;
  phone?: string;
  subject: string;
  message: string;
}

// ============================
// 21. Instructor Request DTOs
// ============================
export type InstructorRequestStatus = 'Pending' | 'Approved' | 'Rejected' | 'RequiresMoreInfo';
export type DocumentType = 'CV' | 'Certificate' | 'IDCard' | 'Degree' | 'PortfolioLink' | 'Transcript' | 'Other';

export interface InstructorRequestDocumentDto {
  documentType: DocumentType;
  fileId?: string;
  urlValue?: string;
}

export interface SubmitInstructorRequestDto {
  message: string;
  documents: InstructorRequestDocumentDto[];
}

export interface InstructorRequestDto {
  id: string;
  userName: string;
  userEmail: string;
  status: InstructorRequestStatus;
  message?: string;
  submittedAt: string;
  processedAt?: string;
  processedByUserName?: string;
  documentsCount: number;
}

export interface InstructorRequestDetailDto extends InstructorRequestDto {
  documents: InstructorRequestDocumentDto[];
  adminNotes?: string;
  rejectionReason?: string;
}

// ============================
// 22. Refund DTOs
// ============================
export type RefundStatus = 'Requested' | 'Approved' | 'Rejected' | 'Processed';

export interface RefundResponseDto {
  id: string;
  paymentId: string;
  amount: number;
  reason?: string;
  status: string;
  orderNumber?: string;
  requestedAt: string;
  processedAt?: string;
  processedByName?: string;
}

export interface RequestRefundRequest {
  paymentId: string;
  reason?: string;
}

// ============================
// 23. Coupon DTOs
// ============================
export interface CouponResponseDto {
  id: string;
  code: string;
  type: string;
  value: number;
  maxDiscountAmount?: number;
  minimumPurchaseAmount?: number;
  applicableTo: string;
  usageLimit?: number;
  userLimitPerUser?: number;
  timesUsed: number;
  isActive: boolean;
  validFrom?: string;
  validUntil?: string;
  createdAt: string;
}

export interface ValidateCouponResponse {
  code: string;
  isValid: boolean;
  discountAmount: number;
  finalAmount: number;
  message?: string;
}
```

---

### 4.3 🔴 إنشاء كل الـ Services (22 Service)

#### 4.3.1 `src/services/auth.service.ts`

```ts
import api from '@/lib/api';
import type {
  ApiResponse,
  AuthResponse,
  LoginRequest,
  RegisterRequest,
  RegisterResponse,
  VerifyEmailRequest,
  ResetPasswordRequest,
  ChangePasswordRequest,
  SessionDto,
} from '@/types/api';

export const authService = {
  login: (data: LoginRequest) =>
    api.post<ApiResponse<AuthResponse>>('/auth/login', data),

  register: (data: RegisterRequest) =>
    api.post<ApiResponse<RegisterResponse>>('/auth/register', data),

  refresh: (refreshToken: string) =>
    api.post<ApiResponse<AuthResponse>>('/auth/refresh', { refreshToken }),

  verifyEmail: (data: VerifyEmailRequest) =>
    api.post<ApiResponse<null>>('/auth/verify-email', data),

  resendVerification: (email: string) =>
    api.post<ApiResponse<null>>('/auth/resend-verification', { email }),

  forgotPassword: (email: string) =>
    api.post<ApiResponse<null>>('/auth/forgot-password', { email }),

  resetPassword: (data: ResetPasswordRequest) =>
    api.post<ApiResponse<null>>('/auth/reset-password', data),

  changePassword: (data: ChangePasswordRequest) =>
    api.post<ApiResponse<null>>('/auth/change-password', data),

  logout: () =>
    api.post<ApiResponse<null>>('/auth/logout'),

  logoutAll: () =>
    api.post<ApiResponse<null>>('/auth/logout-all'),

  getSessions: () =>
    api.get<ApiResponse<SessionDto[]>>('/auth/sessions'),

  revokeSession: (sessionId: string) =>
    api.delete<ApiResponse<null>>(`/auth/sessions/${sessionId}`),

  loginWithGoogle: (idToken: string) =>
    api.post<ApiResponse<AuthResponse>>('/oauth/google', { idToken, provider: 'google' }),

  loginWithMicrosoft: (idToken: string) =>
    api.post<ApiResponse<AuthResponse>>('/oauth/microsoft', { idToken, provider: 'microsoft' }),
};
```

#### 4.3.2 `src/services/course.service.ts`

```ts
import api from '@/lib/api';
import type {
  ApiResponse,
  PagedList,
  PublicCourseDto,
  PublicCourseDetailDto,
  PublicCourseFilterDto,
  PlatformStatsDto,
  FilterOptionsDto,
  CourseSuggestionDto,
} from '@/types/api';

export const courseService = {
  getPublicList: (filters?: PublicCourseFilterDto) =>
    api.get<ApiResponse<PagedList<PublicCourseDto>>>('/public/courses', { params: filters }),

  getPublicById: (id: string) =>
    api.get<ApiResponse<PublicCourseDetailDto>>(`/public/courses/${id}`),

  getBySlug: (slug: string) =>
    api.get<ApiResponse<PublicCourseDetailDto>>(`/public/courses/slug/${slug}`),

  getStats: () =>
    api.get<ApiResponse<PlatformStatsDto>>('/public/courses/stats'),

  getRelated: (id: string, limit = 4) =>
    api.get<ApiResponse<PublicCourseDto[]>>(`/public/courses/${id}/related`, { params: { limit } }),

  getFilterOptions: () =>
    api.get<ApiResponse<FilterOptionsDto>>('/public/courses/filters/options'),

  searchSuggestions: (query: string, limit = 5) =>
    api.get<ApiResponse<CourseSuggestionDto[]>>('/public/courses/search/suggest', { params: { query, limit } }),
};
```

#### 4.3.3 `src/services/cart.service.ts`

```ts
import api from '@/lib/api';
import type {
  ApiResponse,
  CartResponseDto,
  CartItemDto,
  ApplyCouponResponse,
} from '@/types/api';

export const cartService = {
  get: () =>
    api.get<ApiResponse<CartResponseDto>>('/cart'),

  addItem: (courseId: string) =>
    api.post<ApiResponse<CartItemDto>>('/cart/items', { courseId }),

  removeItem: (itemId: string) =>
    api.delete<ApiResponse<null>>(`/cart/items/${itemId}`),

  applyCoupon: (code: string) =>
    api.post<ApiResponse<ApplyCouponResponse>>('/cart/apply-coupon', { code }),

  removeCoupon: () =>
    api.delete<ApiResponse<null>>('/cart/coupon'),
};
```

#### 4.3.4 `src/services/order.service.ts`

```ts
import api from '@/lib/api';
import type {
  ApiResponse,
  OrderResponseDto,
  OrderDetailDto,
} from '@/types/api';

export const orderService = {
  create: (couponCode?: string) =>
    api.post<ApiResponse<OrderResponseDto>>('/orders', { couponCode }),

  getById: (id: string) =>
    api.get<ApiResponse<OrderDetailDto>>(`/orders/${id}`),

  getMyOrders: () =>
    api.get<ApiResponse<OrderResponseDto[]>>('/orders'),
};
```

#### 4.3.5 `src/services/payment.service.ts` (جديد)

```ts
import api from '@/lib/api';
import type {
  ApiResponse,
  PaymentResponseDto,
  PaymentMethodResponse,
} from '@/types/api';

export const paymentService = {
  process: (orderId: string, paymentMethodId: string) =>
    api.post<ApiResponse<PaymentResponseDto>>(
      `/payments/process?orderId=${orderId}`, 
      { paymentMethodId }
    ),

  getMethods: () =>
    api.get<ApiResponse<PaymentMethodResponse[]>>('/payments/methods'),

  getHistory: (orderId: string) =>
    api.get<ApiResponse<PaymentResponseDto[]>>(`/payments/history/${orderId}`),
};
```

#### 4.3.6 `src/services/enrollment.service.ts`

```ts
import api from '@/lib/api';
import type {
  ApiResponse,
  EnrollmentResponseDto,
  EnrollmentDetailDto,
  ContentProgressDto,
  UpdateProgressRequest,
} from '@/types/api';

export const enrollmentService = {
  enroll: (courseId: string, source: 'Purchase' | 'Gift' | 'AdminGrant' | 'Coupon' = 'Purchase') =>
    api.post<ApiResponse<EnrollmentResponseDto>>('/enrollments', { courseId, source }),

  getMyCourses: () =>
    api.get<ApiResponse<EnrollmentResponseDto[]>>('/enrollments'),

  getById: (id: string) =>
    api.get<ApiResponse<EnrollmentDetailDto>>(`/enrollments/${id}`),

  getProgress: (enrollmentId: string) =>
    api.get<ApiResponse<ContentProgressDto[]>>(`/enrollments/${enrollmentId}/progress`),

  updateProgress: (enrollmentId: string, data: UpdateProgressRequest) =>
    api.put<ApiResponse<ContentProgressDto>>(
      `/enrollments/${enrollmentId}/progress`, 
      data
    ),

  markComplete: (enrollmentId: string, contentType: string, contentId: string) =>
    api.post<ApiResponse<ContentProgressDto>>(
      `/enrollments/${enrollmentId}/progress/${contentType}/${contentId}/complete`
    ),
};
```

#### 4.3.7 `src/services/quiz.service.ts`

```ts
import api from '@/lib/api';
import type {
  ApiResponse,
  QuizAttemptResponseDto,
  QuizResultDto,
  SubmitAttemptRequest,
} from '@/types/api';

export const quizService = {
  startAttempt: (enrollmentId: string, quizId: string) =>
    api.post<ApiResponse<QuizAttemptResponseDto>>(
      `/enrollments/${enrollmentId}/quizzes/${quizId}/attempts`
    ),

  submitAttempt: (
    enrollmentId: string, 
    quizId: string, 
    attemptId: string, 
    data: SubmitAttemptRequest
  ) =>
    api.put<ApiResponse<QuizResultDto>>(
      `/enrollments/${enrollmentId}/quizzes/${quizId}/attempts/${attemptId}`,
      data
    ),

  getAttempts: (enrollmentId: string, quizId: string) =>
    api.get<ApiResponse<QuizAttemptResponseDto[]>>(
      `/enrollments/${enrollmentId}/quizzes/${quizId}/attempts`
    ),

  getAttempt: (enrollmentId: string, quizId: string, attemptId: string) =>
    api.get<ApiResponse<QuizResultDto>>(
      `/enrollments/${enrollmentId}/quizzes/${quizId}/attempts/${attemptId}`
    ),
};
```

#### 4.3.8 `src/services/certificate.service.ts`

```ts
import api from '@/lib/api';
import type {
  ApiResponse,
  CertificateResponse,
  CertificateVerificationResponse,
} from '@/types/api';

export const certificateService = {
  getMyCertificates: () =>
    api.get<ApiResponse<CertificateResponse[]>>('/certificates/my'),

  getById: (id: string) =>
    api.get<ApiResponse<CertificateResponse>>(`/certificates/${id}`),

  download: (id: string) =>
    api.get(`/certificates/${id}/download`, { responseType: 'blob' }),

  verify: (code: string) =>
    api.get<ApiResponse<CertificateVerificationResponse>>(`/certificates/verify/${code}`),
};
```

#### 4.3.9 `src/services/notification.service.ts`

```ts
import api from '@/lib/api';
import type {
  ApiResponse,
  NotificationListDto,
} from '@/types/api';

export const notificationService = {
  getAll: (page = 1, pageSize = 20, isRead?: boolean) =>
    api.get<ApiResponse<NotificationListDto>>('/notifications', { params: { page, pageSize, isRead } }),

  getUnreadCount: () =>
    api.get<ApiResponse<number>>('/notifications/unread-count'),

  markRead: (id: string) =>
    api.patch<ApiResponse<null>>(`/notifications/${id}/read`),

  markAllRead: () =>
    api.post<ApiResponse<number>>('/notifications/mark-all-read'),

  delete: (id: string) =>
    api.delete<ApiResponse<null>>(`/notifications/${id}`),

  clearAll: () =>
    api.delete<ApiResponse<number>>('/notifications/clear-all'),
};
```

#### 4.3.10 `src/services/message.service.ts`

```ts
import api from '@/lib/api';
import type {
  ApiResponse,
  ConversationListResponse,
  ConversationMessagesResponse,
  MessageResponse,
} from '@/types/api';

export const messageService = {
  send: (receiverId: string, content: string) =>
    api.post<ApiResponse<MessageResponse>>('/messages', { receiverId, content }),

  getConversations: (page = 1, pageSize = 20) =>
    api.get<ApiResponse<ConversationListResponse>>('/messages/conversations', { params: { page, pageSize } }),

  getConversation: (otherUserId: string, page = 1, pageSize = 50) =>
    api.get<ApiResponse<ConversationMessagesResponse>>(
      `/messages/conversations/${otherUserId}`, 
      { params: { page, pageSize } }
    ),

  getUnreadCount: () =>
    api.get<ApiResponse<{ unreadCount: number }>>('/messages/unread-count'),

  markRead: (messageId: string) =>
    api.patch<ApiResponse<null>>(`/messages/${messageId}/read`),

  delete: (messageId: string) =>
    api.delete<ApiResponse<null>>(`/messages/${messageId}`),
};
```

#### 4.3.11 `src/services/profile.service.ts`

```ts
import api from '@/lib/api';
import type {
  ApiResponse,
  ProfileDto,
  PublicProfileDto,
  UpdateProfileRequest,
  PhoneDto,
  AddressDto,
  AddPhoneRequest,
  AddAddressRequest,
} from '@/types/api';

export const profileService = {
  getMe: () =>
    api.get<ApiResponse<ProfileDto>>('/profile/me'),

  update: (data: UpdateProfileRequest) =>
    api.put<ApiResponse<ProfileDto>>('/profile', data),

  getById: (userId: string) =>
    api.get<ApiResponse<PublicProfileDto>>(`/profile/${userId}`),

  setPicture: (fileId: string) =>
    api.post<ApiResponse<ProfileDto>>('/profile/picture', { fileId }),

  deletePicture: () =>
    api.delete<ApiResponse<null>>('/profile/picture'),

  getPhones: () =>
    api.get<ApiResponse<PhoneDto[]>>('/profile/phones'),

  addPhone: (data: AddPhoneRequest) =>
    api.post<ApiResponse<PhoneDto>>('/profile/phones', data),

  deletePhone: (id: string) =>
    api.delete<ApiResponse<null>>(`/profile/phones/${id}`),

  setDefaultPhone: (id: string) =>
    api.put<ApiResponse<null>>(`/profile/phones/${id}/default`),

  getAddresses: () =>
    api.get<ApiResponse<AddressDto[]>>('/profile/addresses'),

  createAddress: (data: AddAddressRequest) =>
    api.post<ApiResponse<AddressDto>>('/profile/addresses', data),

  updateAddress: (id: string, data: Partial<AddAddressRequest>) =>
    api.put<ApiResponse<AddressDto>>(`/profile/addresses/${id}`, data),

  deleteAddress: (id: string) =>
    api.delete<ApiResponse<null>>(`/profile/addresses/${id}`),

  setDefaultAddress: (id: string) =>
    api.put<ApiResponse<null>>(`/profile/addresses/${id}/default`),
};
```

#### 4.3.12 `src/services/wishlist.service.ts`

```ts
import api from '@/lib/api';
import type {
  ApiResponse,
  WishlistResponseDto,
  WishlistItemDto,
} from '@/types/api';

export const wishlistService = {
  get: () =>
    api.get<ApiResponse<WishlistResponseDto>>('/wishlist'),

  add: (courseId: string) =>
    api.post<ApiResponse<WishlistItemDto>>(`/wishlist/${courseId}`),

  remove: (courseId: string) =>
    api.delete<ApiResponse<null>>(`/wishlist/${courseId}`),
};
```

#### 4.3.13 `src/services/media.service.ts`

```ts
import api from '@/lib/api';
import type {
  ApiResponse,
  UploadUrlRequestDto,
  UploadUrlResponseDto,
  ConfirmUploadDto,
  ViewUrlResponseDto,
  MediaFileDto,
  PagedList,
} from '@/types/api';
import axios from 'axios';

export const mediaService = {
  getUploadUrl: (data: UploadUrlRequestDto) =>
    api.post<ApiResponse<UploadUrlResponseDto>>('/media/upload-url', data),

  uploadToPresignedUrl: async (
    uploadUrl: string, 
    file: File, 
    requiredHeaders: Record<string, string>
  ) => {
    await axios.put(uploadUrl, file, {
      headers: {
        'Content-Type': file.type,
        ...requiredHeaders,
      },
    });
  },

  confirmUpload: (data: ConfirmUploadDto) =>
    api.post<ApiResponse<MediaFileDto>>('/media/confirm-upload', data),

  uploadFile: async (file: File, fileType: UploadUrlRequestDto['fileType'], visibility: UploadUrlRequestDto['visibility'] = 'Public') => {
    const { data: urlData } = await api.post<ApiResponse<UploadUrlResponseDto>>(
      '/media/upload-url', 
      {
        fileType,
        fileName: file.name,
        contentType: file.type,
        fileSizeBytes: file.size,
        visibility,
      }
    );

    await mediaService.uploadToPresignedUrl(
      urlData.data.uploadUrl,
      file,
      urlData.data.requiredHeaders
    );

    const { data: confirmData } = await api.post<ApiResponse<MediaFileDto>>(
      '/media/confirm-upload',
      {
        fileId: urlData.data.fileId,
        objectKey: urlData.data.objectKey,
        bucket: urlData.data.bucket,
      }
    );

    return confirmData.data;
  },

  getViewUrl: (fileId: string) =>
    api.get<ApiResponse<ViewUrlResponseDto>>(`/media/${fileId}/view-url`),

  getAdminList: (params?: {
    page?: number;
    pageSize?: number;
    fileType?: string;
    searchTerm?: string;
  }) =>
    api.get<ApiResponse<PagedList<MediaFileDto>>>('/admin/media', { params }),

  softDelete: (fileId: string) =>
    api.delete<ApiResponse<null>>(`/admin/media/${fileId}/soft`),

  restore: (fileId: string) =>
    api.post<ApiResponse<null>>(`/admin/media/${fileId}/restore`),

  permanentDelete: (fileId: string) =>
    api.delete<ApiResponse<null>>(`/admin/media/${fileId}`),
};
```

#### 4.3.14 `src/services/review.service.ts` (جديد)

```ts
import api from '@/lib/api';
import type {
  ApiResponse,
  ReviewResponse,
  ReviewDetailResponse,
} from '@/types/api';

export const reviewService = {
  create: (courseId: string, rating: number, comment?: string) =>
    api.post<ApiResponse<ReviewResponse>>('/reviews', { courseId, rating, comment }),

  getCourseReviews: (courseId: string, page = 1, pageSize = 10) =>
    api.get<ApiResponse<ReviewResponse[]>>(`/reviews/course/${courseId}`, { params: { page, pageSize } }),

  getById: (id: string) =>
    api.get<ApiResponse<ReviewDetailResponse>>(`/reviews/${id}`),

  update: (id: string, rating: number, comment?: string) =>
    api.put<ApiResponse<ReviewResponse>>(`/reviews/${id}`, { rating, comment }),

  delete: (id: string) =>
    api.delete<ApiResponse<null>>(`/reviews/${id}`),

  markHelpful: (id: string, isHelpful: boolean) =>
    api.post<ApiResponse<null>>(`/reviews/${id}/helpful`, { isHelpful }),

  flag: (id: string) =>
    api.post<ApiResponse<null>>(`/reviews/${id}/flag`),

  getPending: () =>
    api.get<ApiResponse<ReviewDetailResponse[]>>('/reviews/pending'),

  moderate: (id: string, status: 'Approved' | 'Rejected') =>
    api.put<ApiResponse<ReviewResponse>>(`/reviews/${id}/moderate`, { status }),
};
```

#### 4.3.15 `src/services/liveSession.service.ts` (جديد)

```ts
import api from '@/lib/api';
import type {
  ApiResponse,
  LiveSessionResponseDto,
} from '@/types/api';

export const liveSessionService = {
  getByCourse: (courseId: string) =>
    api.get<ApiResponse<LiveSessionResponseDto[]>>(`/courses/${courseId}/live-sessions`),

  create: (courseId: string, data: {
    sectionId: string;
    title: string;
    description?: string;
    scheduledStart: string;
    scheduledEnd: string;
    meetingUrl: string;
    password?: string;
    maxAttendees?: number;
  }) =>
    api.post<ApiResponse<LiveSessionResponseDto>>(`/courses/${courseId}/live-sessions`, data),

  updateStatus: (courseId: string, sessionId: string, data: {
    status: 'Scheduled' | 'Live' | 'Finished' | 'Cancelled';
    meetingUrl?: string;
    password?: string;
    maxAttendees?: number;
  }) =>
    api.put<ApiResponse<LiveSessionResponseDto>>(
      `/courses/${courseId}/live-sessions/${sessionId}/status`, 
      data
    ),

  delete: (courseId: string, sessionId: string) =>
    api.delete<ApiResponse<null>>(`/courses/${courseId}/live-sessions/${sessionId}`),

  join: (sessionId: string) =>
    api.post<ApiResponse<null>>(`/live-sessions/${sessionId}/attendance/join`),

  leave: (sessionId: string) =>
    api.post<ApiResponse<null>>(`/live-sessions/${sessionId}/attendance/leave`),

  getAttendeeCount: (sessionId: string) =>
    api.get<ApiResponse<number>>(`/live-sessions/${sessionId}/attendance/count`),
};
```

#### 4.3.16 `src/services/announcement.service.ts` (جديد)

```ts
import api from '@/lib/api';
import type {
  ApiResponse,
  AnnouncementResponse,
  AnnouncementListResponse,
} from '@/types/api';

export const announcementService = {
  create: (data: {
    title: string;
    content: string;
    target: 'All' | 'Students' | 'Instructors' | 'Admins' | 'SpecificCourse';
    courseId?: string;
  }) =>
    api.post<ApiResponse<AnnouncementResponse>>('/announcements', data),

  createForCourse: (courseId: string, data: {
    title: string;
    content: string;
  }) =>
    api.post<ApiResponse<AnnouncementResponse>>(`/announcements/course/${courseId}`, data),

  getAll: (page = 1, pageSize = 20) =>
    api.get<ApiResponse<AnnouncementListResponse>>('/announcements', { params: { page, pageSize } }),

  update: (id: string, data: {
    title?: string;
    content?: string;
    target?: string;
    courseId?: string;
    isActive?: boolean;
  }) =>
    api.put<ApiResponse<AnnouncementResponse>>(`/announcements/${id}`, data),

  deactivate: (id: string) =>
    api.patch<ApiResponse<null>>(`/announcements/${id}/deactivate`),

  delete: (id: string) =>
    api.delete<ApiResponse<null>>(`/announcements/${id}`),
};
```

#### 4.3.17 `src/services/instructorRequest.service.ts` (جديد)

```ts
import api from '@/lib/api';
import type {
  ApiResponse,
  InstructorRequestDto,
  InstructorRequestDetailDto,
  SubmitInstructorRequestDto,
} from '@/types/api';

export const instructorRequestService = {
  canSubmit: () =>
    api.get<ApiResponse<boolean>>('/instructor-requests/can-submit'),

  submit: (data: SubmitInstructorRequestDto) =>
    api.post<ApiResponse<InstructorRequestDto>>('/instructor-requests', data),

  getMyRequests: () =>
    api.get<ApiResponse<InstructorRequestDto[]>>('/instructor-requests/my-requests'),

  getMyRequestDetail: (requestId: string) =>
    api.get<ApiResponse<InstructorRequestDetailDto>>(`/instructor-requests/my-requests/${requestId}`),

  cancel: (requestId: string) =>
    api.delete<ApiResponse<null>>(`/instructor-requests/${requestId}/cancel`),

  getPending: () =>
    api.get<ApiResponse<InstructorRequestDto[]>>('/instructor-requests/pending'),

  getDetail: (requestId: string) =>
    api.get<ApiResponse<InstructorRequestDetailDto>>(`/instructor-requests/${requestId}`),

  process: (requestId: string, data: {
    status: 'Pending' | 'Approved' | 'Rejected' | 'RequiresMoreInfo';
    adminNotes?: string;
    rejectionReason?: string;
  }) =>
    api.put<ApiResponse<InstructorRequestDto>>(`/instructor-requests/${requestId}/process`, data),

  delete: (requestId: string) =>
    api.delete<ApiResponse<null>>(`/instructor-requests/${requestId}`),
};
```

#### 4.3.18 `src/services/admin.service.ts` (جديد)

```ts
import api from '@/lib/api';
import type {
  ApiResponse,
  CouponResponseDto,
  PaymentMethodResponse,
  RefundResponseDto,
} from '@/types/api';

export const adminService = {
  getCoupons: (isActive?: boolean) =>
    api.get<ApiResponse<CouponResponseDto[]>>('/admin/coupons', { params: { isActive } }),

  createCoupon: (data: any) =>
    api.post<ApiResponse<CouponResponseDto>>('/admin/coupons', data),

  updateCoupon: (id: string, data: any) =>
    api.put<ApiResponse<CouponResponseDto>>(`/admin/coupons/${id}`, data),

  toggleCoupon: (id: string) =>
    api.patch<ApiResponse<boolean>>(`/admin/coupons/${id}/toggle`),

  deleteCoupon: (id: string) =>
    api.delete<ApiResponse<null>>(`/admin/coupons/${id}`),

  getPaymentMethods: () =>
    api.get<ApiResponse<PaymentMethodResponse[]>>('/admin/payment-methods'),

  createPaymentMethod: (data: any) =>
    api.post<ApiResponse<PaymentMethodResponse>>('/admin/payment-methods', data),

  togglePaymentMethod: (id: string) =>
    api.patch<ApiResponse<boolean>>(`/admin/payment-methods/${id}/toggle`),

  getRefunds: (status?: string) =>
    api.get<ApiResponse<RefundResponseDto[]>>('/admin/refunds', { params: { status } }),

  approveRefund: (refundId: string, adminNotes?: string) =>
    api.post<ApiResponse<RefundResponseDto>>('/admin/refunds/approve', { refundId, adminNotes }),

  rejectRefund: (refundId: string, adminNotes?: string) =>
    api.post<ApiResponse<RefundResponseDto>>('/admin/refunds/reject', { refundId, adminNotes }),
};
```

#### 4.3.19 `src/services/dashboard.service.ts`

```ts
import api from '@/lib/api';
import type {
  ApiResponse,
  StudentOverviewDto,
  InstructorOverviewDto,
  AdminOverviewDto,
  ChartSeriesDto,
} from '@/types/api';

export const dashboardService = {
  getStudentOverview: () =>
    api.get<ApiResponse<StudentOverviewDto>>('/student/dashboard/overview'),

  getStudentCourses: () =>
    api.get<ApiResponse<any[]>>('/student/dashboard/courses'),

  getStudentWeeklyActivity: (weeks = 4) =>
    api.get<ApiResponse<ChartSeriesDto>>('/student/dashboard/weekly-activity', { params: { weeks } }),

  getStudentCertificates: () =>
    api.get<ApiResponse<any[]>>('/student/dashboard/certificates'),

  getInstructorOverview: () =>
    api.get<ApiResponse<InstructorOverviewDto>>('/instructor/dashboard/overview'),

  getInstructorCourses: () =>
    api.get<ApiResponse<any[]>>('/instructor/dashboard/courses'),

  getInstructorRevenue: (months = 12) =>
    api.get<ApiResponse<any>>('/instructor/dashboard/revenue', { params: { months } }),

  getInstructorStudents: () =>
    api.get<ApiResponse<any>>('/instructor/dashboard/students'),

  getInstructorPendingRequests: () =>
    api.get<ApiResponse<any[]>>('/instructor/dashboard/pending-requests'),

  getInstructorRecentReviews: (limit = 10) =>
    api.get<ApiResponse<any[]>>('/instructor/dashboard/recent-reviews', { params: { limit } }),

  getAdminOverview: () =>
    api.get<ApiResponse<AdminOverviewDto>>('/admin/dashboard/overview'),

  getAdminRevenue: (months = 12) =>
    api.get<ApiResponse<any>>('/admin/dashboard/revenue', { params: { months } }),

  getAdminUserGrowth: (months = 6) =>
    api.get<ApiResponse<any>>('/admin/dashboard/user-growth', { params: { months } }),

  getAdminEnrollmentTrend: (months = 12) =>
    api.get<ApiResponse<any>>('/admin/dashboard/enrollment-trend', { params: { months } }),

  getAdminTopCourses: (limit = 10) =>
    api.get<ApiResponse<any[]>>('/admin/dashboard/top-courses', { params: { limit } }),
};
```

#### 4.3.20 `src/services/category.service.ts`

```ts
import api from '@/lib/api';
import type {
  ApiResponse,
  CategoryResponseDto,
} from '@/types/api';

export const categoryService = {
  getAll: () =>
    api.get<ApiResponse<CategoryResponseDto[]>>('/categories'),

  getById: (id: string) =>
    api.get<ApiResponse<CategoryResponseDto>>(`/categories/${id}`),

  create: (data: {
    name: string;
    description?: string;
    slug?: string;
    parentId?: string;
    position: number;
  }) =>
    api.post<ApiResponse<CategoryResponseDto>>('/categories', data),

  update: (id: string, data: any) =>
    api.put<ApiResponse<CategoryResponseDto>>(`/categories/${id}`, data),

  delete: (id: string) =>
    api.delete<ApiResponse<null>>(`/categories/${id}`),

  setImage: (categoryId: string, fileId: string) =>
    api.put<ApiResponse<CategoryResponseDto>>(`/categories/${categoryId}/image`, { fileId }),
};
```

#### 4.3.21 `src/services/publicInstructor.service.ts` (جديد)

```ts
import api from '@/lib/api';
import type {
  ApiResponse,
  PublicProfileDto,
  PublicCourseDto,
} from '@/types/api';

export const publicInstructorService = {
  getBySlug: (slug: string) =>
    api.get<ApiResponse<PublicProfileDto>>(`/public/instructors/${slug}`),

  getCourses: (slug: string) =>
    api.get<ApiResponse<PublicCourseDto[]>>(`/public/instructors/${slug}/courses`),

  checkSlug: (slug: string) =>
    api.get<ApiResponse<boolean>>('/public/instructors/check-slug', { params: { slug } }),

  search: (query: string) =>
    api.get<ApiResponse<PublicProfileDto[]>>('/public/instructors/search', { params: { q: query } }),
};
```

#### 4.3.22 `src/services/public.service.ts` (جديد)

```ts
import api from '@/lib/api';
import type {
  ApiResponse,
  LandingDto,
  TestimonialDto,
  ContactMessageDto,
} from '@/types/api';

export const publicService = {
  getLanding: () =>
    api.get<ApiResponse<LandingDto>>('/public/landing'),

  getTestimonials: (page = 1, pageSize = 10, minRating?: number) =>
    api.get<ApiResponse<TestimonialDto[]>>('/public/testimonials', { params: { page, pageSize, minRating } }),

  submitContact: (data: ContactMessageDto) =>
    api.post<ApiResponse<null>>('/public/contact', data),

  getAbout: () =>
    api.get<ApiResponse<any>>('/public/about'),

  getLegal: (type: 'privacy' | 'terms' | 'refund') =>
    api.get<ApiResponse<any>>(`/public/legal/${type}`),
};
```

---

### 4.4 🔴 SignalR Integration

#### 4.4.1 `src/lib/signalr.ts` (جديد)

```ts
import * as signalR from '@microsoft/signalr';

let notificationConnection: signalR.HubConnection | null = null;
let messagingConnection: signalR.HubConnection | null = null;

export const createNotificationHub = (token: string) => {
  if (notificationConnection) return notificationConnection;

  notificationConnection = new signalR.HubConnectionBuilder()
    .withUrl(`${import.meta.env.VITE_API_URL.replace('/api', '')}/hubs/notifications`, {
      accessTokenFactory: () => token,
    })
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .configureLogging(signalR.LogLevel.Information)
    .build();

  return notificationConnection;
};

export const createMessagingHub = (token: string) => {
  if (messagingConnection) return messagingConnection;

  messagingConnection = new signalR.HubConnectionBuilder()
    .withUrl(`${import.meta.env.VITE_API_URL.replace('/api', '')}/hubs/messaging`, {
      accessTokenFactory: () => token,
    })
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .configureLogging(signalR.LogLevel.Information)
    .build();

  return messagingConnection;
};

export const startHubConnection = async (connection: signalR.HubConnection) => {
  try {
    if (connection.state === signalR.HubConnectionState.Disconnected) {
      await connection.start();
      console.log('SignalR connected');
    }
  } catch (error) {
    console.error('SignalR connection error:', error);
    setTimeout(() => startHubConnection(connection), 5000);
  }
};

export const stopHubConnection = async (connection: signalR.HubConnection | null) => {
  if (connection && connection.state === signalR.HubConnectionState.Connected) {
    await connection.stop();
    console.log('SignalR disconnected');
  }
};

export const getNotificationHub = () => notificationConnection;
export const getMessagingHub = () => messagingConnection;
```

#### 4.4.2 `src/hooks/useSignalR.ts` (جديد)

```ts
import { useEffect, useState } from 'react';
import { useAuth } from './useAuth';
import {
  createNotificationHub,
  createMessagingHub,
  startHubConnection,
  stopHubConnection,
  getNotificationHub,
  getMessagingHub,
} from '@/lib/signalr';
import type { NotificationDto, MessageResponse } from '@/types/api';
import { useNotificationStore } from '@/stores/notificationStore';

export const useSignalR = () => {
  const { user, isAuthenticated } = useAuth();
  const [isConnected, setIsConnected] = useState(false);
  const { addNotification } = useNotificationStore();

  useEffect(() => {
    if (!isAuthenticated || !user) return;

    const token = localStorage.getItem('auth-token');
    if (!token) return;

    const notificationHub = createNotificationHub(token);
    
    notificationHub.on('ReceiveNotification', (notification: NotificationDto) => {
      console.log('Received notification:', notification);
      addNotification(notification);
      
      if ('Notification' in window && Notification.permission === 'granted') {
        new Notification(notification.title, {
          body: notification.message,
          icon: notification.icon || '/logo.png',
        });
      }
    });

    notificationHub.onreconnecting(() => setIsConnected(false));
    notificationHub.onreconnected(() => setIsConnected(true));
    notificationHub.onclose(() => setIsConnected(false));

    const messagingHub = createMessagingHub(token);
    
    messagingHub.on('ReceiveMessage', (message: MessageResponse) => {
      console.log('Received message:', message);
    });

    startHubConnection(notificationHub);
    startHubConnection(messagingHub);
    setIsConnected(true);

    return () => {
      stopHubConnection(getNotificationHub());
      stopHubConnection(getMessagingHub());
    };
  }, [isAuthenticated, user, addNotification]);

  return { isConnected };
};
```

---

### 4.5 🟡 Shared Components

#### 4.5.1 `src/components/shared/Pagination.tsx` (جديد)

```tsx
import { ChevronLeft, ChevronRight } from 'lucide-react';

interface PaginationProps {
  currentPage: number;
  totalPages: number;
  onPageChange: (page: number) => void;
}

export const Pagination = ({ currentPage, totalPages, onPageChange }: PaginationProps) => {
  if (totalPages <= 1) return null;

  const pages = [];
  const maxVisible = 5;
  
  let start = Math.max(1, currentPage - Math.floor(maxVisible / 2));
  let end = Math.min(totalPages, start + maxVisible - 1);
  
  if (end - start < maxVisible - 1) {
    start = Math.max(1, end - maxVisible + 1);
  }

  for (let i = start; i <= end; i++) {
    pages.push(i);
  }

  return (
    <div className="flex items-center justify-center gap-2 mt-8">
      <button
        onClick={() => onPageChange(currentPage - 1)}
        disabled={currentPage === 1}
        className="p-2 rounded-lg border border-gray-300 hover:bg-gray-100 disabled:opacity-50 disabled:cursor-not-allowed"
      >
        <ChevronLeft className="w-5 h-5" />
      </button>

      {start > 1 && (
        <>
          <button
            onClick={() => onPageChange(1)}
            className="px-4 py-2 rounded-lg border border-gray-300 hover:bg-gray-100"
          >
            1
          </button>
          {start > 2 && <span className="px-2">...</span>}
        </>
      )}

      {pages.map((page) => (
        <button
          key={page}
          onClick={() => onPageChange(page)}
          className={`px-4 py-2 rounded-lg border ${
            currentPage === page
              ? 'bg-primary-600 text-white border-primary-600'
              : 'border-gray-300 hover:bg-gray-100'
          }`}
        >
          {page}
        </button>
      ))}

      {end < totalPages && (
        <>
          {end < totalPages - 1 && <span className="px-2">...</span>}
          <button
            onClick={() => onPageChange(totalPages)}
            className="px-4 py-2 rounded-lg border border-gray-300 hover:bg-gray-100"
          >
            {totalPages}
          </button>
        </>
      )}

      <button
        onClick={() => onPageChange(currentPage + 1)}
        disabled={currentPage === totalPages}
        className="p-2 rounded-lg border border-gray-300 hover:bg-gray-100 disabled:opacity-50 disabled:cursor-not-allowed"
      >
        <ChevronRight className="w-5 h-5" />
      </button>
    </div>
  );
};
```

#### 4.5.2 `src/components/shared/Skeleton.tsx` (جديد)

```tsx
interface SkeletonProps {
  className?: string;
}

export const Skeleton = ({ className = '' }: SkeletonProps) => {
  return <div className={`animate-pulse bg-gray-200 rounded ${className}`} />;
};

export const CourseCardSkeleton = () => {
  return (
    <div className="border rounded-lg overflow-hidden">
      <Skeleton className="h-48 w-full" />
      <div className="p-4 space-y-3">
        <Skeleton className="h-6 w-3/4" />
        <Skeleton className="h-4 w-1/2" />
        <Skeleton className="h-4 w-full" />
        <div className="flex justify-between">
          <Skeleton className="h-6 w-20" />
          <Skeleton className="h-8 w-24" />
        </div>
      </div>
    </div>
  );
};

export const DashboardSkeleton = () => {
  return (
    <div className="space-y-6">
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        {[1, 2, 3, 4].map((i) => (
          <div key={i} className="p-6 border rounded-lg">
            <Skeleton className="h-4 w-24 mb-2" />
            <Skeleton className="h-8 w-32" />
          </div>
        ))}
      </div>
      <Skeleton className="h-64 w-full" />
    </div>
  );
};
```

#### 4.5.3 `src/components/shared/ErrorFallback.tsx` (جديد)

```tsx
import { AlertCircle, RefreshCw } from 'lucide-react';

interface ErrorFallbackProps {
  error?: Error;
  onRetry?: () => void;
  title?: string;
  message?: string;
}

export const ErrorFallback = ({
  error,
  onRetry,
  title = 'حدث خطأ',
  message,
}: ErrorFallbackProps) => {
  return (
    <div className="flex flex-col items-center justify-center p-8 text-center">
      <AlertCircle className="w-16 h-16 text-red-500 mb-4" />
      <h3 className="text-xl font-semibold mb-2">{title}</h3>
      <p className="text-gray-600 mb-4">
        {message || error?.message || 'حدث خطأ غير متوقع. يرجى المحاولة مرة أخرى.'}
      </p>
      {onRetry && (
        <button
          onClick={onRetry}
          className="flex items-center gap-2 px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700"
        >
          <RefreshCw className="w-4 h-4" />
          إعادة المحاولة
        </button>
      )}
    </div>
  );
};
```

#### 4.5.4 `src/components/shared/EmptyState.tsx` (جديد)

```tsx
import { Inbox } from 'lucide-react';

interface EmptyStateProps {
  icon?: React.ReactNode;
  title: string;
  description?: string;
  action?: {
    label: string;
    onClick: () => void;
  };
}

export const EmptyState = ({
  icon,
  title,
  description,
  action,
}: EmptyStateProps) => {
  return (
    <div className="flex flex-col items-center justify-center p-12 text-center">
      <div className="mb-4 text-gray-400">
        {icon || <Inbox className="w-16 h-16" />}
      </div>
      <h3 className="text-lg font-semibold mb-2">{title}</h3>
      {description && (
        <p className="text-gray-600 mb-4 max-w-md">{description}</p>
      )}
      {action && (
        <button
          onClick={action.onClick}
          className="px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700"
        >
          {action.label}
        </button>
      )}
    </div>
  );
};
```

---

## 5. خطة التنفيذ

### الأسبوع 1-2: Backend Modifications

| اليوم | المهمة | الملفات |
|---|---|---|
| 1-2 | Instructor Slug | `User.cs`, `ProfileService.cs`, `PublicInstructorController.cs`, Migration |
| 3-4 | Landing Endpoint | `LandingDto.cs`, `PublicService.cs`, `PublicController.cs`, `CourseRepository.cs` |
| 5-6 | Testimonials API | `Testimonial.cs`, `TestimonialDto.cs`, Controllers, Seeder |
| 7-8 | Notification Preferences | `NotificationPreference.cs`, Controller, Migration |
| 9-10 | Contact Form + About/Legal | Controllers, Entities, Rate Limiting |

### الأسبوع 3-4: Frontend Foundation

| اليوم | المهمة | الملفات |
|---|---|---|
| 1-2 | Types + API Interceptor | `types/api.ts`, `lib/api.ts` |
| 3-5 | Services (22 files) | `services/*.ts` |
| 6-7 | SignalR Integration | `lib/signalr.ts`, `hooks/useSignalR.ts` |
| 8-9 | Shared Components | `components/shared/*.tsx` |
| 10 | Hooks (React Query) | `hooks/*.ts` |

### الأسبوع 5-6: Frontend Auth + Public Pages

| اليوم | المهمة | الملفات |
|---|---|---|
| 1-3 | Auth Flow | `AuthPage.tsx`, `AppProvider.tsx`, OAuth |
| 4-5 | Landing Page | `LandingPage.tsx` |
| 6-7 | Course Catalog | `CourseCatalog.tsx` |
| 8-9 | Course Details | `CourseDetails.tsx` |
| 10 | Public Profile | `PublicProfile.tsx` |

### الأسبوع 7-8: Cart + Student Dashboard

| اليوم | المهمة | الملفات |
|---|---|---|
| 1-2 | Cart + Checkout | `CartCheckout.tsx` |
| 3-5 | Student Dashboard | `StudentDashboard.tsx`, `LearningRoom.tsx` |
| 6-7 | Quiz + Certificates | `QuizTaking.tsx`, `ManuscriptCertificate.tsx` |
| 8-9 | Messaging + Wishlist | `MessagingCenter.tsx`, `WishlistRefunds.tsx` |
| 10 | Instructor Apply | `InstructorApply.tsx` |

### الأسبوع 9-10: Instructor + Admin

| اليوم | المهمة | الملفات |
|---|---|---|
| 1-3 | Instructor Dashboard | `InstructorDashboard.tsx`, `CourseBuilder.tsx` |
| 4-5 | Live Sessions | `LiveSession.tsx` |
| 6-8 | Admin Dashboard | `AdminDashboard.tsx`, `AdvancedAnalytics.tsx` |
| 9-10 | Admin Pages | `ReviewsModeration.tsx`, `AnnouncementsCenter.tsx`, `MediaLibrary.tsx`, `SystemActivitySettings.tsx` |

### الأسبوع 11: Testing + Polish

| اليوم | المهمة |
|---|---|
| 1-3 | Integration Testing |
| 4-5 | Bug Fixes |
| 6-7 | Performance Optimization |
| 8-9 | Security Review |
| 10 | Documentation |

---

## 6. Time Estimates واقعية

### Backend Modifications

| المهمة | الوقت المقدر |
|---|---|
| Instructor Slug | 2 أيام |
| Landing Endpoint | 2 أيام |
| Testimonials API | 2 أيام |
| Notification Preferences | 1 يوم |
| Contact Form + About/Legal | 2 أيام |
| Testing & Review | 1 يوم |
| **الإجمالي** | **10 أيام (2 أسابيع)** |

### Frontend Integration

| المرحلة | الوقت المقدر |
|---|---|
| Foundation (Types + Services + Hooks) | 10 أيام |
| Auth + Public Pages | 10 أيام |
| Cart + Student Dashboard | 10 أيام |
| Instructor + Admin | 10 أيام |
| Testing + Polish | 10 أيام |
| **الإجمالي** | **50 يوم (10 أسابيع)** |

### **الإجمالي الكلي: 12 أسبوع (3 أشهر)**

---

## 7. Checklist نهائي

### Backend ✅

- [ ] Instructor Slug مع validation + security
- [ ] Landing Aggregated Endpoint مع caching
- [ ] Testimonials API مع seed data
- [ ] Notification Preferences مع migration
- [ ] Contact Form مع rate limiting
- [ ] About Page API
- [ ] Legal Pages API مع versioning
- [ ] Instructor Search
- [ ] Rate Limiting policies
- [ ] Swagger/OpenAPI docs

### Frontend ✅

- [ ] Types مطابقة للـ Backend DTOs (23 section)
- [ ] 22 Service files
- [ ] Refresh Token logic
- [ ] OAuth flow (Google + Microsoft)
- [ ] SignalR integration (Notifications + Messaging)
- [ ] Presigned URL upload flow
- [ ] Shared components (Pagination, Skeleton, Error, Empty)
- [ ] React Hook Form + Zod لكل الفورمات
- [ ] Error boundaries
- [ ] Loading states

### Integration ✅

- [ ] كل الصفحات مربوطة بالـ Backend
- [ ] Real-time notifications
- [ ] Real-time messaging
- [ ] File uploads (images, videos, documents)
- [ ] Payment integration
- [ ] Email verification
- [ ] Password reset
- [ ] Session management

---

## 📝 ملاحظات نهائية

1. **الأولوية القصوى:** ابدأ بـ Backend أولاً، ثم Frontend
2. **الاختبار:** اختبر كل endpoint قبل الانتقال للتالي
3. **التوثيق:** وثّق كل تغيير في Git commits
4. **النسخ الاحتياطي:** اعمل backup قبل كل تعديل كبير
5. **التواصل:** تواصل مع الفريق باستمرار

---

**آخر تحديث:** 23 يونيو 2026  
**المُعد:** AI Assistant  
**الحالة:** جاهز للتنفيذ