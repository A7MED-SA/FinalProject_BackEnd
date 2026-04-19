# Quickstart: Course Management Feature

## Overview
This feature introduces the API endpoints necessary for instructors to create and manage courses, and for administrators to approve them.

## Setup Instructions

1. **Database Migration**
   - The entities already exist in the codebase.
   - Run standard EF Core migrations if new fields (e.g. `Position`, `DeletedAt`) were added during this implementation.
   ```bash
   dotnet ef migrations add CourseManagementUpdates
   dotnet ef database update
   ```

2. **Dependencies**
   - Ensure `FluentValidation.AspNetCore` is installed in the Web API project.
   ```bash
   dotnet add package FluentValidation.AspNetCore
   ```

3. **Running the API**
   - Press F5 in Visual Studio or run `dotnet run` in the `backend_project` directory.
   - Access the Swagger UI at `https://localhost:<port>/swagger` to view the new endpoints under the `Course Management` section.

## Key Workflows to Test

1. **Instructor Flow**:
   - Login as an Instructor.
   - Create a Category (or use an existing one).
   - Create a Course (starts in `Draft`).
   - Add Sections and Items.
   - Hit `/submit-for-review`.
2. **Admin Flow**:
   - Login as Admin.
   - Fetch `/api/admin/courses/pending`.
   - Approve the newly created course.
