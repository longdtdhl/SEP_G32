# Prompt: Generate Full OPCBS Website

You are a senior full-stack .NET engineer. Generate the complete Online Psychological Counseling Booking System (OPCBS) website from the existing specification documents in this repository.

## Repository Context

Project name: Online Psychological Counseling Booking System (OPCBS).

Current repository contains:

- `backend/OPCBS.sln`
- `backend/OPCBS`: early ASP.NET Core API skeleton.
- `backend/OPCBS.Web`: early ASP.NET Core Razor Pages frontend skeleton.
- `docs`: the specification source of truth.
- `docs/ui-template`: static HTML UI references for patient, doctor, customer support, business manager, system admin, and common account pages.

The current code is incomplete and should be expanded into a production-quality implementation. Do not only patch the skeleton. Build the full website according to the specification.

## Mandatory Technology Stack

Use:

- ASP.NET Core 8
- C#
- Razor Pages for the web frontend
- ASP.NET Core Web API for backend endpoints
- Entity Framework Core
- SQL Server
- Bootstrap 5
- JavaScript and jQuery where useful
- JWT authentication for API
- Cookie/JWT handling for Razor Pages frontend
- BCrypt for password hashing
- FluentValidation for create/update request validation
- Swagger/OpenAPI

External integrations must be abstracted behind interfaces:

- Brevo email through `IEmailService`
- Cloudinary file storage through `IFileStorageService`
- VNPay payment through `IPaymentService`
- Google OAuth through an authentication service abstraction

If real credentials are unavailable, implement clean mock/development adapters plus strongly typed options so the real providers can be enabled by configuration.

## Required Architecture

Implement Clean Architecture with this structure:

```text
src/
  OPCBS.API
  OPCBS.Application
  OPCBS.Domain
  OPCBS.Infrastructure
  OPCBS.Shared
  OPCBS.Web
  OPCBS.Tests
```

Layer rules:

- `OPCBS.Domain`: entities, enums, constants, common base entity types only. No EF Core, no services, no infrastructure dependencies.
- `OPCBS.Application`: DTOs, interfaces, services, validators, mappings, business workflows. No DbContext and no SQL.
- `OPCBS.Infrastructure`: EF Core DbContext, repository implementations, external service implementations, identity/token/payment/email/storage infrastructure.
- `OPCBS.API`: controllers, authentication setup, authorization policies, middleware, Swagger, API configuration. Controllers must be thin.
- `OPCBS.Web`: Razor Pages, reusable UI partials/components, API clients, cookie/token handling, static assets.
- `OPCBS.Tests`: unit test skeletons for every service, with meaningful test names.

Never put business logic in controllers or Razor `.cshtml` files. Never return EF entities directly from APIs. Use DTOs only.

## Specification Files: Must Read and Follow

Read and obey the files under `docs` before generating code:

```text
docs/00-ai-context.md
docs/01-scopes.md
docs/02-actors.md
docs/03-business-rules.md
docs/04-use-case-catalog.md
docs/05-domain-model.md
docs/06-system-workflows.md
docs/07-database-design.md
docs/10-code-rules.md
docs/11-api-design.md
docs/12-state-diagrams.md
docs/13-system-overview.md
docs/14-data-dictionary.md
docs/15-seed-data.md
docs/16-screen-components.md
docs/17-sequence-diagrams.md
docs/18-permission-matrix.md
docs/19-validation-rules.md
docs/ui-template/**/*.html
```

When documents conflict, use this priority order:

1. `docs/03-business-rules.md`
2. `docs/19-validation-rules.md`
3. `docs/11-api-design.md`
4. `docs/18-permission-matrix.md`
5. `docs/06-system-workflows.md`
6. `docs/14-data-dictionary.md`
7. `docs/07-database-design.md`
8. `docs/05-domain-model.md`
9. `docs/16-screen-components.md`
10. `docs/ui-template/**/*.html`
11. `docs/10-code-rules.md`

Do not invent undocumented entities, endpoints, roles, or business workflows. If a needed detail is missing, implement the smallest reasonable version and mark it clearly with a `TODO` comment.

## Core Roles

Implement RBAC for:

- Guest
- Patient
- Doctor
- CustomerSupport
- BusinessManager
- SystemAdmin

