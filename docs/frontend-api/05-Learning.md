# Learning (Student) — API Reference

**Controllers**: `EnrollmentsController`, `QuizAttemptController`, `VideoCommentController`, `LiveAttendanceController`

---

## Enrollments

**Base**: `/api/enrollments`
**Auth**: JWT

### Enroll in Course
**POST** `/api/enrollments`
```json
{ "courseId": "guid" }
```
**ملاحظة**: userId يُجبر على المستخدم الحالي.

### My Enrollments
**GET** `/api/enrollments`
```json
[
  {
    "id": "guid",
    "courseId": "guid",
    "courseTitle": "...",
    "enrolledAt": "...",
    "progressPercent": 45,
    "completedAt": null
  }
]
```

### Get Enrollment
**GET** `/api/enrollments/{id}`

---

## Content Progress

### Get Progress
**GET** `/api/enrollments/{enrollmentId}/progress`
```json
{
  "success": true,
  "data": [
    { "contentId": "guid", "contentType": "Video", "isCompleted": true, "completedAt": "..." },
    { "contentId": "guid", "contentType": "Quiz", "isCompleted": false, "score": null }
  ]
}
```

### Update Progress
**PUT** `/api/enrollments/{enrollmentId}/progress`
```json
{
  "contentId": "guid",
  "contentType": "Video",
  "progressPercent": 50,
  "lastPosition": 300
}
```

### Mark Completed
**POST** `/api/enrollments/{enrollmentId}/progress/{contentType}/{contentId}/complete`

ContentType values: `Video`, `Document`, `Quiz`, `LiveSession`

---

## Quiz Attempts

**Base**: `/api/enrollments/{enrollmentId}/quizzes/{quizId}/attempts`
**Auth**: JWT

### Start Attempt
**POST** `/api/enrollments/{enrollmentId}/quizzes/{quizId}/attempts`

ينشئ محاولة جديدة ويبدأ وقت الاختبار.

### Submit Attempt
**PUT** `/api/enrollments/{enrollmentId}/quizzes/{quizId}/attempts/{attemptId}`
```json
{
  "answers": [
    { "questionId": "guid", "selectedOptionIds": ["guid"] },
    { "questionId": "guid", "textAnswer": "..." }
  ]
}
```

### Get Attempts
**GET** `/api/enrollments/{enrollmentId}/quizzes/{quizId}/attempts`

### Get Result
**GET** `/api/enrollments/{enrollmentId}/quizzes/{quizId}/attempts/{attemptId}`
```json
{
  "attemptId": "guid",
  "score": 80,
  "totalPoints": 100,
  "passed": true,
  "answers": [
    { "questionId": "guid", "questionText": "...", "selectedOptions": [...], "isCorrect": true, "pointsEarned": 10 }
  ],
  "startedAt": "...",
  "submittedAt": "..."
}
```

---

## Video Comments

**Base**: `/api/videos/{videoId}/comments`
**Auth**: JWT

### Get Comments
**GET** `/api/videos/{videoId}/comments`

### Add Comment
**POST** `/api/videos/{videoId}/comments`
```json
{ "text": "شرح ممتاز!", "parentCommentId": null }
```

### Update Comment
**PUT** `/api/videos/{videoId}/comments/{commentId}`

### Delete Comment
**DELETE** `/api/videos/{videoId}/comments/{commentId}`

### Toggle Like
**POST** `/api/videos/{videoId}/comments/{commentId}/like`
```json
{ "isLiked": true }
```

---

## Live Session Attendance

**Base**: `/api/live-sessions/{sessionId}/attendance`
**Auth**: JWT

### Join Session
**POST** `/api/live-sessions/{sessionId}/attendance/join`

### Leave Session
**POST** `/api/live-sessions/{sessionId}/attendance/leave`

### Get Attendance Count
**GET** `/api/live-sessions/{sessionId}/attendance/count`
```json
{ "success": true, "data": { "count": 42 } }
```
