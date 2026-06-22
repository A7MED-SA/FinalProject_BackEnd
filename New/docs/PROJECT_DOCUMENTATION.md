# توثيق منصة آثاري - Athary Platform

## 📋 فهرس المحتويات
1. [نظرة عامة عن المشروع](#نظرة-عامة-عن-المشروع)
2. [هيكل المشروع (Project Structure)](#هيكل-المشروع)
3. [التقنيات المستخدمة (Tech Stack)](#التقنيات-المستخدمة)
4. [الصفحات والمسارات (Routes)](#الصفحات-والمسارات)
5. [وصف كل صفحة والبيانات التي تعرضها](#وصف-كل-صفحة-والبيانات)
6. [قاعدة البيانات - الـ API المطلوبة لكل صفحة](#قاعدة-البيانات--api-المطلوبة-لكل-صفحة)
7. [نظام المصادقة (Authentication System)](#نظام-المصادقة)
8. [ملفات الحالة المخزنة محلياً (localStorage Keys)](#localstorage-keys)
9. [المكونات المشتركة (Shared Components)](#المكونات-المشتركة)
10. [الأنواع (TypeScript Types)](#typescript-types)

---

## نظرة عامة عن المشروع

**آثاري** هي منصة تعليمية تراثية عربية تهدف إلى إحياء التراث الإسلامي والعربي عبر دورات تعليمية في:
- التاريخ الإسلامي
- اللغة العربية وآدابها
- الفنون والعمارة التراثية
- علم الآثار والتحقيق

**نوع المشروع:** SPA (Single Page Application) - Vite + React + TypeScript  
**نمط التوجيه:** Client-side Routing (React Router v7)  
**حالة المشروع:** Frontend Prototype (جميع البيانات Mocked، لا يوجد Backend حقيقي)

---

## هيكل المشروع

```
Pro_Front/
├── index.html                     # مدخل Vite
├── package.json                   # ملف الحزم والاعتماديات
├── tsconfig.json                  # إعدادات TypeScript
├── vite.config.ts                 # إعدادات Vite
├── .env.example                   # مثال لمتغيرات البيئة
├── metadata.json                  # بيانات AI Studio
│
├── assets/                        # الأصول الثابتة
├── dist/                          # مخرج البناء النهائي
├── node_modules/                  # حزم npm
│
├── docs/                          # التوثيق
│   └── PROJECT_DOCUMENTATION.md   # هذا الملف
│
└── src/
    ├── main.tsx                   # نقطة الدخول
    ├── router.tsx                 # تعريف المسارات (Routing)
    ├── index.css                  # التنسيقات (Tailwind + Custom CSS)
    │
    ├── types/
    │   └── index.ts               # أنواع TypeScript الأساسية
    │
    ├── data/
    │   └── index.ts               # البيانات الوهمية (Mock Data)
    │
    ├── lib/
    │   ├── api.ts                 # Axios instance مع Interceptors
    │   └── query-client.ts        # React Query client config
    │
    ├── stores/
    │   └── notificationStore.ts   # Zustand store للإشعارات
    │
    ├── hooks/
    │   └── useTheme.ts            # Hook للمظهر (Dark/Light/Colors)
    │
    ├── providers/
    │   └── AppProvider.tsx        # React Context (الحالة العامة)
    │
    ├── layouts/
    │   └── RootLayout.tsx         # الـ Layout الرئيسي (Navbar + Footer + CartDrawer)
    │
    ├── components/
    │   └── layout/
    │       ├── Navbar.tsx         # شريط التنقل العلوي
    │       ├── Footer.tsx         # التذييل
    │       ├── CartDrawer.tsx     # سلة التسوق المنزلقة
    │       ├── NotFound.tsx       # صفحة 404
    │       └── ErrorBoundary.tsx  # حد الأخطاء
    │
    └── features/
        ├── landing/
        │   └── LandingPage.tsx
        ├── catalog/
        │   ├── CourseCatalog.tsx
        │   └── CourseDetails.tsx
        ├── cart/
        │   └── CartCheckout.tsx
        ├── auth/
        │   └── AuthPage.tsx
        ├── student/
        │   ├── StudentDashboard.tsx
        │   ├── LearningRoom.tsx
        │   ├── QuizTaking.tsx
        │   ├── MessagingCenter.tsx
        │   ├── ManuscriptCertificate.tsx
        │   ├── WishlistRefunds.tsx
        │   └── InstructorApply.tsx
        ├── instructor/
        │   ├── InstructorDashboard.tsx
        │   ├── CourseBuilder.tsx
        │   └── LiveSession.tsx
        ├── admin/
        │   ├── AdminDashboard.tsx
        │   ├── AdvancedAnalytics.tsx
        │   ├── ReviewsModeration.tsx
        │   ├── AnnouncementsCenter.tsx
        │   ├── MediaLibrary.tsx
        │   └── SystemActivitySettings.tsx
        ├── profile/
        │   ├── ProfileSettings.tsx
        │   └── PublicProfile.tsx
        ├── about/
        │   └── AboutContactPublic.tsx
        └── theme/
            └── ThemeSettingsPopover.tsx
```

---

## التقنيات المستخدمة

| التقنية | النسخة | الغرض |
|---------|--------|-------|
| Vite | ^6.2.3 | Build tool / Dev server |
| React | ^19.0.1 | UI Library |
| TypeScript | ~5.8.2 | Type Safety |
| React Router DOM | ^7.18.0 | Client-side Routing |
| TanStack React Query | ^5.101.0 | Server State Management |
| Axios | ^1.18.0 | HTTP Client |
| Zustand | ^5.0.14 | State Management |
| Tailwind CSS | ^4.1.14 | Utility-first CSS |
| Motion (Framer Motion) | ^12.23.24 | Animations |
| Recharts | ^3.8.1 | Charts & Graphs |
| react-hook-form | ^7.78.0 | Forms |
| Zod | ^4.4.3 | Validation Schemas |
| Radix UI | Popover, Switch | Unstyled UI Primitives |
| Lucide React | ^0.546.0 | Icons |

---

## الصفحات والمسارات

| المسار (Route) | المكون (Component) | الوصف |
|----------------|-------------------|-------|
| `/` | `LandingPage` | الصفحة الرئيسية |
| `/catalog` | `CourseCatalog` | كتالوج الدورات مع الفلاتر والبحث |
| `/course/:courseId` | `CourseDetails` | صفحة تفاصيل دورة معينة |
| `/checkout` | `CartCheckout` | سلة التسوق وإتمام الشراء |
| `/auth` | `AuthPage` | تسجيل الدخول / إنشاء حساب / نسيت كلمة المرور / OTP / إعادة تعيين |
| `/dashboard` | `StudentDashboard` | لوحة تحكم الطالب |
| `/instructor` | `InstructorDashboard` | لوحة تحكم المدرب |
| `/admin` | `AdminDashboard` | لوحة تحكم المشرف |
| `/about` | `AboutContactPublic` | معلومات عن المنصة + نموذج الاتصال |
| `/profile` | `ProfileSettings` | إعدادات الملف الشخصي |
| `/instructor/:name` | `PublicProfile` | الملف الشخصي العام للمدرب |
| `*` | `NotFound` | صفحة 404 |

---

## وصف كل صفحة والبيانات

### 1. الصفحة الرئيسية (LandingPage) — `/`

#### البيانات المعروضة:
- **Hero Section**: عنوان ترحيبي، وصف المنصة، زرين (تصفح المسارات، بروشور التعريف)
- **Trust Indicators**: نسبة رضا 98%، شهادات موثقة 100%، بث مباشر
- **بطاقة جذابة**: صورة عمارة إسلامية، اقتباس، عداد الطلاب
- **Stats Section**: 4 إحصائيات (أكثر من 10,000 طالب، +500 ساعة، +50 مدرب، 24/7 منتديات)
- **التصنيفات (Categories)**: 4 تصنيفات (التاريخ الإسلامي، اللغة العربية، الفنون التراثية، علم الآثار)
- **الدورات المميزة (Featured Courses)**: أول 3 دورات مع إمكانية الإضافة للسلة
- **How It Works**: 3 خطوات (اختر منبر دراستك → حضور المجالس → الامتحان والإجازة)
- **Testimonials**: شهادات الطلاب

#### البيانات المطلوبة:

| نوع البيانات | المصدر الحالي | الوصف |
|-------------|--------------|-------|
| `Category[]` | Mock (`CATEGORIES`) | التصنيفات: id, name, slug, iconName, courseCount |
| `Course[]` | Mock (`COURSES`) | الدورات المميزة (أول 3): id, title, category, instructorName, price, thumbnail, rating, studentsCount, lessonsCount, duration |
| `Testimonial[]` | Mock (`TESTIMONIALS`) | آراء الطلاب: id, name, role, content, avatar |
| أوقات البث المباشر | Mock (`LIVE_SESSIONS`) | جلسات مباشرة قادمة |

---

### 2. كتالوج الدورات (CourseCatalog) — `/catalog`

#### البيانات المعروضة:
- **Banner**: عنوان ووصف الكتالوج
- **Search Bar**: بحث فوري (بالاسم، المدرب، التصنيف)
- **Filters Sidebar**:
  - الفئة والموضوع (4 categories - Checkboxes)
  - المستوى المستهدف (مبتدئ/متوسط/متقدم - Checkboxes)
  - نموذج السعر (الكل/مجاني/مدفوع - Radio)
  - تقييم الطلاب (4.8/4.5/4.0 فأكثر - Radio)
- **Sort**: الأكثر قيمة، الأعلى تقييماً، السعر (من الأقل/الأعلى)
- **Course Cards**: صورة مصغرة، التصنيف، اسم المدرب، التقييم، السعر، زر الإضافة للسلة
- **Reviews Dialog**: نافذة منبثقة للمراجعات والتعليقات لكل دورة
- **Mobile Filters**: نافذة جانبية للفلاتر على الموبايل

#### البيانات المطلوبة:

| API | Method | الهدف |
|-----|--------|-------|
| `GET /api/courses` | GET | جلب كل الدورات مع دعم الفلاتر: `?search=`, `?category=`, `?level=`, `?price=`, `?minRating=`, `?sortBy=`, `?page=`, `?limit=` |
| `GET /api/categories` | GET | جلب التصنيفات |
| `GET /api/courses/:id/reviews` | GET | جلب مراجعات دورة معينة |
| `POST /api/courses/:id/reviews` | POST | إضافة مراجعة جديدة (body: `{ userName, rating, comment }`) |
| `PUT /api/reviews/:id/helpful` | PUT | التصويت بأن المراجعة مفيدة |

#### هيكل Course Card:
```
{
  id, title, category, categorySlug, instructorName, instructorAvatar,
  rating, studentsCount, price, originalPrice?, duration, lessonsCount,
  thumbnail, progress?, nextLesson?, featured?, level?
}
```

---

### 3. تفاصيل الدورة (CourseDetails) — `/course/:courseId`

#### البيانات المعروضة:
- **Hero Banner**: عنوان الدورة، وصف، breadcrumb، التصنيف
- **Stats Bar**: التقييم (نجوم)، عدد الطلاب، اللغة، آخر تحديث
- **Sidebar (يشتري)**: السعر، زر الإضافة للسلة، حفظ للمفضلة، مشاركة
- **ماذا ستتعلم؟**: قائمة من 4-6 نقاط تعليمية
- **المنهج (Syllabus Accordion)**: فصول ودروس مع مؤشر التقدم وحالة الإكمال
- **ملف المدرب**: صورة، اسم، وصف، إحصائيات
- **التقييمات والمراجعات**: قائمة مراجعات الطلاب مع نظام التقييم بالنجوم

#### البيانات المطلوبة:

| API | Method | الهدف |
|-----|--------|-------|
| `GET /api/courses/:courseId` | GET | تفاصيل الدورة الكاملة بما في ذلك المخرجات التعليمية |
| `GET /api/courses/:courseId/syllabus` | GET | فصول وأقسام المنهج مع الدروس |
| `GET /api/courses/:courseId/reviews` | GET | مراجعات الدورة |
| `POST /api/courses/:courseId/reviews` | POST | إضافة مراجعة |
| `POST /api/courses/:courseId/favorite` | POST | إضافة/إزالة من المفضلة |
| `GET /api/courses/:courseId/progress` | GET | تقدم الطالب في الدورة (إذا مسجل فيها) |
| `PUT /api/courses/:courseId/lessons/:lessonId/progress` | PUT | تحديث تقدم درس معين (body: `{ completed: boolean }`) |

#### هيكل Syllabus:
```
Chapter {
  title: string,
  lessons: Lesson[]
}
Lesson {
  title: string,
  duration: string,
  type: 'video' | 'document' | 'quiz',
  free: boolean,
  videoUrl?: string,
  documentUrl?: string
}
```

---

### 4. سلة التسوق (CartCheckout) — `/checkout`

#### البيانات المعروضة:
- **قائمة العناصر في السلة**: صورة، عنوان، تصنيف، مدرب، السعر
- **حذف عنصر**: نافذة تأكيد الحذف
- **Coupon Code**: إدخال كود الخصم مع أمثلة (`ATHARY_FOUNDER` = 25%, `FREE100` = مجاني)
- **ملخص الفاتورة**: المجموع الفرعي، الخصم، رسوم القيد، الإجمالي النهائي
- **Secure Checkout**: زر تأكيد القيد
- **Payment Methods**: مدى، تحويل بنكي، فيزا، أبل باي

#### البيانات المطلوبة:

| API | Method | الهدف |
|-----|--------|-------|
| `GET /api/cart` | GET | جلب محتويات السلة (إذا مخزنة على السيرفر) |
| `POST /api/cart/add` | POST | إضافة دورة للسلة (body: `{ courseId }`) |
| `DELETE /api/cart/:courseId` | DELETE | إزالة دورة من السلة |
| `POST /api/coupons/validate` | POST | التحقق من صحة كود الخصم (body: `{ code, subtotal }`) |
| `POST /api/checkout` | POST | إتمام عملية الشراء (body: `{ items[], couponCode?, paymentMethod }`) |

---

### 5. صفحة المصادقة (AuthPage) — `/auth`

#### البيانات المعروضة (6 حالات):
1. **Login**: بريد إلكتروني، كلمة مرور، Google sign-in
2. **Register (3 خطوات)**:
   - الخطوة 1: الاسم الأول، اسم العائلة، البريد الإلكتروني، كلمة المرور، تأكيدها + مؤشر قوة كلمة المرور
   - الخطوة 2: رقم الهاتف، الجنس (اختياري)، تاريخ الميلاد (اختياري)
   - الخطوة 3: الدولة، المدينة، عنوان الشارع، الرمز البريدي (اختياري)
3. **Forgot Password**: إدخال البريد الإلكتروني
4. **Verify OTP**: 6 خانات لإدخال الرمز مع عداد إعادة الإرسال
5. **Reset Password**: كلمة مرور جديدة + تأكيد
6. **Success**: رسالة نجاح

#### البيانات المطلوبة:

| API | Method | الهدف |
|-----|--------|-------|
| `POST /api/auth/login` | POST | تسجيل الدخول (body: `{ email, password }`) → يعيد `{ token, user }` |
| `POST /api/auth/register` | POST | إنشاء حساب جديد (body: `{ firstName, lastName, email, password, phone, gender?, dob?, country?, city?, streetLine1?, postalCode? }`) |
| `POST /api/auth/verify-email` | POST | تأكيد البريد الإلكتروني (body: `{ email, otp }`) |
| `POST /api/auth/resend-otp` | POST | إعادة إرسال رمز التحقق (body: `{ email }`) |
| `POST /api/auth/forgot-password` | POST | طلب إعادة تعيين كلمة المرور (body: `{ email }`) |
| `POST /api/auth/reset-password` | POST | إعادة تعيين كلمة المرور (body: `{ email, token, newPassword }`) |
| `POST /api/auth/google` | POST | تسجيل الدخول عبر Google (body: `{ idToken }`) |

---

### 6. لوحة تحكم الطالب (StudentDashboard) — `/dashboard`

#### التبويبات (Tabs):
- **Overview**: إحصائيات (الدورات المكتملة/قيد الدراسة/لم تبدأ)، Bar Chart، Pie Chart
- **My Courses**: قائمة دورات الطالب مع نسبة التقدم وزر "متابعة التعلم"
- **Certificates**: الشهادات المتحصل عليها مع زر التحميل والمشاركة
- **Favorites**: قائمة المفضلة وطلبات الاسترداد
- **Notifications**: إشعارات النظام
- **Instructor Apply**: طلب الانضمام كمدرب

#### المكونات الفرعية المضمنة:
| المكون | الوصف |
|--------|-------|
| `LearningRoom` | مشغل الفيديو التعليمي مع قائمة الدروس والملاحظات |
| `QuizTaking` | واجهة الاختبارات مع مؤقت ودرجات |
| `MessagingCenter` | نظام التراسل مع المدربين |
| `ManuscriptCertificate` | عرض الشهادة مع خيارات التحميل والمشاركة |
| `WishlistRefunds` | قائمة المفضلة وطلبات استرداد المبالغ |
| `InstructorApply` | نموذج طلب الانضمام كمدرب مع رفع الملفات |

#### البيانات المطلوبة:

| API | Method | الهدف |
|-----|--------|-------|
| `GET /api/student/dashboard` | GET | إحصائيات لوحة التحكم |
| `GET /api/student/courses` | GET | دورات الطالب المسجلة مع التقدم |
| `GET /api/student/certificates` | GET | شهادات الطالب |
| `GET /api/student/favorites` | GET | الدورات المفضلة |
| `GET /api/notifications` | GET | الإشعارات |
| `PUT /api/notifications/:id/read` | PUT | تعيين الإشعار كمقروء |
| `POST /api/student/instructor-apply` | POST | تقديم طلب مدرب (FormData مع CV) |
| `GET /api/student/live-sessions` | GET | الجلسات المباشرة القادمة |
| `POST /api/student/refund-request` | POST | طلب استرداد مبلغ |
| `POST /api/courses/:courseId/favorite` | POST | إضافة/إزالة من المفضلة |
| `GET /api/quiz/:quizId` | GET | جلب بيانات الاختبار |
| `POST /api/quiz/:quizId/submit` | POST | تقديم إجابات الاختبار (body: `{ answers[] }`) |

---

### 7. لوحة تحكم المدرب (InstructorDashboard) — `/instructor`

#### التبويبات (Tabs):
- **Overview**: إحصائيات (إجمالي الطلاب، الإيرادات، التقييم، عدد الدورات) + Area Chart للإيرادات
- **My Courses**: قائمة الدورات مع حالة النشر، بحث وتصفية
- **Course Builder**: منشئ الدورات (إضافة فصول ودروس، رفع فيديو، تحديد السعر)
- **Revisions**: طلبات المراجعة من المشرفين
- **Earnings**: تفاصيل الإيرادات مع Bar Chart شهري
- **Notifications**: إشعارات المدرب

#### البيانات المطلوبة:

| API | Method | الهدف |
|-----|--------|-------|
| `GET /api/instructor/dashboard` | GET | إحصائيات المدرب |
| `GET /api/instructor/courses` | GET | دورات المدرب |
| `POST /api/instructor/courses` | POST | إنشاء دورة جديدة (FormData) |
| `PUT /api/instructor/courses/:id` | PUT | تحديث بيانات الدورة |
| `DELETE /api/instructor/courses/:id` | DELETE | حذف دورة |
| `POST /api/instructor/courses/:id/publish` | POST | طلب نشر الدورة |
| `GET /api/instructor/earnings` | GET | تقارير الإيرادات `?from=&to=` |
| `PUT /api/instructor/courses/:id/lessons` | PUT | تحديث الدروس (body: `{ sections[] }`) |
| `POST /api/instructor/live-sessions` | POST | إنشاء جلسة مباشرة |
| `POST /api/media/upload` | POST | رفع ملف وسائط (FormData) |
| `DELETE /api/media/:id` | DELETE | حذف ملف وسائط |

---

### 8. لوحة تحكم المشرف (AdminDashboard) — `/admin`

#### التبويبات (Tabs):
- **Overview**: إحصائيات عامة (مجموع المستخدمين، الإيرادات، الدورات، المدربين)
- **Courses**: مراجعة واعتماد/رفض الدورات الجديدة
- **Teachers**: مراجعة واعتماد/رفض طلبات المدربين الجدد
- **Orders & Refunds**: إدارة الطلبات وطلبات الاسترداد
- **Categories**: إدارة التصنيفات
- **Reviews**: الإشراف على المراجعات المبلغ عنها
- **Announcements**: إرسال إعلانات للمستخدمين
- **Media Library**: إدارة مكتبة الوسائط
- **System Logs**: سجل النشاطات وإعدادات النظام

#### المكونات الفرعية المضمنة:
| المكون | الوصف |
|--------|-------|
| `ReviewsModeration` | مراجعة التقييمات المبلغ عنها |
| `AnnouncementsCenter` | مركز إرسال الإعلانات (Push Notifications) |
| `MediaLibrary` | مكتبة رفع وإدارة الملفات (صور، فيديو، PDF، Excel) |
| `SystemActivitySettings` | سجل التدقيق (Audit Logs) وإعدادات النظام |
| `AdvancedAnalytics` | تحليلات متقدمة مع رسوم بيانية متعددة |

#### البيانات المطلوبة:

| API | Method | الهدف |
|-----|--------|-------|
| `GET /api/admin/dashboard` | GET | إحصائيات لوحة المشرف |
| `GET /api/admin/courses` | GET | جميع الدورات مع حالة المراجعة |
| `PUT /api/admin/courses/:id/approve` | PUT | اعتماد دورة |
| `PUT /api/admin/courses/:id/reject` | PUT | رفض دورة (body: `{ reason }`) |
| `GET /api/admin/teacher-requests` | GET | طلبات المدربين الجدد |
| `PUT /api/admin/teacher-requests/:id/approve` | PUT | اعتماد مدرب |
| `PUT /api/admin/teacher-requests/:id/reject` | PUT | رفض طلب مدرب (body: `{ reason }`) |
| `GET /api/admin/orders` | GET | جميع الطلبات `?status=&page=&limit=` |
| `GET /api/admin/refunds` | GET | طلبات الاسترداد |
| `PUT /api/admin/refunds/:id` | PUT | معالجة طلب استرداد (body: `{ status, reason? }`) |
| `GET /api/admin/reviews/flagged` | GET | المراجعات المبلغ عنها |
| `DELETE /api/admin/reviews/:id` | DELETE | حذف مراجعة مخالفة |
| `PUT /api/admin/reviews/:id/dismiss` | PUT | رفض البلاغ |
| `POST /api/admin/announcements` | POST | إرسال إعلان (body: `{ title, content, targetRole?, courseId? }`) |
| `GET /api/admin/media` | GET | ملفات الوسائط `?type=&page=&limit=` |
| `DELETE /api/admin/media/:id` | DELETE | حذف ملف وسائط |
| `GET /api/admin/activity-logs` | GET | سجل النشاطات `?from=&to=&actionType=` |
| `GET /api/admin/settings` | GET | إعدادات النظام |
| `PUT /api/admin/settings` | PUT | تحديث إعدادات النظام |
| `GET /api/admin/analytics` | GET | بيانات التحليلات `?from=&to=&metric=` |

---

### 9. عن المنصة (AboutContactPublic) — `/about`

#### البيانات المعروضة:
- **Hero**: عنوان، وصف، شعار
- **Stats**: 3 إحصائيات (أكثر من 200 مخطوطة، +10,000 متعلم، 14 عاماً)
- **Contact Form**: الاسم، البريد الإلكتروني، غرض المراسلة (اختيار من قائمة)، نص الرسالة
- **Contact Info**: البريد الإلكتروني للمنصة، رقم الهاتف، العنوان
- **Legal Modals**: نافذة منبثقة لكل من:
  - سياسة الخصوصية
  - شروط الاستخدام
  - سياسة استرداد الرسوم

#### البيانات المطلوبة:

| API | Method | الهدف |
|-----|--------|-------|
| `GET /api/about` | GET | معلومات عن المنصة |
| `POST /api/contact` | POST | إرسال رسالة الاتصال (body: `{ name, email, subject, message }`) |
| `GET /api/legal/:type` | GET | جلب المحتوى القانوني (privacy, terms, refund) |

---

### 10. إعدادات الملف الشخصي (ProfileSettings) — `/profile`

#### التبويبات (Tabs):
- **Personal**: الاسم الكامل، الاسم الأول، اسم العائلة، السيرة الذاتية، الصورة الرمزية، الجنس، تاريخ الميلاد، الدولة، المدينة، العنوان، الرمز البريدي
- **Phones**: إدارة أرقام الهواتف (إضافة/حذف/تعيين كافتراضي)
- **Addresses**: إدارة العناوين
- **Security**: كلمة المرور، جلسات تسجيل الدخول النشطة
- **Notifications**: تفضيلات الإشعارات (SMS، Email، Push، النشرة الأسبوعية)

#### البيانات المطلوبة:

| API | Method | الهدف |
|-----|--------|-------|
| `GET /api/profile` | GET | بيانات الملف الشخصي |
| `PUT /api/profile` | PUT | تحديث الملف الشخصي |
| `POST /api/profile/avatar` | POST | رفع صورة شخصية (FormData) |
| `GET /api/profile/phones` | GET | قائمة أرقام الهواتف |
| `POST /api/profile/phones` | POST | إضافة رقم هاتف |
| `PUT /api/profile/phones/:id` | PUT | تحديث رقم هاتف |
| `DELETE /api/profile/phones/:id` | DELETE | حذف رقم هاتف |
| `PUT /api/profile/phones/:id/default` | PUT | تعيين رقم كافتراضي |
| `GET /api/profile/addresses` | GET | قائمة العناوين |
| `POST /api/profile/addresses` | POST | إضافة عنوان |
| `PUT /api/profile/addresses/:id` | PUT | تحديث عنوان |
| `DELETE /api/profile/addresses/:id` | DELETE | حذف عنوان |
| `PUT /api/profile/password` | PUT | تغيير كلمة المرور (body: `{ currentPassword, newPassword }`) |
| `GET /api/profile/sessions` | GET | جلسات تسجيل الدخول النشطة |
| `DELETE /api/profile/sessions/:id` | DELETE | إنهاء جلسة |
| `GET /api/profile/notification-settings` | GET | تفضيلات الإشعارات |
| `PUT /api/profile/notification-settings` | PUT | تحديث تفضيلات الإشعارات |

---

### 11. الملف الشخصي العام (PublicProfile) — `/instructor/:name`

#### البيانات المعروضة:
- **Header**: شريط ثابت باسم المنصة
- **Instructor Card**: الصورة، الاسم، التخصص، الموقع، الإحصائيات (عدد الطلاب، التقييم، عدد الدورات)
- **السيرة الذاتية**: نبذة عن المدرب
- **قائمة الدورات**: دورات المدرب مع روابط سريعة

#### البيانات المطلوبة:

| API | Method | الهدف |
|-----|--------|-------|
| `GET /api/instructors/:name` | GET | بيانات المدرب العامة |
| `GET /api/instructors/:name/courses` | GET | دورات المدرب المنشورة |
| `POST /api/messages` | POST | إرسال رسالة للمدرب (body: `{ instructorId, message }`) |

---

## نظام المصادقة

**الوضع الحالي:** محاكاة كاملة (Simulated) عبر `localStorage` و `setTimeout`

### التخزين الحالي:
- **Token**: `localStorage.getItem('auth-token')` — محاكى
- **بيانات المستخدم المسجل**: `localStorage.getItem('athari_registered_user')`
- **مسودة التسجيل**: `athari_registration_draft` (تُحفظ تلقائياً)

### Interceptor الموجود في `api.ts`:
```typescript
// طلب: يضيف Bearer token من localStorage
// استجابة: عند 401 يمسح التوكن
```

### تدفق المصادقة المتوقع مع Backend حقيقي:

```
تسجيل الدخول:
  POST /api/auth/login
  ← يعيد { token, user }
  → نخزن token في localStorage
  → نخزن بيانات المستخدم في Context

تسجيل مستخدم جديد:
  POST /api/auth/register (مع بيانات 3 خطوات)
  ← يعيد { message, email }
  → نوجه إلى OTP verification

التحقق من البريد:
  POST /api/auth/verify-email { email, otp }
  ← يعيد { token, user }

التحقق من التوكن:
  GET /api/auth/me (مع Bearer token)
  ← يعيد بيانات المستخدم الحالي

تسجيل الخروج:
  → نمسح token من localStorage
  → نمسح بيانات المستخدم من Context
```

---

## localStorage Keys

### المصادقة والتسجيل:
| المفتاح | الغرض |
|---------|-------|
| `auth-token` | رمز المصادقة |
| `athari_registration_draft` | مسودة التسجيل المحفوظة تلقائياً |
| `athari_registered_user` | بيانات المستخدم المسجل الكاملة |

### الملف الشخصي:
| المفتاح | الغرض |
|---------|-------|
| `athari_fullName` | الاسم الكامل |
| `athari_firstName` | الاسم الأول |
| `athari_lastName` | اسم العائلة |
| `athari_bio` | السيرة الذاتية |
| `athari_avatarUrl` | رابط الصورة الرمزية |
| `athari_gender` | الجنس |
| `athari_dob` | تاريخ الميلاد |
| `athari_country` | الدولة |
| `athari_city` | المدينة |
| `athari_streetLine1` | عنوان الشارع |
| `athari_postalCode` | الرمز البريدي |
| `athari_email` | البريد الإلكتروني |
| `athari_phone` | رقم الهاتف |

### أرقام الهواتف والعناوين:
| المفتاح | الغرض |
|---------|-------|
| `athari_phones` | مصفوفة أرقام الهواتف (JSON) |
| `athari_addresses` | مصفوفة العناوين (JSON) |

### الإشعارات:
| المفتاح | الغرض |
|---------|-------|
| `athari_notif_smsLive` | إشعارات SMS للجلسات المباشرة |
| `athari_notif_emailManuscript` | إشعارات البريد الإلكتروني للمخطوطات |
| `athari_notif_pushAnnouncements` | إشعارات push للإعلانات |
| `athari_notif_weeklyDigest` | الملخص الأسبوعي |

### تقدم التعلم:
| المفتاح | الغرض |
|---------|-------|
| `athari_completed_lessons_{courseId}` | حالة إكمال الدروس لكل دورة (JSON) |
| `quiz_answers_{quizId}` | إجابات الاختبارات |

### المظهر:
| المفتاح | الغرض |
|---------|-------|
| `theme-mode` | الوضع (light/dark) |
| `theme-color` | لون السمة (gold/forest/graphite) |

---

## المكونات المشتركة

### Layout Components:

| المكون | الموقع | الوظيفة |
|--------|--------|---------|
| `Navbar` | `components/layout/Navbar.tsx` | شريط التنقل مع الشعار، الروابط، السلة، المظهر |
| `Footer` | `components/layout/Footer.tsx` | التذييل مع الروابط والمعلومات |
| `CartDrawer` | `components/layout/CartDrawer.tsx` | سلة التسوق المنزلقة (Slide-over) |
| `NotFound` | `components/layout/NotFound.tsx` | صفحة 404 |
| `ErrorBoundary` | `components/layout/ErrorBoundary.tsx` | حد الأخطاء |

### State Management:

| الطبقة | الأداة | النطاق |
|--------|--------|--------|
| Global State | React Context (`AppProvider`) | cart, auth, global toast, courses |
| Notifications | Zustand (`notificationStore`) | notifications[] |
| Server State | TanStack React Query (غير مستخدم حالياً) | — |
| Persistence | localStorage | theme, auth, profile, progress, quiz |

---

## TypeScript Types

### الأنواع الأساسية (src/types/index.ts):

```typescript
interface Course {
  id: string;
  title: string;
  category: string;
  categorySlug: string;
  instructorName: string;
  instructorAvatar: string;
  rating: number;
  studentsCount: number;
  price: number;       // 0 = Free
  originalPrice?: number;
  duration: string;    // e.g. "٢٤ ساعة"
  lessonsCount: number;
  thumbnail: string;
  progress?: number;   // 0-100
  nextLesson?: string;
  featured?: boolean;
}

interface Category {
  id: string;
  name: string;
  slug: string;
  iconName: string;
  courseCount: number;
}

interface LiveSession {
  id: string;
  title: string;
  instructor: string;
  date: string;
  time: string;
  duration: string;
}

interface Testimonial {
  id: string;
  name: string;
  role: string;
  content: string;
  avatar: string;
}

type ViewType = 'landing' | 'catalog' | 'dashboard' | 'auth' | 'course-details'
  | 'cart-checkout' | 'instructor-dashboard' | 'admin-dashboard' | 'about-contact'
  | 'profile-settings' | 'public-profile';

type AuthSubView = 'login' | 'register' | 'forgot' | 'verify' | 'reset' | 'success';
```

### الأنواع الإضافية المضمنة في الملفات:

| الملف | الأنواع |
|-------|---------|
| `notificationStore.ts` | `Notification`, `NotificationState` |
| `AppProvider.tsx` | `AppContextType` |
| `Navbar.tsx` | `MenuLink` |
| `CartDrawer.tsx` | `CartItemProps` (ضمني) |
| `InstructorDashboard.tsx` | `BadgeProps` |
| `CourseBuilder.tsx` | `Section`, `SectionItem`, `FormValues` |
| `LiveSession.tsx` | `ChatMessage` |
| `LearningRoom.tsx` | `LearningRoomProps` |
| `QuizTaking.tsx` | `QuizTakingProps`, `Question`, `Answer` |
| `MessagingCenter.tsx` | `Message`, `Conversation` |
| `ManuscriptCertificate.tsx` | `CertificateProps` |
| `WishlistRefunds.tsx` | `RefundRequest` |
| `InstructorApply.tsx` | `UploadedFile`, `InstructorApplyProps` |
| `AdminDashboard.tsx` | `CourseReview`, `TeacherRequest`, `OrderItem`, `RefundItem`, `Category`, `ActivityLog`, `SignalRNotification` |
| `ReviewsModeration.tsx` | `FlaggedReview` |
| `AnnouncementsCenter.tsx` | `Announcement` |
| `MediaLibrary.tsx` | `MediaFile` |
| `SystemActivitySettings.tsx` | `AuditLog`, `SystemSetting` |
| `ProfileSettings.tsx` | `PhoneItem`, `AddressItem`, `AuthSession`, `SettingsTab` |
| `AdvancedAnalytics.tsx` | `AnalyticsMetric`, `ChartDataPoint` |

---

## ملاحظات هامة للمطورين

### الوضع الحالي:
هذا المشروع هو **Frontend Prototype** بكل البيانات Mocked. لا يوجد أي Backend متصل حالياً.

### للربط مع Backend حقيقي:
1. **React Query**: جاهز للاستخدام (`queryClient` موجود في `lib/query-client.ts`)
2. **Axios instance**: جاهزة مع Interceptors (`lib/api.ts`) — تحتاج لـ `VITE_API_URL`
3. **SignalR**: مذكور في الـ Types (SignalRNotification في AdminDashboard) لكنه غير مطبق
4. **React Hook Form + Zod**: موجودة في `package.json` لكن غير مستخدمة (كل الفورم يدوية حالياً)

### متغيرات البيئة المطلوبة:
```bash
VITE_API_URL='http://localhost:3000/api'
VITE_APP_URL='http://localhost:5173'
VITE_GEMINI_API_KEY=''
```

### قائمة API Endpoints كاملة (للـ Backend):

```
AUTH:
  POST /api/auth/login
  POST /api/auth/register
  POST /api/auth/verify-email
  POST /api/auth/resend-otp
  POST /api/auth/forgot-password
  POST /api/auth/reset-password
  POST /api/auth/google
  GET  /api/auth/me

COURSES:
  GET    /api/courses
  GET    /api/courses/:id
  GET    /api/courses/:id/syllabus
  GET    /api/courses/:id/reviews
  POST   /api/courses/:id/reviews
  POST   /api/courses/:id/favorite
  GET    /api/categories
  GET    /api/courses/:id/progress
  PUT    /api/courses/:id/lessons/:lessonId/progress

CART & CHECKOUT:
  GET    /api/cart
  POST   /api/cart/add
  DELETE /api/cart/:courseId
  POST   /api/coupons/validate
  POST   /api/checkout

STUDENT:
  GET    /api/student/dashboard
  GET    /api/student/courses
  GET    /api/student/certificates
  GET    /api/student/favorites
  GET    /api/student/live-sessions
  POST   /api/student/instructor-apply
  POST   /api/student/refund-request
  GET    /api/quiz/:quizId
  POST   /api/quiz/:quizId/submit

INSTRUCTOR:
  GET    /api/instructor/dashboard
  GET    /api/instructor/courses
  POST   /api/instructor/courses
  PUT    /api/instructor/courses/:id
  DELETE /api/instructor/courses/:id
  POST   /api/instructor/courses/:id/publish
  GET    /api/instructor/earnings
  PUT    /api/instructor/courses/:id/lessons
  POST   /api/instructor/live-sessions

ADMIN:
  GET    /api/admin/dashboard
  GET    /api/admin/courses
  PUT    /api/admin/courses/:id/approve
  PUT    /api/admin/courses/:id/reject
  GET    /api/admin/teacher-requests
  PUT    /api/admin/teacher-requests/:id/approve
  PUT    /api/admin/teacher-requests/:id/reject
  GET    /api/admin/orders
  GET    /api/admin/refunds
  PUT    /api/admin/refunds/:id
  GET    /api/admin/reviews/flagged
  DELETE /api/admin/reviews/:id
  PUT    /api/admin/reviews/:id/dismiss
  POST   /api/admin/announcements
  GET    /api/admin/media
  DELETE /api/admin/media/:id
  GET    /api/admin/activity-logs
  GET    /api/admin/settings
  PUT    /api/admin/settings
  GET    /api/admin/analytics

PROFILE:
  GET    /api/profile
  PUT    /api/profile
  POST   /api/profile/avatar
  GET    /api/profile/phones
  POST   /api/profile/phones
  PUT    /api/profile/phones/:id
  DELETE /api/profile/phones/:id
  PUT    /api/profile/phones/:id/default
  GET    /api/profile/addresses
  POST   /api/profile/addresses
  PUT    /api/profile/addresses/:id
  DELETE /api/profile/addresses/:id
  PUT    /api/profile/password
  GET    /api/profile/sessions
  DELETE /api/profile/sessions/:id
  GET    /api/profile/notification-settings
  PUT    /api/profile/notification-settings

PUBLIC:
  GET    /api/instructors/:name
  GET    /api/instructors/:name/courses
  GET    /api/about
  POST   /api/contact
  GET    /api/legal/:type

NOTIFICATIONS:
  GET    /api/notifications
  PUT    /api/notifications/:id/read
  PUT    /api/notifications/read-all

MEDIA:
  POST   /api/media/upload
  DELETE /api/media/:id

MESSAGES:
  POST   /api/messages
  GET    /api/conversations
  GET    /api/conversations/:id/messages
  POST   /api/conversations/:id/messages
```
