# Feature Specification: Content & Learning System with Deletion Lifecycle

**Feature Branch**: `003-content-learning-deletion`  
**Created**: 2026-05-14  
**Status**: Draft  
**Input**: User description: "Implement deletion lifecycle for categories, courses, sections, and section items across usage stages. Implement Content & Learning entities (Video, Document, VideoComment, CommentLike, Quiz, Question, Option, QuizAttempt, UserAnswer, LiveSession, LiveAttendance, Enrollment, ContentProgress). Review project efficiency and operational workflow."

## Clarifications

### Session 2026-05-14

- Q: Content ownership on draft course deletion — cascade-delete content or only unlink SectionItems? → A: Cascade-delete content entities (1:1 ownership model; content is not shared across courses)
- Q: Student access after published course soft-deletion — revoke immediately or retain read-only? → A: Read-only access preserved (students can view progress and previously accessed content, no new progress tracked)
- Q: Quiz timer expiration behavior — auto-submit or block and wait for manual submit? → A: Auto-submit with answers provided so far; score calculated immediately
- Q: Comment threading depth — block replies-to-replies or flatten under original? → A: Flatten under the original top-level comment (mention the replied-to user in text)
- Q: Video completion threshold configurability — system-wide or per-course override? → A: System-wide only; single threshold value in SystemSettings, managed by admins

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Student Enrollment & Course Access (Priority: P1)

As a student, I want to enroll in a published course and have the system track my enrollment status, so that I can access the course content and monitor my learning progress over time.

**Why this priority**: Enrollment is the gateway to all learning activity. Without enrollment tracking, no student can access courses, and no progress or quiz data can be recorded. Every downstream feature depends on this.

**Independent Test**: Can be fully tested by creating an enrollment for a student on a published course, verifying the enrollment record exists with "InProgress" status, and confirming the student can retrieve the course content.

**Acceptance Scenarios**:

1. **Given** a published course and an authenticated student, **When** the student enrolls (via purchase, admin grant, or coupon), **Then** an enrollment record is created with "InProgress" status, the course's enrollment count is incremented, and the student can access the course content.
2. **Given** a student already enrolled in a course, **When** the student attempts to enroll again, **Then** the system prevents duplicate enrollment and returns an appropriate message.
3. **Given** an enrollment with an expiration date, **When** the access date passes, **Then** the enrollment status transitions to "Expired" and the student loses access to course content.
4. **Given** a refunded order linked to an enrollment, **When** the refund is processed, **Then** the enrollment status transitions to "Refunded" and access is revoked.

---

### User Story 2 - Video & Document Content Consumption with Progress Tracking (Priority: P1)

As an enrolled student, I want to watch videos and download documents within my enrolled course, and have my progress automatically tracked, so that I can resume where I left off and see my overall completion percentage.

**Why this priority**: Video and document consumption is the primary learning activity on the platform. Progress tracking is essential for course completion workflows and certificate issuance.

**Independent Test**: Can be fully tested by an enrolled student opening a video section item, the system recording watch time, marking it as complete when the threshold is met, and updating the enrollment's overall progress percentage.

**Acceptance Scenarios**:

1. **Given** an enrolled student accessing a video section item, **When** they watch the video, **Then** a ContentProgress record is created (or updated) tracking watch time, completion percentage, and last accessed timestamp.
2. **Given** an enrolled student completing a video, **When** the watch time meets the completion threshold (e.g., ≥90% of duration), **Then** the content progress is marked as completed and the enrollment's overall progress percentage is recalculated.
3. **Given** an enrolled student accessing a document section item, **When** they download the document, **Then** the document's download count is incremented and a ContentProgress record is created marking it complete.
4. **Given** a student who is not enrolled, **When** they attempt to access a non-preview video or document, **Then** the system denies access.

---

### User Story 3 - Quiz Taking & Automated Grading (Priority: P1)

As an enrolled student, I want to take quizzes within my course, answer questions, and receive automated grading with immediate or deferred results, so that I can assess my understanding of the course material.

**Why this priority**: Quizzes are a core assessment mechanism. They directly impact the student's progress, completion status, and certificate eligibility.

