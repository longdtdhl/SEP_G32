# 14-data-dictionary.md

# 1. Purpose

This document defines all database fields, data types, constraints, validation requirements, and business meanings used by the Online Psychological Counseling Booking System (OPCBS).

This document is the single source of truth for:

* Entity Framework Entities
* Database Migrations
* DTO Design
* Validation Rules
* API Development
* Business Logic Implementation

---

# 2. Common Audit Fields

The following fields exist in most entities.

| Field     | Type             | Required | Description             |
| --------- | ---------------- | -------- | ----------------------- |
| Id        | uniqueidentifier | Yes      | Primary Key             |
| CreatedAt | datetime2        | Yes      | Creation timestamp      |
| UpdatedAt | datetime2        | No       | Last update timestamp   |
| CreatedBy | uniqueidentifier | No       | User who created record |
| UpdatedBy | uniqueidentifier | No       | User who updated record |

---

# 3. Roles

## Roles

| Field       | Type          | Required | Constraints |
| ----------- | ------------- | -------- | ----------- |
| Id          | Guid          | Yes      | PK          |
| Name        | nvarchar(50)  | Yes      | Unique      |
| Description | nvarchar(500) | No       |             |

### Allowed Values

```text
Patient
Doctor
CustomerSupport
BusinessManager
SystemAdmin
```

---

# 4. Users

## Users

| Field        | Type           | Required | Constraints   |
| ------------ | -------------- | -------- | ------------- |
| Id           | Guid           | Yes      | PK            |
| Email        | nvarchar(255)  | Yes      | Unique        |
| PasswordHash | nvarchar(max)  | Yes      | BCrypt        |
| FullName     | nvarchar(255)  | Yes      |               |
| PhoneNumber  | varchar(20)    | Yes      | Unique        |
| AvatarUrl    | nvarchar(1000) | No       | Cloudinary    |
| Status       | varchar(50)    | Yes      | Enum          |
| RoleId       | Guid           | Yes      | FK Roles      |
| LastLoginAt  | datetime2      | No       |               |
| IsDeleted    | bit            | Yes      | Default False |

### Status

```text
Active
Inactive
Locked
```

---

# 5. PatientProfiles

## PatientProfiles

| Field                 | Type          |
| --------------------- | ------------- |
| Id                    | Guid          |
| UserId                | Guid          |
| DateOfBirth           | date          |
| Gender                | varchar(20)   |
| Address               | nvarchar(500) |
| EmergencyContactName  | nvarchar(255) |
| EmergencyContactPhone | varchar(20)   |

### Gender

```text
Male
Female
Other
```

---

# 6. DoctorProfiles

## DoctorProfiles

| Field                    | Type          |
| ------------------------ | ------------- |
| Id                       | Guid          |
| UserId                   | Guid          |
| Biography                | nvarchar(max) |
| ProfessionalTitle        | nvarchar(255) |
| ExperienceYears          | int           |
| ConsultationIntroduction | nvarchar(max) |
| TreatmentApproach        | nvarchar(max) |
| AverageRating            | decimal(2,1)  |
| VerificationStatus       | varchar(50)   |
| IsVisible                | bit           |

### VerificationStatus

```text
Draft
Submitted
Approved
Rejected
```

---

# 7. Specializations

## Specializations

| Field       | Type           |
| ----------- | -------------- |
| Id          | Guid           |
| Name        | nvarchar(255)  |
| Description | nvarchar(1000) |

---

# 8. VerificationRequests

## VerificationRequests

| Field           | Type           |
| --------------- | -------------- |
| Id              | Guid           |
| DoctorId        | Guid           |
| Status          | varchar(50)    |
| SubmittedAt     | datetime2      |
| ReviewedAt      | datetime2      |
| ReviewedBy      | Guid           |
| RejectionReason | nvarchar(1000) |

### Status

```text
Draft
Submitted
Approved
Rejected
```

---

# 9. Certificates

## Certificates

| Field                 | Type           |
| --------------------- | -------------- |
| Id                    | Guid           |
| VerificationRequestId | Guid           |
| CertificateName       | nvarchar(255)  |
| IssuedOrganization    | nvarchar(255)  |
| FileUrl               | nvarchar(1000) |
| IssuedDate            | date           |

---

# 10. Schedules

## Schedules

| Field        | Type         |
| ------------ | ------------ |
| Id           | Guid         |
| DoctorId     | Guid         |
| WorkingDays  | varchar(100) |
| StartTime    | time         |
| EndTime      | time         |
| SlotDuration | int          |

### Example

```text
WorkingDays = Mon,Tue,Wed,Fri
SlotDuration = 60
```

---

# 11. AppointmentSlots

## AppointmentSlots

| Field     | Type        |
| --------- | ----------- |
| Id        | Guid        |
| DoctorId  | Guid        |
| SlotDate  | date        |
| StartTime | time        |
| EndTime   | time        |
| Status    | varchar(50) |

