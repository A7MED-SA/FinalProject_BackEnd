# Data Model: Content & Learning System with Deletion Lifecycle

**Date**: 2026-05-14

> All 13 entities below already exist as C# models in `backend_project/models/`. This document validates their schemas against the spec requirements and documents the relationships, state machines, and validation rules needed for the service layer.

## Entity Inventory

### 1. Video (`videos`)
**Status**: ✅ Model exists — no changes needed

| Field | Type | Constraint | Notes |
|-------|------|-----------|-------|
| Id | Guid (PK) | BaseEntity | Sequential GUID |
| Title | string(255) | Required | |
| VideoFileId | Guid (FK) | Required → UploadedFile | |
| ThumbnailFileId | Guid? (FK) | Optional → UploadedFile | |
| Provider | VideoProvider (enum) | | Local, YouTube, Vimeo, Minio |
| ProviderVideoId | string?(255) | | External provider reference |
| Quality | VideoQuality (enum) | Default: 720p | |
| DurationSeconds | int | | Used for completion threshold calc |
| Transcript | string? | | |
| Status | VideoStatus (enum) | Default: Processing | Processing → Ready / Failed |
| ViewCount | int | Default: 0 | Incremented on access |

**Relationships**: SectionItem (1:1 via polymorphic), VideoComments (1:N)

---

### 2. Document (`documents`)
**Status**: ✅ Model exists — no changes needed

| Field | Type | Constraint |
|-------|------|-----------|
| Id | Guid (PK) | BaseEntity |
| Title | string(255) | Required |
| Description | string? | |
| FileId | Guid (FK) | Required → UploadedFile |
| DownloadCount | int | Default: 0 |
| CreatedAt | DateTime | Default: UtcNow |

**Relationships**: SectionItem (1:1 via polymorphic)

---

### 3. VideoComment (`video_comments`)
**Status**: ✅ Model exists — no changes needed

| Field | Type | Constraint |
|-------|------|-----------|
| Id | Guid (PK) | BaseEntity |
| VideoId | Guid (FK) | Required → Video |
| UserId | Guid (FK) | Required → User |
| ParentCommentId | Guid? (FK) | Self-ref → VideoComment (flattened to 1 level) |
| Content | string | Required |
| IsEdited | bool | Default: false |
| LikesCount | int | Default: 0 |
| CreatedAt | DateTime | Default: UtcNow |
| UpdatedAt | DateTime? | |
| DeletedAt | DateTime? | Soft delete |

**Relationships**: Replies (1:N self-ref), CommentLikes (1:N), Video (N:1), User (N:1)

**Service Validation**: When `ParentCommentId` is set, service must check if parent has its own parent → flatten to root.

---

### 4. CommentLike (`comment_likes`)
**Status**: ✅ Model exists — no changes needed

| Field | Type | Constraint |
|-------|------|-----------|
| Id | Guid (PK) | BaseEntity |
| CommentId | Guid (FK) | Required → VideoComment |
| UserId | Guid (FK) | Required → User |
| CreatedAt | DateTime | Default: UtcNow |

**Uniqueness**: Enforce `(CommentId, UserId)` unique constraint in DbContext/migration.

---

### 5. Quiz (`quizzes`)
**Status**: ✅ Model exists — no changes needed

| Field | Type | Constraint |
|-------|------|-----------|
| Id | Guid (PK) | BaseEntity |
| Title | string(255) | Required |
| Description | string? | |
| DurationMinutes | int? | Null = unlimited time |
| PassingScorePercent | int | Default: 60 |
| MaxAttempts | int? | Null = unlimited attempts |
| ShuffleQuestions | bool | Default: false |
| ShuffleOptions | bool | Default: false |
| ShowResultsImmediately | bool | Default: true |
| AllowReview | bool | Default: true |
| AvailableFrom | DateTime? | |
| AvailableUntil | DateTime? | |
| CreatedAt | DateTime | Default: UtcNow |

**Relationships**: Questions (1:N), QuizAttempts (1:N), SectionItem (1:1 via polymorphic)

---

### 6. Question (`questions`)
**Status**: ✅ Model exists — no changes needed

| Field | Type | Constraint |
|-------|------|-----------|
| Id | Guid (PK) | BaseEntity |
| QuizId | Guid (FK) | Required → Quiz |
| QuestionText | string | Required |
| Type | QuestionType (enum) | Default: MultipleChoice |
| Points | int | Default: 1 |
| Explanation | string? | |
| Position | int | Default: 0 |

**Relationships**: Options (1:N), UserAnswers (1:N)

---

### 7. Option (`options`)
**Status**: ✅ Model exists — no changes needed

| Field | Type | Constraint |
|-------|------|-----------|
| Id | Guid (PK) | BaseEntity |
| QuestionId | Guid (FK) | Required → Question |
| OptionText | string(500) | Required |
| IsCorrect | bool | Default: false |
| Position | int | Default: 0 |

---

### 8. QuizAttempt (`quiz_attempts`)
**Status**: ✅ Model exists — no changes needed

| Field | Type | Constraint |
|-------|------|-----------|
| Id | Guid (PK) | BaseEntity |
| EnrollmentId | Guid (FK) | Required → Enrollment |
| QuizId | Guid (FK) | Required → Quiz |
| Score | int | Default: 0 |
| MaxScore | int | Default: 0 |
| Status | QuizAttemptStatus (enum) | Default: InProgress |
| StartedAt | DateTime | Default: UtcNow |
| SubmittedAt | DateTime? | |
| AttemptNumber | int | Default: 1 |
| TimeTakenSeconds | int? | |

