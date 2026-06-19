# 04 - Use Case Specification

# Online Psychological Counseling Booking System (OPCBS)

## 1. Purpose

This document describes the functional behavior of the Online Psychological Counseling Booking System (OPCBS) through detailed use case specifications.

Each use case defines how actors interact with the system, including business objectives, normal flows, alternative flows, exception handling, business rules, related screens, and impacted entities.

The purpose of this document is to serve as the primary source for:

* UI Design
* API Specification
* Database Design
* Sequence Diagrams
* Development Tasks
* Test Cases
* AI-Assisted Code Generation

---

# 2. Use Case Priority Classification

## Must Have

### Authentication

* UC-AUTH-01 Register Account
* UC-AUTH-02 Verify OTP
* UC-AUTH-03 Login
* UC-AUTH-04 Logout
* UC-AUTH-05 Forgot Password
* UC-AUTH-06 Manage Profile
* UC-AUTH-07 Change Password

### Doctor Discovery

* UC-DD-01 Browse Home Page
* UC-DD-02 Search Doctors
* UC-DD-03 View Doctor Profile
* UC-DD-04 View Doctor Schedule

### Appointment Management

* UC-BOOK-01 Book Appointment
* UC-BOOK-02 Track Booking
* UC-BOOK-03 View My Appointments
* UC-BOOK-04 Cancel Appointment
* UC-BOOK-05 Reschedule Appointment

### Doctor Operations

* UC-DOC-01 Configure Schedule
* UC-DOC-02 Review Appointment Request
* UC-DOC-03 Update Consultation Status
* UC-DOC-04 Manage Patient Records

### Doctor Verification

* UC-CS-01 Review Doctor Application
* UC-CS-02 Approve Doctor Application
* UC-CS-03 Reject Doctor Application

---

## Should Have

### Consultation Package

* UC-DOC-05 Manage Consultation Packages
* UC-PAT-05 Manage Purchased Packages

### Subscription

* UC-DOC-06 Purchase Subscription Package

### Blog

* UC-DOC-07 Manage Blog Content
* UC-CS-04 Review Blog Content
* UC-CS-05 Approve Blog Content
* UC-CS-06 Reject Blog Content

### Feedback

* UC-PAT-02 Submit Rating
* UC-PAT-03 Submit Feedback
* UC-PAT-04 Comment Blog

---

## Could Have

### Business Management

* UC-BM-01 Manage Subscription Packages
* UC-BM-02 Manage Specializations
* UC-BM-03 View Analytics Dashboard

### System Administration

* UC-SA-01 Manage Accounts
* UC-SA-02 Manage Roles
* UC-SA-03 Manage Permissions
* UC-SA-04 Manage System Configuration
* UC-SA-05 View Audit Logs
* UC-SA-06 View System Reports

---

# UC-AUTH-01 Register Account

## Module

Authentication

## Primary Actor

Guest

## Supporting Actors

Email Service

## Description

Allows a guest user to create a new patient account using email registration.

## Business Goal

Enable new users to access patient functionalities and maintain personalized consultation records.

## Preconditions

* User is not authenticated.
* Email address does not already exist.

## Trigger

User clicks the "Register" button from the Login Page.

## Main Success Flow

1. User opens Register Page.
2. User enters registration information.
3. User submits registration form.
4. System validates all required fields.
5. System verifies email uniqueness.
6. System creates account with status "Pending Verification".
7. System generates OTP code.
8. System sends OTP email.
9. System displays OTP Verification Page.

## Alternative Flows

### A1 – Existing Email

1. System detects existing email.
2. Registration request is rejected.
3. Error message is displayed.

## Exception Flows

### E1 – Email Service Failure

1. OTP email cannot be delivered.
2. System records failure log.
3. User is informed to request another OTP.

## Postconditions

### Success

* Account is created.
* OTP is generated.

### Failure

* No account is created.

## Business Rules

* AUTH-01
* AUTH-02
* NOTI-01

## Related Screens

* Register Page
* OTP Verification Page

## Related APIs

```http
POST /api/auth/register
```

## Related Database Entities

* User
* OTPVerification

## Priority

Must Have

## Frequency of Use

High

---

# UC-AUTH-02 Verify OTP

## Module

Authentication

## Primary Actor

Guest

## Supporting Actors

Email Service

## Description

Allows users to verify ownership of their email address using OTP.

## Business Goal

Activate newly created accounts.

## Preconditions

* Registration completed.
* OTP exists and has not expired.

