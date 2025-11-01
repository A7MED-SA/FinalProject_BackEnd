# ERD to Models Mapping

هذا الملف يوضح كيفية تحويل كل جدول من ERD إلى Model في C#.

## 📋 قواعد التحويل

### أنواع البيانات

| ERD Type | C# Type | EF Core Type |
|----------|---------|--------------|
| int | int | int |
| string | string | nvarchar(255) |
| text | string | nvarchar(max) |
| boolean | bool | bit |
| datetime | DateTime | datetime2 |
| decimal | decimal | decimal(18,2) |
| enum | enum | nvarchar |
| json | string | nvarchar(max) |

### تسمية الملفات والـ Classes

| ERD Table | C# Model | File Name |
|-----------|----------|-----------|
| USER | User | User.cs |
| USER_PHONE | UserPhone | UserPhone.cs |
| TEACHER_REQUEST_DOCUMENT | TeacherRequestDocument | TeacherRequestDocument.cs |

**القاعدة:** PascalCase للـ Class Names، مع إزالة الـ underscores.

### تسمية الـ Properties

| ERD Column | C# Property |
|------------|-------------|
| id | Id |
| user_id | UserId |
| password_hash | PasswordHash |
| is_active | IsActive |
| created_at | CreatedAt |

**القاعدة:** PascalCase مع الحفاظ على المعنى.

### Enum Conversion

| ERD Enum Value | C# Enum Value |
|----------------|---------------|
| primary | Primary |
| secondary | Secondary |
| pending_review | PendingReview |
| requires_more_info | RequiresMoreInfo |

**القاعدة:** PascalCase مع إزالة underscores.

## 📊 ERD #1: User & Course

### من: `Logical ERD User & Course.mmd`

| ERD Table | Model File | Key Features |
|-----------|------------|--------------|
| USER | User.cs | - Email unique index<br>- Soft delete (DeletedAt)<br>- Multiple navigation properties |
| USER_PHONE | UserPhone.cs | - PhoneType enum<br>- IsDefault flag |
| SESSION | Session.cs | - Token & RefreshToken hashes<br>- Unique indexes on both |
| ADDRESS | Address.cs | - AddressType enum<br>- Soft delete |
| TEACHER_REQUEST | TeacherRequest.cs | - TeacherRequestStatus enum<br>- Self-referencing (ProcessedBy) |
| TEACHER_REQUEST_DOCUMENT | TeacherRequestDocument.cs | - DocumentType enum |
| ROLE | Role.cs | - Name unique index |
| PERMISSION | Permission.cs | - Name unique index<br>- Resource field |
| ROLE_PERMISSION | RolePermission.cs | - Many-to-many junction |
| USER_ROLE | UserRole.cs | - ExpiresAt for temporary roles |
| CATEGORY | Category.cs | - Self-referencing (ParentCategory)<br>- Slug unique index |
| COURSE | Course.cs | - Multiple enums (Level, Language, Status)<br>- Slug unique index<br>- Soft delete |
| COURSE_REQUIREMENT | CourseRequirement.cs | - DisplayOrder for sorting |
| COURSE_LEARNING_OUTCOME | CourseLearningOutcome.cs | - DisplayOrder for sorting |
| SECTION | Section.cs | - Position for ordering |
| SECTION_ITEM | SectionItem.cs | - SectionItemType enum<br>- Polymorphic relationship (ItemId as Guid)<br>- **NEW:** Navigation properties to Video/Quiz/Document/LiveSession |

## 📊 ERD #2: Commerce & System

### من: `Logical ERD Commerce & System.mmd`

