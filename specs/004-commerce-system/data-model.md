# Data Model: Commerce System

**Date**: 2026-05-23

> جميع الـ 12 entity موجودة مسبقاً في `backend_project/models/`. هذا المستند يوثق الفالديشن والعلاقات وحالات الـ State Machines المطلوبة للسيرفس layer الجديد.

## Entity Inventory

### 1. Cart (`carts`)
**Status**: ✅ Model exists

| Field | Type | Constraint | Notes |
|-------|------|-----------|-------|
| Id | Guid (PK) | BaseEntity | Sequential GUID |
| UserId | Guid (FK) | Required → User | Unique per user (1 cart per user) |
| SessionId | string?(255) | | للـ guest users (خارج v1) |
| CreatedAt | DateTime | Default: UtcNow | |
| UpdatedAt | DateTime? | | |
| ExpiresAt | DateTime? | | Cart expiration |

**Validation Rules**:
- UserId + SessionId: واحد منهم لازم يكون موجود
- Cart per user: واحد فقط (Unique constraint on UserId)

**Relationships**: User (N:1), CartItems (1:N)

---

### 2. CartItem (`cart_items`)
**Status**: ✅ Model exists

| Field | Type | Constraint | Notes |
|-------|------|-----------|-------|
| Id | Guid (PK) | BaseEntity | |
| CartId | Guid (FK) | Required → Cart | |
| CourseId | Guid (FK) | Required → Course | |
| PriceSnapshot | decimal(18,2) | Required | السعر وقت الإضافة للسلة |
| AddedAt | DateTime | Default: UtcNow | |

**Validation Rules**:
- CourseId فريد داخل نفس Cart (ماينفعش add نفس الكورس مرتين)
- PriceSnapshot ياخد السعر الحالي للكورس وقت إضافة الـ Item
- Course لازم يكون Published

**Relationships**: Cart (N:1), Course (N:1)

---

### 3. Coupon (`coupons`)
**Status**: ✅ Model exists

| Field | Type | Constraint | Notes |
|-------|------|-----------|-------|
| Id | Guid (PK) | BaseEntity | |
| Code | string(50) | Required, Unique | كود الخصم |
| Type | CouponType (enum) | Required | Percentage / Fixed |
| Value | decimal(18,2) | Required | النسبة أو القيمة |
| MaxDiscountAmount | decimal(18,2)? | | أقصى خصم للـ Percentage coupons |
| MinimumPurchaseAmount | decimal(18,2)? | الحد الأدنى للشراء |
| ApplicableTo | CouponApplicableTo | Default: All | All / SpecificCourses / Category |
| UsageLimit | int? | | عدد مرات الاستخدام الكلي |
| UserLimitPerUser | int? | | عدد مرات الاستخدام لكل مستخدم |
| TimesUsed | int | Default: 0 | عداد الاستخدام (يزيد مع كل استخدام) |
| IsPublic | bool | Default: false | ظاهر للكل ولا مخفي |
| ValidFrom | DateTime? | بداية الصلاحية |
| ValidUntil | DateTime? | نهاية الصلاحية |
| IsActive | bool | Default: true | مفعل أم لا |
| CreatedBy | Guid (FK) | Required → User | |
| CreatedAt | DateTime | Default: UtcNow | |

**State Machine**:
```
Inactive ←── Active ←── Expired (ValidUntil < now)
                │
                └──→ Exhausted (TimesUsed >= UsageLimit)
```

**Validation Rules**:
- Code: فريد (Unique Index في DbContext)
- Value: لو Percentage لازم بين 0 و 100
- ValidFrom < ValidUntil (لو الاتنين موجودين)
- لو ApplicableTo = SpecificCourses، لازم CouponCourses تكون مش فاضية

**Relationships**: Creator/User (N:1), CouponCourses (1:N), CouponUsages (1:N), Orders (1:N)

---

### 4. CouponCourse (`coupon_courses`)
**Status**: ✅ Model exists

| Field | Type | Constraint |
|-------|------|-----------|
| Id | Guid (PK) | BaseEntity |
| CouponId | Guid (FK) | Required → Coupon |
| CourseId | Guid (FK) | Required → Course |

**Relationships**: Coupon (N:1), Course (N:1)

---

### 5. CouponUsage (`coupon_usages`)
**Status**: ✅ Model exists

| Field | Type | Constraint |
|-------|------|-----------|
| Id | Guid (PK) | BaseEntity |
| CouponId | Guid (FK) | Required → Coupon |
| UserId | Guid (FK) | Required → User |
| OrderId | Guid (FK) | Required → Order |
| UsedAt | DateTime | Default: UtcNow |

---

### 6. Order (`orders`)
**Status**: ✅ Model exists

