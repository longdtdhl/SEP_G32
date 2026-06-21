# 07-database-design.md

# 1. Purpose

This document defines the database structure for the Online Psychological Counseling Booking System (OPCBS).

The database design serves as the primary reference for:

* Entity Framework Core entities
* Database migrations
* Repository implementation
* API development
* Domain relationships

---

# 2. Design Principles

## Primary Keys

All business tables use:

```text
Id (GUID)
```

as the primary key.

---

## Audit Fields

All entities should contain:

| Column    | Type            |
| --------- | --------------- |
| CreatedAt | datetime        |
| UpdatedAt | datetime        |
| CreatedBy | GUID (nullable) |
| UpdatedBy | GUID (nullable) |

---

## Soft Delete

Entities requiring logical deletion should contain:

| Column    | Type |
| --------- | ---- |
| IsDeleted | bit  |

Applicable entities:

* Users
* Doctors
* Blogs
* Packages
* Subscription Plans

---

# 3. Identity & Access Management

## Roles

### Purpose

Store system roles.

### Main Attributes

| Column      |
| ----------- |
| Id          |
| Name        |
| Description |

---

## Users

### Purpose

Store authentication information.

### Main Attributes

| Column       |
| ------------ |
| Id           |
| Email        |
| PasswordHash |
| FullName     |
| PhoneNumber  |
| Status       |
| RoleId       |

### Relationships

```text
Role (1) ----- (N) Users
```

---

# 4. Patient Domain

## PatientProfiles

### Purpose

Store patient-specific information.

### Main Attributes

| Column           |
| ---------------- |
| Id               |
| UserId           |
| DateOfBirth      |
| Gender           |
| Address          |
| EmergencyContact |

### Relationships

```text
User (1) ----- (1) PatientProfile
```

---

# 5. Doctor Domain

## DoctorProfiles

### Purpose

Store professional information.

### Main Attributes

| Column                   |
| ------------------------ |
| Id                       |
| UserId                   |
| Biography                |
| ConsultationIntroduction |
| TreatmentApproach        |
| ExperienceYears          |
| VerificationStatus       |
| IsVisible                |

### Relationships

```text
User (1) ----- (1) DoctorProfile
```

---

## Specializations

### Purpose

Store psychological specializations.

### Main Attributes

| Column      |
| ----------- |
| Id          |
| Name        |
| Description |

---

## DoctorSpecializations

### Purpose

Many-to-Many relationship.

### Main Attributes

| Column           |
| ---------------- |
| DoctorId         |
| SpecializationId |

### Relationships

```text
DoctorProfile (N)
        |
        |
DoctorSpecialization
        |
        |
Specialization (N)
```

---

# 6. Doctor Verification Domain

## VerificationRequests

### Purpose

Store doctor verification applications.

### Main Attributes

| Column          |
| --------------- |
| Id              |
| DoctorId        |
| Status          |
| SubmittedAt     |
| ReviewedAt      |
| ReviewedBy      |
| RejectionReason |

### Status

```text
Draft
Submitted
Approved
Rejected
Resubmitted
```

---

## Certificates

### Purpose

Store uploaded certificates.

### Main Attributes

| Column                |
| --------------------- |
| Id                    |
| VerificationRequestId |
| CertificateName       |
| FileUrl               |
| IssuedDate            |

---

# 7. Appointment Domain

## Schedules

### Purpose

Store doctor schedule configuration.

### Main Attributes

| Column       |
| ------------ |
| Id           |
| DoctorId     |
| WorkingDays  |
| StartTime    |
| EndTime      |
| SlotDuration |

---

## AppointmentSlots

### Purpose

Store generated booking slots.

### Main Attributes

| Column     |
| ---------- |
| Id         |
| ScheduleId |
| SlotDate   |
| StartTime  |
| EndTime    |
| Status     |

### Status

```text
Available
Booked
Cancelled
Expired
Blocked
Completed
```

---

## Appointments

### Purpose

Store booking information.

### Main Attributes

| Column      |
| ----------- |
| Id          |
| BookingCode |
| PatientId   |
| DoctorId    |
| SlotId      |
| Status      |
| Notes       |

