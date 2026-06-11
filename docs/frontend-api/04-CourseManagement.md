# Course Management (Instructor) — API Reference

**Controllers**: `CourseManagementController`, `SectionController`, `DocumentController`, `VideoContentController`, `QuizManagementController`, `LiveSessionController`, `ManagementCoursesController`

**Base URLs**: `/api/management/courses`, `/api/management`, `/api/courses/{courseId}/...`
**Auth**: JWT (Instructor)

---

## My Courses (List)

**GET** `/api/management/courses?page=1&pageSize=20`
**Auth**: Instructor

```json
{
  "success": true,
  "data": {
    "items": [{ "id": "guid", "title": "...", "status": "Draft", "studentsCount": 0, "rating": 0, "price": 199.99 }],
    "page": 1, "pageSize": 20, "totalCount": 5
  }
}
```

---

## Course CRUD

### Create Course
**POST** `/api/management/courses`
```json
{
  "title": "دورة تعلم البرمجة",
  "slug": "learn-programming",
  "shortDescription": "...",
  "description": "نص كامل",
  "price": 199.99,
  "level": "Beginner",
  "language": "Arabic",
  "categoryId": "guid"
}
```

### Get Course By Id
**GET** `/api/management/courses/{id}`

### Update Course
**PUT** `/api/management/courses/{id}`

### Delete Course
**DELETE** `/api/management/courses/{id}`

### Set Course Image
**PUT** `/api/management/courses/{courseId}/image`
```json
{ "fileId": "guid" }
```

---

## Requirements

### Add Requirement
**POST** `/api/management/courses/{id}/requirements`
```json
{ "text": "معرفة أساسية بالحاسوب" }
```

### Remove Requirement
**DELETE** `/api/management/courses/{id}/requirements/{requirementId}`

---

## Learning Outcomes

### Add Outcome
**POST** `/api/management/courses/{id}/outcomes`
```json
{ "text": "بناء تطبيق ويب كامل" }
```

### Remove Outcome
**DELETE** `/api/management/courses/{id}/outcomes/{outcomeId}`

---

## Submit for Review

**POST** `/api/management/courses/{id}/submit-for-review`

تغيير حالة الكورس إلى "Pending Review" لمراجعته من الأدمن.

---

## Scheduled Deletion

### Schedule
**POST** `/api/management/courses/{id}/schedule-deletion`
```json
{ "scheduledDate": "2026-06-25T00:00:00Z", "reason": "تحديث المحتوى" }
```

### Cancel
**POST** `/api/management/courses/{id}/cancel-scheduled-deletion`

### Get Status
**GET** `/api/management/courses/{id}/deletion-status`

---

## Sections

**Base**: `/api/management/courses/{courseId}/sections`

### Get All
**GET** `/api/management/courses/{courseId}/sections`

### Create
**POST** `/api/management/courses/{courseId}/sections`
```json
{ "title": "المقدمة", "order": 1 }
```

### Get By Id
**GET** `/api/management/courses/{courseId}/sections/{sectionId}`

### Update
**PUT** `/api/management/courses/{courseId}/sections/{sectionId}`

### Delete
**DELETE** `/api/management/courses/{courseId}/sections/{sectionId}`

### Reorder
**PUT** `/api/management/courses/{courseId}/sections/reorder`
```json
{ "items": [{ "id": "guid", "order": 1 }, { "id": "guid", "order": 2 }] }
```

---

## Section Items

### Add Item
**POST** `/api/management/courses/{courseId}/sections/{sectionId}/items`
```json
{ "title": "فيديو 1", "type": "Video", "contentId": "guid", "order": 1 }
```
Types: `Video`, `Document`, `Quiz`, `LiveSession`

### Update Item
**PUT** `/api/management/courses/{courseId}/sections/{sectionId}/items/{itemId}`

### Delete Item
**DELETE** `/api/management/courses/{courseId}/sections/{sectionId}/items/{itemId}`

### Reorder Items
**PUT** `/api/management/courses/{courseId}/sections/{sectionId}/items/reorder`

---

## Edit Requests (Instructor)

### My Edit Requests
**GET** `/api/courses/my-edit-requests?page=1&pageSize=20`

### Cancel
**POST** `/api/courses/edit-requests/{requestId}/cancel`

---

## Documents

**Base**: `/api/courses/{courseId}/documents`
**Auth**: Instructor

### Create
**POST** `/api/courses/{courseId}/documents`
```json
{
  "title": "ملف الدرس الأول",
  "description": "شرح إضافي",
  "fileId": "guid",
  "sectionItemId": "guid"
}
```

### Get
**GET** `/api/courses/{courseId}/documents/{id}`

### Update
**PUT** `/api/courses/{courseId}/documents/{id}`

### Delete
**DELETE** `/api/courses/{courseId}/documents/{id}`

---

## Videos

**Base**: `/api/courses/{courseId}/videos`
**Auth**: Instructor

### Create
**POST** `/api/courses/{courseId}/videos`
```json
{
  "title": "فيديو الدرس الأول",
  "description": "...",
  "fileId": "guid",
  "durationMinutes": 15,
  "sectionItemId": "guid"
}
```

### Get
**GET** `/api/courses/{courseId}/videos/{id}`

### Update
**PUT** `/api/courses/{courseId}/videos/{id}`

### Delete
**DELETE** `/api/courses/{courseId}/videos/{id}`

---

## Quizzes

**Base**: `/api/courses/{courseId}/quizzes`
**Auth**: Instructor

### Create
**POST** `/api/courses/{courseId}/quizzes`
```json
{
  "title": "اختبار الفصل الأول",
  "description": "...",
  "passingScore": 70,
  "timeLimitMinutes": 30,
  "sectionItemId": "guid"
}
```

### Get
**GET** `/api/courses/{courseId}/quizzes/{id}`

### Update
**PUT** `/api/courses/{courseId}/quizzes/{id}`

### Delete
**DELETE** `/api/courses/{courseId}/quizzes/{id}`

### Add Question
**POST** `/api/courses/{courseId}/quizzes/{quizId}/questions`
```json
{
  "text": "ما هي لغة البرمجة المستخدمة؟",
  "type": "MultipleChoice",
  "points": 10,
  "options": [
    { "text": "JavaScript", "isCorrect": true },
    { "text": "HTML", "isCorrect": false }
  ]
}
```

### Update Question
**PUT** `/api/courses/{courseId}/quizzes/{quizId}/questions/{questionId}`

### Delete Question
**DELETE** `/api/courses/{courseId}/quizzes/{quizId}/questions/{questionId}`

---

## Live Sessions

**Base**: `/api/courses/{courseId}/live-sessions`
**Auth**: Instructor

### Get All
**GET** `/api/courses/{courseId}/live-sessions`

### Create
**POST** `/api/courses/{courseId}/live-sessions`
```json
{
  "title": "محاضرة مباشرة 1",
  "description": "...",
  "scheduledAt": "2026-06-01T15:00:00Z",
  "durationMinutes": 60,
  "meetingUrl": "https://zoom.us/j/...",
  "sectionItemId": "guid"
}
```

### Update Status
**PUT** `/api/courses/{courseId}/live-sessions/{sessionId}/status`
```json
{ "status": "Live" }
```
Status: `Scheduled`, `Live`, `Ended`, `Cancelled`

### Delete
**DELETE** `/api/courses/{courseId}/live-sessions/{sessionId}`
