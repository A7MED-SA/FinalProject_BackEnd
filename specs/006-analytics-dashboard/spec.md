# Feature Specification: Analytics Dashboard

## Clarifications

### Session 2026-05-24

- Q: Should dashboard data be real-time, cached, or hybrid? → A: Hybrid — current-moment stats (counts, progress) are real-time; historical trends (revenue, user growth) are recomputed nightly and cached.
- Q: What happens when a data source is temporarily unavailable? → A: Degraded mode — return successfully computed stats with an error indicator for the failed section.
- Q: How should learning hours treat non-video content (quizzes, documents)? → A: Estimated platform minutes — video uses actual watch time; quizzes and documents use standard platform durations.
- Q: Should revenue dashboard access be audit-logged? → A: Yes — log all revenue access (both admin and instructor).
- Q: What should be explicitly marked as out-of-scope? → A: Frontend UI, real-time activity tracking, mobile-specific dashboards, data export, and notification triggers.

**Feature Branch**: `006-analytics-dashboard`  
**Created**: 2026-05-24  
**Status**: Draft  
**Input**: User description: "Dashboard & Analytics — role-based dashboards for Students, Teachers, and Admins with aggregated stats, progress tracking, revenue analytics, and platform insights"

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Student Progress Dashboard (Priority: P1)

A student visits their dashboard and sees an overview of their learning journey: total enrolled courses, in-progress courses, completed courses, total learning hours, and certificates earned. They can view a list of their courses with progress bars and see their recent activity.

**Why this priority**: Core user-facing feature that drives engagement — every student needs visibility into their progress. Existing enrollment and progress APIs can be reused, making this the fastest to deliver.

**Independent Test**: Enroll a student in 3 courses, complete 1 fully, partially progress another. Verify the dashboard returns correct counts (3 enrolled, 1 completed, 1 in-progress) and total learning hours from the completed course.

**Acceptance Scenarios**:

1. **Given** a student is enrolled in multiple courses with varying progress, **When** they request their dashboard data, **Then** they receive accurate counts for enrolled, in-progress, and completed courses plus total learning hours aggregated across all enrollments.
2. **Given** a student has no enrollments, **When** they request their dashboard data, **Then** they receive zero values for all stats with no errors.
3. **Given** a student has completed a course and earned a certificate, **When** they request their dashboard, **Then** the certificate count includes that certificate.

---

### User Story 2 — Teacher Instructor Dashboard (Priority: P1)

A teacher visits their dashboard and sees an overview of their teaching: total students across all their courses, number of active courses, total teaching hours, average rating across courses, and revenue data. They can view a list of their courses with enrollment counts and ratings, pending edit requests, and recent reviews.

**Why this priority**: Essential instructor tool — teachers need to track their course performance and student engagement. Requires new aggregation endpoints to summarize data across multiple courses.

**Independent Test**: A teacher has 3 published courses with varying enrollments. Verify the dashboard returns correct total students (sum of enrollments), active course count (3), and average rating across all courses.

**Acceptance Scenarios**:

1. **Given** a teacher has multiple courses with enrollments, **When** they request their dashboard, **Then** they receive aggregated stats: total students (sum of all enrollments), active courses count, average rating, and total teaching hours.
2. **Given** a teacher has no courses, **When** they request their dashboard, **Then** they receive zero values for all stats.
3. **Given** a teacher has pending edit requests, **When** they request their dashboard, **Then** pending edit requests are included in the response.

---

### User Story 3 — Admin Platform Dashboard (Priority: P2)

An admin visits their dashboard and sees a platform-wide overview: total users, total courses, total revenue, pending course approvals, pending teacher requests, and active instructors. They can view user growth trends over time and monthly revenue breakdowns.

**Why this priority**: Important for platform management but less urgent than student/teacher experiences. Requires the most new backend aggregation logic and potentially new data models for analytics.

**Independent Test**: The platform has 1000 users, 50 courses, 10 instructors, and 5 pending approvals. Verify the admin dashboard returns correct totals for each metric.

