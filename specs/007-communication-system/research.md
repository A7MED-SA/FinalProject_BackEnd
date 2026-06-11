# Research: Communication & System

## Decisions

### Messaging — Conversation Derivation
- **Decision**: Derive conversations from the `Message` table by grouping on the unordered pair (sender, receiver). No separate `Conversation` entity.
- **Rationale**: The existing `Message` model has `SenderId` and `ReceiverId`. A conversation is identified as `MIN(SenderId, ReceiverId)` + `MAX(SenderId, ReceiverId)`, grouping all messages between the same two users in both directions. Simplest approach, no schema change.
- **Alternatives**: Separate `Conversation` table (unnecessary indirection), ordered pair (A→B and B→A would be different conversations, breaking reply flow).

### Messaging — Relationship Validation
- **Decision**: Check enrollment and role before first message in a pair; for replies (existing conversation), skip role check.
- **Rationale**: Clarification confirmed replies always allowed in an existing conversation. First-message validation checks: student→instructor (enrolled course), instructor→student (their course), admin→any. Uses `Enrollment` table + User roles.
- **Alternatives**: Allow all authenticated users to message anyone (too permissive), require instructor→admin approval chain (over-engineered).

### Rate Limiting
- **Decision**: Use ASP.NET Core built-in rate limiting middleware (`AddRateLimiter`) with fixed window policy: 30 requests per minute per authenticated user on messaging endpoints.
- **Rationale**: ASP.NET Core 8+ has built-in rate limiting middleware, no extra NuGet dependency. Fixed window is sufficient for initial version.
- **Alternatives**: Token bucket (more flexible but overkill), custom middleware (duplicated effort).

### Message Content Validation
- **Decision**: Plain text only, max 5000 characters, reject HTML. Validate at both controller (FluentValidation) and service level.
- **Rationale**: Prevents XSS, aligns with initial version scope. FluentValidation integrates cleanly with ASP.NET Core.
- **Alternatives**: Allow markdown (would require rendering), allow HTML with sanitizer (dependency + complexity), no validation (security risk).

### System Settings — Caching
- **Decision**: In-memory cache (`IMemoryCache`) with 60-second sliding expiry. Admins trigger cache invalidation on save.
- **Rationale**: Simplest approach, immediate reflection for admins, stale data tolerated for <60s for non-admins. Matches existing assumption.
- **Alternatives**: Redis (overkill for settings), no cache (DB hit on every request), distributed cache (infrastructure dependency).

### Announcement Lifecycle
- **Decision**: Active/inactive boolean only. Created announcements are active immediately. No draft, no scheduled publishing.
- **Rationale**: Clarification confirmed simple lifecycle for initial version. Matches existing edge case handling.
- **Alternatives**: Draft → Published → Archived (adds complexity), Scheduled publish dates (requires background job).

### Report Duplicate Detection
- **Decision**: Unique composite index on `(ReporterId, EntityType, EntityId)` in the `Reports` table. On conflict, update existing report's `UpdatedAt` and `Description`.
- **Rationale**: Database-level enforcement prevents race conditions. Application layer checks first and calls update. Matches existing FR-026.
- **Alternatives**: Application-level check only (race condition risk), separate dedup table (unnecessary).

### Activity Log Pagination & Filtering
- **Decision**: Server-side pagination with cursor-based or offset-based (max page size 100). Filters: userId, action, entityType, dateFrom, dateTo, ipAddress. Order by timestamp descending.
- **Rationale**: Consistent with FR-019 through FR-023. Offset pagination is sufficient for admin tool with <100k entries.
- **Alternatives**: Cursor-based (better for infinite scroll but admin tool pagination is fine), no pagination (performance risk).

### Soft Delete for Messages
- **Decision**: Use `IsDeleted` boolean on `Message` model (exists or add if missing). Sender sees "message deleted", receiver still sees content.
- **Rationale**: Matches FR-007 requirement. Standard soft-delete pattern already used in other features.
- **Alternatives**: Hard delete (data loss), full audit table (unnecessary for initial version).
