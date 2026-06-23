# Feature Specification: Backend Modifications — Athary Platform

**Feature Branch**: `001-backend-modifications`  
**Created**: 2026-06-23  
**Status**: Draft  
**Input**: User description: "مراجعة المشروع في مجلد New والعمل على BACKEND_PLAN.md مع الاستعانة بـ MODIFICATION_GUIDE.md و FRONTEND_BACKEND_COMPARISON.md"

## Clarifications

### Session 2026-06-23

- Q: Testimonial submission limits? → A: One testimonial per user (update existing if resubmitted)
- Q: Legal page type extensibility? → A: Fixed enum (privacy, terms, refund only) — validated against hardcoded list
- Q: Contact message assignment workflow? → A: No assignment for v1 — remove AssignedToId, admins view all messages

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Public Instructor Profiles via Slug (Priority: P1)

As a visitor, I want to access instructor profiles using a human-readable URL (e.g., `/instructors/ahmed-ali`) instead of GUID-based URLs, so that I can easily share and remember instructor profile links.

**Why this priority**: This is foundational for the public-facing instructor pages and is required before the frontend can properly display instructor profiles.

**Independent Test**: Can be fully tested by creating a new instructor with a slug and accessing their profile via the slug URL. Delivers value by enabling public instructor profiles.

**Acceptance Scenarios**:

1. **Given** an instructor exists with slug "ahmed-ali", **When** a visitor requests `GET /api/public/instructors/ahmed-ali`, **Then** the system returns the instructor's public profile
2. **Given** an instructor does not exist with slug "nonexistent", **When** a visitor requests `GET /api/public/instructors/nonexistent`, **Then** the system returns 404 Not Found
3. **Given** a logged-in instructor, **When** they check slug availability for "ahmed-ali", **Then** the system indicates whether the slug is available
4. **Given** a visitor searches for instructors, **When** they use the search endpoint with query "ahmed", **Then** the system returns matching instructors with their slugs

---

### User Story 2 - Landing Page Aggregated Endpoint (Priority: P1)

As a frontend developer, I want a single API endpoint that returns all data needed for the landing page (stats, featured courses, categories, upcoming live sessions, testimonials), so that the page loads with a single request instead of multiple parallel requests.

**Why this priority**: This reduces network overhead and improves page load performance, which is critical for the main landing page that all visitors see first.

**Independent Test**: Can be tested by calling the landing endpoint and verifying all sections of data are returned. Delivers value by reducing page load requests from 5+ to 1.

**Acceptance Scenarios**:

1. **Given** the platform has courses, categories, and testimonials, **When** a visitor requests `GET /api/public/landing`, **Then** the system returns all landing page data in a single response
2. **Given** the database is temporarily unavailable, **When** a visitor requests the landing data, **Then** the system returns default empty data with a logged error instead of failing
3. **Given** the landing data is cached, **When** multiple requests arrive within 5 minutes, **Then** the cached version is returned without hitting the database

---

### User Story 3 - Testimonials Management (Priority: P2)

As an admin, I want to manage student testimonials (approve, flag, reorder, delete), so that I can curate the testimonials displayed on the landing page.

As a student, I want to submit testimonials about the platform, so that I can share my experience with others.

**Why this priority**: Testimonials are important social proof for the landing page but are secondary to core functionality.

**Independent Test**: Can be tested by creating, approving, flagging, and reordering testimonials. Delivers value by enabling social proof on the landing page.

**Acceptance Scenarios**:

1. **Given** a logged-in student with no existing testimonial, **When** they submit a testimonial with content and rating, **Then** the testimonial is created with IsApproved=false
2. **Given** a logged-in student with an existing testimonial, **When** they submit a new testimonial, **Then** the existing testimonial is updated (not duplicated)
3. **Given** a logged-in admin, **When** they view all testimonials, **Then** they can see pending, approved, and flagged testimonials
4. **Given** a logged-in admin, **When** they approve a testimonial, **Then** it becomes visible on the public testimonials endpoint
5. **Given** a logged-in admin, **When** they flag a testimonial with a reason, **Then** it is marked as flagged and hidden from public view
6. **Given** the platform is being set up, **When** the TestimonialSeeder runs, **Then** Arabic testimonials are seeded for demonstration

