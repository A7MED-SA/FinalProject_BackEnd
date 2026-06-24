# Athary Platform - Project Documentation

**Athary LMS** (منصة آثاري التعليمية) — نظام إدارة التعلم (Learning Management System)

---

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Tech Stack](#2-tech-stack)
3. [Architecture](#3-architecture)
4. [Project Structure](#4-project-structure)
5. [Database & Entities](#5-database--entities)
6. [Enums](#6-enums)
7. [Authentication & Authorization](#7-authentication--authorization)
8. [All Endpoints (API Reference)](#8-all-endpoints-api-reference)
9. [Services Layer](#9-services-layer)
10. [Real-time (SignalR)](#10-real-time-signalr)
11. [Background Workers](#11-background-workers)
12. [Infrastructure Services](#12-infrastructure-services)
13. [Configuration](#13-configuration)
14. [Docker & Deployment](#14-docker--deployment)
15. [Running the Project](#15-running-the-project)

---

## 1. Project Overview

Athary is an Arabic educational platform built with .NET 9 Clean Architecture. It supports:

- **Course Management**: Create, structure, publish, and manage courses with sections, videos, documents, quizzes
- **Commerce**: Shopping cart, coupons, orders, payments, refunds, wishlists
- **Enrollment & Learning**: Student enrollment, progress tracking, quiz attempts, grading
- **Live Sessions**: Scheduled live sessions with attendance tracking
- **Reviews & Certificates**: Course reviews, auto-generated certificates with verification codes
- **Communication**: Internal messaging, announcements, system settings, content reporting
- **Dashboards**: Student, Instructor, and Admin dashboards with analytics
- **Media**: MinIO-based object storage for files, images, videos, documents
- **Authentication**: JWT + Google OAuth + Microsoft OAuth

---

## 2. Tech Stack

| Component | Technology | Version |
|-----------|-----------|---------|
| Runtime | .NET | 9.0 |
| Language | C# | latest |
| Database | SQL Server | 2022 |
| ORM | Entity Framework Core | 9.0.10 |
| Auth | ASP.NET Core Identity + JWT | 9.0 |
| OAuth | Google + Microsoft | — |
| Object Storage | MinIO (S3-compatible) | 7.0.0 |
| Real-time | SignalR | — |
| API Docs | NSwag / OpenAPI | 14.6.1 |
| Validation | FluentValidation | 12.1.1 |
| Object Mapping | Mapster | 7.4.0 |
| PDF Generation | QuestPDF | 2026.5.0 |
| Logging | Serilog (Console + Seq) | 4.2.0 |
| Observability | OpenTelemetry | 1.16.0 |
| Caching | Redis + In-Memory | — |
| Messaging | MassTransit Abstractions | 8.2.5 |

---

## 3. Architecture

**Clean Architecture** with 4 layers:

```
┌─────────────────────────────────────┐
│           Athary.API                │  ← Entry point, Controllers, Middleware
├─────────────────────────────────────┤
│        Athary.Application           │  ← DTOs, Interfaces, Validators, Mappings
├─────────────────────────────────────┤
│        Athary.Infrastructure        │  ← Services, Data (EF Core), Repositories, Hubs
├─────────────────────────────────────┤
│          Athary.Domain              │  ← Entities, Enums, Domain Interfaces
└─────────────────────────────────────┘
```

**Dependencies**:
- Domain → No dependencies
- Application → Domain
- Infrastructure → Application
- API → Application + Infrastructure

---

## 4. Project Structure

```
New/
├── src/
│   ├── Athary.Domain/
│   │   ├── Entities/          (57 entities)
│   │   ├── Enums/             (37 enums)
│   │   └── Interfaces/        (IRepository, IUnitOfWork)
│   │
│   ├── Athary.Application/
│   │   ├── Common/            (ApiResponse, PagedList)
│   │   ├── DTOs/              (54 DTO files organized by feature)
│   │   ├── Interfaces/        (47 service interfaces)
│   │   ├── Mappings/          (Mapster mappings)
│   │   └── Validators/        (FluentValidation validators)
│   │
│   ├── Athary.Infrastructure/
│   │   ├── Data/              (ApplicationDbContext, DbSeeder, Migrations)
│   │   ├── Services/          (41 service implementations)
│   │   ├── Hubs/              (NotificationHub, MessageHub)
│   │   ├── Helpers/           (EnrollmentGuard)
│   │   ├── HealthChecks/      (Database, Liveness)
│   │   ├── Repositories/      (GenericRepository, UnitOfWork)
│   │   ├── Settings/          (JWT, Email, MinIO, Redis settings)
│   │   └── Workers/           (Background services)
│   │
│   └── Athary.API/
│       ├── Controllers/       (50+ controllers)
│       ├── Middleware/        (ExceptionMiddleware, SecurityHeaders)
│       ├── Program.cs         (Application setup)
│       └── appsettings.json   (Configuration)
│
├── infrastructure/
│   ├── Dockerfile             (Multi-stage Alpine build)
│   └── docker-compose.yml     (SQL Server, MinIO, Mailpit, Seq)
│
└── run.sh                     (Development run script)
```

---

## 5. Database & Entities

### 5.1 Core Entities

| Entity | Description | Key Properties |
|--------|-------------|----------------|
| **User** | Extends IdentityUser\<Guid\> | FirstName, LastName, Slug, Bio, IsActive, RevenueSharePercentage |
| **Role** | Extends IdentityRole\<Guid\> | Description, IsActive |
| **Session** | User auth sessions | RefreshTokenHash, IpAddress, UserAgent, ExpiresAt |
| **Address** | User addresses | StreetLine1, City, PostalCode, Country, IsDefault |
| **Permission** | RBAC permissions | Name, Resource, IsActive |
| **RolePermission** | Role-Permission mapping | RoleId, PermissionId |

### 5.2 Course System

| Entity | Description | Key Properties |
|--------|-------------|----------------|
| **Category** | Course categories (hierarchical) | Name, Slug, ParentCategoryId, Position |
| **Course** | Main course entity | Title, Slug, Price, Status, Level, Language, Version |
| **CourseRequirement** | Course prerequisites | Description, DisplayOrder |
| **CourseLearningOutcome** | What students will learn | Description, DisplayOrder |
| **Section** | Course sections | Title, Position, IsLocked |
| **SectionItem** | Polymorphic content link | ItemType (Video/Quiz/Document/LiveSession), ItemId, IsMandatory |
| **CourseEditRequest** | Edit approval workflow | RequestType, Operation, JsonPayload, Status, IsEmergency |
| **CourseLog** | Course audit trail | Action, Details, Timestamp |

### 5.3 Content Entities

| Entity | Description | Key Properties |
|--------|-------------|----------------|
| **Video** | Video content | Title, VideoFileId, DurationSeconds |
| **Document** | Document content | Title, FileId, DownloadCount |
| **Quiz** | Quiz assessment | Title, DurationMinutes, PassingScorePercent, MaxAttempts |
| **Question** | Quiz questions | QuestionText, Type, Points, Position |
| **Option** | Question options | OptionText, IsCorrect, Position |

### 5.4 Learning & Enrollment

| Entity | Description | Key Properties |
|--------|-------------|----------------|
| **Enrollment** | Student course enrollment | Status, ProgressPercentage, Source, CertificateId |
| **ContentProgress** | Per-item progress | ContentType, ContentId, IsCompleted, WatchTimeSeconds |
| **QuizAttempt** | Quiz attempt record | Score, MaxScore, Status, AttemptNumber, TimeTakenSeconds |
| **UserAnswer** | Individual quiz answer | SelectedOptionId |

### 5.5 Commerce

| Entity | Description | Key Properties |
|--------|-------------|----------------|
| **Cart** | Shopping cart | UserId, SessionId, ExpiresAt |
| **CartItem** | Cart item | CourseId, PriceSnapshot |
| **Order** | Purchase order | OrderNumber, SubtotalAmount, DiscountAmount, FinalAmount, Status |
| **OrderItem** | Order line item | CourseId, PriceAtPurchase |
| **Payment** | Payment record | Amount, Currency, Status, TransactionRef, GatewayResponse |
| **PaymentMethod** | Available payment methods | Name, Provider, Type, IsActive, Configuration |
| **Refund** | Refund request | Amount, Reason, Status, ProcessedAt |
| **TransactionLog** | Payment audit trail | — |
| **Coupon** | Discount coupons | Code, Type, Value, UsageLimit, ValidFrom/Until |
| **CouponCourse** | Coupon-Course mapping | — |
| **CouponUsage** | Coupon usage tracking | UserId, OrderId, UsedAt |
| **Wishlist** | User wishlists | CourseId |

### 5.6 Media

| Entity | Description | Key Properties |
|--------|-------------|----------------|
| **UploadedFile** | File metadata | FileName, FilePath, ContentType, FileSize, IsDeleted |
| **Certificate** | Course completion cert | VerificationCode, Status, IssuedAt, CompletedAt |
| **Review** | Course reviews | Rating, Comment, Status, IsFlagged, HelpfulCount |
| **ReviewHelpful** | Review helpful votes | IsHelpful |

### 5.7 Communication

| Entity | Description | Key Properties |
|--------|-------------|----------------|
| **Message** | Internal messages | SenderId, ReceiverId, Content, IsRead, IsDeleted |
| **Notification** | User notifications | Type, Title, Message, IsRead, LinkUrl |
| **NotificationPreference** | Per-user notification settings | EmailNotifications, PushNotifications, CourseUpdates, etc. |
| **Announcement** | Platform announcements | Title, Content, Target, CourseId, IsActive |
| **ContactMessage** | Contact form submissions | FullName, Email, Phone, Subject, Message, IsRead |
| **Report** | Content reports | EntityType, EntityId, Reason, Status, AdminNote |
| **ActivityLog** | Audit trail | Action, EntityType, EntityId, IpAddress |
| **SystemSetting** | Key-value settings | Key, Value, DataType, SettingGroup, IsPublic |

### 5.8 Live Sessions

| Entity | Description | Key Properties |
|--------|-------------|----------------|
| **LiveSession** | Scheduled live sessions | Title, MeetingUrl, Status, ScheduledStart/End |
| **LiveAttendance** | Session attendance | JoinedAt, LeftAt, DurationMinutes |

### 5.9 Other

| Entity | Description | Key Properties |
|--------|-------------|----------------|
| **LegalPage** | Legal content pages | Type (privacy/terms/refund), Title, Content, IsPublished |
| **Testimonial** | Student testimonials | Content, Rating, IsApproved, IsFlagged, DisplayOrder |
| **InstructorRequest** | Instructor application | Status, Message, AdminNotes, RejectionReason |
| **InstructorRequestDocument** | Application documents | DocumentType, FileId, UrlValue |

---

## 6. Enums

| Enum | Values |
|------|--------|
| **CourseStatus** | Draft, PendingReview, Published, Rejected, Archived |
| **CourseLevel** | Beginner, Intermediate, Advanced |
| **CourseLanguage** | Ar, En |
| **EnrollmentStatus** | InProgress, Completed, Expired, Refunded |
| **EnrollmentSource** | Purchase, Free, Admin, Referral |
| **ContentType** | Video, Document, Quiz |
| **SectionItemType** | Video, Quiz, Document, LiveSession |
| **QuizAttemptStatus** | InProgress, Submitted, AutoSubmitted, Expired |
| **QuestionType** | MultipleChoice, TrueFalse, ShortAnswer |
| **OrderStatus** | Pending, Processing, Completed, Cancelled, Refunded |
| **PaymentStatus** | Pending, Completed, Failed, Refunded, Cancelled |
| **PaymentCurrency** | EGP, USD |
| **PaymentMethodType** | CreditCard, DebitCard, BankTransfer, Wallet, Cash |
| **RefundStatus** | Requested, Approved, Rejected, Processed |
| **CouponType** | Percentage, FixedAmount |
| **CouponApplicableTo** | All, SpecificCourses, FirstPurchase |
| **CertificateStatus** | Valid, Revoked, Expired |
| **ReviewStatus** | Pending, Approved, Rejected, Flagged |
| **LiveSessionStatus** | Scheduled, Live, Finished, Cancelled |
| **InstructorRequestStatus** | Pending, Approved, Rejected |
| **DocumentType** | NationalId, Certificate, Other |
| **EditRequestType** | LowRisk, HighRisk |
| **EditOperation** | Create, Update, Delete |
| **EditRequestStatus** | Pending, Approved, Rejected, Expired |
| **ReportEntityType** | Course, Review, Message |
| **ReportReason** | Inappropriate, Spam, Copyright, Harassment, Other |
| **ReportStatus** | Pending, Dismissed, ActionTaken |
| **NotificationType** | Course, Enrollment, Payment, System, Achievement |
| **AnnouncementTarget** | All, Students, Instructors, SpecificCourse |
| **ActivityLogEntityType** | User, Course, Category, Order, Payment, Certificate, Message, Announcement, SystemSetting, Report |
| **CourseLogAction** | Created, Updated, Published, Rejected, Deleted, etc. |
| **SettingDataType** | String, Int, Bool, Json, Date |
| **Gender** | Male, Female |
| **TransactionLogStatus** | Success, Failed, Pending |

---

## 7. Authentication & Authorization

### 7.1 Authentication Methods

| Method | Endpoint | Description |
|--------|----------|-------------|
| Email/Password | `POST /api/auth/login` | Standard login |
| Register | `POST /api/auth/register` | New account creation |
| Google OAuth | `POST /api/oauth/google` | Google social login |
| Microsoft OAuth | `POST /api/oauth/microsoft` | Microsoft social login |

### 7.2 Token System

- **Access Token**: JWT, 60 minutes expiration
- **Refresh Token**: 7 days expiration, stored as hash in Session table
- **Token Refresh**: `POST /api/auth/refresh`

### 7.3 Roles

| Role | Description |
|------|-------------|
| **Admin** | Full platform access, user management, course approval, content moderation |
| **Instructor** | Create/manage courses, view analytics, manage live sessions |
| **Student** | Enroll in courses, take quizzes, write reviews, purchase courses |

### 7.4 Authorization Patterns

```csharp
[Authorize]                    // Any authenticated user
[Authorize(Roles = "Admin")]   // Admin only
[Authorize(Roles = "Instructor")] // Instructor only
[AllowAnonymous]               // Public access
```

---

## 8. All Endpoints (API Reference)

> **Base URL**: `http://localhost:5000`
> **Content-Type**: `application/json` (for all POST/PUT/PATCH requests)
> **Auth**: Bearer token in `Authorization: Bearer <token>` header

### 8.0 Response Envelope

All API responses follow this wrapper:

```json
{
  "success": true,
  "data": { ... },
  "message": "Optional success message",
  "errors": null
}
```

Error example:
```json
{
  "success": false,
  "data": null,
  "message": "Validation failed",
  "errors": ["Email is required", "Password must be at least 6 characters"]
}
```

---

### 8.1 Authentication (`/api/auth`)

#### `POST /api/auth/register`
Create a new user account. Returns a verification code sent to email.

**Request Body:**
```json
{
  "firstName": "أحمد",
  "lastName": "محمد",
  "email": "ahmed@example.com",
  "password": "Admin@123456",
  "confirmPassword": "Admin@123456",
  "gender": "Male",
  "dateOfBirth": "1995-06-15",
  "phoneNumber": "+201234567890",
  "country": "Egypt",
  "city": "Cairo",
  "streetLine1": "123 Main St",
  "postalCode": "12345"
}
```
> Only `firstName`, `lastName`, `email`, `password`, `confirmPassword` are required. All others are optional.

**Response (200):**
```json
{
  "success": true,
  "data": {
    "userId": "guid",
    "email": "ahmed@example.com"
  },
  "message": "Registration successful. Please check your email for verification code."
}
```

---

#### `POST /api/auth/login`
Authenticate with email/password. Returns JWT tokens.

**Request Body:**
```json
{
  "email": "ahmed@example.com",
  "password": "Admin@123456",
  "rememberMe": false
}
```

**Response (200):**
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "random-refresh-token-string",
    "sessionId": "guid",
    "expiresAt": "2026-06-24T22:00:00Z",
    "user": {
      "id": "guid",
      "email": "ahmed@example.com",
      "fullName": "أحمد محمد",
      "profilePictureUrl": null,
      "isActive": true,
      "emailConfirmed": true,
      "roles": ["Student"]
    }
  }
}
```

---

#### `POST /api/auth/refresh`
Refresh an expired access token using a refresh token.

**Request Body:**
```json
{
  "refreshToken": "random-refresh-token-string"
}
```

**Response (200):** Same as login response (new accessToken + refreshToken).

---

#### `POST /api/auth/verify-email`
Verify email with the code sent during registration.

**Request Body:**
```json
{
  "email": "ahmed@example.com",
  "code": "123456"
}
```

---

#### `POST /api/auth/resend-verification`
Resend the verification code.

**Request Body:**
```json
{
  "email": "ahmed@example.com"
}
```

---

#### `POST /api/auth/forgot-password`
Request a password reset code.

**Request Body:**
```json
{
  "email": "ahmed@example.com"
}
```

---

#### `POST /api/auth/reset-password`
Reset password using the code from forgot-password.

**Request Body:**
```json
{
  "email": "ahmed@example.com",
  "code": "123456",
  "newPassword": "NewPass@123",
  "confirmPassword": "NewPass@123"
}
```

---

#### `POST /api/auth/change-password` 🔒
Change password for authenticated user.

**Request Body:**
```json
{
  "currentPassword": "Admin@123456",
  "newPassword": "NewPass@123",
  "confirmPassword": "NewPass@123"
}
```

---

#### `POST /api/auth/logout` 🔒
Logout current session.

**No request body.**

---

#### `POST /api/auth/logout-all` 🔒
Logout all sessions except current.

**No request body.**

---

#### `GET /api/auth/sessions` 🔒
List all active sessions for current user.

**Response (200):**
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "ipAddress": "192.168.1.1",
      "userAgent": "Mozilla/5.0...",
      "createdAt": "2026-06-24T10:00:00Z",
      "expiresAt": "2026-07-01T10:00:00Z"
    }
  ]
}
```

> 🔒 = Requires `Authorization: Bearer <token>` header

---

### 8.2 OAuth (`/api/oauth`)

#### `POST /api/oauth/google`
Login/register via Google OAuth.

**Request Body:**
```json
{
  "idToken": "google-id-token-string"
}
```

**Response:** Same as login response.

#### `POST /api/oauth/microsoft`
Login/register via Microsoft OAuth.

**Request Body:**
```json
{
  "idToken": "microsoft-id-token-string"
}
```

---

### 8.3 Profile (`/api/profile`)

#### `GET /api/profile/me` 🔒
Get current user's full profile.

**Response (200):**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "fullName": "أحمد محمد",
    "email": "ahmed@example.com",
    "bio": "طالب في منصة آثاري",
    "gender": "Male",
    "dateOfBirth": "1995-06-15",
    "nationality": "Egyptian",
    "profileImageUrl": "https://minio.../images/photo.jpg",
    "createdAt": "2026-01-01T00:00:00Z",
    "phones": [
      {
        "id": "guid",
        "phoneNumber": "+201234567890",
        "type": "Primary",
        "isVerified": true,
        "isDefault": true
      }
    ],
    "addresses": [
      {
        "id": "guid",
        "type": "Home",
        "streetLine1": "123 Main St",
        "streetLine2": null,
        "city": "Cairo",
        "stateProvince": null,
        "postalCode": "12345",
        "country": "Egypt",
        "contactPhone": "+201234567890",
        "isDefault": true
      }
    ]
  }
}
```

#### `GET /api/profile/{userId}`
Get public profile (no auth required).

#### `PUT /api/profile` 🔒
Update profile fields.

**Request Body (all optional):**
```json
{
  "firstName": "أحمد",
  "lastName": "علي",
  "bio": "مطور .NET",
  "gender": "Male",
  "dateOfBirth": "1995-06-15",
  "nationality": "Egyptian"
}
```

#### `POST /api/profile/picture` 🔒
Set profile picture from uploaded file.

**Request Body:**
```json
{
  "fileId": "guid-of-uploaded-file"
}
```

#### `DELETE /api/profile/picture` 🔒
Remove profile picture.

#### `POST /api/profile/phones` 🔒
**Request Body:**
```json
{
  "phoneNumber": "+201234567890",
  "type": "Primary",
  "isDefault": true
}
```

#### `DELETE /api/profile/phones/{phoneId}` 🔒
#### `PUT /api/profile/phones/{phoneId}/default` 🔒

#### `POST /api/profile/addresses` 🔒
**Request Body:**
```json
{
  "type": "Home",
  "streetLine1": "123 Main St",
  "streetLine2": "Apt 4",
  "city": "Cairo",
  "stateProvince": "Cairo Governorate",
  "postalCode": "12345",
  "country": "Egypt",
  "contactPhone": "+201234567890",
  "isDefault": true
}
```

#### `PUT /api/profile/addresses/{addressId}` 🔒
#### `DELETE /api/profile/addresses/{addressId}` 🔒
#### `PUT /api/profile/addresses/{addressId}/default` 🔒

---

### 8.4 Categories (`/api/categories`)

#### `GET /api/categories`
**Response (200):**
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "name": "برمجة",
      "slug": "programming",
      "description": "دورات البرمجة",
      "imageUrl": "https://minio.../images/cat.jpg",
      "parentId": null,
      "position": 1,
      "courseCount": 15
    }
  ]
}
```

#### `GET /api/categories/{id}`
#### `POST /api/categories` 🔒Admin
**Request Body:**
```json
{
  "name": "تصميم",
  "slug": "design",
  "description": "دورات التصميم",
  "parentId": null,
  "position": 2
}
```
#### `PUT /api/categories/{id}` 🔒Admin
#### `DELETE /api/categories/{id}` 🔒Admin
#### `PUT /api/categories/{categoryId}/image` 🔒Admin

---

### 8.5 Course Management (`/api/management/courses`)

#### `POST /api/management/courses` 🔒
Create a new course (draft).

**Request Body:**
```json
{
  "title": "دورة C# للمبتدئين",
  "slug": "csharp-for-beginners",
  "description": "تعلم C# من الصفر",
  "categoryId": "guid",
  "level": "Beginner",
  "language": "Ar",
  "price": 199.99
}
```

**Response (200):**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "title": "دورة C# للمبتدئين",
    "slug": "csharp-for-beginners",
    "description": "تعلم C# من الصفر",
    "price": 199.99,
    "level": "Beginner",
    "language": "Ar",
    "status": "Draft",
    "totalDurationMinutes": 0,
    "enrollmentCount": 0,
    "averageRating": 0,
    "thumbnailUrl": null,
    "categoryName": "برمجة",
    "createdAt": "2026-06-24T10:00:00Z"
  }
}
```

#### `GET /api/management/courses` 🔒Instructor
List courses owned by current instructor.

#### `GET /api/management/courses/{id}` 🔒
Get course details (includes Requirements + LearningOutcomes).

#### `PUT /api/management/courses/{id}` 🔒
Update course. Only `Draft` or `Rejected` courses can be edited.

**Request Body (all optional):**
```json
{
  "title": "دورة C# للمبتدئين - النسخة المحدثة",
  "description": "وصف محدث",
  "categoryId": "guid",
  "level": "Intermediate",
  "language": "Ar",
  "price": 299.99
}
```

#### `DELETE /api/management/courses/{id}` 🔒
#### `POST /api/management/courses/{id}/requirements` 🔒
**Request Body:** `{ "requirementText": "معرفة básica بالبرمجة" }`
#### `DELETE /api/management/courses/{id}/requirements/{requirementId}` 🔒
#### `POST /api/management/courses/{id}/outcomes` 🔒
**Request Body:** `{ "outcomeText": "ستتعلم بناء تطبيقات ويب" }`
#### `DELETE /api/management/courses/{id}/outcomes/{outcomeId}` 🔒
#### `POST /api/management/courses/{id}/submit-for-review` 🔒
Submit course for admin review.
#### `POST /api/management/courses/{id}/schedule-deletion` 🔒
**Request Body:** `{ "scheduledDate": "2026-07-01T00:00:00Z", "reason": "محتوى قديم" }`
#### `POST /api/management/courses/{id}/cancel-scheduled-deletion` 🔒
#### `GET /api/management/courses/{id}/deletion-status` 🔒
#### `PUT /api/management/courses/{courseId}/image` 🔒
**Request Body:** `{ "fileId": "guid" }`

---

### 8.6 Sections (`/api/management/courses/{courseId}/sections`)

#### `GET .../sections` 🔒
**Response:** Array of `SectionDto` with nested `Items`.

#### `POST .../sections` 🔒
**Request Body:**
```json
{
  "title": "المحاضرة الأولى: مقدمة في C#",
  "description": "مقدمة شاملة"
}
```

#### `PUT .../sections/{sectionId}` 🔒
**Request Body:** `{ "title": "...", "description": "...", "isLocked": false }`

#### `DELETE .../sections/{sectionId}` 🔒

#### `PUT .../sections/reorder` 🔒
**Request Body:**
```json
{
  "items": [
    { "id": "section-guid-1", "position": 1 },
    { "id": "section-guid-2", "position": 2 }
  ]
}
```

#### `POST .../sections/{sectionId}/items` 🔒
**Request Body:**
```json
{
  "itemType": "Video",
  "itemId": "video-guid",
  "isPreviewAllowed": false,
  "isMandatory": true
}
```
> `itemType` values: `"Video"`, `"Quiz"`, `"Document"`, `"LiveSession"`

#### `PUT .../sections/{sectionId}/items/{itemId}` 🔒
**Request Body:** `{ "isPreviewAllowed": true, "isMandatory": false }`

#### `DELETE .../sections/{sectionId}/items/{itemId}` 🔒
#### `PUT .../sections/{sectionId}/items/reorder` 🔒

---

### 8.7 Video Content (`/api/courses/{courseId}/videos`)

#### `POST .../videos` 🔒Instructor
**Request Body:**
```json
{
  "sectionId": "guid",
  "title": "محاضرة 1: تثبيت بيئة التطوير",
  "videoFileId": "guid-of-uploaded-video",
  "provider": "Local",
  "durationSeconds": 1800,
  "transcript": "نص المحاضرة...",
  "isPreview": false
}
```

#### `GET .../videos/{id}` 🔒Instructor
#### `PUT .../videos/{id}` 🔒Instructor
#### `DELETE .../videos/{id}` 🔒Instructor

---

### 8.8 Document Content (`/api/courses/{courseId}/documents`)

#### `POST .../documents` 🔒Instructor
**Request Body:**
```json
{
  "sectionId": "guid",
  "title": "ملحق الدورة",
  "description": "PDF يحتوي على ملخص المحاضرات",
  "fileId": "guid-of-uploaded-file",
  "isDownloadable": true
}
```

#### `GET .../documents/{id}` 🔒Instructor
#### `PUT .../documents/{id}` 🔒Instructor
#### `DELETE .../documents/{id}` 🔒Instructor

---

### 8.9 Quiz Management (`/api/courses/{courseId}/quizzes`)

#### `POST .../quizzes` 🔒Instructor
**Request Body:**
```json
{
  "sectionId": "guid",
  "title": "اختبار الوحدة الأولى",
  "description": "اختبار قصير",
  "durationMinutes": 30,
  "passingScorePercent": 60,
  "maxAttempts": 3,
  "shuffleQuestions": true,
  "shuffleOptions": true,
  "showResultsImmediately": true,
  "allowReview": true,
  "availableFrom": "2026-06-25T00:00:00Z",
  "availableUntil": "2026-07-01T23:59:59Z"
}
```

#### `GET .../quizzes/{id}` 🔒Instructor
Returns quiz with all questions and options.

#### `PUT .../quizzes/{id}` 🔒Instructor
#### `DELETE .../quizzes/{id}` 🔒Instructor

#### `POST .../quizzes/{quizId}/questions` 🔒Instructor
**Request Body:**
```json
{
  "questionText": "ما هو نوع البيانات الصحيح لتخزين عدد صحيح؟",
  "type": "MultipleChoice",
  "points": 1,
  "explanation": "int هو نوع البيانات الأساسي للأعداد الصحيحة",
  "position": 1,
  "options": [
    { "optionText": "int", "isCorrect": true, "position": 1 },
    { "optionText": "string", "isCorrect": false, "position": 2 },
    { "optionText": "bool", "isCorrect": false, "position": 3 },
    { "optionText": "float", "isCorrect": false, "position": 4 }
  ]
}
```
> `type` values: `"MultipleChoice"`, `"TrueFalse"`, `"ShortAnswer"`

#### `PUT .../quizzes/{quizId}/questions/{questionId}` 🔒Instructor
#### `DELETE .../quizzes/{quizId}/questions/{questionId}` 🔒Instructor

---

### 8.10 Quiz Attempts (`/api/enrollments/{enrollmentId}/quizzes/{quizId}/attempts`)

#### `POST .../attempts` 🔒
Start a new quiz attempt.

**Response (200):**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "enrollmentId": "guid",
    "quizId": "guid",
    "attemptNumber": 1,
    "startedAt": "2026-06-24T10:00:00Z",
    "submittedAt": null,
    "scorePercentage": 0,
    "isPassed": false,
    "status": "InProgress"
  }
}
```

#### `PUT .../attempts/{attemptId}` 🔒
Submit quiz answers.

**Request Body:**
```json
{
  "answers": [
    {
      "questionId": "guid",
      "selectedOptionId": "guid",
      "answerText": null
    },
    {
      "questionId": "guid",
      "selectedOptionId": null,
      "answerText": "إجابة نصية"
    }
  ]
}
```

#### `GET .../attempts` 🔒
#### `GET .../attempts/{attemptId}` 🔒

---

### 8.11 Enrollments (`/api/enrollments`)

#### `POST /api/enrollments` 🔒
Enroll in a course.

**Request Body:**
```json
{
  "courseId": "guid",
  "userId": "guid",
  "source": "Purchase"
}
```
> `source` values: `"Purchase"`, `"Free"`, `"Admin"`, `"Referral"`

#### `GET /api/enrollments` 🔒
List current user's enrollments.

#### `GET /api/enrollments/{id}` 🔒
Get enrollment details with progress.

#### `GET /api/enrollments/{enrollmentId}/progress` 🔒

#### `PUT /api/enrollments/{enrollmentId}/progress` 🔒
**Request Body:**
```json
{
  "watchTimeSeconds": 300,
  "completionPercentage": 75.5,
  "metadata": null,
  "markAsCompleted": false
}
```

#### `POST /api/enrollments/{enrollmentId}/progress/{contentType}/{contentId}/complete` 🔒
Mark a content item as completed.

---

### 8.12 Admin Course Management (`/api/admin/courses`)

#### `POST .../courses/{id}/approve` 🔒Admin
#### `POST .../courses/{id}/reject` 🔒Admin
#### `GET .../courses/edit-requests` 🔒Admin
#### `GET .../courses/edit-requests/{requestId}` 🔒Admin
#### `POST .../courses/edit-requests/{requestId}/review` 🔒Admin
**Request Body:**
```json
{
  "approve": true,
  "notes": "تمت المراجعة والموافقة"
}
```

---

### 8.13 Live Sessions (`/api/courses/{courseId}/live-sessions`)

#### `POST .../live-sessions` 🔒Instructor
**Request Body:**
```json
{
  "title": "جلسة مباشرة: مراجعة الأسئلة",
  "description": "جلسة تفاعلية",
  "meetingUrl": "https://zoom.us/j/123456",
  "scheduledStart": "2026-06-25T18:00:00Z",
  "scheduledEnd": "2026-06-25T19:00:00Z",
  "maxAttendees": 50
}
```

#### `GET .../live-sessions` 🔒Instructor
#### `PUT .../live-sessions/{sessionId}/status` 🔒Instructor
#### `DELETE .../live-sessions/{sessionId}` 🔒Instructor

---

### 8.14 Live Attendance (`/api/live-sessions/{sessionId}/attendance`)

#### `POST .../attendance/join` 🔒
#### `POST .../attendance/leave` 🔒
#### `GET .../attendance/count` 🔒

---

### 8.15 Video Comments (`/api/videos/{videoId}/comments`)

#### `GET .../comments`
**Response:** Array of comments with replies.

#### `POST .../comments` 🔒
**Request Body:** `{ "content": "محاضرة ممتازة!" }`

#### `PUT .../comments/{commentId}` 🔒
#### `DELETE .../comments/{commentId}` 🔒
#### `POST .../comments/{commentId}/like` 🔒

---

### 8.16 Reviews (`/api/reviews`)

#### `POST /api/reviews` 🔒
**Request Body:**
```json
{
  "courseId": "guid",
  "rating": 5,
  "comment": "دورة ممتازة ومحتوى غني"
}
```

#### `GET /api/reviews/course/{courseId}`
#### `GET /api/reviews/{id}`
#### `PUT /api/reviews/{id}` 🔒
#### `DELETE /api/reviews/{id}` 🔒
#### `POST /api/reviews/{id}/helpful` 🔒
#### `POST /api/reviews/{id}/flag` 🔒Instructor
#### `GET /api/reviews/pending` 🔒Admin
#### `PUT /api/reviews/{id}/moderate` 🔒Admin

---

### 8.17 Certificates (`/api/certificates`)

#### `GET /api/certificates/my` 🔒
#### `GET /api/certificates/{id}` 🔒
#### `GET /api/certificates/{id}/download` 🔒
Returns PDF file.
#### `GET /api/certificates/verify/{code}`
Public verification endpoint.

---

### 8.18 Admin Certificates (`/api/admin/certificates`)

#### `GET /api/admin/certificates` 🔒Admin
#### `POST /api/admin/certificates/{id}/revoke` 🔒Admin
#### `POST /api/admin/certificates/issue` 🔒Admin

---

### 8.19 Cart (`/api/cart`)

#### `GET /api/cart` 🔒
**Response (200):**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "items": [
      {
        "id": "guid",
        "courseId": "guid",
        "courseTitle": "دورة C# للمبتدئين",
        "courseImageUrl": "https://minio.../images/course.jpg",
        "instructorName": "أحمد محمد",
        "priceSnapshot": 199.99,
        "currentPrice": 199.99,
        "addedAt": "2026-06-24T10:00:00Z"
      }
    ],
    "subtotal": 199.99,
    "couponCode": null,
    "discountAmount": 0,
    "finalAmount": 199.99
  }
}
```

#### `POST /api/cart/items` 🔒
**Request Body:** `{ "courseId": "guid" }`

#### `DELETE /api/cart/items/{itemId}` 🔒

#### `POST /api/cart/apply-coupon` 🔒
**Request Body:** `{ "code": "SUMMER20" }`

**Response:**
```json
{
  "success": true,
  "data": {
    "code": "SUMMER20",
    "discountAmount": 40.00,
    "finalAmount": 159.99,
    "message": "تم تطبيق الخصم بنجاح"
  }
}
```

#### `DELETE /api/cart/coupon` 🔒

---

### 8.20 Coupons (`/api/coupons`)

#### `POST /api/coupons/validate` 🔒
**Request Body:**
```json
{
  "code": "SUMMER20",
  "cartTotal": 199.99,
  "courseIds": ["guid1", "guid2"]
}
```

---

### 8.21 Admin Coupons (`/api/admin/coupons`)

#### `GET /api/admin/coupons` 🔒Admin
#### `GET /api/admin/coupons/{couponId}` 🔒Admin
#### `POST /api/admin/coupons` 🔒Admin
**Request Body:**
```json
{
  "code": "SUMMER20",
  "type": "Percentage",
  "value": 20,
  "maxDiscountAmount": 50,
  "minimumPurchaseAmount": 100,
  "applicableTo": "All",
  "courseIds": null,
  "usageLimit": 100,
  "userLimitPerUser": 1,
  "isPublic": true,
  "validFrom": "2026-06-01T00:00:00Z",
  "validUntil": "2026-08-31T23:59:59Z"
}
```
> `type` values: `"Percentage"`, `"FixedAmount"`
> `applicableTo` values: `"All"`, `"SpecificCourses"`, `"FirstPurchase"`

#### `PUT /api/admin/coupons/{couponId}` 🔒Admin
#### `PATCH /api/admin/coupons/{couponId}/toggle` 🔒Admin
#### `DELETE /api/admin/coupons/{couponId}` 🔒Admin

---

### 8.22 Orders (`/api/orders`)

#### `GET /api/orders` 🔒
List current user's orders.

#### `GET /api/orders/{orderId}` 🔒
Get order details with items and payment history.

#### `POST /api/orders` 🔒
Create order from cart contents.

**Request Body:**
```json
{
  "couponCode": "SUMMER20"
}
```
> `couponCode` is optional.

**Response (200):**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "orderNumber": "ORD-20260624-001",
    "subtotal": 199.99,
    "discountAmount": 40.00,
    "finalAmount": 159.99,
    "status": "Pending",
    "couponCode": "SUMMER20",
    "itemCount": 1,
    "createdAt": "2026-06-24T10:00:00Z"
  }
}
```

---

### 8.23 Payments (`/api/payments`)

#### `POST /api/payments/process` 🔒
Process payment for an order.

**Query Parameter:** `orderId=guid`
**Request Body:**
```json
{
  "paymentMethodId": "guid"
}
```

**Response (200):**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "orderId": "guid",
    "amount": 159.99,
    "status": "Completed",
    "gatewayTransactionId": "TXN-123456",
    "gatewayResponse": "Payment processed successfully",
    "paymentMethodName": "Credit Card",
    "createdAt": "2026-06-24T10:05:00Z"
  }
}
```

#### `GET /api/payments/methods`
List active payment methods (public).

#### `GET /api/payments/history/{orderId}` 🔒

---

### 8.24 Admin Payment Methods (`/api/admin/payment-methods`)

#### `GET .../payment-methods` 🔒Admin
#### `POST .../payment-methods` 🔒Admin
**Request Body:**
```json
{
  "name": "بطاقة ائتمان",
  "provider": "Stripe",
  "type": "CreditCard",
  "configuration": "{ \"apiKey\": \"sk_test_...\" }"
}
```
#### `PATCH .../payment-methods/{id}/toggle` 🔒Admin

---

### 8.25 Refunds (`/api/refunds`)

#### `POST /api/refunds` 🔒
**Request Body:**
```json
{
  "paymentId": "guid",
  "reason": "أريد استرداد المبلغ"
}
```

#### `GET /api/refunds` 🔒

---

### 8.26 Admin Refunds (`/api/admin/refunds`)

#### `GET /api/admin/refunds` 🔒Admin
#### `POST /api/admin/refunds/approve` 🔒Admin
**Request Body:** `{ "refundId": "guid", "adminNotes": "تمت الموافقة" }`
#### `POST /api/admin/refunds/reject` 🔒Admin
**Request Body:** `{ "refundId": "guid", "adminNotes": "السبب غير كافٍ" }`

---

### 8.27 Wishlist (`/api/wishlist`)

#### `GET /api/wishlist` 🔒
#### `POST /api/wishlist/{courseId}` 🔒
#### `DELETE /api/wishlist/{courseId}` 🔒

---

### 8.28 Notifications (`/api/notifications`)

#### `GET /api/notifications` 🔒
#### `GET /api/notifications/unread-count` 🔒
#### `PATCH /api/notifications/{notificationId}/read` 🔒
#### `POST /api/notifications/mark-all-read` 🔒
#### `DELETE /api/notifications/{notificationId}` 🔒
#### `DELETE /api/notifications/clear-all` 🔒

---

### 8.29 Notification Preferences (`/api/notifications/preferences`)

#### `GET /api/notifications/preferences` 🔒
#### `PUT /api/notifications/preferences` 🔒
**Request Body:**
```json
{
  "emailNotifications": true,
  "pushNotifications": true,
  "courseUpdates": true,
  "newMessages": true,
  "announcements": true,
  "marketingEmails": false
}
```

---

### 8.30 Media (`/api/media`)

#### `POST /api/media/upload-url` 🔒
Generate a pre-signed upload URL.

**Request Body:** `{ "fileName": "video.mp4", "contentType": "video/mp4", "fileSize": 104857600 }`

#### `POST /api/media/confirm-upload` 🔒
**Request Body:** `{ "fileId": "guid" }`

#### `GET /api/media/{fileId}/view-url`
Get a pre-signed view/download URL.

---

### 8.31 Admin Media (`/api/admin/media`)

#### `GET /api/admin/media` 🔒Admin
#### `GET /api/admin/media/{fileId}` 🔒Admin
#### `DELETE /api/admin/media/{fileId}/soft` 🔒Admin
#### `POST /api/admin/media/{fileId}/restore` 🔒Admin
#### `DELETE /api/admin/media/{fileId}` 🔒Admin
#### `GET /api/admin/media/stats` 🔒Admin

---

### 8.32 Instructor Requests (`/api/instructor-requests`)

#### `GET /api/instructor-requests/can-submit` 🔒
#### `POST /api/instructor-requests` 🔒
**Request Body:**
```json
{
  "message": "أريد أن أكون مدرّب في المنصة",
  "documents": [
    {
      "documentType": "Certificate",
      "fileId": "guid",
      "urlValue": null
    }
  ]
}
```

#### `PUT /api/instructor-requests/{requestId}` 🔒
#### `POST /api/instructor-requests/{requestId}/documents` 🔒
#### `GET /api/instructor-requests/my-requests` 🔒
#### `GET /api/instructor-requests/my-requests/{requestId}` 🔒
#### `DELETE /api/instructor-requests/{requestId}/cancel` 🔒
#### `GET /api/instructor-requests/pending` 🔒Admin
#### `GET /api/instructor-requests/{requestId}` 🔒Admin
#### `PUT /api/instructor-requests/{requestId}/process` 🔒Admin
#### `DELETE /api/instructor-requests/{requestId}` 🔒Admin

---

### 8.33 Admin Users (`/api/admin/users`)

#### `GET /api/admin/users` 🔒Admin
#### `GET /api/admin/users/{userId}` 🔒Admin
#### `PATCH /api/admin/users/{userId}/toggle-active` 🔒Admin
#### `DELETE /api/admin/users/{userId}` 🔒Admin

---

### 8.34 Dashboards

#### Student (`/api/student/dashboard`)
| Endpoint | Auth | Description |
|----------|------|-------------|
| `GET .../overview` 🔒 | Student | Stats: enrolled courses, completed, certificates |
| `GET .../courses` 🔒 | Student | List enrolled courses with progress |
| `GET .../weekly-activity` 🔒 | Student | Activity chart data |
| `GET .../certificates` 🔒 | Student | Student's certificates |

#### Instructor (`/api/instructor/dashboard`)
| Endpoint | Auth | Description |
|----------|------|-------------|
| `GET .../overview` 🔒 | Instructor | Stats: total courses, students, revenue |
| `GET .../courses` 🔒 | Instructor | Instructor's courses |
| `GET .../revenue` 🔒 | Instructor | Revenue data |
| `GET .../students` 🔒 | Instructor | Student stats |
| `GET .../pending-requests` 🔒 | Instructor | Pending edit requests |
| `GET .../recent-reviews` 🔒 | Instructor | Recent reviews |

#### Admin (`/api/admin/dashboard`)
| Endpoint | Auth | Description |
|----------|------|-------------|
| `GET .../overview` 🔒 | Admin | Platform-wide stats |
| `GET .../revenue` 🔒 | Admin | Revenue trends |
| `GET .../user-growth` 🔒 | Admin | User growth data |
| `GET .../enrollment-trend` 🔒 | Admin | Enrollment trends |
| `GET .../top-courses` 🔒 | Admin | Top performing courses |

---

### 8.35 Communication

#### Announcements (`/api/announcements`)
| Endpoint | Auth | Description |
|----------|------|-------------|
| `GET .../feed` 🔒 | Auth | Get announcement feed |
| `POST ...` 🔒Admin | Admin | Create announcement |
| `PUT .../{id}` 🔒Admin | Admin | Update announcement |
| `PATCH .../{id}/deactivate` 🔒Admin | Admin | Deactivate |
| `DELETE .../{id}` 🔒Admin | Admin | Delete |

#### Messages (`/api/messages`)
| Endpoint | Auth | Description |
|----------|------|-------------|
| `POST .../send` 🔒 | Auth | Send message |
| `GET .../conversations` 🔒 | Auth | List conversations |
| `GET .../conversations/{id}` 🔒 | Auth | Get messages in conversation |
| `PATCH .../{messageId}/read` 🔒 | Auth | Mark as read |
| `DELETE .../{messageId}` 🔒 | Auth | Soft delete |
| `GET .../unread-count` 🔒 | Auth | Unread count |

#### System Settings (`/api/system-settings`)
| Endpoint | Auth | Description |
|----------|------|-------------|
| `GET ...` 🔒Admin | Admin | List all settings |
| `GET .../{key}` 🔒Admin | Admin | Get setting by key |
| `POST ...` 🔒Admin | Admin | Create setting |
| `PUT .../{key}` 🔒Admin | Admin | Update setting |
| `DELETE .../{key}` 🔒Admin | Admin | Delete setting |

#### Activity Logs (`/api/activity-logs`)
| Endpoint | Auth | Description |
|----------|------|-------------|
| `GET ...` 🔒Admin | Admin | List logs with filters |

#### Reports (`/api/reports`)
| Endpoint | Auth | Description |
|----------|------|-------------|
| `POST ...` 🔒 | Auth | Create report |
| `GET .../pending` 🔒Admin | Admin | Pending reports |
| `PATCH .../{id}/resolve` 🔒Admin | Admin | Resolve report |

---

### 8.36 Public Endpoints

| Endpoint | Auth | Description |
|----------|------|-------------|
| `GET /api/public/landing` | — | Aggregated landing page data |
| `GET /api/public/stats` | — | Platform statistics |
| `GET /api/public/about` | — | About page content |
| `GET /api/public/legal/{type}` | — | Legal pages (`"privacy"`, `"terms"`, `"refund"`) |
| `GET /api/public/testimonials` | — | Approved testimonials |
| `POST /api/public/testimonials` 🔒 | Auth | Submit testimonial |
| `GET /api/public/instructors/{slug}` | — | Instructor profile by slug |
| `GET /api/public/instructors/check-slug` 🔒 | Auth | Check slug availability |
| `GET /api/public/instructors/search` | — | Search instructors |
| `GET /api/public/courses` | — | Browse published courses (with filters) |
| `GET /api/public/courses/{id}` | — | Course details |
| `GET /api/public/courses/slug/{slug}` | — | Course by slug |
| `GET /api/public/courses/search/suggest` | — | Search suggestions |
| `GET /api/public/courses/stats` | — | Platform stats |
| `GET /api/public/courses/{id}/related` | — | Related courses |
| `GET /api/public/courses/filters/options` | — | Available filter options |
| `POST /api/public/contact` | — | Submit contact message |

#### `GET /api/public/courses` Query Parameters:
```
?searchQuery=CSharp&categoryId=guid&level=Beginner&language=Ar
&minPrice=0&maxPrice=500&isFreeOnly=false&minRating=4
&sortBy=PublishedAt&sortDescending=true&page=1&pageSize=12
```
> `sortBy` values: `"PublishedAt"`, `"Price"`, `"AverageRating"`, `"EnrollmentCount"`, `"Title"`

---

### 8.37 Health Checks

| Endpoint | Description |
|----------|-------------|
| `GET /health` | Basic health check |
| `GET /healthz` | Liveness probe |
| `GET /ready` | Readiness probe (checks database) |

---

### 8.38 Root

#### `GET /`
**Response:**
```json
{
  "name": "Athary LMS API",
  "version": "1.0.0",
  "swagger": "/swagger",
  "health": "/health"
}
```
| PUT | `/api/categories/{id}` | Admin | Update category |
| DELETE | `/api/categories/{id}` | Admin | Delete category |
| PUT | `/api/categories/{categoryId}/image` | Admin | Set category image |

### 8.5 Course Management (`/api/management/courses`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/management/courses` | Auth | Create course |
| GET | `/api/management/courses` | Instructor | List instructor's courses |
| GET | `/api/management/courses/{id}` | Auth | Get course details |
| PUT | `/api/management/courses/{id}` | Auth | Update course |
| DELETE | `/api/management/courses/{id}` | Auth | Delete course |
| POST | `/api/management/courses/{id}/requirements` | Auth | Add requirement |
| DELETE | `/api/management/courses/{id}/requirements/{requirementId}` | Auth | Remove requirement |
| POST | `/api/management/courses/{id}/outcomes` | Auth | Add learning outcome |
| DELETE | `/api/management/courses/{id}/outcomes/{outcomeId}` | Auth | Remove learning outcome |
| POST | `/api/management/courses/{id}/submit-for-review` | Auth | Submit for review |
| POST | `/api/management/courses/{id}/schedule-deletion` | Auth | Schedule deletion |
| POST | `/api/management/courses/{id}/cancel-scheduled-deletion` | Auth | Cancel scheduled deletion |
| GET | `/api/management/courses/{id}/deletion-status` | Auth | Get deletion status |
| PUT | `/api/management/courses/{courseId}/image` | Auth | Set course image |

### 8.6 Section Management (`/api/management/courses/{courseId}/sections`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/management/courses/{courseId}/sections` | Auth | List sections |
| POST | `/api/management/courses/{courseId}/sections` | Auth | Create section |
| GET | `/api/management/courses/{courseId}/sections/{sectionId}` | Auth | Get section |
| PUT | `/api/management/courses/{courseId}/sections/{sectionId}` | Auth | Update section |
| DELETE | `/api/management/courses/{courseId}/sections/{sectionId}` | Auth | Delete section |
| PUT | `/api/management/courses/{courseId}/sections/reorder` | Auth | Reorder sections |
| POST | `/api/management/courses/{courseId}/sections/{sectionId}/items` | Auth | Add item to section |
| PUT | `/api/management/courses/{courseId}/sections/{sectionId}/items/{itemId}` | Auth | Update section item |
| DELETE | `/api/management/courses/{courseId}/sections/{sectionId}/items/{itemId}` | Auth | Delete section item |
| PUT | `/api/management/courses/{courseId}/sections/{sectionId}/items/reorder` | Auth | Reorder section items |

### 8.7 Video Content (`/api/courses/{courseId}/videos`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/courses/{courseId}/videos/{id}` | Instructor | Get video |
| POST | `/api/courses/{courseId}/videos` | Instructor | Create video |
| PUT | `/api/courses/{courseId}/videos/{id}` | Instructor | Update video |
| DELETE | `/api/courses/{courseId}/videos/{id}` | Instructor | Delete video |

### 8.8 Document Content (`/api/courses/{courseId}/documents`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/courses/{courseId}/documents/{id}` | Instructor | Get document |
| POST | `/api/courses/{courseId}/documents` | Instructor | Create document |
| PUT | `/api/courses/{courseId}/documents/{id}` | Instructor | Update document |
| DELETE | `/api/courses/{courseId}/documents/{id}` | Instructor | Delete document |

### 8.9 Quiz Management (`/api/courses/{courseId}/quizzes`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/courses/{courseId}/quizzes/{id}` | Instructor | Get quiz |
| POST | `/api/courses/{courseId}/quizzes` | Instructor | Create quiz |
| PUT | `/api/courses/{courseId}/quizzes/{id}` | Instructor | Update quiz |
| DELETE | `/api/courses/{courseId}/quizzes/{id}` | Instructor | Delete quiz |
| POST | `/api/courses/{courseId}/quizzes/{quizId}/questions` | Instructor | Add question |
| PUT | `/api/courses/{courseId}/quizzes/{quizId}/questions/{questionId}` | Instructor | Update question |
| DELETE | `/api/courses/{courseId}/quizzes/{quizId}/questions/{questionId}` | Instructor | Delete question |

### 8.10 Quiz Attempts (`/api/enrollments/{enrollmentId}/quizzes/{quizId}/attempts`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `.../attempts` | Auth | Start quiz attempt |
| PUT | `.../attempts/{attemptId}` | Auth | Submit attempt |
| GET | `.../attempts` | Auth | List attempts |
| GET | `.../attempts/{attemptId}` | Auth | Get attempt result |

### 8.11 Enrollments (`/api/enrollments`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/enrollments` | Auth | Enroll in course |
| GET | `/api/enrollments` | Auth | List my enrollments |
| GET | `/api/enrollments/{id}` | Auth | Get enrollment details |
| GET | `/api/enrollments/{enrollmentId}/progress` | Auth | Get progress |
| PUT | `/api/enrollments/{enrollmentId}/progress` | Auth | Update progress |
| POST | `/api/enrollments/{enrollmentId}/progress/{contentType}/{contentId}/complete` | Auth | Mark content complete |

### 8.12 Admin Course Management (`/api/admin/courses`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/admin/courses/{id}/approve` | Admin | Approve course |
| POST | `/api/admin/courses/{id}/reject` | Admin | Reject course |
| GET | `/api/admin/courses/edit-requests` | Admin | List edit requests |
| GET | `/api/admin/courses/edit-requests/{requestId}` | Admin | Get request details |
| POST | `/api/admin/courses/edit-requests/{requestId}/review` | Admin | Review edit request |

### 8.13 Live Sessions (`/api/courses/{courseId}/live-sessions`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/courses/{courseId}/live-sessions` | Instructor | List sessions |
| POST | `/api/courses/{courseId}/live-sessions` | Instructor | Create session |
| PUT | `/api/courses/{courseId}/live-sessions/{sessionId}/status` | Instructor | Update status |
| DELETE | `/api/courses/{courseId}/live-sessions/{sessionId}` | Instructor | Delete session |

### 8.14 Live Attendance (`/api/live-sessions/{sessionId}/attendance`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `.../attendance/join` | Auth | Join session |
| POST | `.../attendance/leave` | Auth | Leave session |
| GET | `.../attendance/count` | Auth | Get attendee count |

### 8.15 Video Comments (`/api/videos/{videoId}/comments`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/videos/{videoId}/comments` | — | List comments |
| POST | `/api/videos/{videoId}/comments` | Auth | Add comment |
| PUT | `/api/videos/{videoId}/comments/{commentId}` | Auth | Update comment |
| DELETE | `/api/videos/{videoId}/comments/{commentId}` | Auth | Delete comment |
| POST | `/api/videos/{videoId}/comments/{commentId}/like` | Auth | Toggle like |

### 8.16 Reviews (`/api/reviews`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/reviews` | Auth | Create review |
| GET | `/api/reviews/course/{courseId}` | — | Get course reviews |
| GET | `/api/reviews/{id}` | — | Get review details |
| PUT | `/api/reviews/{id}` | Auth | Update review |
| DELETE | `/api/reviews/{id}` | Auth | Delete review |
| POST | `/api/reviews/{id}/helpful` | Auth | Toggle helpful |
| POST | `/api/reviews/{id}/flag` | Instructor | Flag review |
| GET | `/api/reviews/pending` | Admin | Get pending reviews |
| PUT | `/api/reviews/{id}/moderate` | Admin | Moderate review |

### 8.17 Certificates (`/api/certificates`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/certificates/my` | Auth | List my certificates |
| GET | `/api/certificates/{id}` | Auth | Get certificate |
| GET | `/api/certificates/{id}/download` | Auth | Download PDF |
| GET | `/api/certificates/verify/{code}` | — | Verify certificate |

### 8.18 Admin Certificates (`/api/admin/certificates`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/admin/certificates` | Admin | List all certificates |
| POST | `/api/admin/certificates/{id}/revoke` | Admin | Revoke certificate |
| POST | `/api/admin/certificates/issue` | Admin | Issue certificate |

### 8.19 Cart (`/api/cart`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/cart` | Auth | Get cart |
| POST | `/api/cart/items` | Auth | Add item |
| DELETE | `/api/cart/items/{itemId}` | Auth | Remove item |
| POST | `/api/cart/apply-coupon` | Auth | Apply coupon |
| DELETE | `/api/cart/coupon` | Auth | Remove coupon |

### 8.20 Coupons (`/api/coupons`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/coupons/validate` | Auth | Validate coupon code |

### 8.21 Admin Coupons (`/api/admin/coupons`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/admin/coupons` | Admin | List coupons |
| GET | `/api/admin/coupons/{couponId}` | Admin | Get coupon |
| POST | `/api/admin/coupons` | Admin | Create coupon |
| PUT | `/api/admin/coupons/{couponId}` | Admin | Update coupon |
| PATCH | `/api/admin/coupons/{couponId}/toggle` | Admin | Toggle active |
| DELETE | `/api/admin/coupons/{couponId}` | Admin | Delete coupon |

### 8.22 Orders (`/api/orders`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/orders` | Auth | List my orders |
| GET | `/api/orders/{orderId}` | Auth | Get order details |
| POST | `/api/orders` | Auth | Create order |

### 8.23 Payments (`/api/payments`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/payments/process` | Auth | Process payment |
| GET | `/api/payments/methods` | — | List payment methods |
| GET | `/api/payments/history/{orderId}` | Auth | Payment history |

### 8.24 Admin Payment Methods (`/api/admin/payment-methods`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/admin/payment-methods` | Admin | List methods |
| POST | `/api/admin/payment-methods` | Admin | Create method |
| PATCH | `/api/admin/payment-methods/{id}/toggle` | Admin | Toggle active |

### 8.25 Refunds (`/api/refunds`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/refunds` | Auth | Request refund |
| GET | `/api/refunds` | Auth | List my refunds |

### 8.26 Admin Refunds (`/api/admin/refunds`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/admin/refunds` | Admin | List refunds |
| POST | `/api/admin/refunds/approve` | Admin | Approve refund |
| POST | `/api/admin/refunds/reject` | Admin | Reject refund |

### 8.27 Wishlist (`/api/wishlist`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/wishlist` | Auth | Get wishlist |
| POST | `/api/wishlist/{courseId}` | Auth | Add to wishlist |
| DELETE | `/api/wishlist/{courseId}` | Auth | Remove from wishlist |

### 8.28 Notifications (`/api/notifications`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/notifications` | Auth | List notifications |
| GET | `/api/notifications/unread-count` | Auth | Get unread count |
| PATCH | `/api/notifications/{notificationId}/read` | Auth | Mark as read |
| POST | `/api/notifications/mark-all-read` | Auth | Mark all as read |
| DELETE | `/api/notifications/{notificationId}` | Auth | Delete notification |
| DELETE | `/api/notifications/clear-all` | Auth | Clear all |

### 8.29 Notification Preferences (`/api/notifications/preferences`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/notifications/preferences` | Auth | Get preferences |
| PUT | `/api/notifications/preferences` | Auth | Update preferences |

### 8.30 Media (`/api/media`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/media/upload-url` | Auth | Generate upload URL |
| POST | `/api/media/confirm-upload` | Auth | Confirm upload |
| GET | `/api/media/{fileId}/view-url` | — | Get view URL |

### 8.31 Admin Media (`/api/admin/media`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/admin/media` | Admin | List media |
| GET | `/api/admin/media/{fileId}` | Admin | Get media details |
| DELETE | `/api/admin/media/{fileId}/soft` | Admin | Soft delete |
| POST | `/api/admin/media/{fileId}/restore` | Admin | Restore |
| DELETE | `/api/admin/media/{fileId}` | Admin | Permanent delete |
| GET | `/api/admin/media/stats` | Admin | Storage stats |

### 8.32 Instructor Requests (`/api/instructor-requests`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/instructor-requests/can-submit` | Auth | Check if can submit |
| POST | `/api/instructor-requests` | Auth | Submit request |
| PUT | `/api/instructor-requests/{requestId}` | Auth | Update request |
| POST | `/api/instructor-requests/{requestId}/documents` | Auth | Add document |
| GET | `/api/instructor-requests/my-requests` | Auth | List my requests |
| GET | `/api/instructor-requests/my-requests/{requestId}` | Auth | Get request |
| DELETE | `/api/instructor-requests/{requestId}/cancel` | Auth | Cancel request |
| GET | `/api/instructor-requests/pending` | Admin | Pending requests |
| GET | `/api/instructor-requests/{requestId}` | Admin | Get request details |
| PUT | `/api/instructor-requests/{requestId}/process` | Admin | Process request |
| DELETE | `/api/instructor-requests/{requestId}` | Admin | Delete request |

### 8.33 Admin Users (`/api/admin/users`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/admin/users` | Admin | List users |
| GET | `/api/admin/users/{userId}` | Admin | Get user |
| PATCH | `/api/admin/users/{userId}/toggle-active` | Admin | Toggle active |
| DELETE | `/api/admin/users/{userId}` | Admin | Delete user |

### 8.34 Dashboards

**Student** (`/api/student/dashboard`):
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/student/dashboard/overview` | Student | Overview stats |
| GET | `/api/student/dashboard/courses` | Student | Enrolled courses |
| GET | `/api/student/dashboard/weekly-activity` | Student | Weekly activity chart |
| GET | `/api/student/dashboard/certificates` | Student | Certificates |

**Instructor** (`/api/instructor/dashboard`):
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/instructor/dashboard/overview` | Instructor | Overview stats |
| GET | `/api/instructor/dashboard/courses` | Instructor | My courses |
| GET | `/api/instructor/dashboard/revenue` | Instructor | Revenue data |
| GET | `/api/instructor/dashboard/students` | Instructor | Student stats |
| GET | `/api/instructor/dashboard/pending-requests` | Instructor | Pending edit requests |
| GET | `/api/instructor/dashboard/recent-reviews` | Instructor | Recent reviews |

**Admin** (`/api/admin/dashboard`):
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/admin/dashboard/overview` | Admin | Platform overview |
| GET | `/api/admin/dashboard/revenue` | Admin | Revenue trends |
| GET | `/api/admin/dashboard/user-growth` | Admin | User growth |
| GET | `/api/admin/dashboard/enrollment-trend` | Admin | Enrollment trends |
| GET | `/api/admin/dashboard/top-courses` | Admin | Top courses |

### 8.35 Communication

**Announcements** (`/api/announcements`):
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/announcements/feed` | Auth | Get announcement feed |
| POST | `/api/announcements` | Admin | Create announcement |
| PUT | `/api/announcements/{id}` | Admin | Update announcement |
| PATCH | `/api/announcements/{id}/deactivate` | Admin | Deactivate |
| DELETE | `/api/announcements/{id}` | Admin | Delete announcement |

**Messages** (`/api/messages`):
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/messages/send` | Auth | Send message |
| GET | `/api/messages/conversations` | Auth | List conversations |
| GET | `/api/messages/conversations/{conversationId}` | Auth | Get messages |
| PATCH | `/api/messages/{messageId}/read` | Auth | Mark as read |
| DELETE | `/api/messages/{messageId}` | Auth | Soft delete |
| GET | `/api/messages/unread-count` | Auth | Unread count |

**System Settings** (`/api/system-settings`):
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/system-settings` | Admin | List settings |
| GET | `/api/system-settings/{key}` | Admin | Get setting |
| POST | `/api/system-settings` | Admin | Create setting |
| PUT | `/api/system-settings/{key}` | Admin | Update setting |
| DELETE | `/api/system-settings/{key}` | Admin | Delete setting |

**Activity Logs** (`/api/activity-logs`):
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/activity-logs` | Admin | List logs with filters |

**Reports** (`/api/reports`):
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/reports` | Auth | Create report |
| GET | `/api/reports/pending` | Admin | Pending reports |
| PATCH | `/api/reports/{id}/resolve` | Admin | Resolve report |

### 8.36 Public Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/public/landing` | — | Aggregated landing data |
| GET | `/api/public/stats` | — | Platform statistics |
| GET | `/api/public/about` | — | About page content |
| GET | `/api/public/legal/{type}` | — | Legal pages (privacy/terms/refund) |
| GET | `/api/public/testimonials` | — | Approved testimonials |
| POST | `/api/public/testimonials` | Auth | Submit testimonial |
| GET | `/api/public/instructors/{slug}` | — | Instructor profile by slug |
| GET | `/api/public/instructors/check-slug` | Auth | Check slug availability |
| GET | `/api/public/instructors/search` | — | Search instructors |
| GET | `/api/public/courses` | — | Browse published courses |
| GET | `/api/public/courses/{id}` | — | Course details |
| GET | `/api/public/courses/slug/{slug}` | — | Course by slug |
| GET | `/api/public/courses/search/suggest` | — | Search suggestions |
| GET | `/api/public/courses/stats` | — | Platform stats |
| GET | `/api/public/courses/{id}/related` | — | Related courses |
| GET | `/api/public/courses/filters/options` | — | Filter options |
| POST | `/api/public/contact` | — | Submit contact message |

### 8.37 Health Checks

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/health` | Basic health check |
| GET | `/healthz` | Liveness probe |
| GET | `/ready` | Readiness probe (database) |

### 8.38 Root

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | API info (name, version, links) |

---

## 9. Services Layer

### 9.1 Authentication Services

| Service | Location | Description |
|---------|----------|-------------|
| `IAuthenticationService` | Infrastructure/Services/Authentication/ | Login, register, password reset |
| `ITokenService` | Infrastructure/Services/Authentication/ | JWT generation and validation |
| `ISessionService` | Infrastructure/Services/Authentication/ | Session management |
| `IOAuthService` | Infrastructure/Services/Authentication/ | Google/Microsoft OAuth |
| `IVerificationService` | Infrastructure/Services/Authentication/ | Email verification |

### 9.2 Course Services

| Service | Location | Description |
|---------|----------|-------------|
| `ICourseService` | Infrastructure/Services/Courses/ | CRUD, approval workflow |
| `ISectionService` | Infrastructure/Services/Courses/ | Section and item management |
| `IVideoService` | Infrastructure/Services/Courses/ | Video content CRUD |
| `IDocumentService` | Infrastructure/Services/Courses/ | Document content CRUD |
| `IQuizService` | Infrastructure/Services/Courses/ | Quiz management |
| `IQuizAttemptService` | Infrastructure/Services/Courses/ | Quiz taking and grading |
| `IEnrollmentService` | Infrastructure/Services/Courses/ | Enrollment management |
| `IContentProgressService` | Infrastructure/Services/Courses/ | Progress tracking |
| `ICourseEditApprovalService` | Infrastructure/Services/Courses/ | Edit approval workflow |
| `IPublicCourseService` | Infrastructure/Services/Courses/ | Public course browsing |

### 9.3 Commerce Services

| Service | Location | Description |
|---------|----------|-------------|
| `ICartService` | Infrastructure/Services/Commerce/ | Shopping cart |
| `ICouponService` | Infrastructure/Services/Commerce/ | Coupon management |
| `IOrderService` | Infrastructure/Services/Commerce/ | Order processing |
| `IPaymentService` | Infrastructure/Services/Commerce/ | Payment processing |
| `IRefundService` | Infrastructure/Services/Commerce/ | Refund handling |
| `IWishlistService` | Infrastructure/Services/Wishlist/ | Wishlist management |
| `IPaymentGateway` | Infrastructure/Services/Commerce/PaymentGateway/ | Mock payment gateway |

### 9.4 Communication Services

| Service | Location | Description |
|---------|----------|-------------|
| `IMessageService` | Infrastructure/Services/Communication/ | Internal messaging |
| `IAnnouncementService` | Infrastructure/Services/Communication/ | Announcements |
| `ISystemSettingService` | Infrastructure/Services/Communication/ | System settings |
| `IReportService` | Infrastructure/Services/Communication/ | Content reporting |
| `IActivityLogService` | Infrastructure/Services/Communication/ | Audit logging |
| `IEmailService` | Infrastructure/Services/Communication/ | Email sending |
| `INotificationService` | Infrastructure/Services/Communication/ | Notifications |

### 9.5 Other Services

| Service | Location | Description |
|---------|----------|-------------|
| `IProfileService` | Infrastructure/Services/Profile/ | User profile management |
| `ICategoryService` | Infrastructure/Services/Category/ | Category CRUD |
| `ICertificateService` | Infrastructure/Services/Certificate/ | Certificate generation |
| `IReviewService` | Infrastructure/Services/Review/ | Review management |
| `IMediaService` | Infrastructure/Services/Media/ | File upload/download |
| `IObjectStorage` | Infrastructure/Services/Media/ | MinIO integration |
| `IVideoProcessingService` | Infrastructure/Services/Media/ | Video processing |
| `ILiveSessionService` | Infrastructure/Services/LiveSession/ | Live session management |
| `ILiveAttendanceService` | Infrastructure/Services/LiveSession/ | Attendance tracking |
| `IVideoCommentService` | Infrastructure/Services/VideoComment/ | Video comments |
| `INotificationPreferenceService` | Infrastructure/Services/Notification/ | Notification preferences |
| `IContactService` | Infrastructure/Services/Contact/ | Contact form |
| `ILegalPageService` | Infrastructure/Services/Public/ | Legal pages |
| `IPublicService` | Infrastructure/Services/Public/ | Landing page data |
| `ITestimonialService` | Infrastructure/Services/Public/ | Testimonials |
| `IInstructorRequestService` | Infrastructure/Services/InstructorRequests/ | Instructor applications |
| `IAdminUserService` | Infrastructure/Services/Admin/ | Admin user management |
| `IStudentDashboardService` | Infrastructure/Services/Dashboard/ | Student dashboard |
| `IInstructorDashboardService` | Infrastructure/Services/Dashboard/ | Instructor dashboard |
| `IAdminDashboardService` | Infrastructure/Services/Dashboard/ | Admin dashboard |

---

## 10. Real-time (SignalR)

| Hub | URL | Description |
|-----|-----|-------------|
| `NotificationHub` | `/api/hubs/notifications` | Real-time notifications |
| `MessageHub` | `/api/hubs/messaging` | Real-time messaging |

---

## 11. Background Workers

| Worker | Description |
|--------|-------------|
| `VideoProcessingWorker` | Processes uploaded videos |
| `EditRequestCleanupService` | Cleans up expired edit requests |
| `ScheduledDeletionService` | Handles scheduled course deletions |

---

## 12. Infrastructure Services

| Service | Description |
|---------|-------------|
| **SQL Server** | Primary database (port 1433) |
| **MinIO** | Object storage for files, images, videos (port 9000/9001) |
| **Mailpit** | Dev email testing (SMTP port 1025, UI port 8025) |
| **Seq** | Structured logging UI (port 5341/8081) |
| **Redis** | Distributed caching (optional) |

---

## 13. Configuration

### 13.1 Connection Strings

```json
{
  "DefaultConnection": "Server=localhost,1433;Database=Athary;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;MultipleActiveResultSets=true",
  "Redis": ""
}
```

### 13.2 JWT Settings

```json
{
  "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
  "Issuer": "Athary",
  "Audience": "AtharyClient",
  "AccessTokenExpirationMinutes": 60,
  "RefreshTokenExpirationDays": 7
}
```

### 13.3 Email Settings

```json
{
  "SmtpHost": "localhost",
  "SmtpPort": 1025,
  "EnableSsl": false,
  "FromEmail": "noreply@athary.com",
  "FromName": "منصة آثاري التعليمية"
}
```

### 13.4 MinIO Settings

```json
{
  "Endpoint": "localhost:9000",
  "AccessKey": "minioadmin",
  "SecretKey": "minioadmin",
  "Buckets": ["private", "images", "videos", "documents", "recordings", "certificates"]
}
```

---

## 14. Docker & Deployment

### 14.1 Docker Compose Services

| Service | Image | Ports |
|---------|-------|-------|
| sqlserver | mssql/server:2022-latest | 1433:1433 |
| minio | minio/minio:latest | 8002:9000, 9001:9001 |
| mailpit | axllent/mailpit:latest | 8025:8025, 1025:1025 |
| seq | datalust/seq:latest | 5341:5341, 8081:80 |

### 14.2 Dockerfile

- Multi-stage build (SDK → Alpine runtime)
- Non-root user (`appuser`)
- Port 8080
- Health check on `/healthz`

---

## 15. Running the Project

### 15.1 Prerequisites

- .NET 9 SDK
- SQL Server (local or Docker)
- MinIO (optional, for media)

### 15.2 Development

```bash
# Start infrastructure services
cd infrastructure && docker-compose up -d

# Run the API
./run.sh
# or
dotnet run --project src/Athary.API

# API will be available at:
# http://localhost:5000
# http://localhost:5000/swagger (Swagger UI)
# http://localhost:5000/openapi/v1.json (OpenAPI spec)
```

### 15.3 Default Admin Account

- **Email**: admin@lms.com
- **Password**: Admin@123456

### 15.4 Database Migrations

```bash
# Create migration
dotnet ef migrations add <MigrationName> --project src/Athary.Infrastructure --startup-project src/Athary.API

# Apply migration
dotnet ef database update --project src/Athary.Infrastructure --startup-project src/Athary.API
```

---

*Documentation generated for Athary Platform v1.0 — 2026-06-24*