**Independent Test**: Can be fully tested by starting a quiz attempt, submitting answers, and verifying the system calculates the score, determines pass/fail, and updates the enrollment progress.

**Acceptance Scenarios**:

1. **Given** an enrolled student accessing a quiz section item, **When** they start the quiz, **Then** a QuizAttempt record is created with "InProgress" status, and the attempt number is tracked.
2. **Given** a quiz with a maximum attempt limit, **When** a student has reached the limit, **Then** the system prevents additional attempts.
3. **Given** a student submitting answers to all questions, **When** they submit the quiz, **Then** each UserAnswer is recorded, correct answers are auto-graded, the score is calculated, and the attempt status changes to "Submitted" or "Graded".
4. **Given** a quiz configured to show results immediately, **When** the student submits, **Then** the results (score, correct/incorrect per question, explanations) are returned immediately.
5. **Given** a quiz with a passing score threshold, **When** the student achieves a score at or above the threshold, **Then** the quiz's ContentProgress is marked as completed and enrollment progress is updated.

---

### User Story 4 - Live Session Management & Attendance (Priority: P2)

As an instructor, I want to schedule live sessions for my course and track student attendance, so that students get real-time interaction and I can monitor engagement.

**Why this priority**: Live sessions add significant educational value but are not required for the core learning path. They are supplementary to recorded content.

**Independent Test**: Can be fully tested by an instructor creating a live session, students joining and the attendance being recorded with join/leave timestamps.

**Acceptance Scenarios**:

1. **Given** an instructor owns a course, **When** they create a live session with a scheduled start/end time and meeting URL, **Then** a LiveSession record is created with "Scheduled" status.
2. **Given** a scheduled live session, **When** the session time arrives and the instructor starts it, **Then** the status transitions to "Live" and the actual start time is recorded.
3. **Given** a live session in "Live" status, **When** an enrolled student joins, **Then** a LiveAttendance record is created with their join timestamp.
4. **Given** a student leaving a live session, **When** they disconnect, **Then** the leave timestamp is recorded and the duration is calculated.
5. **Given** a live session that has ended, **When** the instructor ends it, **Then** the status transitions to "Ended", the actual end time is recorded, and the session's ContentProgress is updated for attendees.

---

### User Story 5 - Video Commenting & Engagement (Priority: P2)

As an enrolled student, I want to comment on videos (including threaded replies) and like other comments, so that I can engage with the course community and ask questions about the content.

**Why this priority**: Community engagement improves learning outcomes but is not critical for core course consumption.

**Independent Test**: Can be fully tested by posting a comment on a video, replying to it, liking a comment, and verifying the comment hierarchy and like count.

**Acceptance Scenarios**:

1. **Given** an enrolled student watching a video, **When** they post a comment, **Then** a VideoComment record is created linked to the video and the student.
2. **Given** an existing comment, **When** a student replies to it, **Then** a new VideoComment is created with the ParentCommentId set to the original comment's ID.
3. **Given** a comment, **When** a student likes it, **Then** a CommentLike record is created and the comment's likes_count is incremented.
4. **Given** a student who already liked a comment, **When** they try to like it again, **Then** the system prevents duplicate likes (toggle behavior: unlike the comment instead).
5. **Given** a comment, **When** the author soft-deletes it, **Then** the DeletedAt timestamp is set and the comment is hidden from public view but retained for moderation.

---

### User Story 6 - Deletion Lifecycle for Categories, Courses, Sections & Content (Priority: P1)

As a platform administrator or instructor, I want clear rules governing when and how I can delete categories, courses, sections, section items, and content entities, so that the system protects data integrity while allowing necessary cleanup.

**Why this priority**: Deletion rules are foundational to data safety. Incorrect deletions can orphan student progress, break enrollment records, and corrupt the learning path. This must be defined before any content is created at scale.

**Independent Test**: Can be fully tested by attempting to delete entities at various lifecycle stages (empty category, category with courses, draft course, published course with enrollments, section with active progress) and verifying the system either proceeds or blocks with the correct message.

**Acceptance Scenarios**:

1. **Given** a category with no courses, **When** an admin deletes it, **Then** the category is permanently removed (or soft-deleted if it has subcategories).
2. **Given** a category with associated courses, **When** an admin attempts to delete it, **Then** the system blocks the deletion and requires the admin to reassign or delete the courses first.
3. **Given** a draft course with no enrollments, **When** the instructor deletes it, **Then** all sections, section items, and linked content (videos, quizzes, documents, live sessions) are cascade-deleted or soft-deleted.
4. **Given** a published course with active enrollments, **When** an instructor requests deletion, **Then** the system soft-deletes the course (sets DeletedAt), hides it from the public catalog, preserves enrollment records and student progress, and grants enrolled students continued read-only access to previously accessed content (no new progress is tracked).
5. **Given** a section within a published course that has active student progress, **When** the instructor requests deletion, **Then** the system routes the deletion through the existing edit approval workflow (treated as a high-risk edit requiring admin approval).
6. **Given** a section item (video/quiz/document/live session) within a published course, **When** the instructor requests deletion, **Then** the system routes it through the edit approval workflow, and upon approval, soft-deletes the item and preserves existing ContentProgress records.
7. **Given** a quiz with existing QuizAttempts, **When** it is deleted, **Then** the quiz is soft-deleted but all attempt and answer records are preserved for academic integrity.

---

### User Story 7 - Instructor Content Management (Videos, Quizzes, Documents) (Priority: P1)

As an instructor, I want to create, update, and manage videos, documents, and quizzes (with questions and options) within my course sections, so that I can build comprehensive learning content.

**Why this priority**: This is the CRUD backbone for all content types. Without it, no content can be added to courses.

**Independent Test**: Can be fully tested by creating a video, quiz (with questions and options), and document entity, linking them to section items, updating their properties, and retrieving them.

**Acceptance Scenarios**:

1. **Given** an instructor creating a video, **When** they provide title, URL, provider, quality, and duration, **Then** a Video record is created with "Processing" status initially.
2. **Given** an instructor creating a quiz, **When** they provide the quiz title and settings (duration, passing score, attempt limits, shuffle options), **Then** a Quiz record is created.
3. **Given** an existing quiz, **When** the instructor adds a question with multiple options (marking one as correct), **Then** Question and Option records are created with proper ordering.
4. **Given** an instructor creating a document, **When** they provide the title, file URL, and file type, **Then** a Document record is created.
5. **Given** content linked to a section item in a published course, **When** the instructor updates the content, **Then** the update follows the existing edit approval workflow based on risk assessment.

---

### Edge Cases

- What happens when an instructor deletes a quiz that has in-progress (unsubmitted) attempts? The system should block deletion until all in-progress attempts are submitted or expired.
- What happens if a video referenced by ContentProgress records is deleted? The ContentProgress records should be preserved with a flag indicating the content is no longer available.
- What happens when a student's enrollment expires mid-quiz? The in-progress attempt should be auto-submitted with answers provided so far.
- How does the system handle bulk deletion of a section with 50+ items? The operation should be atomic (all succeed or all fail) and respect the edit approval workflow.
- What happens when an admin deletes a category that is the parent of subcategories with courses? The system should require recursive handling: block the deletion until all descendant categories are empty.
- How does the system prevent deletion of the last section in a published course? The system should warn but allow it, since a published course with zero sections should revert to draft status.
- What happens when a live session is deleted while in "Live" status? Deletion should be blocked; the session must be ended first.

## Requirements *(mandatory)*

### Functional Requirements

#### Enrollment & Access
- **FR-001**: System MUST allow students to enroll in published courses through purchase, admin grant, gift, or coupon redemption.
- **FR-002**: System MUST prevent duplicate enrollments for the same user-course combination.
- **FR-003**: System MUST enforce enrollment expiration, automatically transitioning the status to "Expired" when the access date passes.
- **FR-004**: System MUST track enrollment source (purchase, gift, admin_grant, coupon) for analytics and audit purposes.
- **FR-005**: System MUST recalculate enrollment progress percentage whenever any ContentProgress record for that enrollment is updated.

