# API Contracts: Content & Learning System with Deletion Lifecycle

**Date**: 2026-05-14  
**Base URL**: `/api`  
**Auth**: JWT Bearer (unless marked `[AllowAnonymous]`)

## Response Envelope

All responses use the existing `ApiResponse<T>` wrapper:
```json
{
  "success": true,
  "message": "...",
  "data": { ... },
  "errors": []
}
```

---

## 1. Enrollment Controller

**Path prefix**: `/api/enrollments`  
**Auth**: `[Authorize]` (Student role)

### POST `/api/enrollments`
Create enrollment (via purchase, admin grant, coupon).

**Request**:
```json
{
  "courseId": "guid",
  "source": "Purchase|Gift|AdminGrant|Coupon",
  "couponCode": "string?" 
}
```

**Response** (201): `EnrollmentResponseDto`
```json
{
  "id": "guid",
  "courseId": "guid",
  "courseTitle": "string",
  "status": "InProgress",
  "enrolledAt": "datetime",
  "progressPercentage": 0,
  "accessExpiresAt": "datetime?"
}
```

**Error Cases**: 400 (already enrolled), 404 (course not found), 403 (course not published)

### GET `/api/enrollments`
Get current user's enrollments.

**Query**: `?status=InProgress&page=1&pageSize=10`

**Response** (200): `PagedResult<EnrollmentResponseDto>`

### GET `/api/enrollments/{enrollmentId}`
Get enrollment details with progress breakdown.

**Response** (200): `EnrollmentDetailDto`
```json
{
  "id": "guid",
  "courseId": "guid",
  "courseTitle": "string",
  "status": "InProgress",
  "progressPercentage": 65.5,
  "contentProgresses": [
    {
      "contentType": "Video",
      "contentId": "guid",
      "contentTitle": "string",
      "isCompleted": true,
      "completionPercentage": 100,
      "lastAccessedAt": "datetime"
    }
  ]
}
```

---

## 2. Content Progress Controller

**Path prefix**: `/api/enrollments/{enrollmentId}/progress`  
**Auth**: `[Authorize]` (Student role, must own enrollment)

### PUT `/api/enrollments/{enrollmentId}/progress`
Update progress for a content item.

**Request**:
```json
{
  "contentType": "Video|Quiz|Document|LiveSession",
  "contentId": "guid",
  "watchTimeSeconds": 120,
  "completionPercentage": 45.5
}
```

**Response** (200): `ContentProgressDto` + recalculated `enrollmentProgress`

### GET `/api/enrollments/{enrollmentId}/progress`
Get all progress for an enrollment.

**Response** (200): `List<ContentProgressDto>`

---

## 3. Video Content Controller (Instructor)

**Path prefix**: `/api/courses/{courseId}/videos`  
**Auth**: `[Authorize(Roles = "Instructor")]` (must own course)

### POST `/api/courses/{courseId}/videos`
Create a video content item.

**Request**:
```json
{
  "title": "string",
  "videoFileId": "guid",
  "thumbnailFileId": "guid?",
  "provider": "Local|YouTube|Vimeo|Minio",
  "providerVideoId": "string?",
  "quality": "_720p|_1080p|_4k",
  "durationSeconds": 600,
  "transcript": "string?",
  "sectionId": "guid",
  "position": 1,
  "isPreviewAllowed": false,
  "isMandatory": true
}
```

**Response** (201): `VideoResponseDto`

### PUT `/api/courses/{courseId}/videos/{videoId}`
Update video details (goes through edit approval if course is published).

### DELETE `/api/courses/{courseId}/videos/{videoId}`
Delete video (cascade for draft, approval workflow for published).

**Response** (200): `{ "message": "Deleted" }` or `{ "editRequestId": "guid", "message": "Deletion requires approval" }`

---

## 4. Document Content Controller (Instructor)

**Path prefix**: `/api/courses/{courseId}/documents`  
**Auth**: `[Authorize(Roles = "Instructor")]`

### POST `/api/courses/{courseId}/documents`
### PUT `/api/courses/{courseId}/documents/{documentId}`
### DELETE `/api/courses/{courseId}/documents/{documentId}`

*Same pattern as Video endpoints.*

---

## 5. Quiz Management Controller (Instructor)

**Path prefix**: `/api/courses/{courseId}/quizzes`  
**Auth**: `[Authorize(Roles = "Instructor")]`

### POST `/api/courses/{courseId}/quizzes`
Create a quiz with settings.

**Request**:
```json
{
  "title": "string",
  "description": "string?",
  "durationMinutes": 30,
  "passingScorePercent": 60,
  "maxAttempts": 3,
  "shuffleQuestions": true,
  "shuffleOptions": true,
  "showResultsImmediately": true,
  "allowReview": true,
  "sectionId": "guid",
  "position": 2,
  "isMandatory": true
}
```

