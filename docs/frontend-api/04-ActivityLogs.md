# Activity Logs — API Reference

**Base URL**: `/api/activity-logs`
**Auth**: JWT Bearer token + **Admin role only**

سجل النشاطات — كل الإجراءات اللي صارت على المنصة (تسجيل دخول، شراء، تعديل إعدادات، ...)

---

## Get Activity Logs

جلب سجل النشاطات مع فلترة

**GET** `/api/activity-logs?page=1&pageSize=50&userId=guid&action=Login&entityType=User&dateFrom=2026-01-01&dateTo=2026-05-25&ipAddress=192.168.1.1`

### Query Parameters (كلها اختيارية)

| Parameter | Type | Description |
|-----------|------|-------------|
| `userId` | guid | فلترة حسب المستخدم |
| `action` | string | فلترة حسب نوع الإجراء (Login, SettingUpdated, ReportResolved, ...) |
| `entityType` | string | فلترة حسب نوع الكيان (User, Course, Review, Order, Message, Announcement, SystemSetting, Report) |
| `dateFrom` | date | بداية التاريخ (متضمن) |
| `dateTo` | date | نهاية التاريخ (متضمن) |
| `ipAddress` | string | فلترة حسب الـ IP |
| `page` | int | رقم الصفحة (default 1) |
| `pageSize` | int | عدد العناصر (default 50, max 100) |

### Response (200 OK)

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "userId": "guid",
        "userName": "ahmed@example.com",
        "action": "Login",
        "entityType": "User",
        "entityId": "guid",
        "details": "تفاصيل الإجراء",
        "ipAddress": "192.168.1.1",
        "createdAt": "2026-05-25T09:00:00Z"
      }
    ],
    "page": 1,
    "pageSize": 50,
    "totalCount": 1000
  }
}
```

**ملاحظة**: `userName` بيجيب `UserName` من Identity (الـ email). لو المستخدم محذوف بيظهر "Deleted User".

---

## قائمة الـ Entity Types

| Value | المعنى |
|-------|--------|
| `User` | مستخدم |
| `Course` | كورس |
| `Review` | تقييم |
| `Order` | طلب شراء |
| `Message` | رسالة |
| `Announcement` | إعلان |
| `SystemSetting` | إعداد النظام |
| `Report` | بلاغ |

---

## Flow للـ Frontend

1. اعرض جدول بالـ logs مرتب من الأحدث للأقدم
2. أضف filters: اختيار مستخدم, اختيار action, range تاريخ, IP
3. الـ pagination: `page` + `pageSize` مع `totalCount`
4. MAX page size = 100 (لو بعت أكبر يرجع 100)

---

## كيف تتسجل الـ Logs؟ (للمعلومية)

الـ Backend يسجل تلقائياً:
- ✅ **Login/Logout** — من Authentication system
- ✅ **تحديث الإعدادات** — من System Settings
- ✅ **حل البلاغات** — من Reports
- ✅ **إصدار الشهادات** — من Certificates
- ✅ **إدارة المحتوى** — من Content Management

ما يحتاج الـ Frontend يعمل أي شيء للتسجيل.
