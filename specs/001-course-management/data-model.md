# Data Model: Course Management System

## Core Entities (EF Core Models)

The following entities have been identified from the ERD and are expected to exist in `backend_project/Models`:

1. **Category**
   - Fields: `Id`, `Name`, `Description`, `ParentId`, `Position`
   - Relationships: Self-referencing (Parent/Children), Courses

2. **Course**
   - Fields: `Id`, `Title`, `Description`, `Status` (Draft, PendingReview, Published), `CategoryId`, `CreatedById`, `DeletedAt` (Soft Delete)
   - Relationships: Category, Sections, Requirements, LearningOutcomes

3. **CourseRequirement**
   - Fields: `Id`, `CourseId`, `RequirementText`

4. **CourseLearningOutcome**
   - Fields: `Id`, `CourseId`, `OutcomeText`

5. **Section**
   - Fields: `Id`, `CourseId`, `Title`, `Position`
   - Relationships: Course, SectionItems

6. **SectionItem**
   - Fields: `Id`, `SectionId`, `Title`, `ItemType` (Video, Quiz, Document), `FileId` (Guid), `Position`
   - Relationships: Section

## Data Transfer Objects (DTOs)

### Category DTOs
- `CategoryResponseDto`: Includes `Id`, `Name`, and nested `List<CategoryResponseDto> Children`.
- `CreateCategoryDto` / `UpdateCategoryDto`

### Course DTOs
- `CourseSummaryDto`: Flattened minimal view for list screens.
- `CourseDetailsDto`: Aggregated view. Includes `Sections`, `Requirements`, and `LearningOutcomes`.
- `CreateCourseDto`: `Title`, `CategoryId`, `Description`.
- `UpdateCourseDto`: `Title`, `CategoryId`, `Description`.

### Section DTOs
- `SectionDto`: `Id`, `Title`, `Position`, `List<SectionItemDto> Items`.
- `CreateSectionDto`: `Title`, `CourseId`.
- `ReorderRequestDto`: `Id`, `Position`.

### Validation Rules (FluentValidation)
- **Title/Name**: `NotEmpty()`, `MaximumLength(200)`.
- **Course Status**: Validated against allowed Enum transitions (Draft -> PendingReview -> Published).
- **Position**: Must be non-negative.
