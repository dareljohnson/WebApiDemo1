# Product Requirements: TodoApp Solution

## Overview
TodoApp is a .NET Framework 4.8.1 solution for managing Todo items, designed with a layered architecture and robust test coverage. The following requirements reflect all features proposed and implemented as of August 27, 2025.

## Functional Requirements

### 1. Todo Management
- Users can create, read, update, and delete Todo items.
- Each Todo item includes:
  - Title (required, max 200 chars)
  - Description (optional, max 1000 chars)
  - Priority (None, Low, Medium, High)
  - Completion status (IsCompleted)
  - CreatedDate (auto-set)
  - CompletedDate (optional)


### 2. API Layer
- RESTful endpoints for all CRUD operations on Todo items.
- Returns appropriate HTTP status codes and error messages.

- Implements OWASP security best practices:
  - Global security headers (X-Content-Type-Options, X-Frame-Options, X-XSS-Protection, Referrer-Policy, etc.)
  - Custom message handler for security headers
  - Unit tests for security headers
- Provides interactive API documentation:
  - **Swagger/OpenAPI** documentation and live testing via Swagger UI (`/swagger`) powered by Swashbuckle
  - Built-in HelpPage at `/help` for auto-generated docs

### 3. Service Layer
- Business logic for:
  - Validating Todo item data
  - Calculating statistics (e.g., completed vs. pending)
  - Handling edge cases and errors

### 4. Repository Layer
- Generic repository for CRUD operations.
- Todo-specific repository for advanced queries:
  - Get by title (partial match)
  - Get by priority
  - Get completed/pending items
- All data access via Entity Framework 6.

### 5. Dependency Injection
- All controllers, services, and repositories are resolved via Unity.

### 6. Logging
- All errors and key activities are logged using log4net.
- Log output is configurable (file, console, etc.).


### 7. Test Infrastructure
- MSTest for all unit and integration tests.
- Moq for mocking dependencies in service tests.
- Repository tests use LocalDB and TestTodoContext for integration.
- Security handler unit tests for header presence
- SQL script provided to clear test database.

### 8. Configuration
- All connection strings and provider settings in App.config/Web.config.
- Test database is isolated from production.
- log4net and Unity configuration files included.

## Non-Functional Requirements
- Solution builds and runs on .NET Framework 4.8.1.
- All dependencies managed via NuGet.
- Tests must be runnable via CLI (`dotnet test`) and Visual Studio Test Explorer.
- No hardcoded connection strings in code.
- All business logic covered by unit or integration tests.
- No SQL datetime errors in test or production runs (test DB can be cleared with provided script).

## Out of Scope / Future Work
- UI/web frontend (current solution is API and backend only)
- Authentication/authorization
- (None) [Swagger/OpenAPI documentation is now implemented]
- Advanced reporting or analytics

---
This document is up to date as of August 27, 2025. For implementation details, see the README and inline code comments.
