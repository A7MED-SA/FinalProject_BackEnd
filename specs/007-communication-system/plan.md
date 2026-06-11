# Implementation Plan: Communication & System

**Branch**: `007-007-communication-system` | **Date**: 2026-05-25 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/007-communication-system/spec.md`

## Summary

Implement Communication & System module for the Athary E-Learning platform: internal messaging (P1), announcements (P1), system settings (P2), activity log viewer (P2), and content reporting (P3). The module reuses existing models (`Message`, `Announcement`, `SystemSetting`, `ActivityLog`, `Report`, `Notification`) and existing services (`INotificationService`, `IActivityLogService`). Authentication (JWT + Identity) is reused without changes. No WebSocket or email notifications in the initial version.

## Technical Context

**Language/Version**: C# 12 / .NET 9.0 (or 10.0)  
**Primary Dependencies**: ASP.NET Core Web API, Entity Framework Core, SQL Server, FluentValidation  
**Storage**: SQL Server via EF Core — existing `ApplicationDbContext` with DbSets for all communication entities  
**Testing**: xUnit, Moq, FluentAssertions  
**Target Platform**: Linux server (REST API), JWT-authenticated clients  
**Project Type**: Web API (backend only, ASP.NET Core)  
**Performance Goals**: Sub-2s message delivery, sub-5s activity log queries at 100k+ entries, sub-5s announcement feed  
**Constraints**: Rate limiting at 30 msg/min/user, max 5000 chars per message (plain text only), pagination with 100 max page size  
**Scale/Scope**: 10k+ users, indefinite activity log growth (external archiving)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Gate | Check | Status |
|------|-------|--------|
| I. Clean Code | SOLID, DRY, KISS followed; no over-engineering | ✅ PASS |
| II. C# Best Practices | Built-in DI, async/await, no blocking calls | ✅ PASS |
| III. Naming | PascalCase, I-prefix interfaces, camelCase locals | ✅ PASS |
| IV. Structure | Small focused classes, layered architecture | ✅ PASS |
| V. Error Handling | Centralized global exception handler, structured logging | ✅ PASS |

**No violations. Complexity is proportional to the 5 user stories; each maps to one service and one controller, consistent with the existing Dashboard (006) pattern.**

**Post-Phase 1 Re-evaluation**: All gates still pass. Data model extensions (enum values + columns) are minimal and justified by spec requirements. No new architectural patterns introduced.

## Project Structure

### Documentation (this feature)

```text
specs/007-communication-system/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
backend_project/
├── Controllers/
│   ├── Communication/
│   │   ├── MessagesController.cs       # Messaging endpoints (US1)
│   │   ├── AnnouncementsController.cs  # Announcement endpoints (US2)
│   │   ├── SystemSettingsController.cs # Settings CRUD (US3)
│   │   ├── ActivityLogsController.cs   # Activity log viewer (US4)
│   │   └── ReportsController.cs        # Content reporting (US5)
│   └── Notifications/
│       └── NotificationController.cs   # Existing — reused/extended
├── Services/
│   ├── Interfaces/
│   │   ├── IMessageService.cs
│   │   ├── IAnnouncementService.cs
│   │   ├── ISystemSettingService.cs
│   │   ├── IActivityLogService.cs      # Existing — reused
│   │   └── IReportService.cs
│   └── Implementations/
│       ├── MessageService.cs
│       ├── AnnouncementService.cs
│       ├── SystemSettingService.cs
│       ├── ActivityLogService.cs       # Existing — reused
│       └── ReportService.cs
├── Models/                              # Existing models — no new entities
│   ├── Message.cs
│   ├── Announcement.cs
│   ├── SystemSetting.cs
│   ├── ActivityLog.cs
│   ├── Report.cs
│   └── Notification.cs
├── Data/
│   └── ApplicationDbContext.cs          # Existing — DbSets already registered
└── DTOs/
    └── Communication/
        ├── MessageDtos.cs
        ├── AnnouncementDtos.cs
        ├── SystemSettingDtos.cs
        ├── ActivityLogDtos.cs
        └── ReportDtos.cs

backend_project.Tests/
├── Controllers/
│   └── Communication/
│       ├── MessagesControllerTests.cs
│       ├── AnnouncementsControllerTests.cs
│       ├── SystemSettingsControllerTests.cs
│       ├── ActivityLogsControllerTests.cs
│       └── ReportsControllerTests.cs
└── Services/
    └── Communication/
        ├── MessageServiceTests.cs
        ├── AnnouncementServiceTests.cs
        ├── SystemSettingServiceTests.cs
        └── ReportServiceTests.cs
```

**Structure Decision**: Web API backend (ASP.NET Core) following the existing pattern from Dashboard (006) — one controller and one service per feature area, matching `backend_project/Controllers/` and `backend_project/Services/` layout.

## Complexity Tracking

N/A — no constitution violations.
