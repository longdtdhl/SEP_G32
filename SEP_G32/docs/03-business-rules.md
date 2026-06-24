# 03-business-rules.md

# Business Rules

## 1. Purpose

This document defines all business constraints, policies, validations, and operational rules for the Online Psychological Counseling Booking System (OPCBS).

Business Rules are the highest-priority source for:

* Domain Model
* Database Design
* API Design
* Validation Rules
* Workflow Design
* AI Code Generation

When conflicts occur:

```text
Business Rules
    ↓
Validation Rules
    ↓
API Design
    ↓
Screen Design
```

---

# 2. Rule Categories

| Prefix | Category                       |
| ------ | ------------------------------ |
| AUTH   | Authentication & Authorization |
| BOOK   | Appointment Booking            |
| APPT   | Appointment Management         |
| SCHED  | Schedule Management            |
| PAT    | Patient Management             |
| DOC    | Doctor Management              |
| RECORD | Consultation Records           |
| PACK   | Treatment Package              |
| SP     | Service Package                |
| BLOG   | Blog Management                |
| RATE   | Rating & Feedback              |
| VERIFY | Doctor Verification            |
| PAY    | Payment                        |
| NOTI   | Notification                   |
| FILE   | File Storage                   |
| AUDIT  | Audit Logging                  |
| SEC    | Security                       |
| ADMIN  | Administration                 |
| SYS    | System Rules                   |

---

# 3. Authentication Rules

| ID      | Business Rule                                                   |
| ------- | --------------------------------------------------------------- |
| AUTH-01 | Users must verify email via OTP before account activation.      |
| AUTH-02 | One email can only exist once in the system.                    |
| AUTH-03 | JWT authentication is required for protected resources.         |
| AUTH-04 | Passwords must be stored using BCrypt hashing.                  |
| AUTH-05 | Expired JWT tokens must be rejected.                            |
| AUTH-06 | Google OAuth login may be enabled through system configuration. |
| AUTH-07 | Users may update their own profile information.                 |
| AUTH-08 | Users may change their password only after authentication.      |

---

# 4. Appointment Booking Rules

| ID      | Business Rule                                                                                                        |
| ------- | -------------------------------------------------------------------------------------------------------------------- |
| BOOK-01 | Guests and Patients may create appointments.                                                                         |
| BOOK-02 | Guests must provide Full Name, Email, and Phone Number.                                                              |
| BOOK-03 | Appointments cannot be booked in the past.                                                                           |
| BOOK-04 | Appointments can only be booked with doctors that are Verified, Active, Visible, and have an Active Service Package. |
| BOOK-05 | Appointment slots must belong to the doctor's schedule.                                                              |
| BOOK-06 | One slot can only belong to one appointment.                                                                         |
| BOOK-07 | Duplicate booking attempts must be prevented.                                                                        |
| BOOK-08 | Booked slots become unavailable immediately.                                                                         |
| BOOK-09 | Patients cannot have overlapping appointments.                                                                       |
| BOOK-10 | Appointment creation must be transactional.                                                                          |
| BOOK-11 | The system must generate a unique Booking Code.                                                                      |
| BOOK-12 | Guests may track appointments using Booking Code and Email.                                                          |
| BOOK-13 | Doctors cannot create appointments on behalf of patients.                                                            |

---

# 5. Appointment Management Rules

| ID      | Business Rule                                                                                |
| ------- | -------------------------------------------------------------------------------------------- |
| APPT-01 | Appointment status must follow the defined state workflow.                                   |
| APPT-02 | Only doctors may approve or reject pending appointments.                                     |
| APPT-03 | Approved appointments may move to InProgress.                                                |
| APPT-04 | InProgress appointments may move to Completed.                                               |
| APPT-05 | Appointment cancellation policy is configurable. Default = 24 hours before appointment time. |
| APPT-06 | Appointment rescheduling policy is configurable.                                             |
| APPT-07 | Appointment history must never be deleted.                                                   |
| APPT-08 | Completed appointments cannot be modified.                                                   |
| APPT-09 | Appointment status changes must be audited.                                                  |

---

# 6. Schedule Management Rules

