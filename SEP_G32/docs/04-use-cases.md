# 04-use-cases.md

# OPCBS - Use Case Catalog

## 1. Purpose

This document provides a complete catalog of all business use cases supported by the Online Psychological Counseling Booking System (OPCBS).

The use cases identified in this document serve as the foundation for:

* Use Case Diagrams
* Use Case Specifications
* Activity Diagrams
* Sequence Diagrams
* API Specifications
* Screen Specifications
* Test Cases

Detailed use case descriptions are maintained separately in the Use Case Specification repository.

---

# 2. Actors

## Internal Actors

| Actor                | Description                                                               |
| -------------------- | ------------------------------------------------------------------------- |
| Guest                | Unregistered user who can browse doctors, blogs, and create appointments. |
| Patient              | Registered user seeking psychological consultation services.              |
| Doctor               | Verified psychologist providing consultation services.                    |
| Customer Support     | Reviews doctor applications and blog submissions.                         |
| Business Manager     | Manages business-related configurations and analytics.                    |
| System Administrator | Manages users, permissions, settings, and audit information.              |

---

## External Actors

| Actor                         | Description                             |
| ----------------------------- | --------------------------------------- |
| Google Authentication Service | OAuth authentication provider.          |
| Email Service                 | Sends OTP and notification emails.      |
| Payment Gateway               | Processes online subscription payments. |

---

# 3. Authentication Module

| ID         | Use Case Name    | Primary Actor   |
| ---------- | ---------------- | --------------- |
| UC-AUTH-01 | Register Account | Guest           |
| UC-AUTH-02 | Verify OTP       | Guest           |
| UC-AUTH-03 | Login            | User            |
| UC-AUTH-04 | Logout           | User            |
| UC-AUTH-05 | Forgot Password  | Guest           |
| UC-AUTH-06 | Manage Profile   | Patient, Doctor |
| UC-AUTH-07 | Change Password  | User            |

---

# 4. Doctor Discovery Module

| ID       | Use Case Name        | Primary Actor  |
| -------- | -------------------- | -------------- |
| UC-DD-01 | View Home Page       | Guest, Patient |
| UC-DD-02 | Search Doctors       | Guest, Patient |
| UC-DD-03 | View Doctor Profile  | Guest, Patient |
| UC-DD-04 | View Doctor Schedule | Guest, Patient |

---

# 5. Appointment Booking Module

| ID       | Use Case Name           | Primary Actor  |
| -------- | ----------------------- | -------------- |
| UC-AB-01 | Book Appointment        | Guest, Patient |
| UC-AB-02 | View Appointment Status | Patient        |
| UC-AB-03 | Cancel Appointment      | Patient        |
| UC-AB-04 | Reschedule Appointment  | Patient        |
| UC-AB-05 | Track Appointment       | Guest          |

---

# 6. Schedule Management Module

| ID       | Use Case Name              | Primary Actor |
| -------- | -------------------------- | ------------- |
| UC-SM-01 | Configure Working Schedule | Doctor        |
| UC-SM-02 | Manage Unavailable Dates   | Doctor        |
| UC-SM-03 | View Schedule Calendar     | Doctor        |

---

# 7. Appointment Management Module

| ID       | Use Case Name              | Primary Actor |
| -------- | -------------------------- | ------------- |
| UC-AM-01 | View Appointment Requests  | Doctor        |
| UC-AM-02 | Approve Appointment        | Doctor        |
| UC-AM-03 | Reject Appointment         | Doctor        |
| UC-AM-04 | View Appointment History   | Doctor        |
| UC-AM-05 | Update Consultation Status | Doctor        |

---

# 8. Patient Record Management Module

| ID       | Use Case Name             | Primary Actor   |
| -------- | ------------------------- | --------------- |
| UC-PR-01 | View Patient Records      | Doctor          |
| UC-PR-02 | Manage Patient Records    | Doctor          |
| UC-PR-03 | View Consultation History | Doctor, Patient |

---

# 9. Treatment Package Module

| ID       | Use Case Name                | Primary Actor   |
| -------- | ---------------------------- | --------------- |
| UC-CP-01 | Create Treatment Package     | Doctor          |
| UC-CP-02 | Update Treatment Package     | Doctor          |
| UC-CP-03 | Delete Treatment Package     | Doctor          |
| UC-CP-04 | Recommend Package To Patient | Doctor          |
| UC-CP-05 | View Proposed Packages       | Patient         |
| UC-CP-06 | Accept Package               | Patient         |
| UC-CP-07 | Reject Package               | Patient         |
| UC-CP-08 | View Active Packages         | Patient         |
| UC-CP-09 | View Package Details         | Patient         |
| UC-CP-10 | Track Package Progress       | Patient, Doctor |

