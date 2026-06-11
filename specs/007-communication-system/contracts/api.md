# API Contracts: Communication & System

Base URL: `/api`

## Authentication
All endpoints require JWT Bearer token in `Authorization` header.
Admin-only endpoints are marked with 🔒.

---

## 1. Messaging

### Send Message
`POST /api/messages`

**Request Body:**
```json
{
  "receiverId": "guid",
  "content": "string (max 5000 chars, plain text)"
}
```

**Response** `201 Created`:
```json
{
  "id": "guid",
  "senderId": "guid",
  "receiverId": "guid",
  "content": "string",
  "sentAt": "2026-05-25T10:00:00Z",
  "isRead": false,
  "readAt": null
}
```

**Errors:**
- `400 Bad Request` — validation failure (empty content, >5000 chars, HTML detected, self-message)
- `403 Forbidden` — no relationship between sender and receiver, and reply not permitted in existing conversation
- `429 Too Many Requests` — rate limit exceeded (30/min)

---

### Get Conversations
`GET /api/messages/conversations?page=1&pageSize=20`

**Response** `200 OK`:
```json
{
  "items": [
    {
      "otherUserId": "guid",
      "otherUserName": "string",
      "lastMessage": "string",
      "lastMessageAt": "2026-05-25T10:00:00Z",
      "unreadCount": 2
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 5
}
```

---

### Get Conversation Messages
`GET /api/messages/conversations/{otherUserId}?page=1&pageSize=50`

**Response** `200 OK`:
```json
{
  "items": [
    {
      "id": "guid",
      "senderId": "guid",
      "receiverId": "guid",
      "content": "string",
      "sentAt": "2026-05-25T10:00:00Z",
      "isRead": true,
      "readAt": "2026-05-25T10:05:00Z",
      "isDeletedForSender": false
    }
  ],
  "page": 1,
  "pageSize": 50,
  "totalCount": 10
}
```

---

### Get Unread Count
`GET /api/messages/unread-count`

**Response** `200 OK`:
```json
{
  "unreadCount": 3
}
```

---

### Mark Message as Read
`PATCH /api/messages/{messageId}/read`

**Response** `200 OK`

---

### Delete Message (Soft Delete)
`DELETE /api/messages/{messageId}`

**Response** `204 No Content`

**Note**: Soft delete — the message is hidden from sender's view but remains visible to the receiver.

---

## 2. Announcements 🔒 (Admin/Instructor)

### Create Announcement (Admin)
`POST /api/announcements`

**Request Body:**
```json
{
  "title": "string (max 255)",
  "content": "string",
  "target": "All | Students | Teachers | Admins | SpecificCourse",
  "courseId": "guid | null"
}
```

**Response** `201 Created`:
```json
{
  "id": "guid",
  "title": "string",
  "content": "string",
  "target": "All",
  "courseId": null,
  "createdBy": "guid",
  "isActive": true,
  "publishedAt": "2026-05-25T10:00:00Z"
}
```

**Errors:**
- `400 Bad Request` — validation failure (missing title, invalid target)
- `403 Forbidden` — non-admin/instructor trying to create

---

### Create Course Announcement (Instructor)
`POST /api/announcements/course/{courseId}`

**Request Body:** Same as above, `target` must be `SpecificCourse`

**Response** `201 Created`

**Errors:**
- `403 Forbidden` — instructor doesn't teach the course

---

### Get Announcement Feed
`GET /api/announcements?page=1&pageSize=20`

**Response** `200 OK`:
```json
{
  "items": [
    {
      "id": "guid",
      "title": "string",
      "content": "string",
      "target": "All",
      "courseId": null,
      "createdBy": "guid",
      "createdByName": "string",
      "isActive": true,
      "publishedAt": "2026-05-25T10:00:00Z"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 3
}
```

**Note**: Feed is filtered by user enrollments and platform-wide visibility automatically.

---

### Update Announcement
`PUT /api/announcements/{id}`

**Response** `200 OK`

---

### Deactivate Announcement
`PATCH /api/announcements/{id}/deactivate`

