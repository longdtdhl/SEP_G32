# 05-domain-model.md

# 1. Purpose

This document defines the core business entities of the Online Psychological Counseling Booking System (OPCBS), their responsibilities, and relationships.

The domain model serves as the primary reference for backend entities, database design, API development, and business logic implementation.

---

# 2. Core Domain Areas

The OPCBS domain is organized into the following business areas:

1. Identity & Access Management
2. Doctor Management
3. Appointment Management
4. Patient Records
5. Treatment Packages
6. Blog Management
7. Doctor Verification
8. Subscription Management
9. Payment Management
10. Notification Management

---

# 3. Domain Entities

## 3.1 User

### Purpose

Represents an authenticated account within the system.

### Responsibilities

* Authentication
* Authorization
* Account management
* Password management

### Relationships

| Relationship          | Cardinality |
| --------------------- | ----------- |
| User → Role           | Many-to-One |
| User → DoctorProfile  | One-to-One  |
| User → PatientProfile | One-to-One  |
| User → Notifications  | One-to-Many |

---

## 3.2 Role

### Purpose

Defines access permissions within the platform.

### Roles

* Patient
* Doctor
* Customer Support
* Business Manager
* System Administrator

### Relationships

| Relationship | Cardinality |
| ------------ | ----------- |
| Role → Users | One-to-Many |

---

## 3.3 PatientProfile

### Purpose

Stores patient-specific information.

### Responsibilities

* Personal information
* Appointment ownership
* Package ownership
* Feedback submission

### Relationships

| Relationship                       | Cardinality |
| ---------------------------------- | ----------- |
| PatientProfile → User              | One-to-One  |
| PatientProfile → Appointments      | One-to-Many |
| PatientProfile → PatientRecords    | One-to-Many |
| PatientProfile → Reviews           | One-to-Many |
| PatientProfile → TreatmentPackages | One-to-Many |

---

## 3.4 DoctorProfile

### Purpose

Stores professional information of verified doctors.

### Responsibilities

* Professional profile
* Consultation services
* Working schedules
* Package management
* Blog management

### Relationships

| Relationship                    | Cardinality  |
| ------------------------------- | ------------ |
| DoctorProfile → User            | One-to-One   |
| DoctorProfile → Specializations | Many-to-Many |
| DoctorProfile → Schedules       | One-to-Many  |
| DoctorProfile → Appointments    | One-to-Many  |
| DoctorProfile → Blogs           | One-to-Many  |
| DoctorProfile → Reviews         | One-to-Many  |
| DoctorProfile → Subscriptions   | One-to-Many  |

---

## 3.5 Specialization

### Purpose

Represents a medical or psychological specialization.

### Relationships

| Relationship             | Cardinality  |
| ------------------------ | ------------ |
| Specialization → Doctors | Many-to-Many |

---

# 4. Appointment Domain

## 4.1 Schedule

### Purpose

Defines a doctor's availability configuration.

### Responsibilities

* Working days
* Working hours
* Slot generation

### Relationships

| Relationship                | Cardinality |
| --------------------------- | ----------- |
| Schedule → DoctorProfile    | Many-to-One |
| Schedule → AppointmentSlots | One-to-Many |

---

## 4.2 AppointmentSlot

### Purpose

Represents a bookable consultation slot.

### Status

* Available
* Booked
* Cancelled
* Expired
* Blocked
* Completed

### Relationships

| Relationship                  | Cardinality |
| ----------------------------- | ----------- |
| AppointmentSlot → Schedule    | Many-to-One |
| AppointmentSlot → Appointment | One-to-One  |

---

## 4.3 Appointment

### Purpose

Represents a consultation booking.

### Status

* Pending
* Approved
* Rejected
* InProgress
* Completed
* Cancelled

### Relationships

| Relationship                     | Cardinality |
| -------------------------------- | ----------- |
| Appointment → PatientProfile     | Many-to-One |
| Appointment → DoctorProfile      | Many-to-One |
| Appointment → AppointmentSlot    | One-to-One  |
| Appointment → Review             | One-to-One  |
| Appointment → ConsultationRecord | One-to-One  |

---

# 5. Patient Record Domain

## 5.1 ConsultationRecord

### Purpose

Stores consultation outcomes.

### Responsibilities

