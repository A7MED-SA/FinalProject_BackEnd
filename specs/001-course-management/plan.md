# Implementation Plan: Course Management System

**Branch**: `001-course-management` | **Date**: 2026-04-19 | **Spec**: [spec.md](./spec.md)

## Summary

This feature implements the core Course Management System API. It allows Instructors to create, structure (Sections, Items), and submit courses for Admin approval. It uses FluentValidation for input validation and handles soft deletion to preserve historical data.

## Technical Context

**Language/Version**: C# 12 / .NET 9.0  
**Primary Dependencies**: ASP.NET Core Web API, Entity Framework Core 9.0, FluentValidation
**Storage**: SQL Server  
**Testing**: xUnit, Moq  
**Target Platform**: Web API Server
**Project Type**: Web Service
**Performance Goals**: < 300ms response for retrieving full course structures; Bulk update capable of 100+ items.
**Constraints**: Must isolate business logic from controllers; Ensure transaction atomicity for bulk operations.
**Scale/Scope**: ~56 Entities, 3 User Roles (Student, Instructor, Admin).

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] **I. Clean Code Foundations**: Architecture enforces SOLID and KISS. DTOs map to simple EF Core models.
- [x] **II. C# & .NET Best Practices**: All Service and Controller methods will use `async/await`. Services injected via built-in DI.
- [x] **III. Naming & Formatting**: Standard Microsoft conventions will be strictly followed.
- [x] **IV. Structure**: Distinct layers: Controllers, Services (Business Logic), and Repositories/EF Context.
- [x] **V. Error Handling**: Validation errors will return structured 400 Bad Request responses. Standard exceptions will be caught by global error middleware.

## Project Structure

### Documentation (this feature)

```text
specs/001-course-management/
├── plan.md              # This file
├── research.md          # Technical decisions
├── data-model.md        # Entities & DTOs
└── contracts/           # API Endpoints
```

### Source Code

```text
backend_project/
├── Controllers/
│   ├── CategoryController.cs
│   ├── CourseController.cs
│   ├── CourseManagementController.cs
│   └── SectionController.cs
├── DTOs/
│   ├── Course/
│   ├── Category/
│   └── Section/
├── Services/
│   ├── Interfaces/
│   └── Implementations/
├── Validators/          # FluentValidation rules
└── Data/
    └── ApplicationDbContext.cs
```

**Structure Decision**: Standard ASP.NET Core Web API with dedicated Folders for DTOs, Services, and Validators, keeping business logic out of the Controllers.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| *None*    | N/A        | Adhering strictly to Clean Code constitution. |
