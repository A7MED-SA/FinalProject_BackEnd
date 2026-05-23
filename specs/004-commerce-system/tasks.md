# Tasks: Commerce System

**Input**: Design documents from `/specs/004-commerce-system/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/api.md ✅, quickstart.md ✅

**Tests**: مطلوبة — الـ spec يذكر "مع الاختبار" و SC-008 تنص على 100% تمرير للاختبارات

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: إنشاء البنية التحتية المشتركة لكل الـ User Stories

- [x] T001 [P] إنشاء مجلدات DTOs المطلوبة في `backend_project/DTOs/Cart/`, `Order/`, `Coupon/`, `Payment/`, `Refund/`, `Wishlist/`
- [x] T002 [P] إنشاء مجلدات Validators المطلوبة في `backend_project/Validators/Cart/`, `Order/`, `Coupon/`, `Payment/`, `Refund/`
- [x] T003 [P] إنشاء مجلد Tests في `tests/Unit/` و `tests/Integration/`
- [x] T004 [P] إنشاء واجهة `IPaymentGateway` في `backend_project/services/Interfaces/IPaymentGateway.cs`
- [x] T005 تنفيذ `MockPaymentGateway` في `backend_project/services/Implementations/PaymentGateway/MockPaymentGateway.cs` (يدعم Success/Failure/Timeout)
- [x] T006 إنشاء ملف `ApiResponse` موجود بالفعل — تأكيد استخدامه لكل الـ Responses
- [x] T007 تسجيل كل الـ Services الجديدة في `backend_project/Program.cs` (DI)

---

## Phase 2: User Story 1 - إدارة سلة التسوق (Priority: P1) 🎯 MVP

**Goal**: الطالب يقدر يضيف كورسات لسلة التسوق، يشوفها، يحذفها، ويطبق كوبون خصم
**Independent Test**: Add course → verify in cart → add another → verify total → remove one → verify total

### Tests for User Story 1 ⚠️

- [x] T008 [P] [US1] Unit test: AddToCart — يضيف كورس جديد للسلة في `tests/CommerceTests/CartServiceTests.cs`
- [x] T009 [P] [US1] Unit test: AddToCart — يمنع إضافة كورس موجود بالفعل في `tests/CommerceTests/CartServiceTests.cs`
- [x] T010 [P] [US1] Unit test: AddToCart — يمنع إضافة كورس مسجل به في `tests/CommerceTests/CartServiceTests.cs`
- [x] T011 [P] [US1] Unit test: RemoveFromCart — يحذف Item من السلة في `tests/CommerceTests/CartServiceTests.cs`
- [x] T012 [US1] Unit test: GetCart — يرجع السلة مع items والمجموع في `tests/CommerceTests/CartServiceTests.cs`

### Implementation for User Story 1

- [x] T013 [P] [US1] إنشاء `CartDtos.cs` في `backend_project/DTOs/Cart/CartDtos.cs` (CartResponseDto, AddToCartRequest, CartItemDto)
- [x] T014 [P] [US1] إنشاء `AddToCartValidator.cs` في `backend_project/Validators/Cart/AddToCartValidator.cs`
- [x] T015 [P] [US1] إنشاء واجهة `ICartService` في `backend_project/services/Interfaces/ICartService.cs`
- [x] T016 [US1] تنفيذ `CartService` في `backend_project/services/Implementations/CartService.cs` (GetCart, AddItem, RemoveItem)
- [x] T017 [US1] إنشاء `CartController` في `backend_project/Controllers/CartController.cs` (GET /api/cart, POST /api/cart/items, DELETE /api/cart/items/{itemId})
- [x] T018 [US1] تسجيل `ICartService` و `CartService` في Program.cs

**Checkpoint**: US1 كامل — السلة شغالة مع الاختبارات ✅

---

## Phase 3: User Story 2 - نظام الكوبونات والخصم (Priority: P1)

**Goal**: المسؤول يدير كوبونات الخصم (إنشاء/تعديل/تعطيل)، والطالب يطبق كود خصم على السلة
**Independent Test**: Admin create coupon → Student apply to cart → verify discount → try invalid code → error

### Tests for User Story 2 ⚠️

- [x] T019 [P] [US2] Unit test: CreateCoupon — ينشئ كوبون Percentage صحيح في `tests/CommerceTests/CouponServiceTests.cs`
- [x] T020 [P] [US2] Unit test: ValidateCoupon — كود صحيح يرجع نجاح في `tests/CommerceTests/CouponServiceTests.cs`
- [x] T021 [P] [US2] Unit test: ValidateCoupon — كود منتهي الصلاحية يرجع خطأ في `tests/CommerceTests/CouponServiceTests.cs`
- [x] T022 [P] [US2] Unit test: ValidateCoupon — كود استنفذ الاستخدام يرجع خطأ في `tests/CommerceTests/CouponServiceTests.cs`
- [x] T023 [P] [US2] Unit test: ValidateCoupon — minimum purchase not met يرجع خطأ في `tests/CommerceTests/CouponServiceTests.cs`
- [x] T024 [US2] Unit test: ApplyCoupon — يطبق كوبون على السلة ويحسب الخصم في `tests/CommerceTests/CouponServiceTests.cs`
- [x] T025 [US2] Unit test: ApplyCoupon — كود SpecificCourses على كورس غير مشمول يرجع خطأ في `tests/CommerceTests/CouponServiceTests.cs`

### Implementation for User Story 2

- [x] T026 [P] [US2] إنشاء `CouponDtos.cs` في `backend_project/DTOs/Coupon/CouponDtos.cs` (CreateCouponDto, CouponResponseDto, ApplyCouponRequest, ValidateCouponRequest, ValidateCouponResponse)
- [x] T027 [P] [US2] إنشاء `CreateCouponValidator.cs` في `backend_project/Validators/Coupon/CreateCouponValidator.cs`
- [x] T028 [P] [US2] إنشاء `ApplyCouponValidator.cs` في `backend_project/Validators/Coupon/ApplyCouponValidator.cs`
- [x] T029 [P] [US2] إنشاء واجهة `ICouponService` في `backend_project/services/Interfaces/ICouponService.cs`
- [x] T030 [US2] تنفيذ `CouponService` في `backend_project/services/Implementations/CouponService.cs` (CRUD + Validate + Apply)
- [x] T031 [US2] إنشاء `CouponController` (Student) في `backend_project/Controllers/CouponController.cs` (POST /api/coupons/validate)
- [x] T032 [US2] إنشاء `AdminCouponController` في `backend_project/Controllers/Admin/AdminCouponController.cs` (GET/POST/PUT/DELETE /api/admin/coupons)
- [x] T033 [US2] ربط Apply Coupon مع Cart Service (تطبيق كود الخصم على السلة)
- [x] T034 [P] [US2] تسجيل `ICouponService` و `CouponService` in Program.cs

**Checkpoint**: US1 + US2 كاملين — السلة مع الكوبونات شغالة ✅

---

## Phase 4: User Story 3 - إنشاء الطلب واتمام الشراء (Priority: P1)

**Goal**: الطالب يحول السلة لطلب، يدفع، ويتسجل في الكورسات تلقائياً
**Independent Test**: Add courses to cart → Create order → Process payment → Verify enrollment created

### Tests for User Story 3 ⚠️

- [x] T035 [P] [US3] Unit test: CreateOrder — يحول السلة لطلب بحالة Pending في `tests/CommerceTests/OrderServiceTests.cs`
- [x] T036 [P] [US3] Unit test: CreateOrder — سلة فاضية ترمي خطأ في `tests/CommerceTests/OrderServiceTests.cs`
- [x] T037 [P] [US3] Unit test: ProcessPayment — دفع ناجح يحول الطلب لـ Completed في `tests/CommerceTests/PaymentServiceTests.cs`
- [x] T038 [P] [US3] Unit test: ProcessPayment — دفع ناجح ينشئ Enrollment في `tests/CommerceTests/PaymentServiceTests.cs`
- [x] T039 [P] [US3] Unit test: ProcessPayment — دفع فاشل يحافظ على Pending في `tests/CommerceTests/PaymentServiceTests.cs`
- [x] T040 [P] [US3] Unit test: ProcessPayment — 3 محاولات فاشلة تلغي الطلب (Cancelled) في `tests/CommerceTests/PaymentServiceTests.cs`
- [x] T041 [US3] Integration test: Commerce flow كامل (Cart → Order → Payment → Enrollment) في `tests/CommerceTests/CommerceFlowIntegrationTests.cs`

### Implementation for User Story 3

- [x] T042 [P] [US3] إنشاء `OrderDtos.cs` في `backend_project/DTOs/Order/OrderDtos.cs` (CreateOrderRequest, OrderResponseDto, OrderDetailDto, OrderItemDto)
- [x] T043 [P] [US3] إنشاء `PaymentDtos.cs` في `backend_project/DTOs/Payment/PaymentDtos.cs` (ProcessPaymentRequest, PaymentResponseDto, PaymentHistoryDto)
- [x] T044 [P] [US3] إنشاء `CreateOrderValidator.cs` في `backend_project/Validators/Order/CreateOrderValidator.cs`
- [x] T045 [P] [US3] إنشاء `ProcessPaymentValidator.cs` في `backend_project/Validators/Payment/ProcessPaymentValidator.cs`
- [x] T046 [P] [US3] إنشاء واجهة `IOrderService` في `backend_project/services/Interfaces/IOrderService.cs`
- [x] T047 [P] [US3] إنشاء واجهة `IPaymentService` في `backend_project/services/Interfaces/IPaymentService.cs`
- [x] T048 [US3] تنفيذ `OrderService` في `backend_project/services/Implementations/OrderService.cs` (CreateOrder, GetOrders, GetOrderDetails)
- [x] T049 [US3] تنفيذ `PaymentService` في `backend_project/services/Implementations/PaymentService.cs` (ProcessPayment, GetPaymentHistory, tracking retries count)
- [x] T050 [US3] إنشاء `OrderController` في `backend_project/Controllers/OrderController.cs` (POST /api/orders, GET /api/orders, GET /api/orders/{id})
- [x] T051 [US3] إنشاء Checkout endpoints في `OrderController` (POST /api/orders/{orderId}/payments, GET /api/orders/{orderId}/payments)
- [x] T052 [US3] ربط PaymentService مع EnrollmentService (تسجيل تلقائي بعد نجاح الدفع)
- [x] T053 [US3] ربط PaymentService مع MockPaymentGateway
- [x] T054 [US3] ربط PaymentService مع NotificationService (إشعار نجاح/فشل الدفع)
- [x] T055 [US3] ربط PaymentService مع ActivityLogService (تسجيل الأحداث)
- [x] T056 [P] [US3] تسجيل `IOrderService`, `IPaymentService` في Program.cs

**Checkpoint**: US1 + US2 + US3 كاملين — تدفق الشراء كامل شغال ✅

---

## Phase 5: User Story 4 - إدارة طرق الدفع (Priority: P2)

**Goal**: المسؤول يدير طرق الدفع المتاحة
**Independent Test**: Admin add payment method → verify it's listed → deactivate → verify hidden from students

### Tests for User Story 4 ⚠️

- [x] T057 [P] [US4] Unit test: CreatePaymentMethod — ينشئ طريقة دفع جديدة في `tests/CommerceTests/PaymentServiceTests.cs`
- [x] T058 [US4] Unit test: TogglePaymentMethod — تعطيل/تفعيل طريقة دفع في `tests/CommerceTests/PaymentServiceTests.cs`

### Implementation for User Story 4

- [x] T059 [P] [US4] إنشاء DTOs لـ PaymentMethod في `backend_project/DTOs/Payment/PaymentDtos.cs` (CreatePaymentMethodRequest, PaymentMethodResponse)
- [x] T060 [US4] إضافة دوال PaymentMethod لـ `IPaymentService` (CreatePaymentMethod, TogglePaymentMethod, GetActivePaymentMethods)
- [x] T061 [US4] تنفيذ دوال PaymentMethod في `PaymentService`
- [x] T062 [US4] إنشاء `AdminPaymentMethodController` في `backend_project/Controllers/AdminPaymentMethodController.cs` (GET/POST/PUT toggle)
- [x] T063 [US4] إضافة endpoint عام `GET /api/payment-methods` في `PaymentMethodsController`

**Checkpoint**: US1-4 كاملين — طرق الدفع شغالة ✅

---

## Phase 6: User Story 5 - طلب استرداد المبلغ (Priority: P2)

**Goal**: الطالب يطلب استرداد، المسؤول يوافق أو يرفض
**Independent Test**: Student request refund → Admin approve → verify enrollment refunded & order status updated

### Tests for User Story 5 ⚠️

- [x] T064 [P] [US5] Unit test: RequestRefund — ينشئ طلب استرداد بحالة Requested في `tests/CommerceTests/RefundServiceTests.cs`
- [x] T065 [P] [US5] Unit test: RequestRefund — يمنع استرداد لكورس مجاني في `tests/CommerceTests/RefundServiceTests.cs`
- [x] T066 [P] [US5] Unit test: ApproveRefund — يوافق ويحدث Order لـ Refunded في `tests/CommerceTests/RefundServiceTests.cs`
- [x] T067 [P] [US5] Unit test: ApproveRefund — يحدث Enrollment لـ Refunded في `tests/CommerceTests/RefundServiceTests.cs`
- [x] T068 [US5] Unit test: RejectRefund — يرفض ويحافظ على حالة الـ Order في `tests/CommerceTests/RefundServiceTests.cs`

### Implementation for User Story 5

- [x] T069 [P] [US5] إنشاء `RefundDtos.cs` في `backend_project/DTOs/Refund/RefundDtos.cs` (RequestRefundRequest, RefundResponseDto, ProcessRefundRequest)
- [x] T070 [P] [US5] إنشاء `RequestRefundValidator.cs` في `backend_project/Validators/Refund/RequestRefundValidator.cs`
- [x] T071 [P] [US5] إنشاء `ProcessRefundValidator.cs` في `backend_project/Validators/Refund/ProcessRefundValidator.cs`
- [x] T072 [P] [US5] إنشاء واجهة `IRefundService` في `backend_project/services/Interfaces/IRefundService.cs`
- [x] T073 [US5] تنفيذ `RefundService` في `backend_project/services/Implementations/RefundService.cs` (RequestRefund, ApproveRefund, RejectRefund, GetRefunds)
- [x] T074 [US5] إنشاء Student Refund endpoints في `backend_project/Controllers/RefundController.cs` (POST /api/refunds, GET /api/refunds)
- [x] T075 [US5] إنشاء `AdminRefundController` في `backend_project/Controllers/AdminRefundController.cs` (GET list, POST approve, POST reject)
- [x] T076 [US5] ربط RefundService مع EnrollmentService (تحديث حالة Enrollment)
- [x] T077 [US5] ربط RefundService مع NotificationService (إشعار الموافقة/الرفض)
- [x] T078 [P] [US5] تسجيل `IRefundService` و `RefundService` في Program.cs

**Checkpoint**: US1-5 كاملين — الاسترداد شغال ✅

---

## Phase 7: User Story 6 - قائمة الأمنيات (Priority: P3)

**Goal**: الطالب يضيف/يشيل/يشوف كورسات في قائمة الأمنيات
**Independent Test**: Add course to wishlist → verify listed → add duplicate → error → remove → verify gone

### Tests for User Story 6 ⚠️

- [x] T079 [P] [US6] Unit test: AddToWishlist — يضيف كورس جديد في `tests/CommerceTests/WishlistServiceTests.cs`
- [x] T080 [P] [US6] Unit test: AddToWishlist — يمنع تكرار الكورس في `tests/CommerceTests/WishlistServiceTests.cs`
- [x] T081 [US6] Unit test: RemoveFromWishlist — يحذف كورس من القائمة في `tests/CommerceTests/WishlistServiceTests.cs`
- [x] T082 [US6] Unit test: GetWishlist — يرجع قائمة الأمنيات في `tests/CommerceTests/WishlistServiceTests.cs`

### Implementation for User Story 6

- [x] T083 [P] [US6] إنشاء `WishlistDtos.cs` في `backend_project/DTOs/Wishlist/WishlistDtos.cs` (WishlistResponseDto, WishlistItemDto)
- [x] T084 [P] [US6] إنشاء واجهة `IWishlistService` في `backend_project/services/Interfaces/IWishlistService.cs`
- [x] T085 [US6] تنفيذ `WishlistService` في `backend_project/services/Implementations/WishlistService.cs` (Add, Remove, GetWishlist)
- [x] T086 [US6] إنشاء `WishlistController` في `backend_project/Controllers/WishlistController.cs` (GET /api/wishlist, POST /api/wishlist/{courseId}, DELETE /api/wishlist/{courseId})
- [x] T087 [P] [US6] تسجيل `IWishlistService` و `WishlistService` في Program.cs

**Checkpoint**: US1-6 كاملين — النظام التجاري كامل شغال ✅

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: تنظيف، تحسينات، وضمان الجودة

- [x] T088 [P] مراجعة كل الـ Validators تغطي كل الحالات المطلوبة
- [x] T089 [P] مراجعة كل الـ ApiResponses تستخدم النمط الموحد (success, message, data, errors)
- [x] T090 [P] التأكد من تسجيل كل الأحداث في ActivityLog (إنشاء طلب، دفع، استرداد)
- [x] T091 [P] التأكد من إرسال الإشعارات (نجاح دفع، فشل، استرداد)
- [x] T092 [P] مراجعة حالات الـ Edge: سلة فاضية، كوبون منتهي، محاولات دفع فاشلة
- [x] T093 [P] التأكد من صحة الـ Transaction atomicity في تدفق الدفع
- [x] T094 [P] تشغيل كل الاختبارات (`dotnet test`) والتأكد من 100% نجاح
- [x] T095 [P] مراجعة كل الـ [P] tasks أنها ما عندها dependencies على بعض

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: لا تبعيات — يبدأ فوراً
- **US1 Cart (Phase 2)**: يعتمد على Phase 1 — ما يعتمدش على أي US تاني
- **US2 Coupon (Phase 3)**: يعتمد على Phase 1 — ما يعتمدش على US1 لكن فيه تكامل مع Cart (ربط الكوبون بالسلة)
- **US3 Order/Payment (Phase 4)**: يعتمد على Phase 1 — يحتاج Cart (US1) لاكتمال تدفق الشراء
- **US4 PaymentMethod (Phase 5)**: يعتمد على Phase 1 — ما يعتمدش على US3
- **US5 Refund (Phase 6)**: يعتمد على US3 (الأوردر لازم يكون Completed)
- **US6 Wishlist (Phase 7)**: يعتمد على Phase 1 فقط — مستقل تماماً

### User Story Dependencies

```
US1 (Cart) ────┐
                ├──► US3 (Order/Payment) ──► US5 (Refund)
