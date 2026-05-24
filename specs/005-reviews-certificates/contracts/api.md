# API Contracts: Reviews & Certificates

All endpoints follow existing platform conventions: JWT auth, `ApiResponse<T>` wrapper, FluentValidation.

---

## Reviews

### POST /api/reviews — Submit a review

**Auth**: Required (Student role)

**Request**:
```json
{
  "courseId": "guid",
  "rating": 4,
  "content": "Great course! (optional, max 2000 chars)"
}
```

**Response 201**:
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "courseId": "guid",
    "rating": 4,
    "content": "Great course!",
    "createdAt": "2026-05-23T10:00:00Z"
  },
  "message": "Review submitted."
}
```

**Errors**: 400 if already reviewed this course, not completed, or validation fails.

---

### PUT /api/reviews/{id} — Edit a review

**Auth**: Required (owner only)

**Request**:
```json
{
  "rating": 5,
  "content": "Even better than I thought!"
}
```

**Response 200**: Updated review DTO.

**Errors**: 404 if not found, 403 if not owner.

---

### DELETE /api/reviews/{id} — Delete own review

**Auth**: Required (owner only)

**Response**: `204 No Content`

---

### GET /api/courses/{courseId}/reviews — List course reviews (public)

**Auth**: None (public)

**Query**: `?page=1&pageSize=10`

**Response 200**:
```json
{
  "success": true,
  "data": {
    "averageRating": 4.2,
    "totalCount": 25,
    "items": [
      {
        "id": "guid",
        "userName": "Ahmed E.",
        "rating": 5,
        "content": "Excellent!",
        "createdAt": "2026-05-20T10:00:00Z"
      }
    ],
    "page": 1,
    "pageSize": 10,
    "totalPages": 3
  }
}
```

**Note**: Only shows non-hidden, non-flagged reviews to the public. Includes user's first name + last initial for privacy.

---

### POST /api/reviews/{id}/flag — Instructor flags a review

**Auth**: Required (Instructor role, owner of the course)

**Response 200**: Review flagged for admin moderation.

---

### GET /api/admin/reviews/flagged — Admin views flagged reviews

**Auth**: Required (Admin role)

**Query**: `?page=1&pageSize=20`

**Response 200**: Paginated list of flagged + hidden reviews.

---

### POST /api/admin/reviews/{id}/hide — Admin hides a review

**Auth**: Required (Admin role)

**Response 200**: Review soft-deleted (hidden from public).

**Body**: `{ "reason": "Inappropriate content" }`

---

### POST /api/admin/reviews/{id}/dismiss — Admin dismisses a flag

**Auth**: Required (Admin role)

**Response 200**: Flag removed, review stays visible.

---

## Certificates

### GET /api/certificates — List my certificates

**Auth**: Required (Student role)

**Response 200**:
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "courseTitle": "C# Masterclass",
        "code": "CERT-A3F8B2C1",
        "status": "Valid",
        "completedAt": "2026-05-20T10:00:00Z",
        "downloadUrl": "/api/certificates/{id}/download",
        "verifyUrl": "/api/certificates/verify/CERT-A3F8B2C1"
      }
    ],
    "count": 5
  }
}
```

---

### GET /api/certificates/{id} — Certificate details

**Auth**: Required (owner only)

**Response 200**: Full certificate DTO with all fields.

---

### GET /api/certificates/{id}/download — Download PDF

**Auth**: Required (owner only)

**Response**: `application/pdf` binary stream.

---

### GET /api/certificates/verify/{code} — Public verification

**Auth**: None (public)

**Response 200** (valid):
```json
{
  "success": true,
  "data": {
    "holderName": "Ahmed Elewa",
    "courseTitle": "C# Masterclass",
    "completedAt": "2026-05-20T10:00:00Z",
    "status": "Valid"
  }
}
```

**Response 200** (revoked):
```json
{
  "success": true,
  "data": {
    "holderName": "Ahmed Elewa",
    "courseTitle": "C# Masterclass",
    "completedAt": "2026-05-20T10:00:00Z",
    "status": "Revoked",
    "revokedAt": "2026-05-22T10:00:00Z"
  }
}
```

**Response 404**:
```json
{
  "success": false,
  "message": "Certificate not found.",
  "data": null
}
```

---

### POST /api/admin/certificates/{id}/revoke — Admin revoke certificate

**Auth**: Required (Admin role)

**Response 200**: Certificate status changed to Revoked.

---

## Integration Points

### On Course Completion (ContentProgressService → CertificateService)

When `RecalculateEnrollmentProgressAsync` transitions enrollment to `Completed`, call:
```
ICertificateService.GenerateCertificateAsync(enrollmentId)
```

### On Refund (RefundService → CertificateService)

When `ApproveRefundAsync` sets enrollment to `Refunded`, call:
```
ICertificateService.RevokeByEnrollmentAsync(enrollmentId)
```
