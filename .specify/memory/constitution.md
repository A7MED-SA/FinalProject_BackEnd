<!-- SYNC IMPACT REPORT
Version change: 1.0.0 → 1.1.0
Modified principles: 
  - [PRINCIPLE_1_NAME] → Clean Code Foundations
  - [PRINCIPLE_2_NAME] → C# & .NET Best Practices
  - [PRINCIPLE_3_NAME] → Naming & Formatting
  - [PRINCIPLE_4_NAME] → Structure
  - [PRINCIPLE_5_NAME] → Error Handling
Added sections: None
Removed sections: Additional Constraints, Development Workflow
Templates requiring updates: 
  - .specify/templates/plan-template.md (⚠ pending)
  - .specify/templates/spec-template.md (⚠ pending)
  - .specify/templates/tasks-template.md (⚠ pending)
Follow-up TODOs: None
-->
# Learning Management System Constitution

## Core Principles

### I. Clean Code Foundations
Enforce SOLID principles, DRY (Don't Repeat Yourself), and KISS (Keep It Simple, Stupid).
- **Rationale**: Strictly prevent over-engineering to ensure maintainability and readability. Code should be as simple as possible while meeting requirements.

### II. C# & .NET Best Practices
Mandate the use of built-in Dependency Injection, proper async/await mechanisms, and modern C# features.
- **Rationale**: Utilizing built-in framework features like DI and avoiding blocking calls (`.Result` or `.Wait()`) ensures scalability and prevents deadlocks.

### III. Naming & Formatting
Require intention-revealing names and follow Microsoft's standard C# conventions.
- **Rationale**: Code is read more often than written. PascalCase must be used for classes, records, and methods. Interfaces must have an "I" prefix. Local variables must use camelCase.

### IV. Structure
Keep classes small and focused on a Single Responsibility.
- **Rationale**: Isolating core business logic from framework-specific details (e.g., using layered architecture or Clean Architecture principles) makes the system easier to test and modify without breaking unrelated components.

### V. Error Handling
Enforce structured and centralized exception handling.
- **Rationale**: Silent failures and empty catch blocks hide bugs and make debugging impossible. All exceptions should be logged properly and handled at an appropriate centralized boundary (e.g., global exception handler middleware).

## Governance

- **Review Process**: All Pull Requests must verify compliance with the Core Principles outlined above.
- **Enforcement**: Code complexity must be justified during review. Refactoring to simplify code is always encouraged.
- **Amendments**: Amendments require updating this document and bumping the version according to semantic versioning.

**Version**: 1.1.0 | **Ratified**: 2026-04-19 | **Last Amended**: 2026-04-19
