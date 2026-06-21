# Athary API Documentation (Frontend Reference)

## 1. Base URL & Authentication

### Base URL
- **Development**: `https://localhost:5001` (or the port configured in appsettings)
- **Production**: Configured via `CorsSettings:AllowedOrigins` in appsettings.json

### Authentication

#### JWT Bearer Token
All protected endpoints require `Authorization: Bearer <accessToken>` header.

The JWT contains these claims:
- `nameid` / `uid` — User GUID
- `sid` — Session GUID
- `role` — User role(s) (Admin, Instructor, Student)
- `permission` — Permission claims (optional, for `[HasPermission]`)

#### Token Flow
1. `POST /api/auth/login` → returns `accessToken`, `refreshToken`, `sessionId`, `expiresAt`, `user`
2. `POST /api/auth/register` → returns `userId`, `email`, `message` (email verification required)
3. Use `accessToken` for all subsequent API calls
4. When token expires, call `POST /api/auth/refresh` with `{ refreshToken }`
5. `POST /api/auth/logout` — revokes the session
6. `POST /api/auth/logout-all` — revokes all sessions except current

### Available Roles
| Role | Description |
|------|------------|
| `Admin` | Full system access |
| `Instructor` | Course creation & management, live sessions, announcements |
| `Student` | Enrollment, learning, reviews |

### Authorization Policies (defined in Program.cs)
| Policy | Behavior |
|--------|----------|
| `CanSubmitTeacherRequest` | Authenticated user who is NOT an Instructor |
| `CanViewOwnTeacherRequests` | Authenticated user |
| `CanProcessTeacherRequests` | Admin only |

### Error Response Format
Every endpoint returns `ApiResponse<T>`:
```json
{
  "success": true|false,
  "data": { ... } | null,
  "message": "string | null",
  "errors": ["string", ...] | null
}
```

### Standard HTTP Status Codes
| Status | When |
|--------|------|
| 200 | Success |
| 201 | Created (with Location header) |
| 204 | No Content (deletion) |
| 400 | Bad Request (validation / InvalidOperationException) |
| 401 | Unauthorized |
| 403 | Forbidden (UnauthorizedAccessException) |
| 404 | Not Found (KeyNotFoundException) |
| 429 | Rate Limited |

---

## 2. All API Endpoints

### 2.1 Authentication (`/api/auth`) — Rate Limited: `Auth` policy (10 req/min)

#### `POST /api/auth/register`
- **Auth**: None
- **Rate Limit**: Auth (10/min)
- **Body** (`RegisterDto`):
  - `firstName` (string, required)
  - `lastName` (string, required)
  - `email` (string, required)
  - `password` (string, required, min 8, digit+lower+upper+non-alphanumeric)
  - `confirmPassword` (string, required)
  - `gender` (Gender enum? optional)
  - `dateOfBirth` (DateOnly? optional)
  - `phoneNumber` (string? optional)
  - `country` (string? optional)
  - `city` (string? optional)
  - `streetLine1` (string? optional)
  - `postalCode` (string? optional)
- **Response** `200`: `ApiResponse<RegisterResponseDto>`
  ```json
  { "success": true, "data": { "userId": "guid", "email": "string", "message": "string" } }
  ```

#### `POST /api/auth/login`
- **Auth**: None
- **Rate Limit**: Auth (10/min)
- **Body** (`LoginDto`):
  - `email` (string)
  - `password` (string)
  - `rememberMe` (boolean, default false)
- **Response** `200`: `ApiResponse<AuthResponseDto>`
  ```json
  { "success": true, "data": { "accessToken": "string", "refreshToken": "string", "sessionId": "guid", "expiresAt": "datetime", "user": { ... UserInfoDto } } }
  ```

#### `POST /api/auth/refresh`
- **Auth**: None
- **Rate Limit**: Auth (10/min)
- **Body** (`RefreshTokenDto`):
  - `refreshToken` (string)
- **Response** `200`: `ApiResponse<AuthResponseDto>`

#### `POST /api/auth/verify-email`
- **Auth**: None
- **Rate Limit**: Auth (10/min)
- **Body** (`VerifyEmailDto`):
  - `token` (string)
  - `email` (string)
- **Response** `200`: `ApiResponse<null>`

#### `POST /api/auth/resend-verification`
- **Auth**: None
- **Rate Limit**: Auth (10/min)
- **Body** (`ResendVerificationDto`):
  - `email` (string)
- **Response** `200`: `ApiResponse<null>`

#### `POST /api/auth/forgot-password`
- **Auth**: None
- **Rate Limit**: Auth (10/min)
- **Body** (`ForgotPasswordDto`):
  - `email` (string)
- **Response** `200`: `ApiResponse<null>` (always returns success to prevent enumeration)

#### `POST /api/auth/reset-password`
- **Auth**: None
- **Rate Limit**: Auth (10/min)
- **Body** (`ResetPasswordDto`):
  - `token` (string)
  - `email` (string)
  - `newPassword` (string)
  - `confirmPassword` (string)
- **Response** `200`: `ApiResponse<null>`

#### `POST /api/auth/change-password`
- **Auth**: Bearer token
- **Rate Limit**: Auth (10/min)
- **Body** (`ChangePasswordDto`):
  - `currentPassword` (string)
  - `newPassword` (string)
- **Response** `200`: `ApiResponse<null>`

#### `POST /api/auth/logout`
- **Auth**: Bearer token
- **Body**: None
- **Response** `200`: `ApiResponse<null>`

#### `POST /api/auth/logout-all`
- **Auth**: Bearer token
- **Body**: None
- **Response** `200`: `ApiResponse<null>`

#### `GET /api/auth/sessions`
- **Auth**: Bearer token
- **Response** `200`: `ApiResponse<List<SessionDto>>`

---

### 2.2 OAuth (`/api/oauth`)

#### `POST /api/oauth/google`
- **Auth**: None
- **Body** (`OAuthLoginDto`):
  - `idToken` (string)
  - `provider` (must be `"google"`)
- **Response** `200`: `ApiResponse<AuthResponseDto>`

#### `POST /api/oauth/microsoft`
- **Auth**: None
- **Body** (`OAuthLoginDto`):
  - `idToken` (string)
  - `provider` (must be `"microsoft"`)
- **Response** `200`: `ApiResponse<AuthResponseDto>`

---

### 2.3 Profile (`/api/profile`)

#### `GET /api/profile/me`
- **Auth**: Bearer token
- **Response** `200`: `ApiResponse<ProfileDto>`

#### `GET /api/profile/{userId:guid}`
- **Auth**: AllowAnonymous
- **Response** `200`: `ApiResponse<PublicProfileDto>`

#### `PUT /api/profile`
- **Auth**: Bearer token
- **Body** (`UpdateProfileDto`):
  - `firstName` (string?)
  - `lastName` (string?)
  - `bio` (string?)
  - `gender` (Gender?)
  - `dateOfBirth` (DateOnly?)
  - `nationality` (string?)
- **Response** `200`: `ApiResponse<ProfileDto>`

#### `POST /api/profile/picture`
- **Auth**: Bearer token
- **Body** (`SetProfileImageDto`):
  - `fileId` (Guid)
- **Response** `200`: `ApiResponse<ProfileDto>`

#### `DELETE /api/profile/picture`
- **Auth**: Bearer token
- **Response** `200`: `ApiResponse<null>`

#### `POST /api/profile/phones`
- **Auth**: Bearer token
- **Body** (`AddPhoneDto`):
  - `phoneNumber` (string)
  - `type` (PhoneType: Primary, Secondary)
  - `isDefault` (boolean)
- **Response** `201`: `ApiResponse<PhoneDto>`

#### `DELETE /api/profile/phones/{phoneId:guid}`
- **Auth**: Bearer token
- **Response** `200`: `ApiResponse<null>`

#### `PUT /api/profile/phones/{phoneId:guid}/default`
- **Auth**: Bearer token
- **Response** `200`: `ApiResponse<null>`

#### `POST /api/profile/addresses`
- **Auth**: Bearer token
- **Body** (`AddAddressDto`):
  - `type` (string, default "Home")
  - `streetLine1` (string)
  - `streetLine2` (string?)
  - `city` (string)
  - `stateProvince` (string?)
  - `postalCode` (string)
  - `country` (string)
  - `contactPhone` (string?)
  - `isDefault` (boolean)
- **Response** `201`: `ApiResponse<AddressDto>`

#### `PUT /api/profile/addresses/{addressId:guid}`
- **Auth**: Bearer token
- **Body** (`UpdateAddressDto`) — all fields optional
  - `type` (string?)
  - `streetLine1` (string?)
  - `streetLine2` (string?)
  - `city` (string?)
  - `stateProvince` (string?)
  - `postalCode` (string?)
  - `country` (string?)
  - `contactPhone` (string?)
  - `isDefault` (boolean?)
- **Response** `200`: `ApiResponse<AddressDto>`

#### `DELETE /api/profile/addresses/{addressId:guid}`
- **Auth**: Bearer token
- **Response** `200`: `ApiResponse<null>`

#### `PUT /api/profile/addresses/{addressId:guid}/default`
- **Auth**: Bearer token
- **Response** `200`: `ApiResponse<null>`

---

### 2.4 Categories (`/api/categories`)

#### `GET /api/categories`
- **Auth**: AllowAnonymous
- **Response** `200`: `ApiResponse<List<CategoryResponseDto>>`

#### `GET /api/categories/{id}`
- **Auth**: AllowAnonymous
- **Response** `200`: `ApiResponse<CategoryResponseDto>`

#### `POST /api/categories`
- **Auth**: Admin
- **Body** (`CreateCategoryDto`):
  - `name` (string)
  - `description` (string?)
  - `slug` (string?)
  - `parentId` (Guid?)
  - `position` (int)
- **Response** `201`: `ApiResponse<CategoryResponseDto>`

#### `PUT /api/categories/{id}`
- **Auth**: Admin
- **Body** (`UpdateCategoryDto`):
  - `name` (string)
  - `description` (string?)
  - `slug` (string?)
  - `parentId` (Guid?)
  - `position` (int)
- **Response** `200`: `ApiResponse<CategoryResponseDto>`

#### `DELETE /api/categories/{id}`
- **Auth**: Admin
- **Response** `204`: No Content

#### `PUT /api/categories/{categoryId}/image`
- **Auth**: Admin
- **Body** (`SetCategoryImageRequest`):
  - `fileId` (Guid)
- **Response** `200`: `ApiResponse<CategoryResponseDto>`

---

### 2.5 Media (`/api/media`)

#### `POST /api/media/upload-url`
- **Auth**: Bearer token
- **Body** (`UploadUrlRequestDto`):
  - `fileType` (StoredFileType: Image, Video, Document, Recording, Certificate)
  - `fileName` (string)
  - `contentType` (string)
  - `fileSizeBytes` (long)
  - `visibility` (FileVisibility: Public, EnrolledOnly, Private)
  - `relatedEntityId` (Guid?)
  - `relatedEntityType` (string?)
- **Response** `200`: `ApiResponse<UploadUrlResponseDto>`
  - `fileId` (Guid)
  - `uploadUrl` (string) — presigned S3/MinIO URL
  - `objectKey` (string)
  - `bucket` (string)
  - `expiresAt` (DateTime)
  - `requiredHeaders` (Dictionary<string,string>)

#### `POST /api/media/confirm-upload`
- **Auth**: Bearer token
- **Body** (`ConfirmUploadDto`):
  - `fileId` (Guid)
  - `objectKey` (string)
  - `bucket` (string)
- **Response** `200`: `ApiResponse<MediaFileDto>`

#### `GET /api/media/{fileId:guid}/view-url`
- **Auth**: AllowAnonymous (but checks user roles for restricted files)
- **Query**: `?userId=guid` (optional)
- **Response** `200`: `ApiResponse<ViewUrlResponseDto>`
  - `fileId` (Guid)
  - `viewUrl` (string)
  - `expiresAt` (DateTime)
  - `contentType` (string)
  - `sizeBytes` (long)

---

### 2.6 Admin Media (`/api/admin/media`) — Admin only

#### `GET /api/admin/media`
- **Auth**: Admin
- **Query** (`MediaFilterDto`):
  - `fileType` (StoredFileType?)
  - `bucket` (string?)
  - `visibility` (FileVisibility?)
  - `uploadedBy` (Guid?)
  - `fromDate` (DateTime?)
  - `toDate` (DateTime?)
  - `status` (FileStatus?)
  - `includeDeleted` (boolean, default false)
  - `page` (int, default 1)
  - `pageSize` (int, default 20)
  - `searchTerm` (string?)
- **Response** `200`: `ApiResponse<PagedResult<MediaFileDto>>`

#### `GET /api/admin/media/{fileId:guid}`
- **Auth**: Admin
- **Response** `200`: `ApiResponse<MediaDetailDto>`

