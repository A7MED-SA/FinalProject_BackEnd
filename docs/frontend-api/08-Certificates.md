# Certificates — API Reference

**Controller**: `CertificatesController`
**Base URL**: `/api/certificates`
**Rate Limit**: 20 req/min (verify endpoint)

---

## My Certificates

**GET** `/api/certificates/my`
**Auth**: JWT

```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "courseId": "guid",
      "courseTitle": "دورة تعلم البرمجة",
      "studentName": "أحمد محمد",
      "verificationCode": "CERT-ABC123",
      "issuedAt": "2026-05-25T10:00:00Z",
      "isRevoked": false
    }
  ]
}
```

---

## Get Certificate

**GET** `/api/certificates/{id}`
**Auth**: JWT

---

## Download Certificate (PDF)

**GET** `/api/certificates/{id}/download`
**Auth**: JWT

**Response**: PDF file (application/pdf)

يُرجع ملف PDF للشهادة مباشرة باستخدام QuestPDF.

---

## Verify Certificate

**GET** `/api/certificates/verify/{code}`
**Auth**: None
**Rate Limited**: 20 req/min

```json
{
  "success": true,
  "data": {
    "isValid": true,
    "studentName": "أحمد محمد",
    "courseTitle": "دورة تعلم البرمجة",
    "issuedAt": "...",
    "instructorName": "...",
    "duration": "40 ساعة"
  }
}
```
لو الكود مش صحيح: `{ "isValid": false, "message": "الشهادة غير صالحة" }`
