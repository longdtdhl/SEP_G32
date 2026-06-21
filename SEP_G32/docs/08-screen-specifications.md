# 08-screen-specifications.md

# 1. Purpose

This document defines all screens of the Online Psychological Counseling Booking System (OPCBS).

The purpose of this document is to:

* Define screen responsibilities.
* Define screen components.
* Define user actions.
* Define screen-to-use-case mapping.
* Support React frontend implementation.
* Support API integration.

---

# 2. Public Screens

## SCR-PUB-01 Home Page

### Actors

* Guest
* Patient

### Purpose

Display featured doctors, blog articles, and platform information.

### Main Components

* Header Navigation
* Hero Banner
* Featured Doctors
* Search Bar
* Latest Blogs
* Footer

### Actions

* Search Doctors
* View Doctor Profile
* View Blog Details
* Register
* Login

### Related Use Cases

* UC-DD-01
* UC-DD-02
* UC-BLOG-01

---

## SCR-PUB-02 Doctor List Page

### Actors

* Guest
* Patient

### Purpose

Display searchable doctor directory.

### Main Components

* Search Bar
* Specialization Filter
* Rating Filter
* Doctor Cards
* Pagination

### Actions

* Search Doctors
* Filter Doctors
* View Doctor Profile

### Related Use Cases

* UC-DD-02

---

## SCR-PUB-03 Doctor Profile Page

### Actors

* Guest
* Patient

### Purpose

Display detailed doctor information.

### Main Components

* Doctor Information
* Biography
* Specializations
* Ratings
* Consultation Packages
* Available Schedule
* Book Appointment Button

### Actions

* View Schedule
* Book Appointment

### Related Use Cases

* UC-DD-03
* UC-DD-04
* UC-AB-01

---

## SCR-PUB-04 Doctor Schedule Page

### Actors

* Guest
* Patient

### Purpose

Display available consultation slots.

### Main Components

* Calendar
* Available Slots
* Slot Status Indicators

### Actions

* Select Slot
* Continue Booking

### Related Use Cases

* UC-DD-04
* UC-AB-01

---

## SCR-PUB-05 Blog List Page

### Actors

* Guest
* Patient

### Purpose

Display published blogs.

### Main Components

* Blog List
* Search
* Category Filter
* Pagination

### Actions

* View Blog Details

### Related Use Cases

* UC-BLOG-01

---

## SCR-PUB-06 Blog Detail Page

### Actors

* Guest
* Patient

### Purpose

Display blog content and comments.

### Main Components

* Blog Content
* Author Information
* Comments Section

### Actions

* Submit Comment

### Related Use Cases

* UC-BLOG-02
* UC-BLOG-07

---

# 3. Authentication Screens

## SCR-AUTH-01 Register Page

### Related Use Cases

* UC-AUTH-01

### Main Components

* Full Name
* Email
* Password
* Confirm Password

---

## SCR-AUTH-02 OTP Verification Page

### Related Use Cases

* UC-AUTH-02

### Main Components

* OTP Input
* Verify Button
* Resend OTP

---

## SCR-AUTH-03 Login Page

### Related Use Cases

* UC-AUTH-03

### Main Components

* Email
* Password
* Google Login
* Forgot Password

---

## SCR-AUTH-04 Forgot Password Page

### Related Use Cases

* UC-AUTH-05

### Main Components

* Email Input
* Reset Request Button

---

## SCR-AUTH-05 Profile Page

### Related Use Cases

* UC-AUTH-06

### Main Components

* Personal Information Form
* Avatar Upload
* Save Button

---

## SCR-AUTH-06 Change Password Page

### Related Use Cases

* UC-AUTH-07

### Main Components

* Current Password
* New Password
* Confirm Password

---

# 4. Appointment Screens

## SCR-AB-01 Booking Page

### Actors

* Guest
* Patient

### Purpose

Create appointment booking.

### Main Components

* Doctor Summary
* Slot Information
* Patient Information Form
* Consultation Notes
* Submit Button

### Related Use Cases

* UC-AB-01

---

## SCR-AB-02 Appointment Status Page

### Actors

* Patient

### Main Components

* Appointment List
* Status Badges
* Filters

### Related Use Cases

* UC-AB-02

---

## SCR-AB-03 Track Appointment Page

### Actors

* Guest

### Main Components

* Booking Code Input
* Email Input
* Search Button
* Appointment Result

### Related Use Cases

* UC-AB-05

---

# 5. Patient Screens

## SCR-PAT-01 Consultation History Page

### Actors

* Patient

### Main Components

* Appointment History
* Consultation Notes
* Treatment Recommendations

### Related Use Cases

* UC-PR-03

---

## SCR-PAT-02 Proposed Packages Page

### Actors

* Patient

### Main Components

