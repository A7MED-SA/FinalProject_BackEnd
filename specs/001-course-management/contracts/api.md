# API Contracts: Course Management System

## Category Endpoints (Public & Admin)

`GET /api/categories`
- **Response**: `200 OK` `List<CategoryResponseDto>` (Tree structure)

`POST /api/categories` *(Admin Only)*
- **Body**: `CreateCategoryDto`
- **Response**: `201 Created`

## Course Endpoints (Public)

`GET /api/courses`
- **Query Params**: `CategoryId`, `SearchTerm`, `Page`, `PageSize`
- **Response**: `200 OK` `PagedList<CourseSummaryDto>`

`GET /api/courses/{id}`
- **Response**: `200 OK` `CourseDetailsDto`

## Course Management Endpoints (Instructor)

`POST /api/management/courses`
- **Body**: `CreateCourseDto`
- **Response**: `201 Created`

`PUT /api/management/courses/{id}`
- **Body**: `UpdateCourseDto`
- **Response**: `204 No Content`

`POST /api/management/courses/{id}/submit-for-review`
- **Response**: `204 No Content`

`POST /api/management/courses/{id}/sections`
- **Body**: `CreateSectionDto`
- **Response**: `201 Created`

`PUT /api/management/courses/{id}/sections/reorder`
- **Body**: `List<ReorderRequestDto>`
- **Response**: `204 No Content`

`POST /api/management/sections/{sectionId}/items`
- **Body**: `CreateSectionItemDto`
- **Response**: `201 Created`

## Admin Course Approval Endpoints (Admin)

`GET /api/admin/courses/pending`
- **Response**: `200 OK` `List<CourseSummaryDto>`

`POST /api/admin/courses/{id}/approve`
- **Response**: `204 No Content`

`POST /api/admin/courses/{id}/reject`
- **Body**: `{ "Reason": "string" }`
- **Response**: `204 No Content`
