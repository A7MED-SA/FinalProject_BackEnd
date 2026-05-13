# كتيب APIs: Auth و Profile

تاريخ التوثيق: 2026-05-13  
المصدر: الكود الحالي داخل `backend_project`، خصوصا:

- `Controllers/Auth/AuthController.cs`
- `Controllers/Auth/OAuthController.cs`
- `Controllers/ProfileController.cs`
- `services/Authentication/*`
- `services/ProfileService.cs`
- `DTOs/Auth/*`
- `DTOs/Profile/*`

> ملاحظة: أمثلة المسارات مكتوبة lowercase مثل `/api/auth/login`. ASP.NET Core routing عادة غير حساس لحالة الحروف، لكن أسماء الـ controllers في الكود تنتج أيضا `/api/Auth`, `/api/OAuth`, `/api/Profile`.

## 1. نظرة عامة

النظام يستخدم:

- ASP.NET Core Identity لإدارة المستخدمين وكلمات المرور والأدوار.
- JWT Bearer Authentication للـ access token.
- Refresh token محفوظ في جدول `sessions` كـ hash فقط.
- OTP من 6 أرقام لتأكيد الإيميل وإعادة تعيين كلمة المرور.
- Roles افتراضيا: `Student`, `Teacher`, `Admin`.
- JSON enum values تظهر كنصوص بسبب `JsonStringEnumConverter`.

أي endpoint عليها `[Authorize]` لازم تبعت:

```http
Authorization: Bearer <accessToken>
Content-Type: application/json
```

## 2. إعدادات مهمة من الكود

### Password Policy

كلمة المرور لازم تحقق:

- 8 أحرف على الأقل.
- تحتوي رقم.
- تحتوي حرف lowercase.
- تحتوي حرف uppercase.
- تحتوي رمز non-alphanumeric مثل `@` أو `#`.

### Lockout

- بعد 5 محاولات دخول فاشلة، الحساب يتقفل.
- مدة القفل 15 دقيقة.

### Token Settings

- Access token صالح حسب `JwtSettings.AccessTokenExpirationMinutes`، والقيمة الحالية في `appsettings.json` هي 60 دقيقة.
- Refresh token صالح نظريا حسب `JwtSettings.RefreshTokenExpirationDays`، والقيمة الحالية 7 أيام.
- في الكود الحالي، refresh endpoint يبحث عن session active بنفس refresh token hash، ويتحقق من IP و User-Agent.

### JWT Claims

الـ access token يحتوي:

- `sub`: User Id.
- `email`: Email.
- `jti`: token id عشوائي.
- `sid`: Session Id.
- `FullName`: الاسم الكامل.
- `role`: لكل role عند المستخدم.
- `permission`: لكل permission جاية من roles.

Controllers تستخرج المستخدم غالبا من `ClaimTypes.NameIdentifier`، والـ `sub` يتم مابته له في JWT middleware الافتراضي.

## 3. DTOs والـ Enums

### Enums

```json
{
  "Gender": ["Male", "Female", "Other", "PreferNotToSay"],
  "PhoneType": ["Primary", "Secondary"],
  "AddressType": ["Billing", "Home", "Work"],
  "StoredFileType": ["Image", "Video", "Document", "Recording", "Certificate"],
  "FileVisibility": ["Public", "EnrolledOnly", "Private"]
}
```

### AuthResponseDto

يرجع من login و refresh و OAuth:

```json
{
  "accessToken": "jwt...",
  "refreshToken": "base64-random-token",
  "sessionId": "guid",
  "expiresAt": "2026-05-13T12:00:00Z",
  "user": {
    "id": "guid",
    "email": "user@example.com",
    "fullName": "Ahmed Ali",
    "profilePictureUrl": null,
    "isActive": true,
    "emailConfirmed": true,
    "roles": ["Student"]
  }
}
```

> `profilePictureUrl` داخل AuthResponse حاليا بيرجع `null` حتى لو للمستخدم صورة؛ صورة البروفايل الحقيقية تظهر في Profile APIs.

