# ✅ Identity & JWT Authentication Setup

تم إضافة ASP.NET Core Identity مع JWT Authentication بنجاح!

---

## 🎉 ما تم إنجازه

### ✅ 1. Packages المضافة
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` (9.0.10)
- `Microsoft.AspNetCore.Authentication.JwtBearer` (9.0.10)
- `System.IdentityModel.Tokens.Jwt` (8.2.1)

### ✅ 2. Identity Models

#### User Model
```csharp
public class User : IdentityUser<Guid>
{
    // Custom properties
    public string Name { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? Bio { get; set; }
    public DateTime? LastLogin { get; set; }
    // ... + Navigation Properties
}
```

**مميزات:**
- يرث من `IdentityUser<Guid>` (بدلاً من `string`)
- Sequential GUID generation باستخدام `MassTransit.NewId`
- يحتوي على: Email, PasswordHash, EmailConfirmed, PhoneNumber, etc.

#### Role Model
```csharp
public class Role : IdentityRole<Guid>
{
    // Custom properties
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

#### UserRole Model
```csharp
public class UserRole : IdentityUserRole<Guid>
{
    // Custom properties
    public DateTime AssignedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
```

### ✅ 3. DbContext
```csharp
public class ApplicationDbContext : IdentityDbContext<User, Role, Guid, 
    IdentityUserClaim<Guid>, UserRole, IdentityUserLogin<Guid>, 
    IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
```

**Identity Tables:**
- `users` - المستخدمين
- `roles` - الأدوار
- `user_roles` - ربط المستخدمين بالأدوار
- `user_claims` - Claims إضافية
- `user_logins` - External logins (Google, Facebook, etc.)
- `user_tokens` - Refresh tokens, email confirmation tokens
- `role_claims` - Claims للأدوار

### ✅ 4. JWT Configuration

#### appsettings.json
```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLong",
    "Issuer": "LMS_Backend",
    "Audience": "LMS_Frontend",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}
```

⚠️ **مهم:** في Production، ضع `SecretKey` في **Environment Variables** أو **Azure Key Vault**!

### ✅ 5. Identity Configuration

```csharp
builder.Services.AddIdentity<User, Role>(options =>
{
    // Password requirements
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    
    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    
    // User settings
    options.User.RequireUniqueEmail = true;
});
```

### ✅ 6. Services & Controllers

#### IAuthService
- `RegisterAsync()` - تسجيل مستخدم جديد
- `LoginAsync()` - تسجيل دخول
- `RefreshTokenAsync()` - تجديد Access Token
- `RevokeTokenAsync()` - إلغاء Refresh Token (Logout)

#### AuthController
```
POST /api/auth/register    - Register new user
POST /api/auth/login       - Login
POST /api/auth/refresh     - Refresh access token
POST /api/auth/logout      - Logout (revoke refresh token)
GET  /api/auth/me          - Get current user info (Protected)
```

### ✅ 7. Database Seeder

**Default Roles:**
- Admin
- Teacher
- Student

**Default Admin User:**
- Email: `admin@lms.com`
- Password: `Admin@123456`
- Role: Admin

---

## 🚀 كيفية الاستخدام

### 1️⃣ إنشاء Migration

```bash
cd d:\Final_Project\BackEnd\backend_project

# احذف migrations القديمة إذا وجدت
rmdir /S /Q Migrations

# أنشئ migration جديد
dotnet ef migrations add InitialWithIdentityAndJwt
```

### 2️⃣ إنشاء Database

```bash
# أنشئ database
dotnet ef database update
```

### 3️⃣ تشغيل التطبيق

```bash
dotnet run
```

الـ Seeder سيعمل تلقائياً ويضيف:
- 3 Roles (Admin, Teacher, Student)
- Admin user

---

## 📋 API Examples

### Register (تسجيل مستخدم جديد)

```http
POST /api/auth/register
Content-Type: application/json

{
  "name": "Ahmed Mohamed",
  "email": "ahmed@example.com",
  "password": "Test@123456",
  "confirmPassword": "Test@123456",
  "phoneNumber": "+201234567890"
}
```

**Response:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "zxcv1234...",
  "expiresAt": "2025-10-24T12:00:00Z",
  "user": {
    "id": "0000018b-...",
    "name": "Ahmed Mohamed",
    "email": "ahmed@example.com",
    "roles": ["Student"]
  }
}
```

### Login (تسجيل دخول)

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "admin@lms.com",
  "password": "Admin@123456"
}
```

**Response:** Same as Register

### Get Current User (Protected)

```http
GET /api/auth/me
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

**Response:**
```json
{
  "id": "0000018b-...",
  "name": "Ahmed Mohamed",
  "email": "ahmed@example.com",
  "roles": ["Student"]
}
```

### Refresh Token

```http
POST /api/auth/refresh
Content-Type: application/json

{
  "refreshToken": "zxcv1234..."
}
```

### Logout

```http
POST /api/auth/logout
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
Content-Type: application/json

{
  "refreshToken": "zxcv1234..."
}
```

---

## 🔒 استخدام Authorization

### في Controllers

```csharp
[Authorize] // يتطلب تسجيل دخول
public class CoursesController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCourses()
    {
        // Accessible by any authenticated user
    }

