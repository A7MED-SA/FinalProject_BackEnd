# Courses (Public) — API Reference

**Controllers**: `PublicCourseController`, `CategoryController`

---

## Published Courses

### Get List
**GET** `/api/public/courses?categoryId=guid&level=Beginner&language=ar&minPrice=0&maxPrice=500&sortBy=popularity&search=keyword&page=1&pageSize=20`
**Auth**: None

**Response**:
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "title": "دورة تعلم البرمجة",
        "slug": "learn-programming",
        "shortDescription": "...",
        "thumbnailUrl": "https://...",
        "price": 199.99,
        "originalPrice": 499.99,
        "instructorName": "أحمد محمد",
        "rating": 4.5,
        "studentsCount": 1234,
        "durationHours": 40,
        "level": "Beginner",
        "category": "Programming"
      }
    ],
    "page": 1,
    "pageSize": 20,
    "totalCount": 50
  }
}
```

### Get Course Details
**GET** `/api/public/courses/{id}`

### Get Course By Slug
**GET** `/api/public/courses/slug/{slug}`

**Response** (تفصيلي):
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "title": "...",
    "slug": "...",
    "description": "نص كامل",
    "requirements": [{ "id": "guid", "text": "..." }],
    "outcomes": [{ "id": "guid", "text": "..." }],
    "sections": [
      {
        "id": "guid",
        "title": "...",
        "order": 1,
        "items": [
          { "id": "guid", "title": "...", "type": "Video", "durationMinutes": 15, "order": 1 }
        ]
      }
    ],
    "instructor": { "id": "guid", "name": "...", "headline": "...", "avatarUrl": "..." },
    "rating": 4.5,
    "reviewsCount": 120,
    "studentsCount": 1234,
    "price": 199.99,
    "originalPrice": 499.99,
    "level": "Beginner",
    "language": "Arabic",
    "category": "Programming",
    "totalDuration": 2400,
    "lastUpdated": "..."
  }
}
```

### Search Suggestions
**GET** `/api/public/courses/search/suggest?query=pro&limit=5`

### Platform Stats
**GET** `/api/public/courses/stats`
```json
{
  "totalCourses": 150,
  "totalStudents": 50000,
  "totalInstructors": 200,
  "totalReviews": 10000
}
```

### Related Courses
**GET** `/api/public/courses/{id}/related?limit=4`

### Filter Options
**GET** `/api/public/courses/filters/options`
```json
{
  "categories": [{ "id": "guid", "name": "Programming", "count": 50 }],
  "levels": ["Beginner", "Intermediate", "Advanced"],
  "languages": ["Arabic", "English"],
  "priceRange": { "min": 0, "max": 1000 }
}
```

---

## Categories

### Get All Categories
**GET** `/api/categories`
```json
{ "success": true, "data": [{ "id": "guid", "name": "Programming", "slug": "programming", "parentId": null, "imageUrl": "..." }] }
```

### Get Category By Id
**GET** `/api/categories/{id}`

### Create Category
**POST** `/api/categories` — Admin only

### Update Category
**PUT** `/api/categories/{id}` — Admin only

### Delete Category
**DELETE** `/api/categories/{id}` — Admin only

### Set Category Image
**PUT** `/api/categories/{categoryId}/image` — Admin only
```json
{ "fileId": "guid" }
```
