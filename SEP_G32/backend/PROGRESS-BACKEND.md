# OPCBS - Online Psychological Counseling Booking System

A complete .NET Core 8 implementation of a psychological counseling booking platform with Clean Architecture principles.

## Project Structure

```
OPCBS.sln
├── OPCBS                    (API Layer - ASP.NET Core REST API)
├── OPCBS.Web               (Presentation Layer - ASP.NET Razor Pages)
├── OPCBS.Application       (Business Logic Layer - Services, DTOs, Validators)
├── OPCBS.Infrastructure    (Data Access Layer - EF Core, Repositories)
├── OPCBS.Domain            (Core Domain Layer - Entities, Enums, Constants)
├── OPCBS.Shared            (Shared Layer - Common Models, Helpers)
└── OPCBS.Tests             (Unit Tests)
```

## Current Implementation Status

### ✅ COMPLETED

1. **Solution Architecture**
   - Clean Architecture with 7 projects properly configured
   - Correct project references and dependencies
   - NuGet packages installed for all layers

2. **Domain Layer (OPCBS.Domain)**
   - 41 Domain Entities fully implemented:
     - Identity: User, Role, Permission, RolePermission, OtpVerification
     - Profiles: PatientProfile, DoctorProfile, Specialization, DoctorSpecialization, Certificate, VerificationRequest
     - Appointments: Schedule, DoctorDayOff, AppointmentSlot, Appointment, AppointmentHistory, ConsultationRecord
     - Packages: ServicePackage, TreatmentPackage, DoctorSubscription, PaymentTransaction
     - Content: BlogPost, BlogComment, Review
     - System: Notification, AuditLog, SystemConfig
   - All Status Enums (9 types)
   - Base Entity classes with audit fields
   - Soft-delete support where specified

3. **Infrastructure Layer (OPCBS.Infrastructure)**
   - Comprehensive OpcbsDbContext with 50+ DbSets
   - Detailed entity configuration for SQL Server
   - Relationships, constraints, and indexes defined
   - Initial EF Core migration created

4. **Shared Layer (OPCBS.Shared)**
   - StandardAPIResponse model (generic and non-generic)
   - PaginationMetadata for list endpoints

5. **Appointments Module — Backend Complete** *(Updated: June 26, 2026)*
   - **All 9 endpoints match API spec (docs/11-api-design.md §9):**

     | Method | Route                       | Role           | Status |
     |--------|-----------------------------|----------------|--------|
     | POST   | /api/v1/appointments        | Guest/Patient  | ✅     |
     | GET    | /my-appointments            | Patient        | ✅     |
     | GET    | /track/{bookingCode}        | Guest/Patient  | ✅     |
     | PUT    | /cancel/{id}                | Authorized     | ✅     |
     | PUT    | /reschedule/{id}            | Patient        | ✅     |
     | GET    | /doctor                     | Doctor         | ✅     |
     | PUT    | /approve/{id}               | Doctor         | ✅     |
     | PUT    | /reject/{id}                | Doctor         | ✅     |
     | PUT    | /complete/{id}              | Doctor         | ✅     |

   - **Business Rules Enforced:**
     - BOOK-03: No past booking — slot date/time must be in the future
     - BOOK-04 / DOC-12 / SP-01: Doctor must be Verified + Active Subscription
     - BOOK-06 / BOOK-07 / BOOK-09: No double booking for same patient on same slot
     - APPT-05: 24-hour cancellation policy enforced
     - APPT-08: Completed appointments cannot be modified
     - Reschedule: cannot reschedule to a past slot

   - **Validators (FluentValidation):**
     - CreateAppointmentDtoValidator — fixed guest booking conditional
     - CancelAppointmentDtoValidator
     - RejectAppointmentDtoValidator
     - TrackAppointmentDtoValidator
     - CreateConsultationRecordDtoValidator
     - UpdateConsultationRecordDtoValidator

   - **Unit Tests: 13 tests — ALL PASSING ✅**
     - CreateAppointment_Success
     - CreateAppointment_DoctorNotVerified_Fails
     - CreateAppointment_NoActiveSubscription_Fails
     - CreateAppointment_SlotNotAvailable_Fails
     - CreateAppointment_PastSlot_Fails
     - CreateAppointment_DoubleBooking_Fails
     - CancelAppointment_Within24Hours_Fails
     - CancelAppointment_Success
     - ApproveAppointment_Success
     - ApproveAppointment_NotPending_Fails
     - RejectAppointment_Success
     - CompleteAppointment_Success
     - RescheduleAppointment_PastSlot_Fails
