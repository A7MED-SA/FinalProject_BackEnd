# Research: Backend Modifications — Athary Platform

**Date**: 2026-06-23  
**Feature**: Backend Modifications  
**Spec**: [spec.md](./spec.md)

## Research Areas

### 1. Slug Generation for Arabic Names

**Decision**: Use regex to preserve Arabic characters (Unicode range \u0600-\u06FF) while removing special characters.

**Rationale**: 
- Arabic names are primary content on the platform
- Slugs must be URL-safe while remaining readable
- Fallback to GUID-based slug if result is too short

**Alternatives Considered**:
- Transliteration to Latin: Rejected - loses cultural context
- Pure GUID: Rejected - not human-readable
- Hybrid approach: Selected - Arabic with fallback

**Implementation Pattern**:
```csharp
baseSlug = Regex.Replace(baseSlug, @"[^a-z0-9-\u0600-\u06FF]", "");
if (string.IsNullOrWhiteSpace(baseSlug) || baseSlug.Length < 3)
    baseSlug = $"user-{Guid.NewGuid().ToString()[..8]}";
```

### 2. Landing Endpoint Caching Strategy

**Decision**: Use IMemoryCache with 5-minute expiration and try-catch fallback.

**Rationale**:
- Landing page is high-traffic, needs fast response
- 5-minute cache balances freshness vs performance
- Fallback to empty data ensures availability

**Alternatives Considered**:
- Distributed cache (Redis): Overkill for v1
- No caching: Would cause DB overload
- Longer cache (1 hour): Too stale for dynamic content

### 3. Testimonial One-Per-User Enforcement

**Decision**: Unique constraint on UserId column + upsert logic in service.

**Rationale**:
- Prevents duplicate testimonials per user
- Simplifies admin curation
- Update existing instead of creating new

**Alternatives Considered**:
- Multiple testimonials: Rejected - dilutes social proof
- Soft delete + create: More complex, same result
- Unique constraint: Cleanest solution

### 4. Contact Form Spam Detection

**Decision**: Keyword-based detection with predefined spam list.

**Rationale**:
- Simple to implement and maintain
- Covers common spam patterns
- Can be extended with ML later

**Alternatives Considered**:
- CAPTCHA: Adds UX friction
- ML-based: Overkill for v1
- Rate limiting only: Doesn't catch content

### 5. Legal Page Type Validation

**Decision**: Fixed enum with hardcoded validation list.

**Rationale**:
- Legal pages have regulatory implications
- Types should be deliberately added via code changes
- Prevents accidental or malicious type creation

**Alternatives Considered**:
- Dynamic types: Too risky for legal content
- Database-driven types: Adds complexity
- Fixed enum: Safest approach

### 6. Rate Limiting Configuration

**Decision**: Fixed window limiter, 5 requests per minute per IP.

**Rationale**:
- Prevents abuse of contact form
- Reasonable limit for legitimate users
- IP-based partitioning prevents distributed abuse

**Alternatives Considered**:
- Sliding window: More complex, similar result
- Token bucket: Overkill for simple use case
- User-based: Anonymous users have no identity

## Summary

All research areas resolved with clear decisions. No NEEDS CLARIFICATION items remain. Ready for Phase 1 design.
