# 10-code-rules.md

# 1. Purpose

This document defines coding standards, project structure, naming conventions, architecture rules, and AI generation constraints for OPCBS.

All generated code must comply with this document.

---

# 2. Architecture Rules

## AR-001 Clean Architecture

The system must follow Clean Architecture.

```text
src/

├── OPCBS.API
├── OPCBS.Application
├── OPCBS.Domain
├── OPCBS.Infrastructure
└── OPCBS.Shared
```

Dependencies:

```text
API
 ↓
Application
 ↓
Domain

Infrastructure
 ↓
Application
 ↓
Domain
```

Domain must never depend on any other layer.

---

## AR-002 Layer Responsibilities

### OPCBS.API

Responsible for:

* Controllers
* Authentication
* Middleware
* Swagger
* API Configuration

Must NOT contain:

* Business Logic
* Database Logic

---

### OPCBS.Application

Responsible for:

* Services
* DTOs
* Validators
* Interfaces
* Business Workflows

Must NOT contain:

* EF Core DbContext
* Database Queries

---

### OPCBS.Domain

Responsible for:

* Entities
* Enums
* Constants
* Domain Rules

Must NOT contain:

* Service Logic
* Repository Logic

---

### OPCBS.Infrastructure

Responsible for:

* EF Core
* Repository Implementations
* External Services
* Database Access
* Cloudinary
* Brevo
* VNPay

---

# 3. General Coding Rules

## CR-001 Single Responsibility Principle

Each class must have only one responsibility.

---

## CR-002 Keep Classes Small

Maximum:

```text
500 lines per class
```

---

## CR-003 Keep Methods Small

Maximum:

```text
50 lines per method
```

---

## CR-004 Explicit Naming

Names must clearly describe business purpose.

Good:

```text
ApproveAppointment

CreateVerificationRequest

GetDoctorProfile
```

Bad:

```text
Process

Execute

Handle
```

---

## CR-005 No Duplicate Logic

Business logic must exist only once.

---

## CR-006 No Magic Strings

Bad:

```csharp
if(status == "Approved")
```

Good:

```csharp
AppointmentStatus.Approved
```

---

# 4. Backend Folder Structure

## OPCBS.API

```text
Controllers/

Middlewares/

Configurations/

Extensions/

Program.cs
```

---

## OPCBS.Application

```text
DTOs/

Interfaces/

Services/

Validators/

Mappings/

Features/
```

---

## OPCBS.Domain

```text
Entities/

Enums/

Constants/

Common/
```

---

## OPCBS.Infrastructure

```text
Persistence/

Repositories/

ExternalServices/

Identity/

Configurations/
```

---

# 5. Entity Rules

## Naming

Pattern:

```text
PascalCase
Singular
```

Examples:

```text
User

DoctorProfile

Appointment

ConsultationRecord

TreatmentPackage
```

---

## Entity Requirements

Every Entity must contain:

```csharp
Id

CreatedAt

UpdatedAt

CreatedBy

UpdatedBy
```

Soft Delete Entities:

```csharp
IsDeleted
```

---

# 6. DTO Rules

## Request DTO

Pattern

```text
<Action><Entity>Request
```

Examples

```text
CreateAppointmentRequest

UpdateDoctorProfileRequest

LoginRequest
```

---

## Response DTO

Pattern

```text
<Entity>Response
```

Examples

```text
DoctorResponse

AppointmentResponse

BlogResponse
```

---

# 7. Service Rules

## Interface

Pattern

```text
I<Entity>Service
```

Example

```text
IAppointmentService
```

---

## Implementation

Pattern

```text
<Entity>Service
```

Example

```text
AppointmentService
```

---

## Service Responsibilities

Services may:

* Validate business rules
* Coordinate repositories
* Trigger notifications
* Execute workflows

Services must NOT:

* Return Entity directly
* Access HTTP Context directly
* Contain SQL

---

# 8. Repository Rules

## Interface

Pattern

```text
I<Entity>Repository
```

---

## Implementation

Pattern

```text
<Entity>Repository
```

---

## Responsibilities

Repositories may:

* Execute EF Core queries
* Save data

Repositories must NOT:

* Contain business logic

---

# 9. Controller Rules

## Responsibilities

Controllers only:

```text
Receive Request

Call Service

Return Response
```

---

## Forbidden

```csharp
DbContext

Business Logic

Repository Access
```

inside Controller.

---

# 10. Validation Rules

## FluentValidation Required

Every Create / Update Request must have Validator.

Examples:

```text
RegisterValidator

CreateAppointmentValidator

CreateTreatmentPackageValidator
```

---

## Validation Location

```text
Application/Validators
```

---

# 11. Razor Pages Rules

## Folder Structure

```text
Pages/

Shared/

Components/

wwwroot/

wwwroot/css

wwwroot/js

wwwroot/images
```

---

## Page Naming

Pattern

```text
FeatureName.cshtml

FeatureName.cshtml.cs
```

Examples

```text
Login.cshtml

DoctorProfile.cshtml

AppointmentDetails.cshtml
```

---

## Razor Rules

Business logic must NOT exist inside:

```html
.cshtml
```

Only display logic is allowed.

---

# 12. Database Rules

## Table Naming

Pattern

```text
Plural
PascalCase
```

Examples

```text
Users

Appointments

TreatmentPackages
```

---

## Column Naming

Pattern

```text
PascalCase
```

Examples

```text
DoctorId

CreatedAt

VerificationStatus
```

---

## Foreign Keys

Pattern

```text
<Entity>Id
```

Examples

```text
DoctorId

PatientId

AppointmentId
```

---

# 13. Security Rules

## Authentication

```text
JWT Authentication
```

Required.

---

## Password

```text
BCrypt
```

Required.

Never store:

```text
Plain Text Password
```

---

## Authorization

Every protected endpoint must use:

```text
Role-Based Access Control (RBAC)
```

---

# 14. Logging Rules

Log:

* Authentication Events
* Verification Events
* Appointment Workflow
* Payment Workflow
* Errors

Do NOT log:

* Passwords
* OTP Codes
* Sensitive Medical Notes

---

# 15. External Service Rules

## Brevo

Only through:

```text
IEmailService
```

---

## Cloudinary

Only through:

```text
IFileStorageService
```

---

## VNPay

Only through:

```text
IPaymentService
```

---

# 16. Testing Rules

Minimum:

```text
Unit Test Skeleton
```

for every Service.

Test Naming:

```text
MethodName_ShouldExpectedResult_WhenCondition
```

Example:

```text
ApproveAppointment_ShouldSetApproved_WhenAppointmentExists
```

---

# 17. AI Generation Rules

AI must follow:

1. Scope.md
2. Business Rules.md
3. Domain Model.md
4. Database Design.md
5. System Workflows.md
6. API Design.md
7. Screen Specifications.md
8. Code Rules.md

AI must:

* Generate Interfaces
* Generate DTOs
* Generate Validators
* Generate Repository Layer
* Generate Service Layer
* Generate Unit Test Skeletons

AI must NOT:

* Create entities outside Domain Model
* Create endpoints outside API Design
* Bypass Repository Layer
* Put business logic inside Controllers
* Put business logic inside Razor Pages
* Generate unused code
