# دليل إنشاء Database من Models

## ✅ ما تم إنجازه

1. ✅ تحويل جميع ملفات ERD إلى Models جاهزة
2. ✅ إضافة EF Core packages (SqlServer, Tools, Design)
3. ✅ إنشاء ApplicationDbContext مع جميع DbSets
4. ✅ تكوين جميع العلاقات والـ Indexes
5. ✅ إضافة Connection String في appsettings.json
6. ✅ تسجيل DbContext في Program.cs

## 📊 إحصائيات

**إجمالي Models المُنشأة:** 56 Model

### تفصيل Models:

#### User & Authentication (6)
- User, UserPhone, Session, Address
- TeacherRequest, TeacherRequestDocument

#### Roles & Permissions (4)
- Role, Permission, RolePermission, UserRole

#### Courses (6)
- Category, Course, CourseRequirement, CourseLearningOutcome
- Section, SectionItem

#### Commerce & Orders (16)
- Wishlist, Cart, CartItem
- Review, ReviewHelpful, Certificate
- Coupon, CouponCourse, CouponUsage
- Order, OrderItem
- PaymentMethod, Payment, Refund, TransactionLog

#### Content & Learning (13)
- Video, Document, VideoComment, CommentLike
- Quiz, Question, Option
- QuizAttempt, UserAnswer
- LiveSession, LiveAttendance
- Enrollment, ContentProgress

#### System (7)
- File, Notification, Message, Announcement
- Report, ActivityLog, CourseLog
- SystemSetting

## 🚀 الخطوات التالية لإنشاء Database

### 1️⃣ التأكد من تثبيت EF Tools

افتح Terminal في مجلد المشروع وقم بتشغيل:

```bash
dotnet tool install --global dotnet-ef
```

للتأكد من التثبيت:
```bash
dotnet ef --version
```

### 2️⃣ تعديل Connection String (إذا لزم الأمر)

افتح `appsettings.json` وعدل الـ Connection String حسب إعداداتك:

**Windows Authentication:**
```json
"DefaultConnection": "Server=localhost;Database=LMS_Database;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

**SQL Authentication:**
```json
"DefaultConnection": "Server=localhost;Database=LMS_Database;User Id=sa;Password=YourPassword;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

**Azure SQL:**
```json
"DefaultConnection": "Server=tcp:yourserver.database.windows.net,1433;Database=LMS_Database;User Id=yourusername;Password=yourpassword;Encrypt=True;"
```

### 3️⃣ إنشاء Initial Migration

```bash
cd d:\Final_Project\BackEnd\backend_project
dotnet ef migrations add InitialCreate
```

هذا سينشئ مجلد `Migrations` يحتوي على:
- `{timestamp}_InitialCreate.cs` - Migration file
- `ApplicationDbContextModelSnapshot.cs` - Snapshot

### 4️⃣ مراجعة Migration (اختياري)

يمكنك مراجعة SQL Script الذي سيتم تنفيذه:

```bash
dotnet ef migrations script
```

أو حفظه في ملف:
```bash
dotnet ef migrations script > migration.sql
```

### 5️⃣ تطبيق Migration على Database

```bash
dotnet ef database update
```

هذا سينشئ:
- Database جديدة باسم `LMS_Database`
- جميع الجداول (56 جدول)
- جميع العلاقات والـ Foreign Keys
- جميع الـ Indexes

### 6️⃣ التحقق من نجاح العملية

افتح SQL Server Management Studio وتحقق من:
- ✅ Database تم إنشاؤها
- ✅ جميع الجداول موجودة
- ✅ العلاقات صحيحة
- ✅ Indexes تم إنشاؤها

## 🔧 أوامر مفيدة إضافية

### حذف آخر Migration (قبل تطبيقه)
```bash
dotnet ef migrations remove
```

### العودة لـ Migration سابق
```bash
dotnet ef database update PreviousMigrationName
```

### حذف Database بالكامل
```bash
dotnet ef database drop
```

### عرض قائمة Migrations
```bash
dotnet ef migrations list
```

### إنشاء Migration جديد (للتعديلات المستقبلية)
```bash
dotnet ef migrations add MigrationName
```

## 📝 ملاحظات مهمة

### Connection String
- تأكد من أن SQL Server يعمل
- تأكد من صحة اسم Server
- تأكد من الصلاحيات الكافية لإنشاء Database

### Migration
- كل مرة تعدل Model، تحتاج Migration جديد
- لا تعدل ملفات Migration يدوياً
- احتفظ بنسخة احتياطية قبل `database drop`

### Database Size
- المتوقع حجم Database الفارغة: ~50-100 MB
- مع Indexes: قد يزيد الحجم قليلاً
- التوقعات مع البيانات: يعتمد على الاستخدام

## 🎯 بعد إنشاء Database

### الخطوات التالية الموصى بها:

1. **Seed Data** - إنشاء بيانات أولية:
   - Admin User
   - Default Roles (Admin, Teacher, Student)
   - Default Permissions
   - Default Categories

2. **Repositories** - إنشاء Repository Pattern:
   - Generic Repository
   - Specific Repositories لكل Entity

3. **Services** - Business Logic:
   - UserService
   - CourseService
   - OrderService
   - إلخ...

4. **Controllers** - API Endpoints:
   - AuthController
   - CoursesController
   - OrdersController
   - إلخ...

5. **Authentication & Authorization**:
   - JWT Configuration
   - Role-based Authorization
   - Permission-based Authorization

## 🐛 حل المشاكل الشائعة

### مشكلة: "Unable to create an object of type 'ApplicationDbContext'"

**الحل:**
```bash
dotnet build
dotnet ef migrations add InitialCreate
```

### مشكلة: "A network-related or instance-specific error"

**الحل:**
- تأكد من تشغيل SQL Server
- تحقق من Connection String
- جرب: `Server=localhost\\SQLEXPRESS` أو `Server=.\\SQLEXPRESS`

### مشكلة: "Login failed for user"

**الحل:**
- تحقق من صلاحيات User
- استخدم Windows Authentication إذا أمكن
- تأكد من Password صحيح

### مشكلة: Cascade Delete Conflict

**الحل:** تم حلها مسبقاً في DbContext باستخدام:
- `DeleteBehavior.Restrict` للعلاقات التي قد تسبب مشاكل
- `DeleteBehavior.Cascade` للعلاقات الآمنة

## 📞 الدعم

إذا واجهت أي مشكلة:
1. تحقق من Error Message بعناية
2. ابحث في Documentation الرسمي لـ EF Core
3. راجع هذا الدليل
4. تحقق من Models و DbContext

---

**تم التحويل من ERD إلى Models بنجاح! 🎉**
