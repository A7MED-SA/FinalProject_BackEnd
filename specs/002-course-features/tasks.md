# Tasks: course-features

**Input**: Design documents from `/specs/002-course-features/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/api.md

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [x] T001 Create `AddCourseEditApprovalSystem` migration for EF Core based on `data-model.md`
- [x] T002 Apply EF Core database migration

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T003 Create `CourseEditRequest` model in `Models/CourseEditRequest.cs`
- [x] T004 Update `Course` model to include `Version` and `LastContentUpdateAt` in `Models/Course.cs`
- [x] T005 Add `CourseLogAction` enum additions in `Models/CourseLog.cs`
- [x] T006 [P] Create DTOs in `DTOs/EditRequest/EditRequestDto.cs`
- [x] T007 [P] Implement `EditPolicyHelper` in `Helpers/EditPolicyHelper.cs`

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Course Edit Approval Workflow (Priority: P1) 🎯 MVP

**Goal**: Instructors can edit courses; low-risk edits apply immediately, high-risk edits become pending requests.

**Independent Test**: Can be tested by invoking `PUT /api/courses/{courseId}/sections/{sectionId}` as an instructor and verifying the outcome based on edit risk level.

### Implementation for User Story 1

- [x] T008 [US1] Create `ICourseEditApprovalService` interface in `Services/Interfaces/ICourseEditApprovalService.cs`
- [x] T009 [US1] Implement `RequestEditAsync` and `CancelRequestAsync` in `Services/CourseEditApprovalService.cs`
- [x] T010 [US1] Modify `SectionController` to use `ICourseEditApprovalService` in `Controllers/SectionController.cs`
- [x] T011 [US1] Create `EditRequestCleanupService` background service in `Services/Background/EditRequestCleanupService.cs`
- [x] T012 [US1] Register `EditRequestCleanupService` in `Program.cs`

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently

---

## Phase 4: User Story 2 - Reviewing Edit Requests (Priority: P1)

**Goal**: Admins can view pending requests, inspect differences, and approve or reject them.

**Independent Test**: Can be tested by an admin querying pending requests, reviewing one, and applying an approval/rejection.

### Implementation for User Story 2

- [x] T013 [P] [US2] Expand `EmailService` with overloaded notification methods in `Services/EmailService.cs`
- [x] T014 [US2] Implement `GetPendingRequestsAsync`, `GetRequestDetailsAsync`, and `ReviewRequestAsync` in `Services/CourseEditApprovalService.cs`
- [x] T015 [US2] Create `AdminCourseController` with endpoints to list, view, and action edit requests in `Controllers/AdminCourseController.cs`

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently

---

## Phase 5: User Story 3 - Public Course Browsing (Priority: P2)

**Goal**: Visitors can browse the published course catalog with filters, search, and pagination.

**Independent Test**: Test the unauthenticated `GET /api/public/courses` endpoint with various query parameters.

### Implementation for User Story 3

- [x] T016 [P] [US3] Create `PublicCourseDto` and `PublicCourseFilterDto` in `DTOs/Course/`
- [x] T017 [US3] Create `IPublicCourseService` interface in `Services/Interfaces/IPublicCourseService.cs`
- [x] T018 [US3] Implement `GetPublishedCoursesAsync`, `SearchCoursesSuggestAsync`, and `GetPlatformStatsAsync` in `Services/PublicCourseService.cs`
- [x] T019 [US3] Create `PublicCourseController` and wire up basic endpoints in `Controllers/PublicCourseController.cs`

**Checkpoint**: All user stories should now be independently functional

---

## Phase 6: User Story 4 - Public Course Details (Priority: P2)

**Goal**: Visitors can view detailed info about a specific course (excluding protected content).

**Independent Test**: Test `GET /api/public/courses/{id}` and verify learning content is absent.

### Implementation for User Story 4

- [x] T020 [P] [US4] Create `PublicCourseDetailDto` and related section projection DTOs in `DTOs/Course/`
- [x] T021 [US4] Implement `GetCourseDetailsAsync`, `GetCourseDetailsBySlugAsync`, and `GetRelatedCoursesAsync` in `Services/PublicCourseService.cs`
- [x] T022 [US4] Add detail, slug, and related course endpoints to `Controllers/PublicCourseController.cs`

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [x] T023 Update `appsettings.json` with `EditPolicy` and `BackgroundServices` configuration
- [x] T024 Register `IPublicCourseService` and `ICourseEditApprovalService` in DI container in `Program.cs`
- [x] T025 Run quickstart.md validation to ensure everything builds and runs correctly

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3+)**: All depend on Foundational phase completion
  - User Story 1 (US1) and User Story 3 (US3) can proceed in parallel
  - User Story 2 (US2) depends on US1 to generate data
  - User Story 4 (US4) depends on US3 foundational components
- **Polish (Final Phase)**: Depends on all desired user stories being complete

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel (within Phase 2)
- DTO creation across different user stories can run in parallel

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: Test User Story 1 independently
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test independently
3. Add User Story 2 → Test admin workflow → (MVP for edit approvals!)
4. Add User Story 3 → Test listing independently
5. Add User Story 4 → Test details independently → (Full Feature Release)
