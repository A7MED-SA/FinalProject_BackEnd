# Tasks: Reviews & Certificates

**Input**: Design documents from `/specs/005-reviews-certificates/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Not explicitly requested in spec — test tasks omitted. Add later via `/speckit.checklist` if needed.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Backend**: `backend_project/` (existing ASP.NET Core project)
- **Tests**: `tests/CommerceTests/`

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Model creation and DI registration shared across all stories

- [X] T001 [P] Create Review model in `backend_project/Models/Review.cs`
- [X] T002 [P] Create Certificate model in `backend_project/Models/Certificate.cs` with CertificateStatus enum
- [X] T003 [P] Create ReviewDtos in `backend_project/DTOs/Review/ReviewDtos.cs`
- [X] T004 [P] Create CertificateDtos in `backend_project/DTOs/Certificate/CertificateDtos.cs`
- [X] T005 [P] Create IReviewService interface in `backend_project/Services/Interfaces/IReviewService.cs`
- [X] T006 [P] Create ICertificateService interface in `backend_project/Services/Interfaces/ICertificateService.cs`
- [X] T007 Register `DbSet<Review>` and `DbSet<Certificate>` with entity config in `backend_project/Data/ApplicationDbContext.cs`

---

## Phase 2: User Story 1 — Course Reviews & Ratings (Priority: P1) 🎯

**Goal**: Students who completed a course can submit/edit/delete reviews with star ratings and optional text. Reviews appear publicly on course pages. Instructors can flag reviews; admins can moderate flagged reviews.

**Independent Test**: Enroll a student, complete the course, submit a 4-star review, verify it appears on the course page, edit to 5 stars, verify update, delete, verify removal.

### Implementation for User Story 1

- [X] T008 [P] [US1] Create CreateReviewValidator in `backend_project/Validators/Review/CreateReviewValidator.cs`
- [X] T009 [US1] Implement ReviewService in `backend_project/Services/Implementations/ReviewService.cs`
- [X] T010 [US1] Create ReviewsController in `backend_project/Controllers/ReviewsController.cs` (CRUD endpoints + flag)
- [X] T011 [US1] Create admin review moderation controller or add admin actions to existing controller
- [X] T012 [US1] Register `IReviewService` in DI in `backend_project/Program.cs`

**Checkpoint**: Students can submit, view, edit, and delete reviews. Instructors can flag. Admins can moderate. Reviews visible on course page.

---

## Phase 3: User Story 2 — Course Completion Certificates (Priority: P1)

**Goal**: When a student completes a course, a certificate with a unique verification code (e.g., `CERT-A3F8B2C1`) is auto-generated. Certificates include student name, course title, completion date, and verification code.

**Independent Test**: Enroll a student, complete all mandatory course content, verify a Certificate record with a unique code exists for that student+course.

### Implementation for User Story 2

- [X] T013 [US2] Implement CertificateService (generation + code generation + PDF) in `backend_project/Services/Implementations/CertificateService.cs`
- [X] T014 [US2] Integrate certificate generation with ContentProgressService in `backend_project/Services/Implementations/ContentProgressService.cs` (call GenerateCertificateAsync on completion)
- [X] T015 [US2] Integrate certificate revocation with RefundService in `backend_project/Services/Implementations/RefundService.cs` (call RevokeByEnrollmentAsync on refund approve)
- [X] T016 [US2] Register `ICertificateService` in DI in `backend_project/Program.cs`

**Checkpoint**: Certificates auto-generate on course completion and auto-revoke on refund.

---

## Phase 4: User Story 3 — Public Certificate Verification (Priority: P2)

**Goal**: Anyone (logged out) can verify a certificate's authenticity via its unique code. Shows holder name, course title, completion date, and validity status.

**Independent Test**: Visit `/api/certificates/verify/{valid-code}` → see certificate details with "Valid" status. Visit with fake code → see "Certificate not found".

### Implementation for User Story 3

- [X] T017 [P] [US3] Add public verification endpoint `GET /api/certificates/verify/{code}` to CertificatesController in `backend_project/Controllers/CertificatesController.cs`
- [X] T018 [US3] Add `GetByCodeAsync` method to ICertificateService / CertificateService

**Checkpoint**: Public verification works without authentication — valid, revoked, and non-existent codes all return appropriate responses.

---

## Phase 5: User Story 4 — Certificate Sharing & Download (Priority: P3)

**Goal**: Students can view all their certificates, download each as a branded PDF, and share a verification link.

**Independent Test**: Student with completed courses navigates to certificates list, selects a certificate, downloads PDF, copies verification link.

### Implementation for User Story 4

- [X] T019 [P] [US4] Create `GET /api/certificates` (list) and `GET /api/certificates/{id}` (detail) endpoints in CertificatesController
- [X] T020 [US4] Create `GET /api/certificates/{id}/download` endpoint returning PDF in CertificatesController
- [X] T021 [US4] Implement PDF generation with QuestPDF in CertificateService (platform logo, student name, course title, completion date, verification code)
- [X] T022 [US4] Create admin certificate revocation endpoint `POST /api/admin/certificates/{id}/revoke` in `backend_project/Controllers/AdminCertificatesController.cs`

**Checkpoint**: Students can see, download, and share certificates. Admins can manually revoke.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Cleanup, DI audit, and edge case hardening

- [X] T023 Audit all DI registrations in `backend_project/Program.cs` for reviews and certificates services
- [X] T024 [P] Add ActivityLog calls for certificate generation and admin revocation
- [X] T025 [P] Add guard for zero-mandatory-items courses (certificate generation should not trigger for courses with no mandatory content)
- [X] T026 Add rate limiting consideration for public verification endpoint (prevent abuse)
- [X] T027 [P] Add unique constraint for `(UserId, CourseId)` on Reviews in ApplicationDbContext configuration
- [X] T028 [P] Add unique index on Certificate.Code in ApplicationDbContext configuration
- [X] T029 Run `dotnet build` and verify 0 errors

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — creates models, interfaces, DTOs
- **US1 Reviews (Phase 2)**: Depends on Phase 1 — independent of US2/US3/US4
- **US2 Certificates (Phase 3)**: Depends on Phase 1 — independent of US1/US3/US4
- **US3 Verification (Phase 4)**: Depends on US2 (requires Certificate entity and service)
- **US4 Sharing/Download (Phase 5)**: Depends on US2 (requires Certificate entity) + US3 (shares verification link)
- **Polish (Phase 6)**: Depends on all prior phases

### User Story Dependencies

```
Phase 1 (Setup) ───┬──► US1 (Reviews, P1)
                    ├──► US2 (Certificates, P1) ──► US3 (Verification, P2) ──► US4 (Sharing, P3)
                    └──► All stories depend on models in Setup
