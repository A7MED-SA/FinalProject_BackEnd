# 🚀 Quick Start - إنشاء Database

## ✅ الحالة الحالية
- ✅ **56 Model** جاهزة
- ✅ **DbContext** مكوّن
- ✅ **Packages** مثبتة
- ✅ **Build** ناجح بدون أخطاء

## 📋 الخطوات التالية (5 دقائق)

### 1️⃣ تأكد من SQL Server
تأكد أن SQL Server يعمل على جهازك.

### 2️⃣ عدّل Connection String (إذا لزم)

افتح `appsettings.json` وعدّل إذا احتجت:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=LMS_Database;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

**بدائل:**
- `Server=.` أو `Server=(localdb)\MSSQLLocalDB`
- `Server=localhost\\SQLEXPRESS`

### 3️⃣ افتح Terminal

```bash
cd d:\Final_Project\BackEnd\backend_project
```

### 4️⃣ أنشئ Migration

```bash
dotnet ef migrations add InitialCreate
```

**المتوقع:** رسالة نجاح + إنشاء مجلد `Migrations/`

### 5️⃣ أنشئ Database

```bash
dotnet ef database update
```

**المتوقع:** 
- إنشاء database باسم `LMS_Database`
- إنشاء 56 جدول
- إنشاء جميع العلاقات والـ indexes

### 6️⃣ تحقق من النجاح ✅

افتح **SQL Server Management Studio** أو **Azure Data Studio** وتحقق من:
- Database موجودة
- الجداول تم إنشاؤها (56 جدول)

---

## 🎯 أنت الآن جاهز!

Database جاهزة للاستخدام. يمكنك الآن:

### البدء في البرمجة:
```bash
dotnet run
```

### إضافة Data أولية:
إنشاء Seeder للبيانات الأساسية (Admin user, Roles, etc.)

### بناء API:
إنشاء Controllers للـ endpoints

---

## 📊 قائمة الجداول (56)

<details>
<summary>👥 User & Auth (6)</summary>

- users
- user_phones
- sessions
- addresses
- teacher_requests
- teacher_request_documents
</details>

<details>
<summary>🔐 Roles & Permissions (4)</summary>

- roles
- permissions
- role_permissions
- user_roles
</details>

<details>
<summary>📚 Courses (6)</summary>

- categories
- courses
- course_requirements
- course_learning_outcomes
- sections
- section_items
</details>

<details>
<summary>💰 Commerce (15)</summary>

- wishlists
- carts
- cart_items
- reviews
- review_helpfuls
- certificates
- coupons
- coupon_courses
- coupon_usages
- orders
- order_items
- payment_methods
- payments
- refunds
- transaction_logs
</details>

<details>
<summary>📹 Content & Learning (13)</summary>

- videos
- documents
- video_comments
- comment_likes
- quizzes
- questions
- options
- quiz_attempts
- user_answers
- live_sessions
- live_attendances
- enrollments
- content_progresses
</details>

<details>
<summary>⚙️ System (7)</summary>

- files
- notifications
- messages
- announcements
- reports
- activity_logs
- course_logs
- system_settings
</details>

---

## 🔧 أوامر مفيدة

### Migration
```bash
# إنشاء migration جديد
dotnet ef migrations add MigrationName

# حذف آخر migration (قبل التطبيق)
dotnet ef migrations remove

# عرض SQL script
dotnet ef migrations script

# عرض قائمة migrations
dotnet ef migrations list
```

### Database
```bash
# تحديث database
dotnet ef database update

# العودة لـ migration معين
dotnet ef database update MigrationName

# حذف database
dotnet ef database drop

# إنشاء من جديد (بعد الحذف)
dotnet ef database update
```

### Build & Run
```bash
# بناء المشروع
dotnet build

# تشغيل المشروع
dotnet run

# مع hot reload
dotnet watch run
```

---

## 🆘 حل المشاكل السريع

### ❌ "Unable to create an object of type 'ApplicationDbContext'"
```bash
dotnet build
dotnet ef migrations add InitialCreate
```

### ❌ "A network-related error"
- تأكد من تشغيل SQL Server
- جرّب: `Server=localhost\\SQLEXPRESS`
- أو: `Server=(localdb)\\MSSQLLocalDB`

### ❌ "Login failed"
- استخدم Windows Authentication: `Trusted_Connection=True`
- أو SQL Auth: `User Id=sa;Password=YourPassword`

### ❌ Build Errors
```bash
# نظف وأعد البناء
dotnet clean
dotnet build
```

---

## 📚 التوثيق الكامل

- **Models/README.md** - شرح Models
- **MIGRATION_GUIDE.md** - دليل Migration مفصّل
- **ERD_TO_MODEL_MAPPING.md** - خريطة التحويل
- **CONVERSION_SUMMARY.md** - ملخص شامل

---

## ⚡ Quick Commands (Copy & Paste)

**للمبتدئين - نفذ بالترتيب:**

```bash
# 1. انتقل للمشروع
cd d:\Final_Project\BackEnd\backend_project

# 2. أنشئ migration
dotnet ef migrations add InitialCreate

# 3. أنشئ database
dotnet ef database update

# 4. شغّل المشروع
dotnet run
```

---

**🎉 بالتوفيق في مشروعك!**

في حالة أي استفسار، راجع الملفات التوثيقية أو افتح issue.