    [HttpPost]
    [Authorize(Roles = "Teacher,Admin")] // فقط Teacher أو Admin
    public async Task<IActionResult> CreateCourse()
    {
        // Only Teacher or Admin can create
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")] // فقط Admin
    public async Task<IActionResult> DeleteCourse(Guid id)
    {
        // Only Admin can delete
    }
}
```

### الحصول على معلومات المستخدم الحالي

```csharp
[Authorize]
[HttpGet("my-courses")]
public async Task<IActionResult> GetMyCourses()
{
    // Get current user ID
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var userGuid = Guid.Parse(userId);
    
    // Get user name
    var userName = User.FindFirst(ClaimTypes.Name)?.Value;
    
    // Get user email
    var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
    
    // Check if user has role
    var isAdmin = User.IsInRole("Admin");
    
    // Get all roles
    var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
    
    // Your logic here...
}
```

---

## 🔐 Password Requirements

- **طول:** 8 أحرف على الأقل
- **رقم:** رقم واحد على الأقل
- **حرف صغير:** a-z
- **حرف كبير:** A-Z
- **رمز خاص:** @, #, $, %, etc.

**أمثلة صحيحة:**
- `Test@123456`
- `Admin@123`
- `MyPass$2024`

---

## 🛡️ Security Features

### ✅ Password Hashing
- يستخدم Identity `PBKDF2` مع salt
- لا يتم تخزين passwords في plain text

### ✅ Lockout Protection
- بعد 5 محاولات فاشلة → حظر لمدة 15 دقيقة

### ✅ JWT Tokens
- Access Token: صالح لمدة ساعة
- Refresh Token: صالح لمدة 7 أيام
- Tokens مخزنة بشكل مشفر (SHA256 Hash)

### ✅ HTTPS Only
- JWT يعمل فقط على HTTPS في Production

---

## 📊 Database Tables

| Table | Description |
|-------|-------------|
| `users` | جدول المستخدمين + Identity fields |
| `roles` | الأدوار |
| `user_roles` | ربط Users × Roles |
| `user_claims` | Claims إضافية للمستخدمين |
| `user_logins` | External logins |
| `user_tokens` | Password reset, Email confirmation |
| `role_claims` | Claims للأدوار |
| `sessions` | Refresh tokens (Custom table) |

---

## 🎯 الخطوات التالية الموصى بها

### 1. Email Confirmation
- إضافة Email service (SendGrid, MailKit)
- تفعيل `options.SignIn.RequireConfirmedEmail = true`
- إرسال email تأكيد عند التسجيل

### 2. External Logins
- Google OAuth
- Facebook Login
- Microsoft Account

### 3. Two-Factor Authentication (2FA)
```csharp
options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
```

### 4. Password Reset
- إضافة endpoints للـ forgot password
- إرسال reset link بالـ email

### 5. Role-Based Permissions
- إضافة Permission system أكثر تفصيلاً
- Claims-based authorization

---

## ⚠️ ملاحظات الأمان

### Production Checklist

- [ ] تغيير `JwtSettings:SecretKey` لـ key قوي (64+ characters)
- [ ] نقل Secret Key لـ Environment Variables
- [ ] تفعيل HTTPS
- [ ] تفعيل Email Confirmation
- [ ] Rate limiting على endpoints الحساسة
- [ ] تفعيل CORS بشكل صحيح
- [ ] Logging للـ security events
- [ ] تفعيل 2FA للـ Admins

---

## 🐛 Troubleshooting

### مشكلة: "Invalid email or password"
- تأكد من Password requirements
- تحقق من email صحيح

### مشكلة: "Account is locked out"
- انتظر 15 دقيقة
- أو reset من database مباشرة

### مشكلة: "Unauthorized" (401)
- تأكد من Access Token صالح
- Token قد يكون منتهي → استخدم Refresh Token

### مشكلة: Token منتهي بسرعة
- غيّر `AccessTokenExpirationMinutes` في appsettings.json

---

## 📚 Resources

- [ASP.NET Core Identity Documentation](https://docs.microsoft.com/aspnet/core/security/authentication/identity)
- [JWT.io](https://jwt.io/) - لفحص JWT tokens
- [OWASP Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)

---

**🎊 Identity & JWT Authentication جاهز للاستخدام!**

**Default Admin:**
- Email: `admin@lms.com`
- Password: `Admin@123456`
