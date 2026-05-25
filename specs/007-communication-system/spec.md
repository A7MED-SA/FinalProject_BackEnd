# Feature Specification: Communication & System

**Feature Branch**: `007-communication-system`  
**Created**: 2026-05-25  
**Status**: Draft  
**Input**: User description: "عايز اعمل Communication & System بالكامل تكون متكاملة مع المشروع"

## Clarifications

### Session 2026-05-25

- Q: When an admin initiates a conversation with a user, can that user reply back? → A: Yes — replies within an existing conversation are always permitted regardless of the original sender's role.
- Q: How is a conversation identified — ordered sender→receiver or unordered both directions? → A: Unordered — all messages between the same two users (both directions) belong to one conversation.
- Q: What are the message content validation rules? → A: Plain text only, max 5000 characters, no HTML or markup.
- Q: What lifecycle stages do announcements support? → A: Simple active/inactive only — announcements are active immediately upon creation and deactivated manually. No draft or scheduled states.
- Q: Should rate limiting be applied to messaging endpoints? → A: Yes — 30 messages per minute per user to prevent spam.

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Internal Messaging (Priority: P1)

A student sends a message to their course instructor asking about course content. The instructor receives the message, replies, and both can see the conversation thread. Admins can send announcements to all students or specific users. Users see unread message counts and can mark messages as read.

**Why this priority**: Core communication feature — enables direct interaction between students and instructors within the platform, replacing external communication channels.

**Independent Test**: Student A sends a message to Instructor B. Instructor B sees the message in their inbox, replies. Student A sees the reply. Both see correct read/unread status and timestamps.

**Acceptance Scenarios**:

1. **Given** a student is enrolled in a course, **When** they send a message to the course instructor, **Then** the instructor receives the message and both can view the conversation thread.
2. **Given** a user has unread messages, **When** they view their inbox, **Then** they see an unread count and the most recent messages sorted by sent time.
3. **Given** a user opens a message, **When** they view it, **Then** it is marked as read with a timestamp.
4. **Given** an admin wants to contact a user, **When** they compose a message, **Then** they can send to any user regardless of enrollment relationship.
5. **Given** a user tries to message someone they have no relationship with (not their instructor, not their student, not admin), **When** they attempt to send, **Then** the system rejects the message.

---

### User Story 2 — Announcements (Priority: P1)

Admins create platform-wide announcements visible to all users on their dashboard. Instructors create course-specific announcements visible only to enrolled students. Users see active announcements sorted by publish date.

**Why this priority**: Essential for platform communication — admins need to broadcast updates, instructors need to communicate with their classes.

**Independent Test**: Admin creates a platform announcement. All users see it in their announcements feed. Instructor creates a course announcement. Only enrolled students of that course see it.

**Acceptance Scenarios**:

1. **Given** an admin creates a platform-wide announcement, **When** any user views announcements, **Then** they see the announcement in the list.
2. **Given** an instructor creates a course announcement, **When** an enrolled student views announcements, **Then** they see the course announcement.
3. **Given** a user is not enrolled in a course, **When** they view announcements, **Then** they do not see course-specific announcements for that course.
4. **Given** an announcement is marked inactive, **When** users view announcements, **Then** it does not appear.

---

### User Story 3 — System Settings (Priority: P2)

Admins view and manage platform settings through a structured interface grouped by category (general, payments, emails, registration, etc.). Settings are key-value pairs with data types (string, number, boolean, JSON). Settings changes are logged for audit.

**Why this priority**: Provides runtime configurability without code changes, enabling non-technical admins to adjust platform behavior.

**Independent Test**: Admin updates a boolean setting from true to false. Verify the change is persisted and reflected on next read. Audit log contains the change record.

**Acceptance Scenarios**:

1. **Given** an admin views system settings, **When** they filter by group, **Then** they see only settings in that group.
2. **Given** an admin updates a setting value, **When** the update is saved, **Then** the new value is immediately used by the system.
3. **Given** an admin updates a setting, **When** the change is saved, **Then** an audit log entry records the change (who, what, old value, new value, timestamp).
4. **Given** a non-admin user tries to access settings endpoints, **When** they request, **Then** access is denied.

