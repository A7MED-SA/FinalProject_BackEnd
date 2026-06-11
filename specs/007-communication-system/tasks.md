# Tasks: Communication & System

**Input**: Design documents from `/specs/007-communication-system/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/api.md

**Tests**: Not explicitly requested in spec — manual Independent Test scenarios per user story suffice for initial version.

**Organization**: Tasks grouped by user story for independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1-US5)
- Exact file paths included in descriptions

## Path Convention

```
backend_project/Controllers/Communication/
backend_project/Services/Implementations/
backend_project/Services/Interfaces/
backend_project/Models/
backend_project/DTOs/Communication/
backend_project/Data/
```

---

## Phase 1: Schema & Infrastructure

**Purpose**: Apply required schema changes and register new services

- [ ] T001 Extend `ActivityLogEntityType` enum with `Message`, `Announcement`, `SystemSetting`, `Report` in `backend_project/models/ActivityLog.cs`
- [ ] T002 [P] Extend `AnnouncementTarget` enum with `SpecificCourse`; add `CourseId` column to `Announcement` model in `backend_project/models/Announcement.cs`
- [ ] T003 [P] Add `IsDeleted` column to `Message` model in `backend_project/models/Message.cs`
- [ ] T004 [P] Update `ReportEntityType` enum (add `Message`), `ReportReason` enum (add `Harassment`), `ReportStatus` enum (use `Pending`, `Dismissed`, `ActionTaken`); add `AdminNote` column in `backend_project/models/Report.cs`
- [ ] T005 Add unique composite index on `(ReporterId, EntityType, EntityId)` for `Report` in `backend_project/Data/ApplicationDbContext.cs` or via Fluent API
- [ ] T006 Generate and apply EF Core migration — `dotnet ef migrations add AddCommunicationFeature`
- [ ] T007 Register all new services in DI in `backend_project/Program.cs`: `IMessageService`, `IAnnouncementService`, `ISystemSettingService`, `IReportService`

**Checkpoint**: Schema updated, models modified, migration applied, DI ready.

---

## Phase 2: Internal Messaging — User Story 1 (Priority: P1) 🎯 MVP

**Goal**: Students message instructors, instructors reply, admins message anyone. Conversations derived from unordered user pairs. 30 msg/min rate limit.

**Independent Test**: Student A sends message to Instructor B → Instructor B sees in inbox, replies → Student A sees reply with correct read/unread timestamps.

- [ ] T008 [P] [US1] Create `IMessageService` interface in `backend_project/Services/Interfaces/IMessageService.cs`
- [ ] T009 [P] [US1] Create Message DTOs (`SendMessageRequest`, `MessageResponse`, `ConversationResponse`, `UnreadCountResponse`) in `backend_project/DTOs/Communication/MessageDtos.cs`
- [ ] T010 [US1] Implement `MessageService` in `backend_project/Services/Implementations/MessageService.cs` — send (with relationship validation + reply allowance), get conversations (unordered pair grouping), get messages, mark read, soft delete, unread count
- [ ] T011 [US1] Implement `MessagesController` in `backend_project/Controllers/Communication/MessagesController.cs` — endpoints: POST send, GET conversations, GET conversation messages, PATCH mark read, DELETE soft delete, GET unread-count
- [ ] T012 [US1] Configure rate limiting for Messaging controller: 30 requests/min per user in `backend_project/Program.cs`

**Checkpoint**: Messaging fully functional — send, receive, read status, soft delete, rate limited.

---

## Phase 3: Announcements — User Story 2 (Priority: P1)

**Goal**: Admins create platform-wide announcements; instructors create course-specific ones. Active/inactive lifecycle only.

**Independent Test**: Admin creates platform announcement → all users see it. Instructor creates course announcement → only enrolled students see it.

- [ ] T013 [P] [US2] Create `IAnnouncementService` interface in `backend_project/Services/Interfaces/IAnnouncementService.cs`
- [ ] T014 [P] [US2] Create Announcement DTOs (`CreateAnnouncementRequest`, `UpdateAnnouncementRequest`, `AnnouncementResponse`) in `backend_project/DTOs/Communication/AnnouncementDtos.cs`
- [ ] T015 [US2] Implement `AnnouncementService` in `backend_project/Services/Implementations/AnnouncementService.cs` — CRUD, feed filtered by enrollments + platform-wide, deactivate, authorization (admin vs instructor on their course)
- [ ] T016 [US2] Implement `AnnouncementsController` in `backend_project/Controllers/Communication/AnnouncementsController.cs` — endpoints: POST create, GET feed, PUT update, PATCH deactivate, DELETE

**Checkpoint**: Announcements work for both admin (global) and instructor (course-specific) roles.

---

## Phase 4: System Settings — User Story 3 (Priority: P2)

**Goal**: Admins manage key-value settings with data type validation. Changes logged to ActivityLog.

**Independent Test**: Admin updates boolean setting true→false → persisted immediately. Audit log entry created with old/new values.

- [ ] T017 [P] [US3] Create `ISystemSettingService` interface in `backend_project/Services/Interfaces/ISystemSettingService.cs`
- [ ] T018 [P] [US3] Create SystemSetting DTOs (`UpdateSettingRequest`, `CreateSettingRequest`, `SettingResponse`) in `backend_project/DTOs/Communication/SystemSettingDtos.cs`
- [ ] T019 [US3] Implement `SystemSettingService` in `backend_project/Services/Implementations/SystemSettingService.cs` — CRUD, data type validation, ActivityLog logging on change, in-memory cache with immediate admin invalidation
- [ ] T020 [US3] Implement `SystemSettingsController` in `backend_project/Controllers/Communication/SystemSettingsController.cs` — endpoints: GET all, GET by key, PUT update, POST create, DELETE (all admin-only)

**Checkpoint**: Settings CRUD works with validation, caching, and audit logging.

---

## Phase 5: Activity Log Viewer — User Story 4 (Priority: P2)

**Goal**: Admins view and filter activity logs. Reuses existing `IActivityLogService`.

**Independent Test**: Perform action (e.g., update setting) → query logs filtered by that action → correct entry returned with user/timestamp/details.

- [ ] T021 [P] [US4] Create ActivityLog DTOs (`ActivityLogResponse`, `ActivityLogFilterRequest`) in `backend_project/DTOs/Communication/ActivityLogDtos.cs`
- [ ] T022 [US4] Implement `ActivityLogsController` in `backend_project/Controllers/Communication/ActivityLogsController.cs` — endpoints: GET paginated logs with filters (userId, action, entityType, dateFrom, dateTo, ipAddress), max page size 100 (admin-only)

**Note**: Reuses existing `IActivityLogService`/`ActivityLogService`. The new entity types (Message, Announcement, SystemSetting, Report) were added in Phase 1.

**Checkpoint**: Activity logs viewable and filterable by admins.

---

## Phase 6: Content Reporting — User Story 3 (Priority: P3)

**Goal**: Users report inappropriate content (courses, reviews, messages). Admins review and resolve. Reporter notified.

**Independent Test**: User reports review as inappropriate → Admin sees pending report → Admin takes action → Reporter receives notification.

- [ ] T023 [P] [US5] Create `IReportService` interface in `backend_project/Services/Interfaces/IReportService.cs`
- [ ] T024 [P] [US5] Create Report DTOs (`CreateReportRequest`, `ResolveReportRequest`, `ReportResponse`) in `backend_project/DTOs/Communication/ReportDtos.cs`
- [ ] T025 [US5] Implement `ReportService` in `backend_project/Services/Implementations/ReportService.cs` — create (with duplicate check → update existing), list pending (admin), resolve (admin, with status Dismissed/ActionTaken), notify reporter via existing `INotificationService`
- [ ] T026 [US5] Implement `ReportsController` in `backend_project/Controllers/Communication/ReportsController.cs` — endpoints: POST create, GET pending (admin), PATCH resolve (admin)

**Checkpoint**: Reporting flow complete — submit, review, resolve, notify.

---

## Phase 7: Build & Verify

**Purpose**: Ensure everything compiles and basic sanity checks pass

- [ ] T027 Run `dotnet build` and resolve any compilation errors
- [ ] T028 Run `dotnet test` and ensure all existing tests still pass
- [ ] T029 Run quickstart.md verification steps

**Checkpoint**: Build clean, existing tests pass, feature ready.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Schema)**: No dependencies — start here
- **Phases 2–6 (User Stories)**: All depend on Phase 1 completion
- **Phase 7 (Build)**: Depends on Phases 1–6 completion

### User Story Dependencies

- **US1 (Messaging, P1)**: No dependencies on other stories — independent
- **US2 (Announcements, P1)**: No dependencies on other stories — independent
- **US3 (System Settings, P2)**: No dependencies on other stories — independent
- **US4 (Activity Logs, P2)**: Depends on Phase 1 enum extensions only — independent
- **US5 (Reporting, P3)**: Uses existing INotificationService — no direct dependency on other stories

### Within Each User Story

- DTOs → Service Interface → Service Implementation → Controller

### Parallel Opportunities

- Phase 1 tasks T001–T005 can all run in parallel
- All [P] tasks within a user story can run in parallel
- Stories US1–US5 can be worked on in parallel after Phase 1 completes (if team capacity allows)

---

## Parallel Example: User Story 1 (Messaging)

```bash
# Launch interface + DTOs in parallel:
Task: "Create IMessageService interface in backend_project/Services/Interfaces/IMessageService.cs"
Task: "Create MessageDtos in backend_project/DTOs/Communication/MessageDtos.cs"