| ERD Table | Model File | Key Features |
|-----------|------------|--------------|
| WISHLIST | Wishlist.cs | - Simple M-N relationship |
| CART | Cart.cs | - User unique index<br>- SessionId for guests |
| CART_ITEM | CartItem.cs | - PriceSnapshot for price tracking |
| REVIEW | Review.cs | - ReviewStatus enum<br>- Helpful counts<br>- Soft delete |
| REVIEW_HELPFUL | ReviewHelpful.cs | - IsHelpful boolean |
| CERTIFICATE | Certificate.cs | - VerificationCode unique<br>- ExpiresAt optional |
| COUPON | Coupon.cs | - CouponType enum<br>- CouponApplicableTo enum<br>- Code unique index |
| COUPON_COURSE | CouponCourse.cs | - Junction table |
| COUPON_USAGE | CouponUsage.cs | - Tracks usage history |
| ORDER | Order.cs | - OrderNumber unique<br>- OrderStatus enum<br>- Multiple decimal fields |
| ORDER_ITEM | OrderItem.cs | - PriceAtPurchase snapshot |
| PAYMENT_METHOD | PaymentMethod.cs | - PaymentMethodType enum<br>- JSON configuration |
| PAYMENT | Payment.cs | - TransactionRef unique<br>- PaymentStatus & PaymentCurrency enums |
| REFUND | Refund.cs | - RefundStatus enum |
| TRANSACTION_LOG | TransactionLog.cs | - Audit trail |
| FILE | UploadedFile.cs | - FileEntityType enum<br>- Polymorphic (EntityId)<br>- Soft delete<br>- **Note:** Named UploadedFile to avoid conflict with System.IO.File |
| NOTIFICATION | Notification.cs | - NotificationType enum<br>- ReadAt tracking |
| MESSAGE | Message.cs | - Sender/Receiver pattern<br>- IsRead tracking |
| ANNOUNCEMENT | Announcement.cs | - AnnouncementTarget enum<br>- Expiration support |
| REPORT | Report.cs | - Multiple enums<br>- Polymorphic entity |
| ACTIVITY_LOG | ActivityLog.cs | - ActivityLogEntityType enum<br>- IP tracking |
| COURSE_LOG | CourseLog.cs | - CourseLogAction enum |
| SYSTEM_SETTING | SystemSetting.cs | - SettingDataType enum<br>- Key unique index |

## 📊 ERD #3: Content & Learning

### من: `Logical ERD Content & Learning.mmd`

| ERD Table | Model File | Key Features |
|-----------|------------|--------------|
| VIDEO | Video.cs | - VideoProvider enum<br>- VideoQuality enum (with underscore)<br>- VideoStatus enum<br>- **NEW:** Reverse navigation to SectionItem |
| DOCUMENT | Document.cs | - DocumentFileType enum<br>- **NEW:** Reverse navigation to SectionItem |
| VIDEO_COMMENT | VideoComment.cs | - Self-referencing (ParentComment)<br>- Soft delete |
| COMMENT_LIKE | CommentLike.cs | - Simple like tracking |
| QUIZ | Quiz.cs | - Multiple boolean settings<br>- PassingScore percentage<br>- **NEW:** Reverse navigation to SectionItem |
| QUESTION | Question.cs | - QuestionType enum<br>- Points system<br>- Position ordering |
| OPTION | Option.cs | - IsCorrect flag<br>- Position ordering |
| QUIZ_ATTEMPT | QuizAttempt.cs | - QuizAttemptStatus enum<br>- Score tracking |
| USER_ANSWER | UserAnswer.cs | - Optional SelectedOptionId<br>- Optional AnswerText |
| LIVE_SESSION | LiveSession.cs | - LiveSessionStatus enum<br>- Scheduled vs Actual times<br>- **NEW:** Reverse navigation to SectionItem |
| LIVE_ATTENDANCE | LiveAttendance.cs | - Duration calculation |
| ENROLLMENT | Enrollment.cs | - EnrollmentStatus enum<br>- EnrollmentSource enum<br>- Progress percentage |
| CONTENT_PROGRESS | ContentProgress.cs | - ContentType enum<br>- Polymorphic content<br>- JSON metadata |

