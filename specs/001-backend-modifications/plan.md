# Implementation Plan: Backend Modifications — Athary Platform

**Branch**: `001-backend-modifications` | **Date**: 2026-06-23 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-backend-modifications/spec.md`

## Summary

Implement 7 backend features for the Athary educational platform: instructor slug profiles, aggregated landing endpoint, testimonials management, notification preferences, contact form, about page API, and legal pages API. All features follow the existing Clean Architecture patterns with .NET 9, PostgreSQL, and Entity Framework Core.

## Technical Context

**Language/Version**: C# 12 / .NET 9  
**Primary Dependencies**: ASP.NET Core, Entity Framework Core, Npgsql, IMemoryCache  
**Storage**: PostgreSQL  
**Testing**: xUnit, Moq, FluentAssertions (existing test projects)  
**Target Platform**: Linux server (Docker)  
**Project Type**: Web API (REST)  
**Performance Goals**: Landing endpoint < 2s response, legal pages < 1s with caching  
**Constraints**: Follow existing Clean Architecture, DataAnnotations for validation, ApiResponse wrapper pattern  
**Scale/Scope**: 7 features, ~23 new files, ~9 modified files, 4-5 migrations

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

No constitution file exists. Proceeding with standard best practices.

**Assumed Principles**:
- Clean Architecture layer separation (Domain → Application → Infrastructure → API)
- Repository pattern for data access
- Service layer for business logic
- DTOs for API contracts
- DataAnnotations for validation
- EF Core migrations for schema changes

## Project Structure

### Documentation (this feature)

```text
specs/001-backend-modifications/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
New/
├── src/
│   ├── Athary.Domain/
│   │   └── Entities/
│   │       ├── User.cs (modified - add Slug)
│   │       ├── Testimonial.cs (new)
│   │       ├── NotificationPreference.cs (new)
│   │       ├── ContactMessage.cs (new)
│   │       └── LegalPage.cs (new)
│   ├── Athary.Application/
│   │   ├── DTOs/
│   │   │   ├── Public/
│   │   │   │   ├── LandingDto.cs (new)
│   │   │   │   ├── LandingStatsDto.cs (new)
│   │   │   │   ├── TestimonialDto.cs (new)
│   │   │   │   ├── CreateTestimonialDto.cs (new)
│   │   │   │   ├── UpdateTestimonialDto.cs (new)
│   │   │   │   ├── PublicProfileDto.cs (modified - add Slug)
│   │   │   │   └── LegalPageDto.cs (new)
│   │   │   ├── Notification/
│   │   │   │   ├── NotificationPreferencesDto.cs (new)
│   │   │   │   └── UpdateNotificationPreferencesDto.cs (new)
│   │   │   └── Contact/
│   │   │       └── ContactMessageDto.cs (new)
│   │   ├── Interfaces/
│   │   │   ├── IPublicService.cs (new)
│   │   │   ├── ITestimonialService.cs (new)
│   │   │   ├── INotificationPreferenceService.cs (new)
│   │   │   ├── IContactService.cs (new)
│   │   │   ├── ILegalPageService.cs (new)
│   │   │   └── IProfileService.cs (modified - add slug methods)
│   │   └── Services/
│   │       ├── PublicService.cs (new)
│   │       ├── TestimonialService.cs (new)
│   │       ├── NotificationPreferenceService.cs (new)
│   │       ├── ContactService.cs (new)
│   │       ├── LegalPageService.cs (new)
│   │       └── ProfileService.cs (modified - add slug methods)
│   ├── Athary.Infrastructure/
│   │   ├── Data/
│   │   │   ├── AppDbContext.cs (modified - add DbSets, indexes)
│   │   │   └── Seeders/
│   │   │       └── TestimonialSeeder.cs (new)
│   │   └── Repositories/
│   │       └── CourseRepository.cs (modified - add landing stats)
│   └── Athary.API/
│       ├── Controllers/
│       │   ├── PublicInstructorController.cs (new)
│       │   ├── PublicController.cs (new)
│       │   ├── PublicTestimonialsController.cs (new)
│       │   ├── AdminTestimonialsController.cs (new)
│       │   ├── NotificationPreferencesController.cs (new)
│       │   ├── PublicContactController.cs (new)
│       │   ├── AdminContactController.cs (new)
│       │   ├── PublicAboutController.cs (new)
│       │   └── PublicLegalController.cs (new)
│       └── Program.cs (modified - add rate limiting, DI)
└── tests/
    ├── Athary.API.IntegrationTests/
    ├── Athary.Application.UnitTests/
    ├── Athary.Domain.UnitTests/
    └── Athary.Infrastructure.Tests/
```

**Structure Decision**: Existing Clean Architecture structure maintained. New files added to appropriate layers following established patterns.

## Complexity Tracking

No constitution violations to justify.
