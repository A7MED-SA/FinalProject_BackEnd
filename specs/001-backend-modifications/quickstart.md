# Quickstart Validation Guide: Backend Modifications

**Date**: 2026-06-23  
**Feature**: Backend Modifications  
**Spec**: [spec.md](./spec.md)

## Prerequisites

- .NET 9 SDK installed
- PostgreSQL running with connection string configured
- Project built successfully (`dotnet build`)
- Database migrated to latest version

## Validation Scenarios

### 1. Instructor Slug Profile

**Setup**: Create an instructor user with FirstName="Ahmed", LastName="Ali"

**Test**:
```bash
# Access profile via slug
curl -s https://localhost:7001/api/public/instructors/ahmed-ali | jq .

# Expected: 200 OK with PublicProfileDto containing slug

# Check availability
curl -s "https://localhost:7001/api/public/instructors/check-slug?slug=ahmed-ali" -H "Authorization: Bearer $TOKEN" | jq .

# Expected: 200 OK with false (not available)

# Search instructors
curl -s "https://localhost:7001/api/public/instructors/search?q=ahmed" | jq .

# Expected: 200 OK with list containing the instructor
```

### 2. Landing Aggregated Endpoint

**Test**:
```bash
# Get all landing data
curl -s https://localhost:7001/api/public/landing | jq .

# Expected: 200 OK with:
# - stats (totalStudents, totalCourses, etc.)
# - categories (list)
# - featuredCourses (list, max 6)
# - upcomingLiveSessions (list, max 4)
# - testimonials (list, max 10)

# Second request should be faster (cached)
time curl -s https://localhost:7001/api/public/landing > /dev/null
# Expected: < 1 second
```

### 3. Testimonials Management

**Setup**: Login as student user

**Test**:
```bash
# Submit testimonial (student)
curl -s -X POST https://localhost:7001/api/public/testimonials \
  -H "Authorization: Bearer $STUDENT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"content": "Great platform!", "rating": 5}' | jq .

# Expected: 201 Created

# Submit again (should update, not create)
curl -s -X POST https://localhost:7001/api/public/testimonials \
  -H "Authorization: Bearer $STUDENT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"content": "Updated review", "rating": 4}' | jq .

# Expected: 200 OK (updated, not 201)

# Admin approve
curl -s -X PATCH https://localhost:7001/api/admin/testimonials/{id}/approve \
  -H "Authorization: Bearer $ADMIN_TOKEN" | jq .

# Expected: 200 OK

# Public sees approved only
curl -s https://localhost:7001/api/public/testimonials | jq .
# Expected: Only approved testimonials
```

### 4. Notification Preferences

**Setup**: Login as any user

**Test**:
```bash
# Get preferences
curl -s https://localhost:7001/api/notifications/preferences \
  -H "Authorization: Bearer $TOKEN" | jq .

# Expected: 200 OK with default preferences

# Update marketing emails to false
curl -s -X PUT https://localhost:7001/api/notifications/preferences \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"marketingEmails": false}' | jq .

# Expected: 200 OK with updated preferences

# Verify persistence
curl -s https://localhost:7001/api/notifications/preferences \
  -H "Authorization: Bearer $TOKEN" | jq .marketingEmails
# Expected: false
```

### 5. Contact Form

**Test**:
```bash
# Submit valid message
curl -s -X POST https://localhost:7001/api/public/contact \
  -H "Content-Type: application/json" \
  -d '{"fullName": "Test User", "email": "test@example.com", "subject": "Question", "message": "Hello, I have a question."}' | jq .

# Expected: 200 OK

# Submit spam message
curl -s -X POST https://localhost:7001/api/public/contact \
  -H "Content-Type: application/json" \
  -d '{"fullName": "Spammer", "email": "spam@evil.com", "subject": "Buy viagra", "message": "viagra cheap"}' | jq .

# Expected: 400 Bad Request with spam error

# Admin view messages
curl -s https://localhost:7001/api/admin/contact \
  -H "Authorization: Bearer $ADMIN_TOKEN" | jq .

# Expected: 200 OK with message list
```

### 6. Legal Pages

**Test**:
```bash
# Get privacy policy
curl -s https://localhost:7001/api/public/legal/privacy | jq .

# Expected: 200 OK with content (or 404 if not created)

# Get invalid type
curl -s https://localhost:7001/api/public/legal/invalid | jq .

# Expected: 400 Bad Request
```

## Success Criteria Validation

| Criteria | How to Validate |
|----------|-----------------|
| SC-001 | Access instructor via slug URL |
| SC-002 | Single request returns all landing data |
| SC-003 | Admin can approve testimonial in one click |
| SC-004 | User updates preferences, sees change immediately |
| SC-005 | Contact submission completes in < 2s |
| SC-006 | 6th request in 1 minute gets rate limited |
| SC-007 | Legal page returns in < 1s |
| SC-008 | All responses wrapped in ApiResponse format |
| SC-009 | Migrations run without errors |
| SC-010 | Edge cases return appropriate errors |