* Consultation notes
* Treatment recommendations
* Follow-up notes

### Relationships

| Relationship                        | Cardinality |
| ----------------------------------- | ----------- |
| ConsultationRecord → Appointment    | One-to-One  |
| ConsultationRecord → PatientProfile | Many-to-One |
| ConsultationRecord → DoctorProfile  | Many-to-One |

---

# 6. Treatment Package Domain

## 6.1 TreatmentPackage

### Purpose

Represents a treatment or consultation package proposed by a doctor.

### Status

* Created
* Assigned
* Accepted
* Active
* Completed
* Expired
* Rejected
* Cancelled

### Relationships

| Relationship                      | Cardinality |
| --------------------------------- | ----------- |
| TreatmentPackage → DoctorProfile  | Many-to-One |
| TreatmentPackage → PatientProfile | Many-to-One |

---

# 7. Blog Domain

## 7.1 BlogPost

### Purpose

Represents educational content created by doctors.

### Status

* Draft
* PendingReview
* Published
* Rejected
* Archived

### Relationships

| Relationship             | Cardinality |
| ------------------------ | ----------- |
| BlogPost → DoctorProfile | Many-to-One |
| BlogPost → BlogComments  | One-to-Many |

---

## 7.2 BlogComment

### Purpose

Represents patient comments on blog articles.

### Relationships

| Relationship                 | Cardinality |
| ---------------------------- | ----------- |
| BlogComment → BlogPost       | Many-to-One |
| BlogComment → PatientProfile | Many-to-One |

---

# 8. Feedback Domain

## 8.1 Review

### Purpose

Represents ratings and feedback submitted after completed consultations.

### Relationships

| Relationship            | Cardinality |
| ----------------------- | ----------- |
| Review → Appointment    | One-to-One  |
| Review → DoctorProfile  | Many-to-One |
| Review → PatientProfile | Many-to-One |

---

# 9. Doctor Verification Domain

## 9.1 VerificationRequest

### Purpose

Represents a doctor's verification application.

### Status

* Draft
* Submitted
* Approved
* Rejected
* Resubmitted

### Relationships

| Relationship                        | Cardinality |
| ----------------------------------- | ----------- |
| VerificationRequest → DoctorProfile | One-to-One  |
| VerificationRequest → Certificates  | One-to-Many |

---

## 9.2 Certificate

### Purpose

Stores uploaded professional certificates.

### Relationships

| Relationship                      | Cardinality |
| --------------------------------- | ----------- |
| Certificate → VerificationRequest | Many-to-One |

---

# 10. Subscription Domain

## 10.1 SubscriptionPlan

### Purpose

Represents a purchasable service package for doctors.

### Relationships

| Relationship                           | Cardinality |
| -------------------------------------- | ----------- |
| SubscriptionPlan → DoctorSubscriptions | One-to-Many |

---

## 10.2 DoctorSubscription

### Purpose

Represents a doctor's purchased subscription.

### Status

* PendingPayment
* Active
* Expired
* Cancelled

### Relationships

| Relationship                            | Cardinality |
| --------------------------------------- | ----------- |
| DoctorSubscription → DoctorProfile      | Many-to-One |
| DoctorSubscription → SubscriptionPlan   | Many-to-One |
| DoctorSubscription → PaymentTransaction | One-to-One  |

---

# 11. Payment Domain

## 11.1 PaymentTransaction

### Purpose

Stores payment transaction information.

### Responsibilities

* Payment tracking
* Payment auditing
* Subscription activation support

### Relationships

| Relationship                            | Cardinality |
| --------------------------------------- | ----------- |
| PaymentTransaction → DoctorSubscription | One-to-One  |

---

# 12. Notification Domain

## 12.1 Notification

### Purpose

Represents system-generated notifications.

### Responsibilities

* OTP delivery
* Appointment notifications
* Verification notifications
* Subscription notifications

### Relationships

| Relationship        | Cardinality |
| ------------------- | ----------- |
| Notification → User | Many-to-One |

---

# 13. Aggregate Roots

The following entities are considered aggregate roots:

* User
* DoctorProfile
* PatientProfile
* Appointment
* TreatmentPackage
* BlogPost
* VerificationRequest
* DoctorSubscription

These entities act as the primary entry points for business operations and transaction boundaries.

---