# After both complete — implement service + controller sequentially:
Task: "Implement MessageService in backend_project/Services/Implementations/MessageService.cs"
Task: "Implement MessagesController in backend_project/Controllers/Communication/MessagesController.cs"
```

---

## Implementation Strategy

### MVP First (US1 + US2 — both P1)

1. Complete Phase 1: Schema & Infrastructure
2. Complete Phase 2: User Story 1 (Messaging) ✓ independently testable
3. Complete Phase 3: User Story 2 (Announcements) ✓ independently testable
4. **STOP and VALIDATE**: Both P1 features functional
5. Deploy/demo if ready

### Incremental Delivery

1. Schema → Foundation ready
2. Add US1 (Messaging, P1) → Test independently → Deploy/Demo (MVP!)
3. Add US2 (Announcements, P1) → Test independently → Deploy
4. Add US3 (Settings, P2) + US4 (Activity Logs, P2) → Test → Deploy
5. Add US5 (Reporting, P3) → Test → Deploy

### Parallel Team Strategy

With multiple developers:
1. Complete Phase 1 together
2. Developer A: US1 (Messaging) — highest priority
3. Developer B: US2 (Announcements) — highest priority
4. Developer C: US3 (Settings) + US4 (Activity Logs) — second priority
5. Developer A (after US1): US5 (Reporting) — third priority

---

## Notes

- [P] tasks = different files, no cross-dependencies
- [US1-5] labels map tasks to specific user stories for traceability
- All existing models are modified in-place (no new entities)
- Existing `IActivityLogService`/`INotificationService` reused throughout
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
