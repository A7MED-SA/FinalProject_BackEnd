# Admin — API Reference

**Controllers**: `AdminDashboardController`, `AdminCourseController`, `AdminCertificatesController`, `AdminRefundController`, `AdminPaymentMethodController`, `AdminCouponController`, `AdminMediaController`

**Auth**: Admin (جميع endpoints)

---

## Admin Dashboard

**Base**: `/api/admin/dashboard`

### Overview
**GET** `/api/admin/dashboard/overview`
```json
{
  "totalUsers": 5000,
  "totalStudents": 4500,
  "totalInstructors": 200,
  "totalCourses": 150,
  "publishedCourses": 120,
  "pendingCourses": 10,
  "totalRevenue": 500000.00,
  "totalEnrollments": 15000,
  "newUsersThisMonth": 300,
  "newEnrollmentsThisMonth": 800
}
```

### Monthly Revenue
**GET** `/api/admin/dashboard/revenue?months=12`

### User Growth
**GET** `/api/admin/dashboard/user-growth?months=6`

### Enrollment Trends
**GET** `/api/admin/dashboard/enrollment-trends?months=12`

---

## Course Management (Admin)

**Base**: `/api/admin/courses`

### Pending Courses
**GET** `/api/admin/courses/pending`

### Approve Course
**POST** `/api/admin/courses/{id}/approve`

### Reject Course
**POST** `/api/admin/courses/{id}/reject`
```json
{ "reason": "المحتوى غير مكتمل" }
```

### Edit Requests
**GET** `/api/admin/courses/edit-requests?status=pending&page=1&pageSize=20`
**GET** `/api/admin/courses/edit-requests/{requestId}`
**POST** `/api/admin/courses/edit-requests/{requestId}/review`
```json
{ "approve": true, "notes": "التعديلات مقبولة" }
```

---

## Certificates (Admin)

**Base**: `/api/admin/AdminCertificates` (تنبيه: route هو `[controller]`)

### Get All
**GET** `/api/admin/AdminCertificates?page=1&pageSize=20`

### Revoke
**POST** `/api/admin/AdminCertificates/{id}/revoke`
```json
{ "reason": "انتهاك الشروط" }
```

### Issue Manually
**POST** `/api/admin/AdminCertificates/issue`
```json
{
  "userId": "guid",
  "courseId": "guid",
  "reason": "إصدار يدوي"
}
```

---

## Refunds (Admin)

**Base**: `/api/admin/refunds`

### Get All
**GET** `/api/admin/refunds?status=Requested`
Filters: `null` (الكل), `Requested`, `Approved`, `Rejected`

### Approve
**POST** `/api/admin/refunds/{id}/approve`

### Reject
**POST** `/api/admin/refunds/{id}/reject`

---

## Payment Methods (Admin)

**Base**: `/api/admin/payment-methods`

### Get All
**GET** `/api/admin/payment-methods`

### Create
**POST** `/api/admin/payment-methods`
```json
{
  "name": "Visa / Mastercard",
  "provider": "CHIPS",
  "description": "الدفع عبر البطاقات الائتمانية",
  "isActive": true,
  "config": { "merchantId": "...", "apiKey": "..." }
}
```

### Toggle Active
**PATCH** `/api/admin/payment-methods/{id}/toggle`

---

## Coupons (Admin)

**Base**: `/api/admin/coupons`

### Get All
**GET** `/api/admin/coupons?isActive=true`

### Get By Id
**GET** `/api/admin/coupons/{id}`

### Create
**POST** `/api/admin/coupons`
```json
{
  "code": "SAVE20",
  "discountType": "Percentage",
  "discountValue": 20,
  "maxUsageCount": 100,
  "maxUsagePerUser": 1,
  "expiresAt": "2026-12-31T23:59:59Z",
  "minCartTotal": 100.00,
  "applicableCourseIds": ["guid"]
}
```

### Update
**PUT** `/api/admin/coupons/{id}`

### Toggle Active
**PATCH** `/api/admin/coupons/{id}/toggle`

### Delete
**DELETE** `/api/admin/coupons/{id}`

---

## Media (Admin)

**Base**: `/api/admin/media`

### List
**GET** `/api/admin/media?userId=guid&purpose=CourseVideo&isDeleted=false&page=1&pageSize=20`

### Details
**GET** `/api/admin/media/{fileId}`

### Soft Delete
**DELETE** `/api/admin/media/{fileId}`

### Restore
**POST** `/api/admin/media/{fileId}/restore`

### Permanent Delete
**DELETE** `/api/admin/media/{fileId}/permanent`
⚠️ خطر — لا يمكن التراجع

### Storage Stats
**GET** `/api/admin/media/stats`