### ProfileDto

يرجع من `/api/profile/me` و update profile و set picture:

```json
{
  "id": "guid",
  "fullName": "Ahmed Ali",
  "email": "ahmed@example.com",
  "bio": "Short bio",
  "gender": "Male",
  "dateOfBirth": "2000-01-01",
  "nationality": "Egyptian",
  "profileImageUrl": "http://...",
  "createdAt": "2026-05-13T10:00:00Z",
  "phones": [
    {
      "id": "guid",
      "phoneNumber": "+201000000000",
      "type": "Primary",
      "isVerified": false,
      "isDefault": true
    }
  ],
  "addresses": [
    {
      "id": "guid",
      "type": "Home",
      "streetLine1": "Street 1",
      "streetLine2": null,
      "city": "Cairo",
      "stateProvince": null,
      "postalCode": "11511",
      "country": "Egypt",
      "contactPhone": null,
      "isDefault": true
    }
  ]
}
```

## 4. Auth APIs

### 4.1 Register

```http
POST /api/auth/register
```

ينشئ مستخدم جديد، يضيف له role افتراضي `Student`، يسجل phone/address لو موجودين، ويرسل OTP لتأكيد الإيميل.

Request:

```json
{
  "firstName": "Ahmed",
  "lastName": "Ali",
  "email": "ahmed@example.com",
  "password": "Test@123456",
  "confirmPassword": "Test@123456",
  "gender": "Male",
  "dateOfBirth": "2000-01-01",
  "phoneNumber": "+201000000000",
  "country": "Egypt",
  "city": "Cairo",
  "streetLine1": "Street 1",
  "postalCode": "11511"
}
```

Required:

- `email`
- `password`
- `confirmPassword`
- `firstName`, `lastName`
- `gender`, `dateOfBirth`
- `phoneNumber`
- `country`, `city`, `streetLine1`, `postalCode`

Response 200:

```json
{
  "message": "Registration successful. Please check your email for verification code.",
  "user": {
    "userId": "guid",
    "email": "ahmed@example.com",
    "message": "User registered successfully. Please verify your email."
  }
}
```

كيف تعمل داخليا:

1. يتأكد أن الإيميل غير مستخدم.
2. يبدأ database transaction.
3. ينشئ `User` بـ `EmailConfirmed = false` و `IsActive = false`.
4. ينشئ password hash عن طريق Identity.
5. يضيف role `Student`.
6. لو `phoneNumber` موجود، يضيفه كـ default primary phone وغير verified.
7. لو `country` و `city` و `postalCode` موجودين، يضيف address default من نوع `Home`.
8. يولد OTP تأكيد الإيميل لمدة 15 دقيقة ويرسله بالإيميل.
9. يسجل activity log باسم `Register`.

أخطاء شائعة:

```json
{ "error": "User with this email already exists" }
```

أو validation response من ASP.NET لو fields غير صحيحة:

```json
{
  "success": false,
  "data": null,
  "message": "Validation failed",
  "errors": ["The Email field is not a valid e-mail address."]
}
```

### 4.2 Verify Email

```http
POST /api/auth/verify-email
```

يفعل الحساب بعد إدخال OTP.

Request:

```json
{
  "email": "ahmed@example.com",
  "token": "123456"
}
```

Response 200:

```json
{
  "message": "Email verified successfully. Your account is now active."
}
```

كيف تعمل داخليا:

1. يبحث عن المستخدم بالإيميل.
2. يعمل hash للـ OTP ويقارنه بآخر token غير مستخدم وغير منتهي.
3. يضبط `EmailConfirmed = true`.
4. يضبط `IsActive = true`.
5. يعلم الـ OTP كـ used.
6. يسجل activity log باسم `EmailVerified`.

أخطاء:

```json
{ "error": "User not found" }
```

```json
{ "error": "Invalid or expired OTP" }
```

### 4.3 Resend Verification