---

### User Story 4 - Notification Preferences (Priority: P2)

As a user, I want to customize which notifications I receive (email, push, course updates, marketing), so that I only get notifications that matter to me.

**Why this priority**: Notification preferences improve user experience and reduce notification fatigue.

**Independent Test**: Can be tested by updating notification preferences and verifying changes persist. Delivers value by giving users control over their notification experience.

**Acceptance Scenarios**:

1. **Given** a logged-in user, **When** they request their notification preferences, **Then** the system returns their current settings
2. **Given** a logged-in user, **When** they update their notification preferences (e.g., disable marketing emails), **Then** the changes are saved and reflected in future requests
3. **Given** a new user registers, **When** their account is created, **Then** default notification preferences are automatically created for them
4. **Given** existing users without preferences, **When** the migration runs, **Then** default preferences are seeded for all active users

---

### User Story 5 - Contact Form (Priority: P2)

As a visitor, I want to send a message to the platform via a contact form, so that I can ask questions or report issues.

As an admin, I want to view and manage incoming contact messages, so that I can respond to user inquiries.

**Why this priority**: Contact form is essential for user support and communication.

**Independent Test**: Can be tested by submitting a contact message and verifying it's stored and visible to admins. Delivers value by enabling user-platform communication.

**Acceptance Scenarios**:

1. **Given** a visitor fills out the contact form with valid data, **When** they submit the form, **Then** the message is saved and a success response is returned
2. **Given** a visitor submits a message with spam keywords (e.g., "viagra", "casino"), **When** the system processes the request, **Then** the message is rejected with an error
3. **Given** a visitor submits more than 5 messages in one minute, **When** the system processes the request, **Then** rate limiting is applied and the request is rejected
4. **Given** a logged-in admin, **When** they view contact messages, **Then** they can see all messages filtered by read/unread status
5. **Given** a logged-in admin, **When** they mark a message as read, **Then** the message's IsRead status is updated

---

### User Story 6 - About Page API (Priority: P3)

As a frontend developer, I want an API endpoint that returns about page content, so that the about page can display dynamic content from the backend.

**Why this priority**: About page content is relatively static and can be served from the frontend initially.

**Independent Test**: Can be tested by requesting the about endpoint and verifying content is returned. Delivers value by enabling dynamic about page content.

**Acceptance Scenarios**:

1. **Given** about content exists in system settings, **When** a visitor requests `GET /api/public/about`, **Then** the system returns the about page content
2. **Given** about content is cached, **When** multiple requests arrive within 1 hour, **Then** the cached version is returned

---

### User Story 7 - Legal Pages API (Priority: P3)

As a frontend developer, I want API endpoints for legal pages (privacy policy, terms of service, refund policy), so that legal content can be managed from the backend.

**Why this priority**: Legal pages are important for compliance but can be served statically initially.

**Independent Test**: Can be tested by requesting legal page content and verifying it's returned correctly. Delivers value by enabling dynamic legal page management.

**Acceptance Scenarios**:

1. **Given** a legal page of type "privacy" exists and is published, **When** a visitor requests `GET /api/public/legal/privacy`, **Then** the system returns the privacy policy content
2. **Given** a legal page of type "terms" does not exist, **When** a visitor requests `GET /api/public/legal/terms`, **Then** the system returns 404 Not Found
3. **Given** an invalid legal page type (e.g., "invalid"), **When** a visitor requests `GET /api/public/legal/invalid`, **Then** the system returns 400 Bad Request

---

### Edge Cases

