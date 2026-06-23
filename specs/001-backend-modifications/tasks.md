# Tasks: Backend Modifications — Athary Platform

**Input**: Design documents from `/specs/001-backend-modifications/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Not explicitly requested in feature specification. Test tasks omitted.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and shared configurations

- [x] T001 Add Slug property to User entity in New/src/Athary.Domain/Entities/User.cs
- [x] T002 [P] Create Testimonial entity in New/src/Athary.Domain/Entities/Testimonial.cs
- [x] T003 [P] Create NotificationPreference entity in New/src/Athary.Domain/Entities/NotificationPreference.cs
- [x] T004 [P] Create ContactMessage entity in New/src/Athary.Domain/Entities/ContactMessage.cs
- [x] T005 [P] Create LegalPage entity in New/src/Athary.Domain/Entities/LegalPage.cs
- [x] T006 Add DbSets and indexes for new entities in New/src/Athary.Infrastructure/Data/AppDbContext.cs
- [x] T007 Create EF Core migration for new entities and indexes

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T008 Update IProfileService interface with slug methods in New/src/Athary.Application/Interfaces/IProfileService.cs
- [x] T009 Implement slug generation and availability check in New/src/Athary.Application/Services/ProfileService.cs
- [x] T010 [P] Register new services in DI container in New/src/Athary.API/Program.cs
- [x] T011 [P] Configure rate limiting policy for Contact endpoint in New/src/Athary.API/Program.cs

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Public Instructor Profiles via Slug (Priority: P1) 🎯 MVP

**Goal**: Allow visitors to access instructor profiles via human-readable slug URLs

**Independent Test**: Create instructor with slug, access profile via `/api/public/instructors/{slug}`, verify 200 OK response

### Implementation for User Story 1

- [x] T012 [P] [US1] Update PublicProfileDto with Slug field in New/src/Athary.Application/DTOs/Public/PublicProfileDto.cs
- [x] T013 [P] [US1] Create PublicInstructorController in New/src/Athary.API/Controllers/PublicInstructorController.cs
- [x] T014 [US1] Add slug uniqueness index in New/src/Athary.Infrastructure/Data/AppDbContext.cs

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently

---

## Phase 4: User Story 2 - Landing Page Aggregated Endpoint (Priority: P1)

**Goal**: Single API endpoint returning all landing page data (stats, courses, categories, sessions, testimonials)

**Independent Test**: Call `GET /api/public/landing`, verify all sections returned in single response

### Implementation for User Story 2

- [x] T015 [P] [US2] Create LandingDto and LandingStatsDto in New/src/Athary.Application/DTOs/Public/LandingDto.cs
- [x] T016 [P] [US2] Create IPublicService interface in New/src/Athary.Application/Interfaces/IPublicService.cs
- [x] T017 [US2] Implement PublicService with caching and error fallback in New/src/Athary.Application/Services/PublicService.cs
- [x] T018 [US2] Add GetLandingStatsAsync and GetFeaturedCoursesAsync to CourseRepository in New/src/Athary.Infrastructure/Repositories/CourseRepository.cs
- [x] T019 [US2] Create PublicController with landing endpoint in New/src/Athary.API/Controllers/PublicController.cs

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently

---

## Phase 5: User Story 3 - Testimonials Management (Priority: P2)

**Goal**: Allow students to submit testimonials and admins to manage them (approve, flag, reorder)

**Independent Test**: Submit testimonial as student, approve as admin, verify appears on public endpoint

### Implementation for User Story 3

- [x] T020 [P] [US3] Create TestimonialDto, CreateTestimonialDto, UpdateTestimonialDto in New/src/Athary.Application/DTOs/Public/TestimonialDto.cs
- [x] T021 [P] [US3] Create ITestimonialService interface in New/src/Athary.Application/Interfaces/ITestimonialService.cs
- [x] T022 [US3] Implement TestimonialService with upsert logic in New/src/Athary.Application/Services/TestimonialService.cs
- [x] T023 [US3] Create PublicTestimonialsController in New/src/Athary.API/Controllers/PublicTestimonialsController.cs
- [x] T024 [US3] Create AdminTestimonialsController in New/src/Athary.API/Controllers/AdminTestimonialsController.cs
- [x] T025 [US3] Create TestimonialSeeder for Arabic demo data in New/src/Athary.Infrastructure/Data/Seeders/TestimonialSeeder.cs

**Checkpoint**: At this point, User Stories 1, 2, AND 3 should all work independently

---

## Phase 6: User Story 4 - Notification Preferences (Priority: P2)

**Goal**: Allow users to customize which notifications they receive

**Independent Test**: Update notification preferences, verify changes persist and are returned correctly

### Implementation for User Story 4

- [x] T026 [P] [US4] Create NotificationPreferencesDto and UpdateNotificationPreferencesDto in New/src/Athary.Application/DTOs/Notification/NotificationPreferencesDto.cs
- [x] T027 [P] [US4] Create INotificationPreferenceService interface in New/src/Athary.Application/Interfaces/INotificationPreferenceService.cs
- [x] T028 [US4] Implement NotificationPreferenceService in New/src/Athary.Application/Services/NotificationPreferenceService.cs
- [x] T029 [US4] Create NotificationPreferencesController in New/src/Athary.API/Controllers/NotificationPreferencesController.cs
- [x] T030 [US4] Add seed data migration for existing users in New/src/Athary.Infrastructure/Data/AppDbContext.cs

**Checkpoint**: At this point, User Stories 1-4 should all work independently

---

## Phase 7: User Story 5 - Contact Form (Priority: P2)

**Goal**: Accept contact form submissions from anonymous users with spam detection and rate limiting

**Independent Test**: Submit valid contact message, verify stored; submit spam, verify rejected

### Implementation for User Story 5

- [x] T031 [P] [US5] Create ContactMessageDto in New/src/Athary.Application/DTOs/Contact/ContactMessageDto.cs
- [x] T032 [P] [US5] Create IContactService interface in New/src/Athary.Application/Interfaces/IContactService.cs
- [x] T033 [US5] Implement ContactService with spam detection in New/src/Athary.Application/Services/ContactService.cs
- [x] T034 [US5] Create PublicContactController with rate limiting in New/src/Athary.API/Controllers/PublicContactController.cs
- [x] T035 [US5] Create AdminContactController in New/src/Athary.API/Controllers/AdminContactController.cs

**Checkpoint**: At this point, User Stories 1-5 should all work independently

---

## Phase 8: User Story 6 - About Page API (Priority: P3)

**Goal**: Provide API endpoint for about page content from system settings

**Independent Test**: Call `GET /api/public/about`, verify content returned from settings

### Implementation for User Story 6

- [x] T036 [US6] Create PublicAboutController in New/src/Athary.API/Controllers/PublicAboutController.cs

**Checkpoint**: At this point, User Stories 1-6 should all work independently

---

## Phase 9: User Story 7 - Legal Pages API (Priority: P3)

**Goal**: Serve legal pages (privacy, terms, refund) via type-based routing

**Independent Test**: Call `GET /api/public/legal/privacy`, verify content returned; call invalid type, verify 400

### Implementation for User Story 7

- [x] T037 [P] [US7] Create LegalPageDto in New/src/Athary.Application/DTOs/Public/LegalPageDto.cs
- [x] T038 [P] [US7] Create ILegalPageService interface in New/src/Athary.Application/Interfaces/ILegalPageService.cs
- [x] T039 [US7] Implement LegalPageService in New/src/Athary.Application/Services/LegalPageService.cs
- [x] T040 [US7] Create PublicLegalController with type validation in New/src/Athary.API/Controllers/PublicLegalController.cs

**Checkpoint**: All user stories should now be independently functional

---

## Phase 10: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [x] T041 [P] Add Swagger/OpenAPI documentation attributes to all new controllers
- [x] T042 [P] Add performance indexes for search queries in New/src/Athary.Infrastructure/Data/AppDbContext.cs
- [ ] T043 Run quickstart.md validation scenarios
- [x] T044 Verify all migrations apply cleanly

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-9)**: All depend on Foundational phase completion
  - User stories can then proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 → P2 → P3)
- **Polish (Phase 10)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 3 (P2)**: Can start after Foundational (Phase 2) - Depends on Testimonial entity (Setup T002)
- **User Story 4 (P2)**: Can start after Foundational (Phase 2) - Depends on NotificationPreference entity (Setup T003)
- **User Story 5 (P2)**: Can start after Foundational (Phase 2) - Depends on ContactMessage entity (Setup T004) and rate limiting (Foundational T011)
- **User Story 6 (P3)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 7 (P3)**: Can start after Foundational (Phase 2) - Depends on LegalPage entity (Setup T005)

### Within Each User Story

- DTOs before services
- Services before controllers
- Core implementation before integration
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel (within Phase 2)
- Once Foundational phase completes, US1 and US2 can start in parallel
- All DTOs within a story marked [P] can run in parallel
- Different user stories can be worked on in parallel by different team members

---

## Parallel Example: User Story 3 (Testimonials)

```bash
# Launch all DTOs for Testimonials together:
Task: "Create TestimonialDto, CreateTestimonialDto, UpdateTestimonialDto in New/src/Athary.Application/DTOs/Public/TestimonialDto.cs"
Task: "Create ITestimonialService interface in New/src/Athary.Application/Interfaces/ITestimonialService.cs"