```http
POST /api/auth/resend-verification
```

يرسل OTP جديد لتأكيد الإيميل.

Request:

```json
{
  "email": "ahmed@example.com"
}
```

Response 200:

```json
{
  "message": "Verification code sent to your email."
}
```

كيف تعمل داخليا:

1. يبحث عن المستخدم.
2. لو الإيميل متأكد بالفعل يرجع error.
3. يلغي أي OTP قديم غير مستخدم لنفس النوع.
4. يولد OTP جديد ويرسله.

أخطاء:

```json
{ "error": "User not found" }
```

```json
{ "error": "Email already verified" }
```

### 4.4 Login

```http
POST /api/auth/login
```

يسجل دخول ويرجع access token و refresh token و session id.

Request:

```json
{
  "email": "ahmed@example.com",
  "password": "Test@123456",
  "rememberMe": false
}
```

> `rememberMe` موجود في DTO لكن غير مستخدم حاليا في منطق الخدمة.

Response 200:

```json
{
  "accessToken": "jwt...",
  "refreshToken": "random-refresh-token",
  "sessionId": "guid",
  "expiresAt": "2026-05-13T12:00:00Z",
  "user": {
    "id": "guid",
    "email": "ahmed@example.com",
    "fullName": "Ahmed Ali",
    "profilePictureUrl": null,
    "isActive": true,
    "emailConfirmed": true,
    "roles": ["Student"]
  }
}
```

كيف تعمل داخليا:

1. يبحث عن المستخدم بالإيميل.
2. يرفض الدخول لو `IsActive = false` أو `EmailConfirmed = false`.
3. يرفض لو الحساب locked.
4. يتحقق من كلمة المرور مع `lockoutOnFailure = true`.
5. عند الفشل يسجل `LoginFailed` وقد يقفل الحساب بعد المحاولات.
6. عند النجاح يحدث `LastLogin`.
7. يولد refresh token عشوائي.
8. يخزن hash للـ refresh token في session جديدة.
9. يولد access token وفيه `sid` الحقيقي للـ session.
10. يسجل activity log باسم `Login`.

أخطاء:

```json
{ "error": "Invalid email or password" }
```

```json
{ "error": "Account is not active. Please verify your email." }
```

```json
{ "error": "Account is locked. Please try again later." }
```

### 4.5 Refresh Token

```http
POST /api/auth/refresh
```

يعمل token rotation: يأخذ refresh token الحالي ويرجع access token جديد و refresh token جديد.

Request:

```json
{
  "refreshToken": "old-refresh-token"
}
```

Response 200: نفس شكل `AuthResponseDto`.

كيف تعمل داخليا:

1. يعمل hash للـ refresh token القادم.
2. يبحث عن session active بنفس hash.
3. يتحقق أن IP الحالي نفس IP وقت login.
4. يتحقق أن User-Agent الحالي نفس User-Agent وقت login.
5. يرفض لو المستخدم لم يعد active أو email confirmed.
6. يولد access token جديد.
7. يولد refresh token جديد.
8. يحدث session بـ hash الجديد، وبذلك القديم لا يعود صالحا.

أخطاء:

```json
{ "error": "Invalid refresh token" }
```

```json
{ "error": "Session validation failed" }
```

> مهم للفرونت: لازم refresh يحصل من نفس الجهاز/المتصفح غالبا، لأن التحقق صارم على IP و User-Agent.

### 4.6 Forgot Password

```http
POST /api/auth/forgot-password
```

يرسل OTP لإعادة تعيين كلمة المرور.

Request:

```json
{
  "email": "ahmed@example.com"
}
```

Response 200:

```json
{
  "message": "If the email exists, a password reset code has been sent."
}
```

كيف تعمل داخليا:

1. يبحث عن المستخدم.
2. لو غير موجود يرجع نجاح بدون كشف أن الإيميل غير موجود.
3. لو موجود يولد OTP من نوع `PasswordReset` ويرسله.

### 4.7 Reset Password

