# API Contracts: course-features

## Public Course Service (No Authentication)

### `GET /api/public/courses`
Fetches a paginated list of published courses.

**Query Parameters:**
- `SearchQuery` (string, optional)
- `CategoryId` (Guid, optional)
- `Level` (CourseLevel, optional)
- `Language` (string, optional)
- `MinPrice` (decimal, optional)
- `MaxPrice` (decimal, optional)
- `IsFreeOnly` (bool, optional)
- `MinRating` (float, optional)
- `SortBy` (enum, default: `PublishedAt`)
- `SortDescending` (bool, default: `true`)
- `Page` (int, default: 1)
- `PageSize` (int, default: 12)

**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "items": [ /* PublicCourseDto */ ],
    "totalCount": 48,
    "page": 1,
    "pageSize": 12,
    "totalPages": 4
  }
}
```

### `GET /api/public/courses/{id}`
Fetches detailed info for a published course by ID.

**Response:** `200 OK`
```json
{
  "success": true,
  "data": { /* PublicCourseDetailDto */ }
}
```

### `GET /api/public/courses/slug/{slug}`
Fetches detailed info for a published course by Slug.

### `GET /api/public/courses/search/suggest`
Quick autocomplete search.

**Query Parameters:**
- `query` (string, required)
- `limit` (int, default: 5)

### `GET /api/public/courses/stats`
Gets platform-wide stats.

### `GET /api/public/courses/{id}/related`
Gets related courses.

### `GET /api/public/courses/filters/options`
Gets available filter dropdown options.

---

## Course Edit Approval Workflow (Instructor)

*Requires `[Authorize(Roles = "Instructor")]`*

### `PUT /api/courses/{courseId}/sections/{sectionId}`
Modifies a section. If published, evaluates edit policy.

**Response:**
- `200 OK` (Applied immediately)
- `202 Accepted` (Pending approval) -> Returns `EditResultDto`

### `DELETE /api/courses/{courseId}/sections/{sectionId}`
Deletes a section. If published, requires approval.

### `GET /api/courses/my-edit-requests`
Lists edit requests submitted by the current instructor.

### `POST /api/courses/edit-requests/{requestId}/cancel`
Cancels a pending edit request.

---

## Course Edit Approval Workflow (Admin)

*Requires `[Authorize(Roles = "Admin")]`*

### `GET /api/admin/courses/edit-requests/pending`
Lists all pending edit requests.

### `GET /api/admin/courses/edit-requests/{requestId}`
Gets full details of an edit request (including diffs and student impact).

### `POST /api/admin/courses/edit-requests/{requestId}/review`
Approves or rejects a request.

**Payload:**
```json
{
  "approve": true,
  "notes": "Looks good"
}
```