#### Content Management (Video, Document, Quiz)
- **FR-006**: System MUST allow instructors to create, read, update, and delete Video entities with attributes including title, URL, provider, quality, duration, transcript, subtitle support, and processing status.
- **FR-007**: System MUST allow instructors to create, read, update, and delete Document entities with attributes including title, file URL, file type, and file size.
- **FR-008**: System MUST allow instructors to create, read, update, and delete Quiz entities with settings for duration, passing score, max attempts, question/option shuffling, immediate results, and review permissions.
- **FR-009**: System MUST allow instructors to manage Questions within a quiz, including question text, type (multiple choice, true/false, short answer), points, explanation, and ordering.
- **FR-010**: System MUST allow instructors to manage Options within a question, including option text, correctness flag, and ordering.

#### Quiz Assessment
- **FR-011**: System MUST create a QuizAttempt record when a student starts a quiz, tracking the attempt number, start time, and "InProgress" status.
- **FR-012**: System MUST enforce maximum attempt limits per quiz per enrollment.
- **FR-012a**: System MUST auto-submit a quiz attempt when the configured time limit expires, recording all answers provided so far and calculating the score immediately. The attempt status transitions to "Submitted" with a flag indicating auto-submission due to timeout.
- **FR-013**: System MUST record each UserAnswer, linking it to the attempt, question, and selected option (or free text for short answer questions).
- **FR-014**: System MUST auto-grade multiple choice and true/false questions immediately upon submission.
- **FR-015**: System MUST calculate the total score and determine pass/fail based on the quiz's passing score percentage.
- **FR-016**: System MUST update ContentProgress and enrollment progress upon quiz completion (when the student passes).

#### Live Sessions
- **FR-017**: System MUST allow instructors to create live sessions with scheduled start/end times, meeting URL, password, max attendees, and recording options.
- **FR-018**: System MUST track live session lifecycle through statuses: Scheduled → Live → Ended (or Cancelled).
- **FR-019**: System MUST record student attendance with join time, leave time, and calculated duration.
- **FR-020**: System MUST block student joins when the attendee limit is reached.

#### Video Commenting
- **FR-021**: System MUST allow enrolled students to post comments on videos they have access to.
- **FR-022**: System MUST support threaded replies with a single nesting level. If a user replies to a reply (depth > 1), the system MUST flatten it by setting the ParentCommentId to the original top-level comment and prepending a @mention of the user being replied to in the comment text.
- **FR-023**: System MUST support comment likes with toggle behavior (like/unlike), preventing duplicate likes.
- **FR-024**: System MUST support soft-deletion of comments, preserving them for moderation history.
- **FR-025**: System MUST maintain accurate likes_count on comments as likes are added or removed.

#### Progress Tracking
- **FR-026**: System MUST create and update ContentProgress records for each content type (Video, Quiz, Document, LiveSession) per enrollment.
- **FR-027**: System MUST track watch time for videos and mark them complete when the system-wide watch threshold is met (stored as a SystemSetting, default 90%, managed by admins only).
- **FR-028**: System MUST recalculate overall enrollment progress as a weighted percentage of all mandatory section items completed.
- **FR-029**: System MUST transition the enrollment status to "Completed" when progress reaches 100% on all mandatory items.
- **FR-030**: System MUST issue a certificate (linking to the Certificate entity) upon course completion if the course has certificate support enabled.

#### Deletion Lifecycle
- **FR-031**: System MUST support soft-deletion for courses, video comments, and reviews (using the existing DeletedAt pattern).
- **FR-032**: System MUST block hard-deletion of categories that have associated courses; require course reassignment or deletion first.
- **FR-033**: System MUST block hard-deletion of categories that have subcategories; require recursive handling.
- **FR-034**: System MUST allow instructors to delete draft courses with full cascade-delete of sections, section items, and owned content entities (Video, Quiz, Document, LiveSession). Content follows a 1:1 ownership model — each content entity belongs to exactly one course and is not shared across courses.
- **FR-035**: System MUST enforce soft-deletion for published courses with active enrollments, preserving all student data. Enrolled students retain read-only access to their progress and previously accessed content; no new progress tracking occurs on soft-deleted courses.
- **FR-036**: System MUST route section and section-item deletions for published courses through the existing edit approval workflow.
- **FR-037**: System MUST block deletion of quizzes that have in-progress (unsubmitted) attempts.
- **FR-038**: System MUST block deletion of live sessions in "Live" status.
- **FR-039**: System MUST preserve ContentProgress, QuizAttempt, and UserAnswer records when their associated content is deleted (historical integrity).
- **FR-040**: System MUST log all deletion operations in the ActivityLog for audit purposes.