**Acceptance Scenarios**:

1. **Given** the platform has users, courses, and financial data, **When** an admin requests the overview stats, **Then** they receive correct totals for users, courses, revenue, pending approvals, and instructors.
2. **Given** the platform has 12 months of revenue data, **When** an admin requests monthly revenue breakdown, **Then** they receive a list of months with corresponding revenue amounts.
3. **Given** the platform has 6 months of user signup data, **When** an admin requests user growth trends, **Then** they receive a list of months with new user counts and cumulative totals.

---

## Out of Scope

The following are explicitly excluded from this feature:
- **Frontend dashboard UI** — This spec covers only backend API endpoints. Frontend implementation (React components, charts, layouts) is a separate effort.
- **Real-time activity tracking** — Daily active users, live concurrent viewers, or real-time notifications require infrastructure investments beyond this feature.
- **Mobile-specific dashboards** — The API serves all clients equally. No separate mobile backend endpoints are needed.
- **Data export** — Downloading dashboard data as CSV/Excel is not included.
- **Push notifications or alerts** — Dashboard insights triggering notifications (e.g., "your course reached 100 students") is separate scope.

### Edge Cases

- What happens when a teacher course has zero enrollments? Stats should return 0 not errors.
- What happens when enrollment data contains refunded students? Revenue stats should exclude refunded amounts. Student counts should exclude refunded enrollments.
- What happens when the platform is new with minimal data? All stats should return 0 or empty arrays gracefully.
- What happens when an admin has no access to revenue data? The revenue endpoint should respect role-based access.
- What happens when the order/revenue service is temporarily unavailable? The dashboard returns computed stats from available sources plus a degradation indicator.
- How often is historical dashboard data (revenue trends, user growth, enrollment trends) refreshed? Historical trend data is recomputed nightly (off-peak) and cached. Current-moment stats (enrollment counts, progress) are queried in real-time.

## Requirements *(mandatory)*

### Functional Requirements

#### Student Dashboard API

- **FR-001**: System MUST provide a dashboard data endpoint for authenticated students that returns their aggregated learning stats.
- **FR-002**: The student dashboard MUST include: total enrolled courses count, in-progress courses count, completed courses count, total learning hours (estimated: video content uses actual watch time; quizzes and documents use standard platform durations), and certificates earned count.
- **FR-003**: System MUST return a list of the student's recent enrollments with course title, progress percentage, and last accessed timestamp.
- **FR-004**: System MUST return a list of enrollments eligible for certificates (completed courses where no certificate exists yet).

#### Teacher Dashboard API

