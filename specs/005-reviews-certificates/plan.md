# Implementation Plan: Reviews & Certificates

**Branch**: `005-reviews-certificates` | **Date**: 2026-05-23 | **Spec**: `specs/005-reviews-certificates/spec.md`
**Input**: Feature specification from `/specs/005-reviews-certificates/spec.md`

## Summary

Add course reviews (ratings + written feedback) and completion certificates (auto-generated on course completion, public verification via unique code, PDF download) to the Athary E-Learning Platform. Both features build on existing enrollment/completion tracking and authentication systems.

## Technical Context

**Language/Version**: C# 12 / .NET 9.0  
**Primary Dependencies**: ASP.NET Core Web API, EF Core 9.0, FluentValidation, QuestPDF, xUnit + Moq + FluentAssertions  
**Storage**: SQL Server (via EF Core 9.0)  
**Testing**: xUnit + Moq + FluentAssertions  
**Target Platform**: Linux server (web API)  
**Project Type**: ASP.NET Core Web API  
**Performance Goals**: Certificate verification <1s, PDF generation <5s  
**Constraints**: Reuse existing auth (JWT + Identity), enrollment system, global ExceptionMiddleware, SignalR for notifications  
**Scale/Scope**: Part of existing Athary E-Learning Platform — reviews + certificates feature

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Clean Code Foundations (SOLID, DRY, KISS) | ✅ PASS | Services will follow Single Responsibility; no over-engineering |
| II. C# & .NET Best Practices (DI, async/await) | ✅ PASS | Built-in DI, async all the way, no blocking calls |
| III. Naming & Formatting (C# conventions) | ✅ PASS | PascalCase for public, I-prefix for interfaces, camelCase locals |
| IV. Structure (small classes, single responsibility) | ✅ PASS | Separate ReviewService, CertificateService per SRP |
| V. Error Handling (centralized, no silent catches) | ✅ PASS | Reuse existing ExceptionMiddleware; service methods throw typed exceptions |

**All gates pass** — no complexity violations expected.

## Project Structure

### Documentation (this feature)

```text
specs/005-reviews-certificates/
├── plan.md              # This file
├── spec.md              # Feature specification
├── research.md          # Phase 0 — technology decisions
├── data-model.md        # Phase 1 — entity definitions
├── quickstart.md        # Phase 1 — integration notes
├── contracts/           # Phase 1 — API contracts
└── tasks.md             # Phase 2 (/speckit.tasks output)
```

### Source Code (existing project structure)

```text
backend_project/
├── Controllers/
│   ├── ReviewsController.cs
│   ├── CertificatesController.cs
│   └── AdminCertificatesController.cs
├── Services/
│   ├── Interfaces/
│   │   ├── IReviewService.cs
│   │   └── ICertificateService.cs
│   └── Implementations/
│       ├── ReviewService.cs
│       └── CertificateService.cs
├── DTOs/
│   ├── Review/
│   │   └── ReviewDtos.cs
│   └── Certificate/
│       └── CertificateDtos.cs
├── Validators/
│   ├── Review/
│   │   └── CreateReviewValidator.cs
│   └── Certificate/
│       └── (optional minimal)
├── Models/
│   ├── Review.cs
│   └── Certificate.cs
├── Data/
│   └── ApplicationDbContext.cs  # (+ new DbSets + config)
└── Program.cs                   # (+ DI registrations)

tests/CommerceTests/
├── ReviewServiceTests.cs
├── CertificateServiceTests.cs
└── CertificateVerificationTests.cs
```

**Structure Decision**: Follow existing project conventions (Controllers/Services/Interfaces/Implementations/DTOs/Validators/Models). New models go in `backend_project/Models/`, new services in `Services/Implementations/`, and tests in `tests/CommerceTests/`.

## Complexity Tracking

No constitution violations expected — complexity tracking not needed.
