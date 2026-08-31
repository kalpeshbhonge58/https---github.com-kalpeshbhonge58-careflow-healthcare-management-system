# CareFlow: Phase 2 Preparation Guide
## Database & EF Core Migrations

**Previous Phase**: Phase 1 - Architecture & Setup ✅ **COMPLETE**  
**Current Phase**: Phase 2 - Database & EF Core Migrations (Ready to Start)  
**Date**: August 31, 2026

---

## Phase 2 Overview

Phase 2 focuses on:
1. Creating Entity Framework Core migrations
2. Applying migrations to SQL Server
3. Verifying database schema
4. Testing and documenting the database design
5. Creating ER diagram documentation

**Expected Duration**: 1-2 hours  
**Outcome**: Working database with seeded sample data

---

## Prerequisites

✅ All completed in Phase 1:
- .NET 8 SDK installed
- SQL Server or SQL Server Express installed
- LocalDB available (included with Visual Studio)
- All NuGet packages restored
- Project structure complete
- Entities and configurations defined

---

## What Will Be Accomplished

### 1. Create Initial Migration
The migration captures the database schema from the entity models and configurations.

**Command**:
```bash
cd "c:\Users\kalpe\Desktop\Angular Learning Tutorials\CareFlow Healthcare Management System"
dotnet ef migrations add InitialCreate -p src/CareFlow.Infrastructure -s src/CareFlow.Api -v
```

**Expected Result**:
- New file: `src/CareFlow.Infrastructure/Migrations/[timestamp]_InitialCreate.cs`
- Migration file contains schema creation (Up) and rollback (Down) logic
- Migrations folder has `ApplicationDbContextModelSnapshot.cs`

### 2. Apply Migration to Database
The migration script updates the database with the schema.

**Command**:
```bash
dotnet ef database update -p src/CareFlow.Infrastructure -s src/CareFlow.Api
```

**Expected Result**:
- CareFlowDb database created on LocalDB
- All 16 tables created
- Foreign keys and constraints applied
- Indexes created

### 3. Seed Sample Data
Automatic seeding happens when API starts or can be triggered manually.

**Option A - Via API Startup**:
```bash
cd src/CareFlow.Api
dotnet run
```

**Option B - Programmatic**:
```bash
# Via C# code in Program.cs (already implemented)
await app.Services.SeedDatabaseAsync();
```

**Expected Result**:
- Database populated with sample data
- 11 users (admin, receptionists, doctors, patients)
- 5 departments
- 10 medicines
- 10 appointments
- 2 medical records
- 2 prescriptions
- 4 invoices

### 4. Verify Database Schema

**Via SQL Server Management Studio**:
```sql
-- List all tables
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA = 'dbo'
ORDER BY TABLE_NAME;

-- Check Users table structure
EXEC sp_help 'Users';

-- Verify relationships
SELECT * 
FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE 
WHERE TABLE_SCHEMA = 'dbo';
```

**Expected Tables**:
1. Users
2. Roles
3. UserRoles
4. Patients
5. Doctors
6. Departments
7. Appointments
8. MedicalRecords
9. Medicines
10. Prescriptions
11. PrescriptionItems
12. Invoices
13. InvoiceItems
14. Payments
15. Notifications
16. AuditLogs

### 5. Verify Sample Data

```sql
-- Check seeded data
SELECT COUNT(*) as UserCount FROM Users;
SELECT COUNT(*) as RoleCount FROM Roles;
SELECT COUNT(*) as AppointmentCount FROM Appointments;
SELECT COUNT(*) as InvoiceCount FROM Invoices;

-- Sample query
SELECT 
    u.FirstName + ' ' + u.LastName as PatientName,
    p.PatientNumber,
    a.AppointmentDate,
    d.Name as DoctorName
FROM Appointments a
JOIN Patients p ON a.PatientId = p.Id
JOIN Users u ON p.UserId = u.Id
JOIN Doctors doc ON a.DoctorId = doc.Id
JOIN Users du ON doc.UserId = du.Id
JOIN Doctors d ON doc.DepartmentId = d.DepartmentId
ORDER BY a.AppointmentDate;
```

---

## Step-by-Step Instructions

### Step 1: Verify Prerequisites

**Check .NET Version**:
```bash
dotnet --version
# Expected: 8.x.x
```

**Check EF Core Tools**:
```bash
dotnet ef --version
# Expected: version 8.x.x
```

**If EF Core Tools Not Installed**:
```bash
dotnet tool install --global dotnet-ef
```

### Step 2: Create Migration

```bash
# Navigate to project directory
cd "c:\Users\kalpe\Desktop\Angular Learning Tutorials\CareFlow Healthcare Management System"

# Create migration with verbose output
dotnet ef migrations add InitialCreate `
  -p src/CareFlow.Infrastructure `
  -s src/CareFlow.Api `
  -v
```

**What This Does**:
- Analyzes all entity models
- Analyzes all configurations
- Creates migration file with SQL schema
- Compares against existing migrations
- Shows detailed output

**Expected Output**:
```
Done. To undo this action, use 'dotnet ef migrations remove'
```

### Step 3: Apply Migration

```bash
dotnet ef database update `
  -p src/CareFlow.Infrastructure `
  -s src/CareFlow.Api `
  -v
```

