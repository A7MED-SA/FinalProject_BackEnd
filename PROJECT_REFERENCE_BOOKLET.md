# 📘 كتيب المرجع الشامل لمشروع LMS Backend

## 📋 مقدمة
هذا الكتيب يعتبر مرجعاً شاملاً (documentation) لكل ما تم إنجازه في مشروع **Learning Management System (LMS) Backend**. يغطي الكتيب الهيكلية التقنية، الميزات التي تم برمجتها بالكامل، تصميم قاعدة البيانات، وكيفية تشغيل النظام.

---

## 🏗️ 1. البنية التقنية (Architecture & Tech Stack)

تم بناء المشروع باستخدام أحدث تقنيات Microsoft لضمان الأداء العالي والقابلية للتوسع:

- **الإطار البرمجي (Framework):** ASP.NET Core 9.0
- **قاعدة البيانات (Database):** SQL Server
- **تقنية التعامل مع البيانات (ORM):** Entity Framework Core 9.0.10
- **لغة البرمجة:** C# 12
- **نمط التصميم (Architecture Pattern):**
  - **Clean Architecture / N-Tier:** تقسيم المشروع إلى طبقات (Controllers, Services, Repositories, Models).
  - **Code-First Approach:** بناء قاعدة البيانات انطلاقاً من الكود.
  - **Repository Pattern:** (جاهز للتطبيق) لفصل منطق البيانات عن منطق العمل.

---

## ✅ 2. ما تم إنجازه وبرمجته (Implemented Core)

الأجزاء التالية تم كتابة الكود الخاص بها وهي **تعمل بالكامل**:

### 🔐 أ. نظام الحماية والتوثيق (Authentication & Authorization)
هو العمود الفقري للنظام، وتم بناؤه باستخدام **ASP.NET Core Identity** مع **JWT (JSON Web Tokens)**.

*   **المكونات:**
    *   `AuthController`: للتحكم في تسجيل الدخول/الخروج.
    *   `AuthenticationService` & `TokenService`: لإدارة إصدار والتحقق من التوكنز.
    *   `SessionService`: لإدارة جلسات المستخدمين (Refresh Tokens).
*   **المميزات:**
    *   **تسجيل الدخول/التسجيل (Login/Register):** دعم كامل للبريد الإلكتروني وكلمة المرور.
    *   **Roles & Permissions:** أدوار افتراضية (Admin, Teacher, Student) وصلاحيات دقيقة.
    *   **Social Login:** هيكلية جاهزة لدعم Google و Microsoft Login (`OAuthService`).
    *   **سياسات الأمان:** تشفير كلمات المرور، قفل الحساب بعد محاولات فاشلة، والتحقق من صحة الإيميل.

### 👤 ب. إدارة الملف الشخصي (User Profile)
*   **المكونات:** `ProfileController`, `ProfileService`.
*   **المميزات:**
    *   عرض وتحديث بيانات المستخدم.
    *   إدارة صور الملف الشخصي.
    *   تغيير كلمة المرور.

### 📼 ج. إدارة الوسائط والملفات (Media Management)
*   **المكونات:** `MediaController`, `AdminMediaController`, `MediaService`.
*   **المميزات:**
    *   رفع الملفات (صور، فيديوهات، مستندات).
    *   دعم التخزين المحلي أو السحابي (MinIO/S3 Integration عبر `MinioObjectStorage`).
    *   معالجة الفيديوهات (`VideoProcessingService`) - *جاهزية للهيكلية*.

### 📧 د. الخدمات المساعدة (Infrastructure Services)
*   **EmailService:** لإرسال إيميلات التفعيل واستعادة كلمة المرور.
*   **ActivityLogService:** لتسجيل تحركات المستخدمين للأمان والمراقبة.
*   **FileService:** للتعامل مع ملفات النظام.

---

## 🗄️ 3. تصميم قاعدة البيانات (Database Schema)

تم تحويل مخططات ERD بالكامل إلى **56 Data Model** جاهزة في C#. تغطي هذه الجداول كافة جوانب النظام:

### 1️⃣ المستخدمين والصلاحيات (Users & Roles)
*   `User`: الجدول الأساسي للمستخدم.
*   `Role`, `Permission`: جداول الصلاحيات.
*   `UserRole`, `RolePermission`: جداول الربط.
*   `Session`, `UserPhone`, `Address`: بيانات إضافية.

