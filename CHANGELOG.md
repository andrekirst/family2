# Changelog

All notable changes to the Family Hub project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Family creation feature (#15) - 2025-12-30
  - GraphQL mutation `createFamily` for authenticated users
  - Business rule: One family per user enforced
  - Complete unit and integration test coverage (94% - 48/51 tests passing)
  - Repository pattern with EF Core and PostgreSQL
  - CQRS command/query handlers with MediatR
  - Source-generated logging for zero-allocation performance

### Technical Implementation

- **Domain Layer:**
  - `IFamilyRepository` interface
  - `Family` aggregate root with factory methods
  - `UserFamily` entity for family membership
  - Vogen value objects: `FamilyId`, `FamilyName`

- **Application Layer:**
  - `CreateFamilyCommandHandler` with FluentValidation
  - `GetUserFamiliesQueryHandler` with source-generated logging
  - Business rule validation (one family per user)

- **Presentation Layer (GraphQL):**
  - `CreateFamilyInput` (primitive types)
  - `CreateFamilyPayload` with error handling
  - `FamilyType` and `UserFamilyType` GraphQL types
  - `FamilyMutations` with authentication via ICurrentUserService

- **Testing:**
  - Unit tests: 36 tests (FamilyTests + CreateFamilyCommandHandlerTests)
  - Integration tests: 15 tests (7 command + 8 GraphQL API tests)
  - Test patterns: FluentAssertions + AutoFixture + NSubstitute
  - Coverage: 94% (48/51 tests passing, 2 intentionally skipped)

### Infrastructure

- PostgreSQL 16 with Row-Level Security (RLS)
- EF Core Code-First migrations
- Zitadel OAuth 2.0 authentication
- HotChocolate GraphQL 14.1.0
- .NET 10 / C# 14

### Documentation

- ADR-003: GraphQL Input → Command mapping pattern
- Code review completed with "Approved with Comments" status
- XML documentation for all public APIs
- Inline test documentation for mocking patterns

## [0.1.0-alpha] - 2025-12-30

### Project Setup

- Initial project structure (Modular Monolith architecture)
- Phase 1 Preparation completed (Issues #4-11)
- OAuth integration with Zitadel (ADR-002)
- 51 core documentation files (280,000+ words)
- Technology stack finalized (ADR-001)

---

**Note:** This changelog tracks feature completion, bugfixes, and infrastructure changes. For detailed implementation notes, see individual issue discussions and ADRs.
