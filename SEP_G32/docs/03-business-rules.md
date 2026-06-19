# Business Rules

## Purpose

This document defines the business rules governing the Online Psychological Counseling Booking System (OPCBS).

Business rules establish constraints, policies, validations, and operational procedures that must be enforced throughout the system.

---

# Rule Categories

| Prefix | Category                            |
| ------ | ----------------------------------- |
| AUTH   | Authentication & Account Management |
| BOOK   | Appointment Booking                 |
| APPT   | Appointment Management              |
| SCHED  | Schedule Management                 |
| PAT    | Patient Management                  |
| DOC    | Doctor Management                   |
| BLOG   | Blog Management                     |
| RATE   | Rating & Feedback                   |
| PACK   | Consultation Package                |
| SUB    | Subscription                        |
| NOTI   | Notification                        |
| PAY    | Payment                             |
| SEC    | Security                            |
| ADMIN  | Administration                      |
| SYS    | System Rules                        |

---

# AUTHENTICATION RULES

| ID      | Business Rule                                                               |
| ------- | --------------------------------------------------------------------------- |
| AUTH-01 | Users must verify their email address via OTP before activating an account. |
| AUTH-02 | One email address can only be registered once in the system.                |
| AUTH-03 | Users must authenticate before accessing protected resources.               |
| AUTH-04 | Passwords must never be stored in plain text.                               |
| AUTH-05 | Expired authentication tokens must be rejected.                             |
| AUTH-06 | Google Authentication may be used as an alternative login method.           |
| AUTH-07 | Users may update profile information after authentication.                  |

---

# APPOINTMENT BOOKING RULES

| ID      | Business Rule                                                           |
| ------- | ----------------------------------------------------------------------- |
| BOOK-01 | Guests and Patients can create appointment bookings.                    |
| BOOK-02 | Customers must provide full name, phone number, and email when booking. |
| BOOK-03 | Users cannot book appointments in the past.                             |
| BOOK-04 | Appointments can only be booked with approved and active doctors.       |
| BOOK-05 | Appointment slots must belong to the doctor's published schedule.       |
| BOOK-06 | A doctor cannot receive multiple bookings for the same slot.            |
| BOOK-07 | The system must prevent duplicate bookings.                             |
| BOOK-08 | Booked slots must immediately become unavailable.                       |
| BOOK-09 | Customers cannot create overlapping appointments with the same doctor.  |
| BOOK-10 | Appointment creation must be executed within a database transaction.    |
| BOOK-11 | The system must generate a unique booking code for guest bookings.      |
| BOOK-12 | Guests may track appointments using booking code and email address.     |

---

# APPOINTMENT MANAGEMENT RULES

| ID      | Business Rule                                                                |
| ------- | ---------------------------------------------------------------------------- |
| APPT-01 | Appointment status must follow the defined workflow.                         |
| APPT-02 | Pending appointments may be Approved or Rejected by the doctor.              |
| APPT-03 | Approved appointments may progress to InProgress.                            |
| APPT-04 | InProgress appointments may progress to Completed.                           |
| APPT-05 | Patients may cancel appointments at least 24 hours before consultation time. |
| APPT-06 | Patients may request appointment rescheduling within the allowed period.     |
| APPT-07 | Appointment history must be retained permanently.                            |
| APPT-08 | The system must automatically update expired appointments when applicable.   |
| APPT-09 | Completed appointments cannot be modified.                                   |

---

# SCHEDULE MANAGEMENT RULES

| ID       | Business Rule                                                          |
| -------- | ---------------------------------------------------------------------- |
| SCHED-01 | Doctors can only maintain one active schedule configuration at a time. |
| SCHED-02 | Doctors may configure working days and consultation hours.             |
| SCHED-03 | Doctors may configure slot duration.                                   |
| SCHED-04 | The system must automatically generate appointment slots.              |
| SCHED-05 | Booked or expired slots must be locked automatically.                  |
| SCHED-06 | Booking calendars must display only available slots.                   |

---

# PATIENT MANAGEMENT RULES

| ID     | Business Rule                                           |
| ------ | ------------------------------------------------------- |
| PAT-01 | Patients may access only their own profile information. |
| PAT-02 | Patients may access only their own appointments.        |
| PAT-03 | Patients may view consultation history.                 |
| PAT-04 | Patients may manage personal information.               |

---

# DOCTOR MANAGEMENT RULES

| ID     | Business Rule                                                           |
| ------ | ----------------------------------------------------------------------- |
| DOC-01 | Doctors must submit professional certificates before activation.        |
| DOC-02 | Doctor applications must be approved before becoming publicly visible.  |
| DOC-03 | Doctors must provide a biography and consultation introduction.         |
| DOC-04 | Doctors must register at least one specialization.                      |
| DOC-05 | Doctors may register multiple treatment categories.                     |
| DOC-06 | Doctors may temporarily hide their profiles.                            |
| DOC-07 | Hidden, inactive, or expired doctors must not appear in search results. |
| DOC-08 | Doctors may access only their own appointments and patients.            |
| DOC-09 | Doctors may update consultation statuses.                               |
| DOC-10 | Doctors may add consultation notes and treatment recommendations.       |
| DOC-11 | Doctors may approve or reject appointment requests.                     |
| DOC-12 | Doctors may view ratings and feedback related to their services.        |