### Status

```text
Pending
Approved
Rejected
InProgress
Completed
Cancelled
```

### Relationships

```text
PatientProfile (1)
      |
      |
Appointment
      |
      |
DoctorProfile (1)
```

---

# 8. Patient Records Domain

## PatientRecords

### Purpose

Store consultation records and treatment notes.

### Main Attributes

| Column                  |
| ----------------------- |
| Id                      |
| AppointmentId           |
| PatientId               |
| DoctorId                |
| ConsultationSummary     |
| TreatmentRecommendation |
| FollowUpNotes           |

### Relationships

```text
Appointment (1)
        |
        |
PatientRecord
```

---

# 9. Treatment Package Domain

## TreatmentPackages

### Purpose

Store treatment package proposals.

### Main Attributes

| Column            |
| ----------------- |
| Id                |
| DoctorId          |
| PatientId         |
| Name              |
| Description       |
| SessionQuantity   |
| RemainingSessions |
| ValidityDays      |
| Price             |
| Status            |

### Status

```text
Created
Assigned
Accepted
Active
Completed
Expired
Rejected
Cancelled
```

---

# 10. Blog Domain

## BlogPosts

### Purpose

Store doctor-created articles.

### Main Attributes

| Column      |
| ----------- |
| Id          |
| DoctorId    |
| Title       |
| Content     |
| Status      |
| PublishedAt |

### Status

```text
Draft
PendingReview
Published
Rejected
Archived
```

---

## BlogComments

### Purpose

Store blog comments.

### Main Attributes

| Column    |
| --------- |
| Id        |
| BlogId    |
| PatientId |
| Content   |

---

# 11. Feedback Domain

## Reviews

### Purpose

Store doctor ratings and feedback.

### Main Attributes

| Column        |
| ------------- |
| Id            |
| AppointmentId |
| DoctorId      |
| PatientId     |
| Rating        |
| Comment       |

### Constraints

```text
One Appointment
      |
      |
One Review
```

---

# 12. Subscription Domain

## SubscriptionPlans

### Purpose

Store available subscription packages.

### Main Attributes

| Column       |
| ------------ |
| Id           |
| Name         |
| Description  |
| Price        |
| DurationDays |

---

## DoctorSubscriptions

### Purpose

Store doctor subscriptions.

### Main Attributes

| Column    |
| --------- |
| Id        |
| DoctorId  |
| PlanId    |
| StartDate |
| EndDate   |
| Status    |

### Status

```text
PendingPayment
Active
Expired
Cancelled
```

---

# 13. Payment Domain

## PaymentTransactions

### Purpose

Store payment history.

### Main Attributes

| Column          |
| --------------- |
| Id              |
| SubscriptionId  |
| TransactionCode |
| Amount          |
| PaymentMethod   |
| PaymentStatus   |
| PaidAt          |

---

# 14. Notification Domain

## Notifications

### Purpose

Store system notifications.

### Main Attributes

| Column  |
| ------- |
| Id      |
| UserId  |
| Title   |
| Content |
| Type    |
| IsRead  |

---

# 15. Core Relationships Summary

```text
Role
 └── Users

Users
 ├── PatientProfiles
 └── DoctorProfiles

DoctorProfiles
 ├── DoctorSpecializations
 ├── VerificationRequests
 ├── Schedules
 ├── Appointments
 ├── Blogs
 ├── Reviews
 ├── TreatmentPackages
 └── DoctorSubscriptions

PatientProfiles
 ├── Appointments
 ├── PatientRecords
 ├── Reviews
 └── TreatmentPackages

Schedules
 └── AppointmentSlots

AppointmentSlots
 └── Appointments

Appointments
 ├── PatientRecords
 └── Reviews

DoctorSubscriptions
 └── PaymentTransactions
```

---

# 16. Aggregate Roots

The following entities should be treated as Aggregate Roots:

* User
* DoctorProfile
* PatientProfile
* Appointment
* PatientRecord
* TreatmentPackage
* BlogPost
* VerificationRequest
* DoctorSubscription

These entities define transaction boundaries within the system.
