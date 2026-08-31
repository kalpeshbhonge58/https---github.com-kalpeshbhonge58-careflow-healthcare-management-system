# CareFlow Development Log

This document tracks major milestones, implementations, and architectural decisions made during CareFlow development.

## Phase 1: Architecture & Setup ✅ COMPLETE

**Completed**: August 31, 2026

### What Was Implemented

#### 1. Solution Structure
- Created CareFlow.sln with proper folder organization
- Organized projects into `/src`, `/tests`, and `/client` directories
- Projects: Domain, Application, Infrastructure, Shared, API, MVC, UnitTests, IntegrationTests

#### 2. Domain Layer (CareFlow.Domain)
Implemented 16 core domain entities:
- **User Management**: User, Role, UserRole
- **Healthcare Core**: Patient, Doctor, Department, Appointment
- **Medical**: MedicalRecord, Prescription, PrescriptionItem, Medicine
- **Billing**: Invoice, InvoiceItem, Payment
- **System**: Notification, AuditLog

**Design Patterns Used**:
- Base Entity pattern with Id, CreatedAt, UpdatedAt
- Enums for statuses (AppointmentStatus, PaymentStatus, Gender)
- Relationships: 1:N, N:M using ICollection

#### 3. Application Layer (CareFlow.Application)
Implemented 11 core services:
- AuthService - Authentication logic
- PatientService - Patient management
- DoctorService - Doctor management
- AppointmentService - Appointment booking and management
- MedicalRecordService - Medical records
- PrescriptionService - Prescriptions
- InvoiceService - Billing
- DashboardService - Dashboard aggregation
- DepartmentService - Department management
- MedicineService - Medicine catalog
- AuditService - Audit logging

**Supporting Infrastructure**:
- AutoMapper profiles for Entity ↔ DTO mapping
- FluentValidation for request validation
- Interfaces for dependency injection
- Request/Response DTOs for all entities

#### 4. Infrastructure Layer (CareFlow.Infrastructure)
Implemented data access layer:
- **Database**: ApplicationDbContext with 16 DbSets
- **Entity Configurations**: Fluent API configurations for all entities
- **Repository Pattern**:
  - Generic Repository<T> with CRUD operations
  - Specialized repositories for complex queries
  - UnitOfWork for transaction management
- **Services**:
  - PasswordHasher - BCrypt hashing
  - JwtTokenService - JWT token generation/validation
  - CacheService - Memory caching
  - ResendNotificationService - Email notifications
  - CurrentUserService - User context extraction
- **Database Seeding**:
  - DbSeeder with realistic sample data
  - 4 roles, 11 users, 5 departments
  - 10 medicines, 10 appointments, sample invoices

#### 5. API Layer (CareFlow.Api)
Implemented REST API foundation:
- **Program.cs Configuration**:
  - Dependency Injection setup
  - Database context configuration
  - Serilog structured logging
  - JWT authentication with Bearer scheme
  - Authorization policies (AdminOnly, DoctorOnly, etc.)
  - CORS configuration for Angular
  - Swagger/OpenAPI documentation
  - Database seeding on startup
- **Middleware**: ExceptionHandlingMiddleware for global error handling
- **Security**: JWT token validation, authorization checks

#### 6. Shared Layer (CareFlow.Shared)
Created shared constants:
- Role constants (Admin, Doctor, Receptionist, Patient)
- JWT settings configuration

#### 7. Testing Infrastructure
Set up testing projects:
- CareFlow.UnitTests (xUnit)
- CareFlow.IntegrationTests

### Technologies Implemented

**Backend Stack**:
- .NET 8.0
- ASP.NET Core Web API
- Entity Framework Core
- LINQ
- AutoMapper
- FluentValidation
- Serilog (structured logging)
- JWT (System.IdentityModel.Tokens.Jwt)
- BCrypt (password hashing)
- SQL Server with LocalDB

**Architecture Patterns**:
- Layered Architecture
- Repository Pattern
- Service Layer
- Dependency Injection
- Unit of Work
- DTO Pattern

**Design Principles**:
- SOLID
- DRY
- KISS
- Clean Code

