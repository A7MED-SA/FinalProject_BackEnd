# Commerce (Cart, Wishlist, Orders, Payments, Coupons, Refunds) — API Reference

**Controllers**: `CartController`, `WishlistController`, `OrderController`, `CouponController`, `RefundController`, `PaymentMethodsController`

---

## Cart

**Base**: `/api/cart`
**Auth**: JWT

### Get Cart
**GET** `/api/cart`
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "courseId": "guid",
        "courseTitle": "دورة تعلم البرمجة",
        "price": 199.99,
        "thumbnailUrl": "https://..."
      }
    ],
    "subtotal": 199.99,
    "discount": 0,
    "total": 199.99,
    "appliedCoupon": null
  }
}
```

### Add Item
**POST** `/api/cart/items`
```json
{ "courseId": "guid" }
```

### Remove Item
**DELETE** `/api/cart/items/{itemId}`

### Apply Coupon
**POST** `/api/cart/coupon`
```json
{ "code": "SAVE20" }
```

### Remove Coupon
**DELETE** `/api/cart/coupon`

---

## Wishlist

**Base**: `/api/wishlist`
**Auth**: JWT

### Get Wishlist
**GET** `/api/wishlist`

### Add Item
**POST** `/api/wishlist/{courseId}`

### Remove Item
**DELETE** `/api/wishlist/{courseId}`

---

## Orders

**Base**: `/api/orders`
**Auth**: JWT

### Create Order
**POST** `/api/orders`
```json
{
  "cartItems": [{ "cartItemId": "guid" }]
}
```

### Get Orders
**GET** `/api/orders`

### Get Order By Id
**GET** `/api/orders/{id}`
```json
{
  "id": "guid",
  "status": "Pending",
  "items": [{ "courseId": "guid", "courseTitle": "...", "price": 199.99 }],
  "subtotal": 199.99,
  "discount": 0,
  "total": 199.99,
  "paymentStatus": "Pending",
  "createdAt": "...",
  "paidAt": null
}
```
Status: `Pending`, `Completed`, `Cancelled`

### Process Payment
**POST** `/api/orders/{orderId}/payments`
```json
{ "paymentMethodId": "guid" }
```
**Response**: رابط الدفع من CHIPS/PayTabs للتحويل للبوابة.

### Get Payment History
**GET** `/api/orders/{orderId}/payments`

---

## Coupon Validation (Public)

### Validate
**POST** `/api/coupons/validate`
**Auth**: JWT
```json
{
  "code": "SAVE20",
  "cartTotal": 499.99,
  "courseIds": ["guid", "guid"]
}
```
```json
{
  "isValid": true,
  "discount": 100.00,
  "totalAfterDiscount": 399.99,
  "message": "تم تطبيق الخصم"
}
```

---

## Payment Methods

### Get Active Methods
**GET** `/api/payment-methods`
**Auth**: None
```json
{
  "success": true,
  "data": [
    { "id": "guid", "name": "Visa / Mastercard", "provider": "CHIPS", "isActive": true }
  ]
}
```

---

## Refunds

**Base**: `/api/refunds`
**Auth**: JWT

### Request Refund
**POST** `/api/refunds`
```json
{
  "orderItemId": "guid",
  "reason": "الدورة لا تناسب مستواي"
}
```

### My Refunds
**GET** `/api/refunds`
```json
[
  {
    "id": "guid",
    "orderItemId": "guid",
    "courseTitle": "...",
    "amount": 199.99,
    "status": "Requested",
    "reason": "...",
    "createdAt": "...",
    "processedAt": null
  }
]
```
Status: `Requested`, `Approved`, `Rejected`