# Then implement service and controllers:
Task: "Implement TestimonialService with upsert logic in New/src/Athary.Application/Services/TestimonialService.cs"
Task: "Create PublicTestimonialsController in New/src/Athary.API/Controllers/PublicTestimonialsController.cs"
Task: "Create AdminTestimonialsController in New/src/Athary.API/Controllers/AdminTestimonialsController.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 + 2)

1. Complete Phase 1: Setup (entities, migrations)
2. Complete Phase 2: Foundational (profile service, DI, rate limiting)
3. Complete Phase 3: User Story 1 (instructor slug)
4. Complete Phase 4: User Story 2 (landing endpoint)
5. **STOP and VALIDATE**: Test US1 and US2 independently
6. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 + 2 → Test independently → Deploy/Demo (MVP!)
3. Add User Story 3 (Testimonials) → Test independently → Deploy/Demo
4. Add User Story 4 + 5 (Notifications + Contact) → Test independently → Deploy/Demo
5. Add User Story 6 + 7 (About + Legal) → Test independently → Deploy/Demo
6. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1 (Instructor Slug)
   - Developer B: User Story 2 (Landing Endpoint)
   - Developer C: User Story 3 (Testimonials)
3. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence

## Task Summary

| Phase | Tasks | Parallel Tasks |
|-------|-------|----------------|
| Phase 1: Setup | 7 | 4 |
| Phase 2: Foundational | 4 | 2 |
| Phase 3: US1 (Instructor Slug) | 3 | 2 |
| Phase 4: US2 (Landing) | 5 | 2 |
| Phase 5: US3 (Testimonials) | 6 | 2 |
| Phase 6: US4 (Notifications) | 5 | 2 |
| Phase 7: US5 (Contact) | 5 | 2 |
| Phase 8: US6 (About) | 1 | 0 |
| Phase 9: US7 (Legal) | 4 | 2 |
| Phase 10: Polish | 4 | 2 |
| **Total** | **44** | **20** |
