# Frontend API Reference — Full Platform

## Base URL
```
https://your-domain.com/api
```

---

## Common Response Format

### Success
```json
{
  "success": true,
  "data": { ... },
  "message": null
}
```

### Error
```json
{
  "success": false,
  "data": null,
  "message": "وصف الخطأ",
  "errors": ["تفاصيل إضافية"]
}
```

---

## Authentication
JWT Bearer token في Header كل الـ requests:
```
Authorization: Bearer <token>
```
لجلب token → `POST /api/auth/login`

---

## Pagination
| Parameter | Default | Max |
|-----------|---------|-----|
| `page` | 1 | - |
| `pageSize` | 20–50 | 100 |

```json
{
  "items": [...],
  "page": 1,
  "pageSize": 20,
  "totalCount": 42
}
```

---

## Roles
| Role | Usage |
|------|-------|
| `Student` | المستخدم العادي المسجل |
| `Instructor` | المدرس (بعد الموافقة على طلبه) |
| `Admin` | مدير النظام |

بعض الـ endpoints تسمح بـ `Student + Instructor` (مجرد `[Authorize]`)، وبعضها محدد بدور معين.

---

## Feature Index

| # | Feature | File | Auth |
|---|---------|------|------|
| 1 | **Authentication** | [01-Auth.md](./01-Auth.md) | Mixed |
| 2 | **Profile & Account** | [02-Profile.md](./02-Profile.md) | JWT |
| 3 | **Courses (Public)** | [03-PublicCourses.md](./03-PublicCourses.md) | Mixed |
| 4 | **Course Management (Instructor)** | [04-CourseManagement.md](./04-CourseManagement.md) | Instructor |
| 5 | **Learning (Student)** | [05-Learning.md](./05-Learning.md) | Student/JWT |
| 6 | **Reviews** | [06-Reviews.md](./06-Reviews.md) | Mixed |
| 7 | **Cart & Wishlist** | [07-Commerce.md](./07-Commerce.md) | JWT |
| 8 | **Orders & Payments** | [07-Commerce.md](./07-Commerce.md) | JWT |
| 9 | **Coupons** | [07-Commerce.md](./07-Commerce.md) | Mixed |
| 10 | **Certificates** | [08-Certificates.md](./08-Certificates.md) | Mixed |
| 11 | **Media** | [09-Media.md](./09-Media.md) | Mixed |
| 12 | **Notifications** | [10-Notifications.md](./10-Notifications.md) | JWT |
| 13 | **Communication** | [11-Communication.md](./11-Communication.md) | Mixed |
| 14 | **Teacher Requests** | [12-TeacherRequests.md](./12-TeacherRequests.md) | Mixed |
| 15 | **Dashboard** | [13-Dashboard.md](./13-Dashboard.md) | JWT |
| 16 | **Admin** | [14-Admin.md](./14-Admin.md) | Admin |

---

## SignalR Hubs
| Hub | URL | Purpose |
|-----|-----|---------|
| Notifications | `/hubs/notifications` | إشعارات لحظية |
| Messages | `/hubs/messages` | محادثات لحظية (إرسال/استقبال/قراءة) |

**Auth**: JWT token in query string `?access_token=<token>`

## Rate Limits
| Endpoint | Limit |
|----------|-------|
| Messaging (all) | 30 req/min per user |
| Certificate Verification | 20 req/min |
