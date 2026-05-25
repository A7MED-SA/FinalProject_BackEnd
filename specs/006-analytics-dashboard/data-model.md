# Data Model: Analytics Dashboard

No new database entities are required. All dashboard data is computed via aggregation queries over existing models. The "models" below are response DTOs only.

## DTO: StudentDashboardDto

| Field | Type | Source | Notes |
|-------|------|--------|-------|
| TotalEnrolledCourses | int | `Enrollments.Count(e => e.UserId == userId)` | Includes all statuses except Refunded |
| InProgressCourses | int | `Enrollments.Count(e => e.Status == InProgress)` | |
| CompletedCourses | int | `Enrollments.Count(e => e.Status == Completed)` | |
| TotalLearningHours | decimal | Sum of video `WatchTimeSeconds / 3600` + estimated quiz/doc hours | Video: real watch time; Quiz: 30min per quiz; Doc: 15min per document |
| CertificatesEarned | int | `Enrollments.Count(e => e.CertificateId != null)` | |
| RecentEnrollments | List<EnrollmentBriefDto> | Last 5 enrollments by `LastAccessedAt` | |
| CertificateEligibleCourses | List<EnrollmentBriefDto> | Completed enrollments without a certificate yet | |

## DTO: EnrollmentBriefDto

| Field | Type | Source |
|-------|------|--------|
| EnrollmentId | Guid | `Enrollment.Id` |
| CourseId | Guid | `Enrollment.CourseId` |
| CourseTitle | string | `Course.Title` |
| ProgressPercentage | decimal | `Enrollment.ProgressPercentage` |
| Status | string | `Enrollment.Status.ToString()` |
| LastAccessedAt | DateTime? | `Enrollment.LastAccessedAt` |
| CertificateId | Guid? | `Enrollment.CertificateId` |

## DTO: InstructorDashboardDto

| Field | Type | Source |
|-------|------|--------|
| TotalStudents | int | Distinct users across instructor's enrollments |
| PublishedCourses | int | `Courses.Count(c => c.CreatedBy == userId && c.IsPublished)` |
| TotalTeachingHours | decimal | Sum of `Course.TotalDurationMinutes / 60` across all courses |
| AverageRating | decimal | Average of course ratings (from Reviews table) |
| TotalRevenue | decimal | Net revenue (Gross × RevenueSharePercentage / 100) |
| GrossRevenue | decimal | Sum of Order amounts for instructor's courses (excl. refunds) |
| Courses | List<ManagementCourseDto> | Instructor's courses with stats |
| PendingEditRequests | List<EditRequestSummaryDto> | Pending course edit requests |

## DTO: ManagementCourseDto

| Field | Type | Source |
|-------|------|--------|
| Id | Guid | `Course.Id` |
| Title | string | `Course.Title` |
| Status | string | `Course.Status.ToString()` |
| IsPublished | bool | `Course.IsPublished` |
| EnrollmentCount | int | `Enrollments.Count(e => e.CourseId == courseId)` |
| AverageRating | decimal | Average of `Review.Rating` for this course |
| Price | decimal | `Course.Price` |
| CreatedAt | DateTime | `Course.CreatedAt` |

## DTO: AdminOverviewDto

| Field | Type | Source |
|-------|------|--------|
| TotalUsers | int | `Users.Count()` |
| TotalCourses | int | `Courses.Count()` |
| TotalRevenue | decimal | Sum of completed Order totals minus refunds |
| PendingCourseApprovals | int | `Courses.Count(c => c.Status == PendingReview)` |
| PendingTeacherRequests | int | `TeacherRequests.Count(r => r.Status == Pending)` |
| ActiveInstructors | int | Instructors with at least one published course |

## DTO: MonthlyRevenueDto

| Field | Type | Source |
|-------|------|--------|
| Month | string | "YYYY-MM" format |
| GrossAmount | decimal | Total Order amounts in that month |
| NetAmount | decimal | Gross minus platform commission |

## DTO: UserGrowthDto

| Field | Type | Source |
|-------|------|--------|
| Month | string | "YYYY-MM" format |
| NewUsers | int | Users created in that month |
| CumulativeTotal | int | Running total of users up to that month |

## DTO: DashboardError

| Field | Type | Purpose |
|-------|------|---------|
| Section | string | Which section failed (e.g., "revenue", "trends") |
| Message | string | Error description or "Service unavailable" |
