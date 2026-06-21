# Migration Gap Analysis: `backend_project` → `New/`

## Summary

| Area | Old (`backend_project`) | New (`New/`) | Gap |
|---|---|---|---|
| Controllers | ~41 | 8 | **33 missing** |
| Service interfaces | ~55 | 21 | **34 missing** |
| Service implementations | ~60+ | 20 | **40+ missing** |
| Background workers | 3 (VideoProcessing, ScheduledDeletion, EditRequestCleanup) | 0 | **3 missing** |
| Custom middleware | 1 (ExceptionMiddleware) | 0 | **1 missing** |
| SignalR Hubs | 2 (NotificationHub, MessageHub) | 1 (NotificationHub) | **1 missing** |
| Authorization | 2 (HasPermissionAttribute, TeacherRequestPolicies) | 0 | **2 missing** |
| Helpers | 2 (EnrollmentGuard, EditPolicyHelper) | 0 | **2 missing** |

## Already Migrated (✓)

### Services
| Service | Interface | Notes |
|---|---|---|
| RegistrationService | IRegistrationService | ✓ |
| LoginService | ILoginService | ✓ |
| EmailService | IEmailService | ✓ |
| OAuthService | IOAuthService | ✓ |
| TokenService | ITokenService | ✓ |
| SessionService | ISessionService | ✓ |
| VerificationService | IVerificationService | ✓ |
| ActivityLogService | IActivityLogService | ✓ (dead overload removed) |
| ProfileService | IProfileService | ✓ (refactored to repo pattern) |
| PermissionService | IPermissionService | ✓ |
| MediaService | IMediaService | ✓ |
| AdminMediaService | IAdminMediaService | ✓ |
| VideoProcessingService | IVideoProcessingService | ✓ |
| NotificationService | INotificationService | ✓ |

### Entities in Domain (all transferred, but with some TODOs)
- `Course` has 2 TODO comments for `UploadedFile` navigation properties (CourseImageFile, IntroVideoFile) — these are present in old model
- `UserRole` matches (both have AssignedAt/ExpiresAt)
- `VerificationToken` in new is actually **better** (uses TokenHash, UsedAt, extends BaseEntity)

---

## NOT Yet Migrated: Services

### Course Domain (~10 services)
| Old Interface | Old Implementation | Priority |
|---|---|---|
| ICourseService | CourseService (CRUD, publish, submit, approve, reject) | HIGH |
| ICourseEditApprovalService | CourseEditApprovalService | HIGH |
| ISectionService | SectionService (CRUD, reorder) | HIGH |
| IVideoContentService | VideoContentService | HIGH |
| IDocumentService | DocumentService | HIGH |
| IQuizManagementService | QuizManagementService (Quiz+Question CRUD) | HIGH |
| IQuizAttemptService | QuizAttemptService (start, submit, results) | HIGH |
| IContentProgressService | ContentProgressService | HIGH |
| IEnrollmentService | EnrollmentService (enroll, unenroll, complete) | HIGH |
| IPublicCourseService | PublicCourseService | HIGH |

### Live Session (~2 services)
| Old Interface | Old Implementation | Priority |
|---|---|---|
| ILiveSessionService | LiveSessionService | MEDIUM |
| ILiveAttendanceService | LiveAttendanceService | MEDIUM |

### Commerce (~6 services)
| Old Interface | Old Implementation | Priority |
|---|---|---|
| ICartService | CartService | HIGH |
| IWishlistService | WishlistService | MEDIUM |
| ICouponService | CouponService | HIGH |
| IOrderService | OrderService | HIGH |
| IPaymentService | PaymentService + MockPaymentGateway | HIGH |
| IRefundService | RefundService | MEDIUM |

### Reviews & Certificates (~2 services)
| Old Interface | Old Implementation | Priority |
|---|---|---|
| IReviewService | ReviewService | MEDIUM |
| ICertificateService | CertificateService | MEDIUM |

### Category (~1 service)
| Old Interface | Old Implementation | Priority |
|---|---|---|
| ICategoryService | CategoryService | HIGH |

### Dashboard (~3 services)
| Old Interface | Old Implementation | Priority |
|---|---|---|
| IAdminDashboardService | AdminDashboardService | LOW |
| IInstructorDashboardService | InstructorDashboardService | LOW |
| IStudentDashboardService | StudentDashboardService | LOW |