```http
POST /api/auth/reset-password
```

يعيد تعيين كلمة المرور باستخدام OTP.

Request:

```json
{
  "email": "ahmed@example.com",
  "token": "123456",
  "newPassword": "NewTest@123456",
  "confirmPassword": "NewTest@123456"
}
```

Response 200:

```json
{
  "message": "Password reset successfully. Please login with your new password."
}
```

كيف تعمل داخليا:

1. يبحث عن المستخدم.
2. يتحقق من OTP من نوع `PasswordReset`.
3. يولد Identity password reset token داخلي.
4. يغير كلمة المرور.
5. يعلم OTP كـ used.
6. يعمل revoke لكل sessions الخاصة بالمستخدم، وبالتالي يلزم login جديد.
7. يسجل activity log باسم `PasswordReset`.

أخطاء:

```json
{ "error": "Invalid or expired OTP" }
```

```json
{ "error": "Failed to reset password: ..." }
```

### 4.8 Logout

```http
POST /api/auth/logout
Authorization: Bearer <accessToken>
```

يلغي session الحالية فقط.

Response 200:

```json
{
  "message": "Logged out successfully"
}
```

كيف تعمل داخليا:

1. يقرأ claim `sid` من access token.
2. يعمل `IsActive = false` للـ session.

أخطاء:

```json
{ "error": "Invalid session" }
```

### 4.9 Logout All

```http
POST /api/auth/logout-all
Authorization: Bearer <accessToken>
```

يلغي كل sessions النشطة للمستخدم ما عدا session الحالية.

Response 200:

```json
{
  "message": "All other sessions logged out successfully"
}
```

كيف تعمل داخليا:

1. يقرأ user id من token.
2. يقرأ `sid` الحالي.
3. يجعل كل sessions للمستخدم inactive باستثناء session الحالية.

### 4.10 Get Sessions

```http
GET /api/auth/sessions
Authorization: Bearer <accessToken>
```

يرجع sessions النشطة للمستخدم.

Response 200:

```json
[
  {
    "id": "guid",
    "ipAddress": "127.0.0.1",
    "userAgent": "Mozilla/5.0 ...",
    "createdAt": "2026-05-13T10:00:00Z",
    "lastUsed": "2026-05-13T10:30:00Z",
    "isActive": true
  }
]
```

## 5. OAuth APIs

### 5.1 Google Login

```http
POST /api/oauth/google
```

Request:

```json
{
  "idToken": "google-id-token",
  "provider": "google"
}
```

Response 200: نفس `AuthResponseDto`.

كيف تعمل داخليا:

1. يتأكد أن `provider` يساوي `google`.
2. يتحقق من Google ID token باستخدام `Authentication:Google:ClientId`.
3. يأخذ email و givenName و familyName من payload.
4. يبحث عن user بالإيميل.
5. لو غير موجود ينشئ user جديد active ومؤكد الإيميل ويضيف role `Student`.
6. يربط external login provider باسم `Google`.
7. ينشئ session و access/refresh tokens مثل login العادي.

أخطاء:

```json
{ "error": "Invalid provider. Use 'google' for this endpoint." }
```

```json
{ "error": "Google authentication failed: ..." }
```

### 5.2 Microsoft Login

```http
POST /api/oauth/microsoft
```

Request:

```json
{
  "idToken": "microsoft-access-token",
  "provider": "microsoft"
}
```

Response 200: نفس `AuthResponseDto`.

كيف تعمل داخليا:

1. يتأكد أن `provider` يساوي `microsoft`.
2. يستخدم token كـ Bearer للنداء على Microsoft Graph: `https://graph.microsoft.com/v1.0/me`.
3. يأخذ email من `mail` أو `userPrincipalName`.
4. ينشئ أو يجد المستخدم.
5. يربط external login provider باسم `Microsoft`.
6. ينشئ session و tokens.

أخطاء:

```json
{ "error": "Invalid provider. Use 'microsoft' for this endpoint." }
```