```

### Parallel Opportunities

- Phase 1 T001-T007: All marked [P] — models, DTOs, interfaces, DbContext config in parallel
- US1 and US2 can be implemented in parallel (different services, independent)
- US3 and US4 are sequential (US3 verification logic used by US4 sharing)

### Parallel Example: US1 + US2 Together

```bash
# US1 parallel:
Task: "Create validator in Validators/Review/CreateReviewValidator.cs"
Task: "Implement ReviewService + ReviewsController"

# US2 parallel:
Task: "Implement CertificateService + integration hooks"
```

---

## Implementation Strategy

### MVP First (US1 — Reviews)

1. Complete Phase 1: Setup (models, interfaces, DTOs)
2. Complete Phase 2: US1 (Reviews) — **MVP!**
3. Validate/test US1 independently
4. Deploy/demo if ready

### Incremental Delivery

1. Phase 1 → Foundation ready
2. Phase 2 → US1 Reviews → Test → Deploy
3. Phase 3 → US2 Certificates → Test → Deploy
4. Phase 4 → US3 Verification → Test → Deploy
5. Phase 5 → US4 Sharing → Test → Deploy
6. Phase 6 → Polish

### Key Integration Points

- **Certificate generation**: Hook into `ContentProgressService.RecalculateEnrollmentProgressAsync` (already sets enrollment to Completed)
- **Certificate revocation**: Hook into `RefundService.ApproveRefundAsync` (already sets enrollment to Refunded)
- **PDF library**: QuestPDF via `dotnet add package QuestPDF`

---

## Summary

| User Story | Priority | Tasks | Independent |
|------------|----------|-------|-------------|
| US1: Reviews | P1 | 5 (T008-T012) | ✅ Yes |
| US2: Certificate Gen | P1 | 4 (T013-T016) | ✅ Yes |
| US3: Verification | P2 | 2 (T017-T018) | Depends on US2 |
| US4: Sharing | P3 | 4 (T019-T022) | Depends on US2+US3 |
| Setup | — | 7 (T001-T007) | — |
| Polish | — | 7 (T023-T029) | — |
| **Total** | | **29 tasks** | |