---

### User Story 4 — Activity Log Viewer (Priority: P2)

Admins view and filter platform activity logs including user actions, system events, and security events. Logs can be filtered by user, action type, entity type, date range, and IP address.

**Why this priority**: Essential for platform monitoring, security auditing, and troubleshooting.

**Independent Test**: Perform a known action (e.g., approve a refund), then query the activity log filtered by that action type. Verify the log entry exists with correct user, timestamp, and details.

**Acceptance Scenarios**:

1. **Given** an admin views the activity log, **When** they apply filters (user, action, date range), **Then** only matching log entries are displayed.
2. **Given** a user performs an action (login, refund, certificate issue), **When** the action completes, **Then** a corresponding activity log entry is created.
3. **Given** an admin views logs paginated, **When** they navigate pages, **Then** they see consistent ordering by timestamp.

---

### User Story 5 — Content Reporting (Priority: P3)

Users report inappropriate content (courses, reviews, messages). Admins review reports, take action (dismiss or take action against the reported content), and the reporter is notified of the outcome.

**Why this priority**: Important for platform safety and community guidelines enforcement, but can be launched after core communication features are stable.

**Independent Test**: User reports a review as inappropriate. Admin views the pending report, approves action against the review. The reporter receives notification of the outcome.

**Acceptance Scenarios**:

1. **Given** a user finds inappropriate content, **When** they submit a report with reason and description, **Then** the report is recorded with status "Pending".
2. **Given** an admin views pending reports, **When** they review and take action, **Then** the report status updates and the reporter is notified.
3. **Given** a user submits a duplicate report for the same content, **When** they attempt to submit, **Then** the system rejects the duplicate or updates the existing report.

---

### Edge Cases

- What happens when a student sends a message to an instructor who no longer teaches the course? Messaging should be allowed for past course relationships.
- What happens when a user is deleted/deactivated? Their sent messages should remain visible to recipients but show "Deleted User".
- What happens when announcement has no active period? It should be active immediately until manually deactivated.
- What happens when a system setting value fails type validation? The system should reject with a clear error message.
- What happens when millions of log entries exist? Listing should be paginated with maximum page size limits.
- What happens when reporting the same content twice? Second report should update the existing report's metadata rather than creating a duplicate.
- What happens when reported content is already deleted? The report should still be reviewable with a note that content was removed.
- What happens when a user sends messages too quickly? The system should enforce rate limiting: maximum 30 messages per minute per user, returning HTTP 429 when exceeded.

## Requirements *(mandatory)*

### Functional Requirements

#### Messaging

- **FR-001**: System MUST allow authenticated users to send messages to other users based on relationship rules: student→instructor of enrolled course, instructor→student in their course, admin→any user. Replies within an existing conversation are always permitted regardless of the original sender's role (e.g., a student can reply to an admin who initiated).
- **FR-002**: System MUST maintain message threads/conversations between two users with timestamps and read status.
- **FR-003**: System MUST return unread message count for each user.
- **FR-004**: System MUST mark messages as read when the recipient opens them, recording the read timestamp.
- **FR-005**: System MUST support paginated listing of conversations (most recent first).
- **FR-006**: System MUST support paginated listing of messages within a conversation (chronological order).
- **FR-007**: System MUST allow users to delete their own messages (soft delete — remains visible to the other party).
- **FR-008**: System MUST validate that the sender has a relationship with the recipient before allowing message delivery.
- **FR-009**: System MUST validate message content — plain text only, maximum 5000 characters, no HTML or markup allowed. Requests exceeding the limit or containing HTML MUST be rejected with a clear error.

#### Announcements

- **FR-010**: System MUST allow admins to create, update, deactivate, and delete platform-wide announcements.
- **FR-011**: System MUST allow instructors to create, update, and manage announcements for their own courses.
- **FR-012**: System MUST return a feed of active announcements for the current user, filtered by their enrollments and platform-wide visibility.
- **FR-013**: Announcements MUST have title, content, target audience (All, StudentsOnly, InstructorsOnly, SpecificCourse), and active status.
- **FR-014**: Announcements MUST be ordered by publish date (newest first).

