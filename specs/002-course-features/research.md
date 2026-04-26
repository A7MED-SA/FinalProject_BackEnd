# Research Findings: course-features

**Date**: 2026-04-25

## Resolved Clarifications

### Background Service Implementation
- **Decision**: Use `Microsoft.Extensions.Hosting.BackgroundService` to implement `EditRequestCleanupService`.
- **Rationale**: Built-in ASP.NET Core construct for background tasks; integrates natively with the DI container to resolve scoped services (like `ICourseEditApprovalService`) via `IServiceProvider.CreateScope()`.
- **Alternatives considered**: Quartz.NET or Hangfire. Rejected because they introduce unnecessary overhead for a single nightly cleanup task.

### Email and Notification Services Integration
- **Decision**: Extend existing `IEmailService` and `INotificationService` using `partial` classes or Overloads.
- **Rationale**: Keeps existing code intact while introducing new behaviors (`SendEditRequestPendingAsync`, etc.), adhering to the Open/Closed Principle.

### JsonPayload Handling
- **Decision**: Store `JsonPayload` as a `string?` in `CourseEditRequest` and use `System.Text.Json.JsonSerializer` to deserialize into strictly typed payload classes (`SectionEditPayload`, etc.) when applying the edits.
- **Rationale**: Flexible enough to handle any type of edit (sections, items, properties) without needing multiple columns or tables, but type-safe during execution.
- **Alternatives considered**: A separate table for changes (e.g. `EditRequestChanges` with PropertyName, OldValue, NewValue). Rejected because reconstructing nested objects (like Section vs SectionItem) would be overly complex compared to a direct payload representation.

### Diff Generation (Admin View)
- **Decision**: Use a helper method `GenerateChangesListAsync` that compares the live database entity with the requested `JsonPayload`.
- **Rationale**: We do not store `OldValue` in the JSON (only NewValue), so comparing against the live data ensures the Admin sees what the exact impact will be *at the time of review*.
