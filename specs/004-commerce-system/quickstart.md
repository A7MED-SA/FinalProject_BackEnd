# Quickstart: Commerce System

**Date**: 2026-05-23 | **Feature**: [spec.md](./spec.md)

## Prerequisites

- المشروع شغال (dotnet run)
- SQL Server متصل
- الـ 12 Commerce model موجودة مسبقاً في `models/` و `ApplicationDbContext`

## الـ Build

```bash
# المشروع كله
dotnet build backend_project/backend_project.csproj

# لو عايز تشوف الأخطاء بالتفصيل
dotnet build backend_project/backend_project.csproj --no-restore 2>&1
```

## الـ Migrations

**مافيش حاجة** — الـ models موجودة فعلاً في DbContext و Migrations سابقة. لو حصل حذف للميجرشن، run:

```bash
dotnet ef migrations add AddCommerceSystem -p backend_project/
dotnet ef database update -p backend_project/
```

## هيكل الملفات الجديدة

### Services
```bash
# Interfaces
touch backend_project/services/Interfaces/ICartService.cs
touch backend_project/services/Interfaces/IOrderService.cs
touch backend_project/services/Interfaces/ICouponService.cs
touch backend_project/services/Interfaces/IPaymentService.cs
touch backend_project/services/Interfaces/IPaymentGateway.cs
touch backend_project/services/Interfaces/IRefundService.cs
touch backend_project/services/Interfaces/IWishlistService.cs

# Implementations
touch backend_project/services/Implementations/CartService.cs
touch backend_project/services/Implementations/OrderService.cs
touch backend_project/services/Implementations/CouponService.cs
touch backend_project/services/Implementations/PaymentService.cs
touch backend_project/services/Implementations/PaymentGateway/MockPaymentGateway.cs
touch backend_project/services/Implementations/RefundService.cs
touch backend_project/services/Implementations/WishlistService.cs
```

### Controllers
```bash
mkdir -p backend_project/Controllers/Admin
touch backend_project/Controllers/CartController.cs
touch backend_project/Controllers/OrderController.cs
touch backend_project/Controllers/CouponController.cs
touch backend_project/Controllers/WishlistController.cs
touch backend_project/Controllers/Admin/AdminCouponController.cs
touch backend_project/Controllers/Admin/AdminPaymentMethodController.cs
touch backend_project/Controllers/Admin/AdminRefundController.cs
```

### DTOs
```bash
mkdir -p backend_project/DTOs/{Cart,Order,Coupon,Payment,Refund,Wishlist}
touch backend_project/DTOs/Cart/CartDtos.cs
touch backend_project/DTOs/Order/OrderDtos.cs
touch backend_project/DTOs/Coupon/CouponDtos.cs
touch backend_project/DTOs/Payment/PaymentDtos.cs
touch backend_project/DTOs/Refund/RefundDtos.cs
touch backend_project/DTOs/Wishlist/WishlistDtos.cs
```

### Validators
```bash
mkdir -p backend_project/Validators/{Cart,Order,Coupon,Payment,Refund}
touch backend_project/Validators/Cart/AddToCartValidator.cs
touch backend_project/Validators/Order/CreateOrderValidator.cs
touch backend_project/Validators/Coupon/ApplyCouponValidator.cs
touch backend_project/Validators/Coupon/CreateCouponValidator.cs
touch backend_project/Validators/Payment/ProcessPaymentValidator.cs
touch backend_project/Validators/Refund/RequestRefundValidator.cs
touch backend_project/Validators/Refund/ProcessRefundValidator.cs
```

### Tests
```bash
mkdir -p tests/Unit tests/Integration
touch tests/Unit/CartServiceTests.cs
touch tests/Unit/OrderServiceTests.cs
touch tests/Unit/CouponServiceTests.cs
touch tests/Unit/PaymentServiceTests.cs
touch tests/Unit/RefundServiceTests.cs
touch tests/Unit/WishlistServiceTests.cs
touch tests/Integration/CommerceFlowTests.cs
```

## التسجيل في Program.cs (DI)

```csharp
// Services
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IPaymentGateway, MockPaymentGateway>();
builder.Services.AddScoped<IRefundService, RefundService>();
builder.Services.AddScoped<IWishlistService, WishlistService>();
```

## اختبار الدفع بـ Mock Gateway

```http
# 1. إنشاء طلب
POST /api/orders
Content-Type: application/json
Authorization: Bearer <token>

{}

# 2. دفع الطلب بنجاح
POST /api/orders/{orderId}/payments
Content-Type: application/json
Authorization: Bearer <token>

{
  "paymentMethodId": "00000000-0000-0000-0000-000000000001",
  "amount": 948.00
}
# Result: Status 200, payment Succeeded

# 3. محاكاة فشل الدفع (استخدم methodId خاص)
# paymentMethodId = guid with specific prefix to simulate failure
# Mock يقبل success/fail حسب الـ paymentMethodId
```

## ترتيب التنفيذ

1. MockPaymentGateway — أبسط حاجة، مش محتاجة Dependency
2. CouponService + CouponController + AdminCouponController
3. CartService + CartController
4. OrderService + OrderController
5. PaymentService + Checkout endpoints
6. RefundService + Admin/Student Refund Controllers
7. WishlistService + WishlistController
8. Validators لكل الخطوات
9. Tests لكل Service
10. تسجيل DI في Program.cs