---

# 10. Blog Management Module

| ID         | Use Case Name          | Primary Actor    |
| ---------- | ---------------------- | ---------------- |
| UC-BLOG-01 | View Blog Articles     | Guest, Patient   |
| UC-BLOG-02 | View Blog Details      | Guest, Patient   |
| UC-BLOG-03 | Create Blog            | Doctor           |
| UC-BLOG-04 | Update Blog            | Doctor           |
| UC-BLOG-05 | Delete Blog            | Doctor           |
| UC-BLOG-06 | Submit Blog For Review | Doctor           |
| UC-BLOG-07 | Submit Blog Comment    | Patient          |
| UC-BLOG-08 | Review Blog Content    | Customer Support |
| UC-BLOG-09 | Approve Blog Content   | Customer Support |
| UC-BLOG-10 | Reject Blog Content    | Customer Support |
| UC-BLOG-11 | View My Blogs          | Doctor           |

---

# 11. Feedback & Rating Module

| ID       | Use Case Name            | Primary Actor  |
| -------- | ------------------------ | -------------- |
| UC-FB-01 | Submit Feedback & Rating | Patient        |
| UC-FB-02 | View Doctor Ratings      | Guest, Patient |

---

# 12. Doctor Verification Module

| ID       | Use Case Name               | Primary Actor    |
| -------- | --------------------------- | ---------------- |
| UC-DV-01 | Complete Doctor Profile     | Doctor           |
| UC-DV-02 | Upload Certificates         | Doctor           |
| UC-DV-03 | Submit Verification Request | Doctor           |
| UC-DV-04 | Review Doctor Application   | Customer Support |
| UC-DV-05 | Approve Doctor Application  | Customer Support |
| UC-DV-06 | Reject Doctor Application   | Customer Support |
| UC-DV-07 | View Verification Status    | Doctor           |

---

# 13. Subscription Module

| ID        | Use Case Name               | Primary Actor |
| --------- | --------------------------- | ------------- |
| UC-SUB-01 | View Service Packages       | Doctor        |
| UC-SUB-02 | Purchase Service Package    | Doctor        |
| UC-SUB-03 | View Service Package Status | Doctor        |

---

# 14. Payment Integration Module

| ID        | Use Case Name                | Primary Actor |
| --------- | ---------------------------- | ------------- |
| UC-PAY-01 | Process Subscription Payment | Doctor        |

---

# 15. Customer Support Module

| ID          | Use Case Name                    | Primary Actor    |
| ----------- | -------------------------------- | ---------------- |
| UC-STAFF-01 | View Staff Dashboard             | Customer Support |
| UC-STAFF-02 | View Pending Doctor Applications | Customer Support |
| UC-STAFF-03 | View Pending Blogs               | Customer Support |

---

# 16. Business Manager Module

| ID       | Use Case Name                | Primary Actor    |
| -------- | ---------------------------- | ---------------- |
| UC-BM-01 | Manage Subscription Packages | Business Manager |
| UC-BM-02 | Manage Specializations       | Business Manager |
| UC-BM-03 | View Business Analytics      | Business Manager |
| UC-BM-04 | View Operational Reports     | Business Manager |

---

# 17. System Administration Module

| ID          | Use Case Name           | Primary Actor        |
| ----------- | ----------------------- | -------------------- |
| UC-ADMIN-01 | Manage Users            | System Administrator |
| UC-ADMIN-02 | Manage Roles            | System Administrator |
| UC-ADMIN-03 | Manage Permissions      | System Administrator |
| UC-ADMIN-04 | Manage System Settings  | System Administrator |
| UC-ADMIN-05 | View Audit Logs         | System Administrator |
| UC-ADMIN-06 | Generate System Reports | System Administrator |
| UC-ADMIN-07 | View System Dashboard   | System Administrator |

---

# 18. Summary

| Item               | Value |
| ------------------ | ----- |
| Internal Actors    | 6     |
| External Actors    | 3     |
| Functional Modules | 15    |
| Business Use Cases | 75    |

---

# 19. References

* 01-scope.md
* 02-actors.md
* 03-business-rules.md
* 05-use-case-specifications/*
* Screen Specifications
* Database Design