#### `DELETE /api/admin/media/{fileId:guid}/soft`
- **Auth**: Admin
- **Response** `200`: `ApiResponse<null>` ("File soft deleted")

#### `POST /api/admin/media/{fileId:guid}/restore`
- **Auth**: Admin
- **Response** `200`: `ApiResponse<null>` ("File restored")

#### `DELETE /api/admin/media/{fileId:guid}`
- **Auth**: Admin
- **Response** `200`: `ApiResponse<null>` ("File permanently deleted")

#### `GET /api/admin/media/stats`
- **Auth**: Admin
- **Response** `200`: `ApiResponse<StorageStatsDto>`

---

### 2.7 Reviews (`/api/reviews`)

#### `POST /api/reviews`
- **Auth**: Bearer token
- **Body** (`CreateReviewRequest`):
  - `courseId` (Guid)
  - `rating` (int, 1-5)
  - `comment` (string?)
- **Response** `200`: `ApiResponse<ReviewResponse>`

#### `GET /api/reviews/course/{courseId}`
- **Auth**: AllowAnonymous
- **Query**: `?page=1&pageSize=10`
- **Response** `200`: `ApiResponse<List<ReviewResponse>>`

#### `GET /api/reviews/{id}`
- **Auth**: AllowAnonymous
- **Response** `200`: `ApiResponse<ReviewDetailResponse>`

#### `PUT /api/reviews/{id}`
- **Auth**: Bearer token
- **Body**: `CreateReviewRequest`
- **Response** `200`: `ApiResponse<ReviewResponse>`

#### `DELETE /api/reviews/{id}`
- **Auth**: Bearer token
- **Response** `204`: No Content

#### `POST /api/reviews/{id}/helpful`
- **Auth**: Bearer token
- **Body** (`ReviewHelpfulRequest`):
  - `isHelpful` (boolean)
- **Response** `200`: `ApiResponse<object>`

#### `POST /api/reviews/{id}/flag`
- **Auth**: Instructor
- **Response** `200`: `ApiResponse<object>`

#### `GET /api/reviews/pending`
- **Auth**: Admin
- **Response** `200`: `ApiResponse<List<ReviewDetailResponse>>`

#### `PUT /api/reviews/{id}/moderate`
- **Auth**: Admin
- **Body** (`ModerateReviewRequest`):
  - `status` (string: "Approved" | "Rejected")
- **Response** `200`: `ApiResponse<ReviewResponse>`

---

### 2.8 Wishlist (`/api/wishlist`) — Authenticated

#### `GET /api/wishlist`
- **Auth**: Bearer token
- **Response** `200`: `ApiResponse<WishlistResponseDto>`

#### `POST /api/wishlist/{courseId}`
- **Auth**: Bearer token
- **Response** `200`: `ApiResponse<WishlistItemDto>`

#### `DELETE /api/wishlist/{courseId}`
- **Auth**: Bearer token
- **Response** `204`: No Content

---

### 2.9 Video Comments (`/api/videos/{videoId}/comments`)

#### `GET /api/videos/{videoId}/comments`
- **Auth**: AllowAnonymous (userId optional if authenticated)
- **Response** `200`: `ApiResponse<List<CommentResponseDto>>`

#### `POST /api/videos/{videoId}/comments`
- **Auth**: Bearer token
- **Body** (`CreateCommentDto`):
  - `content` (string)
  - `parentCommentId` (Guid? — for replies)
- **Response** `201`: `ApiResponse<CommentResponseDto>`

#### `PUT /api/videos/{videoId}/comments/{commentId}`
- **Auth**: Bearer token
- **Body**: `CreateCommentDto`
- **Response** `200`: `ApiResponse<CommentResponseDto>`

#### `DELETE /api/videos/{videoId}/comments/{commentId}`
- **Auth**: Bearer token
- **Response** `204`: No Content

#### `POST /api/videos/{videoId}/comments/{commentId}/like`
- **Auth**: Bearer token
- **Response** `200`: `ApiResponse<CommentLikeResponseDto>`

---

### 2.10 Notifications (`/api/notifications`) — Authenticated

#### `GET /api/notifications`
- **Auth**: Bearer token
- **Query**: `?page=1&pageSize=20&isRead=bool|null`
- **Response** `200`: `ApiResponse<NotificationListDto>`

#### `GET /api/notifications/unread-count`
- **Auth**: Bearer token
- **Response** `200`: `ApiResponse<int>`

#### `PATCH /api/notifications/{notificationId:guid}/read`
- **Auth**: Bearer token
- **Response** `200`: `ApiResponse<null>`

#### `POST /api/notifications/mark-all-read`
- **Auth**: Bearer token
- **Response** `200`: `ApiResponse<int>` (count marked)

#### `DELETE /api/notifications/{notificationId:guid}`
- **Auth**: Bearer token
- **Response** `200`: `ApiResponse<null>`

#### `DELETE /api/notifications/clear-all`
- **Auth**: Bearer token
- **Response** `200`: `ApiResponse<int>` (count deleted)

---

### 2.11 Instructor Requests (`/api/instructor-requests`)

#### `GET /api/instructor-requests/can-submit`
- **Auth**: Bearer token
- **Response** `200`: `ApiResponse<bool>`

#### `POST /api/instructor-requests`
- **Auth**: Bearer token (policy: CanSubmitTeacherRequest)
- **Body** (`SubmitInstructorRequestDto`):
  - `message` (string)
  - `documents` (List of `InstructorRequestDocumentDto`):
    - `documentType` (DocumentType)
    - `fileId` (Guid?)
    - `urlValue` (string?)
- **Response** `201`: `ApiResponse<InstructorRequestDto>`

#### `PUT /api/instructor-requests/{requestId:guid}`
- **Auth**: Bearer token
- **Body** (`UpdateInstructorRequestDto`):
  - `message` (string)
  - `documents` (List<InstructorRequestDocumentDto>)
- **Response** `200`: `ApiResponse<InstructorRequestDto>`

#### `POST /api/instructor-requests/{requestId:guid}/documents`
- **Auth**: Bearer token
- **Body** (`AddDocumentToRequestDto`):
  - `documentType` (DocumentType)
  - `fileId` (Guid?)
  - `urlValue` (string?)
- **Response** `200`: `ApiResponse<null>`

#### `GET /api/instructor-requests/my-requests`
- **Auth**: Bearer token
- **Response** `200`: `ApiResponse<List<InstructorRequestDto>>`

#### `GET /api/instructor-requests/my-requests/{requestId:guid}`
- **Auth**: Bearer token
- **Response** `200`: `ApiResponse<InstructorRequestDetailDto>`

#### `DELETE /api/instructor-requests/{requestId:guid}/cancel`
- **Auth**: Bearer token
- **Response** `200`: `ApiResponse<null>`

#### `GET /api/instructor-requests/pending`
- **Auth**: Admin
- **Response** `200`: `ApiResponse<List<InstructorRequestDto>>`

#### `GET /api/instructor-requests/{requestId:guid}`
- **Auth**: Admin
- **Response** `200`: `ApiResponse<InstructorRequestDetailDto>`

#### `PUT /api/instructor-requests/{requestId:guid}/process`
- **Auth**: Admin
- **Body** (`ProcessInstructorRequestDto`):
  - `status` (InstructorRequestStatus: Pending, Approved, Rejected, RequiresMoreInfo)
  - `adminNotes` (string?)
  - `rejectionReason` (string?)
- **Response** `200`: `ApiResponse<InstructorRequestDto>`

#### `DELETE /api/instructor-requests/{requestId:guid}`
- **Auth**: Admin
- **Response** `200`: `ApiResponse<null>`

---

### 2.12 Certificates (`/api/certificates`)

#### `GET /api/certificates/my`
- **Auth**: Bearer token
- **Response** `200`: `ApiResponse<List<CertificateResponse>>`

#### `GET /api/certificates/{id:guid}`
- **Auth**: Bearer token
- **Response** `200`: `ApiResponse<CertificateResponse>`

#### `GET /api/certificates/{id:guid}/download`
- **Auth**: Bearer token
- **Response** `200`: `File` (application/pdf)

#### `GET /api/certificates/verify/{code}`
- **Auth**: AllowAnonymous
- **Response** `200`: `ApiResponse<CertificateVerificationResponse>`

#### Admin Certificates (`/api/admin/certificates`) — Admin

#### `GET /api/admin/certificates`
- **Auth**: Admin
- **Query**: `?page=1&pageSize=20`
- **Response** `200`: `ApiResponse<List<CertificateResponse>>`

#### `POST /api/admin/certificates/{id:guid}/revoke`
- **Auth**: Admin
- **Body** (`RevokeCertificateRequest`):
  - `reason` (string?)
- **Response** `200`: `ApiResponse<null>`

#### `POST /api/admin/certificates/issue`
- **Auth**: Admin
- **Body** (`IssueCertificateRequest`):
  - `userId` (Guid)
  - `courseId` (Guid)
  - `enrollmentId` (Guid)
- **Response** `200`: `ApiResponse<CertificateResponse>`

---

### 2.13 Course Management — Courses (`/api/management/courses`) — Authenticated

#### `POST /api/management/courses`
- **Auth**: Bearer token (Instructor role recommended)
- **Body** (`CreateCourseDto`):
  - `title` (string)
  - `slug` (string?)
  - `description` (string?)
  - `categoryId` (Guid)
  - `level` (CourseLevel: Beginner, Intermediate, Advanced)
  - `language` (CourseLanguage: Ar, En)
  - `price` (decimal)
- **Response** `201`: `ApiResponse<CourseDetailsDto>`

#### `GET /api/management/courses/{id:guid}`
- **Auth**: Bearer token
- **Response** `200`: `ApiResponse<CourseDetailsDto>`

#### `PUT /api/management/courses/{id:guid}`
- **Auth**: Bearer token
- **Body** (`UpdateCourseDto`):
  - `title` (string?)
  - `slug` (string?)
  - `description` (string?)
  - `categoryId` (Guid?)
  - `level` (CourseLevel?)
  - `language` (CourseLanguage?)
  - `price` (decimal?)
- **Response** `200`: `ApiResponse<CourseDetailsDto>`

#### `POST /api/management/courses/{id:guid}/requirements`
- **Auth**: Bearer token
- **Body** (`AddRequirementDto`):
  - `requirementText` (string)
- **Response** `200`: `ApiResponse<CourseRequirementDto>`

#### `DELETE /api/management/courses/{id:guid}/requirements/{requirementId:guid}`
- **Auth**: Bearer token
- **Response** `200`: `ApiResponse<null>`

#### `POST /api/management/courses/{id:guid}/outcomes`
- **Auth**: Bearer token
- **Body** (`AddLearningOutcomeDto`):
  - `outcomeText` (string)
- **Response** `200`: `ApiResponse<CourseLearningOutcomeDto>`

#### `DELETE /api/management/courses/{id:guid}/outcomes/{outcomeId:guid}`
- **Auth**: Bearer token
- **Response** `200`: `ApiResponse<null>`

#### `POST /api/management/courses/{id:guid}/submit-for-review`
- **Auth**: Bearer token
- **Response** `200`: `ApiResponse<null>`

#### `DELETE /api/management/courses/{id:guid}`
- **Auth**: Bearer token
- **Response** `200`: `ApiResponse<null>` or 403/404/400

#### `POST /api/management/courses/{id:guid}/schedule-deletion`
- **Auth**: Bearer token
- **Body** (`ScheduleDeletionDto`):
  - `scheduledDate` (DateTime)
  - `reason` (string?)
- **Response** `200`: `ApiResponse<null>`

#### `POST /api/management/courses/{id:guid}/cancel-scheduled-deletion`
- **Auth**: Bearer token
- **Response** `200`: `ApiResponse<null>`

#### `GET /api/management/courses/{id:guid}/deletion-status`
- **Auth**: Bearer token
- **Response** `200`: `ApiResponse<ScheduledDeletionStatusDto>`

#### `PUT /api/management/courses/{courseId:guid}/image`
- **Auth**: Bearer token
- **Body** (`SetCourseImageRequest`):
  - `fileId` (Guid)
- **Response** `200`: `ApiResponse<CourseDetailsDto>`

---

### 2.14 Course Management — Sections (`/api/management/courses/{courseId:guid}/sections`)

#### `GET /api/management/courses/{courseId:guid}/sections`
- **Auth**: Bearer token
- **Response** `200`: `ApiResponse<List<SectionDto>>`

#### `POST /api/management/courses/{courseId:guid}/sections`
- **Auth**: Bearer token
- **Body** (`CreateSectionDto`):
  - `title` (string)
  - `description` (string?)
- **Response** `201`: `ApiResponse<SectionDto>`

#### `GET /api/management/courses/{courseId:guid}/sections/{sectionId:guid}`
- **Auth**: Bearer token
- **Response** `200`: `ApiResponse<SectionDto>`

