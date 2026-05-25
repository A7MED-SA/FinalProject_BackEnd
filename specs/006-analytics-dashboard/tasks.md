---

description: "Task list for Analytics Dashboard feature implementation"
---

# Tasks: Analytics Dashboard

**Input**: Design documents from `specs/006-analytics-dashboard/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/api.md

**Tests**: Included optionally per story — the spec defines independent test criteria for each user story, so test tasks are generated to validate those criteria.

**Organization**: Tasks grouped by user story for independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies on incomplete tasks)
- **[Story]**: Which user story this task belongs to (US1, US2, US3)
- Include exact file paths

---

## Phase 1: Setup — Dashboard DTOs

**Purpose**: Create the shared DTO types that all dashboard services depend on.

- [X] T001 Create `backend_project/DTOs/Dashboard/` directory
- [X] T002 [P] Create `StudentDashboardDto` and `EnrollmentBriefDto` in `backend_project/DTOs/Dashboard/StudentDashboardDtos.cs` (fields per data-model.md)
- [X] T003 [P] Create `InstructorDashboardDto` and `ManagementCourseDto` in `backend_project/DTOs/Dashboard/InstructorDashboardDtos.cs` (fields per data-model.md)
- [X] T004 [P] Create `AdminOverviewDto`, `MonthlyRevenueDto`, `UserGrowthDto`, and `DashboardError` in `backend_project/DTOs/Dashboard/AdminDashboardDtos.cs` (fields per data-model.md)

**Checkpoint**: All DTOs defined — ready for service layer.

---

## Phase 2: Foundational — Shared Infrastructure

**Purpose**: Prerequisites that MUST be complete before any user story work begins.

- [X] T005 Add `RevenueSharePercentage` property (decimal, default 50.00m) to `User` model in `backend_project/models/User.cs`
- [X] T006 Create and apply EF Core migration for `RevenueSharePercentage` on `AspNetUsers` table
- [X] T007 [P] Create `IStudentDashboardService` interface in `backend_project/services/Interfaces/IStudentDashboardService.cs` (methods mirroring US1 needs)
- [X] T008 [P] Create `IInstructorDashboardService` interface in `backend_project/services/Interfaces/IInstructorDashboardService.cs` (methods mirroring US2 needs)
- [X] T009 [P] Create `IAdminDashboardService` interface in `backend_project/services/Interfaces/IAdminDashboardService.cs` (methods mirroring US3 needs)
- [X] T010 Register `IStudentDashboardService`, `IInstructorDashboardService`, `IAdminDashboardService` in DI container in `backend_project/Program.cs`

**Checkpoint**: Foundation ready — all three user stories can now be implemented independently.

---

## Phase 3: User Story 1 — Student Progress Dashboard (Priority: P1) 🎯 MVP

**Goal**: Students see aggregated learning stats (enrolled, in-progress, completed courses; total hours; certificates) plus recent enrollments and certificate-eligible courses.

**Independent Test**: Enroll a student in 3 courses, complete 1 fully, partially progress another. Verify the dashboard returns correct counts (3 enrolled, 1 completed, 1 in-progress) and total learning hours from the completed course.

### Implementation for User Story 1

- [X] T011 [P] [US1] Write `StudentDashboardService` unit tests in `tests/CommerceTests/StudentDashboardServiceTests.cs` (enrollment aggregation, learning hours computation, zero-data edge case)
- [X] T012 [US1] Implement `StudentDashboardService` in `backend_project/services/Implementations/StudentDashboardService.cs` — compute student stats via EF Core GroupBy/Sum queries; compute learning hours (video watch time, quiz 30min, doc 15min); wrap each sub-aggregation in try/catch with `DashboardError` list
- [X] T013 [US1] Implement `GET /api/dashboard/student` in `backend_project/Controllers/DashboardController.cs` — inject `IStudentDashboardService`, return `ApiResponse<StudentDashboardDto>`, authorize Student role, return zero defaults on empty data
- [X] T014 [US1] Verify student dashboard returns deterministic zero values (not null) when student has no enrollments; verify `DashboardError` list appended to response when a sub-aggregation fails

**Checkpoint**: Student dashboard fully functional and independently testable.

---

## Phase 4: User Story 2 — Teacher Instructor Dashboard (Priority: P1)

**Goal**: Teachers see aggregated teaching stats (total students, published courses, teaching hours, average rating, gross/net revenue) plus course list with enrollments/ratings and pending edit requests.

**Independent Test**: A teacher has 3 published courses with varying enrollments. Verify the dashboard returns correct total students (sum of enrollments), active course count (3), and average rating across all courses.

### Implementation for User Story 2

- [X] T015 [P] [US2] Write `InstructorDashboardService` unit tests in `tests/CommerceTests/InstructorDashboardServiceTests.cs` (student count aggregation, revenue calculation, zero-data edge case)
- [X] T016 [US2] Implement `InstructorDashboardService` in `backend_project/services/Implementations/InstructorDashboardService.cs` — compute teacher stats via EF Core; compute gross revenue (completed orders minus refunds) and net revenue (gross × `RevenueSharePercentage` / 100); include pending edit requests; wrap sub-aggregations in try/catch
- [X] T017 [US2] Implement `GET /api/dashboard/instructor` in `backend_project/Controllers/DashboardController.cs` — inject `IInstructorDashboardService`, return `ApiResponse<InstructorDashboardDto>`, authorize Instructor role
- [X] T018 [P] [US2] Implement `GET /api/management/courses` (paginated) in `backend_project/Controllers/ManagementCoursesController.cs` — expose instructor's courses with enrollment count, average rating, approval status, price; separate from public course listing per FR-018
- [X] T019 [US2] Add audit logging — call `IActivityLogService.LogActivityAsync` with action `"DashboardViewed_Revenue"` when instructor dashboard revenue data is accessed

**Checkpoint**: Teacher dashboard and course management fully functional.

---

## Phase 5: User Story 3 — Admin Platform Dashboard (Priority: P2)

**Goal**: Admins see platform-wide stats (total users, courses, revenue, pending approvals/requests, active instructors) plus monthly revenue breakdown, user growth, and enrollment trends.

**Independent Test**: The platform has 1000 users, 50 courses, 10 instructors, and 5 pending approvals. Verify the admin dashboard returns correct totals for each metric.

### Implementation for User Story 3

- [X] T020 [P] [US3] Write `AdminDashboardService` unit tests in `tests/CommerceTests/AdminDashboardServiceTests.cs` (platform stats aggregation, revenue trend computation, user growth/trend data)
- [X] T021 [US3] Implement `AdminDashboardService` in `backend_project/services/Implementations/AdminDashboardService.cs` — compute platform-wide stats; implement `GetMonthlyRevenueAsync(months)` (last N months of gross/net revenue grouped by YYYY-MM); implement `GetUserGrowthAsync(months)` (monthly new users + cumulative totals); implement `GetEnrollmentTrendsAsync(months)`; wrap in try/catch with `DashboardError` list
- [X] T022 [P] [US3] Implement `GET /api/admin/dashboard/overview` in `backend_project/Controllers/AdminDashboardController.cs` — return `ApiResponse<AdminOverviewDto>`, authorize Admin role
- [X] T023 [P] [US3] Implement `GET /api/admin/dashboard/revenue?months=12` in `AdminDashboardController.cs` — return `ApiResponse<List<MonthlyRevenueDto>>`
- [X] T024 [P] [US3] Implement `GET /api/admin/dashboard/user-growth?months=6` and `GET /api/admin/dashboard/enrollment-trends?months=12` in `AdminDashboardController.cs`
- [X] T025 [US3] Add `IMemoryCache` for trend data in `AdminDashboardService` — cache revenue/growth/trend data with absolute expiration at midnight; fallback to on-demand recomputation if cache is empty
- [X] T026 [US3] Add audit logging — call `IActivityLogService.LogActivityAsync` with action `"DashboardViewed_AdminOverview"` when admin overview or revenue endpoints are accessed

**Checkpoint**: Admin dashboard fully functional with all trend endpoints.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Verification and final touches across all user stories.

- [X] T027 Run `dotnet build` to verify zero compilation errors across all new and modified files
- [X] T028 Run `dotnet test` in `tests/CommerceTests/` to verify all dashboard tests pass

**Checkpoint**: Feature complete — all user stories verified.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — can start immediately
- **Phase 2 (Foundational)**: Depends on Phase 1 — BLOCKS all user stories
- **Phase 3 (US1)**: Depends on Phase 2 — no dependencies on other stories
- **Phase 4 (US2)**: Depends on Phase 2 — no dependencies on other stories
- **Phase 5 (US3)**: Depends on Phase 2 — no dependencies on other stories
- **Phase 6 (Polish)**: Depends on Phase 3, 4, 5

### User Story Dependencies

| Story | Priority | Depends On | Independent? |
|-------|----------|------------|-------------|
| US1 — Student Dashboard | P1 | Phase 2 only | ✅ Yes — fully independent |
| US2 — Instructor Dashboard | P1 | Phase 2 only | ✅ Yes — fully independent |
| US3 — Admin Dashboard | P2 | Phase 2 only | ✅ Yes — fully independent |

All three user stories are independent of each other — they share no implementation files and serve different roles. They can be implemented in parallel or sequentially in any order.

### Parallel Opportunities

- **Phase 1**: T002, T003, T004 can run in parallel (different DTO files)
- **Phase 2**: T007, T008, T009 can run in parallel (different interface files)
- **Phase 3**: T011 (tests) can run in parallel with T012 (implementation)
- **Phase 4**: T015 (tests) + T018 (management controller) can run in parallel with T016/T017 (dashboard)
- **Phase 5**: T020 (tests) + T022, T023, T024 (multiple controller endpoints) can all run in parallel
- **All user stories**: US1, US2, US3 can be implemented in parallel by different developers

---

## Parallel Example: User Story 1

```bash
# Can run in parallel:
Task: "Write StudentDashboardService unit tests in tests/CommerceTests/StudentDashboardServiceTests.cs"
Task: "Implement StudentDashboardService in backend_project/Services/Implementations/StudentDashboardService.cs"
```

## Parallel Example: User Story 2

```bash
# Can run in parallel:
Task: "Write InstructorDashboardService unit tests in tests/CommerceTests/InstructorDashboardServiceTests.cs"
Task: "Implement GET /api/management/courses in backend_project/Controllers/ManagementCoursesController.cs"

