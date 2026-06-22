# Backend Execution Plan — Athary API

> **النسخة المعدلة** — بناءً على `MODIFICATION_GUIDE.md`
> الخطة التنفيذية الكاملة للتعديلات والإضافات المطلوبة في مشروع Backend (.NET 9 Clean Architecture)

---

## الفهرس

1. [نظرة عامة](#1)
2. [التصحيحات عن الخطة السابقة](#2)
3. [الخطة التنفيذية لكل إضافة](#3)

---

## 1. نظرة عامة <a id="1"></a>

### ✅ التصحيحات عن الخطة السابقة (`BACKEND_PLAN.md`)

| البند | الخطة القديمة | الخطة الجديدة |
|-------|--------------|---------------|
| Seed Data Testimonials | ❌ غير موجود | ✅ `TestimonialSeeder.cs` مع بيانات عربية |
| Rate Limiting للـ Contact | ❌ غير موجود | ✅ Policy "Contact" مع PermitLimit=5/d |
| Spam Detection | ❌ غير موجود | ✅ `ContainsSpamKeywords()` |
| Migration كود صريح | ✅ موجود لكن ناقص | ✅ كود `Up()` كامل لـ NotificationPreferences |
| Testimonial Flag | ❌ غير موجود | ✅ `IsFlagged`, `FlagReason`, `FlagAsync()` |
| LegalPage Versioning | ❌ غير موجود | ✅ `IsPublished`, `Version`, `LastUpdatedById` |
| PublicService Error Handling | ❌ غير موجود | ✅ try/catch مع `GetDefaultLandingData()` |
| Caching في Landing | ✅ موجود | ✅ `IMemoryCache` مضبوط |

---

## 2. التصحيحات الأساسية في الأكواد <a id="2"></a>

### 2.1 Instructor Slug — استخدام FirstName + LastName

في الخطة السابقة استخدمت `fullName`، لكن MODIFICATION_GUIDE يستخدم `FirstName` + `LastName`:

```csharp
// ✅ الصحيح
FullName = $"{u.FirstName} {u.LastName}",
```

### 2.2 Landing — إضافة Cache + Error Fallback + Testimonials

```csharp
public class PublicService : IPublicService
{
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
}
```

### 2.3 LandingStats — حساب Satisfaction من Reviews

```csharp
public async Task<LandingStatsDto> GetLandingStatsAsync()
{
    var satisfactionRate = await _context.Reviews
        .Where(r => r.DeletedAt == null)
        .AverageAsync(r => (double?)r.Rating) ?? 0;

    return new LandingStatsDto
    {
        SatisfactionRate = Math.Round(satisfactionRate / 5.0 * 100, 1),
        // ... باقي الحقول
    };
}
```

### 2.4 Testimonial — إضافة Flag + Seeder

**حقول إضافية في الـ Entity:**
```csharp
public bool IsFlagged { get; set; } = false;
public string? FlagReason { get; set; }
```

**Seeder:**
```csharp
public static class TestimonialSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Testimonials.AnyAsync()) return;

        var testimonials = new List<Testimonial>
        {
            new() { Content = "منصة رائعة، تعلمت الكثير عن التاريخ الإسلامي بطريقة ممتعة.", Rating = 5, IsApproved = true, DisplayOrder = 1 },
            new() { Content = "المدربون محترفون والمحتوى عالي الجودة. أنصح بها بشدة.", Rating = 5, IsApproved = true, DisplayOrder = 2 },
            new() { Content = "تجربة تعليمية فريدة من نوعها. المنصة سهلة الاستخدام والمحتوى ثري.", Rating = 5, IsApproved = true, DisplayOrder = 3 },
        };
        // أضف GUIDs للمستخدمين الحقيقيين
        context.Testimonials.AddRange(testimonials);
        await context.SaveChangesAsync();
    }
}
```

### 2.5 Contact — إضافة Rate Limiting + Spam Detection

**Spam Detection:**
```csharp
private bool ContainsSpamKeywords(string content)
{
    var spamKeywords = new[] { "viagra", "casino", "lottery", "xxx", "free money" };
    return spamKeywords.Any(keyword => content.ToLower().Contains(keyword));
}
```

**Rate Limiting Config في Program.cs:**
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

### 2.6 NotificationPreferences Migration — كود كامل

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

    // Seed for existing users
    migrationBuilder.Sql(@"
        INSERT INTO ""NotificationPreferences"" (""Id"", ""UserId"", ""EmailNotifications"", 
            ""PushNotifications"", ""CourseUpdates"", ""MarketingEmails"", ""NewMessageAlerts"", 
            ""LiveSessionReminders"", ""QuizReminders"", ""CertificateAchievements"", 
            ""AnnouncementAlerts"", ""CreatedAt"")
        SELECT 
            gen_random_uuid(), u.""Id"",
            true, true, true, false, true, true, true, true, true,
            NOW()
        FROM ""Users"" u
        WHERE NOT EXISTS (
            SELECT 1 FROM ""NotificationPreferences"" np WHERE np.""UserId"" = u.""Id""
        ) AND u.""DeletedAt"" IS NULL
    ");
}
```

### 2.7 LegalPage — إضافة Versioning + IsPublished

```csharp
public class LegalPage : BaseEntity
{
    public string Type { get; set; }  // "privacy", "terms", "refund"
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    public string? Version { get; set; }
    public bool IsPublished { get; set; } = true;
    public Guid? LastUpdatedById { get; set; }
    public User? LastUpdatedBy { get; set; }
}
```

---

## 3. الخطة التنفيذية لكل إضافة <a id="3"></a>

### 3.1 Instructor Slug (🔴 أولوية عالية)
**ملفات جديدة**: `PublicInstructorController.cs`
**ملفات معدلة**: `User.cs`, `ProfileService.cs`, `PublicProfileDto.cs`, `AppDbContext.cs`, `IProfileService.cs`
**ملاحظة**: استخدام `FirstName` + `LastName` وليس `fullName`

### 3.2 Landing Aggregated Endpoint (🔴 أولوية عالية)
**ملفات جديدة**: `PublicService.cs`, `IPublicService.cs`, `LandingDto.cs`, `PublicController.cs`
**ملفات معدلة**: `ICourseRepository.cs`, `CourseRepository.cs`
**إضافات**: `IMemoryCache`, `ILogger`, try/catch مع `GetDefaultLandingData()`

### 3.3 Testimonials API (🟡 أولوية متوسطة)
**ملفات جديدة**: `Testimonial.cs`, `TestimonialDto.cs`, `CreateTestimonialDto.cs`, `UpdateTestimonialDto.cs`, `TestimonialsController.cs`, `AdminTestimonialsController.cs`, `TestimonialSeeder.cs`
**إضافات عن الخطة السابقة**: `IsFlagged`, `FlagReason`, `FlagAsync()`, Seeder

### 3.4 Notification Preferences (🟡 أولوية متوسطة)
**ملفات جديدة**: `NotificationPreference.cs`, `NotificationPreferencesDto.cs`, `UpdateNotificationPreferencesDto.cs`, `NotificationPreferencesController.cs`
**إضافات عن الخطة السابقة**: Migration كود `Up()` كامل مع Seed للمستخدمين الحاليين

### 3.5 Contact Form (🟡 أولوية متوسطة)
**ملفات جديدة**: `ContactMessage.cs`, `ContactMessageDto.cs`, `PublicContactController.cs`, `AdminContactController.cs`
**إضافات عن الخطة السابقة**: `ContainsSpamKeywords()`, Rate Limiting policy, إسناد الرسالة إلى مسؤول (`AssignedToId`)

### 3.6 About Page (🟢 أولوية منخفضة)
**ملف جديد**: `PublicAboutController.cs`

### 3.7 Legal Pages (🟢 أولوية منخفضة)
**ملفات جديدة**: `LegalPage.cs`, `LegalPageDto.cs`, `PublicLegalController.cs`
**إضافات عن الخطة السابقة**: `IsPublished`, `Version`, `LastUpdatedById`

### 3.8 Instructor Search (🟢 أولوية منخفضة)
**إضافة**: إلى `PublicInstructorController.cs`

---

## ملخص الملفات النهائي

| # | الإضافة | ملفات جديدة | ملفات معدلة | Migrations |
|---|---------|-------------|-------------|------------|
| 1 | Instructor Slug | 1 | 5 | ✅ |
| 2 | Landing Endpoint | 4 | 2 | ❌ |
| 3 | Testimonials | 6 | 1 | ✅ |
| 4 | Notification Preferences | 4 | 0 | ✅ (مع Seed) |
| 5 | Contact Form | 4 | 1 (Program.cs) | ✅ |
| 6 | About Page | 1 | 0 | ❌ |
| 7 | Legal Pages | 3 | 0 | ✅ |
| **إجمالي** | | **23 ملف جديد** | **9 ملفات معدلة** | **4-5 Migrations** |

---

## 4. إضافات بنيوية وتحسينات <a id="4"></a>

### 4.1 🔴 DI Registration — تسجيل الـ Services الجديدة في Program.cs

**الملف**: `Athary.API/Program.cs`

كل Service جديد يحتاج تسجيل في `Program.cs` باستخدام `AddScoped()` أو `AddTransient()`:

```csharp
// === Services ===
builder.Services.AddScoped<IPublicService, PublicService>();
builder.Services.AddScoped<ITestimonialService, TestimonialService>();
builder.Services.AddScoped<INotificationPreferenceService, NotificationPreferenceService>();
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddScoped<ILegalPageService, LegalPageService>();
builder.Services.AddScoped<IProfileService, ProfileService>();  // ← إن لم يكن مسجلاً

// === Repositories (إن وجدت جديدة) ===
builder.Services.AddScoped<ITestimonialRepository, TestimonialRepository>();
builder.Services.AddScoped<ILiveSessionRepository, LiveSessionRepository>();
```

**تنبيه**: قد يكون الـ Backend يستخدم Scrutor أو Assembly Scanning للتسجيل التلقائي — تأكد قبل إضافة أي سطر.

### 4.2 🟡 Exception Handling — GenerateSlugAsync

**المشكلة**: إذا كان `fullName` فارغاً أو يحتوي رموزاً غير قابلة للتحويل (مثلاً `@@@`)، الدالة ترجع `""` مما يسبب Unique Constraint Violation أو NULL.

**الحل المقترح** — إضافة validate قبل الـ replace:

```csharp
public async Task<string> GenerateSlugAsync(string fullName)
{
    if (string.IsNullOrWhiteSpace(fullName))
        throw new ArgumentException("Full name cannot be empty", nameof(fullName));

    var baseSlug = fullName.Trim().ToLower()
        .Replace(" ", "-")
        .Replace(".", "")
        .Replace("'", "")
        .Replace("(", "")
        .Replace(")", "");

    baseSlug = Regex.Replace(baseSlug, @"[^a-z0-9-\u0600-\u06FF]", "");  // ← دعم الأحرف العربية
    
    // لو بعد التنظيف slug فاضي → استخدم GUID مختصر
    if (string.IsNullOrWhiteSpace(baseSlug) || baseSlug.Length < 3)
        baseSlug = $"user-{Guid.NewGuid().ToString()[..8]}";

    var slug = baseSlug;
    var counter = 1;
    
    while (!await IsSlugAvailableAsync(slug))
    {
        slug = $"{baseSlug}-{counter}";
        counter++;
    }
    
    return slug;
}
```

### 4.3 🟡 Validation Layer — FluentValidation للـ DTOs الجديدة

المشروع الحالي يستخدم **DataAnnotations** (`[Required]`, `[StringLength]`). للحفاظ على الاتساق:

```csharp
// ✅ استمرار باستخدام DataAnnotations (نفس أسلوب المشروع)
public class CreateTestimonialDto
{
    [Required]
    [StringLength(1000, MinimumLength = 10)]
    public string Content { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }
}
```

إذا أردت الترقية لاحقاً لـ FluentValidation:
```bash
dotnet add package FluentValidation.AspNetCore
```

ثم:
```csharp
public class CreateTestimonialValidator : AbstractValidator<CreateTestimonialDto>
{
    public CreateTestimonialValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("محتوى التوصية مطلوب")
            .MinimumLength(10).WithMessage("يجب أن يكون المحتوى 10 أحرف على الأقل")
            .MaximumLength(1000);
        RuleFor(x => x.Rating).InclusiveBetween(1, 5);
    }
}
```

### 4.4 🟡 Background Jobs — إرسال إيميلات Contact

**المشكلة**: عند إرسال رسالة تواصل، نحتاج إشعار المسؤولين دون إبطاء الـ Response.

**الحل**: إرسال الحدث إلى Background Queue (Hangfire/Quartz أو `IHostedService` و `Channel<T>`):

```csharp
// ✅ حل خفيف باستخدام Channel<T> (بدون إضافات خارجية)
public interface IBackgroundTaskQueue
{
    ValueTask QueueAsync(Func<CancellationToken, ValueTask> workItem);
    ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(CancellationToken ct);
}

public class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<Func<CancellationToken, ValueTask>> _queue = 
        Channel.CreateBounded<Func<CancellationToken, ValueTask>>(100);

    public async ValueTask QueueAsync(Func<CancellationToken, ValueTask> workItem) =>
        await _queue.Writer.WriteAsync(workItem);

    public async ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(CancellationToken ct) =>
        await _queue.Reader.ReadAsync(ct);
}

// التسجيل في Program.cs
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddHostedService<EmailBackgroundService>();
```

للاستخدام في ContactController:
```csharp
[HttpPost]
[AllowAnonymous]
public async Task<IActionResult> Send([FromBody] ContactMessageDto dto)
{
    // 1. حفظ الرسالة
    var message = await _contactService.CreateAsync(dto);
    
    // 2. إرسال إيميل في الخلفية
    await _queue.QueueAsync(async ct =>
    {
        await _emailService.SendEmailAsync(
            to: "admin@athary.com",
            subject: $"رسالة جديدة من {dto.FullName}",
            body: $"<h3>{dto.Subject}</h3><p>{dto.Message}</p>"
        );
    });
    
    return Ok(ApiResponse<null>.Success(null, "Message sent successfully"));
}
```

### 4.5 🟢 Unit Tests — إضافة Tests/ Layer

**بنية مقترحة للمجلدات**:
```
Athary.sln
├── Athary.API/                    # المشروع الحالي
├── Athary.Application/            # المشروع الحالي
├── Athary.Domain/                 # المشروع الحالي
├── Athary.Infrastructure/         # المشروع الحالي
└── tests/
    ├── Athary.API.Tests/          # Integration Tests للـ Controllers
    │   ├── Controllers/
    │   │   ├── PublicControllerTests.cs
    │   │   ├── TestimonialsControllerTests.cs
    │   │   └── ContactControllerTests.cs
    │   └── Usings.cs
    ├── Athary.Application.Tests/  # Unit Tests للـ Services
    │   ├── Services/
    │   │   ├── ProfileServiceTests.cs
    │   │   ├── PublicServiceTests.cs
    │   │   └── TestimonialServiceTests.cs
    │   └── Usings.cs
    └── Directory.Build.props      # مشترك (xUnit + FluentAssertions + Moq)
```

**أمثلة:**
```csharp
// Athary.Application.Tests/Services/PublicServiceTests.cs
public class PublicServiceTests
{
    [Fact]
    public async Task GetLandingDataAsync_WhenCacheMiss_FetchesFromDb()
    {
        // Arrange
        var cache = new Mock<IMemoryCache>();
        var logger = new Mock<ILogger<PublicService>>();
        var courseRepo = new Mock<ICourseRepository>();
        courseRepo.Setup(r => r.GetLandingStatsAsync())
            .ReturnsAsync(new LandingStatsDto { TotalCourses = 10 });
        
        var service = new PublicService(courseRepo.Object, ..., cache.Object, logger.Object);

        // Act
        var result = await service.GetLandingDataAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.Stats.TotalCourses);
    }

    [Fact]
    public async Task GetLandingDataAsync_WhenDbFails_ReturnsDefaultData()
    {
        // Arrange
        var courseRepo = new Mock<ICourseRepository>();
        courseRepo.Setup(r => r.GetLandingStatsAsync())
            .ThrowsAsync(new Exception("DB down"));
        
        var service = new PublicService(..., cache.Object, logger.Object);

        // Act
        var result = await service.GetLandingDataAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.Stats.TotalCourses);  // fallback
    }
}
```

**الأولويات للاختبارات:**
1. `GenerateSlugAsync` — السلوكيات الحدية (فارغ، رموز خاصة، تكرار)
2. `CreateTestimonialAsync` — validation + save
3. `GetLandingDataAsync` — cache hit/miss, DB failure
4. Contact spam detection
5. Instructor CRUD — soft delete, restore

### 4.6 🟢 Swagger/OpenAPI — توثيق الـ endpoints الجديدة

المشروع الحالي يحتوي بالفعل على Swagger. تأكد من إضافة `[ProducesResponseType]` لكل Controller جديد:

```csharp
[ApiController]
[Route("api/public/testimonials")]
public class PublicTestimonialsController : ControllerBase
{
    /// <summary>
    /// الحصول على قائمة التوصيات المعتمدة
    /// </summary>
    /// <param name="page">رقم الصفحة</param>
    /// <param name="pageSize">عدد العناصر في الصفحة</param>
    /// <param name="minRating">أقل تقييم (1-5)</param>
    /// <response code="200">قائمة التوصيات</response>
    [HttpGet]
    [AllowAnonymous]
    [ResponseCache(Duration = 600)]
    [ProducesResponseType(typeof(ApiResponse<List<TestimonialDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? minRating = null)
    {
        var testimonials = await _testimonialService.GetApprovedAsync(page, pageSize, minRating);
        return Ok(ApiResponse<List<TestimonialDto>>.Success(testimonials));
    }
}
```

### 4.7 🟢 Indexing Strategy — تحسين أداء البحث

**إضافة Indexes في `AppDbContext.cs`** للـ fields المستخدمة في البحث:

```csharp
// === Indexes للـ Landing Page ===
entity.HasIndex(e => e.IsFeatured)
    .HasFilter("[IsFeatured] = 1 AND [Status] = 'Published' AND [DeletedAt] IS NULL");

// === Indexes للـ Public Instructor ===
entity.HasIndex(e => e.Slug).IsUnique().HasFilter("[Slug] IS NOT NULL");
entity.HasIndex(e => new { e.Role, e.DeletedAt });  // فلترة Instructors النشطين

// === Indexes للـ Testimonials ===
entity.HasIndex(e => new { e.IsApproved, e.DisplayOrder });

// === Indexes للـ Contact Messages ===
entity.HasIndex(e => e.IsRead).HasFilter("[IsRead] = 0");  // unread only

// === Indexes للـ Courses ===
entity.HasIndex(e => new { e.Status, e.DeletedAt, e.PublishedAt });
entity.HasIndex(e => e.CategoryId);
entity.HasIndex(e => e.CreatedByUserId);  // Instructor ID
```

**إنشاء migration:**
```bash
dotnet ef migrations add AddPerformanceIndexes
```

### 4.8 🟢 Transaction Scope — للـ operations المعقدة

**متى نستخدم Transaction**:
- `ProcessInstructorRequest` (تحديث الطلب + تغيير Role المستخدم + إرسال إيميل)
- `SubmitQuizAttempt` (تسجيل الإجابات + حساب النتيجة + تحديث Enrollment progress)
- `CreateOrder` (إنشاء الطلب + تقليل رصيد الكوبون + إنشاء Enrollments)

**مثال للاستخدام**:
```csharp
public async Task<InstructorRequestDto> ProcessAsync(Guid requestId, ProcessRequestDto dto)
{
    using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
        var request = await _context.InstructorRequests
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == requestId);
        
        if (request is null)
            throw new NotFoundException("Request not found");

        request.Status = dto.Status;
        request.AdminNotes = dto.AdminNotes;
        request.ProcessedAt = DateTime.UtcNow;

        if (dto.Status == InstructorRequestStatus.Approved)
        {
            request.User.Role = UserRole.Instructor;
            request.User.Slug = await _profileService.GenerateSlugAsync(
                $"{request.User.FirstName} {request.User.LastName}");
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        
        return MapToDto(request);
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```

---

## ملخص الإضافات البنيوية

| # | الإضافة | الأولوية | الملفات المتأثرة | الحالة |
|---|---------|---------|-----------------|--------|
| 1 | DI Registration | 🔴 عالية | `Program.cs` | يجب الإضافة |
| 2 | Exception Handling (GenerateSlugAsync) | 🟡 متوسطة | `ProfileService.cs` | ضروري |
| 3 | FluentValidation / DataAnnotations | 🟡 متوسطة | DTOs files | موجود بالفعل (DA) |
| 4 | Background Jobs (Email) | 🟡 متوسطة | `BackgroundTaskQueue.cs` + `EmailBackgroundService.cs` | اختياري |
| 5 | Unit Tests Layer | 🟢 منخفضة | `tests/` مجلد جديد | يستحب |
| 6 | Swagger/OpenAPI Documentation | 🟢 منخفضة | Controllers (attributes) | موجود جزئياً |
| 7 | Indexing Strategy | 🟢 منخفضة | `AppDbContext.cs` | يستحب |
| 8 | Transaction Scope | 🟢 منخفضة | Services | للمعاملات المعقدة |
