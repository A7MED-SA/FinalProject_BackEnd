# Implementation Plan: Analytics Dashboard

**Branch**: `006-analytics-dashboard` | **Date**: 2026-05-24 | **Spec**: `specs/006-analytics-dashboard/spec.md`
**Input**: Feature specification from `/specs/006-analytics-dashboard/spec.md`

## Summary

Add role-based dashboard API endpoints (Student, Teacher, Admin) that aggregate data from existing models (Enrollment, ContentProgress, Course, Order, Payment, Refund) to provide real-time learning stats, teaching analytics, and platform-wide insights. No new data models are needed — all metrics are computed via aggregation queries.

## Technical Context

**Language/Version**: C# 12 / .NET 9.0  
**Primary Dependencies**: ASP.NET Core Web API, EF Core 9.0, FluentValidation  
**Storage**: SQL Server (via EF Core 9.0)  
**Testing**: xUnit + Moq + FluentAssertions  
**Target Platform**: Linux server (web API)  
**Project Type**: ASP.NET Core Web API  
**Performance Goals**: Student dashboard <2s (50 enrollments), Teacher <3s (20 courses), Admin <5s (100k users)  
**Constraints**: Reuse existing auth (JWT + Identity), existing Enrollment/Course/Order/Refund models, centralized ExceptionMiddleware, existing ActivityLog and Notification services  
**Scale/Scope**: Platform with up to 100,000 users, 50 enrollments per student, 20 courses per teacher

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Clean Code Foundations (SOLID, DRY, KISS) | ✅ PASS | Single Responsibility per service (Student/Teacher/Admin dashboard services); no over-engineering |
| II. C# & .NET Best Practices (DI, async/await) | ✅ PASS | Built-in DI, async all the way, no blocking calls |
| III. Naming & Formatting (C# conventions) | ✅ PASS | PascalCase for public, I-prefix for interfaces, camelCase locals |
| IV. Structure (small classes, single responsibility) | ✅ PASS | Separate services per role (StudentDashboardService, InstructorDashboardService, AdminDashboardService) |
| V. Error Handling (centralized, no silent catches) | ✅ PASS | Reuse existing ExceptionMiddleware; service methods throw typed exceptions |

**All gates pass** — no complexity violations expected.

## Project Structure

### Documentation (this feature)

```text
specs/006-analytics-dashboard/
├── plan.md              # This file (/speckit.plan command output)
├── spec.md              # Feature specification
├── research.md          # Phase 0 — technology decisions
├── data-model.md        # Phase 1 — DTO definitions
├── quickstart.md        # Phase 1 — integration notes
├── contracts/           # Phase 1 — API contracts
└── tasks.md             # Phase 2 (/speckit.tasks output)
```

### Source Code (existing project structure)

```text
backend_project/
├── Controllers/
│   ├── DashboardController.cs          # Student + Teacher dashboard endpoints
│   ├── AdminDashboardController.cs     # Admin dashboard endpoints
│   └── ManagementCoursesController.cs  # Instructor course management
├── Services/
│   ├── Interfaces/
│   │   ├── IStudentDashboardService.cs
│   │   ├── IInstructorDashboardService.cs
│   │   └── IAdminDashboardService.cs
│   └── Implementations/
│       ├── StudentDashboardService.cs
│       ├── InstructorDashboardService.cs
│       └── AdminDashboardService.cs
├── DTOs/
│   └── Dashboard/
│       ├── StudentDashboardDtos.cs
│       ├── InstructorDashboardDtos.cs
│       └── AdminDashboardDtos.cs
├── Data/
│   └── ApplicationDbContext.cs         # No changes needed (reuses existing models)
└── Program.cs                          # (+ DI registrations)

tests/CommerceTests/
├── StudentDashboardServiceTests.cs
├── InstructorDashboardServiceTests.cs
└── AdminDashboardServiceTests.cs
```

**Structure Decision**: Follow existing project conventions (Controllers/Services/Interfaces/Implementations/DTOs). All dashboard DTOs live in one DTOs/Dashboard/ folder. Controllers are role-separated for clarity.

## Complexity Tracking

No constitution violations expected — complexity tracking not needed.
