# 13-system-overview.md

# 1. Purpose

This document provides a high-level architectural overview of the Online Psychological Counseling Booking System (OPCBS).

It describes:

* System architecture
* Technology stack
* Major components
* External integrations
* Deployment model
* Security architecture
* Data flow

This document serves as the entry point for understanding the entire system before reviewing detailed specifications.

---

# 2. System Overview

## Project Name

Online Psychological Counseling Booking System (OPCBS)

---

## Project Type

Web-Based Psychological Counseling Management Platform

---

## Main Goal

OPCBS is designed to connect Patients with verified Doctors specializing in psychological counseling services.

The platform provides:

* Doctor discovery
* Appointment booking
* Consultation management
* Treatment package management
* Doctor verification
* Subscription management
* Educational blog management

---

# 3. High-Level Architecture

```text
+--------------------------------------------------+
|                    End Users                     |
+--------------------------------------------------+
          |                |               |
          v                v               v

     Guest            Patient         Doctor
          \              |              /
           \             |             /
            \            |            /

+--------------------------------------------------+
|              OPCBS Web Application               |
|               ASP.NET Razor Pages                |
+--------------------------------------------------+
                        |
                        |
                        v

+--------------------------------------------------+
|                 ASP.NET Core API                 |
|                 Business Layer                   |
+--------------------------------------------------+
                        |
                        |
                        v

+--------------------------------------------------+
|                 SQL Server DB                    |
+--------------------------------------------------+

        |               |               |
        v               v               v

    Brevo           VNPay         Cloudinary
  Email Service    Payment        File Storage
```

---

# 4. Architecture Style

The system follows:

## Layered Architecture

```text
Presentation Layer
        ↓
Application Layer
        ↓
Domain Layer
        ↓
Infrastructure Layer
        ↓
Database
```

---

## Clean Separation of Concerns

Each layer has a single responsibility.

| Layer        | Responsibility    |
| ------------ | ----------------- |
| Razor Pages  | User Interface    |
| Controllers  | API Endpoints     |
| Services     | Business Logic    |
| Repositories | Data Access       |
| Domain       | Business Entities |
| Database     | Data Storage      |

---

# 5. Technology Stack

## Frontend

### Framework

ASP.NET Core Razor Pages

### UI Technologies

* Razor Pages
* HTML5
* CSS3
* Bootstrap 5
* JavaScript
* jQuery

### Purpose

Responsible for:

* Rendering pages
* Form submission
* Client-side validation
* User interaction

---

## Backend

### Framework

ASP.NET Core 8

### Language

C#

### Architecture

* Layered Architecture
* Repository Pattern
* Service Pattern

### Responsibilities

* Business logic
* Authentication
* Authorization
* Validation
* Data processing
* External service integration

---

## Database

### Database Engine

Microsoft SQL Server

### ORM

Entity Framework Core

### Responsibilities

* Persistent storage
* Data integrity
* Relationship management

---

# 6. Project Structure

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

Responsibilities:

* Razor Pages
* Controllers
* DTOs
* Swagger
* Authentication

---

## OPCBS.Application

Responsibilities:

* Services
* Interfaces
* Business logic
* Validators

---

## OPCBS.Domain

Responsibilities:

* Entities
* Enums
* Domain rules

---

## OPCBS.Infrastructure

Responsibilities:

* EF Core
* Repositories
* External services
* Database implementation

---

## OPCBS.Shared

Responsibilities:

* Constants
* Helpers
* Common models
* Extensions

---

# 7. Authentication Architecture

## Authentication Method

JWT Authentication

---

## Login Flow

```text
User Login
        ↓
Validate Credentials
        ↓
Generate JWT Token
        ↓
Store Authentication Cookie
        ↓
Access Protected Resources
```

---

## Authorization Method

Role-Based Access Control (RBAC)

---

## Roles

```text
Patient

Doctor

Customer Support

Business Manager

System Admin
```

---

# 8. External Integrations

## Brevo Email Service

Purpose:

* OTP Verification
* Password Reset
* Appointment Notifications
* Subscription Notifications

---

## VNPay Payment Gateway

Purpose:

* Service Package Payment
* Payment Confirmation

---

## Cloudinary

Purpose:

* Avatar Storage
* Doctor Certificates
* Blog Images

---

## Google OAuth

Purpose:

* Social Login
* Identity Verification

---

# 9. Core Business Modules

## Authentication & User Management

* Register
* Login
* OTP Verification
* Profile Management

---

## Doctor Discovery

* Search Doctors
* Filter Doctors
* View Profiles
* View Ratings

---

## Appointment Management

* Book Appointment
* Track Appointment
* Approve Appointment
* Complete Appointment

---

## Consultation Management

* Consultation Records
* Consultation History

---

## Treatment Package Management

* Create Treatment Package
* Assign Treatment Package
* Accept Package
* Track Progress

---

## Blog Management

* Create Blog
* Review Blog
* Publish Blog

---

## Doctor Verification

* Submit Verification Request
* Review Verification Request

---

## Service Package Management

* Purchase Subscription
* Activate Subscription

---

# 10. Security Overview

## Password Security

```text
BCrypt Hashing
```

Passwords are never stored in plain text.

---

## Authentication

```text
JWT Token
```

Used for identity verification.

---

## Authorization

```text
RBAC
```

Used for access control.

---

## File Security

```text
Cloudinary Secure URLs
```

Used for uploaded files.

---

## API Protection

```text
JWT Authorization
```

Required for protected APIs.

---

# 11. Deployment Architecture

## Development Environment

```text
Razor Pages
+
ASP.NET Core
+
SQL Server
```

---

## Production Environment

```text
Frontend + Backend
        ↓
Azure App Service

Database
        ↓
Azure SQL Database

Files
        ↓
Cloudinary

Email
        ↓
Brevo

Payments
        ↓
VNPay
```

---

# 12. Key Business Rules

The system enforces the following rules:

* Guests can book appointments.
* Guests can track appointments using booking codes.
* Only verified doctors can appear in search results.
* Doctors must have an active Service Package subscription to receive bookings.
* Double booking is not allowed.
* One appointment can only receive one review.
* Blog content must be reviewed before publication.
* Treatment Packages are created by Doctors and accepted by Patients.

---

# 13. System Success Criteria

The system is considered successful when it can:

* Allow Patients and Guests to book appointments successfully.
* Allow Doctors to manage consultations efficiently.
* Support secure authentication and authorization.
* Prevent scheduling conflicts.
* Manage subscriptions and payments correctly.
* Deliver notifications reliably.
* Scale to support increasing numbers of users and consultations.