#### `PUT /api/management/courses/{courseId:guid}/sections/{sectionId:guid}`
- **Auth**: Bearer token
- **Body** (`UpdateSectionDto`):
  - `title` (string)
  - `description` (string?)
  - `isLocked` (boolean)
- **Response** `200`: `ApiResponse<SectionDto>`

#### `DELETE /api/management/courses/{courseId:guid}/sections/{sectionId:guid}`
- **Auth**: Bearer token
- **Response** `200`: `ApiResponse<null>`

#### `PUT /api/management/courses/{courseId:guid}/sections/reorder`
- **Auth**: Bearer token
- **Body** (`ReorderRequestDto`):
  - `items` (List of `ReorderItemDto`: { id: guid, position: int })
- **Response** `200`: `ApiResponse<null>`

#### `POST /api/management/courses/{courseId:guid}/sections/{sectionId:guid}/items`
- **Auth**: Bearer token
- **Body** (`CreateSectionItemDto`):
  - `itemType` (SectionItemType: Video, Quiz, Document, LiveSession)
  - `itemId` (Guid)
  - `isPreviewAllowed` (boolean)
  - `isMandatory` (boolean, default true)
- **Response** `200`: `ApiResponse<SectionItemDto>`

#### `PUT /api/management/courses/{courseId:guid}/sections/{sectionId:guid}/items/{itemId:guid}`
- **Auth**: Bearer token
- **Body** (`UpdateSectionItemDto`):
  - `isPreviewAllowed` (boolean)
  - `isMandatory` (boolean)
- **Response** `200`: `ApiResponse<SectionItemDto>`

#### `DELETE /api/management/courses/{courseId:guid}/sections/{sectionId:guid}/items/{itemId:guid}`
- **Auth**: Bearer token
- **Response** `200`: `ApiResponse<null>`

#### `PUT /api/management/courses/{courseId:guid}/sections/{sectionId:guid}/items/reorder`
- **Auth**: Bearer token
- **Body** (`ReorderRequestDto`)
- **Response** `200`: `ApiResponse<null>`

---

### 2.15 Course Management — Videos (`/api/courses/{courseId:guid}/videos`) — Instructor

#### `GET /api/courses/{courseId:guid}/videos/{id:guid}`
- **Auth**: Instructor
- **Response** `200`: `ApiResponse<VideoResponseDto>`

#### `POST /api/courses/{courseId:guid}/videos`
- **Auth**: Instructor
- **Body** (`CreateVideoDto`):
  - `sectionId` (Guid)
  - `title` (string)
  - `videoFileId` (Guid)
  - `provider` (VideoProvider: Local, YouTube, Vimeo, Minio)
  - `providerVideoId` (string?)
  - `durationSeconds` (int)
  - `transcript` (string?)
  - `isPreview` (boolean)
- **Response** `201`: `ApiResponse<VideoResponseDto>`

#### `PUT /api/courses/{courseId:guid}/videos/{id:guid}`
- **Auth**: Instructor
- **Body** (`UpdateVideoDto`):
  - `title` (string)
  - `transcript` (string?)
  - `isPreview` (boolean)
- **Response** `200`: `ApiResponse<VideoResponseDto>`

#### `DELETE /api/courses/{courseId:guid}/videos/{id:guid}`
- **Auth**: Instructor
- **Response** `200`: `ApiResponse<null>`

---

### 2.16 Course Management — Documents (`/api/courses/{courseId:guid}/documents`) — Instructor

#### `GET /api/courses/{courseId:guid}/documents/{id:guid}`
- **Auth**: Instructor
- **Response** `200`: `ApiResponse<DocumentResponseDto>`

#### `POST /api/courses/{courseId:guid}/documents`
- **Auth**: Instructor
- **Body** (`CreateDocumentDto`):
  - `sectionId` (Guid)
  - `title` (string)
  - `description` (string?)
  - `fileId` (Guid)
  - `isDownloadable` (boolean, default true)
- **Response** `201`: `ApiResponse<DocumentResponseDto>`

#### `PUT /api/courses/{courseId:guid}/documents/{id:guid}`
- **Auth**: Instructor
- **Body** (`UpdateDocumentDto`):
  - `title` (string)
  - `description` (string?)
  - `isDownloadable` (boolean)
- **Response** `200`: `ApiResponse<DocumentResponseDto>`

#### `DELETE /api/courses/{courseId:guid}/documents/{id:guid}`
- **Auth**: Instructor
- **Response** `200`: `ApiResponse<null>`

---

### 2.17 Course Management — Quizzes (`/api/courses/{courseId:guid}/quizzes`) — Instructor

#### `GET /api/courses/{courseId:guid}/quizzes/{id:guid}`
- **Auth**: Instructor
- **Response** `200`: `ApiResponse<QuizResponseDto>`

#### `POST /api/courses/{courseId:guid}/quizzes`
- **Auth**: Instructor
- **Body** (`CreateQuizDto`):
  - `sectionId` (Guid)
  - `title` (string)
  - `description` (string?)
  - `durationMinutes` (int?)
  - `passingScorePercent` (int, default 60)
  - `maxAttempts` (int?)
  - `shuffleQuestions` (boolean)
  - `shuffleOptions` (boolean)
  - `showResultsImmediately` (boolean, default true)
  - `allowReview` (boolean, default true)
  - `availableFrom` (DateTime?)
  - `availableUntil` (DateTime?)
- **Response** `201`: `ApiResponse<QuizResponseDto>`

#### `PUT /api/courses/{courseId:guid}/quizzes/{id:guid}`
- **Auth**: Instructor
- **Body** (`UpdateQuizDto`)
- **Response** `200`: `ApiResponse<QuizResponseDto>`

#### `DELETE /api/courses/{courseId:guid}/quizzes/{id:guid}`
- **Auth**: Instructor
- **Response** `200`: `ApiResponse<null>` (or 409 Conflict)

#### `POST /api/courses/{courseId:guid}/quizzes/{quizId:guid}/questions`
- **Auth**: Instructor
- **Body** (`CreateQuestionDto`):
  - `questionText` (string)
  - `type` (QuestionType: MultipleChoice, TrueFalse, ShortAnswer)
  - `points` (int, default 1)
  - `explanation` (string?)
  - `position` (int)
  - `options` (List of `CreateOptionDto`):
    - `optionText` (string)
    - `isCorrect` (boolean)
    - `position` (int)
- **Response** `201`: `ApiResponse<QuestionResponseDto>`

#### `PUT /api/courses/{courseId:guid}/quizzes/{quizId:guid}/questions/{questionId:guid}`
- **Auth**: Instructor
- **Body**: `CreateQuestionDto`
- **Response** `200`: `ApiResponse<QuestionResponseDto>`

#### `DELETE /api/courses/{courseId:guid}/quizzes/{quizId:guid}/questions/{questionId:guid}`
- **Auth**: Instructor
- **Response** `200`: `ApiResponse<null>`

---

### 2.18 Quiz Attempts (`/api/enrollments/{enrollmentId:guid}/quizzes/{quizId:guid}/attempts`) — Authenticated

#### `POST /api/enrollments/{enrollmentId:guid}/quizzes/{quizId:guid}/attempts`
- **Auth**: Bearer token
- **Response** `201`: `ApiResponse<QuizAttemptResponseDto>`

#### `PUT /api/enrollments/{enrollmentId:guid}/quizzes/{quizId:guid}/attempts/{attemptId:guid}`
- **Auth**: Bearer token
- **Body** (`SubmitAttemptDto`):
  - `answers` (List of `SubmitAnswerDto`):
    - `questionId` (Guid)
    - `selectedOptionId` (Guid?)
    - `answerText` (string?)
- **Response** `200`: `ApiResponse<QuizResultDto>`

#### `GET /api/enrollments/{enrollmentId:guid}/quizzes/{quizId:guid}/attempts`
- **Auth**: Bearer token
- **Response** `200`: `ApiResponse<IEnumerable<QuizAttemptResponseDto>>`

#### `GET /api/enrollments/{enrollmentId:guid}/quizzes/{quizId:guid}/attempts/{attemptId:guid}`
- **Auth**: Bearer token
- **Response** `200`: `ApiResponse<QuizResultDto>`

---

### 2.19 Enrollments (`/api/enrollments`) — Authenticated

#### `POST /api/enrollments`
- **Auth**: Bearer token
- **Body** (`CreateEnrollmentDto`):
  - `courseId` (Guid)
  - `source` (EnrollmentSource: Purchase, Gift, AdminGrant, Coupon)
  - `userId` (auto-injected from token, ignore in request)
- **Response** `201`: `ApiResponse<EnrollmentResponseDto>`

#### `GET /api/enrollments`
- **Auth**: Bearer token
- **Response** `200`: `ApiResponse<IEnumerable<EnrollmentResponseDto>>`

#### `GET /api/enrollments/{id:guid}`
- **Auth**: Bearer token
- **Response** `200`: `ApiResponse<EnrollmentDetailDto>`

#### `GET /api/enrollments/{enrollmentId:guid}/progress`
- **Auth**: Bearer token
- **Response** `200`: `ApiResponse<IEnumerable<ContentProgressDto>>`

#### `PUT /api/enrollments/{enrollmentId:guid}/progress`
- **Auth**: Bearer token
- **Body** (`UpdateProgressDto`):
  - `watchTimeSeconds` (int)
  - `completionPercentage` (decimal?)
  - `metadata` (string?)
  - `markAsCompleted` (boolean)
- **Response** `200`: `ApiResponse<ContentProgressDto>`

#### `POST /api/enrollments/{enrollmentId:guid}/progress/{contentType}/{contentId:guid}/complete`
- **Auth**: Bearer token
- **Path**: `contentType` = one of: "Video", "Quiz", "Document", "LiveSession"
- **Response** `200`: `ApiResponse<ContentProgressDto>`

---

### 2.20 Public Courses (`/api/public/courses`) — AllowAnonymous

#### `GET /api/public/courses`
- **Auth**: AllowAnonymous
- **Query** (`PublicCourseFilterDto`):
  - `searchQuery` (string?)
  - `categoryId` (Guid?)
  - `level` (CourseLevel?)
  - `language` (CourseLanguage?)
  - `minPrice` (decimal?)
  - `maxPrice` (decimal?)
  - `isFreeOnly` (boolean?)
  - `minRating` (decimal?)
  - `sortBy` (PublicCourseSortBy: PublishedAt, Price, AverageRating, EnrollmentCount, Title)
  - `sortDescending` (boolean, default true)
  - `page` (int, default 1)
  - `pageSize` (int, default 12)
- **Response** `200`: `ApiResponse<PagedList<PublicCourseDto>>`

#### `GET /api/public/courses/{id:guid}`
- **Auth**: AllowAnonymous
- **Response** `200`: `ApiResponse<PublicCourseDetailDto>`

#### `GET /api/public/courses/slug/{slug}`
- **Auth**: AllowAnonymous
- **Response** `200`: `ApiResponse<PublicCourseDetailDto>`

#### `GET /api/public/courses/search/suggest?query=...&limit=5`
- **Auth**: AllowAnonymous
- **Query**: `query` (string), `limit` (int, default 5)
- **Response** `200`: `ApiResponse<List<CourseSuggestionDto>>`

#### `GET /api/public/courses/stats`
- **Auth**: AllowAnonymous
- **Response** `200`: `ApiResponse<PlatformStatsDto>`

#### `GET /api/public/courses/{id:guid}/related?limit=4`
- **Auth**: AllowAnonymous
- **Response** `200`: `ApiResponse<List<PublicCourseDto>>`

#### `GET /api/public/courses/filters/options`
- **Auth**: AllowAnonymous
- **Response** `200`: `ApiResponse<FilterOptionsDto>`

---

### 2.21 Admin Courses (`/api/admin/courses`) — Admin

#### `POST /api/admin/courses/{id:guid}/approve`
- **Auth**: Admin
- **Response** `200`: `ApiResponse<null>`

#### `POST /api/admin/courses/{id:guid}/reject`
- **Auth**: Admin
- **Body** (`RejectCourseRequest`):
  - `reason` (string)
- **Response** `200`: `ApiResponse<null>`

#### `GET /api/admin/courses/edit-requests`
- **Auth**: Admin
- **Query** (`EditRequestFilterDto`):
  - `status` (EditRequestStatus?)
  - `requestType` (EditRequestType?)
  - `courseId` (Guid?)
  - `instructorId` (Guid?)
  - `page` (int, default 1)
  - `pageSize` (int, default 20)
- **Response** `200`: `ApiResponse<PagedList<EditRequestSummaryDto>>`

#### `GET /api/admin/courses/edit-requests/{requestId:guid}`
- **Auth**: Admin
- **Response** `200`: `ApiResponse<EditRequestDetailDto>`