**What This Does**:
- Executes the migration script
- Creates database if not exists
- Creates all tables
- Creates all constraints and indexes
- Updates migration history table

**Expected Output**:
```
Done.
```

### Step 4: Verify in SQL Server

**Option A - SQL Server Management Studio**:
1. Connect to: `(localdb)\mssqllocaldb`
2. Expand Databases
3. Look for `CareFlowDb`
4. Expand Tables
5. Should see 16 tables

**Option B - Azure Data Studio**:
1. Create new connection to `(localdb)\mssqllocaldb`
2. Browse databases
3. Select CareFlowDb
4. Expand Tables

**Option C - Command Line**:
```sql
-- Query via sqlcmd
sqlcmd -S "(localdb)\mssqllocaldb" -Q "SELECT name FROM sys.databases;"
```

### Step 5: Run API to Seed Data

```bash
cd src/CareFlow.Api
dotnet run
```

**Expected Output**:
- Application starts
- Logs show: "Seeding CareFlow database..."
- If already seeded: "Database already seeded. Skipping seed operation."
- API available at: `https://localhost:7123`
- Swagger UI: `https://localhost:7123/swagger`

### Step 6: Test Seeded Data

**Via SQL Query**:
```sql
USE CareFlowDb;

-- Verify users
SELECT COUNT(*) as UserCount FROM Users;

-- Verify appointments
SELECT COUNT(*) as AppointmentCount FROM Appointments;

-- Sample patient
SELECT u.FirstName, u.LastName, u.Email, p.PatientNumber
FROM Users u
LEFT JOIN Patients p ON u.Id = p.UserId
WHERE p.Id IS NOT NULL
LIMIT 1;
```

---

## Database Schema Documentation

### Core Entities

#### Users Table
```sql
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Email NVARCHAR(256) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(256) NOT NULL,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    PhoneNumber NVARCHAR(20),
    LastLoginAt DATETIME2,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2
)
```

#### Patients Table
```sql
CREATE TABLE Patients (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL UNIQUE,
    PatientNumber NVARCHAR(20) NOT NULL UNIQUE,
    DateOfBirth DATE NOT NULL,
    Gender NVARCHAR(20) NOT NULL,
    BloodGroup NVARCHAR(10),
    Address NVARCHAR(500),
    EmergencyContactName NVARCHAR(100),
    EmergencyContactPhone NVARCHAR(20),
    MedicalHistory NVARCHAR(MAX),
    Allergies NVARCHAR(MAX),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
)
```

#### Appointments Table
```sql
CREATE TABLE Appointments (
    Id INT PRIMARY KEY IDENTITY(1,1),
    PatientId INT NOT NULL,
    DoctorId INT NOT NULL,
    AppointmentDate DATE NOT NULL,
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    Reason NVARCHAR(500),
    CancellationReason NVARCHAR(500),
    CancelledAt DATETIME2,
    CompletedAt DATETIME2,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    FOREIGN KEY (PatientId) REFERENCES Patients(Id),
    FOREIGN KEY (DoctorId) REFERENCES Doctors(Id)
)
```

### Relationships

```
Users (1:M) UserRoles
  │
  ├─→ (1:1) Patients
  │         │
  │         ├─→ (1:M) Appointments
  │         ├─→ (1:M) MedicalRecords
  │         ├─→ (1:M) Prescriptions
  │         └─→ (1:M) Invoices
  │
  └─→ (1:1) Doctors
            │
            ├─→ (M:1) Departments
            ├─→ (1:M) Appointments
            └─→ (1:M) MedicalRecords

Prescriptions (1:M) PrescriptionItems → (M:1) Medicines
Invoices (1:M) InvoiceItems
Invoices (1:M) Payments
```

---

## Verification Checklist

### Database Creation
- [ ] LocalDB instance exists
- [ ] CareFlowDb database created
- [ ] Can connect with SSMS or ADS

### Migration
- [ ] InitialCreate migration file exists
- [ ] Migration compiled without errors
- [ ] Migration applied to database

### Schema
- [ ] All 16 tables created
- [ ] All columns present with correct types
- [ ] All primary keys created
- [ ] All foreign keys created
- [ ] All unique constraints present
- [ ] All NOT NULL constraints present

### Sample Data
- [ ] Users table has 11 rows
- [ ] Roles table has 4 rows
- [ ] UserRoles table has 11 rows
- [ ] Patients table has 5 rows
- [ ] Doctors table has 3 rows
- [ ] Departments table has 5 rows
- [ ] Medicines table has 10 rows
- [ ] Appointments table has 10 rows

### API Integration
- [ ] API starts without errors
- [ ] Database seeding completes
- [ ] Swagger UI loads
- [ ] No connection errors in logs

---

## Common Issues & Solutions

### Issue: Migration Tool Not Found
**Error**: "dotnet ef: command not found"

**Solution**:
```bash
dotnet tool install --global dotnet-ef
dotnet ef --version
```