```json
{ "error": "Microsoft authentication failed: ..." }
```

## 6. Profile APIs

كل endpoints هنا محمية بـ `[Authorize]` ما عدا public profile:

```http
GET /api/profile/{userId}
```

### 6.1 Get My Profile

```http
GET /api/profile/me
Authorization: Bearer <accessToken>
```

يرجع بيانات المستخدم الحالي كاملة: الاسم، الإيميل، bio، gender، تاريخ الميلاد، الجنسية، صورة البروفايل، phones، addresses.

Response 200: `ProfileDto`.

كيف تعمل داخليا:

1. يقرأ user id من token.
2. يحمل user بشرط `DeletedAt == null`.
3. يعمل include لـ:
   - `UserPhones`
   - `Addresses` غير المحذوفة
   - `ProfileImageFile`
4. يبني `profileImageUrl` من `IObjectStorage.GetPublicUrl` لو فيه صورة.

أخطاء:

```json
{ "error": "User not found" }
```

### 6.2 Get Public Profile

```http
GET /api/profile/{userId}
```

Endpoint عام بدون login. يرجع نسخة مختصرة لا تحتوي email ولا phones ولا addresses.

Response 200:

```json
{
  "id": "guid",
  "fullName": "Ahmed Ali",
  "bio": "Short bio",
  "nationality": "Egyptian",
  "profileImageUrl": "http://...",
  "createdAt": "2026-05-13T10:00:00Z"
}
```

كيف تعمل داخليا:

1. يبحث عن user بالـ `userId` بشرط غير محذوف.
2. يحمل `ProfileImageFile`.
3. يرجع بيانات عامة فقط.

### 6.3 Update Profile

```http
PUT /api/profile
Authorization: Bearer <accessToken>
```

Request:

```json
{
  "firstName": "Ahmed",
  "lastName": "Ali",
  "bio": "Backend student",
  "gender": "Male",
  "dateOfBirth": "2000-01-01",
  "nationality": "Egyptian"
}
```

Response 200: `ProfileDto` بعد التعديل.

Fields:

- `firstName`: max 100.
- `lastName`: max 100.
- `bio`: max 1000.
- `gender`: enum.
- `dateOfBirth`: تاريخ بصيغة `YYYY-MM-DD`.
- `nationality`: max 100.

كيف تعمل داخليا:

1. يحمل user الحالي مع phones و addresses والصورة.
2. يحدث فقط الحقول التي ليست `null`.
3. يضبط `UpdatedAt = DateTime.UtcNow`.
4. يرجع profile كامل.

> ملاحظة مهمة: `firstName` و `lastName` في DTO قيمتهم الافتراضية empty string، فلو بعت body بدونهم قد يتحولوا إلى empty string حسب طريقة serialization. الأفضل للفرونت يبعت القيم الحالية أو يتم تعديل DTO لاحقا إلى nullable لو مطلوب partial update حقيقي.

### 6.4 Set Profile Picture

```http
POST /api/profile/picture
Authorization: Bearer <accessToken>
```

يربط صورة بروفايل موجودة مسبقا في جدول files.

Request:

```json
{
  "fileId": "guid"
}
```

Response 200: `ProfileDto` بعد إضافة الصورة.

كيف تعمل داخليا:

1. يقرأ user id من token.
2. يبحث عن file بالـ `fileId`.
3. يشترط:
   - `FileType = Image`
   - `Status = Ready`
   - `DeletedAt = null`
   - `UploadedBy` هو نفس المستخدم الحالي
4. لو فيه صورة قديمة مختلفة، يعمل soft delete للملف القديم ويضبط status `Deleted`.
5. يغير visibility للصورة الجديدة إلى `Public`.
6. يربط `user.ProfileImageFileId = file.Id`.
7. يرجع profile كامل.

أخطاء:

```json
{ "error": "Invalid image file" }
```

أو 403 لو الملف ليس مرفوعا بواسطة نفس المستخدم.

### 6.5 كيف ترفع صورة قبل Set Profile Picture؟