### POST `/api/courses/{courseId}/quizzes/{quizId}/questions`
Add a question to a quiz.

**Request**:
```json
{
  "questionText": "What is...?",
  "type": "MultipleChoice|TrueFalse|ShortAnswer",
  "points": 5,
  "explanation": "string?",
  "position": 1,
  "options": [
    { "optionText": "Answer A", "isCorrect": false, "position": 1 },
    { "optionText": "Answer B", "isCorrect": true, "position": 2 }
  ]
}
```

### DELETE `/api/courses/{courseId}/quizzes/{quizId}`
Delete quiz (blocked if has in-progress attempts).

---

## 6. Quiz Taking Controller (Student)

**Path prefix**: `/api/enrollments/{enrollmentId}/quizzes/{quizId}`  
**Auth**: `[Authorize]` (Student, must own enrollment)

### POST `/api/enrollments/{enrollmentId}/quizzes/{quizId}/attempts`
Start a new quiz attempt.

**Response** (201): `QuizAttemptDto` with questions (shuffled if configured)

### PUT `/api/enrollments/{enrollmentId}/quizzes/{quizId}/attempts/{attemptId}`
Submit quiz attempt.

**Request**:
```json
{
  "answers": [
    { "questionId": "guid", "selectedOptionId": "guid?", "answerText": "string?" }
  ]
}
```

**Response** (200): `QuizResultDto`
```json
{
  "attemptId": "guid",
  "score": 80,
  "maxScore": 100,
  "passed": true,
  "status": "Graded",
  "timeTakenSeconds": 1200,
  "isAutoSubmitted": false,
  "results": [
    {
      "questionId": "guid",
      "questionText": "...",
      "selectedOptionId": "guid",
      "correctOptionId": "guid",
      "isCorrect": true,
      "pointsEarned": 5,
      "explanation": "..."
    }
  ]
}
```

*Note: `results` array is only returned if `showResultsImmediately` is true.*

---

## 7. Live Session Controller (Instructor)

**Path prefix**: `/api/courses/{courseId}/live-sessions`  
**Auth**: `[Authorize(Roles = "Instructor")]`

### POST `/api/courses/{courseId}/live-sessions`
Schedule a live session.

**Request**:
```json
{
  "title": "string",
  "description": "string?",
  "scheduledStart": "datetime",
  "scheduledEnd": "datetime",
  "meetingUrl": "string",
  "password": "string?",
  "maxAttendees": 100,
  "sectionId": "guid?",
  "position": 3
}
```

### PUT `/api/courses/{courseId}/live-sessions/{sessionId}/status`
Update session status (start/end/cancel).

**Request**: `{ "status": "Live|Finished|Cancelled" }`

### DELETE `/api/courses/{courseId}/live-sessions/{sessionId}`
Delete session (blocked if status = Live).

---

## 8. Live Attendance Controller (Student)

**Path prefix**: `/api/live-sessions/{sessionId}/attendance`  
**Auth**: `[Authorize]` (Student, must be enrolled in session's course)

### POST `/api/live-sessions/{sessionId}/attendance/join`
Record student joining a live session.

### POST `/api/live-sessions/{sessionId}/attendance/leave`
Record student leaving a live session.

---

## 9. Video Comment Controller

**Path prefix**: `/api/videos/{videoId}/comments`  
**Auth**: `[Authorize]` (must be enrolled in the video's course)

### GET `/api/videos/{videoId}/comments`
Get comments with replies (paginated).

**Query**: `?page=1&pageSize=20`

### POST `/api/videos/{videoId}/comments`
Post a comment or reply.

**Request**:
```json
{
  "content": "string",
  "parentCommentId": "guid?"
}
```

### DELETE `/api/videos/{videoId}/comments/{commentId}`
Soft-delete a comment (author or admin only).

### POST `/api/videos/{videoId}/comments/{commentId}/like`
Toggle like on a comment.

---

## 10. Deletion Endpoints (Updates to Existing Controllers)

### DELETE `/api/categories/{categoryId}` (Admin only)
- Empty → hard delete
- Has courses/subcategories → 409 Conflict with message

### DELETE `/api/courses/{courseId}/management` (Instructor)
- Draft → hard cascade delete
- Published (no enrollments) → soft delete
- Published (with enrollments) → soft delete + read-only access preserved

### DELETE `/api/courses/{courseId}/sections/{sectionId}` (Existing, updated)
- Draft → hard delete
- Published → routes through edit approval workflow

### DELETE `/api/courses/{courseId}/sections/{sectionId}/items/{itemId}` (Existing, updated)
- Draft → hard delete item + content
- Published → routes through edit approval workflow
