# Athary LMS - خريطة طريق التطوير (Development Roadmap)

> **آخر تحديث**: 2026-05-23  
> **الإصدار**: v1.0.0  
> **حالة المشروع**: 3 من 7 specs مكتملة (001, 002, 003) — 4 specs جديدة مطلوبة

---

## ملخص الموقف الحالي

| Spec | الاسم | الحالة | Controllers | Services | Models |
|------|------|--------|-------------|----------|--------|
| 001 | Course Management | ✅ مكتمل | 8 | 7 | 11 |
| 002 | Course Features (Edit/Public) | ✅ مكتمل | 3 | 3 | 3 |
| 003 | Content & Learning + Deletion | ✅ مكتمل | 7 | 8 | 15 |
| 004 | **Commerce System** | ❌ جديد | 0 | 0 | 12 (موجودة مهدرة) |
| 005 | **Reviews & Certificates** | ❌ جديد | 0 | 0 | 3 (موجودة مهدرة) |
| 006 | **Dashboard & Analytics** | ❌ جديد | 0 | 0 | 0 |
| 007 | **Communication & System** | ❌ جديد | 0 | 0 | 4 (موجودة مهدرة) |

---

## الـ 4 Specs الجديدة — بالتفصيل

---

## Feature 004: Commerce System 🛒 | الأولوية: 🔴 حرجة

### الوضع الحالي
12 model موجودة ومهدرة بالكامل: `Cart`, `CartItem`, `Order`, `OrderItem`, `Payment`, `PaymentMethod`, `Refund`, `TransactionLog`, `Coupon`, `CouponCourse`, `CouponUsage`, `Wishlist`

### المستخدمون
- **Student**: يضيف كورسات للسلة، يطبق كوبون، يشتري، يشوف المشتريات، يطلب استرداد، يضيف لقائمة الأمنيات
- **Admin**: يدير الكوبونات، يشوف المبيعات، يعالج طلبات الاسترداد، يدير طرق الدفع

### User Stories (مرتبة بالأولوية)

| الأولوية | القصة | الوصف |
|---------|-------|-------|
| **P1** | إدارة السلة (Cart) | إضافة/إزالة items، عرض السلة، تحديث الكميات، Price Snapshot |
| **P1** | نظام الكوبونات (Coupon) | CRUD كوبونات من Admin، تطبيق كود خصم مع التحقق، استخدام tracking |
| **P1** | إنشاء الطلب (Order) | تحويل السلة لـ Order، Order Number generation، حالات الطلب |
| **P1** | معالجة الدفع (Payment) | واجهة دفع (Stripe/PayPal prep)، حالات الدفع، Transaction Log |
| **P2** | استرداد المبالغ (Refund) | طلب استرداد، معالجة، حالات استرداد، تحديث التسجيل |
| **P2** | قائمة الأمنيات (Wishlist) | إضافة/إزالة كورسات لقائمة الأمنيات |

### الفرق عن الـ specs اللي قبل كدا
- **هذه أول feature بتتعامل مع أموال حقيقية** — لازم Transactions، Audit Trail، Validation دقيق
- Payment Gateway هيكون Stripe/PayPal (واجهة abstract عشان نقدر نغير)
- تكامل مع `EnrollmentService` (لما الدفع ينجح → يتسجل تلقائياً)

### نموذج البيانات (الموجود حالياً)

```
Cart (1) ──< CartItem (N) ──> Course
Order (1) ──< OrderItem (N) ──> Course
Order (1) ──< Payment (N) ──< TransactionLog (N)
Payment (1) ──< Refund (N)
Coupon >──< Course (via CouponCourse)
Coupon (1) ──< CouponUsage (N)
Wishlist (1) ──< Course (N)
```

### الـ APIs الجديدة

