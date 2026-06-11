# Teacher Requests — API Reference

**Controller**: `TeacherRequestController`
**Base URL**: `/api/TeacherRequest` (تنبيه: route هو `[controller]`)
**Auth**: JWT (user) / Admin (بعض endpoints)

---

## Can Submit

**GET** `/api/TeacherRequest/can-submit`
**Auth**: JWT

```json
{ "canSubmit": true }
```

---

## Submit Request

**POST** `/api/TeacherRequest`
**Auth**: JWT

```json
{
  "bio": "مطور ويب بخبرة 5 سنوات",
  "expertise": "ASP.NET Core, React",
  "qualifications": "بكالوريوس حاسبات ومعلومات",
  "teachingExperience": "مدرب في منصة يوديمي"
}
```

---

## Update Request

**PUT** `/api/TeacherRequest/{requestId}`
**Auth**: JWT (صاحب الطلب فقط، والطلب لسه Pending)

---

## Add Document

**POST** `/api/TeacherRequest/{requestId}/documents`
**Auth**: JWT (صاحب الطلب)

```json
{ "fileId": "guid", "documentType": "Certificate" }
```

**Document Types**: `Certificate`, `CV`, `Portfolio`, `Other`

---

## My Requests

**GET** `/api/TeacherRequest/my-requests`
**Auth**: JWT

```json
[
  {
    "id": "guid",
    "status": "Pending",
    "bio": "...",
    "expertise": "...",
    "submittedAt": "...",
    "documents": [{ "id": "guid", "fileId": "guid", "documentType": "Certificate" }]
  }
]
```

Status: `Pending`, `Approved`, `Rejected`, `Cancelled`

---

## My Request By Id

**GET** `/api/TeacherRequest/my-requests/{requestId}`
**Auth**: JWT

---

## Cancel Request

**DELETE** `/api/TeacherRequest/{requestId}/cancel`
**Auth**: JWT (صاحب الطلب فقط، والطلب لسه Pending)

---

## Admin — Pending Requests

**GET** `/api/TeacherRequest/pending`
**Auth**: Admin

---

## Admin — Get By Id

**GET** `/api/TeacherRequest/{requestId}`
**Auth**: Admin

---

## Admin — Process Request

**PUT** `/api/TeacherRequest/{requestId}/process`
**Auth**: Admin

```json
{
  "status": "Approved",
  "adminNotes": "المؤهلات مناسبة، تمت الموافقة"
}
```

---

## Admin — Delete Request

**DELETE** `/api/TeacherRequest/{requestId}`
**Auth**: Admin
