# Dashboard Page — Full Role-Based Specification

## Status: Needs Implementation

Current `views/Dashboard.tsx` has hardcoded mock data, references non-existent `MOCK_USER`, uses placeholder stats/charts, and lacks real API integration.

---

## 1. Critical Bug — `MOCK_USER`

**Lines 263, 265, 268** reference `MOCK_USER.name`, `MOCK_USER.email`, `MOCK_USER.avatar` which do not exist.

**Fix:** Replace with data from `AuthContext`:

```tsx
const { user, logout } = useAuth();
```

| Before | After |
|--------|-------|
| `MOCK_USER.name` | `user?.fullName || ''` |
| `MOCK_USER.email` | `user?.email || ''` |
| `MOCK_USER.avatar` | `profile?.profileImageUrl || '/default-avatar.png'` |

---

## 2. Shared Data Source

Add `useProfile()` to the Dashboard component so all roles can access `profileImageUrl`, `fullName`, `email`, `bio`, `nationality`, `dateOfBirth`.

```tsx
import { useProfile } from '../src/features/profile/hooks/useProfile';

const { data: profile } = useProfile();
```

---

## 3. Per-Role Dashboard Requirements

### 3.1 Student Dashboard

#### Stats Cards
| Stat | Source | Endpoint |
|------|--------|----------|
| Total enrolled courses | `enrollments.length` | `GET /api/enrollments` |
| In-progress courses | `enrollments.filter(s => s.status === 'InProgress').length` | same |
| Completed courses | `enrollments.filter(s => s.status === 'Completed').length` | same |
| Learning hours | Sum `ContentProgressDto.WatchTimeSeconds / 3600` across all enrollments | `GET /api/enrollments/{id}/progress` (per enrollment) |
| Certificates earned | `enrollments.filter(s => s.certificateId).length` | `GET /api/enrollments` |

#### Charts
- **Weekly activity** (hours per day): Aggregate `WatchTimeSeconds` from `ContentProgressDto` grouped by day.
- **Course progress bars**: For each active enrollment, show `ProgressPercentage` as a progress bar.

#### Tabs
- **"دوراتي" (My Courses)**: List of `EnrollmentResponseDto` with course title, progress %, last accessed.
  - Empty state: "لم تسجل في أي دورة بعد" → button "تصفح الدورات" navigates to `/courses`.
- **"قيد التقدم" (In Progress)**: Enrollments with `Status === 'InProgress'`, sorted by `LastAccessedAt`.
- **"الشهادات" (Certificates)**: Enrollments where `CertificateId` is not null.

#### API Hooks Needed

```ts
// src/features/enrollment/api/enrollmentApi.ts
export async function getMyEnrollments(): Promise<EnrollmentResponseDto[]>
export async function getEnrollmentProgress(enrollmentId: string): Promise<ContentProgressDto[]>
```

```ts
// src/features/enrollment/hooks/useEnrollments.ts
export function useMyEnrollments(): UseQueryResult<EnrollmentResponseDto[], ApiError>
export function useEnrollmentProgress(enrollmentId: string): UseQueryResult<ContentProgressDto[], ApiError>
```

#### Student DTOs (Frontend)

```ts
export interface EnrollmentResponseDto {
  id: string;
  userId: string;
  courseId: string;
  courseTitle: string;
  enrolledAt: string;
  status: 'InProgress' | 'Completed' | 'Expired' | 'Refunded';
  progressPercentage: number;
  completedAt?: string;
  lastAccessedAt?: string;
  source: 'Purchase' | 'Gift' | 'AdminGrant' | 'Coupon';
  accessExpiresAt?: string;
  isRefunded: boolean;
  certificateId?: string;
}

export interface ContentProgressDto {
  contentType: 'Video' | 'Quiz' | 'Document' | 'LiveSession';
  contentId: string;
  isCompleted: boolean;
  watchTimeSeconds: number;
  attemptsCount: number;
  completionPercentage: number;
  metadata?: string;
  lastAccessedAt?: string;
  completedAt?: string;
}
```

---

### 3.2 Teacher Dashboard

#### Stats Cards
| Stat | Source | Endpoint |
|------|--------|----------|
| Total students | Sum of `EnrollmentResponseDto.length` across all courses | Needs `GET /api/management/courses` (⚠️ **does not exist**) |
| Active courses | Published courses by this instructor (filter locally) | Needs `GET /api/management/courses` (⚠️ **does not exist**) |
| Teaching hours | Sum of `Course.TotalDurationMinutes` across all courses | same |
| Average rating | Average of `Course.AverageRating` across all courses | same |
| Revenue | Sum of instructor's share from orders | Needs new endpoint |