## Trigger

User submits OTP code.

## Main Success Flow

1. User enters OTP code.
2. System validates OTP.
3. System activates account.
4. System marks OTP as used.
5. System redirects user to Login Page.

## Alternative Flows

### A1 – Invalid OTP

1. OTP validation fails.
2. Error message displayed.

### A2 – Expired OTP

1. OTP expiration detected.
2. User is required to request a new OTP.

## Postconditions

### Success

* Account status becomes Active.

## Business Rules

* AUTH-01

## Related Screens

* OTP Verification Page

## Related APIs

```http
POST /api/auth/verify-otp
```

## Related Database Entities

* User
* OTPVerification

## Priority

Must Have

## Frequency of Use

High

---

# UC-AUTH-03 Login

## Module

Authentication

## Primary Actor

Guest

## Supporting Actors

Google Authentication Service

## Description

Allows users to authenticate and access the system.

## Business Goal

Provide secure access to authorized resources.

## Preconditions

* Account is active.
* User credentials are valid.

## Trigger

User submits login form.

## Main Success Flow

1. User opens Login Page.
2. User enters credentials.
3. System validates credentials.
4. Authentication token is generated.
5. User is redirected to dashboard based on role.

## Alternative Flows

### A1 – Google Login

1. User selects Google Login.
2. System redirects to Google Authentication Service.
3. Authentication succeeds.
4. User is logged in.

## Exception Flows

### E1 – Invalid Credentials

1. Authentication fails.
2. Error message displayed.

## Postconditions

* Authenticated session created.

## Business Rules

* AUTH-03
* SEC-02

## Related Screens

* Login Page

## Related APIs

```http
POST /api/auth/login

POST /api/auth/google-login
```

## Related Database Entities

* User
* Role

## Priority

Must Have

## Frequency of Use

Very High
# UC-AUTH-04 Logout

## Module

Authentication

## Primary Actor

Patient, Doctor, Customer Support, Business Manager, System Admin

## Description

Allows authenticated users to terminate their current session securely.

## Business Goal

Protect user accounts and prevent unauthorized access after leaving the system.

## Preconditions

* User is authenticated.

## Trigger

User clicks Logout.

## Main Success Flow

1. User selects Logout.
2. System invalidates authentication token/session.
3. System clears cached authentication data.
4. User is redirected to Login Page.

## Postconditions

* User session is terminated.

## Related Screens

* Header Navigation
* Login Page

## Related APIs

POST /api/auth/logout

---

# UC-AUTH-05 Forgot Password

## Module

Authentication

## Primary Actor

Guest

## Supporting Actors

Email Service

## Main Success Flow

1. User opens Forgot Password Page.
2. User enters email.
3. System validates email existence.
4. System generates reset token.
5. System sends reset email.
6. User opens reset link.
7. User enters new password.
8. System updates password.

## Business Rules

* BR-22 Password must be hashed.
* BR-12 Email verification required.

---

# UC-AUTH-06 Manage Profile

## Module

Authentication

## Primary Actor

Patient, Doctor, Customer Support, Business Manager, System Admin

## Main Success Flow

1. User opens Profile Page.
2. System loads profile information.
3. User updates profile.
4. System validates data.
5. System saves changes.

## Postconditions

* Profile information updated.

---

# UC-AUTH-07 Change Password

## Module

Authentication

## Primary Actor

Authenticated User

## Main Success Flow

1. User opens Change Password Page.
2. User enters current password.
3. User enters new password.
4. System validates credentials.
5. System updates password.

## Business Rules

* BR-22 Password hashing.

---

# UC-DD-01 Browse Home Page

## Module

Doctor Discovery

## Primary Actor

Guest, Patient

## Main Success Flow

1. User accesses Home Page.
2. System displays:

   * Featured Doctors
   * Latest Blogs
   * Categories
   * System Information

## Business Rules

* BR-84 Display featured doctors.

---

# UC-DD-02 Search Doctors

## Module

Doctor Discovery

## Primary Actor

Guest, Patient

## Main Success Flow

1. User enters search criteria.
2. System searches doctors.
3. System returns matching results.

## Business Rules

* BR-56 Search by specialization.
* BR-83 Search by treatment category.
* BR-65 Hidden doctors are excluded.

---

# UC-DD-03 View Doctor Profile

## Module

Doctor Discovery

## Primary Actor

Guest, Patient

## Main Success Flow

