# Content Reporting — API Reference

**Base URL**: `/api/reports`
**Auth**: JWT Bearer token (مطلوب لجميع endpoints)

---

## Create Report

الإبلاغ عن محتوى غير مناسب

**POST** `/api/reports`

### الأنواع المسموح الإبلاغ عنها
- `Course` — كورس
- `Review` — تقييم
- `Message` — رسالة

### الأسباب
| Value | description |
|-------|-------------|
| `Spam` | سبام |
| `Inappropriate` | غير لائق |
| `Harassment` | تحرش |
| `Copyright` | حقوق نشر |
| `Other` | سبب آخر |

### Request Body

```json
{
  "entityType": "Review",
  "entityId": "guid",
  "reason": "Spam",
  "description": "هذا التقييم يحتوي على إعلانات"
}
```

### Response (200 OK)

```json
{
  "success": true,
  "data": {
    "id": "guid",
    "reporterId": "guid",
    "reporterName": "أحمد المستخدم",
    "entityType": "Review",
    "entityId": "guid",
    "reason": "Spam",
    "description": "هذا التقييم يحتوي على إعلانات",
    "status": "Pending",
    "adminNote": null,
    "createdAt": "2026-05-25T10:00:00Z",
    "resolvedAt": null
  }
}
```

### لو كررت الإبلاغ عن نفس المحتوى
لو المستخدم نفسه بلغ عن نفس المحتوى تاني، الـ Backend بيحدث الـ report القديم بدل ما يعمل واحد جديد. هاتجيك نفس الـ Response العادي.

### Errors
- `400` — الـ entityType أو reason مش موجودين في القائمة

---

## Get Pending Reports (Admin Only)

جلب البلاغات المعلقة (للمشرف)

**GET** `/api/reports/pending?page=1&pageSize=20`

### Response (200 OK)

```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "reporterId": "guid",
      "reporterName": "أحمد المستخدم",
      "entityType": "Review",
      "entityId": "guid",
      "reason": "Spam",
      "description": "هذا التقييم يحتوي على إعلانات",
      "status": "Pending",
      "adminNote": null,
      "createdAt": "2026-05-25T10:00:00Z",
      "resolvedAt": null
    }
  ]
}
```

**ملاحظة**: بس الـ Pending تظهر. المرتبة من الأحدث للأقدم.

---

## Resolve Report (Admin Only)

حل البلاغ (إما رفض البلاغ أو اتخاذ إجراء)

**PATCH** `/api/reports/{id}/resolve`

### Request Body

```json
{
  "status": "Dismissed",
  "adminNote": "لا يوجد مخالفة، تم رفض البلاغ"
}
```

### Values للـ `status`
| Value | المعنى |
|-------|--------|
| `Dismissed` | البلاغ مرفوض — لا يوجد مخالفة |
| `ActionTaken` | تم اتخاذ إجراء ضد المحتوى (حذف أو تعديل) |

### Response (200 OK)

```json
{
  "success": true,
  "data": {
    "id": "guid",
    "reporterId": "guid",
    "reporterName": "أحمد المستخدم",
    "entityType": "Review",
    "entityId": "guid",
    "reason": "Spam",
    "description": "هذا التقييم يحتوي على إعلانات",
    "status": "Dismissed",
    "adminNote": "لا يوجد مخالفة، تم رفض البلاغ",
    "createdAt": "2026-05-25T10:00:00Z",
    "resolvedAt": "2026-05-25T11:00:00Z"
  }
}
```

### بعد حل البلاغ
1. يتم تسجيل إجراء في Activity Logs
2. المستخدم المبلّغ يستلم **إشعار** بنتيجة البلاغ

### Errors
- `400` — الـ status مش Dismissed أو ActionTaken
- `404` — البلاغ مش موجود

---

## الـ Flow الكامل للإبلاغ

```
مستخدم → POST /api/reports (ينشئ بلاغ)
    ↓
Admin → GET /api/reports/pending (يشوف البلاغات)
    ↓
Admin → PATCH /api/reports/{id}/resolve (يحل البلاغ)
    ↓
مستخدم يستلم إشعار بنتيجة البلاغ
    ↓
(اختياري) Admin يشوف Activity Logs عشان يسجل الإجراء
```

## واجهة الـ Admin

الصفحة المفترض تحتوي على:
1. **قائمة البلاغات المعلقة** — مع اسم المبلّغ ونوع المحتوى والسبب
2. **زر رفض (Dismiss)** + **زر اتخاذ إجراء (ActionTaken)**
3. **حقل notes** للمشرف عشان يكتب ملاحظة
4. بعد الحل — يختفي البلاغ من القائمة المعلقة
