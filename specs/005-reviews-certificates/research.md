# Research: Reviews & Certificates

## Decision Summary

| Decision | Chosen Approach | Rationale |
|----------|----------------|-----------|
| PDF Library | QuestPDF | Open source (MIT), 14k+ GitHub stars, modern C# fluent API, no external dependencies |
| Verification Code | UUID-based (`CERT-` + UUID truncated) | Globally unique, no collision risk, human-readable prefix |
| Completion Trigger | Subscribe to `RecalculateEnrollmentProgressAsync` | Existing system already sets `EnrollmentStatus.Completed` when progress >= 100% |
| Certificate Revocation | Event-driven on refund + admin manual | Refund flow already exists; admin revocation adds manual override |
| Review Moderation | Flag queue + admin review panel | Manual review avoids false positives from auto-thresholds |

## Research Details

### 1. PDF Generation — QuestPDF

**Decision**: QuestPDF 2026.5.0

**Rationale**:
- MIT-licensed open source library with 14k+ stars
- Modern C# Fluent API — no proprietary scripting
- No external process dependencies (no headless browser, no COM)
- Supports .NET 9.0, Linux deployment, Docker
- Hot-reload capability for rapid prototyping
- PDF/A and PDF/UA compliance available if needed

**Integration**: `dotnet add package QuestPDF`

### 2. Verification Code Format

**Decision**: `CERT-` + 8-char UUID-derived hex

**Rationale**:
- UUID-based ensures global uniqueness without a centralized counter
- Human-readable prefix `CERT-` distinguishes codes visually
- 8 chars after prefix balances readability with uniqueness (16^8 = 4 billion combinations)
- Example: `CERT-A3F8B2C1`

**Storage**: Store code as VARCHAR(50) with unique index in the database.

### 3. Course Completion Trigger

**Decision**: Monitor `EnrollmentStatus.Completed` transition

**Rationale**:
- The existing `ContentProgressService.RecalculateEnrollmentProgressAsync` already sets `Enrollment.Status = EnrollmentStatus.Completed` when all mandatory items are completed
- Certificate generation is triggered when the enrollment status changes to `Completed`
- Implementation: In `ContentProgressService.RecalculateEnrollmentProgressAsync`, after setting completed status, call a new `ICertificateService.GenerateCertificateAsync(enrollmentId)` method
- Alternatively, an event/pub-sub pattern could be used, but direct call keeps things simple (KISS)

### 4. Certificate Revocation

**Decision**: Two triggers — automatic (refund) + manual (admin)

**Automatic**: In `RefundService.ApproveRefundAsync`, after setting `enrollment.Status = Refunded`, call `ICertificateService.RevokeCertificateAsync(enrollmentId)`.

**Manual**: New admin endpoint `POST /api/admin/certificates/{certificateId}/revoke` with admin-only authorization.

### 5. Unique Constraints

**Decision**: Enforce at database level

- `(UserId, CourseId)` unique on Reviews — one review per user per course
- `Code` unique on Certificates — globally unique verification code
- Both enforced via EF Core `HasIndex().IsUnique()` in `ApplicationDbContext`