| ID       | Business Rule                                                |
| -------- | ------------------------------------------------------------ |
| SCHED-01 | Doctors can only maintain one active schedule configuration. |
| SCHED-02 | Doctors may configure working days and working hours.        |
| SCHED-03 | Slot duration must be configurable.                          |
| SCHED-04 | The system automatically generates appointment slots.        |
| SCHED-05 | Booked slots cannot be modified.                             |
| SCHED-06 | Expired slots are automatically locked.                      |
| SCHED-07 | Only available slots are shown to users.                     |

---

# 7. Patient Rules

| ID     | Business Rule                                                 |
| ------ | ------------------------------------------------------------- |
| PAT-01 | Patients may access only their own profile.                   |
| PAT-02 | Patients may access only their own appointments.              |
| PAT-03 | Patients may access only their own consultation history.      |
| PAT-04 | Patients may access only treatment packages assigned to them. |

---

# 8. Doctor Rules

| ID     | Business Rule                                                                                                  |
| ------ | -------------------------------------------------------------------------------------------------------------- |
| DOC-01 | Doctors must complete profile information before verification submission.                                      |
| DOC-02 | Doctors must upload professional certificates.                                                                 |
| DOC-03 | Doctors must have at least one specialization.                                                                 |
| DOC-04 | Doctors may have multiple specializations.                                                                     |
| DOC-05 | Doctors may temporarily hide their profiles.                                                                   |
| DOC-06 | Hidden doctors must not appear in search results.                                                              |
| DOC-07 | Doctors may only access their own appointments.                                                                |
| DOC-08 | Doctors may only access consultation records belonging to their patients.                                      |
| DOC-09 | Doctors may create Treatment Packages.                                                                         |
| DOC-10 | Doctors may create Blogs.                                                                                      |
| DOC-11 | Doctors may view ratings and feedback related to their services.                                               |
| DOC-12 | Doctors may receive appointments only when Verification Status = Approved and Service Package Status = Active. |

---

# 9. Consultation Record Rules

| ID        | Business Rule                                                   |
| --------- | --------------------------------------------------------------- |
| RECORD-01 | Each completed appointment generates one Consultation Record.   |
| RECORD-02 | One appointment can only have one Consultation Record.          |
| RECORD-03 | Only the assigned doctor may create Consultation Records.       |
| RECORD-04 | Consultation Records cannot be deleted.                         |
| RECORD-05 | Consultation Records remain accessible for historical purposes. |

---

# 10. Treatment Package Rules

## Definition

Treatment Package = Package created by a Doctor for a specific Patient.

Treatment Packages are NOT paid through VNPay.

| ID      | Business Rule                                                                             |
| ------- | ----------------------------------------------------------------------------------------- |
| PACK-01 | Doctors may create Treatment Packages for specific patients.                              |
| PACK-02 | Treatment Packages must contain Name, Description, Session Quantity, and Validity Period. |
| PACK-03 | Treatment Packages are private.                                                           |
| PACK-04 | Patients may Accept or Reject Treatment Packages.                                         |
| PACK-05 | Accepted Treatment Packages become Active.                                                |
| PACK-06 | Completed appointments deduct remaining sessions automatically.                           |
| PACK-07 | Packages become Completed when remaining sessions reach zero.                             |
| PACK-08 | Expired packages cannot be used.                                                          |
| PACK-09 | Treatment Packages do not require payment.                                                |

---

# 11. Service Package Rules

## Definition

Service Package = Subscription package purchased by Doctors to use OPCBS.

| ID    | Business Rule                                                              |
| ----- | -------------------------------------------------------------------------- |
| SP-01 | Doctors must have an Active Service Package before receiving appointments. |
| SP-02 | Service Packages are managed by Business Managers.                         |
| SP-03 | Service Packages define platform usage privileges.                         |
| SP-04 | Expired Service Packages automatically hide doctors from search results.   |
| SP-05 | Service Package history must be retained permanently.                      |
| SP-06 | Only Service Packages require online payment.                              |

---

# 12. Blog Rules

| ID      | Business Rule                                         |
| ------- | ----------------------------------------------------- |
| BLOG-01 | Only doctors may create blogs.                        |
| BLOG-02 | Blogs must be reviewed before publication.            |
| BLOG-03 | Unapproved blogs are not visible publicly.            |
| BLOG-04 | Doctors may edit only their own blogs.                |
| BLOG-05 | Customer Support reviews blog content.                |
| BLOG-06 | Published blogs may be archived.                      |
| BLOG-07 | Only approved and verified doctors may publish blogs. |

---

# 13. Rating Rules

