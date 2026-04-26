# Quickstart: course-features

## Purpose
This feature implements the core public course browsing experience (for unauthenticated users and students) alongside a secure edit approval workflow for instructors modifying published content.

## Setup Instructions

1. **Apply Migrations**
   The new `CourseEditRequest` table and the modifications to `Course` and `CourseLog` require an EF Core migration.
   ```bash
   dotnet ef migrations add AddCourseEditApprovalSystem
   dotnet ef database update
   ```

2. **Configure AppSettings**
   Add or verify the following configuration in your `appsettings.json`:
   ```json
   "EditPolicy": {
     "RequireApprovalForPublished": true,
     "SensitivePriceChangePercent": 10,
     "AutoExpirePendingDays": 7,
     "EnableEmergencyBypass": false
   },
   "BackgroundServices": {
     "CleanupExpiredRequests": {
       "Enabled": true,
       "CronExpression": "0 0 * * *"
     }
   }
   ```

3. **Background Services**
   Ensure `builder.Services.AddHostedService<EditRequestCleanupService>();` is registered in `Program.cs`.

## Key Interfaces

- `ICourseEditApprovalService`: The central entry point for submitting, reviewing, and cancelling edit requests.
- `IPublicCourseService`: Provides unauthenticated, read-only queries with `AsNoTracking()` for high-performance public course listings.
- `EditPolicyHelper`: A static utility that evaluates the risk level of requested edits to determine if they can be applied immediately or require admin intervention.

## Dependencies

This feature assumes the presence of:
- `ApplicationDbContext` with `Courses`, `Sections`, `SectionItems`, etc.
- `IActivityLogService` for centralized security/audit logging.
- `INotificationService` (SignalR) for real-time notifications.
- `IEmailService` (SMTP) for email notifications.