#### `POST /api/admin/courses/edit-requests/{requestId:guid}/review`
- **Auth**: Admin
- **Body** (`ReviewRequestDto`):
  - `approve` (boolean)
  - `notes` (string?)
- **Response** `200`: `ApiResponse<EditResultDto>`

---

### 2.22 Live Sessions (`/api/courses/{courseId:guid}/live-sessions`) — Instructor

#### `GET /api/courses/{courseId:guid}/live-sessions`
- **Auth**: Instructor
- **Response** `200`: `ApiResponse<List<LiveSessionResponseDto>>`

#### `POST /api/courses/{courseId:guid}/live-sessions`
- **Auth**: Instructor
- **Body** (`CreateLiveSessionDto`):
  - `sectionId` (Guid)
  - `title` (string)
  - `description` (string?)
  - `scheduledStart` (DateTime)
  - `scheduledEnd` (DateTime)
  - `meetingUrl` (string)
  - `password` (string?)
  - `maxAttendees` (int?)
- **Response** `200`: `ApiResponse<LiveSessionResponseDto>`

#### `PUT /api/courses/{courseId:guid}/live-sessions/{sessionId:guid}/status`
- **Auth**: Instructor
- **Body** (`UpdateLiveSessionStatusDto`):
  - `status` (LiveSessionStatus: Scheduled, Live, Finished, Cancelled)
  - `meetingUrl` (string?)
  - `password` (string?)
  - `maxAttendees` (int?)
- **Response** `200`: `ApiResponse<LiveSessionResponseDto>`

#### `DELETE /api/courses/{courseId:guid}/live-sessions/{sessionId:guid}`
- **Auth**: Instructor
- **Response** `200`: `ApiResponse<null>`

### Live Attendance (`/api/live-sessions/{sessionId:guid}/attendance`) — Authenticated

#### `POST /api/live-sessions/{sessionId:guid}/attendance/join`
- **Auth**: Bearer token
- **Response** `200`: `ApiResponse<null>`

#### `POST /api/live-sessions/{sessionId:guid}/attendance/leave`
- **Auth**: Bearer token
- **Response** `200`: `ApiResponse<null>`

#### `GET /api/live-sessions/{sessionId:guid}/attendance/count`
- **Auth**: Bearer token
- **Response** `200`: `ApiResponse<int>`

---

### 2.23 Management (Instructor Course List) (`/api/management`)

#### `GET /api/management/courses`
- **Auth**: Instructor
- **Response** `200`: `ApiResponse<List<ManagementCourseDto>>`

---

### 2.24 Cart (`/api/cart`) — Authenticated

#### `GET /api/cart`
- **Auth**: Bearer token
- **Response** `200`: `ApiResponse<CartResponseDto>`

#### `POST /api/cart/items`
- **Auth**: Bearer token
- **Body** (`AddToCartRequest`):
  - `courseId` (Guid)
- **Response** `200`: `ApiResponse<CartItemDto>`

#### `DELETE /api/cart/items/{itemId}`
- **Auth**: Bearer token
- **Response** `204`: No Content

#### `POST /api/cart/apply-coupon`
- **Auth**: Bearer token
- **Body** (`ApplyCouponRequest`):
  - `code` (string)
- **Response** `200`: `ApiResponse<ApplyCouponResponse>`

#### `DELETE /api/cart/coupon`
- **Auth**: Bearer token
- **Response** `204`: No Content

### 2.25 Coupons (`/api/coupons`)

#### `POST /api/coupons/validate`
- **Auth**: Bearer token
- **Body** (`ValidateCouponRequest`):
  - `code` (string)
  - `cartTotal` (decimal)
  - `courseIds` (List of Guid)
- **Response** `200`: `ApiResponse<ValidateCouponResponse>`

### 2.26 Admin Coupons (`/api/admin/coupons`) — Admin

#### `GET /api/admin/coupons?isActive=bool|null`
- **Auth**: Admin
- **Response** `200`: `ApiResponse<IEnumerable<CouponResponseDto>>`

#### `GET /api/admin/coupons/{couponId}`
- **Auth**: Admin
- **Response** `200`: `ApiResponse<CouponResponseDto>`

#### `POST /api/admin/coupons`
- **Auth**: Admin
- **Body** (`CreateCouponDto`):
  - `code` (string)
  - `type` (string: "Percentage" | "Fixed")
  - `value` (decimal)
  - `maxDiscountAmount` (decimal?)
  - `minimumPurchaseAmount` (decimal?)
  - `applicableTo` (string: "All" | "SpecificCourses" | "Category")
  - `courseIds` (List<Guid>?)
  - `usageLimit` (int?)
  - `userLimitPerUser` (int?)
  - `isPublic` (boolean, default true)
  - `validFrom` (DateTime?)
  - `validUntil` (DateTime?)
- **Response** `200`: `ApiResponse<CouponResponseDto>`

#### `PUT /api/admin/coupons/{couponId}`
- **Auth**: Admin
- **Body**: `CreateCouponDto`
- **Response** `200`: `ApiResponse<CouponResponseDto>`

#### `PATCH /api/admin/coupons/{couponId}/toggle`
- **Auth**: Admin
- **Response** `200`: `ApiResponse<bool>`

#### `DELETE /api/admin/coupons/{couponId}`
- **Auth**: Admin
- **Response** `204`: No Content

### 2.27 Orders (`/api/orders`) — Authenticated

#### `GET /api/orders`
- **Auth**: Bearer token
- **Response** `200`: `ApiResponse<IEnumerable<OrderResponseDto>>`

#### `GET /api/orders/{orderId}`
- **Auth**: Bearer token
- **Response** `200`: `ApiResponse<OrderDetailDto>`

#### `POST /api/orders`
- **Auth**: Bearer token
- **Body** (`CreateOrderRequest`):
  - `couponCode` (string?)
- **Response** `200`: `ApiResponse<OrderResponseDto>`

### 2.28 Payments (`/api/payments`) — Authenticated

#### `POST /api/payments/process?orderId=guid`
- **Auth**: Bearer token
- **Query**: `orderId` (Guid)
- **Body** (`ProcessPaymentRequest`):
  - `paymentMethodId` (Guid)
- **Response** `200`: `ApiResponse<PaymentResponseDto>`

#### `GET /api/payments/methods`
- **Auth**: AllowAnonymous
- **Response** `200`: `ApiResponse<IEnumerable<PaymentMethodResponse>>`

#### `GET /api/payments/history/{orderId}`
- **Auth**: Bearer token
- **Response** `200`: `ApiResponse<IEnumerable<PaymentResponseDto>>`

### 2.29 Admin Payment Methods (`/api/admin/payment-methods`) — Admin

#### `GET /api/admin/payment-methods`
- **Auth**: Admin
- **Response** `200`: `ApiResponse<IEnumerable<PaymentMethodResponse>>`

#### `POST /api/admin/payment-methods`
- **Auth**: Admin
- **Body** (`CreatePaymentMethodRequest`):
  - `name` (string)
  - `provider` (string)
  - `type` (string, default "CreditCard")
  - `configuration` (string? — JSON)
- **Response** `200`: `ApiResponse<PaymentMethodResponse>`

#### `PATCH /api/admin/payment-methods/{id}/toggle`
- **Auth**: Admin
- **Response** `200`: `ApiResponse<bool>`

### 2.30 Refunds (`/api/refunds`) — Authenticated

#### `POST /api/refunds`
- **Auth**: Bearer token
- **Body** (`RequestRefundRequest`):
  - `paymentId` (Guid)
  - `reason` (string?)
- **Response** `200`: `ApiResponse<RefundResponseDto>`

#### `GET /api/refunds`
- **Auth**: Bearer token
- **Response** `200`: `ApiResponse<IEnumerable<RefundResponseDto>>`

### 2.31 Admin Refunds (`/api/admin/refunds`) — Admin

#### `GET /api/admin/refunds?status=RefundStatus|null`
- **Auth**: Admin
- **Response** `200`: `ApiResponse<IEnumerable<RefundResponseDto>>`

#### `POST /api/admin/refunds/approve`
- **Auth**: Admin
- **Body** (`ProcessRefundRequest`):
  - `refundId` (Guid)
  - `adminNotes` (string?)
- **Response** `200`: `ApiResponse<RefundResponseDto>`

#### `POST /api/admin/refunds/reject`
- **Auth**: Admin
- **Body**: `ProcessRefundRequest`
- **Response** `200`: `ApiResponse<RefundResponseDto>`

---

### 2.32 Messages (`/api/messages`) — Authenticated (Rate Limited: Messaging, 30/min)

#### `POST /api/messages`
- **Auth**: Bearer token
- **Rate Limit**: Messaging (30/min)
- **Body** (`SendMessageRequest`):
  - `receiverId` (Guid)
  - `content` (string)
- **Response** `200`: `ApiResponse<MessageResponse>`

#### `GET /api/messages/conversations?page=1&pageSize=20`
- **Auth**: Bearer token
- **Rate Limit**: Messaging (30/min)
- **Response** `200`: `ApiResponse<ConversationListResponse>`

#### `GET /api/messages/conversations/{otherUserId}?page=1&pageSize=50`
- **Auth**: Bearer token
- **Rate Limit**: Messaging (30/min)
- **Response** `200`: `ApiResponse<ConversationMessagesResponse>`

#### `GET /api/messages/unread-count`
- **Auth**: Bearer token
- **Rate Limit**: Messaging (30/min)
- **Response** `200`: `ApiResponse<UnreadCountResponse>`

#### `PATCH /api/messages/{messageId}/read`
- **Auth**: Bearer token
- **Rate Limit**: Messaging (30/min)
- **Response** `200`: `ApiResponse<object>`

#### `DELETE /api/messages/{messageId}`
- **Auth**: Bearer token
- **Rate Limit**: Messaging (30/min)
- **Response** `204`: No Content

### 2.33 Announcements (`/api/announcements`) — Authenticated

#### `POST /api/announcements`
- **Auth**: Bearer token
- **Body** (`CreateAnnouncementRequest`):
  - `title` (string)
  - `content` (string)
  - `target` (string: "All", "Students", "Instructors", "Admins", "SpecificCourse")
  - `courseId` (Guid?)
- **Response** `200`: `ApiResponse<AnnouncementResponse>`

#### `POST /api/announcements/course/{courseId}`
- **Auth**: Bearer token
- **Body** (`CreateAnnouncementRequest` without courseId — auto-set to SpecificCourse)
- **Response** `200`: `ApiResponse<AnnouncementResponse>`

#### `GET /api/announcements?page=1&pageSize=20`
- **Auth**: Bearer token
- **Response** `200`: `ApiResponse<AnnouncementListResponse>`

#### `PUT /api/announcements/{id}`
- **Auth**: Bearer token
- **Body** (`UpdateAnnouncementRequest`):
  - `title` (string)
  - `content` (string)
  - `target` (string)
  - `courseId` (Guid?)
  - `isActive` (boolean)
- **Response** `200`: `ApiResponse<AnnouncementResponse>`

#### `PATCH /api/announcements/{id}/deactivate`
- **Auth**: Bearer token
- **Response** `200`: `ApiResponse<object>`

#### `DELETE /api/announcements/{id}`
- **Auth**: Bearer token
- **Response** `204`: No Content

### 2.34 Reports (`/api/reports`) — Authenticated

#### `POST /api/reports`
- **Auth**: Bearer token
- **Body** (`CreateReportRequest`):
  - `entityType` (string)
  - `entityId` (Guid)
  - `reason` (string)
  - `description` (string?)
- **Response** `200`: `ApiResponse<ReportResponse>`

#### `GET /api/reports/pending?page=1&pageSize=20`
- **Auth**: Admin
- **Response** `200`: `ApiResponse<ReportListResponse>`

#### `PATCH /api/reports/{id}/resolve`
- **Auth**: Admin
- **Body** (`ResolveReportRequest`):
  - `status` (string, default "Dismissed")
  - `adminNote` (string?)
- **Response** `200`: `ApiResponse<ReportResponse>`

### 2.35 System Settings (`/api/system-settings`) — Admin

#### `GET /api/system-settings?group=string|null`
- **Auth**: Admin
- **Response** `200`: `ApiResponse<List<SettingResponse>>`

#### `GET /api/system-settings/{key}`
- **Auth**: Admin
- **Response** `200`: `ApiResponse<SettingResponse>`

#### `POST /api/system-settings`
- **Auth**: Admin
- **Body** (`CreateSettingRequest`):
  - `group` (string, default "General")
  - `key` (string)
  - `value` (string)
  - `dataType` (string, default "String")
  - `description` (string?)
- **Response** `201`: `ApiResponse<SettingResponse>`

#### `PUT /api/system-settings/{key}`
- **Auth**: Admin
- **Body** (`UpdateSettingRequest`):
  - `value` (string)
- **Response** `200`: `ApiResponse<SettingResponse>`