Use constants for role names and permission codes. Do not hardcode role strings throughout the codebase.

Authorization must deny by default. Every protected API endpoint must declare authorization requirements. Every Razor management page must require the proper role.

## Core Business Rules

Enforce at service layer and database level where appropriate:

- Users must verify email via OTP before account activation.
- Email must be unique.
- Phone number must be unique and match Vietnamese phone format.
- Passwords must be BCrypt hashed and never logged.
- Guests and Patients can book appointments.
- Guests must provide full name, email, and phone.
- Appointments cannot be booked in the past.
- Appointments can only be booked with doctors whose `VerificationStatus = Approved`, `IsVisible = true`, and active service package/subscription status is `Active`.
- One appointment slot can belong to only one appointment.
- Prevent duplicate and overlapping bookings.
- Patients cannot have overlapping appointments.
- Booking creation must be transactional and generate a unique booking code.
- Guests can track appointments by booking code and email.
- Doctors cannot create appointments on behalf of patients.
- Only doctors can approve or reject their own pending appointments.
- Completed appointments cannot be modified.
- Appointment status changes must be audited.
- Doctors can only maintain their own schedules.
- Only available, non-expired slots are visible for booking.
- Patients can access only their own profile, appointments, records, and treatment packages.
- Doctors can access only their own profile, schedule, appointments, records, blogs, and treatment packages.
- Each completed appointment can have only one consultation record.
- Consultation records cannot be deleted.
- Treatment packages are created by doctors for specific patients and are not paid through VNPay.
- Service packages are platform subscriptions purchased by doctors and are the only VNPay-paid package type.
- Expired service package/subscription hides the doctor from search results.
- Blogs are created by doctors and must be approved by CustomerSupport before public publication.
- Only completed appointments can be reviewed.
- One appointment can receive only one review.
- Doctor average rating must be recalculated automatically.
- File uploads must validate type and size and store via Cloudinary abstraction.
- Sensitive information such as passwords, OTP codes, and medical notes must never be logged.
- Critical actions must write immutable audit logs.

## Required Status Enums

Use enums instead of magic strings.

Recommended canonical enum names and values:

```csharp
UserStatus: Active, Inactive, Locked
AppointmentStatus: Pending, Approved, Rejected, InProgress, Completed, Cancelled
AppointmentSlotStatus: Available, Booked, Blocked, Expired, Cancelled, Completed
VerificationStatus: Draft, Submitted, Approved, Rejected
BlogStatus: Draft, Pending, Published, Rejected, Archived
TreatmentPackageStatus: Created, Assigned, Accepted, Active, Completed, Expired, Rejected, Cancelled
SubscriptionStatus: PendingPayment, Active, Expired, Cancelled
PaymentStatus: Pending, Success, Failed
NotificationType: OTP, Appointment, Verification, Subscription, Package, System
```

If an existing document uses a synonym such as `Submitted`, `PendingReview`, or `Approved` for blog status, normalize behavior to the API/business-rule workflow while keeping mapping code explicit.

## Required Domains and Entities

Implement EF Core entities, configurations, migrations, DTOs, repositories, services, validators, and API endpoints for:

- Role
- Permission
- RolePermission
- User
- OtpVerification
- PatientProfile
- DoctorProfile
- Specialization
- DoctorSpecialization
- VerificationRequest
- Certificate / DoctorCertificate
- Schedule / DoctorSchedule
- DoctorDayOff
- AppointmentSlot
- Appointment
- AppointmentHistory
- ConsultationRecord / ConsultationNote
- TreatmentPackage
- ServicePackage
- DoctorSubscription / DoctorServicePackage
- PaymentTransaction
- BlogPost
- BlogComment
- Review
- Notification
- SystemConfig
- AuditLog

All aggregate entities must include:

```csharp
Guid Id
DateTime CreatedAt
DateTime? UpdatedAt
Guid? CreatedBy
Guid? UpdatedBy
```

Soft delete where specified:

```csharp
bool IsDeleted
```

Use SQL Server-friendly types and constraints from `docs/14-data-dictionary.md`.

## Required API Design

Base route: `/api/v1`.

All APIs must return a consistent response envelope:

```json
{
  "success": true,
  "message": "Operation successful",
  "data": {}
}
```

Validation/error response:

```json
{
  "success": false,
  "message": "Validation failed",
  "errors": ["Email is required"]
}
```

Implement the endpoints specified in `docs/11-api-design.md`, including:

- `/api/v1/auth`: register, verify OTP, login, google login, forgot password, reset password, refresh token, logout
- `/api/v1/users`: profile, update profile, change password
- `/api/v1/doctors`: list, details, schedule, reviews, specializations
- `/api/v1/doctor-profile`: doctor self profile, avatar, certificates
- `/api/v1/verifications`: submit/status and CustomerSupport pending/detail/approve/reject
- `/api/v1/schedules`: doctor schedule CRUD and unavailable dates
- `/api/v1/appointments`: create, my appointments, guest tracking, cancel, reschedule, doctor list, approve, reject, complete
- `/api/v1/consultation-records`
- `/api/v1/treatment-packages`
- `/api/v1/blogs`
- `/api/v1/blog-comments`
- `/api/v1/reviews`
- `/api/v1/service-packages`
- `/api/v1/subscriptions`
- `/api/v1/payments`
- `/api/v1/notifications`
- `/api/v1/customer-support`
- `/api/v1/business-manager`
- `/api/v1/admin`

Support pagination using:

```text
?page=1&pageSize=10
```

and return pagination metadata where list endpoints need it.

## Required Frontend Website

Build a complete responsive Razor Pages website using Bootstrap 5 and reusable partials/components.

Use the static HTML files in `docs/ui-template` as visual and workflow references, but adapt them into maintainable Razor Pages and shared components.

Required public/account pages:

- Home page
- Doctor list/search page
- Doctor profile page
- Doctor ratings page
- Blog list page
- Blog detail page
- Register
- Verify OTP
- Login
- Forgot password
- Reset/change password
- Profile page
- Guest appointment tracking

Required patient pages:

- Book appointment
- Appointment summary/detail
- My appointments
- Cancel/reschedule appointment
- Consultation history
- Treatment package list/detail/progress
- Feedback/rating form
- Blog comments where allowed

Required doctor pages:

- Doctor dashboard
- Doctor verification/registration
- Verification status
- Schedule management: create, edit, cancel/unavailable dates
- Appointment management
- Appointment detail with approve/reject/complete actions
- Consultation/case record create/update/list
- Treatment package create/update/assign/progress
- Blog create/edit/list/submit for review
- Service package list
- Purchase/renew service package
- Subscription status/history

Required CustomerSupport pages:

- Dashboard
- Pending doctor applications
- Doctor application detail
- Approve/reject doctor verification
- Blog moderation queue
- Blog detail/editor/review
- Approve/reject blog
- Appointment support view if specified

Required BusinessManager pages:

- Dashboard / analytics
- Manage service packages
- Manage specializations
- Operational/revenue reports
- Doctor/application overview if specified by UI references

Required SystemAdmin pages:

- Dashboard
- Account/user management
- Role and permission management
- Audit logs
- Reports
- System settings

Reusable UI components from `docs/16-screen-components.md` must be implemented as partials/tag helpers/view components where appropriate:

- Navbar
- Footer
- SearchBar
- DoctorCard
- DoctorFilterPanel
- DoctorScheduleCalendar
- AppointmentBookingForm
- AppointmentStatusBadge
- AppointmentTable
- PackageCard
- PackageProgressBar
- BlogCard
- BlogEditor
- RatingStars
- DashboardStatsCard
- DashboardChart
- UserTable
- RoleTable
- AuditLogTable
- ConfirmDialog
- LoadingSpinner
- EmptyState
- ErrorState
- Pagination
- ToastNotification
- FileUploader

Razor Pages should call typed API client services. Do not duplicate backend business rules inside Razor Pages; client-side validation is for user experience only.

## UI/UX Requirements

- Responsive layout for desktop, tablet, and mobile.
- Clean mental-health/counseling visual tone: calm, professional, trustworthy.
- Use Bootstrap 5 consistently.
- Keep dashboards dense and useful, not marketing-heavy.
- Management pages should prioritize scanning, filtering, tables, status badges, and fast actions.
- Public pages should make doctor discovery and appointment booking easy.
- Use empty, loading, error, and toast states.
- Add confirmation dialogs for destructive or workflow-changing actions.
- Use accessible labels and validation messages.

## Validation Requirements