#### System Settings

- **FR-015**: System MUST provide CRUD endpoints for system settings accessible only to admins.
- **FR-016**: Settings MUST be organized by group (General, Payments, Email, Registration, Notifications, etc.) for logical grouping.
- **FR-017**: Each setting MUST have a key, value, data type (String, Number, Boolean, JSON), group, and description.
- **FR-018**: System MUST validate setting values against their declared data type before saving.
- **FR-019**: All setting changes MUST be logged to the activity log with who, what, old value, new value, and timestamp.

#### Activity Logs

- **FR-020**: System MUST provide a paginated, filterable endpoint for activity logs accessible only to admins.
- **FR-021**: Filters MUST include: user ID, action type, entity type, date range (from/to), and IP address.
- **FR-022**: Log entries MUST include user, action, entity type, entity ID, details, IP address, and timestamp.
- **FR-023**: System MUST enforce a maximum page size (100 entries) to prevent performance issues.

#### Content Reporting

- **FR-024**: System MUST allow authenticated users to report content (courses, reviews, and messages).
- **FR-025**: Reports MUST include: entity type, entity ID, reason (Spam, Inappropriate, Harassment, Copyright, Other), and optional description.
- **FR-026**: System MUST prevent duplicate reports from the same user for the same content — updating existing report instead.
- **FR-027**: System MUST provide an admin endpoint to list pending reports with pagination.
- **FR-028**: Admins MUST be able to resolve reports with status (Dismissed, ActionTaken) and optional admin notes.
- **FR-029**: When a report is resolved, the reporter MUST receive a notification with the outcome.

### Key Entities *(include if feature involves data)*

- **Message**: Individual message between two users with sender, receiver, content, timestamps, read status. Existing model in `backend_project/models/Message.cs`.
- **Conversation**: Logical grouping of messages between two users (derived from Message table, not a separate entity). Grouped by the unordered user pair (A↔B), so messages in both directions belong to the same conversation.
- **Announcement**: Platform-wide or course-specific broadcast message with target audience and active status. Existing model in `backend_project/models/Announcement.cs`.
- **SystemSetting**: Key-value configuration with data type and group. Existing model in `backend_project/models/SystemSetting.cs`.
- **ActivityLog**: Record of user actions and system events with filterable metadata. Existing model in `backend_project/models/ActivityLog.cs`.
- **Report**: User-submitted report against content with reason, status, and resolution. Existing model in `backend_project/models/Report.cs`.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Messages between users are delivered and visible to the recipient in under 2 seconds.
- **SC-002**: Users can view their inbox with unread counts and send messages without errors in 95% of attempts.
- **SC-003**: Announcements created by admins appear in all users' feeds within 5 seconds of publishing.
- **SC-004**: System setting changes take effect immediately (within 1 second) after save.
- **SC-005**: Activity log queries with filters return results in under 5 seconds even with 100,000+ log entries.
- **SC-006**: Reports submitted by users are visible to admins immediately with correct status and metadata.
- **SC-007**: 100% of content reporting actions (submit, review, resolve) complete without data loss or inconsistency.

## Assumptions

- Existing `Message`, `Announcement`, `SystemSetting`, `ActivityLog`, `Report`, and `Notification` models in `backend_project/models/` contain sufficient fields — no new schema changes required.
- Existing `INotificationService`/`NotificationService`, `IActivityLogService`/`ActivityLogService` will be reused for notifications and logging.
- Existing `NotificationController` already handles notification listing — this feature will reuse or extend it.
- Authentication and authorization (JWT + Identity) will be reused — no changes to the auth system.
- Messaging is direct 1-on-1 between two users — group chats are out of scope.
- System settings are cached in-memory with periodic refresh for performance — admins see immediate updates after save.
- Activity logs grow indefinitely; archiving or deletion is handled externally or by database retention policies.
- Real-time delivery (WebSocket/SignalR) for new messages is out of scope for initial version — users refresh to see new messages.
- Email notifications for new messages and announcements are out of scope for initial version.