### Issue: Database Already Exists
**Error**: "Cannot create database - already exists"

**Solution A** - Drop and recreate:
```bash
dotnet ef database drop --force
dotnet ef database update
```

**Solution B** - Delete migrations and start over:
```bash
# Remove migration
dotnet ef migrations remove

# Remove database manually via SSMS

# Start fresh
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Issue: Connection String Error
**Error**: "Cannot connect to (localdb)\mssqllocaldb"

**Solution**:
```bash
# Check LocalDB version
sqllocaldb info

# Ensure instance is running
sqllocaldb start mssqllocaldb
```

### Issue: Foreign Key Constraint Violation
**Error**: "The INSERT, UPDATE, or DELETE statement conflicted with a FOREIGN KEY constraint"

**Solution**:
```bash
# Drop all data and recreate
dotnet ef database drop --force
dotnet ef database update
# This will reseed data
```

---

## Success Criteria

All of these must be true:

✅ **Migration Created**
- InitialCreate.cs file exists
- No compilation errors
- Migration history tracked

✅ **Database Created**
- CareFlowDb exists on (localdb)\mssqllocaldb
- Can connect with SSMS/ADS
- All 16 tables present

✅ **Schema Valid**
- All columns correct
- All relationships correct
- Constraints properly defined

✅ **Data Seeded**
- Sample data populated automatically
- No seed errors in logs
- Data matches expected counts

✅ **API Working**
- Starts without database errors
- Swagger accessible
- No connection warnings

---

## Next Phase Preparation

Once Phase 2 is complete, Phase 3 (Authentication) will implement:

1. **Login Endpoint** (`POST /api/auth/login`)
   - Accept email and password
   - Return JWT token

2. **Token Refresh** (`POST /api/auth/refresh`)
   - Accept refresh token
   - Return new JWT token

3. **Logout** (`POST /api/auth/logout`)
   - Invalidate token

4. **Authorization Filters**
   - [Authorize] attribute
   - [Authorize(Roles = "Admin")]
   - Policy-based authorization

5. **Current User Service**
   - Extract user from JWT claims
   - Make available in services

---

## Commands Reference

### Create Migration
```bash
dotnet ef migrations add {MigrationName} -p {ProjectPath} -s {StartupProjectPath} -v
```

**Example**:
```bash
dotnet ef migrations add InitialCreate -p src/CareFlow.Infrastructure -s src/CareFlow.Api -v
```

### Apply Migration
```bash
dotnet ef database update -p {ProjectPath} -s {StartupProjectPath} -v
```

### Remove Last Migration
```bash
dotnet ef migrations remove -p {ProjectPath} -s {StartupProjectPath}
```

### List Migrations
```bash
dotnet ef migrations list -p {ProjectPath} -s {StartupProjectPath}
```

### Generate SQL Script
```bash
dotnet ef migrations script -p {ProjectPath} -s {StartupProjectPath} -o migrations.sql
```

### Drop Database
```bash
dotnet ef database drop -p {ProjectPath} -s {StartupProjectPath}
```

---

## Documentation to Create After Phase 2

1. **Database ER Diagram**
   - Create using Lucidchart or draw.io
   - Show all 16 tables
   - Show relationships
   - Include cardinalities

2. **Schema Documentation**
   - Document each table
   - Document each column
   - Document constraints
   - Document indexes

3. **Sample Query Examples**
   - Joins across multiple tables
   - Aggregations (COUNT, SUM, AVG)
   - Complex filters

4. **Migration History**
   - Track all migrations
   - Document schema changes
   - Versioning strategy

---

## Estimated Time

| Task | Time |
|------|------|
| Create migration | 5 min |
| Apply migration | 2 min |
| Verify schema | 10 min |
| Test seeding | 5 min |
| Document schema | 20 min |
| **Total** | **~45 minutes** |

---

## When Phase 2 is Complete

You can proceed to **Phase 3: Authentication & Authorization**

```
Phase 1 ✅ Architecture & Setup
    ↓
Phase 2 ✅ Database & EF Core [CURRENT]
    ↓
Phase 3 → Authentication & Authorization
    ↓
Phase 4 → Patient, Doctor, Department CRUD APIs
    ↓
Phase 5 → Appointment Module
    ↓
... (more phases)
```

---

## Ready to Start Phase 2?

### Prerequisites Check
```bash
# Verify all prerequisites
dotnet --version              # Should be 8.x.x
dotnet ef --version           # Should be 8.x.x
dotnet build                  # Should succeed
```

### When Ready, Run
```bash
cd "c:\Users\kalpe\Desktop\Angular Learning Tutorials\CareFlow Healthcare Management System"

# Create migration
dotnet ef migrations add InitialCreate -p src/CareFlow.Infrastructure -s src/CareFlow.Api -v

# Apply to database
dotnet ef database update -p src/CareFlow.Infrastructure -s src/CareFlow.Api

# Verify it worked
cd src/CareFlow.Api
dotnet run
```

---

**Next Step**: Start Phase 2 by running the migration commands above.

For detailed information about Phase 1, see: `/docs/PHASE-1-COMPLETION.md`
