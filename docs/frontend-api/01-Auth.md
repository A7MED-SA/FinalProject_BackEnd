# Authentication — API Reference

**Controllers**: `AuthController`, `OAuthController`

---

## Register

**POST** `/api/auth/register`

```json
{
  "email": "user@example.com",
  "password": "P@ssw0rd123",
  "confirmPassword": "P@ssw0rd123",
  "fullName": "أحمد محمد",
  "userName": "ahmed123"
}
```

**Response:**
```json
{
  "message": "Registration successful. Please check your email for verification code.",
  "user": { "id": "guid", "email": "...", "userName": "..." }
}
```

---

## Login

**POST** `/api/auth/login`

```json
{
  "email": "user@example.com",
  "password": "P@ssw0rd123"
}
```

**Response:**
```json
{
  "accessToken": "eyJ...",
  "refreshToken": "guid",
  "expiresAt": "2026-05-26T11:00:00Z",
  "user": { "id": "guid", "email": "...", "userName": "...", "roles": ["Student"] }
}
```

**ملاحظة**: الـ `refreshToken` يستخدم لتجديد الـ access token.

---

## Refresh Token

**POST** `/api/auth/refresh`

```json
{ "refreshToken": "guid" }
```

**Response**: نفس شكل login (accessToken + refreshToken جديدين).

---

## Verify Email

**POST** `/api/auth/verify-email`

```json
{ "email": "user@example.com", "code": "123456" }
```

**Response**: `{ "message": "Email verified successfully." }`

**ملاحظة**: كود التفعيل يُرسل للإيميل بعد التسجيل.

---

## Resend Verification

**POST** `/api/auth/resend-verification`

```json
{ "email": "user@example.com" }
```

**Response**: `{ "message": "Verification code sent to your email." }`

---

## Forgot Password

**POST** `/api/auth/forgot-password`

```json
{ "email": "user@example.com" }
```

**Response**: `{ "message": "If the email exists, a password reset code has been sent." }`

---

## Reset Password

**POST** `/api/auth/reset-password`

```json
{
  "email": "user@example.com",
  "code": "123456",
  "newPassword": "NewP@ss123",
  "confirmPassword": "NewP@ss123"
}
```

**Response**: `{ "message": "Password reset successfully." }`

---

## Logout

**POST** `/api/auth/logout`
**Auth**: JWT

يسجل خروج من الجلسة الحالية.

**Response**: `{ "message": "Logged out successfully" }`

---

## Logout All

**POST** `/api/auth/logout-all`
**Auth**: JWT

يسجل خروج من جميع الجلسات الأخرى (ما عدا الحالية).

**Response**: `{ "message": "All other sessions logged out successfully" }`

---

## Get Sessions

**GET** `/api/auth/sessions`
**Auth**: JWT

جلب جميع الجلسات النشطة للمستخدم.

**Response**:
```json
[
  {
    "id": "guid",
    "ipAddress": "192.168.1.1",
    "userAgent": "Mozilla/5.0...",
    "lastActivity": "2026-05-25T10:00:00Z",
    "createdAt": "2026-05-25T09:00:00Z",
    "isCurrent": true
  }
]
```

---

## OAuth — Google Login

**POST** `/api/oauth/google`

```json
{ "idToken": "google-id-token", "provider": "google" }
```

**Response**: مثل login response.

---

## OAuth — Microsoft Login

**POST** `/api/oauth/microsoft`

```json
{ "idToken": "microsoft-id-token", "provider": "microsoft" }
```

**Response**: مثل login response.
