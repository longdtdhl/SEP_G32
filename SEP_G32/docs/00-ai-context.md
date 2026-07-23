# 00-ai-context.md

# OPCBS AI Context

## Project Name

Online Psychological Counseling Booking System (OPCBS)

---

# 1. Project Overview

OPCBS is a web-based psychological counseling booking platform.

The platform connects Patients with verified Doctors and provides:

* Doctor discovery
* Appointment booking
* Consultation management
* Treatment package management
* Doctor verification
* Subscription management
* Blog management
* Payment integration
* Notification services

This project follows Specification-Driven Development (SDD).

All generated code MUST strictly follow the specification documents inside `/docs`.

---

# 2. Technology Stack

## Frontend

ASP.NET Core Razor Pages

Technologies:

* Razor Pages
* HTML5
* CSS3
* Bootstrap 5
* JavaScript
* jQuery

---

## Backend

ASP.NET Core 8

Language:

* C#

Architecture:

* Layered Architecture
* Repository Pattern
* Service Pattern

---

## Database

Microsoft SQL Server

ORM:

* Entity Framework Core

Migration Strategy:

* Code First

---

## Authentication

JWT Authentication

Authorization:

Role-Based Access Control (RBAC)

Password Hashing:

BCrypt

---

## External Services

### Email

Brevo

Used for:

* OTP Verification
* Appointment Notifications
* Subscription Notifications

---

### Payment

VNPay

Used for:

* Service Package Payment

---

### Storage

Cloudinary

Used for:

* User Avatars
* Doctor Certificates
* Blog Images

---

### OAuth

Google OAuth

Used for:

* Social Login

---

# 3. Architecture

The system follows:

```text
Razor Pages
↓
Controllers
↓
Services
↓
Repositories
↓
Entity Framework Core
↓
SQL Server
```

---

# 4. Project Structure

```text
src/

├── OPCBS.API
│
├── OPCBS.Application
│
├── OPCBS.Domain
│
├── OPCBS.Infrastructure
│
└── OPCBS.Shared
```

---

## OPCBS.API

Contains:

```text
Controllers
DTOs
Razor Pages
Swagger
Authentication
```

---

## OPCBS.Application

Contains:

```text
Interfaces
Services
Validators
Business Logic
```

---

## OPCBS.Domain

Contains:

```text
Entities
Enums
Domain Constants
```

---

## OPCBS.Infrastructure

Contains:

```text
DbContext
Repositories
Cloudinary Services
Brevo Services
VNPay Services
```

---

## OPCBS.Shared

Contains:

```text
Helpers
Extensions
Constants
Shared Models
```

---

# 5. Core Business Rules

The following rules MUST always be enforced.

---

## Doctor Visibility

Doctor appears in search results ONLY IF:

```text
VerificationStatus = Approved

AND

SubscriptionStatus = Active
```

---

## Appointment Booking

Guests can:

```text
Book Appointment
Track Appointment
```

using BookingCode.

---

Patients can:

```text
Book Appointment
Cancel Appointment
Reschedule Appointment
```

---

## Double Booking Prevention

A doctor cannot have two appointments on the same slot.

---

## Blog Publishing

Workflow:

```text
Draft
↓
Pending
↓
Published
```

Only Customer Support can publish blogs.

---

## Treatment Package

Treatment Package is:

```text
Doctor → Patient
```

Not platform subscription.

---

## Service Package

Service Package is:

```text
Platform → Doctor
```

Required before receiving appointments.

---

# 6. Status Definitions

AI MUST generate enums exactly matching specifications.

---

## AppointmentStatus

```csharp
Pending
Approved
Rejected
Completed
Cancelled
```

---

## VerificationStatus

```csharp
Draft
Submitted
Approved
Rejected
```

---

## BlogStatus

```csharp
Draft
Pending
Published
Rejected
Archived
```

---

## TreatmentPackageStatus

```csharp
Created
Assigned
Active
Completed
Expired
Rejected
Cancelled
```

---

## SubscriptionStatus

```csharp
PendingPayment
Active
Expired
Cancelled
```

---

## PaymentStatus

```csharp
Pending
Success
Failed
```

---

# 7. Coding Standards

AI MUST follow:

```text
06-code-rules.md
```

without exception.

---

# 8. Backend Generation Rules

Controllers:

```text
Thin Controllers
```

Controllers ONLY:

```text
Receive Request
Call Service
Return Response
```

---

Services:

```text
Contain Business Logic
```

---

Repositories:

```text
Contain Database Logic
```

---

Entities:

```text
Never returned directly
```

Always use DTOs.

---

Every Create/Update Request:

```text
Must have FluentValidation
```

---

Every Endpoint:

```text
Must have Authorization
```

when required.

---

# 9. Database Rules

Primary Key:

```csharp
Guid Id
```

---

Audit Fields:

```csharp
CreatedAt
UpdatedAt
CreatedBy
UpdatedBy
```

---

Soft Delete:

```csharp
IsDeleted
```

when applicable.

---

# 10. UI Rules

Follow:

```text
08-screen-specifications.md
09-ui-navigation.md
```

---

Razor Pages should:

```text
Be simple
Be maintainable
Use Bootstrap
Follow clean UI principles
```

---

# 11. API Rules

Follow:

```text
11-api-design.md
```

---

Never create:

```text
Endpoints
Entities
DTOs
Statuses
Business Logic
```

outside specifications.

---

# 12. Generation Priority

When generating code, follow documents in this exact order:

```text
1. 00-ai-context.md

2. 01-scope.md

3. 02-actors.md

4. 03-business-rules.md

5. 04-use-cases.md

6. 04-system-workflows.md

7. 10-state-diagrams.md

8. 05-domain-model.md

9. 07-database-design.md

10. 08-screen-specifications.md

11. 09-ui-navigation.md

12. 11-api-design.md

13. 06-code-rules.md
```

---

# 13. AI Instructions

ALWAYS:

* Generate readable code.
* Generate DTOs.
* Generate Interfaces.
* Generate Validators.
* Generate AutoMapper Profiles.
* Generate Swagger Comments.
* Generate Repository Implementations.
* Generate Unit Test Skeletons.
* Follow SOLID principles.
* Follow Clean Code principles.

NEVER:

* Bypass Service Layer.
* Bypass Repository Layer.
* Access DbContext from Controllers.
* Return Entity objects directly.
* Create business logic in Razor Pages.
* Create undocumented endpoints.
* Create undocumented entities.

The specification documents are the single source of truth.