6. **Auth + Users/Profile Module — Backend Complete** *(Updated: June 26, 2026)*
   - **Auth endpoints (AuthController — 8 endpoints):**
     - `POST /api/auth/register` — Patient registration with OTP email
     - `POST /api/auth/register-doctor` — Doctor registration with specializations
     - `POST /api/auth/verify-otp` — Email verification via 6-digit OTP
     - `POST /api/auth/login` — JWT login with refresh token storage
     - `POST /api/auth/forgot-password` — Send password reset OTP
     - `POST /api/auth/reset-password` — Reset password + invalidate refresh token
     - `POST /api/auth/refresh-token` — Token rotation (old invalidated, new issued)
     - `POST /api/auth/logout` — Invalidate refresh token
   - **Users endpoints (UsersController — 3 endpoints):**
     - `GET /api/users/profile` — Get current user profile with role
     - `PUT /api/users/profile` — Update profile (phone uniqueness enforced)
     - `PUT /api/users/change-password` — Change password + invalidate refresh token
   - **Business rules enforced:**
     - Phone uniqueness on registration and profile update
     - Email uniqueness on registration
     - Password complexity: uppercase + lowercase + number, 8-50 chars
     - Vietnamese phone format validation
     - Refresh token rotation on every refresh
     - Refresh token invalidation on password change/reset/logout
     - Account status checks (Active, Locked, Inactive)
   - **Validators:**
     - RegisterDtoValidator, LoginDtoValidator, VerifyOtpDtoValidator
     - ForgotPasswordDtoValidator, ResetPasswordDtoValidator, ChangePasswordDtoValidator
     - RegisterDoctorDtoValidator, UpdateDoctorProfileDtoValidator
     - UpdateUserProfileDtoValidator (NEW — phone format + fullname max length)
   - **Unit tests: 22 new tests (AuthServiceTests + UserServiceTests)**
     - Register_DuplicateEmail_ReturnsError
     - Register_DuplicatePhone_ReturnsError
     - Register_ValidInput_CreatesUserAndSendsOtp
     - Login_InvalidEmail_ReturnsError
     - Login_WrongPassword_ReturnsError
     - Login_EmailNotVerified_ReturnsError
     - Login_LockedAccount_ReturnsError
     - Login_ValidCredentials_ReturnsTokensAndStoresRefreshToken
     - RefreshToken_InvalidToken_ReturnsError
     - RefreshToken_ExpiredToken_ReturnsError
     - RefreshToken_Valid_ReturnsNewTokensAndRotates
     - Logout_InvalidatesRefreshToken
     - VerifyOtp_ValidOtp_ActivatesUser
     - VerifyOtp_ExpiredOtp_ReturnsError
     - ResetPassword_ValidOtp_ChangesPasswordAndInvalidatesRefreshToken
     - GetProfile_UserNotFound_ReturnsError
     - GetProfile_ValidUser_ReturnsProfileWithRole
     - UpdateProfile_DuplicatePhone_ReturnsError
     - UpdateProfile_ValidUpdate_ReturnsUpdatedProfile
     - ChangePassword_WrongCurrentPassword_ReturnsError
     - ChangePassword_SameAsOld_ReturnsError
     - ChangePassword_Valid_ChangesPasswordAndInvalidatesRefreshToken

7. **Doctors + Schedules Module — Backend Complete** *(Updated: June 26, 2026)*
   - **DoctorsController (5 endpoints):**
     - `GET /api/v1/doctors` — Search/list doctors (public)
     - `GET /api/v1/doctors/{id}` — Doctor detail (public)
     - `GET /api/v1/doctors/{id}/schedule` — Doctor available slots (public)
     - `GET /api/v1/doctors/{id}/reviews` — Doctor reviews (public)
     - `GET /api/v1/doctors/specializations` — List specializations (public)
   - **DoctorProfileController (4 endpoints):**
     - `GET /api/v1/doctor-profile` — Get own profile (Doctor)
     - `PUT /api/v1/doctor-profile` — Update own profile (Doctor)
     - `POST /api/v1/doctor-profile/avatar` — Upload avatar (stub)
     - `POST /api/v1/doctor-profile/certificates` — Upload cert (stub)
   - **SchedulesController (5 endpoints — all match spec §8):**
     - `GET /api/v1/schedules` — Get own schedules (Doctor)
     - `POST /api/v1/schedules` — Create schedule (Doctor)
     - `PUT /api/v1/schedules` — Update schedule (Doctor) ✅ NEW
     - `DELETE /api/v1/schedules/{id}` — Delete schedule (Doctor)
     - `POST /api/v1/schedules/unavailable-date` — Add day off (Doctor) ✅ FIXED route

