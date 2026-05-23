# API Contracts: Commerce System

**Date**: 2026-05-23  
**Base URL**: `/api`  
**Auth**: JWT Bearer (unless marked `[AllowAnonymous]`)

## Response Envelope

All responses use the existing `ApiResponse<T>` wrapper:
```json
{
  "success": true,
  "message": "...",
  "data": { ... },
  "errors": []
}
```

---

## 1. Cart Controller

**Path prefix**: `/api/cart`  
**Auth**: `[Authorize]`

### GET `/api/cart`
Get current user's cart with items.

**Response** (200):
```json
{
  "id": "guid",
  "items": [
    {
      "id": "guid",
      "courseId": "guid",
      "courseTitle": "string",
      "courseImageUrl": "string?",
      "instructorName": "string",
      "priceSnapshot": 499.00,
      "currentPrice": 499.00,
      "addedAt": "datetime"
    }
  ],
  "subtotal": 499.00,
  "couponCode": "string?",
  "discountAmount": 0,
  "finalAmount": 499.00
}
```

**Error Cases**: 401 (unauthorized)

### POST `/api/cart/items`
Add course to cart.

**Request**:
```json
{
  "courseId": "guid"
}
```

**Response** (201):
```json
{
  "id": "guid",
  "courseId": "guid",
  "courseTitle": "string",
  "priceSnapshot": 499.00,
  "addedAt": "datetime"
}
```

**Error Cases**: 400 (already in cart, already enrolled, course not published), 404 (course not found)

### DELETE `/api/cart/items/{itemId}`
Remove item from cart.

**Response** (204): No Content

