# 06-code-rules.md

# 1. General Principles

## GR-001 Single Responsibility Principle

Each class must have only one responsibility.

### Good

```text
AppointmentService
BlogService
DoctorService
```

### Bad

```text
SystemService
CommonService
UtilityService
```

---

## GR-002 Avoid God Classes

No class should exceed:

```text
500 lines
```

If exceeded, split responsibilities.

---

## GR-003 Avoid Duplicate Logic

Business logic must exist in only one place.

---

## GR-004 Explicit Naming

Names must clearly describe purpose.

### Good

```text
ApproveAppointment

CreateDoctorVerificationRequest

GetPatientConsultationHistory
```

### Bad

```text
Process()

Handle()

Execute()
```

---

# 2. Backend Folder Structure

```text
src/

├── Controllers/
│
├── DTOs/
│
├── Models/
│
├── Services/
│
├── Repositories/
│
├── Validators/
│
├── Data/
│
├── Mappings/
│
├── Middlewares/
│
├── Configurations/
│
├── Helpers/
│
└── Program.cs
```

---

# 3. Backend Naming Rules

## Entities

Pattern

```text
Noun
```

Examples

```text
User

DoctorProfile

Appointment

PatientRecord

Blog

Subscription
```

---

## DTOs

### Request

```text
<Action><Entity>Request
```

Examples

```text
CreateAppointmentRequest

UpdateDoctorProfileRequest

LoginRequest
```

### Response

```text
<Entity>Response
```

Examples

```text
AppointmentResponse

DoctorResponse

BlogResponse
```

---

## Services

### Interface

```text
I<Entity>Service
```

Examples

```text
IAppointmentService

IBlogService
```

### Implementation

```text
<Entity>Service
```

Examples

```text
AppointmentService

BlogService
```

---

## Repositories

### Interface

```text
I<Entity>Repository
```

### Implementation

```text
<Entity>Repository
```

Examples

```text
IAppointmentRepository

AppointmentRepository
```

---

## Validators

```text
<Action>Validator
```

Examples

```text
RegisterValidator

CreateAppointmentValidator
```

---

## Controllers

```text
<Entity>Controller
```

Examples

```text
AuthController

DoctorController

AppointmentController
```

---

# 4. Backend Code Rules

## BR-001

Controllers must never access DbContext directly.

### Forbidden

```csharp
_context.Appointments.Add(...)
```

inside Controller.

---

## BR-002

Controllers only:

```text
Receive Request

Call Service

Return Response
```

---

## BR-003

Business rules belong only in Services.

---

## BR-004

Database access belongs only in Repositories.

---

## BR-005

Entity must never be returned directly.

Always use DTO.

---

## BR-006

Every Create/Update Request must have Validator.

---

## BR-007

Every endpoint must have authorization rule.

---

## BR-008

No magic strings.

### Bad

```csharp
if(status == "Approved")
```

### Good

```csharp
AppointmentStatus.Approved
```

---

# 5. Frontend Folder Structure

```text
src/

├── api/
│
├── pages/
│
├── components/
│
├── layouts/
│
├── routes/
│
├── hooks/
│
├── services/
│
├── types/
│
├── constants/
│
├── validations/
│
├── utils/
│
├── assets/
│
└── App.tsx
```

---

# 6. Frontend Naming Rules

## Pages

Pattern

```text
<Entity>Page.tsx
```

Examples

```text
DoctorListPage.tsx

DoctorProfilePage.tsx

AppointmentPage.tsx
```

---

## Components

Pattern

```text
<ComponentName>.tsx
```

Examples

```text
DoctorCard.tsx

AppointmentTable.tsx

PackageCard.tsx
```

---

## Hooks

Pattern

```text
use<Entity>.ts
```

Examples

```text
useAuth.ts

useAppointment.ts

useDoctor.ts
```

---

## Services

Pattern

```text
entityService.ts
```

Examples

```text
doctorService.ts

appointmentService.ts
```

---

## Types

Pattern

```text
entity.types.ts
```

Examples

```text
doctor.types.ts

appointment.types.ts
```

---

# 7. Frontend Component Rules

## FE-001

One component = one responsibility.

---

## FE-002

If component > 300 lines

Split component.

---

## FE-003

Move API calls into service layer.

### Forbidden

```tsx
axios.get(...)
```

inside page.

---

## FE-004

Use React Hook Form.

---

## FE-005

Use Zod validation.

---

## FE-006

Use TanStack Query.

Do not manually manage loading state.

---

## FE-007

Avoid prop drilling deeper than 2 levels.

---

# 8. API Naming Rules

Pattern

```text
/api/v1/resource
```

Examples

```text
/api/v1/doctors

/api/v1/appointments

/api/v1/blogs
```

---

# 9. Database Naming Rules

## Tables

```text
Plural
PascalCase
```

Examples

```text
Users

Appointments

DoctorProfiles
```

---

## Columns

```text
PascalCase
```

Examples

```text
DoctorId

CreatedAt

UpdatedAt
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

# 10. Logging Rules

Log only:

```text
Authentication

Payments

Verification

Appointment Workflow

Errors
```

Do not log sensitive data.

---

# 11. Security Rules

Passwords:

```text
BCrypt
```

Required.

Never store:

```text
Plain Text Password
```

---

# 12. AI Generation Rules

When generating code:

1. Follow Business Rules first.
2. Follow Database Design second.
3. Follow API Design third.
4. Follow Screen Specifications fourth.
5. Never create entities outside Domain Model.
6. Never create endpoints outside API Design.
7. Always generate DTOs.
8. Always generate Validators.
9. Always generate Interfaces.
10. Always generate Unit Tests skeletons.
11. Prefer readability over optimization.
12. Keep methods under 50 lines whenever possible.
13. Keep Controllers thin.
14. Keep Services focused.
15. Never bypass Repository layer.