8. **Consultation Records + Reviews Module — Backend Complete** *(Updated: June 26, 2026)*
   - **ConsultationRecordsController (5 endpoints):**
     - `POST /api/v1/consultation-records` — Create record (Doctor)
     - `PUT /api/v1/consultation-records/{id}` — Update record (Doctor)
     - `GET /api/v1/consultation-records/patient/{id}` — By patient (Doctor)
     - `GET /api/v1/consultation-records/appointment/{id}` — By appointment (Doctor) ✅ NEW
     - `GET /api/v1/consultation-records/my-records` — Own records (Patient) ✅ NEW
   - **ReviewsController (2 endpoints):**
     - `POST /api/v1/reviews` — Create review (Patient)
     - `GET /api/v1/reviews/doctor/{id}` — Doctor reviews (public) ✅ NEW

9. **Blogs + Blog Moderation Module — Backend Complete** *(Updated: June 26, 2026)*
   - **BlogsController (9 endpoints):**
     - `GET /api/v1/blogs` — Published blogs (public)
     - `GET /api/v1/blogs/{id}` — Blog detail (public)
     - `POST /api/v1/blogs` — Create blog (Doctor)
     - `PUT /api/v1/blogs/{id}` — Update blog (Doctor)
     - `DELETE /api/v1/blogs/{id}` — Delete blog (Doctor) ✅ NEW
     - `POST /api/v1/blogs/submit-review/{id}` — Submit for review (Doctor) ✅ FIXED route
     - `GET /api/v1/blogs/my-blogs` — Own blogs (Doctor) ✅ FIXED route
     - `GET /api/v1/blogs/pending` — Pending blogs (CS/Admin)
     - `PUT /api/v1/blogs/approve/{id}` — Approve blog (CS/Admin) ✅ FIXED method
     - `PUT /api/v1/blogs/reject/{id}` — Reject blog (CS/Admin) ✅ FIXED method
   - **BlogCommentsController (3 endpoints):** ✅ NEW
     - `POST /api/v1/blog-comments` — Add comment
     - `PUT /api/v1/blog-comments/{id}` — Update comment
     - `DELETE /api/v1/blog-comments/{id}` — Delete comment

10. **Verification Module — Backend Complete** *(Updated: June 26, 2026)*
    - **VerificationsController (6 endpoints):**
      - `POST /api/v1/verifications/submit` — Submit verification (Doctor)
      - `GET /api/v1/verifications/status` — Get own status (Doctor)
      - `GET /api/v1/verifications/{id}` — Get detail (CS/Admin) ✅ NEW
      - `GET /api/v1/verifications/pending` — Pending list (CS/Admin)
      - `PUT /api/v1/verifications/approve` — Approve (CS/Admin) ✅ FIXED method
      - `PUT /api/v1/verifications/reject` — Reject (CS/Admin) ✅ FIXED method

11. **Service Packages + Subscriptions + Payments Module — Backend Complete** *(Updated: June 26, 2026)*
    - **ServicePackagesController (4 endpoints):**
      - `GET /api/v1/service-packages` — Active packages (public)
      - `POST /api/v1/service-packages` — Create (BM/Admin)
      - `PUT /api/v1/service-packages/{id}` — Update (BM/Admin)
      - `DELETE /api/v1/service-packages/{id}` — Toggle active (BM/Admin) ✅ NEW
    - **SubscriptionsController (3 endpoints):** ✅ NEW
      - `GET /api/v1/subscriptions/my-subscription` — Active subscription (Doctor)
      - `POST /api/v1/subscriptions/purchase` — Purchase subscription (Doctor)
      - `GET /api/v1/subscriptions/history` — Subscription history (Doctor)
    - **PaymentsController (3 endpoints):** ✅ NEW
      - `POST /api/v1/payments/create-vnpay` — Create VNPay payment (Doctor)
      - `GET /api/v1/payments/callback` — VNPay callback (anonymous)
      - `GET /api/v1/payments/history` — Payment history (auth)
    - **SubscriptionService — Full implementation:** ✅ NEW
      - PurchaseAsync with PaymentTransaction creation
      - GetActiveSubscriptionAsync
      - GetSubscriptionHistoryAsync
      - ProcessPaymentCallbackAsync (VNPay callback handling)