1. User selects doctor.
2. System displays:

   * Biography
   * Experience
   * Ratings
   * Consultation Packages
   * Available Schedules

## Business Rules

* BR-54 Public profile information.
* BR-85 Biography required.
* BR-86 Consultation introduction required.

---

# UC-DD-04 View Doctor Schedule

## Module

Doctor Discovery

## Primary Actor

Guest, Patient

## Main Success Flow

1. User opens doctor schedule.
2. System displays available slots.
3. User may proceed to booking.

## Business Rules

* BR-18 Calendar only displays configured schedules.
* BR-07 Locked slots are hidden.

---

# UC-BOOK-01 Book Appointment

## Module

Appointment Booking

## Primary Actor

Guest, Patient

## Supporting Actors

Doctor
Email Service

## Main Success Flow

1. User selects doctor.
2. User selects consultation service.
3. User selects date.
4. User selects available slot.
5. User fills booking information.
6. System validates availability.
7. System creates appointment.
8. Appointment status = Pending.
9. System generates Booking Code.
10. Doctor receives notification.

## Alternative Flow

A1. Slot no longer available.

* System rejects booking.

## Business Rules

* BR-01 Customer information required.
* BR-06 No duplicate slot booking.
* BR-25 Cannot book in past.
* BR-88 Doctor must be active.

---

# UC-BOOK-02 Track Booking

## Module

Appointment Booking

## Primary Actor

Guest

## Main Success Flow

1. Guest enters Booking Code.
2. Guest enters Email.
3. System validates information.
4. Booking information is displayed.

## Business Rules

* BR-21 Guest tracking.

---

# UC-BOOK-03 View My Appointments

## Module

Appointment Management

## Primary Actor

Patient

## Main Success Flow

1. Patient opens appointment list.
2. System displays appointments.
3. Patient views status and details.

---

# UC-BOOK-04 Cancel Appointment

## Module

Appointment Management

## Primary Actor

Patient

## Main Success Flow

1. Patient selects appointment.
2. Patient submits cancellation request.
3. System validates cancellation policy.
4. Appointment status = Cancelled.

## Business Rules

* BR-19 Cancel at least 24 hours before appointment.

---

# UC-BOOK-05 Reschedule Appointment

## Module

Appointment Management

## Primary Actor

Patient

## Main Success Flow

1. Patient selects appointment.
2. Patient chooses new slot.
3. System validates availability.
4. Appointment updated.

## Business Rules

* BR-19 Reschedule at least 24 hours before appointment.

---

# UC-DOC-01 Configure Schedule

## Primary Actor

Doctor

## Main Success Flow

1. Doctor defines working days.
2. Doctor defines start/end time.
3. Doctor defines slot duration.
4. System generates slots.

## Business Rules

* BR-17 One active schedule.
* BR-45 Doctor manages schedules.

---

# UC-DOC-02 Review Appointment Request

## Primary Actor

Doctor

## Main Success Flow

1. Doctor views pending requests.
2. Doctor approves or rejects request.
3. System updates appointment status.

## Business Rules

* BR-50 Doctor accepts/rejects bookings.

---

# UC-DOC-03 Update Consultation Status

Status Flow:
Pending → Approved → InProgress → Completed

## Business Rules

* BR-31 Auto complete consultation.
* BR-44 Doctor updates consultation status.

---

# UC-DOC-04 Manage Patient Records

## Primary Actor

Doctor

## Main Success Flow

1. Doctor opens patient record.
2. Doctor adds notes.
3. Doctor adds treatment recommendations.
4. Doctor saves consultation record.

## Business Rules

* BR-43 Access own patients only.

---

# UC-DOC-05 Manage Consultation Packages

## Primary Actor

Doctor

## Main Success Flow

1. Doctor creates package.
2. Doctor updates package.
3. Doctor activates package.
4. Package displayed publicly.

## Business Rules

* BR-76
* BR-77
* BR-78
* BR-81

---

# UC-DOC-06 Purchase Subscription Package

## Primary Actor

Doctor

## Supporting Actors

Payment Gateway

## Main Success Flow

1. Doctor selects subscription.
2. System generates QR payment.
3. Payment processed.
4. Subscription activated.

## Business Rules

* BR-67
* BR-68
* BR-69

---

# UC-DOC-07 Manage Blog Content

## Primary Actor

Doctor

## Main Success Flow

1. Doctor creates blog.
2. Blog status = Draft.
3. Doctor submits for review.
4. Blog status = PendingReview.

