# Research: Commerce System

**Date**: 2026-05-23 | **Feature**: [spec.md](./spec.md) | **Plan**: [plan.md](./plan.md)

## Technical Decisions

### 1. Payment Gateway Abstraction

**Decision**: Interface `IPaymentGateway` مع تنفيذ `MockPaymentGateway` داخلي

**Rationale**:
- يسمح باختبار تدفق الدفع بدون اتصال خارجي (كما تم التوضيح في Clarification Q3)
- Stripe/PayPal integration تكون swapping سهل بعد v1
- Mock يقدر يحاكي Success/Failure/Timeout

**Alternatives considered**:
- Stripe SDK مباشر — lock-in مبكر ويمنع الاختبار بدون Sandbox
- واجهة abstract بدون Mock — يضيف تعقيد بدون فائدة

### 2. Transaction Atomicity

**Decision**: استخدام `IDbContextTransaction` (EF Core native) لكل العمليات المالية

**Rationale**: 
- العمليات المالية لازم تكون Atomic: لو فشل أي خطوة في سلسلة (طلب → دفع → تسجيل)، كل شيء يرجع للوراء
- EF Core `Database.BeginTransactionAsync` متاح حالياً في المشروع

**Pattern**:
```
BeginTransaction → CreateOrder → CreatePayment → ProcessPayment → 
  if Success → EnrollUser → Commit
  if Failed → Rollback
```

**Alternatives considered**:
- Outbox Pattern — overengineering على v1
- Manual compensation — risky و error-prone

### 3. Price Protection Strategy

**Decision**: استخدام PriceSnapshot في CartItem + PriceAtPurchase في OrderItem

**Rationale**:
- PriceSnapshot يحفظ السعر وقت إضافة الكورس للسلة (حتى لو تغير السعر بعد كدا)
- PriceAtPurchase هو السعر الفعلي وقت إنشاء الطلب
- عند تحويل السلة لطلب، ننسخ PriceSnapshot لـ PriceAtPurchase

### 4. Order Number Generation

**Decision**: `ORD-{YYYYMMDD}-{6-digit sequential}`

**Rationale**:
- مقروء للبشر
- يحتوي على التاريخ لتسهيل التتبع
- 6 أرقام متسلسلة لكل طلب (يكفي ~999,999 طلب في اليوم)

**Example**: `ORD-20260523-000001`

### 5. Mock Payment Gateway Design

**Decision**: Mock يقبل `paymentMethodId` كـ parameter للتحكم في النتيجة

**Behavior**:
- `methodId == "pay-success"` → نجاح
- `methodId == "pay-fail"` → فشل
- أي methodId تاني → نجاح (محاكاة طبيعية)
- يستخدم `Task.Delay(100)` لمحاكاة زمن المعالجة

### 6. Integration with Existing Services

**EnrollmentService**: 
- بعد نجاح الدفع → استدعاء `EnrollUserAsync` لكل Course في الطلب
- `Source` يكون `EnrollmentSource.Purchase`

**NotificationService**:
- نجاح الدفع → إشعار "تم شراء الكورس بنجاح"
- الاسترداد → إشعار "تم استرداد المبلغ"
- فشل الدفع → إشعار "فشلت عملية الدفع، حاول مرة أخرى"

**ActivityLogService**:
- تسجيل كل عملية: "CreateOrder", "PaymentSuccess", "PaymentFailed", "RefundRequested", "RefundApproved"

### 7. Models موجودة مسبقاً — لا نحتاج Migrations جديدة

جميع الـ 12 model موجودة فعلاً في `ApplicationDbContext`:
- Cart, CartItem, Order, OrderItem, Payment, PaymentMethod, Refund, TransactionLog, Coupon, CouponCourse, CouponUsage, Wishlist
- Relationships والـ Foreign Keys كلها مضبوطة مسبقاً
- لا حاجة لأي تغيير في Data Layer