Use FluentValidation for every create/update request. Also add client-side HTML validation where useful.

Implement rules from `docs/19-validation-rules.md`, including:

- Register: email required/max 255/valid/unique, password 8-50 with uppercase/lowercase/number, full name required/max 255, phone required/unique/Vietnamese format.
- Doctor profile: professional title required/max 255, biography required/max 5000, experience 0-60, at least one specialization.
- Appointment: doctor exists/verified/active subscription, slot exists/available/not expired, patient exists where applicable, guest info required, notes max 2000.
- Schedule: start earlier than end, slot duration in 30/60/90/120, at least one working day.
- Consultation record: summary required/max 5000, recommendation/follow-up max 5000, appointment must be approved before record creation.
- Treatment package: name required/max 255, session quantity 1-100, price greater than zero, validity 1-365 days.
- Blog: title required/max 500, content required/min 100, thumbnail required/Cloudinary URL only.
- Review: rating 1-5, completed appointment only, one review per appointment.
- Service package: name required/unique, duration 30-365, price positive.
- Verification: at least one certificate, PDF/JPG/PNG, max 10 MB.

## Seed Data

Implement seed data from `docs/15-seed-data.md` and include at minimum:

- Roles
- Permissions
- Role-permission mappings
- System configuration
- Specializations
- Service packages
- A development SystemAdmin account
- Optional demo Patient, Doctor, CustomerSupport, and BusinessManager users for local testing

Never seed plaintext passwords directly; hash seeded passwords with BCrypt.

## Security Requirements

- JWT validation for all protected APIs.
- Refresh token support if specified by API.
- Secure cookie handling in Razor Pages.
- HTTPS redirection.
- Role and permission constants.
- Ownership checks in services.
- Deny access by default.
- No sensitive data in logs.
- Upload file type/size validation.
- Audit logs for sensitive actions listed in `docs/18-permission-matrix.md`.

## Testing Requirements

Create unit test skeletons for every service. Include tests for important rules:

- Register requires unique email and sends OTP.
- Login rejects unverified or invalid users.
- Booking rejects unverified doctors.
- Booking rejects inactive subscriptions.
- Booking prevents double booking.
- Booking generates unique booking code.
- Appointment approve/reject only allowed for assigned doctor.
- Completed appointments cannot be modified.
- Consultation record one-per-appointment.
- Review only after completed appointment and only once.
- Blog publication requires CustomerSupport approval.
- Payment success activates service package/subscription.
- Expired subscription hides doctor from search results.

## Deliverables

Generate:

1. Complete solution/project structure.
2. Domain entities and enums.
3. EF Core DbContext and entity configurations.
4. Migrations.
5. Repository interfaces and implementations.
6. Application services and interfaces.
7. DTOs and AutoMapper profiles.
8. FluentValidation validators.
9. API controllers with Swagger comments.
10. Authentication, JWT, RBAC, policies, middleware.
11. Razor Pages frontend with shared components and role-specific dashboards.
12. API client services for Razor Pages.
13. External service abstractions plus development/mock implementations.
14. Seed data.
15. Unit test skeletons.
16. README instructions for setup, migrations, run, test, and configuration.

## Coding Rules

- Use explicit names.
- Keep methods small and readable.
- Avoid duplicate logic.
- Use async/await for I/O.
- Use cancellation tokens where appropriate.
- Use DTOs only at API boundaries.
- Controllers only receive request, call service, return response.
- Services contain business logic and workflow orchestration.
- Repositories contain database access only.
- Razor `.cshtml` files contain display logic only.
- Do not generate unused code.
- Do not bypass the service or repository layer.

## Implementation Strategy

Build in this order:

1. Solution/projects and references.
2. Domain entities/enums/common base types.
3. Infrastructure DbContext/configurations/migrations.
4. Shared response/pagination/result models.
5. Application DTOs, validators, interfaces.
6. Repositories.
7. Services and business workflows.
8. API authentication/authorization/middleware/controllers.
9. Seed data.
10. Razor Pages API clients and layout/components.
11. Public pages.
12. Patient pages.
13. Doctor pages.
14. CustomerSupport pages.
15. BusinessManager pages.
16. SystemAdmin pages.
17. Tests.
18. README.

At the end, run formatting/build/tests and fix all compile errors.
