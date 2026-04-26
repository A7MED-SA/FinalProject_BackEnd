# Feature Specification: course-features

**Feature Branch**: `[002-course-features]`  
**Created**: 2026-04-25  
**Status**: Draft  
**Input**: User description: "$ARGUMENTS"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Course Edit Approval Workflow (Priority: P1)

As an instructor, I want to edit parts of my published course (such as content, sections, or pricing), so that my course stays up to date. For major or sensitive changes (like deleting videos or changing prices significantly), I expect the system to require an Admin's approval before the changes are visible to students.

**Why this priority**: Protecting the quality and consistency of published courses is critical to the student experience and the platform's reputation.

**Independent Test**: Can be tested by having an instructor submit an edit on a published course. Low-risk edits should apply immediately, while high-risk edits should create a pending request that isn't visible to students until approved.

**Acceptance Scenarios**:

1. **Given** a published course, **When** an instructor changes the course description (low risk), **Then** the change is applied immediately and visible to students.
2. **Given** a published course, **When** an instructor changes a video or the price by more than 10% (high risk), **Then** a pending edit request is created, and the original course remains unchanged for students.

---

### User Story 2 - Reviewing Edit Requests (Priority: P1)

As an admin, I want to view a list of pending course edit requests, see what was changed (old vs. new values), and approve or reject them so that I can maintain quality control over published content.

**Why this priority**: Essential counterpart to Story 1. The approval workflow cannot function without admins being able to review and action the requests.

**Independent Test**: Can be tested by logging in as an admin, viewing pending requests, and approving one, then verifying the course is updated.

**Acceptance Scenarios**:

1. **Given** a pending edit request, **When** an admin approves it, **Then** the changes are merged into the live course and the instructor is notified.
2. **Given** a pending edit request, **When** an admin rejects it with notes, **Then** the live course is unchanged and the instructor is notified with the reason.

---

### User Story 3 - Public Course Browsing (Priority: P2)

As a visitor or student, I want to browse a catalog of published courses, search by keywords, and filter by category, level, and price, so that I can find a course that fits my needs.

**Why this priority**: This is the primary discovery mechanism for the platform, driving enrollments and revenue.

**Independent Test**: Can be tested by visiting the public API endpoints without authentication and verifying that only published courses are returned, and filters work correctly.

**Acceptance Scenarios**:

1. **Given** a mix of draft and published courses, **When** a user views the public catalog, **Then** only published courses are displayed.
2. **Given** the public catalog, **When** a user filters by "Beginner" level and a specific category, **Then** only matching published courses are returned.

---

### User Story 4 - Public Course Details (Priority: P2)

As a visitor or student, I want to view detailed information about a specific course (such as its description, learning outcomes, and section outline), without being able to access the actual protected learning content (e.g., video files) until I enroll.

**Why this priority**: Helps students make informed purchasing decisions while protecting intellectual property.

**Independent Test**: Can be tested by accessing a course's public details page and verifying that metadata is present but protected video URLs or quiz questions are not exposed.

**Acceptance Scenarios**:

1. **Given** a published course with video lessons, **When** a public user views the course details, **Then** they can see the section titles and duration, but cannot access the video files.
2. **Given** an unpublished (draft) course, **When** a public user attempts to view its details, **Then** the system returns a "Not Found" or "Unauthorized" response.

### Edge Cases

- What happens when a pending edit request sits unreviewed for a long time? (The system should automatically expire it after a set duration, e.g., 7 days).
- What happens if an instructor submits an edit request, but then changes their mind? (The instructor should be able to cancel their pending request).
- What happens if an edit involves deleting a section that active students are currently viewing? (The system must evaluate student impact, but ultimately the admin decides whether to approve the deletion).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST allow instructors to edit properties, sections, and items of their own courses.
- **FR-002**: System MUST automatically evaluate edits to published courses against an "Edit Policy" to determine risk level (Low, Medium, High, Critical).
- **FR-003**: System MUST automatically apply "Low" risk edits without requiring admin approval.
- **FR-004**: System MUST create a pending `CourseEditRequest` for edits that require approval, leaving the live course unchanged.
- **FR-005**: System MUST notify Admins (via email and in-app) when a new edit request is submitted.
- **FR-006**: System MUST allow Admins to view pending edit requests, including a diff (Old Value vs. New Value) of the changes.
- **FR-007**: System MUST allow Admins to approve or reject edit requests, providing optional review notes.
- **FR-008**: System MUST apply approved edits to the live course, increment its version, and notify the instructor.
- **FR-009**: System MUST record all edit lifecycle events in the `CourseLog` and `ActivityLog` for auditing and transparency.
- **FR-010**: System MUST automatically expire and reject edit requests that are older than the configured expiration period.
- **FR-011**: System MUST expose public endpoints to retrieve a paginated list of published courses, supporting search queries and filters (Category, Level, Price, etc.).
- **FR-012**: System MUST expose public endpoints to retrieve detailed information for a specific course (by ID or Slug) without exposing protected learning content.
- **FR-013**: System MUST NOT expose any draft, pending, or archived courses through the public endpoints.

### Key Entities

- **CourseEditRequest**: Represents a pending or resolved request to change a published course. Contains the target course, the requester, the payload of changes, the status, and the reviewer.
- **CourseLog / ActivityLog**: Audit entities that track the history of actions on a course, including when edit requests are submitted, approved, or rejected.
- **PublicCourse / PublicCourseDetail**: Read-only projections of Course data tailored for unauthenticated public consumption, strictly omitting protected content.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of high-risk course edits (as defined by the Edit Policy) are blocked from public view until explicitly approved by an Admin.
- **SC-002**: The public course listing API responds in under 500ms on average, even with complex filtering and pagination applied.
- **SC-003**: Instructors and Admins receive related notifications within 1 minute of a relevant event occurring (submission, approval, rejection).
- **SC-004**: Audit logs capture 100% of all edit requests and administrative reviews with accurate timestamps and actor identities.

## Assumptions

- Existing NotificationService and EmailService can be extended to support these new notification types.
- The platform uses a role-based access control system with defined "Instructor" and "Admin" roles.
- Frontend interfaces will handle the display of course data and the review dashboard; this spec covers the system capabilities and rules.
- Only course creators (instructors) can submit edit requests for their own courses.