#### `DELETE /api/system-settings/{key}`
- **Auth**: Admin
- **Response** `204`: No Content

### 2.36 Activity Logs (`/api/activity-logs`) — Admin

#### `GET /api/activity-logs`
- **Auth**: Admin
- **Query** (`ActivityLogFilterRequest`):
  - `userId` (Guid?)
  - `action` (string?)
  - `entityType` (string?)
  - `dateFrom` (DateTime?)
  - `dateTo` (DateTime?)
  - `ipAddress` (string?)
  - `page` (int, default 1)
  - `pageSize` (int, default 50)
- **Response** `200`: `ApiResponse<ActivityLogListResponse>`

---

### 2.37 Student Dashboard (`/api/student/dashboard`) — Student

#### `GET /api/student/dashboard/overview`
- **Auth**: Student
- **Response** `200`: `ApiResponse<StudentOverviewDto>`

#### `GET /api/student/dashboard/courses`
- **Auth**: Student
- **Response** `200`: `ApiResponse<List<StudentCourseDto>>`

#### `GET /api/student/dashboard/weekly-activity?weeks=4`
- **Auth**: Student
- **Response** `200`: `ApiResponse<ChartSeriesDto>`

#### `GET /api/student/dashboard/certificates`
- **Auth**: Student
- **Response** `200`: `ApiResponse<List<StudentCertificateDto>>`

### 2.38 Instructor Dashboard (`/api/instructor/dashboard`) — Instructor

#### `GET /api/instructor/dashboard/overview`
- **Auth**: Instructor
- **Response** `200`: `ApiResponse<InstructorOverviewDto>`

#### `GET /api/instructor/dashboard/courses`
- **Auth**: Instructor
- **Response** `200`: `ApiResponse<List<ManagementCourseDto>>`

#### `GET /api/instructor/dashboard/revenue?months=12`
- **Auth**: Instructor
- **Response** `200`: `ApiResponse<InstructorDashboardRevenueDto>`

#### `GET /api/instructor/dashboard/students`
- **Auth**: Instructor
- **Response** `200`: `ApiResponse<InstructorDashboardStudentsDto>`

#### `GET /api/instructor/dashboard/pending-requests`
- **Auth**: Instructor
- **Response** `200`: `ApiResponse<List<PendingEditRequestDto>>`

#### `GET /api/instructor/dashboard/recent-reviews?limit=10`
- **Auth**: Instructor
- **Response** `200`: `ApiResponse<List<ReviewSummaryDto>>`

### 2.39 Admin Dashboard (`/api/admin/dashboard`) — Admin

#### `GET /api/admin/dashboard/overview`
- **Auth**: Admin
- **Response** `200`: `ApiResponse<AdminOverviewDto>`

#### `GET /api/admin/dashboard/revenue?months=12`
- **Auth**: Admin
- **Response** `200`: `ApiResponse<AdminRevenueDto>`

#### `GET /api/admin/dashboard/user-growth?months=6`
- **Auth**: Admin
- **Response** `200`: `ApiResponse<AdminUserGrowthDto>`

#### `GET /api/admin/dashboard/enrollment-trend?months=12`
- **Auth**: Admin
- **Response** `200`: `ApiResponse<AdminEnrollmentTrendDto>`

#### `GET /api/admin/dashboard/top-courses?limit=10`
- **Auth**: Admin
- **Response** `200`: `ApiResponse<List<TopCourseDto>>`

---

### 2.40 Health Check Endpoints

#### `GET /health`
- **Auth**: None
- **Response**: Health check report for all checks (liveness + database)

#### `GET /healthz`
- **Auth**: None
- **Response**: Liveness check only

#### `GET /ready`
- **Auth**: None
- **Response**: Readiness check (database connectivity)

---

## 3. Data Models / DTOs

### 3.1 Auth DTOs

| DTO | Type | Properties |
|-----|------|-----------|
| **AuthResponseDto** | Response | `accessToken: string`, `refreshToken: string`, `sessionId: guid`, `expiresAt: datetime`, `user: UserInfoDto` |
| **LoginDto** | Request | `email: string`, `password: string`, `rememberMe: bool` |
| **RegisterDto** | Request | `firstName: string`, `lastName: string`, `email: string`, `password: string`, `confirmPassword: string`, `gender: Gender?`, `dateOfBirth: DateOnly?`, `phoneNumber: string?`, `country: string?`, `city: string?`, `streetLine1: string?`, `postalCode: string?` |
| **RegisterResponseDto** | Response | `userId: guid`, `email: string`, `message: string` |
| **RefreshTokenDto** | Request | `refreshToken: string` |
| **VerifyEmailDto** | Request | `token: string`, `email: string` |
| **ResendVerificationDto** | Request | `email: string` |
| **ForgotPasswordDto** | Request | `email: string` |
| **ResetPasswordDto** | Request | `token: string`, `email: string`, `newPassword: string`, `confirmPassword: string` |
| **ChangePasswordDto** | Request | `currentPassword: string`, `newPassword: string` |
| **OAuthLoginDto** | Request | `idToken: string`, `provider: string` (google/microsoft) |
| **UserInfoDto** | Response | `id: guid`, `email: string`, `fullName: string`, `profilePictureUrl: string?`, `isActive: bool`, `emailConfirmed: bool`, `roles: string[]` |
| **SessionDto** | Response | `id: guid`, `ipAddress: string`, `userAgent: string`, `createdAt: datetime`, `lastUsed: datetime?`, `isActive: bool` |

### 3.2 Profile DTOs

| DTO | Type | Properties |
|-----|------|-----------|
| **ProfileDto** | Response | `id: guid`, `fullName: string`, `email: string`, `bio: string?`, `gender: Gender?`, `dateOfBirth: DateOnly?`, `nationality: string?`, `profileImageUrl: string?`, `createdAt: datetime`, `phones: PhoneDto[]`, `addresses: AddressDto[]` |
| **PublicProfileDto** | Response | `id: guid`, `fullName: string?`, `bio: string?`, `nationality: string?`, `profileImageUrl: string?`, `createdAt: datetime` |
| **PhoneDto** | Response | `id: guid`, `phoneNumber: string`, `type: PhoneType`, `isVerified: bool`, `isDefault: bool` |
| **AddressDto** | Response | `id: guid`, `type: string`, `streetLine1: string`, `streetLine2: string?`, `city: string`, `stateProvince: string?`, `postalCode: string`, `country: string`, `contactPhone: string?`, `isDefault: bool` |
| **UpdateProfileDto** | Request | `firstName: string?`, `lastName: string?`, `bio: string?`, `gender: Gender?`, `dateOfBirth: DateOnly?`, `nationality: string?` |
| **AddPhoneDto** | Request | `phoneNumber: string`, `type: PhoneType`, `isDefault: bool` |
| **AddAddressDto** | Request | `type: string`, `streetLine1: string`, `streetLine2: string?`, `city: string`, `stateProvince: string?`, `postalCode: string`, `country: string`, `contactPhone: string?`, `isDefault: bool` |
| **UpdateAddressDto** | Request | `type: string?`, `streetLine1: string?`, `streetLine2: string?`, `city: string?`, `stateProvince: string?`, `postalCode: string?`, `country: string?`, `contactPhone: string?`, `isDefault: bool?` |
| **SetProfileImageDto** | Request | `fileId: guid` |

### 3.3 Category DTOs

| DTO | Type | Properties |
|-----|------|-----------|
| **CategoryResponseDto** | Response | `id: guid`, `name: string`, `description: string?`, `parentId: guid?`, `imageUrl: string?`, `slug: string?`, `position: int`, `children: CategoryResponseDto[]` |
| **CreateCategoryDto** | Request | `name: string`, `description: string?`, `slug: string?`, `parentId: guid?`, `position: int` |
| **UpdateCategoryDto** | Request | `name: string`, `description: string?`, `slug: string?`, `parentId: guid?`, `position: int` |
| **SetCategoryImageRequest** | Request | `fileId: guid` |

### 3.4 Media DTOs

| DTO | Type | Properties |
|-----|------|-----------|
| **UploadUrlRequestDto** | Request | `fileType: StoredFileType`, `fileName: string`, `contentType: string`, `fileSizeBytes: long`, `visibility: FileVisibility`, `relatedEntityId: guid?`, `relatedEntityType: string?` |
| **UploadUrlResponseDto** | Response | `fileId: guid`, `uploadUrl: string`, `objectKey: string`, `bucket: string`, `expiresAt: datetime`, `requiredHeaders: Dictionary<string,string>` |
| **ConfirmUploadDto** | Request | `fileId: guid`, `objectKey: string`, `bucket: string` |
| **ViewUrlResponseDto** | Response | `fileId: guid`, `viewUrl: string`, `expiresAt: datetime`, `contentType: string`, `sizeBytes: long` |
| **MediaFileDto** | Response | `id: guid`, `originalName: string`, `filePath: string`, `bucket: string`, `fileType: StoredFileType`, `visibility: FileVisibility`, `status: FileStatus`, `mimeType: string?`, `sizeBytes: long`, `uploadedAt: datetime`, `uploadedBy: guid`, `uploaderName: string?` |
| **MediaDetailDto** | Response (extends MediaFileDto) | `+ deletedAt: datetime?`, `storageProvider: string?`, `video: RelatedVideoDto?`, `document: RelatedDocumentDto?`, `course: RelatedCourseDto?` |
| **RelatedVideoDto** | Response | `id: guid`, `title: string`, `status: VideoStatus`, `durationSeconds: int`, `sectionId: guid?`, `sectionTitle: string?`, `courseId: guid?`, `courseTitle: string?` |
| **RelatedDocumentDto** | Response | `id: guid`, `title: string`, `sectionId: guid?`, `sectionTitle: string?`, `courseId: guid?`, `courseTitle: string?` |
| **RelatedCourseDto** | Response | `id: guid`, `title: string`, `usageType: string?` |
| **MediaFilterDto** | Request/Query | `fileType: StoredFileType?`, `bucket: string?`, `visibility: FileVisibility?`, `uploadedBy: guid?`, `fromDate: datetime?`, `toDate: datetime?`, `status: FileStatus?`, `includeDeleted: bool`, `page: int`, `pageSize: int`, `searchTerm: string?` |
| **StorageStatsDto** | Response | `totalFilesCount: long`, `totalSizeBytes: long`, `bucketStats: Dictionary<string,BucketStats>`, `fileTypeCounts: Dictionary<StoredFileType,long>` |
| **BucketStats** | Response | `filesCount: long`, `sizeBytes: long` |

### 3.5 Course Management DTOs

| DTO | Type | Properties |
|-----|------|-----------|
| **CreateCourseDto** | Request | `title: string`, `slug: string?`, `description: string?`, `categoryId: guid`, `level: CourseLevel`, `language: CourseLanguage`, `price: decimal` |
| **UpdateCourseDto** | Request | `title: string?`, `slug: string?`, `description: string?`, `categoryId: guid?`, `level: CourseLevel?`, `language: CourseLanguage?`, `price: decimal?` |
| **CourseSummaryDto** | Response | `id: guid`, `title: string`, `slug: string`, `description: string?`, `price: decimal`, `level: CourseLevel`, `language: CourseLanguage`, `status: CourseStatus`, `totalDurationMinutes: int`, `enrollmentCount: int`, `averageRating: decimal`, `thumbnailUrl: string?`, `categoryName: string?`, `createdAt: datetime` |
| **CourseDetailsDto** | Response | `id: guid`, `title: string`, `slug: string`, `description: string?`, `categoryId: guid`, `categoryName: string?`, `createdBy: guid`, `creatorName: string`, `level: CourseLevel`, `language: CourseLanguage`, `status: CourseStatus`, `price: decimal`, `imageUrl: string?`, `courseImageFileId: guid?`, `totalDurationMinutes: int`, `enrollmentCount: int`, `averageRating: decimal`, `createdAt: datetime`, `updatedAt: datetime?`, `publishedAt: datetime?`, `scheduledDeletionAt: datetime?`, `deletionReason: string?`, `isReadOnlyForStudents: bool`, `rejectionReason: string?`, `requirements: CourseRequirementDto[]`, `learningOutcomes: CourseLearningOutcomeDto[]` |
| **CourseRequirementDto** | Response | `id: guid`, `requirementText: string` |
| **CourseLearningOutcomeDto** | Response | `id: guid`, `outcomeText: string` |
| **AddRequirementDto** | Request | `requirementText: string` |
| **AddLearningOutcomeDto** | Request | `outcomeText: string` |
| **SetCourseImageRequest** | Request | `fileId: guid` |
| **ScheduleDeletionDto** | Request | `scheduledDate: datetime`, `reason: string?` |
| **ScheduledDeletionStatusDto** | Response | `courseId: guid`, `scheduledDeletionAt: datetime?`, `deletionReason: string?`, `isReadOnlyForStudents: bool`, `isPending: bool` (computed), `isExecuted: bool` (computed), `enrolledStudentCount: int` |