---

# BLOG MANAGEMENT RULES

| ID      | Business Rule                                         |
| ------- | ----------------------------------------------------- |
| BLOG-01 | Only doctors may create blog content.                 |
| BLOG-02 | Every blog post must belong to at least one category. |
| BLOG-03 | Blog posts must be reviewed before publication.       |
| BLOG-04 | Unapproved blogs must not be visible publicly.        |
| BLOG-05 | Doctors may edit only their own blog posts.           |
| BLOG-06 | Customer Support may approve or reject blogs.         |
| BLOG-07 | Published blogs may be archived.                      |

---

# RATING & FEEDBACK RULES

| ID      | Business Rule                                                       |
| ------- | ------------------------------------------------------------------- |
| RATE-01 | Only completed appointments may receive ratings.                    |
| RATE-02 | Each appointment may receive only one rating.                       |
| RATE-03 | Doctors cannot rate themselves.                                     |
| RATE-04 | The system must recalculate doctor ratings automatically.           |
| RATE-05 | Patients may submit written feedback after consultation completion. |

---

# CONSULTATION PACKAGE RULES

| ID      | Business Rule                                                                                       |
| ------- | --------------------------------------------------------------------------------------------------- |
| PACK-01 | Doctors may create consultation packages.                                                           |
| PACK-02 | Consultation packages must contain name, description, session quantity, price, and validity period. |
| PACK-03 | Active packages may be displayed publicly.                                                          |
| PACK-04 | Expired or inactive packages must not be displayed publicly.                                        |
| PACK-05 | Patients may purchase consultation packages.                                                        |
| PACK-06 | Package sessions must be deducted automatically after completed consultations.                      |
| PACK-07 | Expired packages cannot be used for new appointments.                                               |
| PACK-08 | Remaining sessions must be tracked automatically.                                                   |

---

# SUBSCRIPTION RULES

| ID     | Business Rule                                                       |
| ------ | ------------------------------------------------------------------- |
| SUB-01 | Doctors must have an active subscription before receiving bookings. |
| SUB-02 | Subscription packages may define feature limitations.               |
| SUB-03 | Subscription packages have expiration dates.                        |
| SUB-04 | Expired subscriptions automatically hide doctor profiles.           |
| SUB-05 | Business Managers may manage subscription packages.                 |
| SUB-06 | Subscription purchase history must be retained.                     |

---

# NOTIFICATION RULES

| ID      | Business Rule                                                     |
| ------- | ----------------------------------------------------------------- |
| NOTI-01 | OTP emails must be sent during registration.                      |
| NOTI-02 | Booking notifications must be sent after successful bookings.     |
| NOTI-03 | Appointment approval notifications must be sent to customers.     |
| NOTI-04 | Appointment rejection notifications must be sent to customers.    |
| NOTI-05 | Reminder emails must be sent 24 hours before appointments.        |
| NOTI-06 | Subscription activation notifications must be sent automatically. |

---

# PAYMENT RULES

| ID     | Business Rule                                           |
| ------ | ------------------------------------------------------- |
| PAY-01 | Subscription purchases must be processed through VNPay. |
| PAY-02 | Payment status must be verified before activation.      |
| PAY-03 | Payment transactions must be recorded permanently.      |
| PAY-04 | Failed transactions must not activate subscriptions.    |

---

# SECURITY RULES

| ID     | Business Rule                                                            |
| ------ | ------------------------------------------------------------------------ |
| SEC-01 | All communications must use HTTPS.                                       |
| SEC-02 | All protected APIs must validate authentication tokens.                  |
| SEC-03 | Input validation must be enforced on both client and server.             |
| SEC-04 | Users may access only resources they are authorized to access.           |
| SEC-05 | Role-based access control (RBAC) must be enforced throughout the system. |
| SEC-06 | Uploaded images must not exceed 5 MB.                                    |
| SEC-07 | Only JPG, PNG, and WEBP formats are accepted.                            |

---

# ADMINISTRATION RULES

| ID       | Business Rule                                                          |
| -------- | ---------------------------------------------------------------------- |
| ADMIN-01 | Customer Support may review doctor applications.                       |
| ADMIN-02 | Customer Support may moderate blog content.                            |
| ADMIN-03 | Business Managers may manage subscription packages.                    |
| ADMIN-04 | Business Managers may manage specializations and treatment categories. |
| ADMIN-05 | System Admins may manage users, roles, and permissions.                |
| ADMIN-06 | System Admins may configure platform settings.                         |

---

# SYSTEM RULES

| ID     | Business Rule                                                            |
| ------ | ------------------------------------------------------------------------ |
| SYS-01 | The system must support responsive web design.                           |
| SYS-02 | The system must retain historical business data.                         |
| SYS-03 | The system does not provide integrated video consultation functionality. |
| SYS-04 | The system operates as a web-based platform only.                        |
| SYS-05 | The system must support integration with Firebase, Resend, and VNPay.    |
