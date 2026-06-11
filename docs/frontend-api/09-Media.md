# Media — API Reference

**Controller**: `MediaController`
**Base URL**: `/api/media`

---

## Generate Upload URL

**POST** `/api/media/upload-url`
**Auth**: JWT

```json
{
  "fileName": "video-lesson-1.mp4",
  "fileSizeBytes": 104857600,
  "contentType": "video/mp4",
  "purpose": "CourseVideo"
}
```

**Response**:
```json
{
  "fileId": "guid",
  "uploadUrl": "https://minio.example.com/...",
  "expiresAt": "2026-05-25T11:00:00Z"
}
```

**ملاحظة**: الـ `uploadUrl` هو presigned URL للرفع المباشر إلى MinIO. استخدمه لرفع الملف بـ PUT.

---

## Confirm Upload

**POST** `/api/media/confirm-upload`
**Auth**: JWT

```json
{ "fileId": "guid" }
```

**Response**: تأكيد أن الملف ارفع بنجاح.

---

## Get View URL

**GET** `/api/media/{fileId}/view`
**Auth**: Mixed (يعتمد على صلاحيات الملف)

**Response**:
```json
{
  "viewUrl": "https://minio.example.com/...",
  "expiresAt": "2026-05-25T12:00:00Z"
}
```

ملفات الكورسات العامة: أي زائر يشوفها. ملفات خاصة: بس صاحبها والأدمن.

---

## Admin Media Management

**Base**: `/api/admin/media`
**Auth**: Admin

### List Media
**GET** `/api/admin/media?userId=guid&purpose=CourseVideo&isDeleted=false&page=1&pageSize=20`

### Get Details
**GET** `/api/admin/media/{fileId}`

### Soft Delete
**DELETE** `/api/admin/media/{fileId}`

### Restore
**POST** `/api/admin/media/{fileId}/restore`

### Permanent Delete
**DELETE** `/api/admin/media/{fileId}/permanent`

### Storage Stats
**GET** `/api/admin/media/stats`
```json
{
  "totalFiles": 1500,
  "totalSizeBytes": 536870912000,
  "totalSizeFormatted": "500 GB",
  "filesByPurpose": { "CourseVideo": 800, "CourseDocument": 300, "ProfilePicture": 200, "CategoryImage": 50, "Other": 150 }
}
```