## Business Rules

* BR-46
* BR-47
* BR-75
# UC-CS-01 Review Doctor Application

## Module

Doctor Verification

## Primary Actor

Customer Support

## Description

Allows Customer Support staff to review doctor registration applications before making an approval recommendation.

## Business Goal

Ensure only qualified and verified doctors are allowed to provide services on the platform.

## Preconditions

* Doctor application status = Pending.
* Doctor has submitted all required information and certificates.

## Trigger

Customer Support opens Doctor Application Management Page.

## Main Success Flow

1. Customer Support views pending applications.
2. Customer Support opens application details.
3. System displays:

   * Personal Information
   * Professional Biography
   * Specializations
   * Certificates
   * Experience Information
4. Customer Support reviews submitted information.
5. Customer Support records review result.

## Alternative Flows

### A1 – Missing Information

1. Customer Support detects missing information.
2. Application is flagged for rejection.

## Postconditions

### Success

* Application review completed.

## Business Rules

* BR-42
* BR-74
* BR-85
* BR-86

## Related Screens

* Doctor Application Management Page
* Doctor Application Detail Page

---

# UC-CS-02 Approve Doctor Application

## Module

Doctor Verification

## Primary Actor

Customer Support

## Description

Approves a doctor application and activates the doctor account.

## Preconditions

* Application status = Pending.
* Review completed.

## Main Success Flow

1. Customer Support approves application.
2. System updates application status = Approved.
3. System creates Doctor Profile.
4. System activates doctor account.
5. System sends approval notification.

## Postconditions

* Doctor account becomes active.
* Doctor profile becomes available for scheduling.

## Business Rules

* BR-42
* BR-52
* BR-85
* BR-86

---

# UC-CS-03 Reject Doctor Application

## Module

Doctor Verification

## Primary Actor

Customer Support

## Description

Rejects an unqualified or incomplete doctor application.

## Preconditions

* Application status = Pending.

## Main Success Flow

1. Customer Support selects Reject.
2. Customer Support enters rejection reason.
3. System updates application status = Rejected.
4. System sends rejection notification.

## Business Rules

* BR-42
* BR-74

---

# UC-CS-04 Review Blog Content

## Module

Blog Management

## Primary Actor

Customer Support

## Description

Reviews doctor-created blog content before publication.

## Preconditions

* Blog status = PendingReview.

## Main Success Flow

1. Customer Support opens pending blog list.
2. Customer Support views blog content.
3. Customer Support evaluates content quality.
4. Customer Support selects approval decision.

## Business Rules

* BR-47
* BR-75

## Related Screens

* Content Moderation Page

---

# UC-CS-05 Approve Blog Content

## Module

Blog Management

## Primary Actor

Customer Support

## Main Success Flow

1. Customer Support approves blog.
2. System updates:

   * Status = Approved
3. System publishes blog.
4. Status = Published.
5. Blog becomes publicly visible.

## Business Rules

* BR-15
* BR-16
* BR-47

## Status Flow

PendingReview → Approved → Published

---

# UC-CS-06 Reject Blog Content

## Module

Blog Management

## Primary Actor

Customer Support

## Main Success Flow

1. Customer Support rejects blog.
2. Customer Support enters rejection reason.
3. System updates status = Rejected.
4. Doctor receives notification.

## Business Rules

* BR-16
* BR-47

## Status Flow

PendingReview → Rejected

---

# UC-BM-01 Manage Subscription Packages

## Module

Subscription Management

## Primary Actor

Business Manager

## Description

Create and manage doctor subscription packages.

## Main Success Flow

1. Business Manager opens Package Management Page.
2. Business Manager creates package.
3. System validates package information.
4. Package is saved.

## Available Actions

* Create Package
* Update Package
* Activate Package
* Deactivate Package

## Business Rules

* BR-68
* BR-71

---

# UC-BM-02 Manage Specializations

## Module

Specialization Management

## Primary Actor

Business Manager

## Description

Manage medical specializations and treatment categories available in the system.

## Main Success Flow

1. Business Manager opens Specialization Management Page.
2. Create specialization.
3. Update specialization.
4. Activate or deactivate specialization.

## Business Rules

* BR-52
* BR-53

---

# UC-BM-03 View Analytics Dashboard

## Module

Analytics & Reporting

## Primary Actor

Business Manager

## Description

View operational and business statistics.

## Main Success Flow

