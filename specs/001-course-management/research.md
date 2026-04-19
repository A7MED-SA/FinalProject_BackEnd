# Research: Course Management System

## Resolved Clarifications & Architecture Decisions

### 1. Course Deletion Strategy
- **Decision**: Soft Delete.
- **Rationale**: Preserves historical data for enrolled students, payments, and analytics, which is essential for an LMS.
- **Alternatives considered**: Hard Delete (rejected due to destruction of historical records).

### 2. Input Validation Strategy
- **Decision**: FluentValidation.
- **Rationale**: Keeps validation rules completely separate from DTO models, adhering to the Single Responsibility Principle and enabling complex cross-field validation.
- **Alternatives considered**: Built-in DataAnnotations (rejected as it clutters models and makes cross-field validation difficult).

### 3. Course Approval Workflow
- **Decision**: Admin Approval Required.
- **Rationale**: Ensures high content quality and prevents inappropriate or incomplete courses from being published directly by instructors.
- **Alternatives considered**: Direct Publishing (rejected due to lack of quality control).

### 4. Bulk Update Mechanism (Reordering)
- **Decision**: Entity Framework `ExecuteUpdateAsync` or a database transaction wrapping multiple updates.
- **Rationale**: Ensures atomicity. Reordering operations can easily touch dozens of records, and a transaction guarantees that if one fails, all fail, preventing corrupted ordering states.
- **Alternatives considered**: Sequential API calls from the client (rejected due to excessive network overhead and potential race conditions).