الصورة لا ترفع من `/api/profile/picture` مباشرة. التدفق المتوقع:

1. اطلب upload URL:

```http
POST /api/media/upload-url
Authorization: Bearer <accessToken>
```

```json
{
  "fileType": "Image",
  "fileName": "avatar.jpg",
  "contentType": "image/jpeg",
  "fileSizeBytes": 245000,
  "visibility": "Private"
}
```

2. ارفع الملف فعليا على `uploadUrl` الراجع من Media API.
3. أكد الرفع:

```http
POST /api/media/confirm-upload
Authorization: Bearer <accessToken>
```

```json
{
  "fileId": "guid-from-upload-url",
  "objectKey": "object-key-from-upload-url",
  "bucket": "bucket-from-upload-url"
}
```

4. اربط الصورة بالبروفايل:

```http
POST /api/profile/picture
Authorization: Bearer <accessToken>
```

```json
{
  "fileId": "same-file-id"
}
```

### 6.6 Delete Profile Picture

```http
DELETE /api/profile/picture
Authorization: Bearer <accessToken>
```

Response 200:

```json
{
  "message": "Profile picture deleted successfully"
}
```

كيف تعمل داخليا:

1. يحمل المستخدم وصورة البروفايل.
2. لو لا توجد صورة يرجع error.
3. يعمل soft delete للملف:
   - `DeletedAt = DateTime.UtcNow`
   - `Status = Deleted`
4. يفصل الصورة عن المستخدم:
   - `ProfileImageFileId = null`

أخطاء:

```json
{ "error": "User has no profile picture" }
```

### 6.7 Add Phone

```http
POST /api/profile/phones
Authorization: Bearer <accessToken>
```

Request:

```json
{
  "phoneNumber": "+201000000000",
  "type": "Primary",
  "isDefault": true
}
```

Response 201:

```json
{
  "id": "guid",
  "phoneNumber": "+201000000000",
  "type": "Primary",
  "isVerified": false,
  "isDefault": true
}
```

Fields:

- `phoneNumber`: required, phone format, max 20.
- `type`: `Primary` أو `Secondary`، الافتراضي `Primary`.
- `isDefault`: الافتراضي `false`.

كيف تعمل داخليا:

1. يحمل المستخدم و phones.
2. يمنع تكرار نفس الرقم لنفس المستخدم.
3. لو `isDefault = true` يلغي default من كل الأرقام الأخرى.
4. لو هذا أول رقم، يجعله default حتى لو `isDefault = false`.
5. يضيف الرقم كـ `IsVerified = false`.

أخطاء:

```json
{ "error": "Phone number already exists" }
```

### 6.8 Delete Phone

```http
DELETE /api/profile/phones/{phoneId}
Authorization: Bearer <accessToken>
```

Response 200:

```json
{
  "message": "Phone deleted successfully"
}
```

كيف تعمل داخليا:

1. يبحث عن phone يخص المستخدم الحالي.
2. لو الرقم كان default ويوجد رقم آخر، يجعل أقدم رقم آخر default.
3. يحذف الرقم hard delete من `UserPhones`.

أخطاء:

```json
{ "error": "Phone not found" }
```

### 6.9 Set Default Phone

```http
PUT /api/profile/phones/{phoneId}/default
Authorization: Bearer <accessToken>
```

Response 200:

```json
{
  "message": "Default phone set successfully"
}
```

كيف تعمل داخليا:

1. يحمل كل phones الخاصة بالمستخدم.
2. يتأكد أن `phoneId` موجود.
3. يجعل هذا الرقم فقط `IsDefault = true` والباقي `false`.

### 6.10 Add Address

```http
POST /api/profile/addresses
Authorization: Bearer <accessToken>
```

Request:

```json
{
  "type": "Home",
  "streetLine1": "Street 1",
  "streetLine2": "Floor 2",
  "city": "Cairo",
  "stateProvince": "Cairo",
  "postalCode": "11511",
  "country": "Egypt",
  "contactPhone": "+201000000000",
  "isDefault": true
}
```