**Response** `200 OK`

---

### Delete Announcement
`DELETE /api/announcements/{id}`

**Response** `204 No Content`

---

## 3. System Settings 🔒 (Admin Only)

### Get All Settings
`GET /api/system-settings?group=General`

**Query Parameters:**
- `group` (optional) — filter by group name

**Response** `200 OK`:
```json
{
  "items": [
    {
      "id": "guid",
      "group": "General",
      "key": "site_name",
      "value": "Athary",
      "dataType": "String",
      "description": "Platform display name",
      "updatedAt": "2026-05-25T10:00:00Z",
      "updatedBy": "guid"
    }
  ]
}
```

---

### Get Setting by Key
`GET /api/system-settings/{key}`

**Response** `200 OK` (single setting object)

---

### Update Setting
`PUT /api/system-settings/{key}`

**Request Body:**
```json
{
  "value": "NewValue"
}
```

**Response** `200 OK`

**Note**: Change is logged to ActivityLog automatically (who, what, old value, new value, timestamp).

---

### Create Setting
`POST /api/system-settings`

**Request Body:**
```json
{
  "group": "General",
  "key": "new_setting",
  "value": "value",
  "dataType": "String",
  "description": "Description"
}
```

**Response** `201 Created`

**Errors:**
- `400 Bad Request` — value fails DataType validation (e.g., "abc" for Integer)
- `409 Conflict` — key already exists

---

### Delete Setting
`DELETE /api/system-settings/{key}`

**Response** `204 No Content`

---

## 4. Activity Logs 🔒 (Admin Only)

### Get Activity Logs
`GET /api/activity-logs?page=1&pageSize=50&userId=guid&action=Login&entityType=User&dateFrom=2026-01-01&dateTo=2026-05-25&ipAddress=192.168.1.1`

**Query Parameters (all optional):**
- `userId` — filter by user
- `action` — filter by action name
- `entityType` — filter by entity type
- `dateFrom` — start date (inclusive)
- `dateTo` — end date (inclusive)
- `ipAddress` — filter by IP
- `page` — page number (default 1)
- `pageSize` — entries per page (default 50, max 100)

**Response** `200 OK`:
```json
{
  "items": [
    {
      "id": "guid",
      "userId": "guid",
      "userName": "string",
      "action": "Login",
      "entityType": "User",
      "entityId": "guid",
      "details": "{\"key\": \"value\"}",
      "ipAddress": "192.168.1.1",
      "createdAt": "2026-05-25T09:00:00Z"
    }
  ],
  "page": 1,
  "pageSize": 50,
  "totalCount": 1000
}
```

---

## 5. Content Reporting

### Create Report
`POST /api/reports`

**Request Body:**
```json
{
  "entityType": "Course | Review | Message",
  "entityId": "guid",
  "reason": "Spam | Inappropriate | Harassment | Copyright | Other",
  "description": "string (optional)"
}
```

**Response** `201 Created`:
```json
{
  "id": "guid",
  "reporterId": "guid",
  "entityType": "Review",
  "entityId": "guid",
  "reason": "Spam",
  "status": "Pending",
  "createdAt": "2026-05-25T10:00:00Z"
}
```

**Note**: If same user reports same content again, the existing report is updated and `200 OK` returned instead.

---

### Get Pending Reports 🔒 (Admin Only)
`GET /api/reports/pending?page=1&pageSize=20`

**Response** `200 OK`:
```json
{
  "items": [
    {
      "id": "guid",
      "reporterId": "guid",
      "reporterName": "string",
      "entityType": "Review",
      "entityId": "guid",
      "reason": "Spam",
      "description": "This review ...",
      "status": "Pending",
      "createdAt": "2026-05-25T10:00:00Z"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 5
}
```

---

### Resolve Report 🔒 (Admin Only)
`PATCH /api/reports/{id}/resolve`

**Request Body:**
```json
{
  "status": "Dismissed | ActionTaken",
  "adminNote": "string (optional)"
}
```

**Response** `200 OK`

**Note**: Reporter receives a notification (via existing NotificationService) with the outcome.