### Communication (~4 services)
| Old Interface | Old Implementation | Priority |
|---|---|---|
| IMessageService | MessageService | MEDIUM |
| IAnnouncementService | AnnouncementService | MEDIUM |
| IReportService | ReportService | MEDIUM |
| ISystemSettingService | SystemSettingService | MEDIUM |

### Other (~1 service)
| Old Interface | Old Implementation | Priority |
|---|---|---|
| ITeacherRequestService | TeacherRequestService | MEDIUM |

---

## NOT Yet Migrated: Background Workers

| Worker | File | What it does | Priority |
|---|---|---|---|
| VideoProcessingWorker | `Workers/VideoProcessingWorker.cs` | Processes uploaded video files asynchronously — encoding, thumbnail generation | HIGH |
| ScheduledDeletionService | `services/Background/ScheduledDeletionService.cs` | Handles soft-delete cleanup, permanent deletion after delay | MEDIUM |
| EditRequestCleanupService | `services/Background/EditRequestCleanupService.cs` | Cleans up expired course edit requests | LOW |

## NOT Yet Migrated: Middleware

| Middleware | File | What it does | Priority |
|---|---|---|---|
| ExceptionMiddleware | `Middlewares/ExceptionMiddleware.cs` | Global error handling, structured error responses, logging | HIGH |

Note: The old project does NOT have RequestLogging or AndroidAppChecker middleware (my earlier scan was incorrect).

## NOT Yet Migrated: Hubs

| Hub | File | What it does | Priority |
|---|---|---|---|
| MessageHub | `Hubs/MessageHub.cs` | Real-time messaging between users | MEDIUM |

## NOT Yet Migrated: Authorization

| File | What it does | Priority |
|---|---|---|
| `Authorization/HasPermissionAttribute.cs` | Attribute-based permission check filter | MEDIUM |
| `Authorization/TeacherRequestPolicies.cs` | Authorization policies for teacher requests | LOW |

## NOT Yet Migrated: Helpers

| File | What it does | Priority |
|---|---|---|
| `Helpers/EnrollmentGuard.cs` | Guards against unauthorized enrollment access | MEDIUM |
| `Helpers/EditPolicyHelper.cs` | Policy evaluation for course edits | LOW |

---

## Controllers Not Yet Created

The new project has 7 controllers + 1 hub endpoint. The old project has **~41 controllers**.

### Already migrated (7 + 1 hub)
- AuthController → ✓ (in new)
- OAuthController → ✓ (in new)
- ProfileController → ✓ (in new)
- MediaController → ✓ (in new)
- AdminMediaController → ✓ (in new)
- NotificationsController → ✓ (in new)
- ActivityLogsController → ✓ (in new)
- NotificationHub → ✓ (in new)

### Not yet migrated (33 controllers)

**Course Management (7):**
- CourseManagementController, PublicCourseController, ManagementCoursesController
- SectionController, VideoContentController, DocumentController
- AdminCourseController

**Quiz (2):** QuizManagementController, QuizAttemptController

**Commerce (5):** CartController, CouponController, OrderController, RefundController, PaymentMethodsController

**Live Sessions (2):** LiveSessionController, LiveAttendanceController

**Enrollment (2):** EnrollmentsController, CertificatesController

**Reviews (1):** ReviewsController

**Category (1):** CategoryController

**Communication (3):** AnnouncementsController, MessagesController, ReportsController

**Settings (1):** SystemSettingsController

**Teacher (1):** TeacherRequestController

**Dashboard (3):** DashboardController (student/instructor/admin)

**Other (2):** VideoCommentController, WishlistController

**Admin (3):** AdminCertificatesController, AdminCouponController, AdminRefundController, AdminDashboardController

---

## Other Issues Found

1. **UploadedFile entity missing** — `Course.cs` has TODO comments for `UploadedFile` navigation properties that don't exist yet
2. **Integration tests are empty shells** — `API.IntegrationTests` and `Infrastructure.Tests` projects exist but have 0 test methods
3. **No `.sln` file** in `New/` — projects build individually rather than through a solution
4. **`VideoComment` entity** exists in old but not listed in new query — need to check if it exists in new