### Build Verification
```
✅ CareFlow.Domain built successfully
✅ CareFlow.Application built successfully
✅ CareFlow.Infrastructure built successfully
✅ CareFlow.Shared built successfully
✅ CareFlow.Api built successfully
✅ CareFlow.Mvc built successfully
✅ CareFlow.UnitTests built successfully
✅ CareFlow.IntegrationTests built successfully

Total: 0 Errors, 0 Warnings
Build Time: 8.43 seconds
```

### Key Architectural Decisions

**1. Generic Repository with Specialized Repositories**
- *Why*: Reduces boilerplate for simple CRUD, allows complex domain queries
- *Trade-off*: Small complexity increase for significant flexibility

**2. Service Layer Abstraction**
- *Why*: Centralizes business logic, improves testability
- *Example*: AppointmentService enforces business rules before repository access

**3. Unit of Work Pattern**
- *Why*: Ensures transactional consistency across multiple repositories
- *Benefit*: Single SaveChangesAsync() for multiple operations

**4. DTOs Over Entity Exposure**
- *Why*: Decouples API from database, improves API stability
- *Example*: PatientResponse doesn't expose PasswordHash

**5. LINQ for Database Queries**
- *Why*: Type-safe, composable, efficient database execution
- *Example*: AppointmentRepository uses complex LINQ queries

**6. JWT Bearer Authentication**
- *Why*: Stateless, scalable, ideal for APIs and SPAs
- *Example*: Angular interceptor will attach JWT to requests

**7. Serilog Structured Logging**
- *Why*: Better diagnostics, production-ready logging
- *Output*: Console + rolling file logs

### Database Design Highlights

**Core Tables**:
- Users (authentication)
- Roles (authorization)
- UserRoles (N:M relationship)
- Patients (patient records)
- Doctors (doctor records)
- Departments (organization)
- Appointments (scheduling)
- MedicalRecords (clinical data)
- Prescriptions (medicine orders)
- Invoices (billing)

**Relationships**:
- 1 User : 1 Patient (optional)
- 1 User : 1 Doctor (optional)
- M Doctors : 1 Department
- M Patients : M Doctors (through Appointments)
- 1 Appointment : 1 MedicalRecord (optional)
- 1 Prescription : M PrescriptionItems
- 1 Invoice : M InvoiceItems

**Constraints**:
- Primary keys on all entities
- Foreign keys with cascade delete
- Unique constraints on Email, PatientNumber, LicenseNumber
- NOT NULL on required fields
- Decimal(18,2) for financial values

### Sample Data Included

**Users**:
- 1 Admin (admin@careflow.com)
- 2 Receptionists
- 3 Doctors (Cardiology, Neurology, Orthopedics)
- 5 Patients

**Resources**:
- 5 Departments
- 10 Medicines
- 10 Appointments (various statuses)
- 4 Invoices (various payment statuses)
- 2 Prescriptions with medicine items

**Default Credentials** (for demo):
- Admin: admin@careflow.com / Admin@123
- Receptionist: reception1@careflow.com / Reception@123
- Doctor: dr.smith@careflow.com / Doctor@123
- Patient: john.doe@email.com / Patient@123

### Dependency Injection Setup

**Pattern**: Extension methods on IServiceCollection

**Application Layer** (`AddApplication()`):
```csharp
services.AddAutoMapper(MappingProfile)
services.AddScoped<IAuthService>()
services.AddScoped<IPatientService>()
// ... more services
```

**Infrastructure Layer** (`AddInfrastructure(Configuration)`):
```csharp
services.AddDbContext<ApplicationDbContext>()
services.AddScoped<IRepository<>>()
services.AddScoped<IUnitOfWork>()
services.AddSingleton<IPasswordHasher>()
services.AddScoped<IJwtTokenService>()
services.AddSingleton<ICacheService>()
// ... more infrastructure services
```

**Database Seeding**:
```csharp
// Extension method on IServiceProvider
services.SeedDatabaseAsync()
// Called in Program.cs after app.Build()
```

### Security Measures Implemented

✅ **Authentication**:
- JWT Bearer tokens
- Token expiration (configurable)
- Secure validation

✅ **Authorization**:
- Role-based policies (Admin, Doctor, Receptionist, Patient)
- Policy-based access control
- Claims-based permissions

✅ **Password Security**:
- BCrypt hashing (salted)
- Never store plain text passwords
- Configurable cost factor

✅ **API Security**:
- CORS configured for frontend
- HTTPS redirection
- JWT validation on protected endpoints

