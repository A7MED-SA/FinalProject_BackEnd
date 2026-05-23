# Feature Specification: Reviews & Certificates

**Feature Branch**: `005-reviews-certificates`  
**Created**: 2026-05-23  
**Status**: Draft  
**Input**: User description: "عايز اعمل Reviews & Certificates بس مش عارف ايه فايدة ده - `GET /api/certificates/verify/{code}` — تحقق عام من شهادة"

## User Scenarios & Testing

### User Story 1 - Course Reviews & Ratings (Priority: P1)

Students who have completed a course can leave a star rating (1–5) and an optional written review. Reviews appear on the course page so future students can make informed enrollment decisions. Students can edit or delete their own reviews.

**Why this priority**: Reviews build social proof, the most immediate value for the platform. Without reviews, new students have no way to gauge course quality from peers.

**Independent Test**: Can be tested by enrolling a student in a course, completing it, submitting a 4-star review with text, verifying it appears on the course page, then editing the rating and confirming the update.

**Acceptance Scenarios**:

1. **Given** a student who has completed a course, **When** they submit a review with a 4-star rating and text, **Then** the review is saved and immediately visible on the course page with the student's name and timestamp.
2. **Given** a student who previously submitted a review, **When** they edit the rating from 4 to 5 stars, **Then** the review updates and shows a last-edited timestamp.
3. **Given** a student who submitted a review, **When** they delete it, **Then** the review is removed from the course page.
4. **Given** a student who has NOT completed a course, **When** they attempt to review it, **Then** the system rejects the request with a clear message that course completion is required.
5. **Given** a student viewing a course page with 10+ reviews, **When** they scroll, **Then** reviews are paginated (10 per page) and the average rating is displayed at the top.

---

### User Story 2 - Course Completion Certificates (Priority: P1)

When a student completes all requirements of a course (all sections, required quizzes), a certificate is automatically generated. Each certificate has a unique verification code (e.g., `CERT-XXXX-XXXX-XXXX`) and includes the student's name, course title, completion date, and the verification code.

**Why this priority**: Certificates provide tangible proof of achievement, motivating students to complete courses and giving them shareable credentials.

**Independent Test**: Can be tested by enrolling a student, completing all course content, and verifying that a certificate record with a unique code is generated and accessible to the student.

**Acceptance Scenarios**:

1. **Given** a student who has completed all requirements of a course, **When** the system processes the completion, **Then** a certificate is automatically generated with a unique verification code and the student can view it immediately.
2. **Given** a student who has NOT completed a course, **When** they try to access a certificate for that course, **Then** the system returns "not found" or "not yet earned."
3. **Given** a certificate has been generated, **When** any student or instructor views it, **Then** it displays the student's full name, course title, completion date, and the unique verification code.

---

### User Story 3 - Public Certificate Verification (Priority: P2)

Anyone — including employers, other platforms, or logged-out visitors — can verify a certificate's authenticity by entering its unique verification code. The verification page shows the certificate holder's name, course title, completion date, and whether the certificate is currently valid.

**Why this priority**: External verification is the core trust mechanism. Employers and credential checkers need a way to confirm a certificate is genuine without logging in.

**Independent Test**: Can be tested by entering a valid certificate code on the public verification page and confirming the correct details appear, then entering an invalid code and seeing a "not found" message.

**Acceptance Scenarios**:

1. **Given** a valid certificate code, **When** a visitor enters it on the public verification page (e.g., `GET /certificates/verify/{code}`), **Then** they see the certificate holder's name, course title, completion date, and a "Valid Certificate" status.
2. **Given** a fake or non-existent certificate code, **When** a visitor enters it, **Then** they see a clear "Certificate not found" message.
3. **Given** a certificate that was revoked (e.g., due to a refund), **When** a visitor verifies its code, **Then** they see the holder's details plus a "Revoked" status with the revocation date.

---

### User Story 4 - Certificate Sharing & Download (Priority: P3)

Students can view a list of all their earned certificates, download each as a shareable file (e.g., PDF), and share a direct link to the public verification page. [NEEDS CLARIFICATION: How should the certificate be shareable — as a downloadable PDF, as a link-based share, or both? If PDF, should it include a background design/logo or be a plain text document?]

