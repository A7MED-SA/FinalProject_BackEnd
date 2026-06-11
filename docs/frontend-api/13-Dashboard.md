# Dashboard — API Reference

**Controller**: `DashboardController`
**Base URL**: `/api/dashboard`
**Auth**: JWT

---

## Student Dashboard

**GET** `/api/dashboard/student`
**Auth**: Student

```json
{
  "success": true,
  "data": {
    "enrolledCoursesCount": 5,
    "completedCourses": 2,
    "inProgressCourses": 3,
    "averageProgressPercent": 45,
    "totalLearningHours": 120,
    "recentActivity": [
      {
        "type": "CourseEnrolled",
        "courseTitle": "دورة تعلم البرمجة",
        "date": "..."
      }
    ],
    "upcomingLiveSessions": [
      {
        "sessionId": "guid",
        "courseTitle": "...",
        "title": "محاضرة مباشرة",
        "scheduledAt": "2026-06-01T15:00:00Z"
      }
    ],
    "certificatesCount": 2
  }
}
```

---

## Instructor Dashboard

**GET** `/api/dashboard/instructor`
**Auth**: Instructor

```json
{
  "success": true,
  "data": {
    "totalCourses": 3,
    "publishedCourses": 2,
    "totalStudents": 150,
    "totalRevenue": 29985.00,
    "monthlyRevenue": [
      { "month": "2026-01", "amount": 5000.00 },
      { "month": "2026-02", "amount": 7500.00 }
    ],
    "recentEnrollments": [
      {
        "studentName": "أحمد محمد",
        "courseTitle": "...",
        "enrolledAt": "..."
      }
    ],
    "averageRating": 4.5,
    "totalReviews": 45
  }
}
```
