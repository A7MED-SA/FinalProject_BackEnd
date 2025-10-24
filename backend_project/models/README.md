# Entity Framework Core Models

تم إنشاء جميع الـ Models بناءً على ملفات ERD الموجودة في مجلد `ERD/`.

## 📁 هيكل Models

### User & Authentication
- `User.cs` - المستخدمين
- `UserPhone.cs` - أرقام هواتف المستخدمين
- `Session.cs` - جلسات المستخدمين
- `Address.cs` - عناوين المستخدمين
- `TeacherRequest.cs` - طلبات المدرسين
- `TeacherRequestDocument.cs` - مستندات طلبات المدرسين

### Roles & Permissions
- `Role.cs` - الأدوار
- `Permission.cs` - الصلاحيات
- `RolePermission.cs` - صلاحيات الأدوار
- `UserRole.cs` - أدوار المستخدمين

### Courses
- `Category.cs` - فئات الكورسات
- `Course.cs` - الكورسات
- `CourseRequirement.cs` - متطلبات الكورس
- `CourseLearningOutcome.cs` - نتائج التعلم
- `Section.cs` - أقسام الكورس
- `SectionItem.cs` - عناصر القسم

### Commerce & Orders
- `Wishlist.cs` - قائمة الرغبات
- `Cart.cs` - عربة التسوق
- `CartItem.cs` - عناصر العربة
- `Review.cs` - التقييمات
- `ReviewHelpful.cs` - تقييم المراجعات
- `Certificate.cs` - الشهادات
- `Coupon.cs` - كوبونات الخصم
- `CouponCourse.cs` - ربط الكوبونات بالكورسات
- `CouponUsage.cs` - استخدام الكوبونات
- `Order.cs` - الطلبات
- `OrderItem.cs` - عناصر الطلب
- `PaymentMethod.cs` - طرق الدفع
- `Payment.cs` - المدفوعات
- `Refund.cs` - المرتجعات
- `TransactionLog.cs` - سجلات المعاملات

### Content & Learning
- `Video.cs` - الفيديوهات
- `Document.cs` - المستندات
- `VideoComment.cs` - تعليقات الفيديو
- `CommentLike.cs` - إعجابات التعليقات
- `Quiz.cs` - الاختبارات
- `Question.cs` - الأسئلة
- `Option.cs` - خيارات الأسئلة
- `QuizAttempt.cs` - محاولات الاختبار
- `UserAnswer.cs` - إجابات المستخدمين
- `LiveSession.cs` - الجلسات المباشرة
- `LiveAttendance.cs` - حضور الجلسات
- `Enrollment.cs` - التسجيل في الكورسات
- `ContentProgress.cs` - تقدم المحتوى

### System
- `UploadedFile.cs` - الملفات (تم تسميته UploadedFile لتجنب التعارض مع System.IO.File)
- `Notification.cs` - الإشعارات
- `Message.cs` - الرسائل
- `Announcement.cs` - الإعلانات
- `Report.cs` - البلاغات
- `ActivityLog.cs` - سجل النشاطات
- `CourseLog.cs` - سجل الكورسات
- `SystemSetting.cs` - إعدادات النظام

## 🚀 كيفية إنشاء Database

### 1. تأكد من تثبيت EF Core Tools

```bash
dotnet tool install --global dotnet-ef
```

### 2. تعديل Connection String

في ملف `appsettings.json`، قم بتعديل الـ Connection String حسب إعدادات SQL Server لديك:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=LMS_Database;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

**أو إذا كنت تستخدم SQL Authentication:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=LMS_Database;User Id=your_username;Password=your_password;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

### 3. إنشاء Migration

```bash
cd d:\Final_Project\BackEnd\backend_project
dotnet ef migrations add InitialCreate
```

### 4. تطبيق Migration على Database

```bash
dotnet ef database update
```

## 📊 الميزات

### ✅ Entity Relationships
- جميع العلاقات بين الجداول تم تكوينها بشكل صحيح
- استخدام `DeleteBehavior` المناسب لكل علاقة
- Self-referencing relationships (مثل Category و VideoComment)

### ✅ Indexes
- Unique indexes على الحقول المهمة (Email, Slug, Code, etc.)
- Foreign key indexes تلقائية من EF Core

### ✅ Data Annotations
- `[Required]` للحقول الإلزامية
- `[MaxLength]` لتحديد طول النصوص
- `[Column(TypeName)]` لتحديد أنواع البيانات بدقة
- `[ForeignKey]` لتوضيح العلاقات

### ✅ Enumerations
جميع القيم المحدودة تم تحويلها لـ Enums:
- `PhoneType`, `AddressType`
- `CourseLevel`, `CourseLanguage`, `CourseStatus`
- `ReviewStatus`, `OrderStatus`, `PaymentStatus`
- `VideoProvider`, `VideoQuality`, `VideoStatus`
- وغيرها...

## 🔧 أوامر مفيدة

### إنشاء Migration جديد
```bash
dotnet ef migrations add MigrationName
```

### حذف آخر Migration
```bash
dotnet ef migrations remove
```

### تحديث Database
```bash
dotnet ef database update
```

### العودة لـ Migration معين
```bash
dotnet ef database update MigrationName
```

### عرض SQL Script للـ Migration
```bash
dotnet ef migrations script
```

### حذف Database
```bash
dotnet ef database drop
```

## 📝 ملاحظات مهمة

1. **Soft Delete**: بعض الجداول تحتوي على `DeletedAt` للـ Soft Delete
2. **Timestamps**: معظم الجداول تحتوي على `CreatedAt`, `UpdatedAt`
3. **Navigation Properties**: جميع العلاقات لها navigation properties للتنقل السهل
4. **Cascade Delete**: تم تكوينه بحذر لتجنب مشاكل Cascade Cycles

## 🎯 الخطوات التالية

1. ✅ تثبيت EF Core packages
2. ✅ إنشاء Models
3. ✅ إنشاء DbContext
4. ✅ تكوين العلاقات
5. ⬜ إنشاء Migration
6. ⬜ تطبيق Migration على Database
7. ⬜ إنشاء Repositories
8. ⬜ إنشاء Services
9. ⬜ إنشاء Controllers
