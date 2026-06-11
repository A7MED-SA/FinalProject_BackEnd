# System Settings — API Reference

**Base URL**: `/api/system-settings`
**Auth**: JWT Bearer token + **Admin role only** (كل endpoints الإعدادات للمشرفين بس)

---

## Get All Settings

جلب كل الإعدادات (أو فلترة حسب المجموعة)

**GET** `/api/system-settings?group=General`

### Query Parameters
| Parameter | Type | إجباري | Description |
|-----------|------|--------|-------------|
| `group` | string | لا | فلترة حسب اسم المجموعة (General, Payments, Email, ...) |

### Response (200 OK)

```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "group": "General",
      "key": "site_name",
      "value": "Athary",
      "dataType": "String",
      "description": "اسم المنصة",
      "updatedAt": "2026-05-25T10:00:00Z",
      "updatedBy": "guid"
    }
  ]
}
```

---

## Get Setting by Key

جلب إعداد معين

**GET** `/api/system-settings/{key}`

مثال: `/api/system-settings/site_name`

### Response (200 OK)

```json
{
  "success": true,
  "data": {
    "id": "guid",
    "group": "General",
    "key": "site_name",
    "value": "Athary",
    "dataType": "String",
    "description": "اسم المنصة",
    "updatedAt": "2026-05-25T10:00:00Z",
    "updatedBy": "guid"
  }
}
```

### Errors
- `404` — الإعداد مش موجود

---

## Update Setting

تحديث قيمة إعداد

**PUT** `/api/system-settings/{key}`

مثال: `/api/system-settings/site_name`

### Request Body

```json
{
  "value": "Athary LMS"
}
```

### تحقق الـ Data Type
لما تعدل القيمة، لازم تكون مطابقة لنوع البيانات الأصلي:
- `String` — أي نص
- `Integer` — رقم صحيح فقط (مثلاً `"42"`)
- `Boolean` — `"true"` أو `"false"` فقط
- `Json` — JSON صحيح فقط

### Response (200 OK)

```json
{
  "success": true,
  "data": {
    "id": "guid",
    "group": "General",
    "key": "site_name",
    "value": "Athary LMS",
    "dataType": "String",
    "description": "اسم المنصة",
    "updatedAt": "2026-05-25T10:00:00Z",
    "updatedBy": "guid"
  }
}
```

### Errors
- `400` — القيمة مش مطابقة للـ data type
- `404` — الإعداد مش موجود

**ملاحظة**: كل تغيير يتم تسجيله في Activity Logs تلقائياً.

---

## Create Setting

إضافة إعداد جديد

**POST** `/api/system-settings`

### Request Body

```json
{
  "group": "General",
  "key": "new_setting",
  "value": "some_value",
  "dataType": "String",
  "description": "شرح للإعداد"
}
```

### Response (201 Created)

نفس شكل الـ Response العادي.

### Errors
- `400` — الـ key موجود مسبقاً أو الـ dataType غير صحيح

---

## Delete Setting

حذف إعداد

**DELETE** `/api/system-settings/{key}`

### Response (204 No Content)

### Errors
- `404` — الإعداد مش موجود

---

## أنواع البيانات (Data Types)

| Type | مثال value |
|------|------------|
| `String` | `"Athary"` |
| `Integer` | `"42"` (يرسل كـ string في JSON) |
| `Boolean` | `"true"` أو `"false"` |
| `Json` | `'{"key": "value"}'` (JSON string) |

**ملاحظة مهمة**: كل القيم تُرسَل كـ **string** حتى لو كانت integer أو boolean. الـ Backend هو اللي بيعمل parse حسب الـ dataType.

## Caching

الإعدادات مخزنة في Cache لمدة 60 ثانية. أي تغيير من Admin بيمسح الـ Cache فوراً عشان التحديث يظهر على طول.