### 3.6 Section DTOs

| DTO | Type | Properties |
|-----|------|-----------|
| **SectionDto** | Response | `id: guid`, `courseId: guid`, `title: string`, `description: string?`, `position: int`, `isLocked: bool`, `items: SectionItemDto[]` |
| **SectionItemDto** | Response | `id: guid`, `sectionId: guid`, `itemType: SectionItemType`, `itemId: guid`, `position: int`, `isPreviewAllowed: bool`, `isMandatory: bool` |
| **CreateSectionDto** | Request | `title: string`, `description: string?` |
| **UpdateSectionDto** | Request | `title: string`, `description: string?`, `isLocked: bool` |
| **CreateSectionItemDto** | Request | `itemType: SectionItemType`, `itemId: guid`, `isPreviewAllowed: bool`, `isMandatory: bool` |
| **UpdateSectionItemDto** | Request | `isPreviewAllowed: bool`, `isMandatory: bool` |
| **ReorderItemDto** | Request | `id: guid`, `position: int` |
| **ReorderRequestDto** | Request | `items: ReorderItemDto[]` |

### 3.7 Video DTOs

| DTO | Type | Properties |
|-----|------|-----------|
| **CreateVideoDto** | Request | `sectionId: guid`, `title: string`, `videoFileId: guid`, `provider: VideoProvider`, `providerVideoId: string?`, `durationSeconds: int`, `transcript: string?`, `isPreview: bool` |
| **UpdateVideoDto** | Request | `title: string`, `transcript: string?`, `isPreview: bool` |
| **VideoResponseDto** | Response | `id: guid`, `title: string`, `videoUrl: string?`, `provider: VideoProvider`, `durationSeconds: int`, `quality: VideoQuality`, `status: VideoStatus`, `isPreview: bool`, `viewCount: int`, `transcript: string?`, `createdAt: datetime` |

### 3.8 Document DTOs

| DTO | Type | Properties |
|-----|------|-----------|
| **CreateDocumentDto** | Request | `sectionId: guid`, `title: string`, `description: string?`, `fileId: guid`, `isDownloadable: bool` |
| **UpdateDocumentDto** | Request | `title: string`, `description: string?`, `isDownloadable: bool` |
| **DocumentResponseDto** | Response | `id: guid`, `title: string`, `description: string?`, `fileUrl: string?`, `fileType: string?`, `fileSizeBytes: long?`, `downloadCount: int`, `isDownloadable: bool`, `createdAt: datetime` |

### 3.9 Quiz DTOs

| DTO | Type | Properties |
|-----|------|-----------|
| **CreateQuizDto** | Request | `sectionId: guid`, `title: string`, `description: string?`, `durationMinutes: int?`, `passingScorePercent: int`, `maxAttempts: int?`, `shuffleQuestions: bool`, `shuffleOptions: bool`, `showResultsImmediately: bool`, `allowReview: bool`, `availableFrom: datetime?`, `availableUntil: datetime?` |
| **UpdateQuizDto** | Request | Same as CreateQuizDto (all fields) |
| **QuizResponseDto** | Response | `id: guid`, `title: string`, `description: string?`, `durationMinutes: int?`, `passingScorePercent: int`, `maxAttempts: int?`, `shuffleQuestions: bool`, `shuffleOptions: bool`, `showResultsImmediately: bool`, `allowReview: bool`, `totalPoints: int`, `questionCount: int`, `createdAt: datetime`, `questions: QuestionResponseDto[]` |
| **CreateQuestionDto** | Request | `questionText: string`, `type: QuestionType`, `points: int`, `explanation: string?`, `position: int`, `options: CreateOptionDto[]` |
| **CreateOptionDto** | Request | `optionText: string`, `isCorrect: bool`, `position: int` |
| **QuestionResponseDto** | Response | `id: guid`, `questionText: string`, `type: QuestionType`, `points: int`, `explanation: string?`, `position: int`, `options: OptionResponseDto[]` |
| **OptionResponseDto** | Response | `id: guid`, `optionText: string`, `isCorrect: bool?` (nullable — hidden for students), `position: int` |
| **SubmitAnswerDto** | Request | `questionId: guid`, `selectedOptionId: guid?`, `answerText: string?` |
| **SubmitAttemptDto** | Request | `answers: SubmitAnswerDto[]` |
| **QuizAttemptResponseDto** | Response | `id: guid`, `enrollmentId: guid`, `quizId: guid`, `attemptNumber: int`, `startedAt: datetime`, `submittedAt: datetime?`, `scorePercentage: decimal`, `isPassed: bool`, `status: string` |
| **QuizResultDto** | Response (extends QuizAttemptResponseDto) | `+ isAutoSubmitted: bool`, `answers: AnswerResultDto[]` |
| **AnswerResultDto** | Response | `questionId: guid`, `selectedOptionId: guid?`, `answerText: string?`, `isCorrect: bool`, `earnedPoints: int` |

### 3.10 Enrollment DTOs

| DTO | Type | Properties |
|-----|------|-----------|
| **CreateEnrollmentDto** | Request | `courseId: guid`, `source: EnrollmentSource` (note: `userId` auto-injected from token) |
| **EnrollmentResponseDto** | Response | `id: guid`, `userId: guid`, `courseId: guid`, `courseTitle: string`, `enrolledAt: datetime`, `status: EnrollmentStatus`, `progressPercentage: decimal`, `completedAt: datetime?`, `lastAccessedAt: datetime?`, `source: EnrollmentSource`, `accessExpiresAt: datetime?`, `isRefunded: bool` |
| **EnrollmentDetailDto** | Response | Same as EnrollmentResponseDto + `progresses: ContentProgressDto[]` |
| **ContentProgressDto** | Response | `id: guid`, `enrollmentId: guid`, `contentType: ContentType`, `contentId: guid`, `isCompleted: bool`, `watchTimeSeconds: int`, `attemptsCount: int`, `completionPercentage: decimal`, `metadata: string?`, `lastAccessedAt: datetime?`, `completedAt: datetime?` |
| **UpdateProgressDto** | Request | `watchTimeSeconds: int`, `completionPercentage: decimal?`, `metadata: string?`, `markAsCompleted: bool` |

### 3.11 Public Course DTOs

| DTO | Type | Properties |
|-----|------|-----------|
| **PublicCourseDto** | Response | `id: guid`, `title: string`, `slug: string`, `description: string?`, `courseImageUrl: string?`, `price: decimal`, `isFree: bool` (computed), `level: CourseLevel`, `language: CourseLanguage`, `categoryName: string`, `categoryId: guid`, `instructorName: string`, `averageRating: decimal`, `enrollmentCount: int`, `totalDurationMinutes: int`, `sectionCount: int`, `lessonCount: int`, `publishedAt: datetime?` |
| **PublicCourseDetailDto** | Response | `id: guid`, `title: string`, `slug: string`, `description: string?`, `courseImageUrl: string?`, `introVideoUrl: string?`, `price: decimal`, `isFree: bool` (computed), `level: CourseLevel`, `language: CourseLanguage`, `categoryName: string`, `categoryId: guid`, `instructor: PublicInstructorDto`, `averageRating: decimal`, `enrollmentCount: int`, `totalDurationMinutes: int`, `requirements: string[]`, `learningOutcomes: string[]`, `sections: PublicSectionDto[]`, `version: int`, `publishedAt: datetime?`, `lastContentUpdateAt: datetime?` |
| **PublicInstructorDto** | Response | `id: guid`, `fullName: string`, `bio: string?`, `profileImageUrl: string?` |
| **PublicSectionDto** | Response | `id: guid`, `title: string`, `description: string?`, `position: int`, `items: PublicSectionItemDto[]` |
| **PublicSectionItemDto** | Response | `id: guid`, `itemType: SectionItemType`, `position: int`, `isPreviewAllowed: bool` |
| **PublicCourseFilterDto** | Request/Query | `searchQuery: string?`, `categoryId: guid?`, `level: CourseLevel?`, `language: CourseLanguage?`, `minPrice: decimal?`, `maxPrice: decimal?`, `isFreeOnly: bool?`, `minRating: decimal?`, `sortBy: PublicCourseSortBy`, `sortDescending: bool`, `page: int`, `pageSize: int` |
| **CourseSuggestionDto** | Response | `id: guid`, `title: string`, `slug: string`, `categoryName: string` |
| **PlatformStatsDto** | Response | `totalCourses: int`, `totalStudents: int`, `totalInstructors: int`, `totalCategories: int` |
| **FilterOptionsDto** | Response | `categories: FilterOptionItem[]`, `levels: CourseLevel[]`, `languages: CourseLanguage[]`, `minPrice: decimal`, `maxPrice: decimal` |
| **FilterOptionItem** | Response | `id: guid`, `name: string`, `courseCount: int` |

### 3.12 Edit Request DTOs

| DTO | Type | Properties |
|-----|------|-----------|
| **EditResultDto** | Response | `appliedImmediately: bool`, `requestId: guid?`, `status: EditRequestStatus?`, `message: string`, `processedAt: datetime` |
| **EditRequestSummaryDto** | Response | `id: guid`, `courseTitle: string`, `instructorName: string`, `requestType: EditRequestType`, `operation: EditOperation`, `status: EditRequestStatus`, `requestedAt: datetime`, `timeUntilExpiry: timespan`, `isEmergency: bool` |
| **EditRequestDetailDto** | Response | `requestId: guid`, `courseId: guid`, `courseTitle: string`, `instructorId: guid`, `instructorName: string`, `targetType: EditRequestType`, `operation: EditOperation`, `changes: FieldChangeDto[]`, `requestedAt: datetime`, `expiresAt: datetime?`, `riskLevel: EditRiskLevel` |
| **FieldChangeDto** | Response | `fieldName: string`, `fieldLabel: string`, `oldValue: object?`, `newValue: object?`, `changeType: ChangeType` |
| **EditRequestFilterDto** | Request/Query | `status: EditRequestStatus?`, `requestType: EditRequestType?`, `courseId: guid?`, `instructorId: guid?`, `page: int`, `pageSize: int` |
| **ReviewRequestDto** | Request | `approve: bool`, `notes: string?` |
| **RejectCourseRequest** | Request | `reason: string` |

### 3.13 Review DTOs

| DTO | Type | Properties |
|-----|------|-----------|
| **CreateReviewRequest** | Request | `courseId: guid`, `rating: int`, `comment: string?` |
| **ReviewResponse** | Response | `id: guid`, `userId: guid`, `userFullName: string`, `courseId: guid`, `courseTitle: string`, `rating: int`, `comment: string?`, `status: string`, `isVerified: bool`, `helpfulCount: int`, `notHelpfulCount: int`, `isFlagged: bool`, `createdAt: datetime`, `updatedAt: datetime?` |
| **ReviewDetailResponse** | Response (extends ReviewResponse) | `+ moderatedAt: datetime?`, `moderatedBy: guid?`, `moderatorName: string?`, `deletedAt: datetime?`, `flaggedBy: guid?`, `flaggedAt: datetime?` |
| **ReviewHelpfulRequest** | Request | `isHelpful: bool` |
| **ModerateReviewRequest** | Request | `status: string` |

### 3.14 Wishlist DTOs

| DTO | Type | Properties |
|-----|------|-----------|
| **WishlistItemDto** | Response | `id: guid`, `courseId: guid`, `courseTitle: string`, `courseImageUrl: string?`, `instructorName: string?`, `price: decimal`, `addedAt: datetime` |
| **WishlistResponseDto** | Response | `items: WishlistItemDto[]`, `count: int` |

### 3.15 Video Comment DTOs

| DTO | Type | Properties |
|-----|------|-----------|
| **CreateCommentDto** | Request | `content: string`, `parentCommentId: guid?` |
| **CommentResponseDto** | Response | `id: guid`, `videoId: guid`, `userId: guid`, `userName: string`, `content: string`, `isEdited: bool`, `likesCount: int`, `repliesCount: int`, `createdAt: datetime`, `updatedAt: datetime?`, `parentCommentId: guid?`, `replies: CommentResponseDto[]?` |
| **CommentLikeResponseDto** | Response | `commentId: guid`, `likesCount: int`, `isLikedByCurrentUser: bool` |

### 3.16 Notification DTOs

| DTO | Type | Properties |
|-----|------|-----------|
| **NotificationDto** | Response | `id: guid`, `title: string`, `message: string?`, `type: NotificationType`, `linkUrl: string?`, `icon: string?`, `isRead: bool`, `createdAt: datetime`, `readAt: datetime?` |
| **NotificationListDto** | Response | `notifications: NotificationDto[]`, `totalCount: int`, `page: int`, `pageSize: int`, `totalPages: int` |
| **CreateNotificationDto** | Request (internal) | `userId: guid`, `title: string`, `message: string?`, `type: NotificationType`, `linkUrl: string?`, `icon: string?` |