**Why this priority**: Sharing enables students to showcase achievements on LinkedIn, CVs, and professional profiles — increasing the platform's visibility and the certificate's value.

**Independent Test**: Can be tested by having a student navigate to their certificates list, selecting a certificate, and downloading it or copying a verification link.

**Acceptance Scenarios**:

1. **Given** a student with multiple completed courses, **When** they navigate to their certificates page, **Then** they see a list of all earned certificates ordered by completion date (most recent first).
2. **Given** a student viewing a certificate, **When** they choose to download it, **Then** the system generates and serves the file.
3. **Given** a student viewing a certificate, **When** they copy the verification link, **Then** the link directs to the public verification page with their certificate's unique code.

---

### Edge Cases

- What happens if a student's enrollment is refunded? The certificate should be revoked, and the public verification page should show "Revoked" status.
- Can the same certificate code be generated for two different students? No — each certificate's verification code must be globally unique.
- What happens if a course is deleted or hidden after certificates were issued? Existing certificates should remain valid and verifiable.
- Can an admin manually revoke a certificate? [NEEDS CLARIFICATION: Should admins have the ability to manually revoke certificates (e.g., for academic dishonesty), or is revocation only triggered automatically by refunds?]
- What rating is displayed when a course has no reviews? Display "No reviews yet" instead of a 0-star average.

## Requirements

### Functional Requirements

- **FR-001**: System MUST allow enrolled students who have completed a course to submit a star rating (1–5) and optional written review.
- **FR-002**: System MUST allow students to edit or delete their own reviews at any time.
- **FR-003**: System MUST display the average rating and total review count on the course page, along with individual reviews (paginated).
- **FR-004**: System MUST prevent students from reviewing a course they have not completed, or reviewing the same course more than once.
- **FR-005**: System MUST automatically generate a certificate with a unique verification code when a student completes all course requirements.
- **FR-006**: System MUST provide a public verification endpoint that accepts a certificate code and returns the holder's name, course title, completion date, and validity status.
- **FR-007**: System MUST return "Certificate not found" for non-existent or invalid codes on the verification endpoint.
- **FR-008**: System MUST revoke a certificate when the corresponding enrollment is refunded, and show "Revoked" status on verification.
- **FR-009**: Students MUST be able to view a list of all their earned certificates.
- **FR-010**: Students MUST be able to download their certificates as a shareable file.
- **FR-011**: System MUST provide a shareable link for each certificate pointing to the public verification page.
- **FR-012**: Existing certificates MUST remain valid and verifiable even if the course is later unpublished.

### Key Entities

- **Review**: Represents a student's rating and feedback for a course they completed. Associated with one student and one course. Contains a star rating (1–5), optional written text, and timestamps for creation and last edit.
- **Certificate**: Represents proof of course completion for a student. Associated with one student and one course. Contains a unique verification code, completion date, and a status (valid / revoked). Can be shared externally via the public verification endpoint.

## Success Criteria

### Measurable Outcomes

- **SC-001**: Students can submit a course review in under 1 minute from the course page.
- **SC-002**: Certificates are generated within 5 seconds of course completion being registered.
- **SC-003**: Certificate verification returns results in under 1 second for 99% of requests.
- **SC-004**: Students can locate and download any of their certificates in 2 clicks or fewer.
- **SC-005**: Public verification works without any authentication — any visitor can verify a code.
- **SC-006**: Revoked certificates are detected and displayed as revoked in the verification response immediately after the refund is processed.

## Assumptions

- Course completion is determined by the existing enrollment/completion tracking system (all sections viewed, required quizzes passed).
- Students are already authenticated via the platform's existing authentication system.
- Certificate files (PDFs) will use a simple template with the platform logo, student name, course title, and completion date — custom branding is out of scope for v1.
- The unique verification code format will use a human-readable format (e.g., `CERT-XXXX-XXXX-XXXX`) rather than a raw UUID, for ease of manual entry.
- Mobile app support is out of scope — this feature targets the web platform only.