12. **Notifications + Admin/CS/BM Dashboard APIs — Backend Complete** *(Updated: June 26, 2026)*
    - **NotificationsController (3 endpoints):**
      - `GET /api/v1/notifications` — Get notifications
      - `PUT /api/v1/notifications/mark-read/{id}` — Mark as read ✅ FIXED route
      - `PUT /api/v1/notifications/mark-read-all` — Mark all as read ✅ FIXED route
    - **CustomerSupportController (3 endpoints):** ✅ NEW
      - `GET /api/v1/customer-support/dashboard` — CS dashboard
      - `GET /api/v1/customer-support/pending-doctors` — Pending verifications
      - `GET /api/v1/customer-support/pending-blogs` — Pending blogs
    - **BusinessManagerController (6 endpoints):** ✅ NEW
      - `GET /api/v1/business-manager/dashboard` — BM dashboard
      - `GET /api/v1/business-manager/analytics` — Analytics (stub)
      - `GET /api/v1/business-manager/reports` — Reports (stub)
      - `POST /api/v1/business-manager/specializations` — Create specialization
      - `PUT /api/v1/business-manager/specializations/{id}` — Update (stub)
      - `DELETE /api/v1/business-manager/specializations/{id}` — Delete (stub)
    - **AdminController (8 endpoints):**
      - `GET /api/v1/admin/dashboard` — Dashboard stats
      - `GET /api/v1/admin/users` — User list
      - `PUT /api/v1/admin/users/{id}/lock` — Lock user ✅ FIXED method
      - `PUT /api/v1/admin/users/{id}/unlock` — Unlock user ✅ FIXED method
      - `GET /api/v1/admin/roles` — Roles (stub)
      - `GET /api/v1/admin/permissions` — Permissions (stub)
      - `GET /api/v1/admin/audit-logs` — Audit logs
      - `GET /api/v1/admin/reports` — Reports (stub)
    - **TreatmentPackagesController (8 endpoints):** ✅ NEW
      - `POST /api/v1/treatment-packages` — Create (Doctor)
      - `PUT /api/v1/treatment-packages/{id}` — Update (blocked per business rules)
      - `DELETE /api/v1/treatment-packages/{id}` — Delete (blocked per business rules)
      - `POST /api/v1/treatment-packages/assign` — Assign to patient (Doctor)
      - `GET /api/v1/treatment-packages/my-packages` — Own packages (Patient)
      - `PUT /api/v1/treatment-packages/accept/{id}` — Accept (Patient)
      - `PUT /api/v1/treatment-packages/reject/{id}` — Reject (Patient)
      - `GET /api/v1/treatment-packages/{id}` — Package detail

### 🔄 IN PROGRESS / TODO

1. **Application Layer (OPCBS.Application)** - ✅ ALL SERVICES IMPLEMENTED
   - [x] Appointment DTOs + Service
   - [x] Schedule DTOs + Service (incl. UpdateScheduleAsync)
   - [x] ConsultationRecordService
   - [x] BlogService (incl. DeleteBlog, Comment stubs)
   - [x] ReviewService
   - [x] TreatmentPackageService
   - [x] ServicePackageService
   - [x] SubscriptionService ✅ NEW
   - [x] NotificationService
   - [x] VerificationService (incl. GetByIdAsync)
   - [x] AdminService

2. **Infrastructure Layer - Repositories** - ✅ DONE
   - [x] Generic IRepository<T> interface
   - [x] EF Core repository implementation
   - [x] Unit of Work pattern

3. **API Layer (OPCBS)** - ✅ ALL CONTROLLERS IMPLEMENTED
   - [x] AuthController (8 endpoints)
   - [x] UsersController (3 endpoints)
   - [x] DoctorsController (5 endpoints)
   - [x] DoctorProfileController (4 endpoints)
   - [x] AppointmentsController (9 endpoints)
   - [x] SchedulesController (5 endpoints)
   - [x] ConsultationRecordsController (5 endpoints)
   - [x] BlogsController (9 endpoints)
   - [x] BlogCommentsController (3 endpoints)
   - [x] ReviewsController (2 endpoints)
   - [x] VerificationsController (6 endpoints)
   - [x] ServicePackagesController (4 endpoints)
   - [x] SubscriptionsController (3 endpoints)
   - [x] PaymentsController (3 endpoints)
   - [x] TreatmentPackagesController (8 endpoints)
   - [x] NotificationsController (3 endpoints)
   - [x] CustomerSupportController (3 endpoints)
   - [x] BusinessManagerController (6 endpoints)
   - [x] AdminController (8 endpoints)

4. **External Service Abstractions** - ✅ DONE
   - [x] IEmailService, IFileStorageService, IPaymentService, IJwtTokenService
   - [x] Mock implementations for development