### Key Entities

- **Video**: A video content item with provider info, quality, duration, processing status, and viewer engagement metrics (view count, comments).
- **Document**: A downloadable file resource (PDF, ZIP, DOC, PPT, Excel) with download tracking.
- **VideoComment**: A user comment on a video, supporting threaded replies via self-referencing parent. Supports soft delete.
- **CommentLike**: A like/reaction on a video comment. Prevents duplicates per user-comment pair.
- **Quiz**: An assessment with configurable settings (time limit, passing score, attempts, shuffling, result visibility).
- **Question**: A quiz question with type (MCQ, true/false, short answer), points, explanation, and ordering.
- **Option**: An answer option for a question with correctness flag and ordering.
- **QuizAttempt**: A student's quiz-taking session, tracking score, status, timing, and attempt number. Scoped to an enrollment.
- **UserAnswer**: An individual answer within a quiz attempt, recording the selected option or text and earned points.
- **LiveSession**: A scheduled or ad-hoc live meeting linked to a course, with status lifecycle and attendance tracking.
- **LiveAttendance**: A record of a student's participation in a live session, tracking duration.
- **Enrollment**: The association between a student and a course, tracking status, progress, source, and expiration.
- **ContentProgress**: Per-enrollment tracking of individual content item completion (polymorphic across Video, Quiz, Document, LiveSession).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Enrolled students can access and consume all content types (videos, documents, quizzes, live sessions) with response time under 500ms for content retrieval.
- **SC-002**: Progress tracking accuracy maintains 100% consistency—overall enrollment progress always reflects the true state of individual content completion.
- **SC-003**: Quiz grading produces accurate results within 2 seconds of submission for quizzes with up to 100 questions.
- **SC-004**: The system correctly blocks 100% of invalid deletion attempts (categories with courses, published courses with enrollments via hard delete, quizzes with in-progress attempts, live sessions).
- **SC-005**: All deletion operations generate audit log entries with 100% coverage, capturing the actor, entity, action, and timestamp.
- **SC-006**: Enrollment count on courses is accurate to within 1 second of enrollment/disenrollment events.
- **SC-007**: Certificate issuance occurs automatically within 30 seconds of a student reaching 100% completion on all mandatory items.
- **SC-008**: Live session attendance records are captured with join/leave timestamps accurate to within 5 seconds.

## Assumptions

- The existing 56 models (including Video, Quiz, Document, LiveSession, Enrollment, ContentProgress, etc.) are already defined in the codebase with correct schemas, relationships, and navigation properties. This feature spec covers the service logic, controllers, and business rules—not schema changes.
- The existing CourseEditApprovalService and EditPolicyHelper (from spec 002) are operational and will be extended to support deletion workflows for published course content.
- The existing NotificationService and EmailService will be used to notify students when content in their enrolled courses is deleted or modified.
- Video storage and streaming are handled by external providers (YouTube, Vimeo, Cloudflare Stream). This system manages metadata and references, not the actual video files.
- Quiz questions are auto-graded for multiple-choice and true/false types. Short-answer questions may require manual grading in a future iteration (out of scope for this spec).
- Live session meeting infrastructure (Zoom, Teams, etc.) is external. This system manages scheduling metadata, URLs, and attendance tracking.
- The platform's role-based access control (Admin, Instructor, Student) is already in place and functioning.
- A "completion threshold" for video watching defaults to 90% of video duration. This is a system-wide setting stored in SystemSettings and managed exclusively by admins (not configurable per-course).
- The SectionItem polymorphic relationship (ItemType + ItemId → Video/Quiz/Document/LiveSession) is already configured in the DbContext.