* Package Cards
* Package Status
* Accept Button
* Reject Button

### Related Use Cases

* UC-CP-05
* UC-CP-06
* UC-CP-07

---

## SCR-PAT-03 Active Packages Page

### Actors

* Patient

### Main Components

* Active Package List
* Remaining Sessions
* Progress Indicator

### Related Use Cases

* UC-CP-08
* UC-CP-09
* UC-CP-10

---

## SCR-PAT-04 Feedback Page

### Actors

* Patient

### Main Components

* Rating Control
* Comment Box
* Submit Button

### Related Use Cases

* UC-FB-01

---

# 6. Doctor Screens

## SCR-DOC-01 Doctor Dashboard

### Related Use Cases

* UC-AM-01
* UC-AM-04
* UC-CP-10

### Widgets

* Appointment Summary
* Package Summary
* Rating Summary

---

## SCR-DOC-02 Schedule Management Page

### Related Use Cases

* UC-SM-01
* UC-SM-02
* UC-SM-03

### Main Components

* Weekly Calendar
* Slot Generator
* Unavailable Dates

---

## SCR-DOC-03 Appointment Requests Page

### Related Use Cases

* UC-AM-01
* UC-AM-02
* UC-AM-03

### Main Components

* Request Table
* Approve Button
* Reject Button

---

## SCR-DOC-04 Patient Records Page

### Related Use Cases

* UC-PR-01
* UC-PR-02

### Main Components

* Patient List
* Consultation Records
* Notes Editor

---

## SCR-DOC-05 Package Management Page

### Related Use Cases

* UC-CP-01
* UC-CP-02
* UC-CP-03
* UC-CP-04

### Main Components

* Package List
* Create Form
* Edit Form

---

## SCR-DOC-06 Blog Management Page

### Related Use Cases

* UC-BLOG-03
* UC-BLOG-04
* UC-BLOG-05
* UC-BLOG-06
* UC-BLOG-11

### Main Components

* Blog Table
* Create Blog
* Edit Blog
* Submit Review

---

## SCR-DOC-07 Doctor Verification Page

### Related Use Cases

* UC-DV-01
* UC-DV-02
* UC-DV-03
* UC-DV-07

### Main Components

* Professional Information
* Certificate Upload
* Verification Status

---

## SCR-DOC-08 Subscription Page

### Related Use Cases

* UC-SUB-01
* UC-SUB-02
* UC-SUB-03
* UC-PAY-01

### Main Components

* Package Cards
* Purchase Button
* Subscription Status

---

# 7. Customer Support Screens

## SCR-CS-01 Staff Dashboard

### Related Use Cases

* UC-STAFF-01

---

## SCR-CS-02 Doctor Application Management

### Related Use Cases

* UC-STAFF-02
* UC-DV-04
* UC-DV-05
* UC-DV-06

### Main Components

* Pending Applications Table
* Application Detail
* Approve Button
* Reject Button

---

## SCR-CS-03 Blog Moderation Page

### Related Use Cases

* UC-STAFF-03
* UC-BLOG-08
* UC-BLOG-09
* UC-BLOG-10

### Main Components

* Pending Blog List
* Blog Preview
* Approve Button
* Reject Button

---

# 8. Business Manager Screens

## SCR-BM-01 Subscription Package Management

### Related Use Cases

* UC-BM-01

---

## SCR-BM-02 Specialization Management

### Related Use Cases

* UC-BM-02

---

## SCR-BM-03 Business Analytics Dashboard

### Related Use Cases

* UC-BM-03

---

## SCR-BM-04 Operational Reports

### Related Use Cases

* UC-BM-04

---

# 9. System Administrator Screens

## SCR-ADMIN-01 User Management

### Related Use Cases

* UC-ADMIN-01

---

## SCR-ADMIN-02 Role Management

### Related Use Cases

* UC-ADMIN-02

---

## SCR-ADMIN-03 Permission Management

### Related Use Cases

* UC-ADMIN-03

---

## SCR-ADMIN-04 System Settings

### Related Use Cases

* UC-ADMIN-04

---

## SCR-ADMIN-05 Audit Logs

### Related Use Cases

* UC-ADMIN-05

---

## SCR-ADMIN-06 System Reports

### Related Use Cases

* UC-ADMIN-06

---

## SCR-ADMIN-07 System Dashboard

### Related Use Cases

* UC-ADMIN-07

---

# 10. Screen Naming Convention

```text
SCR-PUB     Public Screens
SCR-AUTH    Authentication
SCR-AB      Appointment Booking
SCR-PAT     Patient
SCR-DOC     Doctor
SCR-CS      Customer Support
SCR-BM      Business Manager
SCR-ADMIN   System Administrator
```

This naming convention shall be used consistently in wireframes, frontend implementation, API mapping, and testing documentation.