US2 (Coupon) ──┘                
US4 (PaymentMethod) ──► مستقل
US6 (Wishlist) ───────► مستقل
```

### Parallel Opportunities

- Phase 1 T001-T003 (مجلدات) — كلها [P]
- US1 T008-T011 (اختبارات السلة) — كلها [P]
- US2 T019-T025 (اختبارات الكوبونات) — كلها [P]
- US3 T035-T041 (اختبارات الطلب والدفع) — كلها [P]
- US5 T064-T068 (اختبارات الاسترداد) — كلها [P]
- US6 T079-T082 (اختبارات الأمنيات) — كلها [P]
- Polish T088-T095 — كلها [P]

### Implementation Strategy

#### MVP First (US1 فقط)

1. Phase 1: Setup
2. Phase 2: US1 (Cart) — **MVP! السلة شغالة**
3. Validate/test US1 independently
4. Deploy/demo إذا جاهز

#### Incremental Delivery

1. Phase 1 → Foundation ready
2. Phase 2 → US1 Cart → **MVP!**
3. Phase 3 → US2 Coupons → Test → Deploy
4. Phase 4 → US3 Order/Payment → Test → Deploy
5. Phase 5 → US4 PaymentMethods → Test → Deploy
6. Phase 6 → US5 Refund → Test → Deploy
7. Phase 7 → US6 Wishlist → Test → Deploy
8. Phase 8 → Polish

---

## Summary

| الـ User Story | الأولوية | المهام | الاختبارات | الملفات الجديدة |
|---------------|---------|--------|-----------|----------------|
| US1: Cart | P1 | T008-T018 (11) | 5 | 5 ملفات |
| US2: Coupon | P1 | T019-T034 (16) | 7 | 8 ملفات |
| US3: Order/Payment | P1 | T035-T056 (22) | 7 | 10 ملفات |
| US4: PaymentMethod | P2 | T057-T063 (7) | 2 | 3 ملفات |
| US5: Refund | P2 | T064-T078 (15) | 5 | 7 ملفات |
| US6: Wishlist | P3 | T079-T087 (9) | 4 | 4 ملفات |
| **المجموع** | **3 P1 + 2 P2 + 1 P3** | **95 مهمة** | **30 اختبار** | **~37 ملف** |
