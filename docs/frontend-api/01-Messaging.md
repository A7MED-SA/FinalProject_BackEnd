# Internal Messaging — API Reference

**Base URL**: `/api/messages`
**Auth**: JWT Bearer token (required for all endpoints)
**Rate Limit**: 30 requests per minute per user

---

## Send Message

إرسال رسالة لمستخدم آخر

**POST** `/api/messages`

### Rules للمراسلة
- **طالب** → **مدرس الكورس** المسجل فيه فقط
- **مدرس** → **طالب مسجل** في كورسه فقط
- **مشرف (Admin)** → **أي مستخدم**
- **الردود**: أي مشارك في محادثة قائمة يقدر يرد (حتى لو ما عنده علاقة)

### Request Body

```json
{
  "receiverId": "guid",
  "content": "نص الرسالة (نص فقط، max 5000 حرف)"
}
```

### Response (201 Created)

```json
{
  "success": true,
  "data": {
    "id": "guid",
    "senderId": "guid",
    "receiverId": "guid",
    "content": "نص الرسالة",
    "sentAt": "2026-05-25T10:00:00Z",
    "isRead": false,
    "readAt": null,
    "isDeletedForSender": false
  }
}
```

### Errors
- `400` — محتوى فاضي أو أطول من 5000 حرف أو فيه HTML أو مراسلة نفسك أو ماعندك علاقة مع المستخدم
- `403` — (يعطى 400 حالياً) ماعندك صلاحية مراسلة هذا الشخص
- `429` — تجاوزت حد 30 رسالة في الدقيقة

---

## Get Conversations

جلب قائمة المحادثات (آخر رسالة من كل محادثة)

**GET** `/api/messages/conversations?page=1&pageSize=20`

### Response (200 OK)

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "otherUserId": "guid",
        "otherUserName": "أحمد محمد",
        "lastMessage": "نص آخر رسالة",
        "lastMessageAt": "2026-05-25T10:00:00Z",
        "unreadCount": 2
      }
    ],
    "page": 1,
    "pageSize": 20,
    "totalCount": 5
  }
}
```

**ملاحظة**: المحادثات مرتّبة من الأحدث إلى الأقدم. كل محادثة = كل الرسائل بين مستخدمين اثنين (الاتجاهين).

---

## Get Conversation Messages

جلب رسائل محادثة معينة (بينك وبين مستخدم آخر)

**GET** `/api/messages/conversations/{otherUserId}?page=1&pageSize=50`

### Response (200 OK)

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "senderId": "guid",
        "receiverId": "guid",
        "content": "نص الرسالة",
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
}
```

**ملاحظة**: الرسائل مرتّبة تصاعدياً (قديم → جديد).

---

## Get Unread Count

عدد الرسائل غير المقروءة

**GET** `/api/messages/unread-count`

### Response (200 OK)

```json
{
  "success": true,
  "data": {
    "unreadCount": 3
  }
}
```

---

## Mark Message as Read

تحديد رسالة كمقروءة

**PATCH** `/api/messages/{messageId}/read`

### Response (200 OK)

```json
{
  "success": true,
  "data": {}
}
```

### Errors
- `400` — مش هاتكون قارئ غير رسايلك
- `404` — الرسالة مش موجودة

---

## Delete Message (Soft Delete)

حذف رسالة (تختفي من طرفك بس الطرف الثاني لسا يشوفها)

**DELETE** `/api/messages/{messageId}`

### Response (204 No Content)

مافيش body في الرد.

### Errors
- `400` — ماتقدر تحذف غير رسايلك انت
- `404` — الرسالة مش موجودة
