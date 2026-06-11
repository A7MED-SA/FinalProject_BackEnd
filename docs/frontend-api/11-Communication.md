# Communication (Messages, Announcements, Settings, Logs, Reports) — API Reference

**Controllers**: `MessagesController`, `AnnouncementsController`, `SystemSettingsController`, `ActivityLogsController`, `ReportsController`

---

## Internal Messaging

### Overview
- **REST API**: للـ history والـ CRUD (جلب المحادثات، إرسال، قراءة، حذف)
- **SignalR**: للـ real-time events (وصول رسالة جديدة، قراءة، حذف)
- **Base**: `/api/messages`
- **Hub URL**: `/hubs/messages`
- **Rate Limit**: 30 req/min (REST endpoints)

### Rules للمراسلة
- طالب → مدرس الكورس المسجل فيه فقط
- مدرس → طالب مسجل في كورسه فقط
- مشرف → أي مستخدم
- الردود: أي مشارك في محادثة قائمة يقدر يرد

### Message Object Shape

كل الرسائل (REST و SignalR) ترجع نفس الشكل:

```json
{
  "id": "guid",
  "senderId": "guid",
  "receiverId": "guid",
  "content": "نص الرسالة",
  "sentAt": "2026-05-25T10:00:00Z",
  "isRead": false,
  "readAt": null,
  "isDeletedForSender": false
}
```

### Flow: كيف تشتغل

```
1. افتح اتصال SignalR → /hubs/messages
2. أرسل رسالة → POST /api/messages (REST)
3. استقبل تأكيد الإرسال → event "MessageSent" (SignalR)
4. المستقبل يستقبل → event "NewMessage" (SignalR)
5. المستقبل يقرأ → PATCH /api/messages/{id}/read (REST)
6. المرسل يستقبل → event "MessageRead" (SignalR)
```

---

## Real-Time Messaging (SignalR)

**Hub URL**: `/hubs/messages`
**Auth**: JWT token in query string

### Frontend Connection Example

```javascript
const connection = new signalR.HubConnectionBuilder()
  .withUrl("https://domain.com/hubs/messages?access_token=" + token)
  .withAutomaticReconnect()
  .build();

connection.start().catch(err => console.error(err));
```

### Events للاستقبال

| Event | يحصل لما | بيبعث لـ | الـ Data |
|-------|----------|----------|---------|
| `NewMessage` | حد بيعتلك رسالة | المستقبل | `MessageResponse` object |
| `MessageSent` | رسالتك اتبعتت | المرسل | `MessageResponse` object |
| `MessageRead` | حد قرا رسالتك | المرسل | `{ messageId, readAt }` |
| `MessageDeleted` | رسالتك اتحذفت | المستقبل | `{ messageId }` |
| `UnreadCountUpdate` | حاجة اتغيرت في المقروء | المتأثر | `{ unreadCount: number }` |

```javascript
// استقبال رسالة جديدة
connection.on("NewMessage", (message) => {
  // message = { id, senderId, receiverId, content, sentAt, isRead, readAt }
  addMessageToChat(message);
  updateUnreadBadge();
});

// تأكيد إرسال رسالتك
connection.on("MessageSent", (message) => {
  updateMessageStatus(message.id, "sent");
});

// حد قرا رسالتك
connection.on("MessageRead", (data) => {
  markAsReadInUI(data.messageId, data.readAt);
});

// رسالة اتحذفت
connection.on("MessageDeleted", (data) => {
  removeMessageFromUI(data.messageId);
});

// تحديث العداد
connection.on("UnreadCountUpdate", (data) => {
  document.getElementById("unread-badge").textContent = data.unreadCount;
});
```

**ملاحظة**: الرسائل بتتبعت عبر REST (POST), والـ SignalR للتحديثات اللحظية بس.

### إعادة الاتصال (Reconnection)

`withAutomaticReconnect()` بيحاول يعيد الاتصال تلقائياً لو انقطع. استمع للأحداث:

```javascript
connection.onreconnecting(() => console.log("جاري إعادة الاتصال..."));
connection.onreconnected(() => console.log("تم إعادة الاتصال"));
connection.onclose(() => console.log("اتقطع الاتصال"));
```

---

## REST API — Messaging

### Send Message
**POST** `/api/messages`

