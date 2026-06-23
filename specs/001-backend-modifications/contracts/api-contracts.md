# API Contracts: Backend Modifications

**Date**: 2026-06-23  
**Feature**: Backend Modifications  
**Spec**: [spec.md](./spec.md)

## Response Envelope

All endpoints return responses wrapped in:

```json
{
  "success": true,
  "message": "optional message",
  "data": {},
  "errors": []
}
```

---

## 1. Public Instructor Endpoints

### GET /api/public/instructors/{slug}

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "fullName": "Ahmed Ali",
    "slug": "ahmed-ali",
    "bio": "...",
    "nationality": "Egyptian",
    "profileImageUrl": "url",
    "createdAt": "2026-01-01T00:00:00Z"
  }
}
```

**Errors**:
- 404: Instructor not found

### GET /api/public/instructors/check-slug?slug={slug}

**Auth**: Required (Bearer token)

**Response** (200 OK):
```json
{
  "success": true,
  "data": true  // true if available, false if taken
}
```

### GET /api/public/instructors/search?q={query}

**Response** (200 OK):
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "fullName": "Ahmed Ali",
      "slug": "ahmed-ali",
      "bio": "...",
      "profileImageUrl": "url"
    }
  ]
}
```

---

## 2. Landing Aggregated Endpoint

### GET /api/public/landing

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "stats": {
      "totalStudents": 1000,
      "totalCourses": 50,
      "totalInstructors": 10,
      "satisfactionRate": 92.5,
      "totalVideoHours": 500,
      "totalCertificatesIssued": 200
    },
    "categories": [],
    "featuredCourses": [],
    "upcomingLiveSessions": [],
    "testimonials": []
  }
}
```

---

## 3. Testimonials Endpoints

### GET /api/public/testimonials

**Query Params**: page (default 1), pageSize (default 10), minRating (optional)

**Response** (200 OK):
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "content": "Great platform!",
      "rating": 5,
      "userName": "Student Name",
      "userAvatar": "url",
      "displayOrder": 1,
      "createdAt": "2026-01-01T00:00:00Z"
    }
  ]
}
```

### POST /api/public/testimonials

**Auth**: Required

**Request**:
```json
{
  "content": "Great platform! (min 10 chars)",
  "rating": 5
}
```

**Response**: 201 Created (new) or 200 OK (updated)

### PATCH /api/admin/testimonials/{id}/approve

**Auth**: Admin role required

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Testimonial approved"
}
```

### PATCH /api/admin/testimonials/{id}/flag

**Auth**: Admin role required

**Request**:
```json
"Spam content"
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Testimonial flagged"
}
```

---

## 4. Notification Preferences Endpoints

### GET /api/notifications/preferences

**Auth**: Required

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "emailNotifications": true,
    "pushNotifications": true,
    "courseUpdates": true,
    "marketingEmails": false,
    "newMessageAlerts": true,
    "liveSessionReminders": true,
    "quizReminders": true,
    "certificateAchievements": true,
    "announcementAlerts": true
  }
}
```

### PUT /api/notifications/preferences

**Auth**: Required

**Request** (partial update):
```json
{
  "marketingEmails": false
}
```

**Response** (200 OK): Updated preferences

---

## 5. Contact Form Endpoints

### POST /api/public/contact

**Auth**: None (anonymous)

**Rate Limit**: 5 per minute per IP

**Request**:
```json
{
  "fullName": "Test User",
  "email": "test@example.com",
  "phone": "+1234567890",  // optional
  "subject": "Question",
  "message": "Hello, I have a question."
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Message sent successfully"
}
```

**Errors**:
- 400: Spam detected
- 429: Rate limit exceeded

### GET /api/admin/contact

**Auth**: Admin role required

**Query Params**: isRead (optional boolean)

**Response** (200 OK):
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "fullName": "Test User",
      "email": "test@example.com",
      "subject": "Question",
      "message": "Hello...",
      "isRead": false,
      "createdAt": "2026-01-01T00:00:00Z"
    }
  ]
}
```

### PUT /api/admin/contact/{id}/mark-read

**Auth**: Admin role required

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Message marked as read"
}
```

---

## 6. About Page Endpoint

### GET /api/public/about

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "title": "About Athary",
    "description": "...",
    "mission": "...",
    "vision": "...",
    "stats": {
      "manuscriptsCount": 200,
      "learnersCount": 10000,
      "yearsOfExperience": 14
    }
  }
}
```

---

## 7. Legal Pages Endpoints

### GET /api/public/legal/{type}

**Path Params**: type (privacy | terms | refund)

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "type": "privacy",
    "title": "Privacy Policy",
    "content": "...",
    "version": "1.0",
    "lastUpdatedAt": "2026-01-01T00:00:00Z"
  }
}
```

**Errors**:
- 400: Invalid type
- 404: Page not found or not published
