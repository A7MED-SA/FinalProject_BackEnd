# 🚀 Quick Authentication Guide

## ✅ تم الإعداد

Identity + JWT تم إضافتهم بنجاح!

---

## 🔧 الخطوات التالية (3 خطوات)

### 1️⃣ إنشاء Migration

```bash
cd d:\Final_Project\BackEnd\backend_project
rmdir /S /Q Migrations
dotnet ef migrations add InitialWithIdentityAndJwt
```

### 2️⃣ إنشاء Database

```bash
dotnet ef database update
```

### 3️⃣ تشغيل التطبيق

```bash
dotnet run
```

---

## 🔑 Default Admin Account

**Email:** `admin@lms.com`  
**Password:** `Admin@123456`

---

## 📋 API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/register` | تسجيل مستخدم جديد |
| POST | `/api/auth/login` | تسجيل دخول |
| POST | `/api/auth/refresh` | تجديد Token |
| POST | `/api/auth/logout` | تسجيل خروج |
| GET | `/api/auth/me` | معلومات المستخدم (Protected) |

---

## 💡 مثال سريع - Register

```bash
POST http://localhost:5000/api/auth/register
Content-Type: application/json

{
  "name": "Ahmed Mohamed",
  "email": "ahmed@example.com",
  "password": "Test@123456",
  "confirmPassword": "Test@123456"
}
```

**Response:**
```json
{
  "accessToken": "eyJhbGci...",
  "refreshToken": "xyz123...",
  "user": {
    "id": "...",
    "name": "Ahmed Mohamed",
    "email": "ahmed@example.com",
    "roles": ["Student"]
  }
}
```

---

## 🔒 استخدام في Controller

```csharp
[Authorize] // يتطلب تسجيل دخول
[HttpGet]
public async Task<IActionResult> GetMyCourses()
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    // ...
}

[Authorize(Roles = "Admin")] // فقط Admin
[HttpDelete]
public async Task<IActionResult> Delete() { }
```

---

## 📊 Default Roles

- **Admin** - كل الصلاحيات
- **Teacher** - إنشاء كورسات
- **Student** - الدور الافتراضي

---

## 🎯 Password Requirements

- 8+ أحرف
- حرف كبير (A-Z)
- حرف صغير (a-z)
- رقم (0-9)
- رمز خاص (@#$%)

---

## ⚠️ مهم للـ Production

في `appsettings.json`:

```json
{
  "JwtSettings": {
    "SecretKey": "ChangeThisToAVeryLongSecretKey64+Characters!"
  }
}
```

📝 **ضع Secret Key في Environment Variables!**

---

## 📚 الوثائق الكاملة

[IDENTITY_JWT_SETUP.md](IDENTITY_JWT_SETUP.md) - دليل شامل

---

**🎉 كل شيء جاهز! ابدأ بإنشاء Migration الآن.**
