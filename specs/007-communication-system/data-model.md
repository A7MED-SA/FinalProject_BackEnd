# Data Model: Communication & System

## Entities

### Message (Existing — needs modification)

| Field | Type | Notes |
|-------|------|-------|
| Id (BaseEntity) | Guid (sequential) | PK, auto-generated |
| SenderId | Guid (FK → User) | Required |
| ReceiverId | Guid (FK → User) | Required |
| Content | string (max 5000) | Plain text, validated |
| SentAt | DateTime | Default UtcNow |
| IsRead | bool | Default false |
| ReadAt | DateTime? | Null until read |
| IsDeleted | bool | **NEW** — soft delete per FR-007 |

**Relationships**: Sender → User (M:1), Receiver → User (M:1)
**Conversation**: Derived — grouped by unordered `(MIN(SenderId,ReceiverId), MAX(SenderId,ReceiverId))`
**Indexes**: Composite on `(SenderId, ReceiverId, SentAt DESC)` for inbox query; composite on `(ReceiverId, IsRead)` for unread count
**Validation**:
- Content: max 5000 chars, plain text only, no HTML (FluentValidation)
- Sender != Receiver (self-message rejected)
- Relationship validated for first message (student→enrolled instructor, instructor→their student, admin→any)

### Announcement (Existing — needs modification)

| Field | Type | Notes |
|-------|------|-------|
| Id (BaseEntity) | Guid (sequential) | PK, auto-generated |
| Title | string (max 255) | Required |
| Content | string | Required |
| Target | AnnouncementTarget (enum) | All, Students, Teachers, Admins — **existing enum needs `SpecificCourse` added** |
| CourseId | Guid? (FK → Course) | **NEW** — required when Target = SpecificCourse |
| CreatedBy | Guid (FK → User) | Required |
| IsActive | bool | Default true — active/inactive lifecycle |
| PublishedAt | DateTime? | Default null (set on creation) |
| ExpiresAt | DateTime? | Existing field (auto-deactivation support) |

**Relationships**: Creator → User (M:1), Course → Course (M:1, nullable)
**Indexes**: Composite on `(IsActive, PublishedAt DESC)` for feed query; on `(Target)` for audience filtering
**Validation**:
- Title: max 255 chars
- CourseId required when Target = SpecificCourse
- Target enum values: All, Students, Teachers, Admins (**extension needed** for SpecificCourse)

### SystemSetting (Existing — no changes needed)

| Field | Type | Notes |
|-------|------|-------|
| Id (BaseEntity) | Guid (sequential) | PK, auto-generated |
| SettingGroup | string (max 50) | e.g., General, Payments, Email |
| Key | string (max 100) | Unique within group |
| Value | string? | Stored as string, cast to DataType |
| DataType | SettingDataType (enum) | String, Integer, Boolean, Json |
| Description | string? | Human-readable |
| IsPublic | bool | Publicly visible flag |
| UpdatedAt | DateTime? | Set on change |
| UpdatedBy | Guid? (FK → User) | Set on change |

**Validation**: Value must be parseable to DataType (FluentValidation with custom validator)
**Caching**: In-memory cache with 60s sliding expiry; admin saves trigger invalidation

### ActivityLog (Existing — enum extension needed)

| Field | Type | Notes |
|-------|------|-------|
| Id (BaseEntity) | Guid (sequential) | PK, auto-generated |
| UserId | Guid (FK → User) | Required |
| Action | string (max 100) | Free-text action name |
| EntityType | ActivityLogEntityType (enum) | **Needs**: Message, Announcement, SystemSetting, Report added |
| EntityId | Guid? | Related entity |
| Details | string? | JSON or free-text details |
| IpAddress | string (max 45) | Client IP |
| CreatedAt | DateTime | Default UtcNow |

**Relationships**: User → User (M:1)
**Indexes**: Composite on `(CreatedAt DESC)`; on `(UserId, Action)` for filtered queries
**Validation**: Max page size 100 (enforced at controller level)

### Report (Existing — enum extensions needed)

| Field | Type | Notes |
|-------|------|-------|
| Id (BaseEntity) | Guid (sequential) | PK, auto-generated |
| ReporterId | Guid (FK → User) | Required |
| EntityType | ReportEntityType (enum) | **Needs**: Message added (currently: Course, Review, Comment, User) |
| EntityId | Guid | Required |
| Reason | ReportReason (enum) | **Needs**: Harassment added (currently: Spam, Inappropriate, Copyright, Other) |
| Description | string? | Optional user description |
| Status | ReportStatus (enum) | **Needs**: Dismissed, ActionTaken (currently: Pending, Resolved, Rejected) |
| AdminNote | string? | **NEW** — admin notes on resolution |
| CreatedAt | DateTime | Default UtcNow |
| ResolvedAt | DateTime? | Set on resolution |
| ResolvedBy | Guid? (FK → User) | Set on resolution |

**Relationships**: Reporter → User (M:1), Resolver → User (M:1, nullable)
**Unique constraint**: Composite index on `(ReporterId, EntityType, EntityId)` — prevents duplicates
**Validation**: Reason must be a valid enum value; EntityId must be non-empty

## Entity Relationship Diagram (text)

```
User ──1:N──> Message (as Sender)
User ──1:N──> Message (as Receiver)
User ──1:N──> Announcement (as Creator)
Course ──1:N──> Announcement (course-specific)
User ──1:N──> ActivityLog
User ──1:N──> Report (as Reporter)
User ──1:N──> Report (as Resolver)

Conversation: derived from Message (no separate table)
  ── grouped by unordered (SenderId, ReceiverId) pair
```

## Summary of Schema Changes Needed

| Entity | Change | Migration Required |
|--------|--------|-------------------|
| Message | Add `IsDeleted` column | ✅ Yes |
| Announcement | Add `CourseId` column + FK; extend `AnnouncementTarget` enum | ✅ Yes |
| ActivityLog | Extend `ActivityLogEntityType` enum (add Message, Announcement, SystemSetting, Report) | ✅ Yes |
| Report | Extend `ReportEntityType` enum (add Message); extend `ReportReason` enum (add Harassment); change `ReportStatus` enum (Dismissed, ActionTaken instead of Resolved, Rejected); add `AdminNote` column | ✅ Yes |

## State Transitions

### Message
```
Sent (IsRead=false) ──> Read (IsRead=true, ReadAt=UtcNow)
Sent/Read ──> Deleted (IsDeleted=true, visible to receiver only)
```

### Announcement
```
Active (IsActive=true) ──> Inactive (IsActive=false)
Inactive ──> Active (re-activation via update)
```

### Report
```
Pending ──> Dismissed (no action needed)
Pending ──> ActionTaken (moderation action performed)
```

### System Setting
```
Created ──> Updated (Value changed, UpdatedAt + UpdatedBy set)
```
