# Implementation Plan: course-features

**Branch**: `[002-course-features]` | **Date**: 2026-04-25 | **Spec**: [specs/002-course-features/spec.md](file:///d:/FinalProject_BackEnd/specs/002-course-features/spec.md)
**Input**: Feature specification from `/specs/002-course-features/spec.md`

## Summary

Implement the "Course Edit Approval Workflow" and "Public Course Service" in the Athary educational platform. This includes extending the existing architecture (without breaking compatibility) to support a robust review process for course modifications based on risk assessment, and exposing public, read-only endpoints for visitors to browse the course catalog securely.

## Technical Context

**Language/Version**: C#, ASP.NET Core 8.0  
**Primary Dependencies**: Entity Framework Core 8.0, SignalR (existing), MailKit/SMTP (existing)  
**Storage**: SQL Server / PostgreSQL (via EF Core)  
**Testing**: xUnit (Unit and Integration tests)  
**Target Platform**: Backend Web API  
**Project Type**: Web Service  
**Performance Goals**: Public course listing API under 500ms on average  
**Constraints**: Do not break existing service signatures; add Overloads. Use `AsNoTracking()` for read queries. Require `[Authorize(Roles = "Admin")]` or `[Authorize(Roles = "Instructor")]` on state-modifying endpoints.  
**Scale/Scope**: Impacts Course entity versions, logs, edit requests, and exposes new unauthenticated query endpoints. Background service needed for cleanup.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Clean Code Foundations**: System extends existing features via Overloads instead of breaking changes. `EditPolicyHelper` extracts risk logic.
- **C# & .NET Best Practices**: Using EF Core migrations, async/await throughout, BackgroundService for scheduled tasks.
- **Naming & Formatting**: Using standard PascalCase for models and controllers, interfaces prefixed with "I".
- **Structure**: N-Tier architecture maintained. `CourseEditApprovalService` and `PublicCourseService` have singular responsibilities.
- **Error Handling**: Using Global Exception Handler (assumed from previous conversations) and returning generic `ApiResponse<T>`.

## Project Structure

### Documentation (this feature)

```text
specs/002-course-features/
├── plan.md              
├── research.md          
├── data-model.md        
├── quickstart.md        
├── contracts/           
└── tasks.md             
```

### Source Code (repository root)

```text
backend/
├── Controllers/
│   ├── AdminCourseController.cs
│   ├── PublicCourseController.cs
│   └── SectionController.cs
├── DTOs/
│   ├── Course/
│   │   ├── PublicCourseDto.cs
│   │   ├── PublicCourseDetailDto.cs
│   │   └── PublicCourseFilterDto.cs
│   └── EditRequest/
│       ├── EditRequestDto.cs
│       └── ...
├── Helpers/
│   └── EditPolicyHelper.cs
├── Models/
│   ├── Course.cs
│   ├── CourseEditRequest.cs
│   └── CourseLog.cs
├── Services/
│   ├── Interfaces/
│   │   ├── ICourseEditApprovalService.cs
│   │   └── IPublicCourseService.cs
│   ├── Background/
│   │   └── EditRequestCleanupService.cs
│   ├── CourseEditApprovalService.cs
│   └── PublicCourseService.cs
└── Tests/
```

**Structure Decision**: The structure integrates neatly into the existing ASP.NET Core MVC-style folder structure (`Controllers`, `Models`, `DTOs`, `Services`, `Helpers`), maintaining consistency with the current backend organization.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | N/A | N/A |