1. Business Manager opens Analytics Dashboard.
2. System displays:

   * Total Users
   * Total Doctors
   * Total Appointments
   * Active Subscriptions
   * Revenue Statistics
   * Popular Specializations
3. Business Manager filters reports.

## Related Screens

* Analytics Dashboard Page

---

# UC-SA-01 Manage Accounts

## Module

System Administration

## Primary Actor

System Admin

## Description

Manage user accounts within the platform.

## Main Success Flow

1. System Admin opens Account Management Page.
2. Search account.
3. View account details.
4. Lock account.
5. Unlock account.
6. Disable account.

## Business Rules

* BR-62

---

# UC-SA-02 Manage Roles

## Module

System Administration

## Primary Actor

System Admin

## Description

Manage system roles and access structures.

## Main Success Flow

1. System Admin views role list.
2. Create role.
3. Update role.
4. Disable role.

## Related Entities

* Role
* Permission

---

# UC-SA-03 Manage Permissions

## Module

System Administration

## Primary Actor

System Admin

## Description

Assign permissions to roles.

## Main Success Flow

1. System Admin selects role.
2. System Admin assigns permissions.
3. System saves configuration.

## Related Entities

* Permission
* RolePermission

---

# UC-SA-04 Manage System Configuration

## Module

System Administration

## Primary Actor

System Admin

## Description

Configure global platform settings.

## Main Success Flow

1. System Admin opens System Configuration.
2. Configure:

   * Email Settings
   * Security Settings
   * Booking Policies
   * Subscription Settings
3. System saves configuration.

---

# UC-SA-05 View Audit Logs

## Module

System Administration

## Primary Actor

System Admin

## Description

Monitor system activities and security events.

## Main Success Flow

1. System Admin opens Audit Logs.
2. Filter logs.
3. View detailed activities.

## Log Examples

* Login
* Logout
* Booking Creation
* Doctor Approval
* Package Purchase
* Role Changes

---

# UC-SA-06 View System Reports

## Module

System Administration

## Primary Actor

System Admin

## Description

Generate administrative and operational reports.

## Main Success Flow

1. System Admin opens Reports Module.
2. Select report type.
3. System generates report.
4. Export PDF/Excel.

## Report Types

* User Reports
* Doctor Reports
* Appointment Reports
* Subscription Reports
* System Activity Reports
# UC-CS-01 Review Doctor Application

## Module

Doctor Verification

## Primary Actor

Customer Support

## Description

Allows Customer Support staff to review doctor registration applications before making an approval recommendation.

## Business Goal

Ensure only qualified and verified doctors are allowed to provide services on the platform.

## Preconditions

* Doctor application status = Pending.
* Doctor has submitted all required information and certificates.

## Trigger

Customer Support opens Doctor Application Management Page.

## Main Success Flow

1. Customer Support views pending applications.
2. Customer Support opens application details.
3. System displays:

   * Personal Information
   * Professional Biography
   * Specializations
   * Certificates
   * Experience Information
4. Customer Support reviews submitted information.
5. Customer Support records review result.

## Alternative Flows

### A1 – Missing Information

1. Customer Support detects missing information.
2. Application is flagged for rejection.

## Postconditions

### Success

* Application review completed.

## Business Rules

* BR-42
* BR-74
* BR-85
* BR-86

## Related Screens

* Doctor Application Management Page
* Doctor Application Detail Page

---

# UC-CS-02 Approve Doctor Application

## Module

Doctor Verification

## Primary Actor

Customer Support

## Description

Approves a doctor application and activates the doctor account.

## Preconditions

* Application status = Pending.
* Review completed.

## Main Success Flow

1. Customer Support approves application.
2. System updates application status = Approved.
3. System creates Doctor Profile.
4. System activates doctor account.
5. System sends approval notification.

## Postconditions

* Doctor account becomes active.
* Doctor profile becomes available for scheduling.

## Business Rules

* BR-42
* BR-52
* BR-85
* BR-86

---

# UC-CS-03 Reject Doctor Application

## Module

Doctor Verification

## Primary Actor

Customer Support

## Description

Rejects an unqualified or incomplete doctor application.

## Preconditions

* Application status = Pending.

## Main Success Flow

1. Customer Support selects Reject.
2. Customer Support enters rejection reason.
3. System updates application status = Rejected.
4. System sends rejection notification.

## Business Rules

* BR-42
* BR-74

---

# UC-CS-04 Review Blog Content

## Module

Blog Management

## Primary Actor

Customer Support

## Description