✅ **Configuration Security**:
- Secrets in user secrets (not appsettings)
- Environment-based configuration
- No hardcoded credentials

### What's NOT Implemented Yet

Intentionally deferred to later phases:
- ❌ API Controllers (Phase 4)
- ❌ Angular Components (Phase 9)
- ❌ MVC Views & Admin Reporting (Phase 10)
- ❌ Third-party API Integration (Phase 11)
- ❌ Unit Tests (Phase 12)

## Angular JWT Authentication ✅ COMPLETE

**Completed**: August 31, 2026

### Authentication Flow
1. `LoginComponent` validates the reactive form and calls `AuthService.login()`.
2. `AuthService` posts to the actual `POST /api/auth/login` endpoint.
3. The API returns `ApiResponse<LoginResponse>` containing `token`, `expiresAt`, and `user`.
4. The service stores the token, expiration, and typed user profile in `localStorage` and publishes the user through a `BehaviorSubject`.
5. `authInterceptor` adds `Authorization: Bearer <token>` to protected requests.

### Guards and Role Authorization
- `authGuard` checks the live `AuthService` state and redirects unauthenticated users to `/login`.
- `roleGuard` reads `data.roles` from route configuration and compares it with the authenticated user's roles.
- Angular guards provide navigation UX only; ASP.NET Core remains responsible for actual authorization.

### Error Handling
- `401 Unauthorized`: stored authentication is cleared and the user is redirected to `/login`.
- `403 Forbidden`: the user remains signed in and is sent to `/unauthorized`.
- Login errors are mapped to friendly messages instead of displaying raw backend exceptions.

### Security Considerations
- Login requests are explicitly excluded from authorization header injection.
- The interceptor clones immutable requests and never logs tokens or passwords.
- Tokens are checked for JWT expiration before being reused.
- The development API URL is configured in `src/environments/environment.ts`; production should use environment-specific deployment configuration.
- ❌ Integration Tests (Phase 12)
- ❌ Advanced Caching (Phase 8)
- ❌ Complete Audit Logging (Phase 8)

---

## Next Phase: Phase 2 - Database & EF Core Migrations

**Planned Activities**:
1. Create initial EF Core migration
2. Apply migrations to SQL Server
3. Verify database schema
4. Test database seeding
5. Document schema
6. Create ER diagram

**Expected Outcomes**:
- Working database schema
- Migrations stored in version control
- Seed data verified in database
- Database documentation complete

---

## Interview Preparation Summary

### Key Points to Mention

1. **Architecture Excellence**
   - Clean layered architecture with clear separation of concerns
   - Repository Pattern for data access abstraction
   - Service Layer for business logic
   - Dependency Injection for testability

2. **Entity Relationships**
   - Healthcare domain modeling
   - Complex entity relationships (1:N, N:M)
   - Proper use of foreign keys and constraints

3. **Technology Choices**
   - Why .NET 8 and ASP.NET Core
   - Why EF Core over alternatives
   - Why JWT for authentication
   - Why BCrypt for passwords

4. **Security Implementation**
   - Authentication mechanism
   - Authorization policies
   - Password hashing strategy
   - API security measures

5. **Code Quality**
   - SOLID principles
   - Clean code practices
   - No code duplication
   - Meaningful exception handling

6. **Database Design**
   - Proper normalization
   - Relationship modeling
   - Constraint definitions
   - Indexing strategy

### Potential Interview Questions

1. "Why did you use the Repository Pattern?"
   - Answer: Decouples business logic from data access, improves testability, allows changing data source without affecting business logic.

2. "What's the advantage of the Unit of Work pattern?"
   - Answer: Ensures transactional consistency across multiple repository operations, single transaction scope for related changes.

3. "Why use DTOs instead of entities?"
   - Answer: Decouples API contract from database schema, provides security (hide fields), enables flexible API design.

4. "How do you handle database transactions?"
   - Answer: Unit of Work pattern with single SaveChangesAsync() call, or explicit transaction scopes.

5. "Why JWT authentication?"
   - Answer: Stateless, scalable, works well with SPAs, no server-side session storage required.

6. "How is the database configured?"
   - Answer: Entity Framework Core with Fluent API, supporting SQL Server, migrations tracked in code.

---

**END OF DEVELOPMENT LOG - PHASE 1**
