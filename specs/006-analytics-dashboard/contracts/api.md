# API Contracts: Analytics Dashboard

All endpoints follow existing platform conventions: JWT auth, `ApiResponse<T>` wrapper, FluentValidation.

---

## Student Dashboard

### GET /api/dashboard/student — Student learning overview

**Auth**: Required (Student role)

**Response 200**:
```json
{
  "success": true,
  "data": {
    "totalEnrolledCourses": 5,
    "inProgressCourses": 2,
    "completedCourses": 3,
    "totalLearningHours": 47.5,
    "certificatesEarned": 2,
    "recentEnrollments": [
      {
        "enrollmentId": "guid",
        "courseId": "guid",
        "courseTitle": "C# Masterclass",
        "progressPercentage": 75.0,
        "status": "InProgress",
        "lastAccessedAt": "2026-05-23T10:00:00Z",
        "certificateId": null
      }
    ],
    "certificateEligibleCourses": [
      {
        "enrollmentId": "guid",
        "courseId": "guid",
        "courseTitle": "ASP.NET Core",
        "progressPercentage": 100.0,
        "status": "Completed",
        "lastAccessedAt": "2026-05-20T10:00:00Z",
        "certificateId": null
      }
    ]
  }
}
```

**Errors**: 401 if not authenticated, 403 if wrong role.

---

## Instructor Dashboard

### GET /api/dashboard/instructor — Teaching overview

**Auth**: Required (Instructor role)

**Response 200**:
```json
{
  "success": true,
  "data": {
    "totalStudents": 342,
    "publishedCourses": 4,
    "totalTeachingHours": 120,
    "averageRating": 4.5,
    "totalRevenue": 12500.00,
    "grossRevenue": 25000.00,
    "courses": [
      {
        "id": "guid",
        "title": "C# Masterclass",
        "status": "Published",
        "isPublished": true,
        "enrollmentCount": 150,
        "averageRating": 4.7,
        "price": 199.99,
        "createdAt": "2026-01-15T10:00:00Z"
      }
    ],
    "pendingEditRequests": [
      {
        "id": "guid",
        "courseTitle": "C# Masterclass",
        "requestType": "ContentUpdate",
        "operation": "Update",
        "status": "Pending",
        "requestedAt": "2026-05-22T10:00:00Z",
        "isEmergency": false
      }
    ]
  }
}
```

### GET /api/management/courses — Instructor's course list

**Auth**: Required (Instructor role)

**Query**: `?page=1&pageSize=20`

**Response 200**:
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "title": "C# Masterclass",
        "status": "Published",
        "isPublished": true,
        "enrollmentCount": 150,
        "averageRating": 4.7,
        "price": 199.99,
        "createdAt": "2026-01-15T10:00:00Z"
      }
    ],
    "page": 1,
    "pageSize": 20,
    "totalCount": 4,
    "totalPages": 1
  }
}
```

**Note**: This is a separate endpoint from the public course listing. It exposes instructor-only data (drafts, pending reviews, rejection reasons, enrollment counts).

---

## Admin Dashboard

### GET /api/admin/dashboard/overview — Platform overview

**Auth**: Required (Admin role)

**Response 200**:
```json
{
  "success": true,
  "data": {
    "totalUsers": 15000,
    "totalCourses": 120,
    "totalRevenue": 450000.00,
    "pendingCourseApprovals": 5,
    "pendingTeacherRequests": 3,
    "activeInstructors": 45
  }
}
```

### GET /api/admin/dashboard/revenue?months=12 — Monthly revenue breakdown

**Auth**: Required (Admin role)

**Response 200**:
```json
{
  "success": true,
  "data": [
    { "month": "2026-05", "grossAmount": 45000.00, "netAmount": 22500.00 },
    { "month": "2026-04", "grossAmount": 42000.00, "netAmount": 21000.00 }
  ]
}
```

### GET /api/admin/dashboard/user-growth?months=6 — User growth trends

**Auth**: Required (Admin role)

**Response 200**:
```json
{
  "success": true,
  "data": [
    { "month": "2026-05", "newUsers": 1200, "cumulativeTotal": 15000 },
    { "month": "2026-04", "newUsers": 1100, "cumulativeTotal": 13800 }
  ]
}
```

### GET /api/admin/dashboard/enrollment-trends?months=12 — Enrollment trends

**Auth**: Required (Admin role)

**Response 200**:
```json
{
  "success": true,
  "data": [
    { "month": "2026-05", "enrollments": 800 },
    { "month": "2026-04", "enrollments": 750 }
  ]
}
```

---

## Shared Error Response (Degraded Mode)

When a sub-aggregation fails:

```json
{
  "success": true,
  "data": {
    "stats": { ... },
    "errors": [
      { "section": "revenue", "message": "Revenue service temporarily unavailable" }
    ]
  }
}
```
