# Reviews — API Reference

**Controller**: `ReviewsController`
**Base URL**: `/api/reviews`

---

## Create Review

**POST** `/api/reviews`
**Auth**: JWT

```json
{
  "courseId": "guid",
  "rating": 5,
  "content": "دورة ممتازة جداً، استفدت كثيراً"
}
```

---

## Get Course Reviews

**GET** `/api/reviews/course/{courseId}?page=1&pageSize=10`
**Auth**: None

```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "userId": "guid",
      "userName": "أحمد محمد",
      "rating": 5,
      "content": "دورة ممتازة",
      "createdAt": "...",
      "updatedAt": null,
      "helpfulCount": 12,
      "isFlagged": false
    }
  ]
}
```

---

## Get Review By Id

**GET** `/api/reviews/{id}`
**Auth**: None

---

## Update Review

**PUT** `/api/reviews/{id}`
**Auth**: JWT (صاحب المراجعة فقط)

---

## Delete Review

**DELETE** `/api/reviews/{id}`
**Auth**: JWT (صاحب المراجعة)

---

## Toggle Helpful

**POST** `/api/reviews/{id}/helpful`
**Auth**: JWT

```json
{ "isHelpful": true }
```

---

## Flag Review

**POST** `/api/reviews/{id}/flag`
**Auth**: Instructor

تبليغ عن مراجعة (لمراجعتها من الأدمن).

---

## Admin — Pending Reviews

**GET** `/api/reviews/pending`
**Auth**: Admin

جميع المراجعات المبلغ عنها.

---

## Admin — Moderate Review

**PUT** `/api/reviews/{id}/moderate`
**Auth**: Admin

```json
{ "action": "approve", "moderationNote": "..." }
```
Actions: `approve`, `reject`, `hide`