| Field | Type | Constraint | Notes |
|-------|------|-----------|-------|
| Id | Guid (PK) | BaseEntity | |
| OrderNumber | string(50) | Required, Unique | ORD-YYYYMMDD-XXXXXX |
| UserId | Guid (FK) | Required → User | |
| BillingAddressId | Guid? (FK) | → Address | |
| SubtotalAmount | decimal(18,2) | Required | مجموع أسعار الكورسات قبل الخصم |
| CouponId | Guid? (FK) | → Coupon | |
| DiscountAmount | decimal(18,2) | Default: 0 | |
| TaxAmount | decimal(18,2) | Default: 0 | |
| FinalAmount | decimal(18,2) | Required | Subtotal - Discount + Tax |
| Currency | string(3) | Default: "EGP" | |
| Status | OrderStatus (enum) | Default: Pending | Pending → Completed / Failed / Cancelled / Refunded |
| Notes | string? | | |
| CreatedAt | DateTime | Default: UtcNow | |
| UpdatedAt | DateTime? | | |

**State Machine**:
```
                ┌──→ Completed ──→ Refunded
Pending ────→ Failed
                └──→ Cancelled (بعد 3 failed payments)
```

**Valid Transitions**:
| From | To | متى |
|------|----|-----|
| Pending | Completed | نجاح الدفع |
| Pending | Failed | فشل الدفع |
| Pending | Cancelled | 3 محاولات فاشلة أو إلغاء يدوي (خارج v1) |
| Completed | Refunded | الموافقة على الاسترداد |

**Relationships**: User (N:1), Address (N:1), Coupon (N:1), OrderItems (1:N), Payments (1:N), CouponUsages (1:N)

---

### 7. OrderItem (`order_items`)
**Status**: ✅ Model exists

| Field | Type | Constraint |
|-------|------|-----------|
| Id | Guid (PK) | BaseEntity |
| OrderId | Guid (FK) | Required → Order |
| CourseId | Guid (FK) | Required → Course |
| PriceAtPurchase | decimal(18,2) | Required |
| AddedAt | DateTime | Default: UtcNow |

**Relationships**: Order (N:1), Course (N:1)

---

### 8. Payment (`payments`)
**Status**: ✅ Model exists

| Field | Type | Constraint | Notes |
|-------|------|-----------|-------|
| Id | Guid (PK) | BaseEntity | |
| UserId | Guid (FK) | Required → User | |
| OrderId | Guid (FK) | Required → Order | |
| PaymentMethodId | Guid (FK) | Required → PaymentMethod | |
| Amount | decimal(18,2) | Required | |
| Currency | PaymentCurrency (enum) | Default: EGP | |
| Status | PaymentStatus (enum) | Default: Pending | Pending → Succeeded / Failed |
| TransactionRef | string(255) | Required, Unique | |
| GatewayResponse | string(2000)? | | استجابة الـ Gateway |
| PaidAt | DateTime? | | |
| CreatedAt | DateTime | Default: UtcNow | |

**State Machine**:
```
Pending ──→ Succeeded
Pending ──→ Failed
```
(كل محاولة دفع جديدة = Payment record جديد، كل طلب يقدر يكون ليه Payments متعددة)

**Relationships**: User (N:1), Order (N:1), PaymentMethod (N:1), TransactionLogs (1:N), Refunds (1:N)

---

### 9. PaymentMethod (`payment_methods`)
**Status**: ✅ Model exists

| Field | Type | Constraint |
|-------|------|-----------|
| Id | Guid (PK) | BaseEntity |
| Name | string(100) | Required |
| Provider | string(100) | Required |
| Type | PaymentMethodType (enum) | Required |
| IsActive | bool | Default: true |
| Configuration | string? | JSON config للـ Gateway |

---

### 10. TransactionLog (`transaction_logs`)
**Status**: ✅ Model exists

| Field | Type | Constraint |
|-------|------|-----------|
| Id | Guid (PK) | BaseEntity |
| PaymentId | Guid (FK) | Required → Payment |
| Action | string(100) | Required |
| Status | string(50) | Required |
| Details | string? | |
| CreatedAt | DateTime | Default: UtcNow |

---

### 11. Refund (`refunds`)
**Status**: ✅ Model exists

| Field | Type | Constraint | Notes |
|-------|------|-----------|-------|
| Id | Guid (PK) | BaseEntity | |
| PaymentId | Guid (FK) | Required → Payment | |
| Amount | decimal(18,2) | Required | |
| Reason | string? | سبب الاسترداد | |
| Status | RefundStatus (enum) | Default: Requested | |
| RequestedAt | DateTime | Default: UtcNow | |
| ProcessedAt | DateTime? | | |
| ProcessedBy | Guid? (FK) | → User (Admin) | |

**State Machine**:
```
Requested ──→ Approved ──→ Processed
Requested ──→ Rejected
```

**Validation Rules**:
- Amount: لازم يكون ≤ Payment.Amount
- يقدر يكون في أكتر من Refund لنفس الـ Payment (استرداد جزئي)
- Approval: أي Admin (FR-024)

**Relationships**: Payment (N:1), ProcessedBy/User (N:1)

---

### 12. Wishlist (`wishlists`)
**Status**: ✅ Model exists

| Field | Type | Constraint |
|-------|------|-----------|
| Id | Guid (PK) | BaseEntity |
| UserId | Guid (FK) | Required → User |
| CourseId | Guid (FK) | Required → Course |
| AddedAt | DateTime | Default: UtcNow |

**Validation**: UserId + CourseId فريد (مع بعض) — ماينفعش تكرار
