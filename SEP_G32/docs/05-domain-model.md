# 05-domain-model.md

# 1. Purpose

This document defines the core business entities of the Online Psychological Counseling Booking System (OPCBS), their responsibilities, relationships, and business boundaries.

The domain model serves as the primary reference for:

* Database Design
* Entity Framework Models
* Business Services
* API Design
* Application Workflows

---

# 2. Core Domain Areas

The OPCBS domain is organized into the following business areas:

1. Identity & Access Management
2. Doctor Discovery
3. Appointment Management
4. Consultation Records
5. Treatment Package Management
6. Blog Management
7. Doctor Verification
8. Service Package Management
9. Payment Management
10. Notification Management

---

# 3. Identity & Access Domain

## 3.1 User

### Purpose

Represents an authenticated account within the platform.

### Responsibilities

* Authentication
* Authorization
* Profile ownership
* Account lifecycle management

### Relationships

| Relationship          | Cardinality           |
| --------------------- | --------------------- |
| User → Role           | Many-to-One           |
| User → DoctorProfile  | One-to-One (Optional) |
| User → PatientProfile | One-to-One (Optional) |

---

## 3.2 Role

### Purpose

Defines system permissions and access boundaries.

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
* Treatment package ownership
* Review ownership

### Relationships

| Relationship                       | Cardinality |
| ---------------------------------- | ----------- |
| PatientProfile → User              | One-to-One  |
| PatientProfile → Appointments      | One-to-Many |
| PatientProfile → Reviews           | One-to-Many |
| PatientProfile → TreatmentPackages | One-to-Many |

---

## 3.4 DoctorProfile

### Purpose

Stores professional information for verified doctors.

### Responsibilities

* Professional profile
* Schedule management
* Consultation management
* Blog ownership
* Treatment package ownership

### Relationships

| Relationship                          | Cardinality  |
| ------------------------------------- | ------------ |
| DoctorProfile → User                  | One-to-One   |
| DoctorProfile → Specializations       | Many-to-Many |
| DoctorProfile → Appointments          | One-to-Many  |
| DoctorProfile → ConsultationRecords   | One-to-Many  |
| DoctorProfile → Blogs                 | One-to-Many  |
| DoctorProfile → TreatmentPackages     | One-to-Many  |
| DoctorProfile → DoctorServicePackages | One-to-Many  |

---

## 3.5 Specialization

### Purpose

Represents a psychological specialization.

### Relationships

| Relationship             | Cardinality  |
| ------------------------ | ------------ |
| Specialization → Doctors | Many-to-Many |

---

# 4. Appointment Domain

## 4.1 Schedule

### Purpose

Defines doctor availability.

### Responsibilities

* Working days
* Working hours
* Consultation duration
* Slot generation

### Relationships

| Relationship             | Cardinality |
| ------------------------ | ----------- |
| Schedule → DoctorProfile | Many-to-One |

---

## 4.2 Appointment

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
| Appointment → ConsultationRecord | One-to-One  |
| Appointment → Review             | One-to-One  |

---

# 5. Consultation Domain

## 5.1 ConsultationRecord

### Purpose

Stores consultation outcomes and recommendations.

### Responsibilities

* Consultation notes
* Psychological assessment
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

Represents a counseling package created by a doctor and assigned to a patient.

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

Represents educational content published by doctors.

### Status

* Draft
* Submitted
* Approved
* Rejected

### Relationships

| Relationship             | Cardinality |
| ------------------------ | ----------- |
| BlogPost → DoctorProfile | Many-to-One |
| BlogPost → BlogComments  | One-to-Many |

---

## 7.2 BlogComment

### Purpose

Represents patient comments on blog posts.

### Relationships

| Relationship                 | Cardinality |
| ---------------------------- | ----------- |
| BlogComment → BlogPost       | Many-to-One |
| BlogComment → PatientProfile | Many-to-One |

---

# 8. Feedback Domain

## 8.1 Review

### Purpose

Represents ratings and feedback after completed consultations.

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

Represents a doctor verification application.

### Status

* Draft
* Submitted
* Approved
* Rejected

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

# 10. Service Package Domain

## 10.1 ServicePackage

### Purpose

Represents a subscription plan offered by OPCBS.

### Relationships

| Relationship                           | Cardinality |
| -------------------------------------- | ----------- |
| ServicePackage → DoctorServicePackages | One-to-Many |

---

## 10.2 DoctorServicePackage

### Purpose

Represents a doctor's purchased service package.

### Status

* PendingPayment
* Active
* Expired
* Cancelled

### Relationships

| Relationship                              | Cardinality |
| ----------------------------------------- | ----------- |
| DoctorServicePackage → DoctorProfile      | Many-to-One |
| DoctorServicePackage → ServicePackage     | Many-to-One |
| DoctorServicePackage → PaymentTransaction | One-to-One  |

---

# 11. Payment Domain

## 11.1 PaymentTransaction

### Purpose

Stores VNPay transaction information.

### Relationships

| Relationship                              | Cardinality |
| ----------------------------------------- | ----------- |
| PaymentTransaction → DoctorServicePackage | One-to-One  |

---

# 12. Notification Domain

## 12.1 Notification

### Purpose

Represents system-generated notifications.

### Types

* OTP
* Appointment
* Verification
* Subscription

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
* ConsultationRecord
* TreatmentPackage
* BlogPost
* VerificationRequest
* DoctorServicePackage

These entities define transaction boundaries and business ownership within OPCBS.
