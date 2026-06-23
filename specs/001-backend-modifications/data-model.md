# Data Model: Backend Modifications — Athary Platform

**Date**: 2026-06-23  
**Feature**: Backend Modifications  
**Spec**: [spec.md](./spec.md)

## Entity Relationship Diagram

```
┌─────────────┐       ┌─────────────────────┐       ┌─────────────────┐
│    User     │       │    Testimonial      │       │ NotificationPref│
├─────────────┤       ├─────────────────────┤       ├─────────────────┤
│ Id (PK)     │◄──1:1─│ UserId (FK, UQ)     │       │ Id (PK)         │
│ FirstName   │       │ Content             │       │ UserId (FK, UQ) │
│ LastName    │       │ Rating              │       │ EmailNotif      │
│ Slug (UQ)   │       │ IsApproved          │       │ PushNotif       │
│ ...         │       │ IsFlagged           │       │ CourseUpdates   │
└─────────────┘       │ FlagReason          │       │ MarketingEmails │
                      │ DisplayOrder        │       │ ...             │
                      └─────────────────────┘       └─────────────────┘

┌─────────────────┐       ┌─────────────────┐
│ ContactMessage  │       │   LegalPage     │
├─────────────────┤       ├─────────────────┤
│ Id (PK)         │       │ Id (PK)         │
│ FullName        │       │ Type (enum)     │
│ Email           │       │ Title           │
│ Phone           │       │ Content         │
│ Subject         │       │ IsPublished     │
│ Message         │       │ Version         │
│ IsRead          │       │ LastUpdatedById │
└─────────────────┘       └─────────────────┘
```

## Entity Definitions

### 1. User (Modified)

**Table**: `Users`

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | uuid | PK | Existing |
| FirstName | varchar(100) | Required | Existing |
| LastName | varchar(100) | Required | Existing |
| Slug | varchar(100) | Nullable, Unique | NEW - for instructor profiles |
| ... | ... | ... | Other existing fields |

**Indexes**:
- `IX_Users_Slug` - Unique, filtered: `[Slug] IS NOT NULL`

**Migration**: Add column + index

### 2. Testimonial (New)

**Table**: `Testimonials`

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | uuid | PK | |
| Content | varchar(1000) | Required, MinLength=10 | |
| Rating | int | Required, Range(1-5) | |
| UserId | uuid | FK → Users, Unique | One per user |
| IsApproved | boolean | Default: false | |
| IsFlagged | boolean | Default: false | |
| FlagReason | varchar(500) | Nullable | |
| DisplayOrder | int | Default: 0 | For admin reordering |
| CreatedAt | timestamp | Required | |
| UpdatedAt | timestamp | Nullable | |

**Indexes**:
- `IX_Testimonials_UserId` - Unique
- `IX_Testimonials_IsApproved_DisplayOrder` - Composite for public queries

**Constraints**:
- Unique constraint on UserId (one testimonial per user)
- FK to Users with cascade delete

### 3. NotificationPreference (New)

**Table**: `NotificationPreferences`

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | uuid | PK | |
| UserId | uuid | FK → Users, Unique | One per user |
| EmailNotifications | boolean | Default: true | |
| PushNotifications | boolean | Default: true | |
| CourseUpdates | boolean | Default: true | |
| MarketingEmails | boolean | Default: false | |
| NewMessageAlerts | boolean | Default: true | |
| LiveSessionReminders | boolean | Default: true | |
| QuizReminders | boolean | Default: true | |
| CertificateAchievements | boolean | Default: true | |
| AnnouncementAlerts | boolean | Default: true | |
| CreatedAt | timestamp | Required | |
| UpdatedAt | timestamp | Nullable | |

**Indexes**:
- `IX_NotificationPreferences_UserId` - Unique

**Constraints**:
- Unique constraint on UserId (one preference per user)
- FK to Users with cascade delete

**Seed Data**: Migration seeds defaults for existing users

### 4. ContactMessage (New)

**Table**: `ContactMessages`

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | uuid | PK | |
| FullName | varchar(100) | Required | |
| Email | varchar(200) | Required, EmailAddress | |
| Phone | varchar(20) | Nullable, Phone | |
| Subject | varchar(200) | Required | |
| Message | varchar(2000) | Required | |
| IsRead | boolean | Default: false | |
| CreatedAt | timestamp | Required | |

**Indexes**:
- `IX_ContactMessages_IsRead` - Filtered: `[IsRead] = 0`

**Constraints**:
- No assignment field in v1 (per clarification)

### 5. LegalPage (New)

**Table**: `LegalPages`

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | uuid | PK | |
| Type | varchar(50) | Required, Fixed Enum | "privacy", "terms", "refund" |
| Title | varchar(200) | Required | |
| Content | text | Required | |
| IsPublished | boolean | Default: true | |
| Version | varchar(20) | Nullable | |
| LastUpdatedById | uuid | FK → Users, Nullable | |
| CreatedAt | timestamp | Required | |
| UpdatedAt | timestamp | Nullable | |

**Indexes**:
- `IX_LegalPages_Type` - Unique (one page per type)

**Constraints**:
- Fixed enum validation: type must be one of "privacy", "terms", "refund"
- FK to Users (LastUpdatedById) with set null on delete

## State Transitions

### Testimonial States

```
[Created] ──approve──► [Approved]
    │                      │
    └────flag────► [Flagged]
                       │
                   unflag──► [Approved]
```

### ContactMessage States

```
[Unread] ──mark-read──► [Read]
    │                      │
    └────delete────► [Deleted]
```

### LegalPage States

```
[Draft] ──publish──► [Published]
    │                    │
    └──unpublish──► [Unpublished]
```

## Validation Rules

| Entity | Field | Rule | Source |
|--------|-------|------|--------|
| Testimonial | Content | Required, 10-1000 chars | FR-008 |
| Testimonial | Rating | Required, 1-5 | FR-008 |
| ContactMessage | Email | Required, valid email | FR-014 |
| ContactMessage | Message | Required, max 2000 chars | FR-014 |
| LegalPage | Type | Must be privacy/terms/refund | FR-020 |
| User | Slug | Max 100 chars, lowercase alphanumeric + hyphens + Arabic | FR-021 |