- What happens when an instructor's first name and last name are both empty when generating a slug?
- What happens when the landing page database query takes longer than expected?
- What happens when a testimonial content exceeds 1000 characters?
- What happens when a user tries to create a duplicate notification preference entry?
- What happens when a contact message contains both valid content and spam keywords?
- What happens when the slug contains special characters or non-ASCII characters?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST allow public access to instructor profiles via slug-based URLs at `/api/public/instructors/{slug}`
- **FR-002**: System MUST generate unique slugs from instructor names using FirstName + LastName format
- **FR-003**: System MUST support slug availability checking for logged-in users
- **FR-004**: System MUST support instructor search by slug or name
- **FR-005**: System MUST provide a single aggregated landing endpoint at `/api/public/landing` returning stats, featured courses, categories, upcoming live sessions, and testimonials
- **FR-006**: System MUST cache landing data for 5 minutes to reduce database load
- **FR-007**: System MUST return default empty data if the database query fails for landing data
- **FR-008**: System MUST allow authenticated users to create testimonials with content and rating (one per user; submitting again updates the existing testimonial)
- **FR-009**: System MUST allow admins to approve, flag, reorder, and delete testimonials
- **FR-010**: System MUST seed Arabic testimonials for initial platform setup
- **FR-011**: System MUST store user notification preferences with default values
- **FR-012**: System MUST allow authenticated users to retrieve and update their notification preferences
- **FR-013**: System MUST automatically create default notification preferences for new users
- **FR-014**: System MUST accept contact form submissions from anonymous users
- **FR-015**: System MUST detect and reject spam messages containing predefined keywords
- **FR-016**: System MUST apply rate limiting of 5 requests per minute for contact form submissions
- **FR-017**: System MUST allow admins to view, mark as read, and delete contact messages (no assignment workflow in v1)
- **FR-018**: System MUST provide about page content from system settings
- **FR-019**: System MUST serve legal pages (privacy, terms, refund) with type-based routing using a fixed enum
- **FR-020**: System MUST validate legal page types against the fixed enum (privacy, terms, refund) and return 400 for invalid types
- **FR-021**: System MUST support slug generation with Arabic character support
- **FR-022**: System MUST use DataAnnotations for DTO validation (consistent with existing project style)

### Key Entities

- **User**: Extended with Slug field for instructor profiles; FirstName + LastName used for slug generation
- **Testimonial**: Content, Rating (1-5), IsApproved, IsFlagged, FlagReason, DisplayOrder; linked to User (one testimonial per user, unique constraint on UserId)
- **NotificationPreference**: Per-user notification settings (email, push, course updates, marketing, etc.); one-to-one with User
- **ContactMessage**: FullName, Email, Phone, Subject, Message, IsRead; stored for admin management (no assignment in v1)
- **LegalPage**: Type (fixed enum: privacy/terms/refund), Title, Content, IsPublished, Version, LastUpdatedById; versioned legal content

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can access any instructor profile via a human-readable URL without needing to know the instructor's GUID
- **SC-002**: Landing page loads with a single API request instead of 5+ parallel requests
- **SC-003**: Admins can approve or reject testimonials within 30 seconds of opening the admin panel
- **SC-004**: Users can update their notification preferences and see changes reflected immediately
- **SC-005**: Contact form submissions are processed in under 2 seconds including spam detection
- **SC-006**: Rate limiting prevents more than 5 contact form submissions per minute from the same IP
- **SC-007**: Legal page content is served within 1 second with caching
- **SC-008**: All new endpoints follow the existing ApiResponse wrapper pattern for consistent API responses
- **SC-009**: Database migrations run cleanly and seed data is applied for existing users
- **SC-010**: System handles edge cases gracefully (empty names, invalid types, duplicate entries)

## Assumptions

- The existing Backend project structure (Clean Architecture with Domain, Application, Infrastructure, API layers) will be followed
- The existing patterns for Controllers, Services, Repositories, and DTOs will be maintained
- DataAnnotations will be used for validation (consistent with existing project style)
- PostgreSQL database with Entity Framework Core will continue to be used
- The existing ApiResponse wrapper pattern will be used for all new endpoints
- Existing seed data mechanisms will be reused for new seeders
- The IMemoryCache pattern already in use will be applied to new caching requirements
- Rate limiting will use the existing ASP.NET Core rate limiting infrastructure
- SignalR hubs already in the project will not be modified in this feature
- Frontend modifications are out of scope for this specification
