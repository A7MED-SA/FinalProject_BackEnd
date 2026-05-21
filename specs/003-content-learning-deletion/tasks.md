# Tasks: Content & Learning System with Deletion Lifecycle

**Input**: Design documents from `specs/003-content-learning-deletion/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅

**Tests**: Not explicitly requested. Tests are omitted.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Schema updates, shared DTOs, and foundation helpers

- [x] T001 Add missing fields to LiveSession model (MeetingUrl, Password, MaxAttendees, ActualStartAt, ActualEndAt) in `backend_project/models/LiveSession.cs`
- [x] T002 Create EF Core migration adding unique constraints: `(UserId, CourseId)` on enrollments, `(CommentId, UserId)` on comment_likes, `(EnrollmentId, ContentType, ContentId)` on content_progresses, and new LiveSession columns
- [x] T003 [P] Create EnrollmentGuard helper for enrollment-based access validation in `backend_project/Helpers/EnrollmentGuard.cs`
- [x] T004 [P] Create Validators directory and base validator structure in `backend_project/Validators/`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Shared DTOs and service interfaces that ALL user stories depend on

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T005 [P] Create Enrollment DTOs (CreateEnrollmentDto, EnrollmentResponseDto, EnrollmentDetailDto) in `backend_project/DTOs/Enrollment/`
- [x] T006 [P] Create ContentProgress DTOs (UpdateProgressDto, ContentProgressDto) in `backend_project/DTOs/ContentProgress/`
- [x] T007 [P] Create Video DTOs (CreateVideoDto, UpdateVideoDto, VideoResponseDto) in `backend_project/DTOs/Video/`
- [x] T008 [P] Create Document DTOs (CreateDocumentDto, UpdateDocumentDto, DocumentResponseDto) in `backend_project/DTOs/Document/`
- [x] T009 [P] Create Quiz DTOs (CreateQuizDto, QuizResponseDto, CreateQuestionDto, QuestionResponseDto, CreateOptionDto, OptionResponseDto) in `backend_project/DTOs/Quiz/`
- [x] T010 [P] Create QuizAttempt DTOs (StartAttemptDto, SubmitAttemptDto, QuizResultDto, QuizAttemptResponseDto) in `backend_project/DTOs/QuizAttempt/`
- [x] T011 [P] Create LiveSession DTOs (CreateLiveSessionDto, UpdateLiveSessionStatusDto, LiveSessionResponseDto) in `backend_project/DTOs/LiveSession/`
- [x] T012 [P] Create VideoComment DTOs (CreateCommentDto, CommentResponseDto, CommentLikeResponseDto) in `backend_project/DTOs/VideoComment/`
- [x] T013 [P] Create IEnrollmentService interface in `backend_project/Services/Interfaces/IEnrollmentService.cs`
- [x] T014 [P] Create IContentProgressService interface in `backend_project/Services/Interfaces/IContentProgressService.cs`
- [x] T015 [P] Create IVideoContentService interface in `backend_project/Services/Interfaces/IVideoContentService.cs`
- [x] T016 [P] Create IDocumentService interface in `backend_project/Services/Interfaces/IDocumentService.cs`
- [x] T017 [P] Create IQuizManagementService interface in `backend_project/Services/Interfaces/IQuizManagementService.cs`
- [x] T018 [P] Create IQuizAttemptService interface in `backend_project/Services/Interfaces/IQuizAttemptService.cs`
- [x] T019 [P] Create ILiveSessionService interface in `backend_project/Services/Interfaces/ILiveSessionService.cs`
- [x] T020 [P] Create ILiveAttendanceService interface in `backend_project/Services/Interfaces/ILiveAttendanceService.cs`
- [x] T021 [P] Create IVideoCommentService interface in `backend_project/Services/Interfaces/IVideoCommentService.cs`

**Checkpoint**: Foundation ready — user story implementation can now begin in parallel

---

## Phase 3: User Story 1 — Student Enrollment & Course Access (Priority: P1) 🎯 MVP

**Goal**: Enable students to enroll in courses, track enrollment status, and enforce access control

**Independent Test**: Create an enrollment for a student on a published course → verify enrollment record exists with "InProgress" status → verify student can retrieve course content → verify duplicate enrollment is blocked

### Implementation for User Story 1

- [x] T022 [P] [US1] Create CreateEnrollmentValidator with FluentValidation rules (courseId required, valid source enum) in `backend_project/Validators/CreateEnrollmentValidator.cs`
- [x] T023 [US1] Implement EnrollmentService: CreateEnrollmentAsync (duplicate check, course published check, increment enrollment count), GetEnrollmentsAsync (paginated, filtered by status), GetEnrollmentDetailAsync (with progress breakdown), UpdateEnrollmentStatusAsync (expire/refund) in `backend_project/Services/Implementations/EnrollmentService.cs`
- [x] T024 [US1] Implement EnrollmentController: POST /api/enrollments, GET /api/enrollments, GET /api/enrollments/{id} with [Authorize] and enrollment ownership validation in `backend_project/Controllers/EnrollmentController.cs`
- [x] T025 [US1] Register IEnrollmentService in DI container in `backend_project/Program.cs`

**Checkpoint**: Enrollment CRUD functional — students can enroll and retrieve enrollment status

---

## Phase 4: User Story 7 — Instructor Content Management (Priority: P1)

**Goal**: Enable instructors to create, update, and manage videos, documents, and quizzes with questions/options

**Independent Test**: Create a video, quiz (with questions and options), and document → link them to section items → update properties → retrieve and verify

**Note**: This story is placed before US2/US3 because content must exist before progress tracking or quiz-taking is possible.

### Implementation for User Story 7

- [ ] T026 [P] [US7] Create CreateVideoValidator (title required, videoFileId required, durationSeconds > 0) in `backend_project/Validators/CreateVideoValidator.cs`
- [ ] T027 [P] [US7] Create CreateQuizValidator (title required, passingScorePercent 0-100) in `backend_project/Validators/CreateQuizValidator.cs`
- [ ] T028 [P] [US7] Create CreateQuestionValidator (questionText required, at least 2 options for MCQ, exactly one correct) in `backend_project/Validators/CreateQuestionValidator.cs`
- [ ] T029 [US7] Implement VideoContentService: CreateVideoAsync (create Video + SectionItem, link to section), GetVideoAsync, UpdateVideoAsync (edit approval for published), DeleteVideoAsync (hard for draft, approval for published) in `backend_project/Services/Implementations/VideoContentService.cs`
- [ ] T030 [US7] Implement DocumentService: CreateDocumentAsync (create Document + SectionItem), GetDocumentAsync, UpdateDocumentAsync, DeleteDocumentAsync, IncrementDownloadCountAsync in `backend_project/Services/Implementations/DocumentService.cs`
- [ ] T031 [US7] Implement QuizManagementService: CreateQuizAsync (create Quiz + SectionItem), UpdateQuizAsync, DeleteQuizAsync (block if in-progress attempts), AddQuestionAsync (with options), UpdateQuestionAsync, DeleteQuestionAsync, ReorderQuestionsAsync in `backend_project/Services/Implementations/QuizManagementService.cs`
- [ ] T032 [US7] Implement VideoContentController: POST/GET/PUT/DELETE /api/courses/{courseId}/videos with [Authorize(Roles="Instructor")] and course ownership validation in `backend_project/Controllers/VideoContentController.cs`
- [ ] T033 [US7] Implement DocumentController: POST/GET/PUT/DELETE /api/courses/{courseId}/documents with [Authorize(Roles="Instructor")] in `backend_project/Controllers/DocumentController.cs`
- [ ] T034 [US7] Implement QuizManagementController: POST/GET/PUT/DELETE /api/courses/{courseId}/quizzes and /quizzes/{quizId}/questions with [Authorize(Roles="Instructor")] in `backend_project/Controllers/QuizManagementController.cs`
- [ ] T035 [US7] Register IVideoContentService, IDocumentService, IQuizManagementService in DI container in `backend_project/Program.cs`

**Checkpoint**: Instructors can fully manage all content types (Video, Document, Quiz with Questions/Options)

---

## Phase 5: User Story 2 — Video & Document Consumption with Progress Tracking (Priority: P1)

**Goal**: Enable enrolled students to consume videos/documents and have progress automatically tracked with enrollment recalculation

**Independent Test**: Enrolled student accesses a video → system records watch time → mark complete at 90% → verify enrollment progress recalculates → access a document → verify download count increments and progress updates

### Implementation for User Story 2

- [ ] T036 [US2] Implement ContentProgressService: UpdateProgressAsync (create or upsert ContentProgress), MarkCompletedAsync (check system-wide threshold for video), GetProgressForEnrollmentAsync, RecalculateEnrollmentProgressAsync (count-based percentage of mandatory items) in `backend_project/Services/Implementations/ContentProgressService.cs`
- [ ] T037 [US2] Implement ContentProgress endpoints in EnrollmentController: PUT /api/enrollments/{enrollmentId}/progress, GET /api/enrollments/{enrollmentId}/progress with enrollment ownership and EnrollmentGuard validation in `backend_project/Controllers/EnrollmentController.cs`
- [ ] T038 [US2] Register IContentProgressService in DI container in `backend_project/Program.cs`

**Checkpoint**: Students can track progress on videos and documents, enrollment progress auto-recalculates

---

## Phase 6: User Story 3 — Quiz Taking & Automated Grading (Priority: P1)

**Goal**: Enable enrolled students to take quizzes, auto-grade answers, enforce attempt limits, and auto-submit on timer expiry

**Independent Test**: Start a quiz attempt → submit answers → verify auto-grading → verify score calculation → verify pass/fail → verify enrollment progress updates → verify attempt limit enforcement → verify timer auto-submit

### Implementation for User Story 3

- [ ] T039 [P] [US3] Create SubmitQuizAttemptValidator (answers array required, each answer has questionId) in `backend_project/Validators/SubmitQuizAttemptValidator.cs`
- [ ] T040 [US3] Implement QuizAttemptService: StartAttemptAsync (enforce attempt limits, create QuizAttempt, return shuffled questions), SubmitAttemptAsync (record UserAnswers, auto-grade MCQ/TrueFalse, calculate score, determine pass/fail, update ContentProgress), CheckAndAutoSubmitExpiredAsync (server-side timer check), GetAttemptResultAsync in `backend_project/Services/Implementations/QuizAttemptService.cs`
- [ ] T041 [US3] Implement QuizAttemptController: POST /api/enrollments/{enrollmentId}/quizzes/{quizId}/attempts (start), PUT /api/enrollments/{enrollmentId}/quizzes/{quizId}/attempts/{attemptId} (submit), GET /api/enrollments/{enrollmentId}/quizzes/{quizId}/attempts (list) with [Authorize] and enrollment ownership in `backend_project/Controllers/QuizAttemptController.cs`
- [ ] T042 [US3] Register IQuizAttemptService in DI container in `backend_project/Program.cs`

**Checkpoint**: Students can take quizzes, get graded, and see results — enrollment progress updates on pass

---

## Phase 7: User Story 6 — Deletion Lifecycle (Priority: P1)

**Goal**: Implement safe deletion rules for categories, courses, sections, and content items with audit logging

**Independent Test**: Delete empty category ✓ → delete category with courses → blocked ✓ → delete draft course → cascade ✓ → delete published course → soft-delete ✓ → delete section in published course → approval workflow ✓ → delete quiz with in-progress attempts → blocked ✓

### Implementation for User Story 6

- [ ] T043 [US6] Update CategoryService.DeleteCategoryAsync: add safety guards (block if has courses, block if has subcategories, hard-delete only if empty) in `backend_project/Services/Implementations/CategoryService.cs`
- [ ] T044 [US6] Implement CourseService.DeleteCourseAsync: draft → hard cascade delete (sections, items, content), published with no enrollments → soft-delete, published with enrollments → soft-delete + read-only access, log to ActivityLog in `backend_project/Services/Implementations/CourseService.cs`
- [ ] T045 [US6] Update SectionService.DeleteSectionAsync: draft → hard delete, published → route through edit approval workflow as high-risk, soft-delete on approval in `backend_project/Services/Implementations/SectionService.cs`
- [ ] T046 [US6] Add delete endpoint to CourseManagementController: DELETE /api/courses/{courseId}/management with [Authorize(Roles="Instructor")] in `backend_project/Controllers/CourseManagementController.cs`
- [ ] T047 [US6] Update CategoryController.DeleteCategory: return 409 Conflict when category has courses or subcategories with descriptive error message in `backend_project/Controllers/CategoryController.cs`
- [ ] T048 [US6] Add deletion audit logging: ensure all delete operations (category, course, section, item, content) create ActivityLog entries with actor, entity type, entity id, action, and timestamp in `backend_project/Services/Implementations/CourseService.cs`

**Checkpoint**: All deletion rules are enforced — system blocks invalid deletions and audits all operations

---

## Phase 8: User Story 4 — Live Session Management & Attendance (Priority: P2)

**Goal**: Enable instructors to schedule live sessions and track student attendance with join/leave timestamps

**Independent Test**: Instructor creates a live session → starts it (status → Live) → student joins → attendance recorded → student leaves → duration calculated → instructor ends session → status → Finished

### Implementation for User Story 4

- [ ] T049 [P] [US4] Create CreateLiveSessionValidator (title required, scheduledStart before scheduledEnd, meetingUrl required) in `backend_project/Validators/CreateLiveSessionValidator.cs`
- [ ] T050 [US4] Implement LiveSessionService: CreateSessionAsync (create LiveSession + optional SectionItem), UpdateStatusAsync (Scheduled→Live→Finished/Cancelled, record actual start/end times), GetSessionsForCourseAsync, DeleteSessionAsync (block if status=Live) in `backend_project/Services/Implementations/LiveSessionService.cs`
- [ ] T051 [US4] Implement LiveAttendanceService: JoinSessionAsync (check enrollment, check attendee limit, create LiveAttendance), LeaveSessionAsync (set LeftAt, calculate duration), GetAttendanceForSessionAsync in `backend_project/Services/Implementations/LiveAttendanceService.cs`
- [ ] T052 [US4] Implement LiveSessionController: POST/GET/PUT/DELETE /api/courses/{courseId}/live-sessions with [Authorize(Roles="Instructor")] in `backend_project/Controllers/LiveSessionController.cs`
- [ ] T053 [US4] Implement LiveAttendanceController: POST /api/live-sessions/{sessionId}/attendance/join, POST .../leave, GET .../attendance with [Authorize] and enrollment check in `backend_project/Controllers/LiveAttendanceController.cs`
- [ ] T054 [US4] Register ILiveSessionService, ILiveAttendanceService in DI container in `backend_project/Program.cs`

**Checkpoint**: Instructors can manage live sessions, students can join/leave with tracked attendance

---

## Phase 9: User Story 5 — Video Commenting & Engagement (Priority: P2)

**Goal**: Enable enrolled students to post comments, reply with flattening, and toggle likes on video comments

**Independent Test**: Post a comment on a video → reply to it → reply to the reply (verify flattening to root) → like a comment → like again (verify toggle/unlike) → soft-delete a comment → verify hidden but preserved

### Implementation for User Story 5

- [ ] T055 [P] [US5] Create CreateCommentValidator (content required, maxLength 2000) in `backend_project/Validators/CreateCommentValidator.cs`
- [ ] T056 [US5] Implement VideoCommentService: CreateCommentAsync (flatten replies to 1-level, prepend @mention), GetCommentsForVideoAsync (paginated, exclude soft-deleted, include replies), ToggleLikeAsync (create/delete CommentLike, increment/decrement LikesCount), SoftDeleteCommentAsync (set DeletedAt, author or admin only) in `backend_project/Services/Implementations/VideoCommentService.cs`
- [ ] T057 [US5] Implement VideoCommentController: GET /api/videos/{videoId}/comments, POST .../comments, DELETE .../comments/{id}, POST .../comments/{id}/like with [Authorize] and enrollment-based access check in `backend_project/Controllers/VideoCommentController.cs`
- [ ] T058 [US5] Register IVideoCommentService in DI container in `backend_project/Program.cs`

**Checkpoint**: Students can comment, reply (with flattening), like/unlike, and soft-delete their comments

---

## Phase 10: Polish & Cross-Cutting Concerns

**Purpose**: Final integration, cleanup, and validation

- [ ] T059 [P] Verify all new services are registered in DI and Program.cs compiles without errors in `backend_project/Program.cs`
- [ ] T060 [P] Add AsNoTracking() to all read-only queries across all new services (GetXxx, ListXxx methods)
- [ ] T061 Run `dotnet build` and fix any compilation errors across all new files
- [ ] T062 Run EF Core migration: `dotnet ef migrations add ContentLearningDeletion` and `dotnet ef database update`
- [ ] T063 Validate Swagger/OpenAPI documentation loads correctly with all new endpoints visible
- [ ] T064 Run quickstart.md validation scenario: create course → add content → publish → enroll → progress → quiz → delete lifecycle

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 (migration must run first) — BLOCKS all user stories
- **US1 Enrollment (Phase 3)**: Depends on Phase 2 — MVP foundation
- **US7 Content Management (Phase 4)**: Depends on Phase 2 — can run parallel with US1
- **US2 Progress Tracking (Phase 5)**: Depends on US1 (needs enrollment) + US7 (needs content to track)
- **US3 Quiz Taking (Phase 6)**: Depends on US7 (needs quiz to exist) + US1 (needs enrollment)
- **US6 Deletion Lifecycle (Phase 7)**: Depends on US7 (needs content CRUD to test deletion)
- **US4 Live Sessions (Phase 8)**: Depends on Phase 2 — can run parallel with US1/US7
- **US5 Commenting (Phase 9)**: Depends on US7 (needs videos to comment on)
- **Polish (Phase 10)**: Depends on ALL user stories being complete

### User Story Dependencies

- **US1 (Enrollment)**: Phase 2 only — no other story dependencies
- **US7 (Content CRUD)**: Phase 2 only — no other story dependencies
- **US2 (Progress)**: Depends on US1 + US7
- **US3 (Quiz Taking)**: Depends on US1 + US7
- **US6 (Deletion)**: Depends on US7 (content must exist to test deletion)
- **US4 (Live Sessions)**: Phase 2 only — independent of other stories
- **US5 (Commenting)**: Depends on US7 (needs videos)

### Within Each User Story

- Validators before services (validators are used by controllers but can be written first)
- Services before controllers
- DI registration after both service + controller exist

### Parallel Opportunities

- **Phase 1**: T003 and T004 can run in parallel
- **Phase 2**: All DTOs (T005–T012) and all interfaces (T013–T021) can run in parallel
- **Phase 3 + Phase 4**: US1 and US7 can run in parallel after Phase 2
- **Phase 8**: US4 can run in parallel with US1/US7 (no cross-dependencies)
- **Within US7**: All three validators (T026–T028) can run in parallel

---

## Parallel Example: Phase 2 (Foundational)

```
# Launch ALL DTO creation tasks together (all [P]):
T005: Enrollment DTOs
T006: ContentProgress DTOs
T007: Video DTOs
T008: Document DTOs
T009: Quiz DTOs
T010: QuizAttempt DTOs
T011: LiveSession DTOs
T012: VideoComment DTOs