### 3.17 Live Session DTOs

| DTO | Type | Properties |
|-----|------|-----------|
| **CreateLiveSessionDto** | Request | `sectionId: guid`, `title: string`, `description: string?`, `scheduledStart: datetime`, `scheduledEnd: datetime`, `meetingUrl: string`, `password: string?`, `maxAttendees: int?` |
| **UpdateLiveSessionStatusDto** | Request | `status: LiveSessionStatus`, `meetingUrl: string?`, `password: string?`, `maxAttendees: int?` |
| **LiveSessionResponseDto** | Response | `id: guid`, `title: string`, `description: string?`, `scheduledStart: datetime`, `scheduledEnd: datetime`, `status: LiveSessionStatus`, `meetingUrl: string`, `password: string?`, `maxAttendees: int?`, `actualStartAt: datetime?`, `actualEndAt: datetime?`, `recordingFileId: guid?`, `currentAttendeesCount: int` |

### 3.18 Certificate DTOs

| DTO | Type | Properties |
|-----|------|-----------|
| **CertificateResponse** | Response | `id: guid`, `userId: guid`, `userFullName: string`, `courseId: guid`, `courseTitle: string`, `verificationCode: string`, `status: string`, `issuedAt: datetime`, `completedAt: datetime`, `certificateFileId: guid?`, `revokedAt: datetime?` |
| **CertificateVerificationResponse** | Response | `isValid: bool`, `fullName: string`, `courseTitle: string`, `issuedAt: datetime`, `completedAt: datetime`, `verificationCode: string`, `status: string` |
| **IssueCertificateRequest** | Request | `userId: guid`, `courseId: guid`, `enrollmentId: guid` |
| **RevokeCertificateRequest** | Request | `reason: string?` |

### 3.19 Instructor Request DTOs

| DTO | Type | Properties |
|-----|------|-----------|
| **SubmitInstructorRequestDto** | Request | `message: string`, `documents: InstructorRequestDocumentDto[]` |
| **UpdateInstructorRequestDto** | Request | `message: string`, `documents: InstructorRequestDocumentDto[]` |
| **AddDocumentToRequestDto** | Request | `documentType: DocumentType`, `fileId: guid?`, `urlValue: string?` |
| **ProcessInstructorRequestDto** | Request | `status: InstructorRequestStatus`, `adminNotes: string?`, `rejectionReason: string?` |
| **InstructorRequestDocumentDto** | Request | `documentType: DocumentType`, `fileId: guid?`, `urlValue: string?` |
| **InstructorRequestDto** | Response | `id: guid`, `userName: string`, `userEmail: string`, `status: InstructorRequestStatus`, `message: string?`, `submittedAt: datetime`, `processedAt: datetime?`, `processedByUserName: string?`, `documentsCount: int` |
| **InstructorRequestDetailDto** | Response (extends InstructorRequestDto) | `+ documents: InstructorRequestDocumentDto[]`, `adminNotes: string?`, `rejectionReason: string?` |
| **MyInstructorRequestsDto** | Response | `requests: InstructorRequestDto[]`, `totalCount: int`, `canSubmitNewRequest: bool` |

### 3.20 Commerce DTOs

| DTO | Type | Properties |
|-----|------|-----------|
| **CartResponseDto** | Response | `id: guid`, `items: CartItemDto[]`, `subtotal: decimal`, `couponCode: string?`, `discountAmount: decimal`, `finalAmount: decimal` |
| **CartItemDto** | Response | `id: guid`, `courseId: guid`, `courseTitle: string`, `courseImageUrl: string?`, `instructorName: string?`, `priceSnapshot: decimal`, `currentPrice: decimal`, `addedAt: datetime` |
| **AddToCartRequest** | Request | `courseId: guid` |
| **ApplyCouponRequest** | Request | `code: string` |
| **ApplyCouponResponse** | Response | `code: string`, `discountAmount: decimal`, `finalAmount: decimal`, `message: string?` |
| **CreateOrderRequest** | Request | `couponCode: string?` |
| **OrderResponseDto** | Response | `id: guid`, `orderNumber: string`, `subtotal: decimal`, `discountAmount: decimal`, `finalAmount: decimal`, `status: string`, `couponCode: string?`, `itemCount: int`, `createdAt: datetime` |
| **OrderDetailDto** | Response | `id: guid`, `orderNumber: string`, `subtotal: decimal`, `discountAmount: decimal`, `finalAmount: decimal`, `status: string`, `couponCode: string?`, `items: OrderItemDto[]`, `payments: PaymentHistoryDto[]`, `createdAt: datetime` |
| **OrderItemDto** | Response | `id: guid`, `courseId: guid`, `courseTitle: string`, `priceAtPurchase: decimal` |
| **PaymentHistoryDto** | Response | `id: guid`, `amount: decimal`, `status: string`, `gatewayResponse: string?`, `createdAt: datetime` |
| **ProcessPaymentRequest** | Request | `paymentMethodId: guid` |
| **PaymentResponseDto** | Response | `id: guid`, `orderId: guid`, `amount: decimal`, `status: string`, `gatewayTransactionId: string?`, `gatewayResponse: string?`, `paymentMethodName: string?`, `createdAt: datetime` |
| **PaymentMethodResponse** | Response | `id: guid`, `name: string`, `provider: string`, `type: string`, `isActive: bool`, `configuration: string?` |
| **CreatePaymentMethodRequest** | Request | `name: string`, `provider: string`, `type: string`, `configuration: string?` |
| **CreateCouponDto** | Request | `code: string`, `type: string`, `value: decimal`, `maxDiscountAmount: decimal?`, `minimumPurchaseAmount: decimal?`, `applicableTo: string`, `courseIds: guid[]?`, `usageLimit: int?`, `userLimitPerUser: int?`, `isPublic: bool`, `validFrom: datetime?`, `validUntil: datetime?` |
| **CouponResponseDto** | Response | `id: guid`, `code: string`, `type: string`, `value: decimal`, `maxDiscountAmount: decimal?`, `minimumPurchaseAmount: decimal?`, `applicableTo: string`, `usageLimit: int?`, `userLimitPerUser: int?`, `timesUsed: int`, `isActive: bool`, `validFrom: datetime?`, `validUntil: datetime?`, `createdAt: datetime` |
| **ValidateCouponRequest** | Request | `code: string`, `cartTotal: decimal`, `courseIds: guid[]` |
| **ValidateCouponResponse** | Response | `code: string`, `isValid: bool`, `discountAmount: decimal`, `finalAmount: decimal`, `message: string?` |
| **RequestRefundRequest** | Request | `paymentId: guid`, `reason: string?` |
| **ProcessRefundRequest** | Request | `refundId: guid`, `adminNotes: string?` |
| **RefundResponseDto** | Response | `id: guid`, `paymentId: guid`, `amount: decimal`, `reason: string?`, `status: string`, `orderNumber: string?`, `requestedAt: datetime`, `processedAt: datetime?`, `processedByName: string?` |

### 3.21 Communication DTOs

| DTO | Type | Properties |
|-----|------|-----------|
| **SendMessageRequest** | Request | `receiverId: guid`, `content: string` |
| **MessageResponse** | Response | `id: guid`, `senderId: guid`, `receiverId: guid`, `content: string`, `sentAt: datetime`, `isRead: bool`, `readAt: datetime?` |
| **ConversationResponse** | Response | `otherUserId: guid`, `otherUserName: string`, `lastMessage: string`, `lastMessageAt: datetime`, `unreadCount: int` |
| **ConversationListResponse** | Response | `items: ConversationResponse[]`, `page: int`, `pageSize: int`, `totalCount: int` |
| **ConversationMessagesResponse** | Response | `items: MessageResponse[]`, `page: int`, `pageSize: int`, `totalCount: int` |
| **UnreadCountResponse** | Response | `unreadCount: int` |
| **CreateAnnouncementRequest** | Request | `title: string`, `content: string`, `target: string`, `courseId: guid?` |
| **UpdateAnnouncementRequest** | Request | `title: string`, `content: string`, `target: string`, `courseId: guid?`, `isActive: bool` |
| **AnnouncementResponse** | Response | `id: guid`, `title: string`, `content: string`, `target: string`, `courseId: guid?`, `createdBy: guid`, `createdByName: string`, `isActive: bool`, `publishedAt: datetime?` |
| **AnnouncementListResponse** | Response | `items: AnnouncementResponse[]`, `page: int`, `pageSize: int`, `totalCount: int` |
| **CreateReportRequest** | Request | `entityType: string`, `entityId: guid`, `reason: string`, `description: string?` |
| **ResolveReportRequest** | Request | `status: string`, `adminNote: string?` |
| **ReportResponse** | Response | `id: guid`, `reporterId: guid`, `reporterName: string`, `entityType: string`, `entityId: guid`, `reason: string`, `description: string?`, `status: string`, `adminNote: string?`, `createdAt: datetime`, `resolvedAt: datetime?` |
| **ReportListResponse** | Response | `items: ReportResponse[]`, `page: int`, `pageSize: int`, `totalCount: int` |
| **CreateSettingRequest** | Request | `group: string`, `key: string`, `value: string`, `dataType: string`, `description: string?` |
| **UpdateSettingRequest** | Request | `value: string` |
| **SettingResponse** | Response | `id: guid`, `group: string`, `key: string`, `value: string`, `dataType: string`, `description: string?`, `isPublic: bool`, `updatedAt: datetime?`, `updatedBy: guid?` |

### 3.22 Dashboard DTOs

| DTO | Type | Properties |
|-----|------|-----------|
| **DashboardMetricDto** | Response | `label: string`, `value: string`, `change: double?`, `trend: string`, `icon: string?`, `color: string?` |
| **ChartSeriesDto** | Response | `labels: string[]`, `series: SeriesItemDto[]` |
| **SeriesItemDto** | Response | `name: string`, `data: double[]` |
| **DistributionItemDto** | Response | `label: string`, `value: double`, `color: string?`, `percentage: double` |
| **TopCourseDto** | Response | `id: guid`, `title: string`, `instructorName: string?`, `price: decimal`, `enrollmentCount: int`, `averageRating: decimal`, `revenue: decimal` |
| **PendingItemsDto** | Response | `pendingCourses: int`, `pendingEditRequests: int`, `pendingTeacherRequests: int`, `flaggedReviews: int` |
| **StudentOverviewDto** | Response | `metrics: DashboardMetricDto[]`, `recentCourses: StudentCourseDto[]`, `weeklyActivity: ChartSeriesDto`, `recentCertificates: StudentCertificateDto[]` |
| **StudentCourseDto** | Response | `id: guid`, `enrollmentId: guid`, `courseId: guid`, `courseTitle: string`, `thumbnailUrl: string?`, `instructorName: string`, `progressPercentage: double`, `status: string`, `lastAccessedAt: datetime` |
| **StudentCertificateDto** | Response | `id: guid`, `courseId: guid`, `courseTitle: string`, `verificationCode: string`, `issuedAt: datetime`, `status: string` |
| **InstructorOverviewDto** | Response | `metrics: DashboardMetricDto[]`, `courses: InstructorCourseDto[]`, `revenueTrend: ChartSeriesDto`, `enrollmentTrend: ChartSeriesDto`, `studentLevelDistribution: DistributionItemDto[]`, `pendingEditRequests: int` |
| **InstructorCourseDto** | Response | `id: guid`, `title: string`, `slug: string`, `thumbnailUrl: string?`, `price: decimal`, `status: string`, `enrollmentCount: int`, `averageRating: decimal`, `totalDurationMinutes: int`, `revenue: decimal`, `progressPercentage: double`, `createdAt: datetime`, `publishedAt: datetime?` |
| **InstructorDashboardRevenueDto** | Response | `totalRevenue: decimal`, `currentMonthRevenue: decimal`, `previousMonthRevenue: decimal`, `revenueChangePercent: double`, `monthlyBreakdown: ChartSeriesDto` |
| **InstructorDashboardStudentsDto** | Response | `totalStudents: int`, `activeStudents: int`, `newStudentsThisMonth: int`, `enrollmentOverTime: ChartSeriesDto` |
| **AdminOverviewDto** | Response | `metrics: DashboardMetricDto[]`, `revenueTrend: ChartSeriesDto`, `enrollmentTrend: ChartSeriesDto`, `userGrowth: ChartSeriesDto`, `courseDistribution: DistributionItemDto[]`, `topCourses: TopCourseDto[]`, `pendingItems: PendingItemsDto` |
| **AdminRevenueDto** | Response | `totalRevenue: decimal`, `monthlyRevenue: ChartSeriesDto`, `revenueByCourse: DistributionItemDto[]` |
| **AdminUserGrowthDto** | Response | `totalUsers: int`, `growth: ChartSeriesDto`, `roleDistribution: DistributionItemDto[]` |
| **AdminEnrollmentTrendDto** | Response | `trend: ChartSeriesDto`, `statusDistribution: DistributionItemDto[]` |
| **ManagementCourseDto** | Response | `id: guid`, `title: string`, `slug: string`, `description: string?`, `thumbnailUrl: string?`, `price: decimal`, `status: string`, `categoryName: string`, `totalDurationMinutes: int`, `enrollmentCount: int`, `averageRating: decimal`, `revenue: decimal`, `sectionCount: int`, `lessonCount: int`, `createdAt: datetime`, `publishedAt: datetime?`, `updatedAt: datetime?` |
| **ReviewSummaryDto** | Response | `id: guid`, `userId: guid`, `userFullName: string`, `userProfileImageUrl: string?`, `courseId: guid`, `courseTitle: string`, `rating: int`, `comment: string?`, `createdAt: datetime` |
| **PendingEditRequestDto** | Response | `id: guid`, `courseId: guid`, `courseTitle: string`, `requestType: string`, `status: string`, `requestedAt: datetime`, `expiresAt: datetime?`, `isEmergency: bool`, `reviewerNote: string?` |

