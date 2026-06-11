# Notifications — API Reference

**Controller**: `NotificationController`
**Base URL**: `/api/notification`
**Auth**: JWT

## SignalR Hub

**Hub URL**: `/notifications-hub`
**Auth**: JWT token (query string)

```javascript
// Frontend connection example (JavaScript)
const connection = new signalR.HubConnectionBuilder()
  .withUrl("https://domain.com/notifications-hub?access_token=" + token)
  .build();

connection.on("ReceiveNotification", (notification) => {
  console.log("New notification:", notification);
});

connection.start();
```

---

## Get Notifications

**GET** `/api/notification?page=1&pageSize=20&isRead=false`

```json
{
  "items": [
    {
      "id": "guid",
      "type": "CourseApproved",
      "title": "تمت الموافقة على كورسك",
      "message": "تمت الموافقة على كورس 'تعلم البرمجة'",
      "referenceId": "guid",
      "referenceType": "Course",
      "isRead": false,
      "createdAt": "2026-05-25T10:00:00Z"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 3
}
```

Notification Types: `CourseApproved`, `CourseRejected`, `NewEnrollment`, `CertificateIssued`, `RefundProcessed`, `TeacherRequestApproved`, `TeacherRequestRejected`, `General`

---

## Get Unread Count

**GET** `/api/notification/unread-count`

```json
{ "count": 3 }
```

---

## Mark As Read

**PATCH** `/api/notification/{notificationId}/read`

---

## Mark All As Read

**POST** `/api/notification/mark-all-read`

---

## Delete Notification

**DELETE** `/api/notification/{notificationId}`

---

## Clear All

**DELETE** `/api/notification/clear-all`
