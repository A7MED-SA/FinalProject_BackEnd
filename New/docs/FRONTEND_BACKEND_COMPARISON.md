# مقارنة شاملة: Frontend vs Backend — منصة آثاري

> تم إعداد هذه الوثيقة بعد استعراض كامل للـ Frontend (React/TypeScript) والـ Backend (.NET 9 ASP.NET Core Clean Architecture) ومقارنتها مع MODIFICATION_GUIDE.md
> تاريخ المقارنة: 23 يونيو 2026 — النسخة المعدلة (22 Service, 10 Frontend Improvements, 8 Backend Improvements)

---

## فهرس المحتويات

1. [نظرة عامة على حالة كل مشروع](#1)
2. [الفجوة الرئيسية: الـ Frontend لا يستخدم API](#2)
3. [مقارنة كل صفحة في الـ Frontend مع الـ Backend](#3)
4. [مقارنة الـ Types مع DTOs](#4)
5. [الخدمات الموجودة في الـ Backend وغير مستخدمة في الـ Frontend](#5)
6. [الخدمات المطلوبة في الـ Backend وغير موجودة](#6)
7. [توصيات التنفيذ حسب الأولوية](#7)
8. [خريطة طريق التكامل](#8)
9. [إحصائيات نهائية محدثة](#9)

> **آخر تحديث**: 23 يونيو 2026 — تم تحديث الأرقام (22 Service, 4 Shared Components, SignalR) بناءً على MODIFICATION_GUIDE.md

---

## 1. نظرة عامة على حالة كل مشروع <a id="1"></a>

### Frontend (React 19 + Vite 6 + Tailwind CSS 4)
| المجال | الحالة |
|--------|--------|
| **التوجيه (Routing)** | ✅ كامل (React Router v7) |
| **المكونات البصرية** | ✅ جميع الصفحات مصممة وجاهزة |
| **التفاعل (State)** | ⚠️ React Context + Zustand (دون API حقيقي) |
| **API Layer** | ❌ Axios instance موجود لكن غير مستخدم |
| **TanStack Query** | ❌ `QueryClientProvider` مربوط لكن `useQuery`/`useMutation` غير مستخدم |
| **نماذج البيانات** | ❌ Types بسيطة جداً ولا تطابق DTOs الـ Backend |
| **المصادقة** | ❌ محاكاة محلية فقط (localStorage, Context) |
| **التحقق من صحة النماذج** | ⚠️ react-hook-form + zod مثبتان لكن غير مستخدَمين |

### Backend (.NET 9 ASP.NET Core Clean Architecture)
| المجال | الحالة |
|--------|--------|
| **الـ API** | ✅ 100+ endpoint في 40+ Controller |
| **المصادقة** | ✅ JWT + Refresh Tokens + Sessions |
| **OAuth** | ✅ Google + Microsoft |
| **قاعدة البيانات** | ✅ Entity Framework Core + PostgreSQL |
| **التخزين** | ✅ MinIO (Presigned URLs) |
| **Real-time** | ✅ SignalR (Notifications + Messaging) |
| **المدفوعات** | ✅ Tap Payment Integration |
| **البريد الإلكتروني** | ✅ Email Service |
| **المهام الخلفية** | ✅ Background Jobs |
| **المراقبة** | ✅ Health Checks + OpenTelemetry |
| **Rate Limiting** | ✅ مثبت على كل الـ endpoints |
| **الأمان** | ✅ Security Headers, Anti-Forgery |

---

## 2. الفجوة الرئيسية <a id="2"></a>

### الـ Frontend لا يتواصل مع API حقيقي

```
src/lib/
├── api.ts            ← ✅ موجود (لكن غير مستورد في أي مكان)
└── query-client.ts   ← ✅ موجود (لكن useQuery غير مستخدم)

جميع الصفحات تستخدم:
src/data/index.ts     ← بيانات وهمية (Mock Data)
src/providers/AppProvider.tsx  ← منطق محاكاة (cart, auth)
src/stores/notificationStore.ts  ← إشعارات وهمية
```

**عدد ملفات Service مطلوبة**: 22 ملف (authService, courseService, cartService, paymentService, reviewService, liveSessionService, ...)
**عدد ملفات Service موجودة**: 0
**عدد ملفات Shared Components مطلوبة**: 4 (Pagination, Skeleton, ErrorFallback, EmptyState)
**SignalR Integration**: ❌ غير موجود (مطلوب: `lib/signalr.ts` + `hooks/useSignalR.ts`)

---

## 3. مقارنة كل صفحة مع الـ Backend <a id="3"></a>

### 3.1 LandingPage

| المكون/البيانات | المصدر الحالي (Frontend) | المصدر المطلوب (Backend) | الحالة في Backend |
|----------------|------------------------|-------------------------|-------------------|
| Hero Section | Mock data | Static (Frontend) أو GET /api/public/courses/stats | ✅ موجود (stats) |
| بطاقة إحصائيات | `data/index.ts` | `GET /api/public/courses/stats` | ✅ موجود |
| التصنيفات (Categories) | Mock | `GET /api/categories?pageSize=50` | ✅ موجود |
| الدورات المميزة (Featured) | Mock Courses | `GET /api/public/courses?isFeatured=true&pageSize=6` | ✅ موجود (Course.IsFeatured) |
| الجلسات المباشرة القادمة | Mock LiveSessions | `GET /api/live-sessions/upcoming` | ✅ موجود (LiveSessionManagementController) |
| التوصيات (Testimonials) | Mock Testimonials | **لا يوجد API مخصص** | ❌ مفقود في Backend |
| كيف تعمل المنصة (How it Works) | Mock | Static Frontend Content | ✅ لا يحتاج Backend |
| شركاء النجاح | Mock | Static Frontend Content | ✅ لا يحتاج Backend |

### 3.2 CourseCatalog

| المكون/البيانات | المصدر الحالي | المصدر المطلوب | الحالة في Backend |
|----------------|---------------|----------------|-------------------|
| قائمة الدورات مع تصفية | Mock Courses | `GET /api/public/courses?page={}&search={}&categoryId={}&sortBy={}&level={}&priceType={}&pageSize=12` | ✅ موجود (PublicCourseController.Search) |
| خيارات التصفية | Mock Categories | `GET /api/public/courses/filters/options` | ✅ موجود |
| التصنيفات Categories | Mock | `GET /api/categories?pageSize=50` | ✅ موجود |
| Pagination | ❌ غير مطبق | Query parameters: page, pageSize, totalCount | ✅ Backend يدعمه |

### 3.3 CourseDetails

| المكون/البيانات | المصدر الحالي | المصدر المطلوب | الحالة في Backend |
|----------------|---------------|----------------|-------------------|
| تفاصيل الدورة | Mock Course | `GET /api/public/courses/{id}` | ✅ موجود |
| المنهج (Syllabus) | Mock | `GET /api/management/courses/{id}/sections` (مع items) | ✅ موجود (SectionController) |
| متطلبات الدورة | Mock none | `GET /api/management/courses/{id}` → Course.Requirements | ✅ موجود (CourseRequirement entity) |
| مخرجات التعلم | Mock none | `GET /api/management/courses/{id}` → Course.LearningOutcomes | ✅ موجود (CourseLearningOutcome entity) |
| تقييمات الطلاب | Mock Reviews | `GET /api/reviews/course/{courseId}` | ✅ موجود (ReviewsController) |
| المدرب | Mock | `GET /api/profile/{userId}` | ✅ موجود |
| دورات مقترحة | Mock | `GET /api/public/courses/{id}/related` | ✅ موجود |
| الحجز (Enroll/Cart) | Mock Context | `POST /api/cart` / `POST /api/enrollments` | ✅ موجود (CartController, EnrollmentsController) |

### 3.4 CartCheckout

| المكون/البيانات | المصدر الحالي | المصدر المطلوب | الحالة في Backend |
|----------------|---------------|----------------|-------------------|
| عربة التسوق | Mock Context | `GET /api/cart` | ✅ موجود |
| إضافة/إزالة عنصر | Mock Context | `POST /api/cart/items` / `DELETE /api/cart/items/{id}` | ✅ موجود |
| تطبيق كوبون | Mock Context | `POST /api/coupons/validate` | ✅ موجود |
| الدفع | Mock | `POST /api/orders` → `POST /api/payments/process` | ✅ موجود (Orders + Payments) |
| طرق الدفع | Mock | `GET /api/payments/methods` | ✅ موجود |

### 3.5 AuthPage

| المكون/البيانات | المصدر الحالي | المصدر المطلوب | الحالة في Backend |
|----------------|---------------|----------------|-------------------|
| تسجيل الدخول | Mock | `POST /api/auth/login` | ✅ موجود |
| إنشاء حساب | Mock | `POST /api/auth/register` | ✅ موجود |
| Refresh Token | ❌ غير مطبق | `POST /api/auth/refresh` | ✅ موجود (غير مستخدم في Frontend) |
| توثيق البريد | Mock | `POST /api/auth/verify-email` | ✅ موجود |
| إعادة إرسال التوثيق | Mock | `POST /api/auth/resend-verification` | ✅ موجود |
| نسيت كلمة المرور | Mock | `POST /api/auth/forgot-password` | ✅ موجود |
| إعادة تعيين كلمة المرور | Mock | `POST /api/auth/reset-password` | ✅ موجود |
| تسجيل الدخول بـ Google | Mock | `GET /api/oauth/google` | ✅ موجود (غير مستخدم في Frontend) |
| تسجيل الدخول بـ Microsoft | Mock | `GET /api/oauth/microsoft` | ✅ موجود (غير مستخدم في Frontend) |

### 3.6 StudentDashboard

| المكون/البيانات | المصدر الحالي | المصدر المطلوب | الحالة في Backend |
|----------------|---------------|----------------|-------------------|
| نظرة عامة (إحصائيات) | Mock | `GET /api/student/dashboard/overview` | ✅ موجود |
| دوراتي المسجلة | Mock | `GET /api/enrollments/my-courses` | ✅ موجود |
| التقدم الأسبوعي | Mock | `GET /api/student/dashboard/weekly-activity` | ✅ موجود |
| الشهادات | Mock | `GET /api/certificates/my` | ✅ موجود |
| الإشعارات | Mock | `GET /api/notifications` + SignalR Hub | ✅ موجود (NotificationController + SignalR) |
| قائمة المفضلة | ❌ زر بدون صفحة | `GET /api/wishlist` | ✅ موجود (WishlistController) |
| طلب استرداد | Mock | `POST /api/refunds` | ✅ موجود (RefundsController) |
| المحادثات | Mock | `GET /api/messages/conversations` + SignalR | ✅ موجود (MessagesController + SignalR) |
| طلب انضمام كمدرب | Mock | `POST /api/instructor-requests/submit` | ✅ موجود (InstructorRequestController) |
| جلسات مباشرة مسجلة | Mock | `GET /api/enrollments/{id}/live-sessions` | ✅ موجود |

### 3.7 LearningRoom

| المكون/البيانات | المصدر الحالي | المصدر المطلوب | الحالة في Backend |
|----------------|---------------|----------------|-------------------|
| محتوى الدورة (فيديو/مستند) | Mock | `GET /api/courses/{id}/videos` + `GET /api/courses/{id}/documents` | ✅ موجود |
| تتبع التقدم | Mock | `PUT /api/enrollments/{id}/progress` | ✅ موجود |
| اكتمال الدرس | Mock | `POST /api/enrollments/{id}/mark-complete` | ✅ موجود |
| الاختبارات | Mock | `GET /api/courses/{id}/quizzes` → `POST /api/enrollments/{id}/quizzes/{id}/attempts/start` | ✅ موجود |
| مشغل الفيديو | وهمي (محاكي) | `GET /api/media/view-url?key={videoKey}` | ✅ موجود (MediaController.PresignedViewUrl) |

### 3.8 QuizTaking

| المكون/البيانات | المصدر الحالي | المصدر المطلوب | الحالة في Backend |
|----------------|---------------|----------------|-------------------|
| عرض الاختبار | Mock | `GET /api/courses/{id}/quizzes/{id}` | ✅ موجود |
| بدء المحاولة | Mock | `POST /api/enrollments/{id}/quizzes/{id}/attempts/start` | ✅ موجود |
| إرسال الإجابات | Mock | `POST /api/enrollments/{id}/quizzes/{id}/attempts/{id}/submit` | ✅ موجود |
| عرض النتيجة | Mock | `GET /api/enrollments/{id}/quizzes/{id}/attempts/{id}` | ✅ موجود |
| محاولات سابقة | Mock | `GET /api/enrollments/{id}/quizzes/{id}/attempts` | ✅ موجود |

### 3.9 MessagingCenter

| المكون/البيانات | المصدر الحالي | المصدر المطلوب | الحالة في Backend |
|----------------|---------------|----------------|-------------------|
| قائمة المحادثات | Mock | `GET /api/messages/conversations` | ✅ موجود |
| تفاصيل المحادثة | Mock | `GET /api/messages/conversations/{id}` | ✅ موجود |
| إرسال رسالة | Mock | `POST /api/messages/send` | ✅ موجود |
| عدد الرسائل غير المقروءة | Mock | `GET /api/messages/unread-count` | ✅ موجود |
| Real-time Chat | ❌ غير مطبق | SignalR Hub `/hubs/messaging` | ✅ موجود |

### 3.10 InstructorDashboard

| المكون/البيانات | المصدر الحالي | المصدر المطلوب | الحالة في Backend |
|----------------|---------------|----------------|-------------------|
| نظرة عامة | Mock | `GET /api/instructor/dashboard/overview` | ✅ موجود |
| دوراتي | Mock | `GET /api/management/courses` | ✅ موجود |
| الإيرادات | Mock | `GET /api/instructor/dashboard/revenue` | ✅ موجود |
| الطلاب | Mock | `GET /api/instructor/dashboard/students` | ✅ موجود |
| طلبات المراجعة المعلقة | Mock | `GET /api/instructor/dashboard/pending-requests` | ✅ موجود |
| آخر التقييمات | Mock | `GET /api/instructor/dashboard/recent-reviews` | ✅ موجود |

### 3.11 CourseBuilder

| المكون/البيانات | المصدر الحالي | المصدر المطلوب | الحالة في Backend |
|----------------|---------------|----------------|-------------------|
| إنشاء/تعديل دورة | Mock | `POST/PUT /api/management/courses` | ✅ موجود |
| رفع صورة الدورة | Mock | `POST /api/management/courses/{id}/image` | ✅ موجود |
| إدارة الأقسام (Sections) | Mock | `GET/POST/PUT/DELETE /api/management/courses/{id}/sections` | ✅ موجود |
| إعادة ترتيب الأقسام | Mock | `PUT /api/management/courses/{id}/sections/reorder` | ✅ موجود |
| إدارة محتوى الفيديو | Mock | `POST/PUT/DELETE /api/courses/{id}/videos` | ✅ موجود |
| إدارة المستندات | Mock | `POST/PUT/DELETE /api/courses/{id}/documents` | ✅ موجود |
| إدارة الاختبارات | Mock | `POST/PUT/DELETE /api/courses/{id}/quizzes` | ✅ موجود |
| إدارة أسئلة الاختبار | Mock | `POST/PUT/DELETE /api/courses/{id}/quizzes/{id}/questions` | ✅ موجود |
| إدارة المتطلبات | ❌ غير موجود في UI | `POST /api/management/courses/{id}/requirements` | ✅ موجود (لكن Frontend لا يدعمه) |
| إدارة مخرجات التعلم | ❌ غير موجود في UI | `POST /api/management/courses/{id}/outcomes` | ✅ موجود (لكن Frontend لا يدعمه) |
| تقديم للمراجعة | Mock | `POST /api/management/courses/{id}/submit-for-review` | ✅ موجود |

### 3.12 LiveSession (Instructor)

| المكون/البيانات | المصدر الحالي | المصدر المطلوب | الحالة في Backend |
|----------------|---------------|----------------|-------------------|
| قائمة الجلسات | Mock | `GET /api/courses/{id}/live-sessions` | ✅ موجود |
| إنشاء جلسة | Mock | `POST /api/courses/{id}/live-sessions` | ✅ موجود |
| تحديث الحالة | Mock | `PATCH /api/courses/{id}/live-sessions/{id}/status` | ✅ موجود |
| حضور الطلاب | Mock | `GET /api/live-sessions/{id}/attendance` | ✅ موجود |

### 3.13 AdminDashboard

| المكون/البيانات | المصدر الحالي | المصدر المطلوب | الحالة في Backend |
|----------------|---------------|----------------|-------------------|
| نظرة عامة | Mock | `GET /api/admin/dashboard/overview` | ✅ موجود |
| الإيرادات | Mock | `GET /api/admin/dashboard/revenue` | ✅ موجود |
| نمو المستخدمين | Mock | `GET /api/admin/dashboard/user-growth` | ✅ موجود |
| منحنى التسجيل | Mock | `GET /api/admin/dashboard/enrollment-trend` | ✅ موجود |
| أفضل الدورات | Mock | `GET /api/admin/dashboard/top-courses` | ✅ موجود |
| إدارة المستخدمين | ❌ غير موجود في UI | `GET /api/admin/users` + CRUD | ✅ موجود (AdminUsersController - غير مستخدم) |
| إدارة الـ Teacher Requests | Mock | `GET/POST /api/instructor-requests/pending` + `/process` | ✅ موجود |
| إدارة الطلبات | Mock | `GET /api/admin/courses/edit-requests` + `/review` | ✅ موجود (لكن Frontend لا يدعمه) |
| إدارة الكوبونات | ❌ غير موجود في UI | `GET/POST/PUT /api/admin/coupons` | ✅ موجود (غير مستخدم) |
| طرق الدفع | ❌ غير موجود في UI | `GET/POST /api/admin/payment-methods` | ✅ موجود (غير مستخدم) |
| سجلات المعاملات | ❌ غير موجود في UI | `GET /api/orders` | ✅ موجود (غير مستخدم بشكل كامل) |

### 3.14 AdvancedAnalytics

| المكون/البيانات | المصدر الحالي | المصدر المطلوب | الحالة في Backend |
|----------------|---------------|----------------|-------------------|
| تحليلات متقدمة | Mock | `GET /api/reports` | ✅ موجود (ReportsController) |
| تصدير | Mock | Backend likely supports CSV/JSON | ⚠️ يحتاج تأكيد |

### 3.15 MediaLibrary

| المكون/البيانات | المصدر الحالي | المصدر المطلوب | الحالة في Backend |
|----------------|---------------|----------------|-------------------|
| قائمة الوسائط | Mock | `GET /api/admin/media?page={}&search={}&type={}` | ✅ موجود |
| رفع وسائط | Mock | `POST /api/media/upload-url` (Presigned URL) | ✅ موجود |
| تأكيد الرفع | Mock | `POST /api/media/confirm-upload` | ✅ موجود |
| حذف ناعم/استعادة | Mock | `PUT /api/admin/media/{id}/soft-delete` + `/restore` | ✅ موجود |
| إحصائيات الوسائط | Mock | `GET /api/admin/media/stats` | ✅ موجود |

### 3.16 ReviewsModeration

| المكون/البيانات | المصدر الحالي | المصدر المطلوب | الحالة في Backend |
|----------------|---------------|----------------|-------------------|
| تقييمات معلقة | Mock | `GET /api/reviews/pending` | ✅ موجود |
| مراجعة تقييم | Mock | `PUT /api/reviews/{id}/moderate` | ✅ موجود |
| تقارير التقييمات | Mock | `POST /api/reports` + `GET /api/reports/pending` | ✅ موجود (ReportsController) |

### 3.17 AnnouncementsCenter

| المكون/البيانات | المصدر الحالي | المصدر المطلوب | الحالة في Backend |
|----------------|---------------|----------------|-------------------|
| إنشاء إعلان | Mock | `POST /api/announcements` | ✅ موجود |
| قائمة الإعلانات | Mock | `GET /api/announcements` | ✅ موجود |
| تحديث/إلغاء | Mock | `PUT/DELETE /api/announcements/{id}` | ✅ موجود |

### 3.18 SystemActivitySettings

| المكون/البيانات | المصدر الحالي | المصدر المطلوب | الحالة في Backend |
|----------------|---------------|----------------|-------------------|
| إعدادات النظام | Mock | `GET/PUT /api/system-settings` (grouped) | ✅ موجود |
| سجل النشاطات | Mock | `GET /api/activity-logs` | ✅ موجود |

### 3.19 ProfileSettings

| المكون/البيانات | المصدر الحالي | المصدر المطلوب | الحالة في Backend |
|----------------|---------------|----------------|-------------------|
| الملف الشخصي | Mock | `GET /api/profile/me` | ✅ موجود |
| تحديث الملف الشخصي | Mock | `PUT /api/profile/me` | ✅ موجود |
| تغيير الصورة | Mock | `POST /api/profile/picture` (مع FileId من Media) | ✅ موجود |
| إدارة الهواتف | Mock | `GET/POST/DELETE /api/profile/phones` | ✅ موجود |
| إدارة العناوين | Mock | `GET/POST/PUT/DELETE /api/profile/addresses` | ✅ موجود |
| تغيير كلمة المرور | Mock | `POST /api/auth/change-password` | ✅ موجود |
| الجلسات النشطة | Mock | `GET /api/auth/sessions` + `DELETE /api/auth/sessions/{id}` | ✅ موجود (لكن Frontend لا يظهرها) |
| إعدادات الإشعارات | Mock | `GET/PUT /api/notifications/settings` | ❌ غير واضح (يحتاج تأكيد) |

### 3.20 PublicProfile

| المكون/البيانات | المصدر الحالي | المصدر المطلوب | الحالة في Backend |
|----------------|---------------|----------------|-------------------|
| ملف المدرب | Mock | `GET /api/profile/{userId}` | ✅ موجود (لكن بالمُعرّف GUID وليس بالاسم كما في Frontend) |
| دورات المدرب | Mock | `GET /api/public/courses?instructorId={}` | ✅ موجود (بعد إضافة الفلتر) |

### 3.21 AboutContactPublic

| المكون/البيانات | المصدر الحالي | المصدر المطلوب | الحالة في Backend |
|----------------|---------------|----------------|-------------------|
| معلومات عن المنصة | Mock (static) | لا يوجد API مخصص | ❌ مفقود |
| نموذج تواصل | Mock | لا يوجد API مخصص | ❌ مفقود |
| صفحات قانونية | Mock (static) | لا يوجد API مخصص | ❌ مفقود |

---

## 4. مقارنة الـ Types مع DTOs <a id="4"></a>

### الـ Types الحالية في Frontend (`src/types/index.ts`)

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
  price: number;
  originalPrice?: number;
  duration: string;
  lessonsCount: number;
  thumbnail: string;
  progress?: number;
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
  duration: number;
}

interface Testimonial {
  id: string;
  name: string;
  role: string;
  content: string;
  avatar: string;
}
```

### المشاكل في الـ Types الحالية

1. **`Course`**:
   - الحقول بالعربية: `instructorName` → المطابق في Backend `instructor.fullName`
   - مفقود: `id` يجب أن يكون GUID (number في mock)
   - مفقود: `description`, `level`, `language`, `requirements`, `learningOutcomes`, `sectionsCount`, `totalDuration`
   - `category` string → Backend يعيد كائن Category مع id, name, slug
   - `rating` number → Backend يعيد كائن Rating مع average, count, distribution

2. **MISSING Types بالكامل**: لا توجد Types لـ:
   - Auth/User, Enrollment, CartItem, Order, Payment, Certificate
   - Quiz, QuizQuestion, QuizAttempt
   - Video content, Document content
   - Notification, Message, Conversation
   - Section, SectionItem
   - API Response envelopes (pagination, error)

### المقارنة مع Backend DTOs

| Frontend Type | Backend DTO | الفروقات |
|--------------|-------------|----------|
| `Course` | `CourseDto`, `CourseListDto`, `CourseDetailDto` | حقل `id` (GUID vs string), `category` (string vs CategoryDto), `rating` (number vs RatingDto), `instructorName` (string vs InstructorBriefDto) |
| `Category` | `CategoryDto` | متطابق تقريباً + `iconUrl`, `imageUrl` |
| `LiveSession` | `LiveSessionDto` | Frontend `instructor: string` vs Backend `instructor: InstructorBriefDto` |
| `Testimonial` | لا يوجد مقابل | غير موجود في Backend |
| — | `UserDto` | لم يُعرّف في Frontend |
| — | `EnrollmentDto` | لم يُعرّف في Frontend |
| — | `CartDto` | لم يُعرّف في Frontend |
| — | `OrderDto` | لم يُعرّف في Frontend |
| — | `QuizAttemptDto` | لم يُعرّف في Frontend |
| — | `PaginatedResult<T>` | لم يُعرّف في Frontend |
| — | `ApiResponse<T>` | لم يُعرّف في Frontend |

---

## 5. الخدمات الموجودة في الـ Backend وغير مستخدمة في الـ Frontend <a id="5"></a>

### خدمات غير مستخدمة بالكامل

| الخدمة | Controller | الأهمية |
|--------|-----------|---------|
| **OAuth (Google/Microsoft)** | `OAuthController` | 🔴 عالية — تسهيل التسجيل |
| **Refresh Token** | `AuthController.Refresh` | 🔴 عالية — استمرارية الجلسة |
| **إدارة الجلسات** | `AuthController.Sessions`, `LogoutAll` | 🟡 متوسطة |
| **إدارة المستخدمين (Admin)** | `AdminUsersController` | 🟡 متوسطة |
| **إدارة الكوبونات (Admin)** | `AdminCouponsController` | 🟡 متوسطة |
| **طرق الدفع (Admin)** | `AdminPaymentMethodsController` | 🟢 منخفضة |
| **سجلات المعاملات** | `OrdersController.List` (admin view) | 🟡 متوسطة |
| **تعليقات الفيديو** | `VideoCommentController` | 🟢 منخفضة |
| **تتبع الحضور للجلسات المباشرة** | `LiveAttendanceController` | 🟡 متوسطة |
| **إدارة المتطلبات والمخرجات** | `CourseManagementController` | 🟡 متوسطة |
| **طلب تعديل الدورة** | `AdminCourseController.EditRequests` | 🟡 متوسطة |
| **تقارير/إبلاغ** | `ReportsController` | 🟢 منخفضة |
| **التحقق من الشهادة بالرمز** | `CertificatesController.Verify` | 🟢 منخفضة |
| **SignalR للرسائل الفورية** | `MessagesHub`, `NotificationsHub` | 🟡 متوسطة |
| **حذف مجدول للدورات** | `CourseManagementController.ScheduleDeletion` | 🟢 منخفضة |

### خدمات موجودة في Frontend Documentation ولكن قد لا تكون دقيقة

| الـ API كما هو موثق في Frontend | الـ API الفعلي في Backend | الملاحظات |
|--------------------------------|--------------------------|-----------|
| `GET /api/instructors/:name` | `GET /api/profile/{userId:guid}` | Frontend يستخدم `name` أما Backend فيستخدم `GUID` |
| `POST /api/auth/:provider` (Google) | `GET /api/oauth/google` | Frontend يرسل POST مع provider، Backend يستقبل GET ويعيد توجيه |
| `GET /api/about` | — | غير موجود في Backend |
| `POST /api/contact` | — | غير موجود في Backend (يمكن استخدام Reports) |
| `GET /api/legal/:type` | — | غير موجود في Backend |

---

## 6. الخدمات المطلوبة في الـ Backend وغير موجودة <a id="6"></a>

### خدمات يحتاجها الـ Frontend لكنها غير موجودة في Backend

| الخدمة المطلوبة | الصفحة | البديل المقترح |
|----------------|--------|----------------|
| `GET /api/public/testimonials` | LandingPage | استخدام أعلى التقييمات (Reviews) مع isTestimonial flag، أو إنشاء Testimonial entity جديدة |
| `GET /api/public/landing` (مجموعة بيانات للصفحة الرئيسية) | LandingPage | Endpoint واحد يعيد stats + featured courses + categories + upcoming live sessions لتسريع التحميل |
| `POST /api/contact` | AboutContactPublic | إنشاء ContactMessage entity أو إعادة استخدام Reports مع نوع Contact |
| `GET /api/about` | AboutContactPublic | Static content من DB أو CMS |
| `GET /api/legal/:type` (privacy, terms, etc.) | AboutContactPublic | LegalPage entity في DB |
| `GET /api/public/instructors/{slug}` | PublicProfile | Backend يدعم فقط البحث بالـ GUID، يحتاج Slug للمدرب |
| `GET /api/notifications/settings` | ProfileSettings | إعدادات الإشعارات لكل مستخدم (NotificationPreference entity) |
| `PUT /api/notifications/settings` | ProfileSettings | تحديث إعدادات الإشعارات |

### تحسينات مقترحة للـ Backend

| الاقتراح | السبب |
|----------|-------|
| إضافة `slug` إلى Profile/User | للسماح بالوصول للملفات الشخصية عبر `/instructor/ahmed` بدلاً من GUID |
| إضافة Testimonial entity + Controller | بديل نظيف عن الاعتماد على Reviews |
| إضافة About/Legal endpoints | دعم الصفحات الثابتة عبر API |
| إضافة ContactMessage entity | تخزين رسائل التواصل بدلاً من الخلط مع Reports |
| إضافة NotificationPreference entity | السماح بتخصيص الإشعارات لكل مستخدم |
| تحسين Instructor search بـ slug | البحث عن المدرب بالاسم المميز (slug) |
| إضافة Landing aggregated endpoint | تحسين أداء الصفحة الرئيسية (تقليل عدد الـ Requests) |

---

## 7. توصيات التنفيذ حسب الأولوية <a id="7"></a>

### المرحلة 1: 🔴 أساسيات — ربط الـ Frontend بالـ Backend

| المهمة | الملفات المتأثرة | الجهد |
|--------|-----------------|-------|
| إنشاء `types/api.ts` مع جميع DTOs (23 قسم) | جديد | كبير |
| إنشاء 22 Service file | جديد | كبير |
| إنشاء SignalR (`lib/signalr.ts` + `hooks/useSignalR.ts`) | جديد | متوسط |
| إنشاء Shared Components (Pagination, Skeleton, ErrorFallback, EmptyState) | جديد | صغير |
| تحديث `src/lib/api.ts` بإدارة التوكين الكاملة (Refresh) + معالجة الأخطاء | `api.ts` | صغير |
| ربط صفحة Auth بالـ API (مع OAuth POST) | `AuthPage.tsx` | متوسط |
| ربط صفحة CourseCatalog بالـ API | `CourseCatalog.tsx` | كبير |
| ربط صفحة CourseDetails بالـ API | `CourseDetails.tsx` | متوسط |

### المرحلة 2: 🟡 الميزات الرئيسية

| المهمة | الملفات المتأثرة | الجهد |
|--------|-----------------|-------|
| ربط StudentDashboard | `StudentDashboard.tsx` | كبير |
| ربط LearningRoom | `LearningRoom.tsx`, `QuizTaking.tsx` | كبير |
| ربط CartCheckout | `CartCheckout.tsx` | كبير |
| ربط InstructorDashboard | `InstructorDashboard.tsx`, `CourseBuilder.tsx` | كبير |
| إنشاء باقي Service files (22 total) + Hooks + SignalR | جديد | كبير |

### المرحلة 3: 🟢 الميزات الثانوية وتحسينات Backend

| المهمة | الجهد |
|--------|-------|
| ربط AdminDashboard (كل التبويبات) | كبير |
| ربط MessagingCenter (مع SignalR) | كبير |
| ربط ProfileSettings | متوسط |
| ربط باقي الصفحات (About, PublicProfile, إلخ) | متوسط |
| إضافة الـ endpoints المفقودة في Backend | متوسط |
| إضافة Testimonial API | صغير |
| إضافة About/Legal/Contact APIs | صغير |
| إضافة الـ Notification Preference في Backend | صغير |
| إضافة الـ Instructor Slug في Backend | صغير |

---

## 8. خريطة طريق التكامل <a id="8"></a>

### الخطوة 1: توحيد الواجهات (Types)
- إنشاء `src/types/api.ts` يحتوي على جميع DTOs المطابقة لـ Backend
- تحديث `src/types/index.ts` لاستخدام DTOs الجديدة
- إضافة أنواع Pagination (`PaginatedResult<T>`, `ApiResponse<T>`)

### الخطوة 2: إنشاء Service Layer (22 ملف)
- `src/services/auth.service.ts` — مصادقة + OAuth + جلسات
- `src/services/course.service.ts` — دورات عامة + بحث
- `src/services/category.service.ts` — تصنيفات CRUD
- `src/services/cart.service.ts` — عربة التسوق
- `src/services/order.service.ts` — طلبات الشراء
- `src/services/payment.service.ts` — معالجة الدفع
- `src/services/enrollment.service.ts` — التسجيل + التقدم
- `src/services/quiz.service.ts` — الاختبارات + المحاولات
- `src/services/certificate.service.ts` — الشهادات + التحقق
- `src/services/notification.service.ts` — الإشعارات
- `src/services/message.service.ts` — المحادثات
- `src/services/profile.service.ts` — الملف الشخصي + هواتف + عناوين
- `src/services/wishlist.service.ts` — المفضلة
- `src/services/review.service.ts` — التقييمات + الإبلاغ
- `src/services/media.service.ts` — رفع الملفات (Presigned URL)
- `src/services/liveSession.service.ts` — الجلسات المباشرة
- `src/services/announcement.service.ts` — الإعلانات
- `src/services/instructorRequest.service.ts` — طلبات المدربين
- `src/services/dashboard.service.ts` — لوحات التحكم
- `src/services/admin.service.ts` — إدارة Admin (كوبونات + طرق دفع + استردادات)
- `src/services/publicInstructor.service.ts` — ملف المدرب العام (slug)
- `src/services/public.service.ts` — Landing + Testimonials + Contact + About + Legal

### الخطوة 3: SignalR + Shared Components
- `src/lib/signalr.ts` — الاتصال بلـ Hubs (Notifications + Messaging)
- `src/hooks/useSignalR.ts` — ربط SignalR مع notificationStore
- `src/components/shared/Pagination.tsx` — تنقل بين الصفحات
- `src/components/shared/Skeleton.tsx` — حالات التحميل
- `src/components/shared/ErrorFallback.tsx` — عرض الأخطاء مع Retry
- `src/components/shared/EmptyState.tsx` — حالة عدم وجود بيانات

### الخطوة 4: إنشاء Custom Hooks (React Query)
- 16 Hook ملف: `useAuth`, `useCourses`, `useCategories`, `useCart`, `useOrders`, `usePayment`, `useEnrollments`, `useQuiz`, `useCertificates`, `useNotifications`, `useMessages`, `useProfile`, `useDashboard`, `useReview`, `useLiveSession`, `useAnnouncement`
- استخدام `useQuery` للقراءة و `useMutation` للكتابة

### الخطوة 5: ربط الصفحات تدريجياً
- البدء بصفحة Auth (أولوية قصوى لاستمرارية الجلسة)
- ثم CourseCatalog + CourseDetails (عرض البيانات)
- ثم Student/Instructor/Admin Dashboards
- وأخيراً الصفحات الثانوية

### الخطوة 6: إضافة الميزات المفقودة
- OAuth buttons حقيقية
- إدارة الجلسات النشطة
- إدارة الكوبونات (Admin)
- إدارة طرق الدفع (Admin)
- صفحة إدارة المستخدمين (Admin)
- إعدادات الإشعارات
- Testimonials من الـ Backend
- صفحات About/Legal من API

---

## 9. إحصائيات نهائية محدثة <a id="9"></a>

| البند | العدد | الحالة |
|-------|-------|--------|
| صفحات Frontend | 21 صفحة | ✅ مصممة كلها |
| Controllers في Backend | 40+ | ✅ موجودة |
| Endpoints في Backend | 100+ | ✅ موجودة |
| Entities في Backend | 40+ | ✅ موجودة |
| ملفات Service مطلوبة | **22** | ❌ لا يوجد (كان 14 في الخطة السابقة) |
| ملفات Hooks مطلوبة (React Query) | **16** | ❌ لا يوجد |
| ملفات Shared Components | **4** | ❌ لا يوجد |
| SignalR Integration | **2** (lib + hook) | ❌ لا يوجد |
| ملفات Types | 1 (23 قسم DTOs) | ❌ موجود 4 أنواع فقط |
| Endpoints مفقودة في Backend | **8** | 🟡 قيد الإضافة (راجع BACKEND_PLAN.md) |
| إضافات بنيوية Frontend | **10** | 🟡 قيد الإضافة (راجع FRONTEND_PLAN.md §10) |
| إضافات بنيوية Backend | **8** | 🟡 قيد الإضافة (راجع BACKEND_PLAN.md §4) |