**State Machine**: InProgress → Submitted → Graded

---

### 9. UserAnswer (`user_answers`)
**Status**: ✅ Model exists — no changes needed

| Field | Type | Constraint |
|-------|------|-----------|
| Id | Guid (PK) | BaseEntity |
| AttemptId | Guid (FK) | Required → QuizAttempt |
| QuestionId | Guid (FK) | Required → Question |
| SelectedOptionId | Guid? (FK) | Optional → Option |
| AnswerText | string? | For ShortAnswer type |
| IsCorrect | bool | Default: false |
| PointsEarned | int | Default: 0 |

---

### 10. LiveSession (`live_sessions`)
**Status**: ✅ Model exists — **note**: Missing `MeetingUrl`, `Password`, and `MaxAttendees` fields

| Field | Type | Constraint | Notes |
|-------|------|-----------|-------|
| Id | Guid (PK) | BaseEntity | |
| CourseId | Guid (FK) | Required → Course | |
| Title | string(255) | Required | |
| Description | string? | | |
| ScheduledStart | DateTime | | |
| ScheduledEnd | DateTime | | |
| Status | LiveSessionStatus (enum) | | Scheduled → Live → Finished/Cancelled |
| RecordingFileId | Guid? (FK) | → UploadedFile | |

**⚠ Missing Fields (to add)**: `MeetingUrl` (string, Required), `Password` (string?), `MaxAttendees` (int?), `ActualStartAt` (DateTime?), `ActualEndAt` (DateTime?)

---

### 11. LiveAttendance (`live_attendances`)
**Status**: ✅ Model exists — no changes needed

| Field | Type | Constraint |
|-------|------|-----------|
| Id | Guid (PK) | BaseEntity |
| SessionId | Guid (FK) | Required → LiveSession |
| UserId | Guid (FK) | Required → User |
| JoinedAt | DateTime | Default: UtcNow |
| LeftAt | DateTime? | |
| DurationMinutes | int? | |

---

### 12. Enrollment (`enrollments`)
**Status**: ✅ Model exists — no changes needed

| Field | Type | Constraint |
|-------|------|-----------|
| Id | Guid (PK) | BaseEntity |
| UserId | Guid (FK) | Required → User |
| CourseId | Guid (FK) | Required → Course |
| EnrolledAt | DateTime | Default: UtcNow |
| Status | EnrollmentStatus (enum) | Default: InProgress |
| CompletedAt | DateTime? | |
| LastAccessedAt | DateTime? | |
| ProgressPercentage | decimal(5,2) | Default: 0 |
| CertificateId | Guid? (FK) | → Certificate |
| Source | EnrollmentSource (enum) | Default: Purchase |
| AccessExpiresAt | DateTime? | |
| IsRefunded | bool | Default: false |

**State Machine**: InProgress → Completed / Expired / Refunded

**Uniqueness**: Enforce `(UserId, CourseId)` unique constraint.

---

### 13. ContentProgress (`content_progresses`)
**Status**: ✅ Model exists — no changes needed

| Field | Type | Constraint |
|-------|------|-----------|
| Id | Guid (PK) | BaseEntity |
| EnrollmentId | Guid (FK) | Required → Enrollment |
| ContentType | ContentType (enum) | Video/Quiz/Document/LiveSession |
| ContentId | Guid | Polymorphic FK |
| IsCompleted | bool | Default: false |
| WatchTimeSeconds | int | Default: 0 (for Video) |
| AttemptsCount | int | Default: 0 (for Quiz) |
| CompletionPercentage | decimal(5,2) | Default: 0 |
| Metadata | string? (nvarchar max) | JSON for extensibility |
| LastAccessedAt | DateTime? | |
| CompletedAt | DateTime? | |

**Uniqueness**: Enforce `(EnrollmentId, ContentType, ContentId)` unique constraint.

---

## Relationship Map

```
Category (1:N) → Course (1:N) → Section (1:N) → SectionItem (1:1) → Video|Quiz|Document|LiveSession
                     ↓
              Enrollment (N:1 User, N:1 Course)
                     ↓
              ContentProgress (N:1 Enrollment, polymorphic → content)
              QuizAttempt (N:1 Enrollment, N:1 Quiz)
                     ↓
              UserAnswer (N:1 QuizAttempt, N:1 Question, N:1? Option)

Video (1:N) → VideoComment (1:N) → CommentLike
LiveSession (1:N) → LiveAttendance
```

## Deletion Decision Matrix

| Entity | Draft Course | Published Course (no enrollments) | Published Course (with enrollments) |
|--------|-------------|-----------------------------------|-------------------------------------|
| Category | Hard delete if empty; block if has courses/children | N/A | N/A |
| Course | **Hard cascade** (sections, items, content) | Soft delete (set DeletedAt) | Soft delete + read-only access for students |
| Section | Hard delete | Soft delete via approval workflow | Soft delete via approval workflow |
| SectionItem | Hard delete | Soft delete via approval workflow | Soft delete via approval workflow |
| Video/Quiz/Doc/Live | Hard delete (1:1 with course) | Soft delete | Soft delete (preserve progress) |
| Quiz (with in-progress attempts) | N/A | Block until resolved | Block until resolved |
| LiveSession (status=Live) | N/A | Block | Block |
| VideoComment | Soft delete always (set DeletedAt) | Same | Same |
| CommentLike | Hard delete (toggle off) | Same | Same |
| Enrollment | Never deleted by user action | N/A | Preserved always |
| ContentProgress | Never deleted by user action | N/A | Preserved always |
| QuizAttempt / UserAnswer | Never deleted | N/A | Preserved always |