| ID      | Business Rule                                        |
| ------- | ---------------------------------------------------- |
| RATE-01 | Only completed appointments may be rated.            |
| RATE-02 | One appointment may receive only one rating.         |
| RATE-03 | Doctors cannot rate themselves.                      |
| RATE-04 | Doctor average rating is recalculated automatically. |
| RATE-05 | Ratings cannot be modified after submission.         |

---

# 14. Doctor Verification Rules

| ID        | Business Rule                                               |
| --------- | ----------------------------------------------------------- |
| VERIFY-01 | Doctors must upload valid certificates.                     |
| VERIFY-02 | Verification requests must be reviewed by Customer Support. |
| VERIFY-03 | Only approved doctors may become publicly visible.          |
| VERIFY-04 | Rejected doctors may resubmit verification requests.        |

---

# 15. Payment Rules

| ID     | Business Rule                                               |
| ------ | ----------------------------------------------------------- |
| PAY-01 | Only Service Package purchases are processed through VNPay. |
| PAY-02 | Payment must be verified before subscription activation.    |
| PAY-03 | All payment transactions must be stored permanently.        |
| PAY-04 | Failed payments must not activate subscriptions.            |
| PAY-05 | Treatment Packages do not require payment processing.       |

---

# 16. Notification Rules

| ID      | Business Rule                                                          |
| ------- | ---------------------------------------------------------------------- |
| NOTI-01 | OTP emails must be sent during registration.                           |
| NOTI-02 | Booking confirmation emails must be sent automatically.                |
| NOTI-03 | Appointment approval notifications must be sent automatically.         |
| NOTI-04 | Appointment rejection notifications must be sent automatically.        |
| NOTI-05 | Appointment reminders are sent 24 hours before appointment time.       |
| NOTI-06 | Verification result notifications must be sent automatically.          |
| NOTI-07 | Treatment Package assignment notifications must be sent automatically. |
| NOTI-08 | Service Package expiration reminders must be sent automatically.       |

---

# 17. File Storage Rules

| ID      | Business Rule                                       |
| ------- | --------------------------------------------------- |
| FILE-01 | Doctor certificates must be stored in Cloudinary.   |
| FILE-02 | Doctor profile images must be stored in Cloudinary. |
| FILE-03 | Only JPG, PNG, and WEBP files are allowed.          |
| FILE-04 | Maximum upload size = 5 MB.                         |

---

# 18. Audit Rules

| ID       | Business Rule                                  |
| -------- | ---------------------------------------------- |
| AUDIT-01 | Role changes must be audited.                  |
| AUDIT-02 | Permission changes must be audited.            |
| AUDIT-03 | Doctor verification decisions must be audited. |
| AUDIT-04 | Service Package changes must be audited.       |
| AUDIT-05 | Audit logs cannot be modified.                 |

---

# 19. Security Rules

| ID     | Business Rule                                                |
| ------ | ------------------------------------------------------------ |
| SEC-01 | HTTPS is mandatory.                                          |
| SEC-02 | JWT validation is required for protected APIs.               |
| SEC-03 | Input validation must be enforced on both client and server. |
| SEC-04 | RBAC must be enforced throughout the system.                 |
| SEC-05 | Users may access only authorized resources.                  |
| SEC-06 | Sensitive information must never be logged.                  |

---

# 20. Administration Rules

| ID       | Business Rule                                                        |
| -------- | -------------------------------------------------------------------- |
| ADMIN-01 | Customer Support reviews doctor verification requests.               |
| ADMIN-02 | Customer Support moderates blog content.                             |
| ADMIN-03 | Business Managers manage Service Packages.                           |
| ADMIN-04 | Business Managers manage Specializations.                            |
| ADMIN-05 | System Admins manage Users, Roles, Permissions, and System Settings. |

---

# 21. System Rules

| ID     | Business Rule                                                          |
| ------ | ---------------------------------------------------------------------- |
| SYS-01 | OPCBS is a responsive web application.                                 |
| SYS-02 | Historical business data must be retained permanently.                 |
| SYS-03 | OPCBS does not provide built-in video consultation.                    |
| SYS-04 | OPCBS does not provide direct doctor-patient chat.                     |
| SYS-05 | The system integrates with Google OAuth, Brevo, VNPay, and Cloudinary. |
| SYS-06 | The system uses Role-Based Access Control (RBAC).                      |