Response 201:

```json
{
  "id": "guid",
  "type": "Home",
  "streetLine1": "Street 1",
  "streetLine2": "Floor 2",
  "city": "Cairo",
  "stateProvince": "Cairo",
  "postalCode": "11511",
  "country": "Egypt",
  "contactPhone": "+201000000000",
  "isDefault": true
}
```

Required:

- `streetLine1`
- `city`
- `postalCode`
- `country`

Optional:

- `type`: الافتراضي `Home`.
- `streetLine2`
- `stateProvince`
- `contactPhone`
- `isDefault`: الافتراضي `false`.

كيف تعمل داخليا:

1. يحمل المستخدم والعناوين غير المحذوفة.
2. لو `isDefault = true` يلغي default من باقي العناوين.
3. لو هذا أول عنوان، يجعله default تلقائيا.
4. ينشئ address جديد.

### 6.11 Update Address

```http
PUT /api/profile/addresses/{addressId}
Authorization: Bearer <accessToken>
```

Request:

```json
{
  "type": "Work",
  "streetLine1": "New Street",
  "streetLine2": null,
  "city": "Giza",
  "stateProvince": "Giza",
  "postalCode": "12511",
  "country": "Egypt",
  "contactPhone": "+201111111111",
  "isDefault": true
}
```

Response 200:

```json
{
  "id": "guid",
  "type": "Work",
  "streetLine1": "New Street",
  "streetLine2": "Floor 2",
  "city": "Giza",
  "stateProvince": "Giza",
  "postalCode": "12511",
  "country": "Egypt",
  "contactPhone": "+201111111111",
  "isDefault": true
}
```

كيف تعمل داخليا:

1. يبحث عن address يخص المستخدم وغير محذوف.
2. يحدث فقط الحقول التي ليست `null`.
3. لو `isDefault = true` يجعل هذا العنوان default ويلغي الباقي.
4. يضبط `UpdatedAt`.

> ملاحظة: لأن التحديث يتجاهل القيم `null`، إرسال `"streetLine2": null` لن يمسح القيمة القديمة. لو مطلوب مسح حقول اختيارية، يحتاج تعديل في API.

### 6.12 Delete Address

```http
DELETE /api/profile/addresses/{addressId}
Authorization: Bearer <accessToken>
```

Response 200:

```json
{
  "message": "Address deleted successfully"
}
```

كيف تعمل داخليا:

1. يبحث عن address يخص المستخدم وغير محذوف.
2. يعمل soft delete بوضع `DeletedAt = DateTime.UtcNow`.

> في الكود الحالي، لو حذفت default address لا يتم تعيين عنوان آخر كـ default تلقائيا.

### 6.13 Set Default Address

```http
PUT /api/profile/addresses/{addressId}/default
Authorization: Bearer <accessToken>
```

Response 200:

```json
{
  "message": "Default address set successfully"
}
```

كيف تعمل داخليا:

1. يحمل كل العناوين غير المحذوفة للمستخدم.
2. يتأكد أن `addressId` موجود.
3. يجعل هذا العنوان فقط default.

## 7. رحلة مستخدم كاملة للفرونت

### 7.1 Register + Verify + Login

1. `POST /api/auth/register`
2. المستخدم يستلم OTP على الإيميل.
3. `POST /api/auth/verify-email`
4. `POST /api/auth/login`
5. خزّن:
   - `accessToken` للـ Authorization header.
   - `refreshToken` في مكان آمن.
   - `sessionId` لو محتاج تعرض الجلسة الحالية.

### 7.2 Auto Refresh

عند قرب انتهاء access token:

1. `POST /api/auth/refresh` بالـ refresh token الحالي.
2. استبدل access token والـ refresh token بالنسخة الجديدة.
3. لا تستخدم refresh token القديم بعد rotation.

### 7.3 Profile Edit

