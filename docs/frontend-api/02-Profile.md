# Profile & Account — API Reference

**Controller**: `ProfileController`
**Base URL**: `/api/profile`
**Auth**: JWT (معظم endpoints)

---

## Get My Profile

**GET** `/api/profile/me`

**Response**:
```json
{
  "id": "guid",
  "email": "user@example.com",
  "userName": "ahmed123",
  "fullName": "أحمد محمد",
  "bio": "...",
  "headline": "...",
  "profileImageUrl": "https://...",
  "phones": [{ "id": "guid", "number": "...", "isDefault": true }],
  "addresses": [{ "id": "guid", "street": "...", "city": "...", "isDefault": true }],
  "createdAt": "..."
}
```

---

## Get Public Profile

**GET** `/api/profile/{userId}`
**Auth**: None (AllowAnonymous)

عرض الملف الشخصي العام لمستخدم معين.

---

## Update Profile

**PUT** `/api/profile`

```json
{
  "fullName": "أحمد محمد",
  "bio": "مطور ويب",
  "headline": "Full Stack Developer"
}
```

---

## Profile Picture

### Set Picture
**POST** `/api/profile/picture`
```json
{ "fileId": "guid" }
```
يستخدم `fileId` من Media Service (رفع الصورة أولاً).

### Delete Picture
**DELETE** `/api/profile/picture`

---

## Phone Management

### Add Phone
**POST** `/api/profile/phones`
```json
{ "number": "+201234567890", "countryCode": "+20" }
```

### Delete Phone
**DELETE** `/api/profile/phones/{phoneId}`

### Set Default Phone
**PUT** `/api/profile/phones/{phoneId}/default`

---

## Address Management

### Add Address
**POST** `/api/profile/addresses`
```json
{
  "street": "شارع النصر",
  "city": "القاهرة",
  "state": "القاهرة",
  "country": "مصر",
  "zipCode": "12345"
}
```

### Update Address
**PUT** `/api/profile/addresses/{addressId}`

### Delete Address
**DELETE** `/api/profile/addresses/{addressId}`

### Set Default Address
**PUT** `/api/profile/addresses/{addressId}/default`