بعد نجاح الإرسال، المستقبل يستقبل `NewMessage` عبر SignalR.

```json
{ "receiverId": "guid", "content": "نص الرسالة (نص فقط، max 5000 حرف)" }
```

ممنوع HTML. **Errors**: 400 (فاضي/HTML/نفسك/مافيش علاقة), 429 (rate limit)

### Get Conversations
**GET** `/api/messages/conversations?page=1&pageSize=20`

```json
{
  "items": [
    {
      "otherUserId": "guid",
      "otherUserName": "أحمد محمد",
      "lastMessage": "نص آخر رسالة",
      "lastMessageAt": "...",
      "unreadCount": 2
    }
  ],
  "page": 1, "pageSize": 20, "totalCount": 5
}
```

### Get Conversation Messages
**GET** `/api/messages/conversations/{otherUserId}?page=1&pageSize=50`

### Get Unread Count
**GET** `/api/messages/unread-count`

```json
{ "success": true, "data": { "unreadCount": 3 } }
```

### Mark As Read
**PATCH** `/api/messages/{messageId}/read`

بعد النجاح، المرسل يستقبل `MessageRead` عبر SignalR.

### Delete Message (Soft Delete)
**DELETE** `/api/messages/{messageId}`

يختفي من طرفك بس. المستقبل يستقبل `MessageDeleted` عبر SignalR.

---

## Announcements

**Base**: `/api/announcements`
**Auth**: JWT

### Create (Platform-wide)
**POST** `/api/announcements`
```json
{
  "title": "إعلان مهم",
  "content": "نص الإعلان",
  "target": "AllUsers",
  "priority": "High"
}
```
Target: `AllUsers`, `Students`, `Instructors`, `SpecificCourse`
Priority: `Low`, `Medium`, `High`

### Create (Course-specific)
**POST** `/api/announcements/course/{courseId}`

### Get Feed
**GET** `/api/announcements?page=1&pageSize=20`
يشوف الإعلانات المخصصة له حسب دوره.

### Update
**PUT** `/api/announcements/{id}`

### Deactivate
**PATCH** `/api/announcements/{id}/deactivate`

### Delete
**DELETE** `/api/announcements/{id}`

---

## System Settings

**Base**: `/api/system-settings`
**Auth**: Admin

### Get All
**GET** `/api/system-settings?group=General`

### Get By Key
**GET** `/api/system-settings/{key}`

### Create
**POST** `/api/system-settings`
```json
{ "key": "SiteName", "value": "منصتي التعليمية", "group": "General", "description": "اسم الموقع" }
```

### Update
**PUT** `/api/system-settings/{key}`
```json
{ "value": "القيمة الجديدة" }
```

### Delete
**DELETE** `/api/system-settings/{key}`

---

## Activity Logs

**Base**: `/api/activity-logs`
**Auth**: Admin

### Get Logs
**GET** `/api/activity-logs?userId=guid&action=Login&entityType=User&dateFrom=2026-01-01&dateTo=2026-12-31&ipAddress=192.168.1.1&page=1&pageSize=50`

PageSize max: 100

```json
{
  "items": [
    {
      "id": "guid",
      "userId": "guid",
      "userName": "أحمد محمد",
      "action": "Login",
      "entityType": "User",
      "entityId": "guid",
      "details": "مستخدم سجل دخول",
      "ipAddress": "192.168.1.1",
      "createdAt": "2026-05-25T10:00:00Z"
    }
  ],
  "page": 1, "pageSize": 50, "totalCount": 100
}
```

---

## Reporting (Content)

**Base**: `/api/reports`
**Auth**: JWT

### Create Report
**POST** `/api/reports`
```json
{
  "entityType": "Course",
  "entityId": "guid",
  "reason": "محتوى مخالف",
  "description": "..."
}
```
EntityType: `Course`, `Review`, `Comment`, `User`
**منع التكرار**: نفس (مُبلّغ, نوع, معرّف) يحدّث التقرير القديم.

### Get Pending (Admin)
**GET** `/api/reports/pending?page=1&pageSize=20`
**Auth**: Admin

### Resolve (Admin)
**PATCH** `/api/reports/{id}/resolve`
**Auth**: Admin
```json
{ "resolution": "Dismissed", "notes": "..." }
```
Resolution: `Dismissed`, `ActionTaken`
