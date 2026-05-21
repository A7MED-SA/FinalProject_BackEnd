# Implementation Plan: Content & Learning System with Deletion Lifecycle

**Branch**: `003-content-learning-deletion` | **Date**: 2026-05-14 | **Spec**: [spec.md](file:///media/ahmedelewaa/01D8D2F130BAA800/Projects/New%20folder%20(2)/FinalProject_BackEnd/specs/003-content-learning-deletion/spec.md)
**Input**: Feature specification from `specs/003-content-learning-deletion/spec.md`

## Summary

Implement the complete Content & Learning module (13 entities: Video, Document, VideoComment, CommentLike, Quiz, Question, Option, QuizAttempt, UserAnswer, LiveSession, LiveAttendance, Enrollment, ContentProgress) and a hierarchical deletion lifecycle for the Athary LMS platform. This builds on the existing 56-model codebase, adding service logic, DTOs, controllers, and business rules for content management, quiz assessment, live session tracking, enrollment/progress management, video commenting, and entity deletion with safety guards.

## Technical Context

**Language/Version**: C# 13, ASP.NET Core 9.0, .NET 9.0  
**Primary Dependencies**: Entity Framework Core 9.0, FluentValidation, SignalR (existing), MassTransit.NewId (existing), System.Text.Json  
**Storage**: SQL Server via EF Core (Code-First migrations)  
**Testing**: Manual API testing via Swagger/OpenAPI (existing setup)  
**Target Platform**: Backend Web API (Linux server)  
**Project Type**: Web Service (N-Tier Architecture)  
**Performance Goals**: Content retrieval < 500ms, quiz grading < 2s, enrollment count accuracy < 1s  
**Constraints**: Use `AsNoTracking()` for read queries. Require `[Authorize]` on all state-modifying endpoints. Extend existing `ApiResponse<T>` envelope. Follow existing service-layer-throws + global-exception-handler pattern.  
**Scale/Scope**: 13 new service interfaces + implementations, ~20 new DTO classes, 9 new controllers, 2 existing controller updates, 1 model update (LiveSession), 1 EF migration

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- ✅ **I. Clean Code Foundations**: Services follow SRP — each content type has its own service. No over-engineering; reuses existing patterns (edit approval, soft delete). DRY via shared `EnrollmentGuard` helper.
- ✅ **II. C# & .NET Best Practices**: Full async/await throughout. Uses built-in DI. No `.Result` or `.Wait()` calls. EF Core migrations for schema changes.
- ✅ **III. Naming & Formatting**: PascalCase for classes/methods. Interfaces prefixed with "I". camelCase for locals. Consistent with existing codebase.
- ✅ **IV. Structure**: N-tier maintained: Controllers → Services → DbContext. Each service class has single responsibility. Business logic isolated from framework details.
- ✅ **V. Error Handling**: Throws domain exceptions from services (`KeyNotFoundException`, `InvalidOperationException`, `UnauthorizedAccessException`). Global exception middleware maps to HTTP status codes. No empty catch blocks.

**Post-Design Re-check**: ✅ All gates pass. No new complexity introduced beyond standard CRUD + business rules.

## Project Structure

### Documentation (this feature)

```text
specs/003-content-learning-deletion/
├── plan.md              # This file
├── spec.md              # Feature specification
├── research.md          # Phase 0 research decisions
├── data-model.md        # Phase 1 entity validation
├── quickstart.md        # Implementation guide
├── contracts/
│   └── api-contracts.md # API endpoint contracts
├── checklists/
│   └── requirements.md  # Quality checklist
└── tasks.md             # Phase 2 task breakdown (via /speckit-tasks)
```

### Source Code (repository root)

```text
backend_project/
├── Controllers/
│   ├── CategoryController.cs            # UPDATE: Add deletion safety guards
│   ├── CourseManagementController.cs     # UPDATE: Add delete endpoint
│   ├── SectionController.cs             # UPDATE: Enhance delete with soft-delete
│   ├── EnrollmentController.cs          # NEW
│   ├── VideoContentController.cs        # NEW
│   ├── DocumentController.cs            # NEW
│   ├── QuizManagementController.cs      # NEW
│   ├── QuizAttemptController.cs         # NEW
│   ├── LiveSessionController.cs         # NEW
│   ├── LiveAttendanceController.cs      # NEW
│   └── VideoCommentController.cs        # NEW
├── DTOs/
│   ├── Enrollment/                      # NEW (3 DTOs)
│   ├── ContentProgress/                 # NEW (2 DTOs)
│   ├── Video/                           # NEW (3 DTOs)
│   ├── Document/                        # NEW (3 DTOs)
│   ├── Quiz/                            # NEW (6 DTOs)
│   ├── QuizAttempt/                     # NEW (4 DTOs)
│   ├── LiveSession/                     # NEW (3 DTOs)
│   └── VideoComment/                    # NEW (3 DTOs)
├── Helpers/
│   ├── EditPolicyHelper.cs              # EXISTING (reused for deletion workflow)
│   └── EnrollmentGuard.cs               # NEW (access check helper)
├── Models/
│   └── LiveSession.cs                   # UPDATE: Add MeetingUrl, Password, MaxAttendees, ActualStartAt, ActualEndAt
├── Services/
│   ├── Interfaces/
│   │   ├── IEnrollmentService.cs        # NEW
│   │   ├── IContentProgressService.cs   # NEW
│   │   ├── IVideoContentService.cs      # NEW
│   │   ├── IDocumentService.cs          # NEW
│   │   ├── IQuizManagementService.cs    # NEW
│   │   ├── IQuizAttemptService.cs       # NEW
│   │   ├── ILiveSessionService.cs       # NEW
│   │   ├── ILiveAttendanceService.cs    # NEW
│   │   └── IVideoCommentService.cs      # NEW
│   └── Implementations/
│       ├── CategoryService.cs           # UPDATE: Add deletion safety guards
│       ├── CourseService.cs             # UPDATE: Add DeleteCourseAsync
│       ├── SectionService.cs            # UPDATE: Soft-delete for published
│       ├── EnrollmentService.cs         # NEW
│       ├── ContentProgressService.cs    # NEW
│       ├── VideoContentService.cs       # NEW
│       ├── DocumentService.cs           # NEW
│       ├── QuizManagementService.cs     # NEW
│       ├── QuizAttemptService.cs        # NEW
│       ├── LiveSessionService.cs        # NEW
│       ├── LiveAttendanceService.cs     # NEW
│       └── VideoCommentService.cs       # NEW
├── Validators/                          # NEW directory
│   ├── CreateEnrollmentValidator.cs     # NEW
│   ├── CreateVideoValidator.cs          # NEW
│   ├── CreateQuizValidator.cs           # NEW
│   ├── CreateQuestionValidator.cs       # NEW
│   ├── SubmitQuizAttemptValidator.cs    # NEW
│   ├── CreateLiveSessionValidator.cs    # NEW
│   └── CreateCommentValidator.cs        # NEW
└── Program.cs                           # UPDATE: Register new services
```

**Structure Decision**: All new files follow the existing N-tier folder convention (Controllers, DTOs, Models, Services, Helpers). No new structural patterns introduced. The `Validators/` directory is new but aligns with the existing FluentValidation registration in Program.cs.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | N/A | N/A |