- `POST /api/cart/items` — إضافة كورس للسلة
- `DELETE /api/cart/items/{itemId}` — إزالة
- `GET /api/cart` — عرض السلة
- `POST /api/orders` — إنشاء طلب (من السلة)
- `GET /api/orders` — طلباتي
- `GET /api/orders/{id}` — تفاصيل طلب
- `POST /api/orders/{id}/pay` — عملية دفع
- `POST /api/orders/{id}/refund` — طلب استرداد
- `POST /api/coupons/validate` — التحقق من كود خصم
- `GET /api/coupons` — (Admin) قائمة الكوبونات
- `POST /api/coupons` — (Admin) إنشاء كوبون
- `GET /api/wishlist` — قائمة الأمنيات
- `POST /api/wishlist/{courseId}` — إضافة
- `DELETE /api/wishlist/{courseId}` — إزالة

### الجدول الزمني: 7-10 أيام

---

## Feature 005: Reviews & Certificates ⭐ | الأولوية: 🔴 عالية

### الوضع الحالي
3 models موجودة: `Review`, `ReviewHelpful`, `Certificate`

### User Stories

| الأولوية | القصة | الوصف |
|---------|-------|-------|
| **P1** | تقييم الكورسات (Review) | إضافة تقييم (1-5) + تعليق، الموافقة/الرفض من Admin |
| **P1** | الإعجاب بالتقييمات | Helpful/Not Helpful toggle، تحديث العداد |
| **P2** | الشهادات (Certificate) | إصدار تلقائي عند إكمال الكورس، رمز تحقق فريد، عرض الشهادة |
| **P2** | التحقق من الشهادة | API عام للتحقق من صحة الشهادة باستخدام الـ Verification Code |

### الـ APIs الجديدة

- `POST /api/courses/{courseId}/reviews` — إضافة تقييم
- `GET /api/courses/{courseId}/reviews` — عرض تقييمات الكورس
- `PUT /api/admin/reviews/{id}/approve` — (Admin) موافقة
- `PUT /api/admin/reviews/{id}/reject` — (Admin) رفض
- `POST /api/reviews/{id}/helpful` — toggle إعجاب
- `GET /api/certificates` — شهاداتي
- `GET /api/certificates/{id}` — تفاصيل شهادة
- `GET /api/certificates/verify/{code}` — تحقق عام من شهادة

### الجدول الزمني: 4-5 أيام

---

## Feature 006: Dashboard & Analytics 📊 | الأولوية: 🔴 عالية

### الوضع الحالي
لا يوجد موديلات جديدة مطلوبة — كل البيانات موجودة. المطلوب Service + Controller جديدين.

### User Stories

| الأولوية | القصة | الوصف |
|---------|-------|-------|
| **P1** | لوحة تحكم المدرس | عرض كورسات المدرس مع إحصائيات (عدد الطلاب، المدة، التقييم) |
| **P1** | لوحة تحكم المشرف | إحصائيات عامة (إجمالي المستخدمين، الكورسات، الإيرادات) |
| **P2** | تقارير الإيرادات | إيرادات شهرية/سنوية، أفضل الكورسات مبيعاً |
| **P2** | تحليلات النمو | نمو المستخدمين/التسجيلات مع مرور الوقت |

### الـ APIs الجديدة

- `GET /api/management/courses` — كورسات المدرس (مفقود حاليًا!)
- `GET /api/management/dashboard/stats` — إحصائيات المدرس
- `GET /api/admin/dashboard/overview` — إحصائيات المشرف
- `GET /api/admin/dashboard/revenue/monthly` — الإيرادات الشهرية
- `GET /api/admin/dashboard/user-growth` — نمو المستخدمين
- `GET /api/admin/dashboard/popular-courses` — أفضل الكورسات

### مهم
هذه الـ APIs **تعتمد على Feature 004** لأن الإحصائيات المالية (Revenue, Top-selling courses) مش هتشتغل من غير Commerce System.  

### الجدول الزمني: 3-4 أيام (بعد Commerce)

---

## Feature 007: Communication & System Settings 💬 | الأولوية: 🟡 متوسطة

