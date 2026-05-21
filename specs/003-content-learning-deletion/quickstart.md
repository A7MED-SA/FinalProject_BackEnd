# Quickstart: Content & Learning System with Deletion Lifecycle

**Date**: 2026-05-14

## Prerequisites

- .NET 9.0 SDK
- SQL Server (connection string in `appsettings.json`)
- MinIO running locally (for file storage)
- Existing database with all 56 models migrated

## Implementation Order

Execute in this order due to dependencies:

### Phase 1: Foundation Services (No Controller Dependencies)

1. **Add missing LiveSession fields** — Add `MeetingUrl`, `Password`, `MaxAttendees`, `ActualStartAt`, `ActualEndAt` to the `LiveSession` model and generate a migration.

2. **Add unique constraints** — Add `(UserId, CourseId)` on Enrollment, `(CommentId, UserId)` on CommentLike, `(EnrollmentId, ContentType, ContentId)` on ContentProgress via migration.

3. **Create DTOs** — Create DTO directories and classes under `DTOs/` for:
   - `Enrollment/` (CreateEnrollmentDto, EnrollmentResponseDto, EnrollmentDetailDto)
   - `ContentProgress/` (UpdateProgressDto, ContentProgressDto)
   - `Video/` (CreateVideoDto, UpdateVideoDto, VideoResponseDto)
   - `Document/` (CreateDocumentDto, UpdateDocumentDto, DocumentResponseDto)
   - `Quiz/` (CreateQuizDto, QuizResponseDto, CreateQuestionDto, QuestionResponseDto, CreateOptionDto)
   - `QuizAttempt/` (StartAttemptDto, SubmitAttemptDto, QuizResultDto)
   - `LiveSession/` (CreateLiveSessionDto, LiveSessionResponseDto, UpdateStatusDto)
   - `VideoComment/` (CreateCommentDto, CommentResponseDto)

### Phase 2: Core Services

4. **EnrollmentService** — CRUD + duplicate prevention + status management + progress recalculation.

5. **ContentProgressService** — Create/update progress, completion threshold check, enrollment progress recalculation trigger.

6. **VideoContentService** — CRUD with edit approval integration for published courses.

7. **DocumentService** — CRUD with edit approval integration.

8. **QuizManagementService** — Quiz CRUD + Question/Option management.

9. **QuizAttemptService** — Start attempt, submit answers, auto-grade, auto-submit on timeout.

10. **LiveSessionService** — CRUD + status lifecycle management.

11. **LiveAttendanceService** — Join/leave tracking.

12. **VideoCommentService** — Comment CRUD with flattening + like toggle.

### Phase 3: Deletion Lifecycle

13. **Update CategoryService** — Add safety guards (block if has courses/subcategories).

14. **Add CourseService.DeleteCourseAsync** — Draft cascade vs. published soft-delete.

15. **Update SectionService** — Switch from hard delete to soft delete for published courses.

16. **DeletionAuditService** — Log all deletion operations to ActivityLog.

### Phase 4: Controllers & Registration

17. **Create Controllers** — EnrollmentController, ContentProgressController, VideoContentController, DocumentController, QuizManagementController, QuizAttemptController, LiveSessionController, LiveAttendanceController, VideoCommentController.

18. **Update Existing Controllers** — Add delete endpoints to CategoryController, CourseManagementController.

19. **Register Services in Program.cs** — Add all new service DI registrations.

## Running the Application

```bash
cd backend_project
dotnet ef migrations add ContentLearningDeletion
dotnet ef database update
dotnet run
```

## Verification

1. Create a course in draft status
2. Add sections and section items (video, quiz, document)
3. Publish the course
4. Create an enrollment
5. Update content progress
6. Verify enrollment progress recalculates
7. Take a quiz and verify grading
8. Attempt to delete the published course → verify soft delete
9. Attempt to delete a category with courses → verify block