### Status

```text
Available
Booked
Blocked
Expired
```

---

# 12. Appointments

## Appointments

| Field       | Type           |
| ----------- | -------------- |
| Id          | Guid           |
| BookingCode | varchar(20)    |
| PatientId   | Guid           |
| DoctorId    | Guid           |
| SlotId      | Guid           |
| GuestName   | nvarchar(255)  |
| GuestEmail  | nvarchar(255)  |
| GuestPhone  | varchar(20)    |
| Notes       | nvarchar(2000) |
| Status      | varchar(50)    |

### Status

```text
Pending
Approved
Rejected
Completed
Cancelled
```

### Business Rule

```text
One Slot
=
One Appointment
```

---

# 13. ConsultationRecords

## ConsultationRecords

| Field               | Type          |
| ------------------- | ------------- |
| Id                  | Guid          |
| AppointmentId       | Guid          |
| PatientId           | Guid          |
| DoctorId            | Guid          |
| ConsultationSummary | nvarchar(max) |
| Diagnosis           | nvarchar(max) |
| Recommendation      | nvarchar(max) |
| FollowUpNotes       | nvarchar(max) |

### Business Rule

```text
One Appointment
=
One ConsultationRecord
```

---

# 14. TreatmentPackages

## TreatmentPackages

| Field             | Type          |
| ----------------- | ------------- |
| Id                | Guid          |
| DoctorId          | Guid          |
| PatientId         | Guid          |
| Name              | nvarchar(255) |
| Description       | nvarchar(max) |
| SessionQuantity   | int           |
| RemainingSessions | int           |
| ValidityDays      | int           |
| Price             | decimal(18,2) |
| Status            | varchar(50)   |

### Status

```text
Created
Assigned
Active
Completed
Expired
Rejected
Cancelled
```

---

# 15. BlogPosts

## BlogPosts

| Field        | Type           |
| ------------ | -------------- |
| Id           | Guid           |
| DoctorId     | Guid           |
| Title        | nvarchar(500)  |
| ThumbnailUrl | nvarchar(1000) |
| Content      | nvarchar(max)  |
| Status       | varchar(50)    |
| PublishedAt  | datetime2      |

### Status

```text
Draft
Pending
Published
Rejected
Archived
```

---

# 16. BlogComments

## BlogComments

| Field     | Type           |
| --------- | -------------- |
| Id        | Guid           |
| BlogId    | Guid           |
| PatientId | Guid           |
| Content   | nvarchar(1000) |

---

# 17. Reviews

## Reviews

| Field         | Type           |
| ------------- | -------------- |
| Id            | Guid           |
| AppointmentId | Guid           |
| DoctorId      | Guid           |
| PatientId     | Guid           |
| Rating        | decimal(2,1)   |
| Comment       | nvarchar(1000) |

### Constraints

```text
Rating: 1 → 5
```

---

# 18. ServicePackages

## ServicePackages

| Field        | Type           |
| ------------ | -------------- |
| Id           | Guid           |
| Name         | nvarchar(255)  |
| Description  | nvarchar(1000) |
| Price        | decimal(18,2)  |
| DurationDays | int            |
| IsActive     | bit            |

### Purpose

Platform subscription package purchased by Doctors.

---

# 19. DoctorSubscriptions

## DoctorSubscriptions

| Field            | Type        |
| ---------------- | ----------- |
| Id               | Guid        |
| DoctorId         | Guid        |
| ServicePackageId | Guid        |
| StartDate        | datetime2   |
| EndDate          | datetime2   |
| Status           | varchar(50) |

### Status

```text
PendingPayment
Active
Expired
Cancelled
```

---

# 20. PaymentTransactions

## PaymentTransactions

| Field           | Type          |
| --------------- | ------------- |
| Id              | Guid          |
| SubscriptionId  | Guid          |
| TransactionCode | varchar(100)  |
| Amount          | decimal(18,2) |
| PaymentMethod   | varchar(50)   |
| PaymentStatus   | varchar(50)   |
| PaidAt          | datetime2     |

### PaymentMethod

```text
VNPay
```

### PaymentStatus

```text
Pending
Success
Failed
```

---

# 21. Notifications

## Notifications

| Field   | Type           |
| ------- | -------------- |
| Id      | Guid           |
| UserId  | Guid           |
| Title   | nvarchar(255)  |
| Content | nvarchar(2000) |
| Type    | varchar(50)    |
| IsRead  | bit            |

### Type

```text
OTP
Appointment
Verification
Subscription
Package
System
```

---

# 22. AuditLogs

## AuditLogs

| Field       | Type          |
| ----------- | ------------- |
| Id          | Guid          |
| UserId      | Guid          |
| Action      | nvarchar(255) |
| EntityName  | nvarchar(255) |
| EntityId    | Guid          |
| Description | nvarchar(max) |
| LoggedAt    | datetime2     |

### Purpose

Used by System Administrator for system monitoring and auditing.
