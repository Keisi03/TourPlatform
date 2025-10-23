# TourPlatform Backend Architecture
Architecture Overview

The TourPlatform backend follows Clean Architecture principles with a layered approach to maintain separation of concerns, testability, and scalability.

Layers & Responsibilities
1. API Layer (TourPlatform.API)

Responsibilities:

Handles HTTP requests and responses.

Applies JWT authentication and role-based authorization.

Provides Swagger/OpenAPI documentation.

Hosts SignalR hub for real-time progress updates.

Patterns Used:

Adapter: Controllers translate HTTP requests to Application layer calls.


2. Application Layer (TourPlatform.Application)

Responsibilities:

Orchestrates business logic and workflows.

Validates input, calls repositories or domain services.

Prepares DTOs for API responses.

Patterns Used:

Service Layer: e.g., AuthService, FileProcessorService.

Repository Interface (Contract): IPricingRepository, IRouteRepository.

DTO (Data Transfer Object): Decouples API contract from domain entities.

Adapter: Consumes infrastructure implementations via interfaces (IJwtTokenService, repositories).

Producer-Consumer / Channel: Asynchronous batch CSV processing.

Task-based Concurrency / Throttling: Limits concurrent batch processing with configurable number of worker tasks.

Singleton / Scoped DI: JWT service as singleton, AuthService as scoped.

3. Domain Layer (TourPlatform.Domain)

Responsibilities:

Contains entities, core business rules, and invariants.

Patterns Used:

Entities: User, Pricingrecord, Route, Season, Touroperator, Uploadhistory.

Business rules could live here (e.g., validation, roles).

4. Infrastructure Layer (TourPlatform.Infrastructure)

Responsibilities:

Provides concrete implementations for repositories, EF Core DbContext, Redis caching, JWT handling, SignalR.

Patterns Used:

Repository Implementation: EF Core CRUD + bulk insert.

Singleton / Scoped DI: Redis connection as singleton, JWT service as singleton.

Unit of Work: EF Core SaveChangesAsync ensures atomic operations.

Background Worker: Handles large CSV uploads in parallel without memory overload.

5. External Dependencies

PostgreSQL: Stores all domain entities.

Redis: Caching and token revocation.

SignalR Hub: Real-time upload progress notifications.

# Architecture Diagram
<img width="871" height="646" alt="image" src="https://github.com/user-attachments/assets/06c52104-9b13-47d3-b280-dac17363b8d1" />
