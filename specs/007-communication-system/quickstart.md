# Quickstart: Communication & System

## Prerequisites

- .NET 9.0 SDK+
- SQL Server instance (or Docker)
- Existing project with migrations applied

## Setup Steps

### 1. Apply Schema Changes

Create a new EF Core migration for the required schema changes:

```bash
dotnet ef migrations add AddCommunicationFeature --context ApplicationDbContext
dotnet ef database update --context ApplicationDbContext
```

**Schema changes needed** (from `data-model.md`):
- `Message.IsDeleted` (bool)
- `Announcement.CourseId` (Guid?, FK → Course)
- `Announcement.Target` enum extended with `SpecificCourse`
- `ActivityLogEntityType` enum extended with `Message`, `Announcement`, `SystemSetting`, `Report`
- `ReportEntityType` enum extended with `Message`
- `ReportReason` enum extended with `Harassment`
- `ReportStatus` enum updated to `Pending`, `Dismissed`, `ActionTaken`
- `Report.AdminNote` (string?)

### 2. Register Services

In `Program.cs` (or `DependencyInjection.cs`), register the new services:

```csharp
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IAnnouncementService, AnnouncementService>();
builder.Services.AddScoped<ISystemSettingService, SystemSettingService>();
builder.Services.AddScoped<IReportService, ReportService>();
```

### 3. Register Controllers

Controllers are auto-discovered in `backend_project/Controllers/`. Ensure `AddControllers()` scans recursively.

### 4. Configure Rate Limiting

In `Program.cs`:

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("Messaging", opt =>
    {
        opt.PermitLimit = 30;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });
});
```

Apply `[EnableRateLimiting("Messaging")]` to `MessagesController`.

### 5. Seed System Settings

Add initial seed data for system settings if desired (e.g., `SiteName`, `RegistrationEnabled`).

## Verification

```bash
dotnet build
dotnet test
```

Run the API and verify:
- `POST /api/messages` — send a message
- `GET /api/messages/conversations` — list conversations
- `GET /api/messages/unread-count` — unread count
- `POST /api/announcements` — create announcement (admin)
- `GET /api/announcements` — feed
- `GET /api/system-settings` — list settings (admin)
- `PUT /api/system-settings/{key}` — update setting (admin)
- `GET /api/activity-logs` — view logs (admin)
- `POST /api/reports` — create report
- `PATCH /api/reports/{id}/resolve` — resolve report (admin)