Reviews doctor-created blog content before publication.

## Preconditions

* Blog status = PendingReview.

## Main Success Flow

1. Customer Support opens pending blog list.
2. Customer Support views blog content.
3. Customer Support evaluates content quality.
4. Customer Support selects approval decision.

## Business Rules

* BR-47
* BR-75

## Related Screens

* Content Moderation Page

---

# UC-CS-05 Approve Blog Content

## Module

Blog Management

## Primary Actor

Customer Support

## Main Success Flow

1. Customer Support approves blog.
2. System updates:

   * Status = Approved
3. System publishes blog.
4. Status = Published.
5. Blog becomes publicly visible.

## Business Rules

* BR-15
* BR-16
* BR-47

## Status Flow

PendingReview → Approved → Published

---

# UC-CS-06 Reject Blog Content

## Module

Blog Management

## Primary Actor

Customer Support

## Main Success Flow

1. Customer Support rejects blog.
2. Customer Support enters rejection reason.
3. System updates status = Rejected.
4. Doctor receives notification.

## Business Rules

* BR-16
* BR-47

## Status Flow

PendingReview → Rejected

---

# UC-BM-01 Manage Subscription Packages

## Module

Subscription Management

## Primary Actor

Business Manager

## Description

Create and manage doctor subscription packages.

## Main Success Flow

1. Business Manager opens Package Management Page.
2. Business Manager creates package.
3. System validates package information.
4. Package is saved.

## Available Actions

* Create Package
* Update Package
* Activate Package
* Deactivate Package

## Business Rules

* BR-68
* BR-71

---

# UC-BM-02 Manage Specializations

## Module

Specialization Management

## Primary Actor

Business Manager

## Description

Manage medical specializations and treatment categories available in the system.

## Main Success Flow

1. Business Manager opens Specialization Management Page.
2. Create specialization.
3. Update specialization.
4. Activate or deactivate specialization.

## Business Rules

* BR-52
* BR-53

---

# UC-BM-03 View Analytics Dashboard

## Module

Analytics & Reporting

## Primary Actor

Business Manager

## Description

View operational and business statistics.

## Main Success Flow

1. Business Manager opens Analytics Dashboard.
2. System displays:

   * Total Users
   * Total Doctors
   * Total Appointments
   * Active Subscriptions
   * Revenue Statistics
   * Popular Specializations
3. Business Manager filters reports.

## Related Screens

* Analytics Dashboard Page

---

# UC-SA-01 Manage Accounts

## Module

System Administration

## Primary Actor

System Admin

## Description

Manage user accounts within the platform.

## Main Success Flow

1. System Admin opens Account Management Page.
2. Search account.
3. View account details.
4. Lock account.
5. Unlock account.
6. Disable account.

## Business Rules

* BR-62

---

# UC-SA-02 Manage Roles

## Module

System Administration

## Primary Actor

System Admin

## Description

Manage system roles and access structures.

## Main Success Flow

1. System Admin views role list.
2. Create role.
3. Update role.
4. Disable role.

## Related Entities

* Role
* Permission

---

# UC-SA-03 Manage Permissions

## Module

System Administration

## Primary Actor

System Admin

## Description

Assign permissions to roles.

## Main Success Flow

1. System Admin selects role.
2. System Admin assigns permissions.
3. System saves configuration.

## Related Entities

* Permission
* RolePermission

---

# UC-SA-04 Manage System Configuration

## Module

System Administration

## Primary Actor

System Admin

## Description

Configure global platform settings.

## Main Success Flow

1. System Admin opens System Configuration.
2. Configure:

   * Email Settings
   * Security Settings
   * Booking Policies
   * Subscription Settings
3. System saves configuration.

---

# UC-SA-05 View Audit Logs

## Module

System Administration

## Primary Actor

System Admin

## Description

Monitor system activities and security events.

## Main Success Flow

1. System Admin opens Audit Logs.
2. Filter logs.
3. View detailed activities.

## Log Examples

* Login
* Logout
* Booking Creation
* Doctor Approval
* Package Purchase
* Role Changes

---

# UC-SA-06 View System Reports

## Module

System Administration

## Primary Actor

System Admin

## Description

Generate administrative and operational reports.

## Main Success Flow

1. System Admin opens Reports Module.
2. Select report type.
3. System generates report.
4. Export PDF/Excel.

## Report Types

* User Reports
* Doctor Reports
* Appointment Reports
* Subscription Reports
* System Activity Reports