# Launch ALL interface creation tasks together (all [P]):
T013–T021: All 9 service interfaces
```

## Parallel Example: After Phase 2

```
# Developer A: US1 (Enrollment) — T022→T025
# Developer B: US7 (Content CRUD) — T026→T035
# Developer C: US4 (Live Sessions) — T049→T054
# All three can work simultaneously
```

---

## Implementation Strategy

### MVP First (US1 + US7)

1. Complete Phase 1: Setup (migration, helpers)
2. Complete Phase 2: Foundational (DTOs, interfaces)
3. Complete Phase 3: US1 Enrollment (students can enroll)
4. Complete Phase 4: US7 Content CRUD (instructors can add content)
5. **STOP and VALIDATE**: Courses have content, students are enrolled
6. Deploy/demo if ready

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. US1 + US7 → Test → Deploy (MVP: enrollment + content management)
3. US2 (Progress) → Test → Deploy (students see progress)
4. US3 (Quiz) → Test → Deploy (assessments functional)
5. US6 (Deletion) → Test → Deploy (data safety enforced)
6. US4 (Live) + US5 (Comments) → Test → Deploy (engagement features)
7. Polish → Final validation → Production

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Models already exist — no model creation tasks needed (except LiveSession update)
- All new services follow the existing pattern: interface → implementation → DI registration → controller