1. `GET /api/profile/me`
2. اعرض البيانات.
3. عند التعديل: `PUT /api/profile`
4. للهواتف:
   - إضافة: `POST /api/profile/phones`
   - default: `PUT /api/profile/phones/{phoneId}/default`
   - حذف: `DELETE /api/profile/phones/{phoneId}`
5. للعناوين:
   - إضافة: `POST /api/profile/addresses`
   - تعديل: `PUT /api/profile/addresses/{addressId}`
   - default: `PUT /api/profile/addresses/{addressId}/default`
   - حذف: `DELETE /api/profile/addresses/{addressId}`

## 8. جدول سريع لكل endpoints

| Method | Endpoint | Auth | الوظيفة |
|---|---|---:|---|
| POST | `/api/auth/register` | لا | تسجيل مستخدم جديد وإرسال OTP |
| POST | `/api/auth/verify-email` | لا | تأكيد الإيميل وتفعيل الحساب |
| POST | `/api/auth/resend-verification` | لا | إرسال OTP تأكيد جديد |
| POST | `/api/auth/login` | لا | تسجيل دخول وإنشاء session |
| POST | `/api/auth/refresh` | لا | تجديد tokens باستخدام refresh token |
| POST | `/api/auth/forgot-password` | لا | إرسال OTP reset password |
| POST | `/api/auth/reset-password` | لا | تغيير كلمة المرور بالـ OTP |
| POST | `/api/auth/logout` | نعم | إلغاء session الحالية |
| POST | `/api/auth/logout-all` | نعم | إلغاء كل الجلسات الأخرى |
| GET | `/api/auth/sessions` | نعم | عرض الجلسات النشطة |
| POST | `/api/oauth/google` | لا | دخول Google |
| POST | `/api/oauth/microsoft` | لا | دخول Microsoft |
| GET | `/api/profile/me` | نعم | عرض بروفايل المستخدم الحالي |
| GET | `/api/profile/{userId}` | لا | عرض بروفايل عام |
| PUT | `/api/profile` | نعم | تحديث بيانات البروفايل |
| POST | `/api/profile/picture` | نعم | ربط صورة بروفايل من fileId |
| DELETE | `/api/profile/picture` | نعم | حذف صورة البروفايل |
| POST | `/api/profile/phones` | نعم | إضافة رقم هاتف |
| DELETE | `/api/profile/phones/{phoneId}` | نعم | حذف رقم هاتف |
| PUT | `/api/profile/phones/{phoneId}/default` | نعم | تعيين رقم default |
| POST | `/api/profile/addresses` | نعم | إضافة عنوان |
| PUT | `/api/profile/addresses/{addressId}` | نعم | تعديل عنوان |
| DELETE | `/api/profile/addresses/{addressId}` | نعم | حذف عنوان |
| PUT | `/api/profile/addresses/{addressId}/default` | نعم | تعيين عنوان default |

## 9. ملاحظات تنفيذية مهمة

- `ChangePasswordDto` موجود في `DTOs/Auth` وخدمة `ChangePasswordAsync` موجودة، لكن لا يوجد endpoint في `AuthController` حاليا لتغيير كلمة المرور للمستخدم المسجل.
- `GET /api/auth/me` مذكور في بعض الملفات القديمة، لكنه غير موجود في `AuthController` الحالي. البديل العملي هو `GET /api/profile/me`.
- `forgot-password` لا يكشف إذا كان الإيميل غير موجود؛ هذه ممارسة جيدة أمنيا.
- `reset-password` يعمل revoke لكل sessions الخاصة بالمستخدم.
- `logout-all` يترك session الحالية active ويلغي الباقي.
- حذف phone هو hard delete، بينما حذف address وصورة profile هو soft delete.
- صورة البروفايل يجب أن تكون file جاهز `Ready` ومرفوع بواسطة نفس المستخدم.
- الـ refresh حساس جدا لتغيير IP أو User-Agent؛ لو المستخدم على شبكة متغيرة قد يفشل refresh.