5. **Seed Data** - MEDIUM PRIORITY
   - [ ] Database seeders for:
     - Roles and permissions
     - Specializations
     - Service packages
     - Demo users (Admin, Patient, Doctor, CS, BM)
     - System configuration

6. **Unit Tests (OPCBS.Tests)** - 35 PASSING
   - [x] AppointmentService tests (13 tests)
   - [x] AuthService tests (16 tests)
   - [x] UserService tests (6 tests)
   - **Total: 35 tests — ALL PASSING**
   - [ ] DoctorService tests
   - [ ] ScheduleService tests
   - [ ] BlogService tests
   - [ ] SubscriptionService tests

## Key Implementation Notes

### Critical Business Rules to Enforce
1. Doctors must have VerificationStatus=Approved AND ServicePackageStatus=Active to:
   - Appear in search results
   - Receive appointment bookings
2. Unique constraints: Email, PhoneNumber, BookingCode
3. One-to-one relationships: AppointmentSlot ↔ Appointment, Appointment ↔ ConsultationRecord, Appointment ↔ Review
4. Treatment Packages are NOT paid (no VNPay)
5. Only Service Packages require VNPay payment

### API Design
- Base URL: `/api/v1`
- Standard response envelope with `success`, `message`, `data`, `errors`
- Pagination support with `page` and `pageSize` query parameters
- All dates in UTC format

### Database
- SQL Server with GUID primary keys
- Audit fields: CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsDeleted
- Soft deletes for User, DoctorProfile, TreatmentPackage, BlogPost, PatientProfile
- Immutable entities: OtpVerification, AppointmentHistory, AuditLog

## Setup and Running

### Prerequisites
- .NET 8 SDK
- SQL Server 2019 or later
- Visual Studio 2022 or VS Code

### Installation

1. **Database Setup**
   ```bash
   cd backend
   dotnet ef database update -p OPCBS.Infrastructure -s OPCBS
   ```

2. **Configuration**
   - Update `appsettings.json` with your SQL Server connection string
   - Configure external services (Brevo, VNPay, Cloudinary) credentials
   - Set JWT secret key

3. **Run API**
   ```bash
   cd backend/OPCBS
   dotnet run
   ```
   API will be available at `https://localhost:7001`
   Swagger docs at `https://localhost:7001/swagger`

4. **Run Web Frontend**
   ```bash
   cd backend/OPCBS.Web
   dotnet run
   ```
   Web will be available at `https://localhost:7101`

## Next Steps for Implementation

1. ✅ ~~ConsultationRecords module~~ — DONE
2. ✅ ~~Blogs module~~ — DONE
3. ✅ ~~Reviews module~~ — DONE
4. ✅ ~~TreatmentPackages module~~ — DONE
5. ✅ ~~ServicePackages + Subscriptions~~ — DONE
6. ✅ ~~Admin/CS/BM controllers~~ — DONE
7. **Seed Data** — For testing and development
8. **Additional Unit Tests** — Expand test coverage
9. **API Route Audit + Cleanup** — Final review
10. **Build Razor Pages** — Frontend implementation

## File Locations

- Domain Entities: `OPCBS.Domain/Entities/`
- Enums: `OPCBS.Domain/Enums/SystemEnums.cs`
- Base Classes: `OPCBS.Domain/Common/BaseEntity.cs`
- DbContext: `OPCBS.Infrastructure/Persistence/OpcbsDbContext.cs`
- Migration: `OPCBS.Infrastructure/Migrations/`

## Dependencies

- `Microsoft.EntityFrameworkCore 8.0.11`
- `AutoMapper 16.1.1` + `AutoMapper.Extensions.Microsoft.DependencyInjection`
- `FluentValidation 11.9.2`
- `Swashbuckle.AspNetCore 6.6.2`
- `Microsoft.AspNetCore.Authentication.JwtBearer 8.0.11`
- `BCrypt.Net-Next 4.0.3`
- `Moq 4.20.70` (Tests)
- `xUnit 2.5.3` (Tests)

## Support and Documentation

Refer to specification files in `docs/` folder for:
- Business rules: `docs/03-business-rules.md`
- API design: `docs/11-api-design.md`
- Validation rules: `docs/19-validation-rules.md`
- Database design: `docs/07-database-design.md`

---

**Status**: Active Development — All Backend API Modules Complete (19 controllers, 82+ endpoints)
**Last Updated**: June 26, 2026
**Build**: ✅ dotnet build — 0 errors, 0 warnings
**Tests**: ✅ dotnet test — 35/35 passing
**Lead Architect**: Full-stack .NET Engineer