**⚠️ Gap:** There is no `GET /api/management/courses` endpoint to list the instructor's own courses.  
**Workaround:** Either:
1. Create a backend endpoint: `GET /api/management/courses` → returns `List<CourseSummaryDto>` filtered by instructor.
2. Or use a dedicated instructor dashboard endpoint: `GET /api/instructor/stats` → returns aggregated stats.

#### Charts
- **Student engagement** (enrollments over time per course): needs backend aggregation.
- **Revenue trend** (monthly): needs backend aggregation + new endpoints.

#### Tabs
- **"دوراتي" (My Courses)**: List of the instructor's courses with `EnrollmentCount`, `AverageRating`, `Status`.
- **"قيد المراجعة" (Under Review)**: Edit requests (`GET /api/courses/my-edit-requests`).
- **"التقييمات" (Reviews)**: Reviews for the instructor's courses.

#### Teacher DTOs (Frontend)

```ts
export interface CourseSummaryDto {
  id: string;
  title: string;
  slug: string;
  description?: string;
  price: number;
  level: string;
  language: string;
  status: string;
  totalDurationMinutes: number;
  enrollmentCount: number;
  averageRating: number;
  createdAt: string;
  thumbnailUrl?: string;
  categoryName?: string;
}

export interface EditRequestSummaryDto {
  id: string;
  courseTitle: string;
  requestType: string;
  operation: string;
  status: string;
  requestedAt: string;
  timeUntilExpiry?: string;
  riskLevel?: string;
  isEmergency: boolean;
}
```

---

### 3.3 Admin Dashboard

#### Stats Cards
| Stat | Source | Endpoint |
|------|--------|----------|
| Total users | Aggregate from `PlatformStatsDto.TotalStudents + TotalInstructors` or dedicated endpoint | `GET /api/public/courses/stats` gives `TotalStudents`, `TotalInstructors`, but **not** total users |
| Published courses | `PlatformStatsDto.TotalCourses` | `GET /api/public/courses/stats` |
| Total revenue | **No endpoint exists** | Needs new `GET /api/admin/revenue/totals` |
| Pending reviews | `pendingCourses.length` | `GET /api/admin/courses/pending` |
| Active instructors | `PlatformStatsDto.TotalInstructors` | `GET /api/public/courses/stats` |

**⚠️ Gaps:**
- No `GET /api/admin/stats/users` — need total user count, role distribution, user growth.
- No `GET /api/admin/revenue/totals` — need total revenue, monthly breakdown.
- `PlatformStatsDto` only has `TotalCourses`, `TotalStudents`, `TotalInstructors`, `TotalCategories`.

**Recommended new backend endpoints for Admin:**
- `GET /api/admin/stats/overview` → `{ totalUsers, totalCourses, totalRevenue, pendingCourses, pendingTeacherRequests, totalInstructors }`
- `GET /api/admin/revenue/monthly?months=12` → `[{ month, amount }]`
- `GET /api/admin/stats/user-growth?months=6` → `[{ month, newUsers, totalUsers }]`

#### Charts
- **Traffic / platform usage**: Area chart (daily active users — needs new endpoint).
- **User growth**: Bar chart (new users per month — needs new endpoint).
- **Revenue trend**: Bar chart (revenue per month — needs new endpoint).

#### Tabs
- **"الدورات" (Courses)**: List of all courses with filtering (pending/approved/rejected).
- **"المستخدمين" (Users)**: User management list (needs new endpoint).
- **"السجلات" (Logs)**: Activity log (needs new endpoint).

#### Admin DTOs (Frontend)

```ts
export interface PlatformStatsDto {
  totalCourses: number;
  totalStudents: number;
  totalInstructors: number;
  totalCategories: number;
}

export interface StorageStatsDto {
  totalFilesCount: number;
  totalSizeBytes: number;
  bucketStats: Record<string, { filesCount: number; sizeBytes: number }>;
  fileTypeCounts: Record<string, number>;
}
```

---

## 4. Shared Navigation & Layout Structure

The Dashboard layout has three main sections (same for all roles):