# Then (dependent on InstructorDashboardService implementation):
Task: "Implement GET /api/dashboard/instructor in DashboardController.cs"
Task: "Add audit logging for instructor revenue access"
```

## Parallel Example: User Story 3

```bash
# Can run in parallel:
Task: "Write AdminDashboardService unit tests"
Task: "Implement AdminDashboardService in AdminDashboardService.cs"
Task: "Implement overview endpoint in AdminDashboardController.cs"
Task: "Implement revenue endpoint in AdminDashboardController.cs"
Task: "Implement user-growth endpoint in AdminDashboardController.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1 (Setup) — DTOs
2. Complete Phase 2 (Foundational) — User model, interfaces, DI
3. Complete Phase 3 (US1) — Student Dashboard
4. **STOP and VALIDATE**: Test student dashboard independently via `dotnet test`
5. Deploy/demo with student dashboard only

### Incremental Delivery

1. Phase 1 + Phase 2 → Foundation ready
2. Add Phase 3 (US1) → Student Dashboard → Deploy/Demo (MVP!)
3. Add Phase 4 (US2) → Instructor Dashboard → Deploy/Demo
4. Add Phase 5 (US3) → Admin Dashboard → Deploy/Demo
5. Phase 6 → Polish and verify all stories work together

### Parallel Team Strategy

With multiple developers:
1. Team completes Phase 1 + Phase 2 together
2. Once Phase 2 is done:
   - Developer A: Phase 3 (US1)
   - Developer B: Phase 4 (US2)
   - Developer C: Phase 5 (US3)
3. All three stories are independent — no merge conflicts expected
4. Team runs Phase 6 together to verify integration

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- No new database entities needed — all stats from aggregation queries
- RevenueSharePercentage is the only model change (User.cs)
- Degraded mode: each sub-aggregation wrapped in try/catch, errors collected in `DashboardError` list
- Cache: `IMemoryCache` for trend data with midnight expiration; live stats always real-time
- Audit: `IActivityLogService.LogActivityAsync` for all revenue-related dashboard access
- Commit after each phase or logical group
- Stop at any checkpoint to validate story independently
