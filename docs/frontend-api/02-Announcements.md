# Announcements — API Reference

**Base URL**: `/api/announcements`
**Auth**: JWT Bearer token (required for all endpoints)

---

## Create Announcement (Admin)

إنشاء إعلان عام على المنصة

**POST** `/api/announcements`

### Roles المسموح لها
- **Admin** فقط (يقدر يعمل إعلانات عامة أو لكورس معين)

### Request Body

```json
{
  "title": "عنوان الإعلان",
  "content": "محتوى الإعلان",
  "target": "All",
  "courseId": null
}
```

### Values للـ `target`
| Value | المعنى |
|-------|--------|
| `All` | الكل (طلاب + معلمين) |
| `Students` | الطلاب فقط |
| `Teachers` | المعلمين فقط |
| `Admins` | المشرفين فقط |
| `SpecificCourse` | طلاب كورس معين (يحتاج `courseId`) |

### Response (201 Created)

```json
{
  "success": true,
  "data": {
    "id": "guid",
    "title": "عنوان الإعلان",
    "content": "محتوى الإعلان",
    "target": "All",
    "courseId": null,
    "createdBy": "guid",
    "createdByName": "أحمد المشرف",
    "isActive": true,
    "publishedAt": "2026-05-25T10:00:00Z"
  }
}
```

### Errors
- `400` — العنوان ناقص أو الـ target غلط أو الـ SpecificCourse بدون courseId

---

## Create Course Announcement (Instructor)

إنشاء إعلان لكورس معين (للمعلمين)

**POST** `/api/announcements/course/{courseId}`

### Roles المسموح لها
- **Instructor** صاحب الكورس فقط

### Request Body (نفس السابق)

```json
{
  "title": "عنوان الإعلان",
  "content": "محتوى الإعلان"
}
```

**ملاحظة**: الـ `target` يتحدد أوتوماتيكياً `SpecificCourse` والـ `courseId` من الـ URL.

### Response (201 Created)

نفس Response الإعلان العادي.

### Errors
- `400` — ماتملكش هذا الكورس

---

## Get Announcement Feed

جلب الإعلانات النشطة للمستخدم الحالي

**GET** `/api/announcements?page=1&pageSize=20`

### كيف تشتغل الفلترة؟
- **طالب**: يشوف الإعلانات العامة (All, Students) + إعلانات الكورسات اللي مسجل فيها
- **مدرس**: يشوف الإعلانات العامة (All, Teachers) + إعلانات الكورسات اللي بيدرسها
- **مشرف**: يشوف كل الإعلانات النشطة
- الإعلانات غير النشطة (`isActive = false`) ماتظهرش

### Response (200 OK)

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "title": "عنوان الإعلان",
        "content": "محتوى الإعلان",
        "target": "All",
        "courseId": null,
        "createdBy": "guid",
        "createdByName": "أحمد المشرف",
        "isActive": true,
        "publishedAt": "2026-05-25T10:00:00Z"
      }
    ],
    "page": 1,
    "pageSize": 20,
    "totalCount": 3
  }
}
```

---

## Update Announcement

تعديل إعلان موجود

**PUT** `/api/announcements/{id}`

### Request Body

```json
{
  "title": "عنوان جديد",
  "content": "محتوى جديد",
  "target": "Students",
  "courseId": null,
  "isActive": true
}
```

### Response (200 OK)

نفس شكل Response الإعلان بعد التعديل.

### Errors
- `400` — ماتقدر تعدل إعلان مش بتاعك
- `404` — الإعلان مش موجود

---

## Deactivate Announcement

إلغاء تفعيل الإعلان (يختفي من الـ feed)

**PATCH** `/api/announcements/{id}/deactivate`

### Response (200 OK)

```json
{
  "success": true,
  "data": {}
}
```

### Errors
- `404` — الإعلان مش موجود

---

## Delete Announcement

حذف الإعلان نهائياً

**DELETE** `/api/announcements/{id}`

### Response (204 No Content)

### Errors
- `404` — الإعلان مش موجود