## 🔗 العلاقات المعقدة

### Self-Referencing Relationships

1. **Category → SubCategories**
   - ParentCategoryId → ParentCategory
   - SubCategories collection

2. **VideoComment → Replies**
   - ParentCommentId → ParentComment
   - Replies collection

### Polymorphic Relationships

1. **SectionItem** ✨ Updated
   - ItemType (enum): Video, Quiz, Document, LiveSession
   - ItemId (Guid): ID of the actual content
   - **Navigation Properties:**
     - `Video? Video`
     - `Quiz? Quiz`
     - `Document? Document`
     - `LiveSession? LiveSession`
   - **Reverse Navigation:** Each content type has `SectionItem? SectionItem`

2. **ContentProgress**
   - ContentType (enum): Video, Quiz, Document, LiveSession
   - ContentId (Guid): ID of the actual content

3. **File**
   - EntityType (enum): ProfilePicture, CourseImage, etc.
   - EntityId (int): ID of the entity

4. **Report**
   - EntityType (enum): Course, Review, Comment, User
   - EntityId (int): ID of reported entity

### Multiple Foreign Keys to Same Table

1. **TeacherRequest**
   - UserId → User (who submitted)
   - ProcessedBy → ProcessedByUser (who processed)

2. **Course**
   - CreatedBy → Creator
   - ApprovedBy → Approver

3. **Message**
   - SenderId → Sender
   - ReceiverId → Receiver

## 🎯 ميزات خاصة

### Soft Delete Pattern

Models التي تدعم Soft Delete (DeletedAt):
- User
- Address
- Course
- Review
- File
- VideoComment

### Audit Trail Pattern

Models مع Timestamps كاملة:
- User (CreatedAt, UpdatedAt, DeletedAt)
- Course (CreatedAt, UpdatedAt, DeletedAt)
- Cart (CreatedAt, UpdatedAt)
- Order (CreatedAt, UpdatedAt)

### Snapshot Pattern

Models التي تحفظ snapshot للأسعار:
- CartItem → PriceSnapshot
- OrderItem → PriceAtPurchase

### Default Values

أمثلة على Default Values:
- `IsActive = true`
- `CreatedAt = DateTime.UtcNow`
- `Status = Pending`
- `Progress = 0`

## 📐 Indexes Strategy

### Unique Indexes

تم إضافة unique indexes على:
- User.Email
- Session.TokenHash, RefreshTokenHash
- Role.Name
- Permission.Name
- Category.Slug
- Course.Slug
- Certificate.VerificationCode
- Coupon.Code
- Order.OrderNumber
- Payment.TransactionRef
- SystemSetting.Key
- Cart.UserId (one cart per user)

### Foreign Key Indexes

EF Core تنشئ indexes تلقائياً على جميع Foreign Keys.

## 🔄 Navigation Properties

### One-to-Many
```csharp
// في الـ Parent
public virtual ICollection<Child> Children { get; set; }

// في الـ Child
[ForeignKey("ParentId")]
public virtual Parent Parent { get; set; }
```

### Many-to-Many (Junction Table)
```csharp
// RolePermission
public virtual Role Role { get; set; }
public virtual Permission Permission { get; set; }
```

### Self-Referencing
```csharp
// Category
public virtual Category? ParentCategory { get; set; }
public virtual ICollection<Category> SubCategories { get; set; }
```

## ✅ Validation Summary

### Required Fields
جميع Foreign Keys مُعرّفة كـ `[Required]` إلا إذا كانت nullable.

### MaxLength
- Codes, Names: 50-255
- URLs: 500
- Descriptions: text (no limit)
- IPs: 45 (IPv6 support)

### Data Types
- Decimals: (18,2) للأموال
- Decimals: (5,2) للنسب المئوية
- Decimals: (3,2) للتقييمات

---

**تم التوثيق الكامل للتحويل من ERD إلى Models 📝**