### الوضع الحالي
4 models موجودة: `Message`, `Announcement`, `Report`, `SystemSetting`

### User Stories

| الأولوية | القصة | الوصف |
|---------|-------|-------|
| **P2** | الرسائل المباشرة | إرسال/استقبال رسائل بين المستخدمين، وسم بالقراءة |
| **P2** | الإعلانات | إنشاء إعلانات من Admin (لفئة محددة)، عرض للمستخدمين |
| **P2** | الإبلاغ عن محتوى | إبلاغ عن كورس/تعليق/مستخدم، معالجة البلاغات من Admin |
| **P3** | إعدادات النظام | Key-Value settings (نسبة إتمام الفيديو، حدود التحميل، إلخ) |

### الـ APIs الجديدة

- `GET /api/messages` — رسائلي
- `POST /api/messages` — إرسال رسالة
- `PUT /api/messages/{id}/read` — وسم مقروء
- `GET /api/announcements` — الإعلانات
- `POST /api/announcements` — (Admin) إنشاء إعلان
- `POST /api/reports` — الإبلاغ
- `GET /api/admin/reports` — (Admin) قائمة البلاغات
- `PUT /api/admin/reports/{id}/resolve` — (Admin) معالجة بلاغ
- `GET /api/admin/settings` — (Admin) الإعدادات
- `PUT /api/admin/settings/{key}` — (Admin) تحديث إعداد

### الجدول الزمني: 5-6 أيام

---

## خريطة التبعيات (Dependency Graph)

```
Feature 004: Commerce  ◄─── يعتمد على (Program.cs DI, DbContext ready)
    │
    ├──► Feature 005: Reviews (مستقل، ممكن بالتوازي)
    │
    └──► Feature 006: Dashboard ◄─── يعتمد على 004 للإحصائيات المالية
    │
    └──► Feature 007: Communication (مستقل، ممكن بالتوازي)
```

### أفضل ترتيب للتنفيذ

```
الأسبوع 1:  Feature 004 (Commerce) — الأهم والأكبر
الأسبوع 2:  Feature 005 (Reviews) + Feature 007 (Communication) بالتوازي
الأسبوع 3:  Feature 006 (Dashboard) + Feature 008 (Testing/Architecture)
الأسبوع 4:  Polish, Bug Fixes, Deployment
```

---

## Feature 008: Testing & Architecture 🧪 | الأولوية: 🟡 متوسطة (بعد كل الفيتشرز)

### الهدف
رفع جودة الكود بعد ما نخلص كل الفيتشرز الأساسية.

### User Stories

| الأولوية | القصة | الوصف |
|---------|-------|-------|
| **P2** | Unit Tests للـ Services | xUnit + Moq لـ Services المهمة (Course, Enrollment, Payment - لما يتعمل) |
| **P2** | Integration Tests | اختبارات API كاملة (WebApplicationFactory) |
| **P3** | Repository Pattern | إضافة طبقة Repository للـ Services **الجديدة فقط** (مش هترجع نعدل القديم) |
| **P3** | Caching | إضافة MemoryCache/Redis للـ Public APIs (تصنيفات، كورسات عامة) |
| **P3** | Health Checks | إضافة `/health` endpoint للمراقبة |

### المهم
- **لا نعيد كتابة الـ Services القديمة** — فقط تحسين التدريجي
- Repository Pattern للجديد بس
- Testing مهم لكنه مش مانع للنشر

---

## إجمالي工作量 (تقديري)

| Feature | أيام | ملفات جديدة | تعديلات |
|---------|------|------------|---------|
| 004-Commerce | 7-10 | ~20 | Program.cs DI |
| 005-Reviews | 4-5 | ~10 | Program.cs DI |
| 006-Dashboard | 3-4 | ~6 | Program.cs DI |
| 007-Communication | 5-6 | ~12 | Program.cs DI |
| 008-Testing | 5-7 | ~25 | — |
| **المجموع** | **24-32 يوم** | **~73 ملف** | — |
