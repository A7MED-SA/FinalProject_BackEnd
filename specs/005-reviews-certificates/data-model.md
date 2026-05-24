# Data Model: Reviews & Certificates

## Entity: Review

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | Guid (sequential) | PK, non-null | Generated via `NewId.NextSequentialGuid()` |
| UserId | Guid | FK → Users, non-null | The student who wrote the review |
| CourseId | Guid | FK → Courses, non-null | The course being reviewed |
| Rating | int | 1–5, non-null | Star rating |
| Content | string? | Max 2000 chars | Optional written review text |
| IsFlagged | bool | Default false | Set when instructor flags review |
| FlaggedBy | Guid? | FK → Users, nullable | Instructor who flagged |
| FlaggedAt | DateTime? | Nullable | When flag was placed |
| IsHidden | bool | Default false | Soft-delete (admin hides review) |
| HiddenAt | DateTime? | Nullable | When admin hid the review |
| HiddenBy | Guid? | FK → Users, nullable | Admin who hid review |
| CreatedAt | DateTime | Non-null, UTC | When review was created |
| UpdatedAt | DateTime? | Nullable | When review was last edited |

**Unique Constraint**: `(UserId, CourseId)` — one review per student per course.

**Soft Delete**: Reviews are soft-deleted (`IsHidden = true`) rather than permanently removed. Hidden reviews are excluded from public queries but retained for admin moderation and analytics.

**Indexes**:
- `(CourseId, CreatedAt DESC)` — for paginated course page display
- `(UserId, CourseId)` — unique constraint + quick lookup
- `(IsFlagged, IsHidden)` — for admin moderation queue

---

## Entity: Certificate

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | Guid (sequential) | PK, non-null | Generated via `NewId.NextSequentialGuid()` |
| UserId | Guid | FK → Users, non-null | The student who earned the certificate |
| CourseId | Guid | FK → Courses, non-null | The completed course |
| EnrollmentId | Guid | FK → Enrollments, non-null | Link to the enrollment record |
| Code | string | Max 50, unique, non-null | Verification code, e.g., `CERT-A3F8B2C1` |
| Status | CertificateStatus (enum) | Non-null | `Valid` or `Revoked` |
| CompletedAt | DateTime | Non-null, UTC | When course was completed |
| RevokedAt | DateTime? | Nullable | When certificate was revoked |
| RevokedBy | Guid? | FK → Users, nullable | Admin who revoked (null = auto-revoke) |
| CreatedAt | DateTime | Non-null, UTC | When certificate was generated |

**Unique Constraint**: `Code` — globally unique verification code.

**Indexes**:
- `Code` — unique index for fast verification lookup
- `(UserId, CourseId)` — unique for lookup
- `(UserId, CreatedAt DESC)` — for student certificate list
- `Status` — for admin queries

---

## Enum: CertificateStatus

```csharp
public enum CertificateStatus
{
    Valid,
    Revoked
}
```

## State Transitions

### Review Lifecycle

```
Created → [Edited] → Deleted (soft: IsHidden = true)
                    → Flagged → Admin reviewed → Hidden (soft: IsHidden = true)
                                                 → Dismissed (IsFlagged = false)
```

### Certificate Lifecycle

```
Generated (Status: Valid) → Completion verified, code assigned
                          → Revoked (Status: Revoked) — on refund or admin action
```