**Error Cases**: 404 (item not found in user's cart)

### POST `/api/cart/coupon`
Apply coupon code.

**Request**:
```json
{
  "code": "string"
}
```

**Response** (200):
```json
{
  "code": "string",
  "discountAmount": 50.00,
  "finalAmount": 449.00
}
```

**Error Cases**: 400 (invalid code, expired, minimum not met, exhausted)

### DELETE `/api/cart/coupon`
Remove applied coupon from cart.

**Response** (204): No Content

---

## 2. Order Controller

**Path prefix**: `/api/orders`  
**Auth**: `[Authorize]`

### POST `/api/orders`
Create order from current user's cart.

**Request**:
```json
{
  "billingAddressId": "guid?"
}
```

**Response** (201):
```json
{
  "id": "guid",
  "orderNumber": "ORD-20260523-000001",
  "subtotalAmount": 998.00,
  "discountAmount": 50.00,
  "taxAmount": 0,
  "finalAmount": 948.00,
  "currency": "EGP",
  "status": "Pending",
  "items": [
    {
      "courseId": "guid",
      "courseTitle": "string",
      "priceAtPurchase": 499.00
    }
  ],
  "createdAt": "datetime"
}
```

**Error Cases**: 400 (cart empty), 404 (cart not found)

### GET `/api/orders`
Get current user's orders.

**Query**: `?status=Pending&page=1&pageSize=10`

**Response** (200):
```json
{
  "items": [
    {
      "id": "guid",
      "orderNumber": "string",
      "finalAmount": 948.00,
      "currency": "EGP",
      "status": "Completed",
      "courseCount": 2,
      "createdAt": "datetime"
    }
  ],
  "totalCount": 5,
  "page": 1,
  "pageSize": 10
}
```

### GET `/api/orders/{orderId}`
Get order details.

**Response** (200):
```json
{
  "id": "guid",
  "orderNumber": "string",
  "status": "Completed",
  "subtotalAmount": 998.00,
  "discountAmount": 50.00,
  "taxAmount": 0,
  "finalAmount": 948.00,
  "currency": "EGP",
  "items": [
    {
      "courseId": "guid",
      "courseTitle": "string",
      "priceAtPurchase": 499.00
    }
  ],
  "payments": [
    {
      "id": "guid",
      "amount": 948.00,
      "status": "Succeeded",
      "paidAt": "datetime",
      "paymentMethodName": "Credit Card"
    }
  ],
  "coupon": {
    "code": "SAVE10",
    "discountAmount": 50.00
  },
  "createdAt": "datetime",
  "updatedAt": "datetime?"
}
```

---

## 3. Checkout (Order Payment)

**Path prefix**: `/api/orders/{orderId}/payments`  
**Auth**: `[Authorize]`

### POST `/api/orders/{orderId}/payments`
Process payment for an order.

**Request**:
```json
{
  "paymentMethodId": "guid",
  "amount": 948.00
}
```

**Response** (200):
```json
{
  "paymentId": "guid",
  "status": "Succeeded",
  "transactionRef": "TXN-20260523-abc123",
  "paidAt": "datetime",
  "message": "Payment successful. You are now enrolled in 2 courses."
}
```

**Error Cases**: 
- 400 (order not Pending, amount mismatch, max retries exceeded)
- 402 (payment failed — Payment Required)

### GET `/api/orders/{orderId}/payments`
Get payment history for an order.

**Response** (200):
```json
{
  "payments": [
    {
      "id": "guid",
      "amount": 948.00,
      "status": "Failed",
      "createdAt": "datetime"
    },
    {
      "id": "guid",
      "amount": 948.00,
      "status": "Succeeded",
      "paidAt": "datetime"
    }
  ]
}
```

---

## 4. Coupon Controller (Student)

**Path prefix**: `/api/coupons`  
**Auth**: `[Authorize]`

### POST `/api/coupons/validate`
Validate a coupon code without applying it.

**Request**:
```json
{
  "code": "string",
  "cartTotal": 499.00,
  "courseIds": ["guid"]
}
```

**Response** (200):
```json
{
  "code": "string",
  "isValid": true,
  "discountAmount": 50.00,
  "finalAmount": 449.00,
  "message": "Coupon applied successfully"
}
```

**Error Cases**: 400 (invalid, expired, exhausted, minimum not met)

---

## 5. Admin Coupon Controller

**Path prefix**: `/api/admin/coupons`  
**Auth**: `[Authorize(Roles = "Admin")]`

### GET `/api/admin/coupons`
List all coupons.

**Query**: `?isActive=true&page=1&pageSize=20`

**Response** (200):
```json
{
  "items": [
    {
      "id": "guid",
      "code": "SAVE10",
      "type": "Percentage",
      "value": 10.00,
      "usageLimit": 100,
      "timesUsed": 45,
      "isActive": true,
      "validFrom": "datetime",
      "validUntil": "datetime",
      "createdAt": "datetime"
    }
  ],
  "totalCount": 3,
  "page": 1,
  "pageSize": 20
}
```

### POST `/api/admin/coupons`
Create coupon.

**Request**:
```json
{
  "code": "SAVE10",
  "type": "Percentage",
  "value": 10.00,
  "maxDiscountAmount": 100.00,
  "minimumPurchaseAmount": 200.00,
  "applicableTo": "All",
  "courseIds": [],
  "usageLimit": 100,
  "userLimitPerUser": 1,
  "isPublic": true,
  "validFrom": "datetime",
  "validUntil": "datetime"
}
```

**Response** (201): Coupon object

### PUT `/api/admin/coupons/{couponId}`
Update coupon (toggle active, adjust limits, etc.).

**Response** (200): Updated coupon

### DELETE `/api/admin/coupons/{couponId}`
Deactivate/delete coupon.

**Response** (204): No Content

---

## 6. Admin Refund Controller

**Path prefix**: `/api/admin/refunds`  
**Auth**: `[Authorize(Roles = "Admin")]`

### GET `/api/admin/refunds`
List all refund requests.

**Query**: `?status=Requested&page=1&pageSize=20`

**Response** (200): Paginated list

### GET `/api/admin/refunds/{refundId}`
Get refund details.

### POST `/api/admin/refunds/{refundId}/approve`
Approve refund request.

**Request**:
```json
{
  "adminNotes": "string?"
}
```

**Response** (200): Refund with status Approved

**Side Effects**: Order status → Refunded, Enrollments → Refunded, EnrollmentCount decremented

### POST `/api/admin/refunds/{refundId}/reject`
Reject refund request.

**Request**:
```json
{
  "reason": "string"
}
```

**Response** (200): Refund with status Rejected

---

## 7. Student Refund

**Path prefix**: `/api/refunds`  
**Auth**: `[Authorize]`

### POST `/api/refunds`
Request a refund.

**Request**:
```json
{
  "orderItemId": "guid",
  "reason": "string?"
}
```

**Response** (201): Refund with status Requested

**Error Cases**: 400 (course is free, already refunded, order not completed)

### GET `/api/refunds`
Get user's refund requests.

---

## 8. Payment Method Controller (Admin)

**Path prefix**: `/api/admin/payment-methods`  
**Auth**: `[Authorize(Roles = "Admin")]`

### GET `/api/admin/payment-methods`
List all payment methods.

### POST `/api/admin/payment-methods`
Create payment method.

### PUT `/api/admin/payment-methods/{id}/toggle`
Toggle active/inactive.

### GET `/api/payment-methods`
Public — list active payment methods. `[AllowAnonymous]`

---

## 9. Wishlist Controller

**Path prefix**: `/api/wishlist`  
**Auth**: `[Authorize]`

### GET `/api/wishlist`
Get user's wishlist.

**Response** (200):
```json
{
  "items": [
    {
      "courseId": "guid",
      "courseTitle": "string",
      "courseImageUrl": "string?",
      "instructorName": "string",
      "price": 499.00,
      "addedAt": "datetime"
    }
  ]
}
```

### POST `/api/wishlist/{courseId}`
Add course to wishlist.

**Response** (201)

**Error Cases**: 400 (already in wishlist)

### DELETE `/api/wishlist/{courseId}`
Remove from wishlist.

**Response** (204)