```
┌─────────────────────────────────────┐
│ Top Nav Bar (logout, theme, lang)   │
├─────────────────────────────────────┤
│ User Header (avatar, name, email)   │
├─────────────────────────────────────┤
│ Stats Grid (5 cards per role)       │
├─────────────────────────────────────┤
│ Charts Section (2 charts)           │
├─────────────────────────────────────┤
│ Tabs (3 tabs per role)              │
│ ┌─────────────────────────────────┐ │
│ │ Tab Content (list/empty state)  │ │
│ └─────────────────────────────────┘ │
├─────────────────────────────────────┤
│ Footer                               │
└─────────────────────────────────────┘
```

---

## 5. Implementation Order

### Phase 1 — Fix MOCK_USER + Add Profile Integration
1. Replace `MOCK_USER` with `useAuth().user` and `useProfile().data`
2. Import `useProfile` hook
3. Add a `photoVersion` state (like EditProfile) for cache busting

### Phase 2 — Student Dashboard
1. Create `src/features/enrollment/api/enrollmentApi.ts` with `getMyEnrollments`, `getEnrollmentProgress`
2. Create `src/features/enrollment/hooks/useEnrollments.ts` with `useMyEnrollments`, `useEnrollmentProgress`
3. Create frontend DTOs: `EnrollmentResponseDto`, `ContentProgressDto`
4. Replace student stats with real data from `useMyEnrollments()`
5. Replace student tab content with real enrollment list
6. Replace student charts with real progress data

### Phase 3 — Teacher Dashboard
1. Coordinate with backend to add `GET /api/management/courses` endpoint
2. Create `src/features/course/api/courseApi.ts` with `getMyCourses`, `getMyEditRequests`
3. Create `src/features/course/hooks/useCourses.ts` with `useMyCourses`, `useMyEditRequests`
4. Replace teacher stats with real data
5. Replace teacher tab content with real course/edit-request lists
6. Replace teacher charts with real engagement/revenue data (may need backend endpoints)

### Phase 4 — Admin Dashboard
1. Coordinate with backend to add `GET /api/admin/stats/overview`, revenue, user-growth endpoints
2. Create `src/features/admin/api/adminApi.ts` with `getPlatformStats`, `getRevenue`, `getUserGrowth`
3. Create `src/features/admin/hooks/useAdmin.ts` with hooks
4. Replace admin stats with real data from backend
5. Replace admin tabs with real user/course/pending-approval lists
6. Replace admin charts with real data

### Phase 5 — Polish
1. Add loading skeletons for stats, charts, and tab content
2. Add error state for each data section
3. Add pull-to-refresh or periodic refetch
4. Connect "Browse Courses" / "Create Course" / "View Reports" buttons to real routes

---

## 6. Loading & Error States

Every data-driven section must handle three states:

```tsx
if (isLoading) return <LoadingSpinner size="lg" />
if (error) return <ErrorMessage message={...} onRetry={() => refetch()} />
if (!data || data.length === 0) return <EmptyState ... />
return <RealContent ... />
```

For stats specifically, show skeleton cards during loading instead of spinner:

```tsx
{isLoading
  ? Array.from({ length: 5 }).map((_, i) => (
      <div key={i} className="animate-pulse bg-card border border-default rounded-xl p-6 h-32" />
    ))
  : content.stats.map(...)
}
```

---

## 7. Existing Frontend — Relevant Files

| File | Purpose |
|------|---------|
| `views/Dashboard.tsx` | The dashboard page to be refactored |
| `src/features/auth/context/AuthContext.tsx` | `useAuth()` — provides `user` with `fullName`, `email`, `profilePictureUrl`, `roles` |
| `src/features/profile/hooks/useProfile.ts` | `useProfile()` — provides `profileImageUrl` |
| `src/shared/types/api.ts` | All shared types, update with new DTOs |
| `src/shared/lib/api/client.ts` | API client with auth interceptor |
| `components/Charts.tsx` | `BarChart`, `AreaChart` components (accept `data: number[]`, `labels: string[]`, `color: string`) |
| `utils/translations.ts` | `t[lang].dashboard` — add any missing translation keys |

---

## 8. Backend Endpoint Gaps Summary

| Endpoint | Needed For | Priority |
|----------|-----------|----------|
| `GET /api/management/courses` — instructor's own courses list | Teacher dashboard | **High** |
| `GET /api/admin/stats/overview` — aggregated stats | Admin dashboard | **High** |
| `GET /api/admin/revenue/monthly` — revenue by month | Admin charts | **Medium** |
| `GET /api/admin/stats/user-growth` — users over time | Admin charts | **Medium** |
| `POST /api/admin/stats` or `GET /api/admin/stats/enrollments` — enrollment trends | Admin + Teacher charts | **Low** |