### 2️⃣ المحتوى التعليمي (Courses & Content)
*   `Course`: الكورسات الأساسية.
*   `Category`: تصنيفات الكورسات (شجري/Nested).
*   `Section`, `SectionItem`: تقسيم الكورس لأقسام ومحتويات.
*   `Video`, `Document`, `Quiz`, `LiveSession`: أنواع المحتوى المختلفة.

### 3️⃣ التجارة والطلبات (Commerce)
*   `Cart`, `CartItem`: سلة المشتريات.
*   `Order`, `OrderItem`: الطلبات المكتملة.
*   `Payment`, `Transaction`: عمليات الدفع.
*   `Coupon`, `Discount`: أنظمة الخصومات.

### 4️⃣ التفاعل (Interaction)
*   `Review`: تقييمات الكورسات.
*   `Comment`, `Reply`: التعليقات على الفيديوهات.
*   `Message`: نظام المراسلة الداخلي (Chat).
*   `Notification`: الإشعارات.

---

## 🚀 4. كيفية العمل (How It Works)

### دورة حياة الطلب (Request Layout):
عندما يرسل الـ Frontend طلباً (مثلاً: تسجيل دخول):

1.  **Controller (`AuthController`):** يستقبل الطلب (`LoginDto`).
2.  **Validation:** يتم التأكد من صحة البيانات.
3.  **Service (`AuthenticationService`):**
    *   يتحقق من المستخدم في قاعدة البيانات (`UserManager`).
    *   يتحقق من كلمة المرور.
    *   يطلب من `TokenService` إنشاء `JWT Access Token`.
    *   يطلب من `SessionService` إنشاء `Refresh Token`.
4.  **Response:** يعود الرد إلى المستخدم مع التوكنز ليدخل في الـ Header للطلبات التالية.

### إدارة الملفات:
عند رفع صورة كورس:
1.  **Controller (`MediaController`):** يستقبل الملف (`IFormFile`).
2.  **Service (`MediaService`):**
    *   يفحص نوع وحجم الملف.
    *   يحفظ الملف (محلياً أو على MinIO S3).
    *   ينشئ سجلاً في جدول `UploadedFile`.
3.  **Database:** يتم حفظ مسار الملف (`FilePath`) وربطه بالكورس.

---

## 🛠️ 5. دليل التشغيل السريع (Setup Guide)

لتشغيل المشروع على جهازك:

1.  **تأكد من تنصيب:** .NET SDK 9.0 و SQL Server.
2.  **اضبط الاتصال:** افتح `appsettings.json` وعدّل `DefaultConnection` ليتناسب مع جهازك.
3.  **إنشاء قاعدة البيانات:**
    ```bash
    dotnet ef migrations add InitialCreate
    dotnet ef database update
    ```
4.  **تشغيل السيرفر:**
    ```bash
    dotnet run
    ```
5.  **التجربة:** افتح المتصفح على `http://localhost:5000/swagger` لرؤية الـ API Documentation.

---

## 🔮 6. ما يحتاج إلى استكمال (Next Steps)

بينما الـ Models (الهيكل) موجودة بالكامل، فإن **المنطق البرمجي (Services & Controllers)** للميزات التالية يحتاج إلى بناء:

1.  **إدارة الكورسات (Course Management):** برمجة الـ CRUD للكورسات والأقسام (موجود Models فقط).
2.  **نظام الاختبارات (Quiz System):** برمجة منطق إنشاء الامتحان وتصحيحه (موجود Models: `Quiz`, `Question`, `Attempt`).
3.  **نظام المحادثة (Chat):** برمجة الـ SignalR Hubs للمراسلة الفورية (موجود Model: `Message`).
4.  **التجارة الإلكترونية:** تفعيل منطق الدفع والطلبات.

> **ملاحظة:** تمت مناقشة منطق "إنشاء الاختبارات" (Quiz Generation) سابقاً، ولكن الكود الخاص به غير مدمج حالياً في ملفات الـ backend الرئيسية، وقد يحتاج إلى دمج أو إعادة كتابة ضمن `Services/QuizService.cs` مستقبلاً.

---

**خاتمة:**
المشروع يمتلك أساساً متيناً جداً (Solid Foundation) من حيث هيكلة البيانات ونظام الأمان. البنية التحتية جاهزة لاستقبال منطق العمل (Business Logic) لباقي المميزات بسهولة.
