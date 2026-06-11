# مراجعة شاملة لمشروع منصة آثاري التعليمية (Athary E-Learning Platform)

> تاريخ المراجعة: 2026-06-10  
> إجمالي عدد الملفات: ~200+ ملف C#  
> الإطار: ASP.NET Core 9.0 Web API  
> قاعدة البيانات: SQL Server عبر Entity Framework Core  
> المصادقة: JWT + ASP.NET Identity  
> التخزين: MinIO (Object Storage)

---

## الفهرس

1. [نظرة عامة على المشروع](#1-نظرة-عامة-على-المشروع)
2. [هيكل المشروع](#2-هيكل-المشروع)
3. [طبقات المشروع](#3-طبقات-المشروع)
4. [شرح كل Controller بالكامل](#4-شرح-كل-controller-بالكامل)
5. [شرح كل Service](#5-شرح-كل-service)
6. [قاعدة البيانات والنماذج (Models)](#6-قاعدة-البيانات-والنماذج-models)
7. [المصادقة والأمان](#7-المصادقة-والأمان)
8. [الـ DTOs](#8-الـ-dtos)
9. [الـ Validators](#9-الـ-validators)
10. [الـ Middleware](#10-الـ-middleware)
11. [التكوين (Configuration)](#11-التكوين-configuration)
12. [الـ Hubs (SignalR)](#12-الـ-hubs-signalr)
13. [الـ Background Workers](#13-الـ-background-workers)
14. [قائمة شاملة بكل API Endpoint](#14-قائمة-شاملة-بكل-api-endpoint)
15. [التقييم العام](#15-التقييم-العام)

---

## 1. نظرة عامة على المشروع

مشروع **منصة آثاري التعليمية** هو نظام إدارة تعلم (LMS) متكامل مبني على ASP.NET Core 9.0. يهدف المشروع إلى توفير منصة تعليمية إلكترونية تدعم:

- **دورات تعليمية** متعددة الوسائط (فيديو، مستندات، اختبارات، جلسات مباشرة)
- **نظام تجارة إلكترونية** (سلة مشتريات، كوبونات، مدفوعات، استرداد)
- **شهادات إتمام** للمتعلمين
- **مراسلات وإعلانات** داخلية
- **نظام تقارير** للمحتوى غير الملائم
- **لوحات تحكم** للطلاب والمدرسين والإداريين
- **إدارة الوسائط** عبر MinIO
- **نظام طلبات تدريس** للمستخدمين الذين يريدون أن يصبحوا مدرسين
- **مراجعات وتقييمات** للدورات

---

## 2. هيكل المشروع

```
FinalProject_BackEnd/
├── BackEnd.sln                         # ملف الحل الرئيسي
├── backend_project/                    # المشروع الرئيسي (Web API)
│   ├── Program.cs                      # نقطة الدخول
│   ├── backend_project.csproj          # ملف المشروع
│   ├── appsettings.json                # إعدادات التطبيق
│   ├── appsettings.Development.json    # إعدادات التطوير
│   ├── Authorization/                  # سياسات الصلاحيات
│   ├── Configuration/                  # فئات الإعدادات
│   ├── Controllers/                    # 40 Controller
│   ├── Data/                           # DbContext + Migrations + Seeder
│   ├── DTOs/                           # 25+ مجلد DTOs
│   ├── Extensions/                     # دوال إضافية للتسجيل
│   ├── Helpers/                        # فئات مساعدة
│   ├── Hubs/                           # SignalR Hubs
│   ├── Middlewares/                    # Middleware مخصص
│   ├── Migrations/                     # 14 EF Core Migration
│   ├── models/                         # 56 Model
│   ├── services/                       # سيرفسات البيزنس لوجيك
│   │   ├── Authentication/             # المصادقة
│   │   ├── Background/                 # خدمات الخلفية
│   │   ├── Implementations/            # تطبيقات السيرفسات
│   │   ├── Interfaces/                 # واجهات السيرفسات
│   │   ├── Media/                      # إدارة الوسائط
│   │   ├── Notifications/             # الإشعارات
│   │   └── TeacherRequests/           # طلبات التدريس
│   ├── Services/                       # نسخة بحرف S كبير (مكررة)
│   ├── Validators/                     # FluentValidation
│   └── Workers/                        # Background Worker
└── tests/                              # مشاريع الاختبارات
    └── CommerceTests/                  # اختبارات التجارة
```

---

## 3. طبقات المشروع (Layers)

### 3.1 طبقة الـ Controllers (طبقة العرض)

تستقبل الـ HTTP Requests وتعيد الـ Responses. كل Controller مسؤول عن مجموعة من الـ Endpoints المتعلقة بمجال معين.

**المميزات:**
- تستخدم `[Authorize]` و `[Roles]` للتحكم بالوصول
- تستخدم `[EnableRateLimiting]` للتحكم بمعدل الطلبات
- تستخدم `[HasPermission]` للتحكم الدقيق بالصلاحيات
- جميعها ترث من `ControllerBase`
- تستخدم `IActionResult` كقيمة معادة
- تستخدم `ApiResponse<T>` لتوحيد شكل الاستجابة

### 3.2 طبقة الـ Services (طبقة البيزنس لوجيك)

تحتوي على منطق الأعمال وتفصل الـ Controllers عن الـ Data Access.

**المميزات:**
- تستخدم نمط الـ Interface/Implementation لفصل التبعيات
- جميع السيرفسات تُسجل كـ Scoped في Program.cs
- تحتوي على Interfaces في `services/Interfaces/` وتطبيقاتها في `services/Implementations/`

### 3.3 طبقة الـ Models (طبقة البيانات)

نماذج EF Core تمثل جداول قاعدة البيانات.

**المميزات:**
- تستخدم `BaseEntity` مع GUID Sequential عبر MassTransit.NewId
- تستخدم Data Annotations + Fluent API في OnModelCreating
- `ApplicationDbContext` يمتد `IdentityDbContext<Guid>` مع تخصيص جداول Identity

### 3.4 طبقة الـ DTOs (طبقة النقل)

تنقل البيانات بين الـ Controller والـ Service.

---

## 4. شرح كل Controller بالكامل

### 4.1 AuthController
**المسار:** `Controllers/Auth/AuthController.cs`  
**الراوت:** `api/auth`  
**عدد الأسطر:** 206

| الـ Endpoint | الـ HTTP | الوظيفة | الصلاحية |
|---|---|---|---|
| `/api/auth/register` | POST | تسجيل مستخدم جديد | عام |
| `/api/auth/login` | POST | تسجيل الدخول | عام |
| `/api/auth/refresh` | POST | تجديد التوكن | عام |
| `/api/auth/verify-email` | POST | تأكيد البريد الإلكتروني | عام |
| `/api/auth/resend-verification` | POST | إعادة إرسال رابط التفعيل | عام |
| `/api/auth/forgot-password` | POST | نسيان كلمة المرور | عام |
| `/api/auth/reset-password` | POST | إعادة تعيين كلمة المرور | عام |
| `/api/auth/logout` | POST | تسجيل الخروج (جلسة واحدة) | مصادق |
| `/api/auth/logout-all` | POST | تسجيل الخروج من كل الجلسات | مصادق |
| `/api/auth/sessions` | GET | عرض الجلسات النشطة | مصادق |

**آلية العمل:**
- `register`: يستدعي `IAuthenticationService.RegisterAsync` الذي ينشئ المستخدم ويرسل رمز التحقق عبر البريد
- `login`: يستدعي `IAuthenticationService.LoginAsync` للتحقق من البيانات وإرجاع JWT + Refresh Token
- `refresh`: يستدعي `ITokenService.RefreshTokenAsync` لتجديد التوكن باستخدام Refresh Token
- `verify-email`: يستدعي `IVerificationService.VerifyEmailAsync` للتحقق من الرمز
- `resend-verification`: يستدعي `IVerificationService.ResendVerificationCodeAsync`
- `forgot-password` و `reset-password`: يستخدمان `IVerificationService` لإرسال وإعادة تعيين
- `logout`: يستخدم `ISessionService.RevokeSessionAsync` لتعطيل الجلسة
- `sessions`: يستخدم `ISessionService.GetUserSessionsAsync`

### 4.2 OAuthController
**المسار:** `Controllers/Auth/OAuthController.cs`  
**الراوت:** `api/oauth`  
**عدد الأسطر:** 67

| الـ Endpoint | الـ HTTP | الوظيفة | الصلاحية |
|---|---|---|---|
| `/api/oauth/google` | POST | تسجيل الدخول عبر Google | عام |
| `/api/oauth/microsoft` | POST | تسجيل الدخول عبر Microsoft | عام |

**آلية العمل:** يستقبل ID Token من المزوّد ويرسله إلى `IOAuthService.ExternalLoginAsync` الذي يتحقق من صحته ويسجل الدخول أو ينشئ حساباً جديداً.

### 4.3 ProfileController
**المسار:** `Controllers/ProfileController.cs`  
**الراوت:** `api/profile`  
**عدد الأسطر:** 330

| الـ Endpoint | الـ HTTP | الوظيفة | الصلاحية |
|---|---|---|---|
| `/api/profile/me` | GET | عرض الملف الشخصي الحالي | مصادق |
| `/api/profile/{userId:guid}` | GET | عرض ملف مستخدم آخر | عام |
| `/api/profile` | PUT | تحديث الملف الشخصي | مصادق |
| `/api/profile/picture` | POST | تعيين صورة الملف الشخصي | مصادق |
| `/api/profile/picture` | DELETE | حذف صورة الملف الشخصي | مصادق |
| `/api/profile/phones` | POST | إضافة رقم هاتف | مصادق |
| `/api/profile/phones/{phoneId}` | DELETE | حذف رقم هاتف | مصادق |
| `/api/profile/phones/{phoneId}/default` | PUT | تعيين هاتف افتراضي | مصادق |
| `/api/profile/addresses` | POST | إضافة عنوان | مصادق |
| `/api/profile/addresses/{addressId}` | PUT | تحديث عنوان | مصادق |
| `/api/profile/addresses/{addressId}` | DELETE | حذف عنوان | مصادق |
| `/api/profile/addresses/{addressId}/default` | PUT | تعيين عنوان افتراضي | مصادق |

**آلية العمل:**
- يستخدم `IProfileService` لجميع العمليات
- `UpdateProfileAsync`: يحدث بيانات المستخدم الأساسية (الاسم، السيرة الذاتية، إلخ)
- إدارة الصور عبر `UploadedFile` وتحديث `ProfileImageFileId`
- إدارة الهواتف عبر `UserPhone` مع تعيين الافتراضي
- إدارة العناوين عبر `Address` مع تعيين الافتراضي

### 4.4 CourseManagementController
**المسار:** `Controllers/CourseManagementController.cs`  
**الراوت:** `api/management/courses`  
**عدد الأسطر:** 203

| الـ Endpoint | الـ HTTP | الوظيفة | الصلاحية |
|---|---|---|---|
| `/api/management/courses` | POST | إنشاء دورة جديدة | مدرس |
| `/api/management/courses/{id}` | GET | عرض تفاصيل الدورة | مدرس |
| `/api/management/courses/{id}` | PUT | تحديث الدورة | مدرس |
| `/api/management/courses/{id}/requirements` | POST | إضافة متطلبات الدورة | مدرس |
| `/api/management/courses/{id}/requirements/{requirementId}` | DELETE | حذف متطلب | مدرس |
| `/api/management/courses/{id}/outcomes` | POST | إضافة مخرجات تعلم | مدرس |
| `/api/management/courses/{id}/outcomes/{outcomeId}` | DELETE | حذف مخرج تعلم | مدرس |
| `/api/management/courses/{id}/submit-for-review` | POST | تقديم الدورة للمراجعة | مدرس |
| `/api/management/courses/{id}` | DELETE | حذف الدورة | مدرس |
| `/api/management/courses/{id}/schedule-deletion` | POST | جدولة حذف الدورة | مدرس |
| `/api/management/courses/{id}/cancel-scheduled-deletion` | POST | إلغاء جدولة الحذف | مدرس |
| `/api/management/courses/{id}/deletion-status` | GET | عرض حالة الحذف المجدول | مدرس |
| `/api/management/courses/{courseId}/image` | PUT | تعيين صورة الدورة | مدرس |

**آلية العمل:**
- يستخدم `ICourseService` لجميع العمليات
- `CreateCourseAsync`: ينشئ الدورة بحالة Draft وينشئ Slug فريد
- `SubmitForReviewAsync`: يغير حالة الدورة إلى PendingReview
- `ScheduleDeletionAsync`: يحدد `ScheduledDeletionAt` و `DeletionReason`
- إدارة المتطلبات والمخرجات عبر جداول منفصلة `CourseRequirement` و `CourseLearningOutcome`

### 4.5 SectionController
**المسار:** `Controllers/SectionController.cs`  
**الراوت:** `api/management/courses/{courseId}/sections`  
**عدد الأسطر:** 222

| الـ Endpoint | الـ HTTP | الوظيفة | الصلاحية |
|---|---|---|---|
| `/api/management/courses/{courseId}/sections` | GET | عرض الأقسام | مصادق |
| `/api/management/courses/{courseId}/sections` | POST | إنشاء قسم جديد | مصادق |
| `/api/management/courses/{courseId}/sections/{sectionId}` | GET | عرض قسم | مصادق |
| `/api/management/courses/{courseId}/sections/{sectionId}` | PUT | تحديث قسم | مصادق |
| `/api/management/courses/{courseId}/sections/{sectionId}` | DELETE | حذف قسم | مصادق |
| `/api/management/courses/{courseId}/sections/reorder` | PUT | إعادة ترتيب الأقسام | مصادق |
| `/api/management/courses/{courseId}/sections/{sectionId}/items` | POST | إضافة عنصر للقسم | مصادق |
| `/api/management/courses/{courseId}/sections/{sectionId}/items/{itemId}` | PUT | تحديث عنصر | مصادق |
| `/api/management/courses/{courseId}/sections/{sectionId}/items/{itemId}` | DELETE | حذف عنصر | مصادق |
| `/api/management/courses/{sectionId}/items/reorder` | PUT | إعادة ترتيب العناصر | مصادق |
| `/api/courses/my-edit-requests` | GET | عرض طلبات التعديل الخاصة بي | مصادق |
| `/api/courses/edit-requests/{requestId}/cancel` | POST | إلغاء طلب تعديل | مصادق |

**آلية العمل:**
- يستخدم `ISectionService` مع `ICourseEditApprovalService`
- للدور المنشورة، يتطلب التعديل موافقة المشرف عبر `CourseEditRequest`
- للدور المسودة أو المؤرشفة، التعديل مباشر دون طلب موافقة
- البوليمورفيزم عبر `SectionItem.ItemType` + `ItemId`

### 4.6 ManagementCoursesController
**المسار:** `Controllers/ManagementCoursesController.cs`  
**الراوت:** `api/management`  
**عدد الأسطر:** 37

| الـ Endpoint | الـ HTTP | الوظيفة | الصلاحية |
|---|---|---|---|
| `/api/management/courses` | GET | عرض كل دورات المدرس | مدرس |

**آلية العمل:** يستخدم `ICourseService.GetInstructorCoursesAsync` لإرجاع دورات المدرس الحالي مع الفلترة والصفحات.

### 4.7 PublicCourseController
**المسار:** `Controllers/PublicCourseController.cs`  
**الراوت:** `api/public/courses`  
**عدد الأسطر:** 89

| الـ Endpoint | الـ HTTP | الوظيفة | الصلاحية |
|---|---|---|---|
| `/api/public/courses` | GET | عرض الدورات المنشورة (مع بحث وفلترة) | عام |
| `/api/public/courses/{id:guid}` | GET | عرض تفاصيل دورة | عام |
| `/api/public/courses/slug/{slug}` | GET | عرض دورة بالـ Slug | عام |
| `/api/public/courses/search/suggest` | GET | اقتراحات بحث | عام |
| `/api/public/courses/stats` | GET | إحصائيات المنصة | عام |
| `/api/public/courses/{id:guid}/related` | GET | دورات مشابهة | عام |
| `/api/public/courses/filters/options` | GET | خيارات الفلترة | عام |

**آلية العمل:**
- يستخدم `IPublicCourseService` لعرض الدورات المنشورة فقط
- يدعم الفلترة حسب: الفئة، المستوى، اللغة، السعر، التقييم، البحث النصي
- يدعم الترتيب حسب: تاريخ النشر، السعر، التقييم، عدد المسجلين
- `PlatformStatsDto`: يعرض إحصائيات عامة (عدد الدورات، الطلاب، المدرسين، الفئات)

### 4.8 VideoContentController
**المسار:** `Controllers/VideoContentController.cs`  
**الراوت:** `api/courses/{courseId}/videos`  
**عدد الأسطر:** 84

| الـ Endpoint | الـ HTTP | الوظيفة | الصلاحية |
|---|---|---|---|
| `/api/courses/{courseId}/videos/{id}` | GET | عرض تفاصيل الفيديو | مدرس |
| `/api/courses/{courseId}/videos` | POST | إنشاء فيديو جديد | مدرس |
| `/api/courses/{courseId}/videos/{id}` | PUT | تحديث الفيديو | مدرس |
| `/api/courses/{courseId}/videos/{id}` | DELETE | حذف الفيديو | مدرس |

### 4.9 DocumentController
**المسار:** `Controllers/DocumentController.cs`  
**الراوت:** `api/courses/{courseId}/documents`  
**عدد الأسطر:** 84

| الـ Endpoint | الـ HTTP | الوظيفة | الصلاحية |
|---|---|---|---|
| `/api/courses/{courseId}/documents/{id}` | GET | عرض المستند | مدرس |
| `/api/courses/{courseId}/documents` | POST | إنشاء مستند | مدرس |
| `/api/courses/{courseId}/documents/{id}` | PUT | تحديث المستند | مدرس |
| `/api/courses/{courseId}/documents/{id}` | DELETE | حذف المستند | مدرس |

### 4.10 QuizManagementController
**المسار:** `Controllers/QuizManagementController.cs`  
**الراوت:** `api/courses/{courseId}/quizzes`  
**عدد الأسطر:** 129

| الـ Endpoint | الـ HTTP | الوظيفة | الصلاحية |
|---|---|---|---|
| `/api/courses/{courseId}/quizzes/{id}` | GET | عرض تفاصيل الاختبار | مدرس |
| `/api/courses/{courseId}/quizzes` | POST | إنشاء اختبار | مدرس |
| `/api/courses/{courseId}/quizzes/{id}` | PUT | تحديث الاختبار | مدرس |
| `/api/courses/{courseId}/quizzes/{id}` | DELETE | حذف الاختبار | مدرس |
| `/api/courses/{courseId}/quizzes/{quizId}/questions` | POST | إضافة سؤال | مدرس |
| `/api/courses/{courseId}/quizzes/{quizId}/questions/{questionId}` | PUT | تحديث سؤال | مدرس |
| `/api/courses/{courseId}/quizzes/{quizId}/questions/{questionId}` | DELETE | حذف سؤال | مدرس |

### 4.11 LiveSessionController
**المسار:** `Controllers/LiveSessionController.cs`  
**الراوت:** `api/courses/{courseId}/live-sessions`  
**عدد الأسطر:** 85

| الـ Endpoint | الـ HTTP | الوظيفة | الصلاحية |
|---|---|---|---|
| `/api/courses/{courseId}/live-sessions` | GET | عرض الجلسات المباشرة | مدرس |
| `/api/courses/{courseId}/live-sessions` | POST | إنشاء جلسة مباشرة | مدرس |
| `/api/courses/{courseId}/live-sessions/{sessionId}/status` | PUT | تحديث حالة الجلسة | مدرس |
| `/api/courses/{courseId}/live-sessions/{sessionId}` | DELETE | حذف جلسة | مدرس |

### 4.12 LiveAttendanceController
**المسار:** `Controllers/LiveAttendanceController.cs`  
**الراوت:** `api/live-sessions/{sessionId}/attendance`  
**عدد الأسطر:** 72

| الـ Endpoint | الـ HTTP | الوظيفة | الصلاحية |
|---|---|---|---|
| `/api/live-sessions/{sessionId}/attendance/join` | POST | الانضمام للجلسة | مصادق |
| `/api/live-sessions/{sessionId}/attendance/leave` | POST | مغادرة الجلسة | مصادق |
| `/api/live-sessions/{sessionId}/attendance/count` | GET | عدد الحضور الحالي | مصادق |

### 4.13 VideoCommentController
**المسار:** `Controllers/VideoCommentController.cs`  
**الراوت:** `api/videos/{videoId}/comments`  
**عدد الأسطر:** 101

| الـ Endpoint | الـ HTTP | الوظيفة | الصلاحية |
|---|---|---|---|
| `/api/videos/{videoId}/comments` | GET | عرض التعليقات | مصادق |
| `/api/videos/{videoId}/comments` | POST | إضافة تعليق | مصادق |
| `/api/videos/{videoId}/comments/{commentId}` | PUT | تحديث تعليق | مصادق |
| `/api/videos/{videoId}/comments/{commentId}` | DELETE | حذف تعليق | مصادق |
| `/api/videos/{videoId}/comments/{commentId}/like` | POST | إعجاب بتعليق | مصادق |

### 4.14 EnrollmentsController
**المسار:** `Controllers/EnrollmentsController.cs`  
**الراوت:** `api/enrollments`  
**عدد الأسطر:** 121

| الـ Endpoint | الـ HTTP | الوظيفة | الصلاحية |
|---|---|---|---|
| `/api/enrollments` | POST | إنشاء تسجيل جديد | مصادق |
| `/api/enrollments` | GET | عرض تسجيلاتي | مصادق |
| `/api/enrollments/{id}` | GET | عرض تفاصيل تسجيل | مصادق |
| `/api/enrollments/{enrollmentId}/progress` | GET | عرض التقدم | مصادق |
| `/api/enrollments/{enrollmentId}/progress` | PUT | تحديث التقدم | مصادق |
| `/api/enrollments/{enrollmentId}/progress/{contentType}/{contentId}/complete` | POST | إكمال محتوى | مصادق |

### 4.15 QuizAttemptController
**المسار:** `Controllers/QuizAttemptController.cs`  
**الراوت:** `api/enrollments/{enrollmentId}/quizzes/{quizId}/attempts`  
**عدد الأسطر:** 90

| الـ Endpoint | الـ HTTP | الوظيفة | الصلاحية |
|---|---|---|---|
| `POST` | POST | بدء محاولة اختبار جديدة | مصادق |
| `PUT /{attemptId}` | PUT | تقديم المحاولة | مصادق |
| `GET /` | GET | عرض المحاولات السابقة | مصادق |
| `GET /{attemptId}` | GET | عرض تفاصيل محاولة | مصادق |

### 4.16 CartController
**المسار:** `Controllers/CartController.cs`  
**الراوت:** `api/cart`  
**عدد الأسطر:** 90

| الـ Endpoint | الـ HTTP | الوظيفة | الصلاحية |
|---|---|---|---|
| `/api/cart` | GET | عرض محتويات السلة | مصادق |
| `/api/cart/items` | POST | إضافة دورة للسلة | مصادق |
| `/api/cart/items/{itemId}` | DELETE | حذف عنصر من السلة | مصادق |
| `/api/cart/coupon` | POST | تطبيق كوبون | مصادق |
| `/api/cart/coupon` | DELETE | إزالة الكوبون | مصادق |

### 4.17 OrderController
**المسار:** `Controllers/OrderController.cs`  
**الراوت:** `api/orders`  
**عدد الأسطر:** 98

| الـ Endpoint | الـ HTTP | الوظيفة | الصلاحية |
|---|---|---|---|
| `/api/orders` | POST | إنشاء طلب جديد | مصادق |
| `/api/orders` | GET | عرض طلباتي (مدفوعة) | مصادق |
| `/api/orders/{id}` | GET | عرض تفاصيل طلب | مصادق |
| `/api/orders/{orderId}/payments` | POST | دفع طلب | مصادق |
| `/api/orders/{orderId}/payments` | GET | عرض مدفوعات الطلب | مصادق |

### 4.18 CouponController
**المسار:** `Controllers/CouponController.cs`  
**الراوت:** `api/coupons`  
**عدد الأسطر:** 34

| الـ Endpoint | الـ HTTP | الوظيفة | الصلاحية |
|---|---|---|---|
| `/api/coupons/validate` | POST | التحقق من صلاحية كوبون | مصادق |

### 4.19 RefundController
**المسار:** `Controllers/RefundController.cs`  
**الراوت:** `api/refunds`  
**عدد الأسطر:** 50

| الـ Endpoint | الـ HTTP | الوظيفة | الصلاحية |
|---|---|---|---|
| `/api/refunds` | POST | طلب استرداد | مصادق |
| `/api/refunds` | GET | عرض طلبات الاسترداد | مصادق |

### 4.20 PaymentMethodsController
**المسار:** `Controllers/PaymentMethodsController.cs`  
**الراوت:** `api/payment-methods`  
**عدد الأسطر:** 25

| الـ Endpoint | الـ HTTP | الوظيفة | الصلاحية |
|---|---|---|---|
| `/api/payment-methods` | GET | عرض طرق الدفع المتاحة | عام |

### 4.21 WishlistController
**المسار:** `Controllers/WishlistController.cs`  
**الراوت:** `api/wishlist`  
**عدد الأسطر:** 65

| الـ Endpoint | الـ HTTP | الوظيفة | الصلاحية |
|---|---|---|---|
| `/api/wishlist` | GET | عرض المفضلة | مصادق |
| `/api/wishlist/{courseId}` | POST | إضافة دورة للمفضلة | مصادق |
| `/api/wishlist/{courseId}` | DELETE | حذف من المفضلة | مصادق |

### 4.22 ReviewsController
**المسار:** `Controllers/ReviewsController.cs`  
**الراوت:** `api/reviews`  
**عدد الأسطر:** 171

| الـ Endpoint | الـ HTTP | الوظيفة | الصلاحية |
|---|---|---|---|
| `/api/reviews` | POST | إضافة مراجعة | مصادق |
| `/api/reviews/course/{courseId}` | GET | عرض مراجعات دورة | عام |
| `/api/reviews/{id}` | GET | عرض مراجعة | عام |
| `/api/reviews/{id}` | PUT | تحديث مراجعة | مصادق |
| `/api/reviews/{id}` | DELETE | حذف مراجعة | مصادق |
| `/api/reviews/{id}/helpful` | POST | التصويت بمفيد | مصادق |
| `/api/reviews/{id}/flag` | POST | الإبلاغ عن مراجعة | مدرس |
| `/api/reviews/pending` | GET | مراجعات معلقة | مشرف |
| `/api/reviews/{id}/moderate` | PUT | مراجعة معتدلة (قبول/رفض) | مشرف |

### 4.23 CertificatesController
**المسار:** `Controllers/CertificatesController.cs`  
**الراوت:** `api/certificates`  
**عدد الأسطر:** 85

| الـ Endpoint | الـ HTTP | الوظيفة | الصلاحية |
|---|---|---|---|
| `/api/certificates/my` | GET | شهاداتي | مصادق |
| `/api/certificates/{id}` | GET | عرض شهادة | مصادق |
| `/api/certificates/{id}/download` | GET | تحميل الشهادة | مصادق |
| `/api/certificates/verify/{code}` | GET | التحقق من شهادة (مع Rate Limiting) | عام |

### 4.24 CategoryController
**المسار:** `Controllers/CategoryController.cs`  
**الراوت:** `api/categories`  
**عدد الأسطر:** 103

| الـ Endpoint | الـ HTTP | الوظيفة | الصلاحية |
|---|---|---|---|
| `/api/categories` | GET | عرض كل الفئات | عام |
| `/api/categories/{id}` | GET | عرض فئة | عام |
| `/api/categories` | POST | إنشاء فئة | مشرف |
| `/api/categories/{id}` | PUT | تحديث فئة | مشرف |
| `/api/categories/{id}` | DELETE | حذف فئة | مشرف |
| `/api/categories/{categoryId}/image` | PUT | تعيين صورة الفئة | مشرف |

### 4.25 DashboardController
**المسار:** `Controllers/DashboardController.cs`  
**الراوت:** `api/dashboard`  
**عدد الأسطر:** 69

| الـ Endpoint | الـ HTTP | الوظيفة | الصلاحية |
|---|---|---|---|
| `/api/dashboard/student` | GET | لوحة تحكم الطالب | طالب |
| `/api/dashboard/instructor` | GET | لوحة تحكم المدرس | مدرس |

### 4.26 MediaController
**المسار:** `Controllers/Media/MediaController.cs`  
**الراوت:** `api/media`  
**عدد الأسطر:** 162

| الـ Endpoint | الـ HTTP | الوظيفة | الصلاحية |
|---|---|---|---|
| `/api/media/upload-url` | POST | الحصول على رابط رفع (Presigned URL) | مصادق |
| `/api/media/confirm-upload` | POST | تأكيد رفع الملف | مصادق |
| `/api/media/{fileId}/view` | GET | عرض/تحميل ملف | عام/مصادق |

**آلية العمل:**
- `upload-url`: يستخدم `IMediaService.GetUploadUrlAsync` لإنشاء Presigned URL من MinIO
- `confirm-upload`: يحدث حالة الملف إلى `Uploaded` عبر `IMediaService.ConfirmUploadAsync`
- `view`: يستخدم `IMediaService.GetViewUrlAsync` لإرجاع رابط مشاهدة

### 4.27 AdminMediaController
**المسار:** `Controllers/Media/AdminMediaController.cs`  
**الراوت:** `api/admin/media`  
**عدد الأسطر:** 160

| الـ Endpoint | الـ HTTP | الوظيفة | الصلاحية |
|---|---|---|---|
| `/api/admin/media` | GET | عرض كل الملفات | مشرف |
| `/api/admin/media/{fileId}` | GET | عرض تفاصيل ملف | مشرف |
| `/api/admin/media/{fileId}` | DELETE | حذف ناعم لملف | مشرف |
| `/api/admin/media/{fileId}/restore` | POST | استعادة ملف محذوف | مشرف |
| `/api/admin/media/{fileId}/permanent` | DELETE | حذف دائم للملف | مشرف |
| `/api/admin/media/stats` | GET | إحصائيات التخزين | مشرف |

### 4.28 NotificationController
**المسار:** `Controllers/Notifications/NotificationController.cs`  
**الراوت:** `api/notifications`  
**عدد الأسطر:** 127

| الـ Endpoint | الـ HTTP | الوظيفة | الصلاحية |
|---|---|---|---|
| `/api/notifications` | GET | عرض الإشعارات | مصادق |
| `/api/notifications/unread-count` | GET | عدد الإشعارات غير المقروءة | مصادق |
| `/api/notifications/{notificationId}/read` | PATCH | تعيين إشعار كمقروء | مصادق |
| `/api/notifications/mark-all-read` | POST | تعيين الكل مقروء | مصادق |
| `/api/notifications/{notificationId}` | DELETE | حذف إشعار | مصادق |
| `/api/notifications/clear-all` | DELETE | حذف كل الإشعارات | مصادق |

### 4.29 TeacherRequestController
**المسار:** `Controllers/TeacherRequests/TeacherRequestController.cs`  
**الراوت:** `api/teacher-requests`  
**عدد الأسطر:** 259

| الـ Endpoint | الـ HTTP | الوظيفة | الصلاحية |
|---|---|---|---|
| `/api/teacher-requests/can-submit` | GET | التحقق من إمكانية التقديم | مصادق |
| `/api/teacher-requests` | POST | تقديم طلب تدريس | مصادق |
| `/api/teacher-requests/{requestId}` | PUT | تحديث طلب | مصادق |
| `/api/teacher-requests/{requestId}/documents` | POST | إضافة وثيقة للطلب | مصادق |
| `/api/teacher-requests/my-requests` | GET | طلباتي | مصادق |
| `/api/teacher-requests/my-requests/{requestId}` | GET | تفاصيل طلبي | مصادق |
| `/api/teacher-requests/{requestId}/cancel` | DELETE | إلغاء طلب | مصادق |
| `/api/teacher-requests/pending` | GET | الطلبات المعلقة (مشرف) | مشرف |
| `/api/teacher-requests/{requestId}` | GET | تفاصيل طلب (مشرف) | مشرف |
| `/api/teacher-requests/{requestId}/process` | PUT | معالجة طلب (قبول/رفض) | مشرف |
| `/api/teacher-requests/{requestId}` | DELETE | حذف طلب (مشرف) | مشرف |

### 4.30 Admin Certificates Controller
**المسار:** `Controllers/AdminCertificatesController.cs`  
**الراوت:** `api/admin/certificates`  
**عدد الأسطر:** 70

| الـ Endpoint | الـ HTTP | الوظيفة | الصلاحية |
|---|---|---|---|
| `/api/admin/certificates` | GET | عرض كل الشهادات | مشرف |
| `/api/admin/certificates/{id}/revoke` | POST | إلغاء شهادة | مشرف |
| `/api/admin/certificates/issue` | POST | إصدار شهادة | مشرف |

### 4.31 Admin Coupon Controller
**المسار:** `Controllers/AdminCouponController.cs`  
**الراوت:** `api/admin/coupons`  
**عدد الأسطر:** 90

| الـ Endpoint | الـ HTTP | الوظيفة | الصلاحية |
|---|---|---|---|
| `/api/admin/coupons` | GET | عرض كل الكوبونات | مشرف |
| `/api/admin/coupons/{id}` | GET | عرض كوبون | مشرف |
| `/api/admin/coupons` | POST | إنشاء كوبون | مشرف |
| `/api/admin/coupons/{id}` | PUT | تحديث كوبون | مشرف |
| `/api/admin/coupons/{id}/toggle` | PATCH | تفعيل/تعطيل كوبون | مشرف |
| `/api/admin/coupons/{id}` | DELETE | حذف كوبون | مشرف |

### 4.32 Admin Course Controller
**المسار:** `Controllers/AdminCourseController.cs`  
**الراوت:** `api/admin/courses`  
**عدد الأسطر:** 97

| الـ Endpoint | الـ HTTP | الوظيفة | الصلاحية |
|---|---|---|---|
| `/api/admin/courses/pending` | GET | الدورات المعلقة للمراجعة | مشرف |
| `/api/admin/courses/{id}/approve` | POST | الموافقة على دورة | مشرف |
| `/api/admin/courses/{id}/reject` | POST | رفض دورة | مشرف |
| `/api/admin/courses/edit-requests` | GET | طلبات التعديل | مشرف |
| `/api/admin/courses/edit-requests/{requestId}` | GET | تفاصيل طلب تعديل | مشرف |
| `/api/admin/courses/edit-requests/{requestId}/review` | POST | مراجعة طلب تعديل (قبول/رفض) | مشرف |

### 4.33 Admin Dashboard Controller
**المسار:** `Controllers/AdminDashboardController.cs`  
**الراوت:** `api/admin/dashboard`  
**عدد الأسطر:** 85

| الـ Endpoint | الـ HTTP | الوظيفة | الصلاحية |
|---|---|---|---|
| `/api/admin/dashboard/overview` | GET | نظرة عامة على المنصة | مشرف |
| `/api/admin/dashboard/revenue` | GET | تقرير الإيرادات | مشرف |
| `/api/admin/dashboard/user-growth` | GET | نمو المستخدمين | مشرف |
| `/api/admin/dashboard/enrollment-trends` | GET | اتجاهات التسجيل | مشرف |

### 4.34 Admin Payment Method Controller
**المسار:** `Controllers/AdminPaymentMethodController.cs`  
**الراوت:** `api/admin/payment-methods`  
**عدد الأسطر:** 53

| الـ Endpoint | الـ HTTP | الوظيفة | الصلاحية |
|---|---|---|---|
| `/api/admin/payment-methods` | GET | عرض طرق الدفع | مشرف |
| `/api/admin/payment-methods` | POST | إضافة طريقة دفع | مشرف |
| `/api/admin/payment-methods/{id}/toggle` | PATCH | تفعيل/تعطيل طريقة دفع | مشرف |

### 4.35 Admin Refund Controller
**المسار:** `Controllers/AdminRefundController.cs`  
**الراوت:** `api/admin/refunds`  
**عدد الأسطر:** 71

| الـ Endpoint | الـ HTTP | الوظيفة | الصلاحية |
|---|---|---|---|
| `/api/admin/refunds` | GET | عرض طلبات الاسترداد (مع فلتر حالة) | مشرف |
| `/api/admin/refunds/{id}/approve` | POST | الموافقة على استرداد | مشرف |
| `/api/admin/refunds/{id}/reject` | POST | رفض استرداد | مشرف |

### 4.36 Communication Controllers

#### MessagesController
**المسار:** `Controllers/Communication/MessagesController.cs`  
**الراوت:** `api/messages`  
**عدد الأسطر:** 104

| الـ Endpoint | الـ HTTP | الوظيفة | الصلاحية |
|---|---|---|---|
| `/api/messages` | POST | إرسال رسالة (محدد بـ 30/دقيقة) | مصادق |
| `/api/messages/conversations` | GET | عرض المحادثات | مصادق |
| `/api/messages/conversations/{otherUserId}` | GET | عرض رسائل محادثة | مصادق |
| `/api/messages/unread-count` | GET | عدد الرسائل غير المقروءة | مصادق |
| `/api/messages/{messageId}/read` | PATCH | تعيين رسالة كمقروءة | مصادق |
| `/api/messages/{messageId}` | DELETE | حذف رسالة (للمرسل) | مصادق |

#### AnnouncementsController
**المسار:** `Controllers/Communication/AnnouncementsController.cs`  
**الراوت:** `api/announcements`  
**عدد الأسطر:** 117

| الـ Endpoint | الـ HTTP | الوظيفة | الصلاحية |
|---|---|---|---|
| `/api/announcements` | POST | إنشاء إعلان | مصادق |
| `/api/announcements/course/{courseId}` | POST | إنشاء إعلان لدورة محددة | مصادق |
| `/api/announcements` | GET | عرض الإعلانات (حسب الدور) | مصادق |
| `/api/announcements/{id}` | PUT | تحديث إعلان | مصادق |
| `/api/announcements/{id}/deactivate` | PATCH | تعطيل إعلان | مصادق |
| `/api/announcements/{id}` | DELETE | حذف إعلان | مصادق |

#### SystemSettingsController
**المسار:** `Controllers/Communication/SystemSettingsController.cs`  
**الراوت:** `api/system-settings`  
**عدد الأسطر:** 93

| الـ Endpoint | الـ HTTP | الوظيفة | الصلاحية |
|---|---|---|---|
| `/api/system-settings` | GET | عرض كل الإعدادات | مشرف |
| `/api/system-settings/{key}` | GET | عرض إعداد معين | مشرف |
| `/api/system-settings/{key}` | PUT | تحديث إعداد | مشرف |
| `/api/system-settings` | POST | إنشاء إعداد جديد | مشرف |
| `/api/system-settings/{key}` | DELETE | حذف إعداد | مشرف |

#### ActivityLogsController
**المسار:** `Controllers/Communication/ActivityLogsController.cs`  
**الراوت:** `api/activity-logs`  
**عدد الأسطر:** 61

| الـ Endpoint | الـ HTTP | الوظيفة | الصلاحية |
|---|---|---|---|
| `/api/activity-logs` | GET | عرض سجل النشاطات (مع فلترة) | مشرف |

#### ReportsController
**المسار:** `Controllers/Communication/ReportsController.cs`  
**الراوت:** `api/reports`  
**عدد الأسطر:** 67

| الـ Endpoint | الـ HTTP | الوظيفة | الصلاحية |
|---|---|---|---|
| `/api/reports` | POST | الإبلاغ عن محتوى | مصادق |
| `/api/reports/pending` | GET | البلاغات المعلقة | مشرف |
| `/api/reports/{id}/resolve` | PATCH | حل بلاغ (قبول/رفض) | مشرف |

---

## 5. شرح كل Service

### 5.1 خدمات المصادقة (Authentication)

| الـ Interface | التطبيق | الوظيفة |
|---|---|---|
| `IAuthenticationService` | `AuthenticationService` | تسجيل/تسجيل دخول المستخدمين |
| `ITokenService` | `TokenService` | إنشاء/تحديث JWT + Refresh Tokens |
| `ISessionService` | `SessionService` | إدارة جلسات المستخدمين |
| `IVerificationService` | `VerificationService` | إرسال/التحقق من رموز التفعيل |
| `IOAuthService` | `OAuthService` | تسجيل الدخول عبر Google/Microsoft |
| `IPermissionService` | `PermissionService` | إدارة الصلاحيات |

### 5.2 خدمات الملف الشخصي

| الـ Interface | التطبيق | الوظيفة |
|---|---|---|
| `IProfileService` | `ProfileService` | إدارة الملف الشخصي والهواتف والعناوين |

### 5.3 خدمات الدورات

| الـ Interface | التطبيق | الوظيفة |
|---|---|---|
| `ICourseService` | `CourseService` | CRUD الدورات، المتطلبات، المخرجات |
| `ICourseEditApprovalService` | `CourseEditApprovalService` | نظام الموافقة على تعديلات الدورات |
| `ISectionService` | `SectionService` | إدارة الأقسام والعناصر |
| `IPublicCourseService` | `PublicCourseService` | عرض الدورات المنشورة للعموم |
| `ICategoryService` | `CategoryService` | إدارة الفئات |

### 5.4 خدمات المحتوى

| الـ Interface | التطبيق | الوظيفة |
|---|---|---|
| `IVideoContentService` | `VideoContentService` | إدارة محتوى الفيديو |
| `IDocumentService` | `DocumentService` | إدارة المستندات |
| `IQuizManagementService` | `QuizManagementService` | إدارة الاختبارات والأسئلة |
| `ILiveSessionService` | `LiveSessionService` | إدارة الجلسات المباشرة |
| `ILiveAttendanceService` | `LiveAttendanceService` | إدارة حضور الجلسات |
| `IVideoCommentService` | `VideoCommentService` | إدارة تعليقات الفيديو |
| `IQuizAttemptService` | `QuizAttemptService` | إدارة محاولات الاختبارات |
| `IContentProgressService` | `ContentProgressService` | تتبع تقدم المتعلم |
| `IEnrollmentService` | `EnrollmentService` | إدارة التسجيلات في الدورات |

### 5.5 خدمات التجارة

| الـ Interface | التطبيق | الوظيفة |
|---|---|---|
| `ICartService` | `CartService` | إدارة سلة المشتريات |
| `IOrderService` | `OrderService` | إنشاء وإدارة الطلبات |
| `IPaymentService` | `PaymentService` | معالجة المدفوعات |
| `IPaymentGateway` | `MockPaymentGateway` | بوابة دفع وهمية للاختبار |
| `ICouponService` | `CouponService` | إدارة الكوبونات |
| `IRefundService` | `RefundService` | إدارة طلبات الاسترداد |
| `IWishlistService` | `WishlistService` | إدارة المفضلة |

### 5.6 خدمات المراجعات والشهادات

| الـ Interface | التطبيق | الوظيفة |
|---|---|---|
| `IReviewService` | `ReviewService` | إدارة المراجعات والتقييمات |
| `ICertificateService` | `CertificateService` | إصدار/التحقق من الشهادات |

### 5.7 خدمات لوحات التحكم

| الـ Interface | التطبيق | الوظيفة |
|---|---|---|
| `IStudentDashboardService` | `StudentDashboardService` | لوحة تحكم الطالب |
| `IInstructorDashboardService` | `InstructorDashboardService` | لوحة تحكم المدرس |
| `IAdminDashboardService` | `AdminDashboardService` | لوحة تحكم المشرف |

### 5.8 خدمات التواصل

| الـ Interface | التطبيق | الوظيفة |
|---|---|---|
| `IMessageService` | `MessageService` | إدارة الرسائل الخاصة |
| `IAnnouncementService` | `AnnouncementService` | إدارة الإعلانات |
| `ISystemSettingService` | `SystemSettingService` | إدارة إعدادات النظام |
| `IReportService` | `ReportService` | إدارة البلاغات |
| `IActivityLogService` | `ActivityLogService` | تسجيل النشاطات |
| `INotificationService` | `NotificationService` | إدارة الإشعارات |

### 5.9 خدمات الوسائط

| الـ Interface | التطبيق | الوظيفة |
|---|---|---|
| `IMediaService` | `MediaService` | إدارة رفع وعرض الملفات |
| `IAdminMediaService` | `AdminMediaService` | إدارة الملفات للمشرف |
| `IFileService` | `FileService` | خدمة الملفات العامة |
| `IObjectStorage` | `MinioObjectStorage` | التفاعل مع MinIO |
| `IVideoProcessingService` | `VideoProcessingService` | معالجة الفيديو |

### 5.10 خدمات أخرى

| الـ Interface | التطبيق | الوظيفة |
|---|---|---|
| `ITeacherRequestService` | `TeacherRequestService` | طلبات التدريس |
| `IEmailService` | `EmailService` | إرسال البريد الإلكتروني |

---

## 6. قاعدة البيانات والنماذج (Models)

### 6.1 BaseEntity
```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; }  // Sequential GUID via MassTransit.NewId
}
```
جميع الكيانات ترث من `BaseEntity` الذي يولد GUID متسلسل (Sequential GUID) لتحسين أداء الـ Index في SQL Server.

### 6.2 نماذج المستخدمين والمصادقة

| الـ Model | الجدول | الوصف |
|---|---|---|
| `User` | `users` | يمتد `IdentityUser<Guid>` مع بيانات إضافية |
| `Role` | `roles` | يمتد `IdentityRole<Guid>` |
| `UserRole` | `user_roles` | يمتد `IdentityUserRole<Guid>` مع `AssignedAt` و `ExpiresAt` |
| `Permission` | `permissions` | صلاحيات دقيقة |
| `RolePermission` | `role_permissions` | ربط الأدوار بالصلاحيات |
| `Session` | `sessions` | جلسات المستخدمين مع Refresh Token |
| `VerificationToken` | `verification_tokens` | رموز التحقق (تأكيد البريد، إعادة تعيين كلمة المرور) |
| `Address` | `addresses` | عناوين المستخدمين |
| `UserPhone` | `user_phones` | أرقام هواتف المستخدمين |

**ملاحظات:**
- `User` يخفي `PhoneNumber` من `IdentityUser` ويستخدم `UserPhone` بدلاً منه
- `UserRole` يضيف حقلي `AssignedAt` و `ExpiresAt` على جدول الأدوار
- `VerificationToken` ليس BaseEntity (لا يستخدم GUID متسلسل)
- يتم تعطيل `ValueGeneratedNever()` لـ `User.Id` و `Role.Id` لأن GUID يولّد من التطبيق

### 6.3 نماذج الدورات

| الـ Model | الجدول | الوصف |
|---|---|---|
| `Course` | `courses` | الدورة التعليمية |
| `CourseRequirement` | `course_requirements` | متطلبات الدورة |
| `CourseLearningOutcome` | `course_learning_outcomes` | مخرجات التعلم |
| `Section` | `sections` | أقسام الدورة |
| `SectionItem` | `section_items` | عناصر القسم (بوليمورفيك) |
| `Category` | `categories` | فئات الدورات (هرمية) |
| `CourseLog` | `course_logs` | سجل تغييرات الدورة |
| `CourseEditRequest` | `course_edit_requests` | طلبات تعديل الدورة |

**ملاحظات:**
- `Course` يحتوي على `Status` (Draft, PendingReview, Published, Archived)
- `SectionItem` هو نموذج بوليمورفيك: يستخدم `ItemType` و `ItemId` للإشارة إلى أنواع محتوى مختلفة
- `Category` يدعم التسلسل الهرمي (Parent-Child) عبر `ParentCategoryId`

### 6.4 نماذج المحتوى

| الـ Model | الجدول | الوصف |
|---|---|---|
| `Video` | `videos` | محتوى فيديو مع حالة المعالجة |
| `Document` | `documents` | مستندات قابلة للتحميل |
| `Quiz` | `quizzes` | اختبارات |
| `Question` | `questions` | أسئلة الاختبارات |
| `Option` | `options` | خيارات الأسئلة |
| `QuizAttempt` | `quiz_attempts` | محاولات الاختبار |
| `UserAnswer` | `user_answers` | إجابات المستخدمين |
| `LiveSession` | `live_sessions` | جلسات مباشرة |
| `LiveAttendance` | `live_attendances` | حضور الجلسات |
| `ContentProgress` | `content_progresses` | تقدم المحتوى |

**ملاحظات:**
- `Video` له `Status` (Processing, Ready, Failed) ويدعم روابط يوتيوب وفيميو وMinIO
- `Quiz` يدعم خلط الأسئلة والخيارات، وحدود المحاولات، ونسبة النجاح
- `ContentProgress` يتتبع وقت المشاهدة ونسبة الإكمال وعدد المحاولات

### 6.5 نماذج التجارة

| الـ Model | الجدول | الوصف |
|---|---|---|
| `Cart` | `carts` | سلة المشتريات (واحد لكل مستخدم) |
| `CartItem` | `cart_items` | عناصر السلة مع سعر لحظي |
| `Order` | `orders` | الطلبات مع رقم طلب فريد |
| `OrderItem` | `order_items` | عناصر الطلب |
| `Payment` | `payments` | المدفوعات مع تتبع رقم المحاولة |
| `PaymentMethod` | `payment_methods` | طرق الدفع المتاحة |
| `Refund` | `refunds` | طلبات الاسترداد |
| `TransactionLog` | `transaction_logs` | سجل المعاملات المالية |
| `Coupon` | `coupons` | كوبونات الخصم |
| `CouponCourse` | `coupon_courses` | ربط الكوبونات بالدورات |
| `CouponUsage` | `coupon_usages` | استخدامات الكوبونات |
| `Wishlist` | `wishlists` | المفضلة |

**ملاحظات:**
- `Cart` له Index فريد على `UserId` (سلة واحدة لكل مستخدم)
- `Order` له `OrderNumber` فريد
- `Payment` له `TransactionRef` فريد و `AttemptNumber` لتتبع محاولات الدفع
- `Coupon` يدعم نوعين: Percentage و Fixed Amount

### 6.6 نماذج التقييمات والشهادات

| الـ Model | الجدول | الوصف |
|---|---|---|
| `Review` | `reviews` | مراجعات وتقييمات الدورات |
| `ReviewHelpful` | `review_helpfuls` | التصويت بمفيد للمراجعة |
| `Certificate` | `certificates` | شهادات الإتمام |

**ملاحظات:**
- `Review` له Index فريد على `(UserId, CourseId)` - مراجعة واحدة لكل مستخدم لكل دورة
- `Review` يدعم الإبلاغ (`IsFlagged`) والمراجعة (`ModeratedAt`)
- `Certificate` له `VerificationCode` فريد

### 6.7 نماذج التواصل

| الـ Model | الجدول | الوصف |
|---|---|---|
| `Message` | `messages` | الرسائل الخاصة بين المستخدمين |
| `Announcement` | `announcements` | الإعلانات (للجميع/فئة/دورة) |
| `Report` | `reports` | البلاغات عن المحتوى |
| `Notification` | `notifications` | الإشعارات |
| `ActivityLog` | `activity_logs` | سجل النشاطات |
| `SystemSetting` | `system_settings` | إعدادات النظام |

**ملاحظات:**
- `Message` يدعم الحذف للمرسل فقط والحذف للمستلم فقط (Soft Delete)
- `Announcement` يدعم الاستهداف: All, Students, Teachers, Admins, SpecificCourse
- `Report` له Index فريد على `(ReporterId, EntityType, EntityId)`
- `Report` يدعم EntityTypes: Course, Review, Comment, User, Message
- `SystemSetting` له Key فريد
- `ActivityLog` يسجل Action, EntityType, EntityId مع IP Address

### 6.8 نماذج أخرى

| الـ Model | الجدول | الوصف |
|---|---|---|
| `TeacherRequest` | `teacher_requests` | طلبات أن يصبح المستخدم مدرساً |
| `TeacherRequestDocument` | `teacher_request_documents` | وثائق طلب التدريس |
| `UploadedFile` | `files` | الملفات المرفوعة مع تتبع التخزين |
| `Enrollment` | `enrollments` | تسجيلات الطلاب في الدورات |
| `CommentLike` | `comment_likes` | الإعجابات على التعليقات |
| `VideoComment` | `video_comments` | تعليقات الفيديو |

---

## 7. المصادقة والأمان

### 7.1 JWT Authentication
- **الخوارزمية:** HMAC-SHA256
- **جهة الإصدار (Issuer):** `LMS_Backend`
- **الجمهور (Audience):** `LMS_Frontend`
- **مدة صلاحية Access Token:** 60 دقيقة
- **مدة صلاحية Refresh Token:** 7 أيام
- **معلمة ClockSkew:** صفر (لا تسامح في الوقت)

### 7.2 Identity Configuration
- **كلمة المرور:** تتطلب حرف كبير، صغير، رقم، رمز خاص، 8 أحرف على الأقل
- **Lockout:** 5 محاولات فاشلة، قفل 15 دقيقة
- **البريد الإلكتروني:** يجب أن يكون فريداً
- **تأكيد البريد:** غير مطلوب حالياً (يُفعل في الإنتاج)

### 7.3 CORS Policy
- يسمح بأي Origin يبدأ بـ: `localhost`, `192.168.`, `10.`, `172.`
- يسمح بأي Header وأي Method مع `AllowCredentials`

### 7.4 Rate Limiting
- **Messaging:** 30 رسالة/دقيقة
- **CertificateVerification:** 20 طلب/دقيقة
- **رمز الرفض:** 429 Too Many Requests

### 7.5 OAuth
- Google OAuth: مشروط بتوفير ClientId
- Microsoft OAuth: مشروط بتوفير ClientId
- يتم تفعيلهما ديناميكياً في `Program.cs`

---

## 8. الـ DTOs

### 8.1 ApiResponse (النمط الموحد للاستجابات)
```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }
}
```
جميع الـ Controllers ترجع `ApiResponse<T>` لتوحيد شكل الاستجابة.

### 8.2 تنظيم الـ DTOs
الـ DTOs منظمة في مجلدات حسب الميزة:
- `DTOs/Auth/`: 13 ملف DTO للمصادقة
- `DTOs/Cart/`: 4 ملفات
- `DTOs/Category/`: 4 ملفات
- `DTOs/Certificate/`: 4 ملفات
- `DTOs/Communication/`: 5 ملفات للتواصل
- `DTOs/Course/`: ملفان رئيسيان
- `DTOs/Dashboard/`: 3 ملفات للوحات التحكم
- `DTOs/Enrollment/`: 3 ملفات
- `DTOs/Media/`: 4 ملفات لإدارة الوسائط
- `DTOs/Notifications/`: 4 ملفات
- `DTOs/Order/`: ملف واحد
- `DTOs/Payment/`: ملف واحد
- `DTOs/Profile/`: ملفان
- `DTOs/Quiz/`: ملفان
- `DTOs/QuizAttempt/`: ملف واحد
- `DTOs/Review/`: ملف واحد
- `DTOs/Section/`: ملفان
- `DTOs/TeacherRequests/`: مجلدان (Requests/Responses)
- `DTOs/Video/`: 3 ملفات
- `DTOs/VideoComment/`: ملف واحد
- `DTOs/Wishlist/`: ملف واحد

---

## 9. الـ Validators

تستخدم FluentValidation مع تسجيل تلقائي لكل الـ Validators في الـ Assembly.

### قائمة الـ Validators:

| الـ Validator | الوظيفة |
|---|---|
| `AddToCartValidator` | التحقق من صحة إضافة للسلة |
| `ApplyCouponRequestValidator` | التحقق من طلب تطبيق كوبون |
| `CreateCouponValidator` | التحقق من صحة إنشاء كوبون |
| `ApplyCouponValidator` | التحقق من صحة تطبيق كوبون |
| `CreateOrderValidator` | التحقق من صحة إنشاء طلب |
| `CreatePaymentMethodValidator` | التحقق من صحة إنشاء طريقة دفع |
| `ProcessPaymentValidator` | التحقق من صحة معالجة الدفع |
| `ProcessRefundValidator` | التحقق من صحة معالجة استرداد |
| `RequestRefundValidator` | التحقق من صحة طلب استرداد |
| `CreateReviewValidator` | التحقق من صحة إنشاء مراجعة |
| `CategoryValidator` | التحقق من صحة الفئة |
| `CourseValidator` | التحقق من صحة الدورة |
| `CreateCommentValidator` | التحقق من صحة إنشاء تعليق |
| `CreateEnrollmentValidator` | التحقق من صحة إنشاء تسجيل |
| `CreateLiveSessionValidator` | التحقق من صحة إنشاء جلسة مباشرة |
| `CreateQuestionValidator` | التحقق من صحة إنشاء سؤال |
| `CreateQuizValidator` | التحقق من صحة إنشاء اختبار |
| `CreateVideoValidator` | التحقق من صحة إنشاء فيديو |
| `SectionValidator` | التحقق من صحة القسم |
| `SubmitQuizAttemptValidator` | التحقق من صحة تقديم محاولة اختبار |
| `SubmitTeacherRequestValidator` | التحقق من صحة تقديم طلب تدريس |
| `ProcessTeacherRequestValidator` | التحقق من صحة معالجة طلب تدريس |
| `UpdateTeacherRequestValidator` | التحقق من صحة تحديث طلب تدريس |
| `AddDocumentToRequestValidator` | التحقق من صحة إضافة وثيقة للطلب |

---

## 10. الـ Middleware

### ExceptionMiddleware
**الملف:** `Middlewares/ExceptionMiddleware.cs`

يقبض على كل الاستثناءات غير المعالجة ويعيد استجابة JSON موحدة.

| نوع الاستثناء | رمز HTTP | الرسالة |
|---|---|---|
| `KeyNotFoundException` | 404 Not Found | رسالة الاستثناء |
| `UnauthorizedAccessException` | 403 Forbidden | رسالة الاستثناء |
| `InvalidOperationException` | 400 Bad Request | رسالة الاستثناء |
| أي استثناء آخر | 500 Internal Server Error | رسالة عامة |

---

## 11. التكوين (Configuration)

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=LMS_Database;..."
  },
  "JwtSettings": {
    "SecretKey": "...",      // 32+ حرف
    "Issuer": "LMS_Backend",
    "Audience": "LMS_Frontend",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "Username": "codeprofs253@gmail.com",
    "Password": "...",
    "FromEmail": "codeprofs253@gmail.com",
    "FromName": "منصة آثاري التعليمية",
    "IsEnabled": true
  },
  "VerificationSettings": {
    "OtpExpirationMinutes": 15,
    "OtpLength": 6
  },
  "MinioSettings": {
    "Endpoint": "localhost:8002",
    "AccessKey": "minioadmin",
    "SecretKey": "minioadmin",
    "UseSSL": false,
    "PresignedUrlExpiryMinutes": 60,
    "Buckets": ["videos", "documents", "images", "recordings", "certificates", "private"]
  },
  "EditPolicy": {
    "RequireApprovalForPublished": true,
    "SensitivePriceChangePercent": 10,
    "AutoExpirePendingDays": 7,
    "EnableEmergencyBypass": false
  },
  "BackgroundServices": {
    "CleanupExpiredRequests": {
      "Enabled": true,
      "IntervalHours": 24
    }
  }
}
```

### فئات الإعدادات:
- `JwtSettings.cs`: إعدادات JWT
- `EmailSettings.cs`: إعدادات SMTP
- `MinioSettings.cs`: إعدادات MinIO

---

## 12. الـ Hubs (SignalR)

### NotificationHub
**المسار:** `Hubs/NotificationHub.cs`  
**الراوت:** `/hubs/notifications`

- يرسل الإشعارات في الوقت الفعلي للمستخدمين
- المستخدمون ينضمون إلى Group خاص بهم عند الاتصال

### MessageHub
**المسار:** `Hubs/MessageHub.cs`  
**الراوت:** `/hubs/messages`

- يرسل الرسائل في الوقت الفعلي
- يدعم الإرسال المباشر بين المستخدمين

---

## 13. الـ Background Workers

### VideoProcessingWorker
**المسار:** `Workers/VideoProcessingWorker.cs`

- يعمل كـ Background Service (Hosted Service)
- يستقصي قاعدة البيانات بحثاً عن فيديوهات بحالة Processing
- يعالجها (يغير الحالة إلى Ready)
- يستخدم استقصاء متكيف: 30 ثانية عادي، دقيقتين بعد 5 جولات فارغة

### EditRequestCleanupService
**المسار:** `services/Background/EditRequestCleanupService.cs`

- ينظف طلبات التعديل منتهية الصلاحية
- يعمل كل 24 ساعة (قابل للتكوين)

### ScheduledDeletionService
**المسار:** `services/Background/ScheduledDeletionService.cs`

- ينفذ الحذف المجدول للدورات
- يحول الدورات إلى وضع القراءة فقط `IsReadOnlyForStudents = true`

---

## 14. قائمة شاملة بكل API Endpoint

### المصادقة (Auth) - 10 Endpoints
| # | الطريقة | المسار | الوصف |
|---|---|---|---|
| 1 | POST | `/api/auth/register` | تسجيل مستخدم جديد |
| 2 | POST | `/api/auth/login` | تسجيل الدخول |
| 3 | POST | `/api/auth/refresh` | تجديد التوكن |
| 4 | POST | `/api/auth/verify-email` | تأكيد البريد الإلكتروني |
| 5 | POST | `/api/auth/resend-verification` | إعادة إرسال رمز التفعيل |
| 6 | POST | `/api/auth/forgot-password` | نسيان كلمة المرور |
| 7 | POST | `/api/auth/reset-password` | إعادة تعيين كلمة المرور |
| 8 | POST | `/api/auth/logout` | تسجيل الخروج |
| 9 | POST | `/api/auth/logout-all` | تسجيل الخروج من الكل |
| 10 | GET | `/api/auth/sessions` | الجلسات النشطة |

### OAuth - 2 Endpoints
| 11 | POST | `/api/oauth/google` | تسجيل الدخول عبر Google |
| 12 | POST | `/api/oauth/microsoft` | تسجيل الدخول عبر Microsoft |

### الملف الشخصي (Profile) - 12 Endpoints
| 13 | GET | `/api/profile/me` | عرض الملف الشخصي |
| 14 | GET | `/api/profile/{userId}` | عرض ملف مستخدم آخر |
| 15 | PUT | `/api/profile` | تحديث الملف الشخصي |
| 16 | POST | `/api/profile/picture` | تعيين صورة الملف |
| 17 | DELETE | `/api/profile/picture` | حذف صورة الملف |
| 18 | POST | `/api/profile/phones` | إضافة هاتف |
| 19 | DELETE | `/api/profile/phones/{phoneId}` | حذف هاتف |
| 20 | PUT | `/api/profile/phones/{phoneId}/default` | تعيين هاتف افتراضي |
| 21 | POST | `/api/profile/addresses` | إضافة عنوان |
| 22 | PUT | `/api/profile/addresses/{addressId}` | تحديث عنوان |
| 23 | DELETE | `/api/profile/addresses/{addressId}` | حذف عنوان |
| 24 | PUT | `/api/profile/addresses/{addressId}/default` | تعيين عنوان افتراضي |

### الدورات (Course Management) - 13 Endpoints
| 25 | POST | `/api/management/courses` | إنشاء دورة |
| 26 | GET | `/api/management/courses/{id}` | عرض دورة |
| 27 | PUT | `/api/management/courses/{id}` | تحديث دورة |
| 28 | POST | `/api/management/courses/{id}/requirements` | إضافة متطلبات |
| 29 | DELETE | `/api/management/courses/{id}/requirements/{reqId}` | حذف متطلب |
| 30 | POST | `/api/management/courses/{id}/outcomes` | إضافة مخرجات |
| 31 | DELETE | `/api/management/courses/{id}/outcomes/{outcomeId}` | حذف مخرج |
| 32 | POST | `/api/management/courses/{id}/submit-for-review` | تقديم للمراجعة |
| 33 | DELETE | `/api/management/courses/{id}` | حذف دورة |
| 34 | POST | `/api/management/courses/{id}/schedule-deletion` | جدولة حذف |
| 35 | POST | `/api/management/courses/{id}/cancel-scheduled-deletion` | إلغاء جدولة حذف |
| 36 | GET | `/api/management/courses/{id}/deletion-status` | حالة الحذف المجدول |
| 37 | PUT | `/api/management/courses/{courseId}/image` | تعيين صورة الدورة |

### إدارة الدورات (Listing) - 1 Endpoint
| 38 | GET | `/api/management/courses` | قائمة دورات المدرس |

### الأقسام (Sections) - 12 Endpoints
| 39 | GET | `/api/management/courses/{courseId}/sections` | عرض الأقسام |
| 40 | POST | `/api/management/courses/{courseId}/sections` | إنشاء قسم |
| 41 | GET | `/api/management/courses/{courseId}/sections/{sectionId}` | عرض قسم |
| 42 | PUT | `/api/management/courses/{courseId}/sections/{sectionId}` | تحديث قسم |
| 43 | DELETE | `/api/management/courses/{courseId}/sections/{sectionId}` | حذف قسم |
| 44 | PUT | `/api/management/courses/{courseId}/sections/reorder` | إعادة ترتيب أقسام |
| 45 | POST | `/api/management/courses/{courseId}/sections/{sectionId}/items` | إضافة عنصر |
| 46 | PUT | `/api/management/courses/{courseId}/sections/{sectionId}/items/{itemId}` | تحديث عنصر |
| 47 | DELETE | `/api/management/courses/{courseId}/sections/{sectionId}/items/{itemId}` | حذف عنصر |
| 48 | PUT | `/api/management/courses/{sectionId}/items/reorder` | إعادة ترتيب عناصر |
| 49 | GET | `/api/courses/my-edit-requests` | طلبات التعديل الخاصة بي |
| 50 | POST | `/api/courses/edit-requests/{requestId}/cancel` | إلغاء طلب تعديل |

### الدورات العامة (Public) - 7 Endpoints
| 51 | GET | `/api/public/courses` | عرض الدورات المنشورة |
| 52 | GET | `/api/public/courses/{id}` | تفاصيل دورة |
| 53 | GET | `/api/public/courses/slug/{slug}` | دورة بالـ Slug |
| 54 | GET | `/api/public/courses/search/suggest` | اقتراحات بحث |
| 55 | GET | `/api/public/courses/stats` | إحصائيات المنصة |
| 56 | GET | `/api/public/courses/{id}/related` | دورات مشابهة |
| 57 | GET | `/api/public/courses/filters/options` | خيارات الفلترة |

### الفئات (Categories) - 6 Endpoints
| 58 | GET | `/api/categories` | عرض الفئات |
| 59 | GET | `/api/categories/{id}` | عرض فئة |
| 60 | POST | `/api/categories` | إنشاء فئة |
| 61 | PUT | `/api/categories/{id}` | تحديث فئة |
| 62 | DELETE | `/api/categories/{id}` | حذف فئة |
| 63 | PUT | `/api/categories/{categoryId}/image` | صورة الفئة |

### المحتوى (Content) - 18 Endpoints
| 64 | GET | `/api/courses/{courseId}/videos/{id}` | عرض فيديو |
| 65 | POST | `/api/courses/{courseId}/videos` | إنشاء فيديو |
| 66 | PUT | `/api/courses/{courseId}/videos/{id}` | تحديث فيديو |
| 67 | DELETE | `/api/courses/{courseId}/videos/{id}` | حذف فيديو |
| 68 | GET | `/api/courses/{courseId}/documents/{id}` | عرض مستند |
| 69 | POST | `/api/courses/{courseId}/documents` | إنشاء مستند |
| 70 | PUT | `/api/courses/{courseId}/documents/{id}` | تحديث مستند |
| 71 | DELETE | `/api/courses/{courseId}/documents/{id}` | حذف مستند |
| 72 | GET | `/api/courses/{courseId}/quizzes/{id}` | عرض اختبار |
| 73 | POST | `/api/courses/{courseId}/quizzes` | إنشاء اختبار |
| 74 | PUT | `/api/courses/{courseId}/quizzes/{id}` | تحديث اختبار |
| 75 | DELETE | `/api/courses/{courseId}/quizzes/{id}` | حذف اختبار |
| 76 | POST | `/api/courses/{courseId}/quizzes/{quizId}/questions` | إضافة سؤال |
| 77 | PUT | `/api/courses/{courseId}/quizzes/{quizId}/questions/{questionId}` | تحديث سؤال |
| 78 | DELETE | `/api/courses/{courseId}/quizzes/{quizId}/questions/{questionId}` | حذف سؤال |
| 79 | GET | `/api/courses/{courseId}/live-sessions` | عرض جلسات مباشرة |
| 80 | POST | `/api/courses/{courseId}/live-sessions` | إنشاء جلسة مباشرة |
| 81 | PUT | `/api/courses/{courseId}/live-sessions/{sessionId}/status` | تحديث حالة جلسة |
| 82 | DELETE | `/api/courses/{courseId}/live-sessions/{sessionId}` | حذف جلسة |

### حضور الجلسات (Live Attendance) - 3 Endpoints
| 83 | POST | `/api/live-sessions/{sessionId}/attendance/join` | انضمام لجلسة |
| 84 | POST | `/api/live-sessions/{sessionId}/attendance/leave` | مغادرة جلسة |
| 85 | GET | `/api/live-sessions/{sessionId}/attendance/count` | عدد الحضور |

### تعليقات الفيديو (Video Comments) - 5 Endpoints
| 86 | GET | `/api/videos/{videoId}/comments` | عرض التعليقات |
| 87 | POST | `/api/videos/{videoId}/comments` | إضافة تعليق |
| 88 | PUT | `/api/videos/{videoId}/comments/{commentId}` | تحديث تعليق |
| 89 | DELETE | `/api/videos/{videoId}/comments/{commentId}` | حذف تعليق |
| 90 | POST | `/api/videos/{videoId}/comments/{commentId}/like` | إعجاب بتعليق |

### التسجيلات (Enrollments) - 6 Endpoints
| 91 | POST | `/api/enrollments` | إنشاء تسجيل |
| 92 | GET | `/api/enrollments` | تسجيلاتي |
| 93 | GET | `/api/enrollments/{id}` | تفاصيل تسجيل |
| 94 | GET | `/api/enrollments/{enrollmentId}/progress` | عرض التقدم |
| 95 | PUT | `/api/enrollments/{enrollmentId}/progress` | تحديث التقدم |
| 96 | POST | `/api/enrollments/{enrollmentId}/progress/{contentType}/{contentId}/complete` | إكمال محتوى |

### الاختبارات (Quiz Attempts) - 4 Endpoints
| 97 | POST | `/api/enrollments/{enrollmentId}/quizzes/{quizId}/attempts` | بدء محاولة |
| 98 | PUT | `/api/enrollments/{enrollmentId}/quizzes/{quizId}/attempts/{attemptId}` | تقديم محاولة |
| 99 | GET | `/api/enrollments/{enrollmentId}/quizzes/{quizId}/attempts` | عرض المحاولات |
| 100 | GET | `/api/enrollments/{enrollmentId}/quizzes/{quizId}/attempts/{attemptId}` | تفاصيل محاولة |

### السلة (Cart) - 5 Endpoints
| 101 | GET | `/api/cart` | عرض السلة |
| 102 | POST | `/api/cart/items` | إضافة للسلة |
| 103 | DELETE | `/api/cart/items/{itemId}` | حذف من السلة |
| 104 | POST | `/api/cart/coupon` | تطبيق كوبون |
| 105 | DELETE | `/api/cart/coupon` | إزالة كوبون |

### الطلبات (Orders) - 5 Endpoints
| 106 | POST | `/api/orders` | إنشاء طلب |
| 107 | GET | `/api/orders` | عرض الطلبات |
| 108 | GET | `/api/orders/{id}` | تفاصيل طلب |
| 109 | POST | `/api/orders/{orderId}/payments` | دفع طلب |
| 110 | GET | `/api/orders/{orderId}/payments` | مدفوعات الطلب |

### المدفوعات (Payments) - 2 Endpoints
| 111 | GET | `/api/payment-methods` | طرق الدفع |
| 112 | POST | `/api/coupons/validate` | التحقق من كوبون |

### الاسترداد (Refunds) - 2 Endpoints
| 113 | POST | `/api/refunds` | طلب استرداد |
| 114 | GET | `/api/refunds` | طلبات الاسترداد |

### المفضلة (Wishlist) - 3 Endpoints
| 115 | GET | `/api/wishlist` | عرض المفضلة |
| 116 | POST | `/api/wishlist/{courseId}` | إضافة للمفضلة |
| 117 | DELETE | `/api/wishlist/{courseId}` | حذف من المفضلة |

### المراجعات (Reviews) - 9 Endpoints
| 118 | POST | `/api/reviews` | إضافة مراجعة |
| 119 | GET | `/api/reviews/course/{courseId}` | مراجعات دورة |
| 120 | GET | `/api/reviews/{id}` | عرض مراجعة |
| 121 | PUT | `/api/reviews/{id}` | تحديث مراجعة |
| 122 | DELETE | `/api/reviews/{id}` | حذف مراجعة |
| 123 | POST | `/api/reviews/{id}/helpful` | تصويت مفيد |
| 124 | POST | `/api/reviews/{id}/flag` | الإبلاغ عن مراجعة |
| 125 | GET | `/api/reviews/pending` | مراجعات معلقة |
| 126 | PUT | `/api/reviews/{id}/moderate` | مراجعة معتدلة |

### الشهادات (Certificates) - 5 Endpoints
| 127 | GET | `/api/certificates/my` | شهاداتي |
| 128 | GET | `/api/certificates/{id}` | عرض شهادة |
| 129 | GET | `/api/certificates/{id}/download` | تحميل شهادة |
| 130 | GET | `/api/certificates/verify/{code}` | التحقق من شهادة |
| 131 | POST | `/api/admin/certificates` | عرض كل الشهادات |
| 132 | POST | `/api/admin/certificates/{id}/revoke` | إلغاء شهادة |
| 133 | POST | `/api/admin/certificates/issue` | إصدار شهادة |

### لوحات التحكم (Dashboards) - 6 Endpoints
| 134 | GET | `/api/dashboard/student` | لوحة الطالب |
| 135 | GET | `/api/dashboard/instructor` | لوحة المدرس |
| 136 | GET | `/api/admin/dashboard/overview` | نظرة عامة مشرف |
| 137 | GET | `/api/admin/dashboard/revenue` | تقرير الإيرادات |
| 138 | GET | `/api/admin/dashboard/user-growth` | نمو المستخدمين |
| 139 | GET | `/api/admin/dashboard/enrollment-trends` | اتجاهات التسجيل |

### التواصل (Communication) - 20 Endpoints
| 140 | POST | `/api/messages` | إرسال رسالة |
| 141 | GET | `/api/messages/conversations` | المحادثات |
| 142 | GET | `/api/messages/conversations/{otherUserId}` | رسائل محادثة |
| 143 | GET | `/api/messages/unread-count` | رسائل غير مقروءة |
| 144 | PATCH | `/api/messages/{messageId}/read` | تعيين رسالة مقروءة |
| 145 | DELETE | `/api/messages/{messageId}` | حذف رسالة |
| 146 | POST | `/api/announcements` | إنشاء إعلان |
| 147 | POST | `/api/announcements/course/{courseId}` | إعلان لدورة |
| 148 | GET | `/api/announcements` | عرض الإعلانات |
| 149 | PUT | `/api/announcements/{id}` | تحديث إعلان |
| 150 | PATCH | `/api/announcements/{id}/deactivate` | تعطيل إعلان |
| 151 | DELETE | `/api/announcements/{id}` | حذف إعلان |
| 152 | GET | `/api/system-settings` | إعدادات النظام |
| 153 | GET | `/api/system-settings/{key}` | إعداد معين |
| 154 | PUT | `/api/system-settings/{key}` | تحديث إعداد |
| 155 | POST | `/api/system-settings` | إنشاء إعداد |
| 156 | DELETE | `/api/system-settings/{key}` | حذف إعداد |
| 157 | GET | `/api/activity-logs` | سجل النشاطات |
| 158 | POST | `/api/reports` | الإبلاغ عن محتوى |
| 159 | GET | `/api/reports/pending` | بلاغات معلقة |
| 160 | PATCH | `/api/reports/{id}/resolve` | حل بلاغ |

### الإشعارات (Notifications) - 6 Endpoints
| 161 | GET | `/api/notifications` | عرض الإشعارات |
| 162 | GET | `/api/notifications/unread-count` | إشعارات غير مقروءة |
| 163 | PATCH | `/api/notifications/{notificationId}/read` | تعيين مقروء |
| 164 | POST | `/api/notifications/mark-all-read` | تعيين الكل مقروء |
| 165 | DELETE | `/api/notifications/{notificationId}` | حذف إشعار |
| 166 | DELETE | `/api/notifications/clear-all` | حذف الكل |

### الوسائط (Media) - 9 Endpoints
| 167 | POST | `/api/media/upload-url` | رابط رفع |
| 168 | POST | `/api/media/confirm-upload` | تأكيد رفع |
| 169 | GET | `/api/media/{fileId}/view` | عرض ملف |
| 170 | GET | `/api/admin/media` | كل الملفات |
| 171 | GET | `/api/admin/media/{fileId}` | تفاصيل ملف |
| 172 | DELETE | `/api/admin/media/{fileId}` | حذف ناعم |
| 173 | POST | `/api/admin/media/{fileId}/restore` | استعادة ملف |
| 174 | DELETE | `/api/admin/media/{fileId}/permanent` | حذف دائم |
| 175 | GET | `/api/admin/media/stats` | إحصائيات التخزين |

### طلبات التدريس (Teacher Requests) - 11 Endpoints
| 176 | GET | `/api/teacher-requests/can-submit` | هل يمكن التقديم |
| 177 | POST | `/api/teacher-requests` | تقديم طلب |
| 178 | PUT | `/api/teacher-requests/{requestId}` | تحديث طلب |
| 179 | POST | `/api/teacher-requests/{requestId}/documents` | إضافة وثيقة |
| 180 | GET | `/api/teacher-requests/my-requests` | طلباتي |
| 181 | GET | `/api/teacher-requests/my-requests/{requestId}` | تفاصيل طلبي |
| 182 | DELETE | `/api/teacher-requests/{requestId}/cancel` | إلغاء طلب |
| 183 | GET | `/api/teacher-requests/pending` | طلبات معلقة |
| 184 | GET | `/api/teacher-requests/{requestId}` | تفاصيل طلب (مشرف) |
| 185 | PUT | `/api/teacher-requests/{requestId}/process` | معالجة طلب |
| 186 | DELETE | `/api/teacher-requests/{requestId}` | حذف طلب (مشرف) |

### إدارة المشرف (Admin Management) - 9 Endpoints
| 187 | GET | `/api/admin/courses/pending` | دورات معلقة |
| 188 | POST | `/api/admin/courses/{id}/approve` | الموافقة على دورة |
| 189 | POST | `/api/admin/courses/{id}/reject` | رفض دورة |
| 190 | GET | `/api/admin/courses/edit-requests` | طلبات تعديل |
| 191 | GET | `/api/admin/courses/edit-requests/{requestId}` | تفاصيل طلب تعديل |
| 192 | POST | `/api/admin/courses/edit-requests/{requestId}/review` | مراجعة طلب تعديل |
| 193 | GET | `/api/admin/coupons` | الكوبونات |
| 194 | GET | `/api/admin/coupons/{id}` | كوبون |
| 195 | POST | `/api/admin/coupons` | إنشاء كوبون |
| 196 | PUT | `/api/admin/coupons/{id}` | تحديث كوبون |
| 197 | PATCH | `/api/admin/coupons/{id}/toggle` | تفعيل/تعطيل كوبون |
| 198 | DELETE | `/api/admin/coupons/{id}` | حذف كوبون |
| 199 | GET | `/api/admin/payment-methods` | طرق الدفع |
| 200 | POST | `/api/admin/payment-methods` | إضافة طريقة دفع |
| 201 | PATCH | `/api/admin/payment-methods/{id}/toggle` | تفعيل/تعطيل طريقة دفع |
| 202 | GET | `/api/admin/refunds` | طلبات استرداد |
| 203 | POST | `/api/admin/refunds/{id}/approve` | موافقة استرداد |
| 204 | POST | `/api/admin/refunds/{id}/reject` | رفض استرداد |

### SignalR Hubs (اتصال فوري)
| 205 | - | `/hubs/notifications` | إشعارات فورية |
| 206 | - | `/hubs/messages` | رسائل فورية |

---

## 15. التقييم العام

### نقاط القوة
1. **هيكل معماري نظيف**: الفصل بين الـ Controllers والـ Services والـ Data Access
2. **استخدام نمط Interface/Implementation**: يسهل الاختبار والاستبدال
3. **تغطية واسعة للميزات**: من المصادقة إلى التجارة إلى التواصل
4. **توحيد شكل الاستجابات**: عبر `ApiResponse<T>`
5. **معالجة مركزية للأخطاء**: عبر `ExceptionMiddleware`
6. **تسجيل تلقائي للخدمات**: عبر `MediaServiceExtensions` و `AddValidatorsFromAssembly`
7. **استخدام GUID متسلسل**: لتحسين أداء قاعدة البيانات
8. **دعم OAuth**: Google و Microsoft
9. **SignalR**: للرسائل والإشعارات الفورية
10. **FluentValidation**: التحقق من صحة البيانات
11. **Rate Limiting**: للحماية من سوء الاستخدام
12. **Soft Delete**: في عدة كيانات (User, Category, VideoComment, UploadedFile)

### نقاط الضعف / ملاحظات
1. **مجلدات مكررة**: يوجد `services/` و `Services/` - يجب توحيدهما
2. **أسماء الملفات في appsettings**: بعض الإعدادات الحساسة (بريد إلكتروني، MinIO) موجودة في ملف الإعدادات
3. **كلمة سر JWT**: `YourSuperSecretKeyThatShouldBeAtLeast32CharactersLongForHS256Algorithm` - يجب تغييرها
4. **بيانات MinIO الافتراضية**: `minioadmin/minioadmin` - يجب تغييرها
5. **كلمة مرور البريد الإلكتروني**: مكتوبة في الملف - يجب استخدام متغيرات البيئة أو Azure Key Vault
6. **نقص بعض الاختبارات**: يوجد اختبارات للتجارة فقط (CommerceTests)، باقي الميزات بدون اختبارات
7. **بعض الخدمات بدون Interface**: مثل `EditRequestCleanupService` و `ScheduledDeletionService`
8. **عدم استخدام متغيرات البيئة**: الإعدادات الحساسة يجب أن تكون في Environment Variables أو User Secrets
9. **قاعدة بيانات التطوير والإنتاج واحدة**: يجب فصل Connection Strings للتطوير والإنتاج
10. **VideoProcessingWorker** يستخدم استقصاء (Polling) بدلاً من الأحداث أو message queue
11. **نقص الـ XML Documentation**: معظم الكود بدون تعليقات توثيق XML

### التوصيات
1. استخدام `dotnet user-secrets` للإعدادات الحساسة في التطوير
2. استخدام Azure Key Vault أو HashiCorp Vault في الإنتاج
3. إضافة اختبارات وحدة للميزات المتبقية
4. توحيد المجلدات المكررة (services/Services)
5. إضافة Swagger UI بشكل كامل (بدلاً من NSwag فقط)
6. إضافة Serilog للتسجيل المتقدم
7. استخدام MediatR لـ CQRS إذا كان المشروع سيستمر في النمو
8. إضافة Health Checks لمراقبة صحة الخدمات
9. استخدام Docker Compose لتنسيق الخدمات (SQL Server, MinIO, API)

---

> **إجمالي الـ Endpoints: 206**
> - REST API Endpoints: 204
> - SignalR Hubs: 2
> - صفحات عامة: 15
> - صفحات تحتاج مصادقة: 98
> - صفحات للمشرف فقط: ~50
> - صفحات للمدرس فقط: ~30
