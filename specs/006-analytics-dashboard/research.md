# Research: Analytics Dashboard

## Decision Summary

| Decision | Chosen Approach | Rationale |
|----------|----------------|-----------|
| Aggregation Strategy | Real-time EF Core queries for current stats; cached nightly snapshots for trends | Current stats must be accurate (enrollments, progress); trends tolerate staleness |
| Dashboard Separation | Three service interfaces (Student/Instructor/Admin) | SRP — each role has distinct aggregation logic |
| Revenue Calculation | Gross from Order totals minus refunded amounts; Net = Gross × instructor share % | Spec requires both values; Net is primary for instructor display |
| Instructor Share Storage | New column on User profile (`RevenueSharePercentage`) | Per-instructor negotiated rate per FR-017 |
| Degraded Mode | Return partial data with error indicator field | Spec FR-019 |
| Audit Logging | Reuse existing `IActivityLogService.LogActivityAsync` | Consistent with existing system pattern |
| Learning Hours | Video: real `WatchTimeSeconds`; Quiz/Doc: standard platform estimate | Spec clarification Q3 |
| Hybrid Cache | In-memory cache for trend data (nightly refresh); no cache for live stats | Balances accuracy vs performance |

## Research Details

### 1. Aggregation Strategy — EF Core Queries

**Decision**: Use EF Core LINQ queries with GroupBy and Sum for real-time aggregation. No new stored procedures or materialized views.

**Rationale**:
- Existing models have proper indexes on key columns (UserId, CourseId, Status, CreatedAt)
- EF Core 9.0 translates GroupBy/Sum to efficient SQL
- Student and Teacher dashboards query a limited scope (one user's data), so performance is predictable
- Admin dashboard may need optimization at scale — addressed via performance target (5s for 100k users)

**Student Stats Query Pattern**:
```
Enrollments.Where(e => e.UserId == userId)
  .GroupBy(e => 1)
  .Select(g => new {
    TotalEnrolled = g.Count(),
    InProgress = g.Count(e => e.Status == InProgress),
    Completed = g.Count(e => e.Status == Completed)
  })
```

### 2. Revenue Computation

**Decision**: Compute from existing Order and Refund models.

**Gross Revenue**: Sum of `Order.TotalAmount` for completed orders, excluding orders where `Order.Status == Refunded` or where linked refunds are approved.

**Net Revenue (Instructor)**: `Gross × Instructor.RevenueSharePercentage / 100`.

**Challenges**:
- Revenue per instructor requires mapping orders to courses to instructors
- The existing Order → OrderItems → Course → Instructor chain must be traversed
- Performance concern: may need a cached instructor revenue aggregate table for high-traffic platforms

### 3. Instructor Revenue Share Storage

**Decision**: Add `RevenueSharePercentage` (decimal, default 50.00) to the User model or instructor profile.

**Rationale**:
- Per-instructor negotiated rate requires a persistent field
- Default 50% is industry standard split
- Admins can adjust via existing user management endpoints

### 4. Trend Data Caching

**Decision**: Use `IMemoryCache` with absolute expiration set to next midnight.

**Rationale**:
- Revenue and user growth data only changes daily (new orders, new signups)
- Nightly recomputation ensures data is fresh once per day
- Falls back gracefully if cache is empty (recompute on demand with log warning)
- Simpler than Redis or distributed cache — single-server deployment is sufficient

### 5. Degraded Mode Implementation

**Decision**: Each sub-aggregation wrapped in try/catch. Successful results collected in a result DTO with an `Errors` dictionary.

**Rationale**:
- Prevents one failed aggregation from blocking the entire dashboard
- Frontend receives `{ stats: {...}, errors: { revenue: "Service unavailable" } }`
- Matches spec FR-019 and clarification Q2

### 6. Audit Logging

**Decision**: Reuse `IActivityLogService.LogActivityAsync` with action strings like `"DashboardViewed_Revenue"`, `"DashboardViewed_AdminOverview"`.

**Rationale**:
- Consistent with existing system (RefundService, EnrollmentService use same pattern)
- No new infrastructure needed
- IP address from HttpContext for audit trail