### 3.23 Activity Log DTOs

| DTO | Type | Properties |
|-----|------|-----------|
| **ActivityLogResponse** | Response | `id: guid`, `userId: guid`, `userName: string`, `action: string`, `entityType: string`, `entityId: guid?`, `details: string?`, `ipAddress: string?`, `createdAt: datetime` |
| **ActivityLogListResponse** | Response | `items: ActivityLogResponse[]`, `page: int`, `pageSize: int`, `totalCount: int` |
| **ActivityLogFilterRequest** | Query | `userId: guid?`, `action: string?`, `entityType: string?`, `dateFrom: datetime?`, `dateTo: datetime?`, `ipAddress: string?`, `page: int`, `pageSize: int` |

### 3.24 Email DTOs

| DTO | Type | Properties |
|-----|------|-----------|
| **EmailMessageDto** | Internal | `toEmails: string[]`, `ccEmails: string[]`, `bccEmails: string[]`, `subject: string`, `body: string`, `isHtml: bool`, `attachments: EmailAttachmentDto[]` |
| **EmailTemplateDto** | Internal | `templateName: string`, `parameters: Dictionary<string,object>` |
| **EmailAttachmentDto** | Internal | `fileName: string`, `content: byte[]`, `contentType: string` |

### 3.25 Common Response Wrappers

| Type | Properties |
|------|-----------|
| **ApiResponse\<T\>** | `success: bool`, `data: T?`, `message: string?`, `errors: string[]?` |
| **PagedList\<T\>** | `items: T[]`, `page: int`, `pageSize: int`, `totalCount: int`, `totalPages: int` (computed), `hasPrevious: bool` (computed), `hasNext: bool` (computed) |

---

## 4. Enums

| Enum | Values |
|------|--------|
| **Gender** | `Male`, `Female`, `Other`, `PreferNotToSay` |
| **CourseLevel** | `Beginner`, `Intermediate`, `Advanced` |
| **CourseLanguage** | `Ar` (Arabic), `En` (English) |
| **CourseStatus** | `Draft`, `PendingReview`, `Published`, `Archived` |
| **ContentType** | `Video`, `Quiz`, `Document`, `LiveSession` |
| **SectionItemType** | `Video`, `Quiz`, `Document`, `LiveSession` |
| **EnrollmentStatus** | `InProgress`, `Completed`, `Expired`, `Refunded` |
| **EnrollmentSource** | `Purchase`, `Gift`, `AdminGrant`, `Coupon` |
| **VideoStatus** | `Processing`, `Ready`, `Failed` |
| **VideoProvider** | `Local`, `YouTube`, `Vimeo`, `Minio` |
| **VideoQuality** | `_720p`, `_1080p`, `_4k` |
| **QuestionType** | `MultipleChoice`, `TrueFalse`, `ShortAnswer` |
| **QuizAttemptStatus** | `InProgress`, `Submitted`, `Graded` |
| **LiveSessionStatus** | `Scheduled`, `Live`, `Finished`, `Cancelled` |
| **PhoneType** | `Primary`, `Secondary` |
| **StoredFileType** | `Image`, `Video`, `Document`, `Recording`, `Certificate` |
| **FileVisibility** | `Public`, `EnrolledOnly`, `Private` |
| **FileStatus** | `Uploading`, `Ready`, `Failed`, `Deleted` |
| **NotificationType** | `Course`, `Payment`, `System`, `Message`, `InstructorRequest`, `Enrollment`, `Assignment` |
| **InstructorRequestStatus** | `Pending`, `Approved`, `Rejected`, `RequiresMoreInfo` |
| **DocumentType** | `CV`, `Certificate`, `IDCard`, `Degree`, `PortfolioLink`, `Transcript`, `Other` |
| **CertificateStatus** | `Valid`, `Revoked` |
| **ReviewStatus** | `Pending`, `Approved`, `Rejected` |
| **OrderStatus** | `Pending`, `Completed`, `Failed`, `Refunded` |
| **PaymentStatus** | `Succeeded`, `Pending`, `Failed` |
| **PaymentMethodType** | `CreditCard`, `DigitalWallet`, `BankTransfer` |
| **RefundStatus** | `Requested`, `Approved`, `Rejected`, `Processed` |
| **CouponType** | `Percentage`, `Fixed` |
| **CouponApplicableTo** | `All`, `SpecificCourses`, `Category` |
| **PublicCourseSortBy** | `PublishedAt`, `Price`, `AverageRating`, `EnrollmentCount`, `Title` |
| **EditRequestStatus** | `Pending=0`, `Approved=1`, `Rejected=2`, `Cancelled=3`, `Expired=4` |
| **EditRequestType** | `Section=0`, `SectionItem=1`, `CourseProperty=2` |
| **EditOperation** | `Create=0`, `Update=1`, `Delete=2` |
| **EditRiskLevel** | `Low`, `Medium`, `High`, `Critical` |
| **ChangeType** | `Added`, `Modified`, `Deleted` |
| **AnnouncementTarget** | `All`, `Students`, `Instructors`, `Admins`, `SpecificCourse` |
| **CourseLogAction** | `Created`, `Updated`, `Published`, `Archived`, `ContentModified`, `StatusChanged` |

---

## 5. SignalR Hubs

### 5.1 Notification Hub (`/hubs/notifications`)
- **Auth**: Bearer token (JWT)
- **Connection**: On connect, user is added to a group named by their userId (`uid` claim)
- **Events received from server**:
  - The server pushes `NotificationDto` objects to the user's group
- **Events sent to server**: None (read-only hub for receiving)

### 5.2 Message Hub (`/hubs/messaging`)
- **Auth**: Bearer token (JWT)
- **Connection**: On connect, user is added to a group named by their user identifier (`Context.UserIdentifier`)
- **Events received from server**:
  - Real-time message delivery: `MessageResponse` objects pushed to receiver's group
- **Events sent to server**: None (read-only hub for receiving)

### Important SignalR Notes
- Both hubs use `[Authorize]`, so the connection must include an access token
- For the JavaScript client, pass the token as `access_token` in the query string or in the `Authorization` header:
  ```js
  const connection = new HubConnectionBuilder()
    .withUrl("/hubs/notifications", { accessTokenFactory: () => token })
    .build();
  ```

---

## 6. Key Behaviors

### 6.1 Exception → HTTP Status Code Mapping (via ExceptionMiddleware)

| Exception Type | HTTP Status |
|---------------|------------|
| `KeyNotFoundException` | 404 Not Found |
| `UnauthorizedAccessException` | 403 Forbidden |
| `InvalidOperationException` | 400 Bad Request |
| Any other exception | 500 Internal Server Error |

All exceptions return: `{ "success": false, "data": null, "message": "error message", "errors": null }`

### 6.2 Rate Limiting (Fixed Window, configured in Program.cs)

| Policy | Requests | Window |
|--------|----------|--------|
| `Auth` | 10 | 1 minute |
| `Strict` | 5 | 1 minute |
| `Messaging` | 30 | 1 minute |

Rate limited endpoints return HTTP 429 Too Many Requests.

Controllers using rate limiting:
- `AuthController` — `[EnableRateLimiting("Auth")]`
- `MessagesController` — `[EnableRateLimiting("Messaging")]`

### 6.3 CORS Setup

- Reads `CorsSettings:AllowedOrigins` from appsettings.json
- If empty/not configured, allows requests from any `localhost`, `192.168.*`, `10.*`, `172.*` origin
- Allows all methods, all headers, and credentials (`AllowCredentials()`)

### 6.4 Health Check Endpoints

| Endpoint | What it checks |
|----------|---------------|
| `GET /health` | All registered checks (liveness + database) |
| `GET /healthz` | Liveness only (app is running) |
| `GET /ready` | Readiness (database can connect) |

The `DatabaseHealthCheck` pings the database via `DbContext.Database.CanConnectAsync()`.

### 6.5 Pagination Format

Most list endpoints accept `page` and `pageSize` query parameters with defaults of `page=1` and `pageSize=20` (or `50` for activity logs).

The `PublicCourseController` returns `PagedList<T>`:
```json
{
  "success": true,
  "data": {
    "items": [...],
    "page": 1,
    "pageSize": 12,
    "totalCount": 150,
    "totalPages": 13,
    "hasPrevious": false,
    "hasNext": true
  }
}
```

Other controllers (Notifications, Messages, Announcements, Reports, ActivityLogs) return their own pagination response shape with `items`, `page`, `pageSize`, `totalCount`.

### 6.6 Security Headers (via SecurityHeadersMiddleware)

All responses include:
- `X-Content-Type-Options: nosniff`
- `X-Frame-Options: DENY`
- `Referrer-Policy: strict-origin-when-cross-origin`
- `Permissions-Policy: camera=(), microphone=(), geolocation=(), interest-cohort=()`
- `X-Permitted-Cross-Domain-Policies: none`

### 6.7 JWT Configuration

| Setting | Value |
|---------|-------|
| Secret Key | Configured in appsettings `JwtSettings:SecretKey` (min 32 chars) |
| Issuer | `JwtSettings:Issuer` (default "Athary") |
| Audience | `JwtSettings:Audience` (default "AtharyClient") |
| Access Token Expiry | `JwtSettings:AccessTokenExpirationMinutes` (default 60 min) |
| Refresh Token Expiry | `JwtSettings:RefreshTokenExpirationDays` (default 7 days) |
| Clock Skew | 0 seconds |
| Token Validation | Issuer, Audience, Lifetime, SigningKey all validated |

### 6.8 Identity Password Policy

- Require digit: yes
- Require lowercase: yes
- Require uppercase: yes
- Require non-alphanumeric: yes
- Minimum length: 8
- Lockout: 5 failed attempts → 15 min lockout
- Unique email required

---

## 7. File Uploads

### Upload Flow (S3/MinIO Presigned URLs)

The system uses presigned URLs — do NOT upload files directly to the API. Instead:

**Step 1**: Request an upload URL
```
POST /api/media/upload-url
Authorization: Bearer <token>
Body: { "fileType": "Image", "fileName": "photo.jpg", "contentType": "image/jpeg", "fileSizeBytes": 500000, "visibility": "Public" }
```

Response includes:
```json
{
  "fileId": "guid",
  "uploadUrl": "https://minio.example.com/...",
  "objectKey": "images/uuid-filename.jpg",
  "bucket": "images",
  "expiresAt": "2026-01-01T00:00:00Z",
  "requiredHeaders": { "x-amz-acl": "public-read" }
}
```

**Step 2**: Upload directly to the presigned URL (client-side, using `PUT` method)
```
PUT <uploadUrl>
Headers: { "Content-Type": "image/jpeg", ...requiredHeaders }
Body: <binary file data>
```

**Step 3**: Confirm upload to the API
```
POST /api/media/confirm-upload
Authorization: Bearer <token>
Body: { "fileId": "guid", "objectKey": "images/uuid-filename.jpg", "bucket": "images" }
```

Response: `ApiResponse<MediaFileDto>` with file metadata

### Getting a View URL (for display)
```
GET /api/media/{fileId}/view-url
```

Response includes a time-limited `viewUrl` that can be used directly in `<img>`, `<video>`, or `<a>` tags.

### Setting Images (Profile, Course, Category)
All image-setting endpoints accept `{ "fileId": "guid" }` where `fileId` comes from the confirmed upload flow above.

### MinIO Buckets (configured in appsettings)
| Bucket | Purpose |
|--------|---------|
| `private` | Private files |
| `images` | Course images, profile pictures, category images |
| `videos` | Course video content |
| `documents` | Course documents (PDFs, etc.) |
| `recordings` | Live session recordings |
| `certificates` | Generated certificate PDFs |

Presigned URLs expire after `PresignedUrlExpiryMinutes` (default 60 min).
