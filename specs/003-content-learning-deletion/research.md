# Research: Content & Learning System with Deletion Lifecycle

**Date**: 2026-05-14

## R-001: Deletion Patterns for Hierarchical LMS Data

**Decision**: Use a dual-strategy approach — **hard delete for draft** (no student impact) and **soft delete for published** (data preservation).

**Rationale**: The existing codebase already uses `DeletedAt` (nullable DateTime) on Course, Category, VideoComment, Review, User, Address, and File models. Extending this pattern to the deletion lifecycle keeps consistency. Draft content has no downstream dependencies (no enrollments, no progress), so hard delete is safe and keeps the database clean. Published content with student data must be soft-deleted to preserve academic integrity.

**Alternatives Considered**:
- **Full soft-delete everywhere**: Rejected because draft content accumulates as dead data with no audit value.
- **Full hard-delete with archive table**: Rejected because it adds schema complexity (mirror tables) without benefit; the existing `DeletedAt` pattern achieves the same goal.
- **Logical delete flag (bool)**: Rejected because `DateTime?` provides both the delete flag AND the timestamp, which is richer for audit queries.

## R-002: Quiz Auto-Submit on Timer Expiry

**Decision**: Implement server-side timer check. When a quiz attempt is fetched or submitted, the API validates `StartedAt + DurationMinutes < UtcNow`. If expired, auto-submit with current answers.

**Rationale**: Client-side timers are unreliable (tab close, network loss). The server-side approach is simpler, deterministic, and doesn't require WebSocket/SignalR for quiz timing. A background service could periodically sweep for expired in-progress attempts, but the primary enforcement happens at the API boundary (on read/submit).

**Alternatives Considered**:
- **Real-time SignalR timer**: Rejected — over-engineered for a quiz timer. Adds complexity to the client and server without meaningful benefit.
- **Background service only**: Rejected as the sole mechanism — could leave a race condition between the sweep interval and a student's submission. Server-side check on API access is primary; background sweep is secondary cleanup.

## R-003: Comment Flattening Strategy

**Decision**: Enforce flattening at the service layer. When creating a comment where `ParentCommentId` references a reply (not a top-level comment), the service looks up the parent's `ParentCommentId` and re-assigns to the top-level comment. The reply text is prepended with `@{username}`.

**Rationale**: This matches YouTube/Udemy threading behavior. Single-level nesting simplifies front-end rendering (only two levels: top-level and replies). The `@mention` preserves conversational context.

**Alternatives Considered**:
- **Unlimited nesting**: Rejected — creates complex rendering and poor UX for mobile views.
- **Block at API**: Rejected — poor UX; the user doesn't know they can't reply to a reply until they try.

## R-004: Progress Calculation Strategy

**Decision**: Use a **count-based percentage** of mandatory section items. `ProgressPercentage = (completed mandatory items / total mandatory items) * 100`. Non-mandatory items don't affect overall progress.

**Rationale**: This is the standard approach used by Udemy, Coursera, and Teachable. It's simple, deterministic, and doesn't require complex weighting logic. The `IsMandatory` flag on `SectionItem` already exists in the model.

**Alternatives Considered**:
- **Weighted by content type** (e.g., quiz = 2x, video = 1x): Rejected for initial implementation — adds configuration complexity. Can be added later without breaking changes.
- **Time-weighted**: Rejected — penalizes short but critical content (like quizzes).

## R-005: Enrollment Access Check Pattern

**Decision**: Create a reusable `EnrollmentGuard` helper that validates enrollment status on every content-access request. This is a synchronous DB check (not cached) using `AsNoTracking()`.

**Rationale**: Content access is the most critical authorization boundary. A stale cache could allow expired students to access content. The DB check is fast (indexed on `UserId + CourseId`) and the performance impact is negligible compared to the content retrieval itself.

**Alternatives Considered**:
- **JWT claim with enrollment info**: Rejected — enrollment status can change (refund, expiration) between token issuance and content access.
- **Redis-cached enrollment**: Rejected for MVP — adds infrastructure dependency. Can be added later for high-traffic scenarios.

## R-006: LiveSession Meeting URL Security

**Decision**: Store the meeting URL and optional password as encrypted fields. The meeting URL is only revealed to enrolled students when the session status is "Live" or within 15 minutes of the scheduled start.

**Rationale**: Prevents link sharing/leaking before the session starts. The instructor provides the meeting URL (from Zoom/Teams/etc.) when creating the session.

**Alternatives Considered**:
- **Generate meeting automatically via Zoom API**: Rejected — out of scope; the platform manages metadata, not meeting infrastructure.
- **No URL restriction**: Rejected — allows unauthorized access to live sessions.