- **FR-005**: System MUST provide a dashboard data endpoint for authenticated instructors that returns aggregated teaching stats.
- **FR-006**: The teacher dashboard MUST include: total students across all courses, number of published courses, total teaching hours (sum of all course durations), average rating across courses, and total revenue (instructor's share from completed orders).
- **FR-007**: System MUST return a list of the teacher's courses with enrollment count, average rating, and approval status for each.
- **FR-008**: System MUST return a list of pending edit requests for the teacher's courses.

#### Admin Dashboard API

- **FR-009**: System MUST provide a platform overview dashboard endpoint for admins returning platform-wide stats.
- **FR-010**: The admin overview MUST include: total users, total courses, total revenue, pending course approvals count, pending teacher requests count, and active instructors count.
- **FR-011**: System MUST provide a monthly revenue breakdown endpoint for admins covering the last 12 months.
- **FR-012**: System MUST provide a user growth endpoint for admins returning monthly signup data for the last 6 months.
- **FR-013**: System MUST provide an enrollment trends endpoint for admins returning enrollment counts over time.

#### Common Requirements

- **FR-014**: All dashboard endpoints MUST respect role-based access — students cannot access instructor endpoints, instructors cannot access admin endpoints.
- **FR-015**: All dashboard endpoints MUST return deterministic zero values (not null or missing) when no data exists.
- **FR-016**: Revenue calculations MUST provide both gross (total paid minus refunds) and net (after platform commission) values. The instructor dashboard MUST display net revenue as the primary metric. Gross revenue is available for admin comparison.
- **FR-017**: Instructor revenue share MUST be configurable per-instructor (a per-instructor negotiated rate). Each instructor has a negotiated revenue percentage stored in their profile. Revenue calculations use this percentage to compute the instructor's net share.
- **FR-018**: System MUST provide a new dedicated endpoint for instructors to list their own courses with enrollment counts and ratings. This MUST be separate from the public course browsing endpoint to securely expose private instructor data (drafts, pending reviews, rejection reasons) without impacting the public API surface.
- **FR-019**: When a sub-aggregation fails (e.g., revenue service unavailable), the dashboard endpoint MUST return successfully computed stats with an error indicator for the failed section. The frontend can display a degradation warning.
- **FR-020**: All access to dashboard endpoints that include revenue/earnings data MUST be logged in the activity log — both admin and instructor accesses.
- **FR-021**: Instructor revenue share percentage MUST be stored as a per-instructor field in the instructor profile and be configurable by admin.

### Key Entities *(include if feature involves data)*

- **StudentDashboardDto**: Aggregated student stats (enrolled, in-progress, completed courses counts; total learning hours; certificates count) plus lists of recent enrollments and certificate-eligible courses.
- **InstructorDashboardDto**: Aggregated instructor stats (total students, published courses count, total hours, average rating, total revenue) plus lists of courses with enrollment/rating data and pending edit requests.
- **AdminOverviewDto**: Platform-wide stats (total users, courses, revenue, pending approvals, pending teacher requests, active instructors).
- **MonthlyRevenueDto**: Month label and revenue amount for revenue trend charts.
- **UserGrowthDto**: Month label, new users count, and cumulative total for growth trend charts.
- **ManagementCourseDto**: Course summary for instructors including enrollment count, average rating, and approval status.
- **RevenueAggregate**: A service-level concept for computing instructor and platform revenue from the existing Order/Payment/Refund system.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Students can view their learning dashboard with all stats populating in under 2 seconds for accounts with up to 50 enrollments.
- **SC-002**: Teachers can view their instructor dashboard with all stats aggregating in under 3 seconds for accounts with up to 20 courses.
- **SC-003**: Admins can view the platform overview dashboard with all global stats in under 5 seconds for platforms with up to 100,000 users.
- **SC-004**: Instructors can view a list of their courses with enrollment counts for the first time without requiring a separate API call.
- **SC-005**: 90% of dashboard data requests return successfully on the first attempt without timeouts or errors.
- **SC-006**: Revenue data in admin and instructor dashboards matches the existing Order/Refund system totals within 0.01 currency unit tolerance.

## Assumptions

- Existing authentication and role system (JWT + Identity) will be reused for all dashboard endpoints.
- Existing Enrollment, ContentProgress, Course, Order, Payment, and Refund models contain sufficient data to compute all required aggregations — no new data models are needed for base metrics.
- Student dashboard data can be derived from existing `/api/enrollments` and progress endpoints — no new endpoints needed for student stats beyond an aggregation wrapper.
- Revenue data will be computed from existing Order records, excluding refunded amounts.
- Instructor revenue share is configured per-instructor as a negotiated percentage stored in the user/instructor profile.
- Performance targets assume database indexes exist on key query columns (UserId, CourseId, Status, CreatedAt).
- Dashboard data does not need to be cached for MVP — real-time queries are acceptable.
- The existing user activity logging system does not have granular enough data for daily active user tracking — this is deferred.
- Dashboard trend data (revenue, user growth, enrollments) is aggregated and served by the platform, not computed by frontend from individual records.
- Current-moment stats (enrollment counts, progress percentages) are queried in real-time from source tables. Historical trend data (revenue by month, user growth) is recomputed nightly and cached.
- All dashboard endpoint accesses that include revenue data are logged to the activity log for audit purposes.
