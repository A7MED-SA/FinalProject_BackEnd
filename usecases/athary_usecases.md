# Athary E-Learning Platform - Use Cases Document v2.0

**Project:** Athary Educational Platform  
**Version:** 2.0  
**Date:** October 26, 2025  
**Status:** 26/80 Complete (32.5%)

---

## 📋 Table of Contents

- [Part 1: Introduction](#part-1-introduction)
- [Part 2: Use Cases (26 Complete)](#part-2-use-cases)
  - [1. User Management (5)](#1-user-management)
  - [2. Course Management (4)](#2-course-management)
  - [3. Enrollment & Payment (4)](#3-enrollment--payment)
  - [4. Educational Content (3)](#4-educational-content)
  - [5. Quizzes & Assessments (3)](#5-quizzes--assessments)
  - [6. Live Streaming (3)](#6-live-streaming)
  - [7. Remaining Sections (54)](#7-remaining-sections)
- [Part 3: Appendices](#part-3-appendices)

---

# Part 1: Introduction

## Document Purpose
This document specifies all use cases for the Athary E-Learning Platform, following standardized format for consistency.

## Actors
| Actor | Description |
|-------|-------------|
| **Student** | Enrolls in courses, learns, takes quizzes |
| **Teacher** | Creates courses, teaches, grades |
| **Assistant** | Helps teacher with grading and monitoring |
| **Admin** | Manages system, users, and platform |
| **System** | Automated processes and background tasks |

## Use Case Template Structure
Each UC includes: ID, Name, Actors, Goal, Scope, Trigger, Preconditions, Main Flow, Alternative Flows, Exception Flows, Postconditions, Requirements, Business Rules, Notes.

---

# Part 2: Use Cases

---

# 1. User Management

## UC-001: Register New User
**Primary Actor:** New User  
**Goal:** Create account on platform  

### Main Flow
1. User clicks "Create Account"
2. System shows registration options (Email or OAuth)
3. **Email Path:** User enters name, email, password, DateOfBirth, Gender, Phone Number, Address → System validates → Creates account (Inactive) → Sends activation email → User activates → Account Active
4. **OAuth Path:** User clicks Google/Facebook/Apple → Authenticates with provider → System creates account (Active) → Auto-login

### Key Features
- Email registration with activation
- OAuth 2.0 (Google, Facebook, Apple)
- Password strength validation
- reCAPTCHA verification
- Account linking capability

### Business Rules
- Password: 8+ chars, mixed case, number, symbol
- Email must be unique
- OAuth accounts pre-verified (no activation needed)
- Rate limit: 5 attempts/hour per IP

---

## UC-002: Login
**Primary Actor:** Registered User  
**Goal:** Authenticate and access platform  

### Main Flow
1. User opens login page
2. System shows login options
3. **Email Path:** Enters email/password → System validates → Creates JWT session → Redirects to dashboard
4. **OAuth Path:** Clicks provider → Authenticates → System finds/creates session → Redirects

### Key Features
- Email/password authentication
- Social login (Google, Facebook, Apple)
- Remember Me (30 days)
- 2FA support
- Account linking during login

### Business Rules
- 5 failed attempts = 30min lockout
- Session: 1h standard, 30 days with Remember Me
- Auto-logout after 30min inactivity
- Log all login attempts

---

## UC-003: Password Recovery
**Primary Actor:** Registered User  
**Goal:** Reset forgotten password  

### Main Flow
1. User clicks "Forgot Password?"
2. Enters email → System generates reset token (1h validity)
3. System sends reset email with link
4. User clicks link → Enters new password → System validates
5. System updates password → Invalidates token → Sends confirmation

### Business Rules
- Reset token: 1h validity, single-use
- Rate limit: 3 resets/hour per email
- Cannot reuse last 3 passwords
- Force logout all devices after change

---

## UC-004: Update Profile
**Primary Actor:** Any Logged-in User  
**Goal:** Update personal information  

### Main Flow
1. User opens Profile Settings
2. System displays edit form
3. User modifies fields (name, picture, phone, bio)
4. System validates → Updates database → Clears cache

### Editable Fields
- Full Name, Profile Picture, Phone Number
- Bio (teachers), Social Links
- Notification Preferences, Language, Timezone

### Business Rules
- Picture: Max 2MB, JPG/PNG/WebP, min 200x200px
- Name: 3-50 chars
- Email change: Separate process, max once/30 days

---

## UC-005: Manage Connected Accounts (OAuth)
**Primary Actor:** Registered User  
**Goal:** Manage linked social login accounts  

### Main Flow
1. User opens Connected Accounts settings
2. System shows all linked methods (Email, Google, Facebook, Apple)
3. User can:
   - Link new provider
   - Unlink existing provider (if >1 method exists)
   - Set primary method

### Business Rules
- Must have ≥1 active authentication method
- Max 3 OAuth providers per account
- Cannot unlink last method
- Unlinking requires password confirmation

---

# 2. Course Management

## UC-006: Create Course
**Primary Actor:** Teacher  
**Goal:** Create new course on platform  

### Main Flow (5-Step Wizard)
1. **Step 1:** Basic info (title, description, category, level, language)
2. **Step 2:** Pricing (free/paid, price, teaching method, schedule)
3. **Step 3:** Media (cover image, preview video)
4. **Step 4:** Course structure (sections, estimated content)
5. **Step 5:** Review all → Create course (status: Draft)

### Business Rules
- Title: 10-100 chars, unique per teacher
- Price: Free (0) or Paid (10-10,000 SAR)
- Cover image required before publishing
- Free teachers: max 3 courses
- Verified teachers: unlimited

---

## UC-007: Edit Course
**Primary Actor:** Teacher  
**Goal:** Modify course information  

### Main Flow
1. Teacher selects course → Clicks "Edit"
2. System loads current data → Shows edit form
3. Teacher modifies fields → Saves changes
4. If published course: System notifies enrolled students

### Editable Fields
- Always: Description, tags, promotional content
- With restrictions: Title (no enrollments), Price (increase anytime, decrease with notice), Schedule (with notice)

### Business Rules
- Price changes: 7-day notice for increases
- Cannot change paid→free if enrollments exist
- Max 5 major edits/month for published courses
- All changes logged (audit trail)

---

## UC-008: Delete/Archive Course
**Primary Actor:** Teacher  
**Goal:** Remove or archive course  

### Main Flow
1. Teacher selects course → Clicks "Delete" or "Archive"
2. System checks enrolled students → Shows impact summary
3. Teacher confirms → Selects action for students (refund/no refund)
4. System processes refunds (if applicable) → Changes status → Notifies students

### Options
- **Delete:** Soft delete (recoverable 30 days), processes refunds
- **Archive:** Hidden from public, students retain access, no refunds

### Business Rules
- Cannot delete if upcoming live sessions exist
- Refund: 0-30% progress = 100%, 31-60% = 50%, 61%+ = No refund
- Data retained: Student progress 1 year, Content 30 days, Financial records 7 years

---

## UC-009: Browse Courses
**Primary Actor:** Student / Guest  
**Goal:** Discover and explore courses  

### Main Flow
1. User visits courses page
2. System retrieves published courses → Displays grid/list
3. User can:
   - Search by keywords
   - Filter (category, price, level, language, rating, duration)
   - Sort (popular, newest, highest rated, price)
4. User selects course → System shows detail page

### Business Rules
- Default: 12 courses/page (grid), 20/page (list)
- Search: title, description, tags, instructor (case-insensitive, fuzzy matching)
- Listings cached 5 minutes
- Guests see all published, logged-in see personalized recommendations

---

# 3. Enrollment & Payment

## UC-010: Enroll in Course
**Primary Actor:** Student  
**Goal:** Register and pay for course  

### Main Flow
1. Student views course → Clicks "Enroll Now"
2. System verifies login, enrollment status, availability
3. System shows checkout page with price breakdown
4. Student optionally enters discount code → System validates and applies
5. Student reviews refund policy → Agrees to terms → Selects payment method
6. System redirects to payment gateway (Stripe/PayPal)
7. Student completes payment → Gateway processes → Sends webhook
8. System validates webhook → Enrolls student → Grants access → Sends confirmation email

### Price Breakdown Example
```
Course Price:        500 SAR
Discount (20%):     -100 SAR
Subtotal:            400 SAR
VAT (15%):          + 60 SAR
Total:               460 SAR
```

### Business Rules
- Min price: 10 SAR
- Platform fee: 10% (from teacher earnings)
- VAT: 15% (added to student price)
- Session timeout: 30 minutes
- Webhook timeout: 10 minutes

---

## UC-011: Process Payment
**Primary Actor:** System (Automated)  
**Goal:** Handle payment transaction  

### Main Flow
1. System receives payment request → Validates → Generates transaction ID
2. Creates payment record (status: Initiated)
3. Calls payment gateway API → Creates payment session
4. Gateway processes transaction → Sends webhook
5. System validates webhook signature → Updates payment record
6. If successful: Enrolls student, sends receipt, logs transaction

### Payment Status Flow
```
Initiated → Processing → Success/Failed/Pending
```

### Business Rules
- Webhook signature mandatory (HMAC-SHA256)
- Idempotency handling (prevent duplicate processing)
- Retry: 3 attempts with exponential backoff (1s, 2s, 4s)
- Transaction logging: 7 years (legal requirement)
- PCI-DSS Level 1 compliance (via gateways)

---

## UC-012: Request Refund
**Primary Actor:** Student / Admin  
**Goal:** Process refund for paid course  

### Main Flow
1. Student selects enrolled course → Clicks "Request Refund"
2. System checks eligibility → Calculates refund amount
3. Student selects reason → Enters details → Confirms request
4. System creates refund request (status: Pending) → Notifies admin/teacher
5. Admin reviews → Approves or rejects
6. If approved: System initiates refund with gateway → Gateway processes → System updates status
7. System revokes access → Logs transaction → Sends confirmation → Adjusts teacher earnings

### Refund Calculation
```
Days: 0-7 = 100%, 8-14 = 50%, 15+ = No refund
OR
Progress: 0-10% = 100%, 11-30% = 75%, 31-60% = 50%, 61%+ = No refund
(Whichever more favorable to student)
```

### Business Rules
- Auto-approval: <48h enrollment + <10% progress
- Processing: 5-7 business days
- Max 3 refunds per student per year
- Teacher impact: High refund rate (>15%) triggers review

---

## UC-013: Apply Discount Code
**Primary Actor:** Student  
**Goal:** Apply discount during checkout  

### Main Flow
1. Student at checkout → Enters discount code → Clicks "Apply"
2. System validates code (exists, active, not expired, usage limits OK, applicable to course)
3. System calculates discount → Updates price breakdown → Recalculates tax
4. System shows success message → Locks code to transaction

### Discount Types
- Percentage (e.g., 20% off)
- Fixed Amount (e.g., 100 SAR off)
- Free Enrollment (100% off)

### Business Rules
- Code format: 4-20 alphanumeric chars, case-insensitive
- Validity: Start/end dates, auto-expires at end date
- Usage limits: Global limit + per-user limit (typically 1)
- Cannot combine multiple codes (generally)
- Teachers create codes for their courses, Admins create platform-wide

---

# 4. Educational Content

## UC-014: Add Content to Course
**Primary Actor:** Teacher  
**Goal:** Add educational materials to course  

### Main Flow
1. Teacher opens course management → Clicks "Add Content"
2. System shows content type selection
3. Teacher selects type → Fills details → Uploads file/enters content
4. Teacher sets properties (title, description, access type, downloadable, required)
5. System validates → Uploads to cloud → Processes file → Creates record → Links to course

### Content Types
1. **Video:** Upload MP4/WebM (max 2GB) or YouTube/Vimeo URL
2. **Document:** Upload PDF (max 50MB), Word, PowerPoint
3. **Quiz:** Create in quiz builder
4. **Text:** Rich text editor with Markdown
5. **External Link:** URL to resource

### Business Rules
- Videos: max 2GB/file, 20GB/course, auto-transcode to H.264, multiple qualities (360p, 720p, 1080p)
- Documents: max 50MB, auto-convert DOCX/PPTX to PDF
- Storage: Cloudflare R2 or AWS S3 with CDN delivery
- Free teachers: max 20 lessons/course
- Backup retention: 30 days after deletion

---

## UC-015: View Course Content
**Primary Actor:** Student (Enrolled)  
**Goal:** Access and consume course content  

### Main Flow
1. Student opens enrolled course → System displays curriculum
2. Student clicks lesson → System checks permissions → Retrieves from CDN
3. System loads appropriate viewer (video player, PDF viewer, text reader)
4. System starts progress tracking → Records viewing time
5. When complete: System updates progress → Unlocks next content (if sequential)

### Content Viewers
- **Video:** Custom player with play/pause, speed (0.5x-2x), quality selector, captions, fullscreen
- **PDF:** Embedded viewer with zoom, navigation, search, download (if enabled)
- **Text:** Reading view with font size adjustment, dark mode, read aloud

### Business Rules
- Progress tracking: Video 80% = viewed, Document scrolled to end = viewed
- Sequential access: If enabled, must complete in order
- Playback speed: Saved per student, applies to all videos
- Concurrent viewing: One device at a time (5-min grace period for switching)

---

## UC-016: Track Student Progress
**Primary Actor:** System (Automated)  
**Goal:** Monitor and record learning progress  

### Main Flow
1. Student performs activity (view lesson, complete quiz, submit assignment)
2. System detects completion → Records details → Calculates updated progress %
3. System updates database → Triggers unlocks (next lesson, badges) → Updates UI

### Tracked Activities
- Lessons viewed, Videos watched (with position), Documents read
- Quizzes attempted/completed, Assignments submitted
- Live class attendance, Time spent, Last access date

### Progress Calculation
```
Overall = (Completed Items / Total Items) × 100%

Weighted = 
  Videos (40% × completion) +
  Quizzes (30% × avg score) +
  Assignments (20% × completion) +
  Participation (10% × activity)
```

### Business Rules
- Update frequency: Video every 10s, Quiz on submit, Lesson on mark/criteria met
- Completion criteria: Video 80%, Reading scrolled or time≥estimated, Quiz submitted
- Streak tracking: Day counts if 1+ min activity, broken if 24h+ without activity
- Certificates: Issued at 100% + passing grade + all required assessments

---

# 5. Quizzes & Assessments

## UC-017: Create Quiz
**Primary Actor:** Teacher  
**Goal:** Create assessment for students  

### Main Flow
1. Teacher navigates to course → Clicks "Add Quiz"
2. System opens quiz builder
3. Teacher enters title/description → Adds questions (select type, enter text, add options, mark correct, set points)
4. Teacher sets quiz settings (time limit, attempts, passing score, show results, randomize)
5. Teacher previews → Saves quiz → System validates → Adds to curriculum

### Question Types
1. Multiple Choice (single answer)
2. Multiple Select (multiple correct answers)
3. True/False
4. Fill in the Blank
5. Essay/Long Answer (manual grading)
6. Matching, Ordering

### Quiz Settings
- Time Limit: None or 5-120 minutes
- Attempts: 1, 2, 3, or Unlimited
- Passing Score: 0-100% (default: 70%)
- Show Results: Immediately, After due date, Manual release
- Randomize: Question order, Answer options

### Business Rules
- Min 1 question, Max 100 questions per quiz, Recommended 10-30
- Point values: 1-100 per question, total calculated automatically
- If timed: Enforced strictly, auto-submit when expires, warning at 5 min
- Objective questions: auto-graded, Essay: manual grading required

---

## UC-018: Take Quiz
**Primary Actor:** Student  
**Goal:** Attempt assessment and receive grade  

### Main Flow
1. Student clicks quiz → System checks eligibility → Shows instructions
2. Student reviews → Clicks "Start Quiz"
3. System generates instance → Randomizes (if set) → Starts timer → Displays questions
4. Student answers questions → System auto-saves every 30s → Navigates between questions
5. Student reviews answers → Clicks "Submit" → Confirms
6. System auto-grades objective → Calculates score → Saves attempt → Updates progress → Shows results (if immediate)

### During Quiz
- Question counter, Timer (countdown), Progress indicator
- Navigation (Next, Previous, Review), Flag questions
- Submit button, Auto-save indicator

### Results Page
- Score (points/total), Percentage, Pass/Fail
- Correct/Incorrect breakdown, Time taken
- Correct answers (if enabled), Explanations, Feedback

### Business Rules
- Navigation: Can move freely, can flag for review, must explicitly submit
- Timing: Grace 5s after expiry, warnings at 10%, 5%, 1%
- Grading: Immediate for objective, delayed for essays
- Scoring: (Points earned / Total) × 100, passing grade per quiz, highest score kept
- Cheating prevention: Randomize order, one question at a time (optional), tab-switching detection (optional)

---

## UC-019: Grade Essay Questions
**Primary Actor:** Teacher / Assistant  
**Goal:** Manually review and grade essays  

### Main Flow
1. Student submits quiz with essays → System auto-grades objective → Marks "Pending Manual Grading" → Notifies teacher
2. Teacher opens grading dashboard → Sees pending submissions
3. Teacher selects submission → Reads essay answers → Assigns points → Adds feedback
4. Teacher repeats for all essays → Clicks "Finalize Grade"
5. System calculates final score → Updates grade → Marks "Graded" → Notifies student

### Grading Features
- Side-by-side: question and answer
- Rubric display (if created)
- AI-suggested points based on keywords
- Previous answers by same student
- Quick feedback templates, Batch grading

### Business Rules
- Grading deadline: Within 7 days, reminder after 3 days, admin notified if >14 days
- Points: 0 to max, partial credit allowed, bonus points possible
- Regrading: Student can request within 7 days, only one request per quiz
- Assistant grading: If authorized, teacher reviews and approves

---

# 6. Live Streaming

## UC-020: Start Live Session
**Primary Actor:** Teacher  
**Goal:** Broadcast live class to students  

### Main Flow
1. Teacher navigates to scheduled session → Clicks "Start Live Session"
2. System checks time window (±15 min) → Tests camera/microphone → Shows device selection
3. Teacher selects devices → Tests preview → Clicks "Go Live"
4. System creates streaming room (WebRTC) → Starts streaming → Updates status "Live" → Sends notifications to students

### Live Dashboard
- Own video feed, Student count (live), Chat messages
- Raised hands queue, Screen share controls, Recording status
- Audio/video toggles, End session button, Participant list

### Streaming Features
- Screen sharing, Whiteboard/drawing tools, Polls and quizzes
- Breakout rooms, Recording (optional), Virtual backgrounds

### Business Rules
- Can start 15 min early, must start within 1 hour of scheduled
- Max duration: 4 hours, warning at 3:45h
- Max participants: 500 (configurable)
- Recording: Requires consent, max 10GB/session, stored 1 year
- Quality: Auto-adjusts (360p, 720p, 1080p), Audio 64-128kbps

---

## UC-021: Join Live Session
**Primary Actor:** Student  
**Goal:** Attend live class  

### Main Flow
1. Student receives notification or sees "Live" indicator → Clicks "Join Session"
2. System verifies enrollment → Checks device permissions → Shows joining screen
3. Student enables/disables camera & mic → Clicks "Join Now"
4. System connects to streaming room → Loads teacher's video → Joins voice (muted) → Updates participant list → Records attendance

### Student Interface
- Teacher's video (main), Shared screen (if sharing), Chat panel
- Participant list, Raise hand button, Reactions (👍, ❤️, 😮)
- Settings (quality), Exit button, Q&A panel

### Participation Options
- Watch/listen, Read/send chat, Raise hand, React with emojis
- Answer polls, View shared materials, Take notes

### Business Rules
- Must be enrolled, session must be live, not banned
- Default: Camera off, Microphone muted
- Attendance: Marked present after 1 min, must stay 75% for "attended"
- Bandwidth: Min 1 Mbps download, recommended 3+ Mbps
- Can only join one live session at a time

---

## UC-022: End Live Session
**Primary Actor:** Teacher  
**Goal:** Close live session and process post-session tasks  

### Main Flow
1. Teacher clicks "End Session" → System shows confirmation
2. Teacher confirms → Chooses recording option (save/discard)
3. System stops streaming → Disconnects all students → Stops recording
4. System calculates duration → Processes attendance → Saves chat transcript → Updates status "Ended"
5. If recording saved: System processes video (transcode, thumbnail, timestamps) → Adds to course
6. System generates analytics → Sends summary email → Notifies students

### Recording Options
- Save & publish immediately
- Save for review (publish later)
- Keep private (teacher only)
- Discard recording

### Session Analytics
- Total duration, Peak/average viewers, Unique participants
- Chat message count, Questions asked, Polls answered
- Engagement score, Attendance report

### Business Rules
- Can end anytime (warning if <5 min)
- Recording: Teacher owns rights, public or private, max 10GB, kept 1 year
- Attendance: 75% of duration = "attended", late joins counted from join time
- Analytics: Available immediately, detailed report within 1 hour
- Cleanup: Room deleted, temp files cleaned, chat logs saved 90 days

---

# 7. Remaining Sections

## Pending Use Cases (54)

### Communication (3 UCs)
- UC-023: Send Private Message
- UC-024: Post Question in Course
- UC-025: Send Group Notification

### Certificates (2 UCs)
- UC-026: Issue Certificate
- UC-027: Verify Certificate

### Reports & Analytics (3 UCs)
- UC-028: View Student Performance Report
- UC-029: View Teacher Analytics
- UC-030: View System Reports (Admin)

### AI Chatbot (5 UCs)
- UC-031: Generate Quiz with AI
- UC-032: Create Google Form Automatically
- UC-033: Analyze Student Answers with AI
- UC-034: Get Help from Chatbot (Student)
- UC-035: Generate Educational Content with AI

[... continues with remaining 41 UCs]

---

# Part 3: Appendices

## Export Instructions

### Quick Export
1. Copy all content (Ctrl+A, Ctrl+C)
2. Paste into Word/Google Docs
3. Apply heading styles
4. Save as DOCX or PDF

### Professional Export (Pandoc)
```bash
pandoc usecases.md -o Athary_UseCases.docx --toc --toc-depth=3
pandoc usecases.md -o Athary_UseCases.pdf --toc --pdf-engine=xelatex
```

## Document Status
**Progress:** 26/80 (32.5%)  
**Completed:** User Management, Course Management, Enrollment & Payment, Educational Content, Quizzes, Live Streaming  
**Remaining:** 54 Use Cases across 23 categories  
**Estimated completion time:** 15-20 hours

## Key Features Documented
✅ OAuth 2.0 (Google, Facebook, Apple)  
✅ Payment Processing (Stripe, PayPal)  
✅ Refund System  
✅ Discount Codes  
✅ Live Streaming (WebRTC/LiveKit)  
✅ Quiz System  
✅ Progress Tracking  
✅ Cloud Storage (R2/S3)

---

*Document Version 2.0*  
*Last Updated: October 26, 2025*  
*Ready for Export and Implementation*