# Implementation Plan: Commerce System

**Branch**: `004-commerce-system` | **Date**: 2026-05-23 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `specs/004-commerce-system/spec.md`

## Summary

تنفيذ نظام تجارة إلكترونية كامل لمنصة Athary LMS — 12 model موجودة حالياً (Cart, CartItem, Order, OrderItem, Payment, PaymentMethod, Refund, TransactionLog, Coupon, CouponCourse, CouponUsage, Wishlist) بدون Services ولا Controllers. الهدف: تمكين الطلاب من شراء الكورسات عبر سلة تسوق → كوبون خصم → طلب → دفع → تسجيل تلقائي، مع دعم الاسترداد وقائمة الأمنيات.

## Technical Context

**Language/Version**: C# 12 / .NET 9.0  
**Primary Dependencies**: ASP.NET Core Web API, Entity Framework Core 9.0, FluentValidation, xUnit, Moq  
**Storage**: SQL Server (via EF Core), MinIO for any receipt/invoice files  
**Testing**: xUnit + Moq للـ Unit Tests, WebApplicationFactory للـ Integration Tests  
**Target Platform**: Linux server (Web API)  
**Project Type**: Web Service — REST API  
**Performance Goals**: 
- السلة والطلبات: < 500ms للاستعلام
- 100 عملية دفع متزامنة بدون فقدان بيانات
- الخصم وتطبيق الكوبون: < 200ms  
**Constraints**: 
- العمليات المالية لازم تكون في Transaction واحد (Atomic)
- PriceSnapshot/PriceAtPurchase لمنع تغير السعر بعد الإضافة للسلة
- 3 محاولات دفع كحد أقصى للطلب الواحد
- Mock Payment Gateway للاختبار (بدون Stripe/PayPal حقيقي)
**Scale/Scope**: 56 Entities total, ~12 new services/controllers for commerce

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] **I. Clean Code Foundations**: SOLID + DRY + KISS. Services مسئولة عن Business Logic، Controllers خفيفة.
- [x] **II. C# & .NET Best Practices**: async/await في كل مكان، DI عبر constructor، عدم استخدام `.Result` أو `.Wait()`.
- [x] **III. Naming & Formatting**: PascalCase للكلاسات والميثودات، camelCase للمتغيرات المحلية، I- prefix للـ Interfaces.
- [x] **IV. Structure**: كل Service له Interface، كل Controller له Service واحد. Services صغيرة الحجم.
- [x] **V. Error Handling**: FluentValidation للمدخلات، Exception Middleware موجود مسبقاً، ApiResponse نمط موحد.

## Project Structure

### Documentation (this feature)

```text
specs/004-commerce-system/
├── plan.md              # This file
├── research.md          # Phase 0 — technical decisions
├── data-model.md        # Phase 1 — entities & DTOs
├── quickstart.md        # Phase 1 — how to build & test
└── contracts/           # Phase 1 — API endpoints
```

### Source Code (backend_project/)

```text
backend_project/
├── Controllers/
│   ├── CartController.cs
│   ├── OrderController.cs
│   ├── CouponController.cs
│   ├── WishlistController.cs
│   └── Admin/
│       ├── AdminCouponController.cs
│       ├── AdminPaymentMethodController.cs
│       └── AdminRefundController.cs
├── services/
│   ├── Interfaces/
│   │   ├── ICartService.cs
│   │   ├── IOrderService.cs
│   │   ├── ICouponService.cs
│   │   ├── IPaymentService.cs
│   │   ├── IPaymentGateway.cs
│   │   ├── IRefundService.cs
│   │   └── IWishlistService.cs
│   └── Implementations/
│       ├── CartService.cs
│       ├── OrderService.cs
│       ├── CouponService.cs
│       ├── PaymentService.cs
│       ├── MockPaymentGateway.cs
│       ├── RefundService.cs
│       └── WishlistService.cs
├── DTOs/
│   ├── Cart/
│   ├── Order/
│   ├── Coupon/
│   ├── Payment/
│   └── Refund/
├── Validators/
│   ├── Cart/
│   ├── Order/
│   ├── Coupon/
│   └── Payment/
└── tests/
    ├── Unit/
    │   ├── CartServiceTests.cs
    │   ├── OrderServiceTests.cs
    │   ├── CouponServiceTests.cs
    │   ├── PaymentServiceTests.cs
    │   ├── RefundServiceTests.cs
    │   └── WishlistServiceTests.cs
    └── Integration/
        └── CommerceFlowTests.cs
```

**Structure Decision**: Single project (backend_project/) — نفس هيكل المشروع الحالي. كل Component جديد يتبع نفس النمط: Controller → Service (Interface + Implementation) → Validator → DTO. Models موجودة مسبقاً. Tests في مجلد tests/ جديد.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| — | — | — |
