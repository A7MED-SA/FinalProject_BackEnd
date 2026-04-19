---
description: "Task list for Course Management System implementation"
---

# Tasks: Course Management System

**Input**: Design documents from `/specs/001-course-management/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/api.md

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [x] T001 Install FluentValidation.AspNetCore package in backend_project/backend_project.csproj
- [x] T002 Register FluentValidation services in backend_project/Program.cs

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T003 Create base ApiResponse/Result classes in backend_project/DTOs/ApiResponse.cs
- [x] T004 [P] Create DTOs for Categories in backend_project/DTOs/Category/CategoryDtos.cs
- [x] T005 [P] Create Category Validators in backend_project/Validators/CategoryValidator.cs
- [x] T006 Implement Category Service Interface in backend_project/Services/Interfaces/ICategoryService.cs
- [x] T007 Implement Category Service in backend_project/Services/Implementations/CategoryService.cs
- [x] T008 Implement Category Controller in backend_project/Controllers/CategoryController.cs

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Course Creation and Drafting (Priority: P1) 🎯 MVP

**Goal**: Allow instructors to create a draft course and link requirements and learning outcomes.

**Independent Test**: Can be fully tested by creating a course through the system interface and verifying that its initial state is saved as Draft, and requirements/outcomes are successfully linked.

### Implementation for User Story 1

- [x] T009 [P] [US1] Create Course DTOs (Create, Summary, Details) in backend_project/DTOs/Course/CourseDtos.cs
- [x] T010 [P] [US1] Create Course Validators in backend_project/Validators/CourseValidator.cs
- [x] T011 [US1] Create Course Service Interface in backend_project/Services/Interfaces/ICourseService.cs
- [x] T012 [US1] Implement Create and AddRequirements logic in backend_project/Services/Implementations/CourseService.cs
- [x] T013 [US1] Create Course Management Controller in backend_project/Controllers/CourseManagementController.cs

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently

---

## Phase 4: User Story 2 - Course Structuring and Content Management (Priority: P1)

**Goal**: Instructors organize courses into sections and add specific content items.

**Independent Test**: Can be fully tested by creating sections for an existing course, adding items to those sections, and retrieving the full course hierarchy.

### Implementation for User Story 2

- [x] T014 [P] [US2] Create Section DTOs in backend_project/DTOs/Section/SectionDtos.cs
- [x] T015 [P] [US2] Create Section Validators in backend_project/Validators/SectionValidator.cs
- [x] T016 [US2] Create Section Service Interface in backend_project/Services/Interfaces/ISectionService.cs
- [x] T017 [US2] Implement Add Section and Add Item logic in backend_project/Services/Implementations/SectionService.cs
- [x] T018 [US2] Implement Section Controller Endpoints in backend_project/Controllers/SectionController.cs

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently

---

## Phase 5: User Story 3 - Rapid Content Reordering (Priority: P2)

**Goal**: Instructors easily reorder sections and section items using a bulk update mechanism.

**Independent Test**: Can be fully tested by sending an array of item IDs and their new positions, and verifying the database reflects the new order correctly.

### Implementation for User Story 3

- [x] T019 [P] [US3] Create ReorderRequestDto in backend_project/DTOs/Section/ReorderRequestDto.cs
- [x] T020 [US3] Add bulk reorder method interface to backend_project/Services/Interfaces/ISectionService.cs
- [x] T021 [US3] Implement transactional ExecuteUpdateAsync logic in backend_project/Services/Implementations/SectionService.cs
- [x] T022 [US3] Add bulk reorder endpoint to backend_project/Controllers/SectionController.cs

**Checkpoint**: User story 3 bulk operations functional.

---

## Phase 6: User Story 4 - Course Approval Workflow (Priority: P2)

**Goal**: Administrators review courses pending publication and approve them.

**Independent Test**: Can be fully tested by an Admin changing a course status from `PendingReview` to `Published`.

### Implementation for User Story 4

- [x] T023 [US4] Add Approval/Rejection and Publish methods to backend_project/Services/Interfaces/ICourseService.cs
- [x] T024 [US4] Implement Approval logic in backend_project/Services/Implementations/CourseService.cs (Handle status change, set PublishedAt, ApprovedBy)
- [x] T025 [US4] Create Admin Course Controller in backend_project/Controllers/AdminCourseController.cs (Endpoints: Approve, Reject, List Pending)

**Checkpoint**: All user stories should now be independently functional

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [x] T026 [P] Add Global Exception Handler middleware in backend_project/Middlewares/ExceptionMiddleware.cs
- [x] T027 Configure custom error responses mapping to 400 Bad Request for validation errors
- [x] T028 [P] Apply strict authorization policies ([Authorize(Roles="Admin")]) across controllers
- [x] T029 Code cleanup and refactoring across all services

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3+)**: All depend on Foundational phase completion
- **Polish (Final Phase)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2)
- **User Story 2 (P1)**: Depends on US1 (requires Course entity to exist logically).
- **User Story 3 (P2)**: Depends on US2 (requires Sections/Items to exist).
- **User Story 4 (P2)**: Depends on US1 (requires Course entity to exist).

### Parallel Opportunities

- DTO creation and Validators can run in parallel within each User Story
- User Story 4 can be developed in parallel with User Story 2/3.

## Parallel Example: User Story 1

```bash
# Launch models/validators for User Story 1 together:
Task: "T009 [P] [US1] Create Course DTOs"
Task: "T010 [P] [US1] Create Course Validators"
```

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: Test User Story 1 independently
5. Deploy/demo if ready
