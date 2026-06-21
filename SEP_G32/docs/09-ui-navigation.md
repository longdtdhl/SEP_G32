# 09-ui-navigation.md

# 1. Purpose

This document defines the navigation structure of the Online Psychological Counseling Booking System (OPCBS).

The navigation model serves as the primary reference for:

* React Router configuration
* Layout design
* Sidebar generation
* Breadcrumb generation
* Route protection
* Role-based navigation

---

# 2. Navigation Principles

## Public Access

The following screens can be accessed without authentication:

* Home Page
* Doctor List
* Doctor Profile
* Doctor Schedule
* Blog List
* Blog Detail
* Register
* Login
* Forgot Password
* OTP Verification
* Guest Appointment Tracking

---

## Authenticated Access

The following screens require login:

* Profile Management
* Appointment Management
* Patient Records
* Treatment Packages
* Doctor Dashboard
* Customer Support Dashboard
* Business Manager Dashboard
* System Administrator Dashboard

---

## Role-Based Navigation

Navigation items shall be displayed according to the authenticated user's role.

---

# 3. Public Navigation

```text
Home
│
├── Doctor Discovery
│   ├── Doctor List
│   ├── Doctor Profile
│   └── Doctor Schedule
│
├── Blogs
│   ├── Blog List
│   └── Blog Detail
│
├── Track Appointment
│
├── Register
│
└── Login
```

---

# 4. Authentication Navigation

```text
Authentication
│
├── Register
│   └── OTP Verification
│
├── Login
│
├── Google Login
│
├── Forgot Password
│
└── Change Password
```

---

# 5. Patient Navigation

## Patient Layout

```text
Patient Dashboard
│
├── My Profile
│
├── My Appointments
│   ├── Appointment List
│   ├── Appointment Detail
│   ├── Cancel Appointment
│   └── Reschedule Appointment
│
├── Consultation History
│
├── Treatment Packages
│   ├── Proposed Packages
│   ├── Active Packages
│   └── Package Details
│
├── Feedback & Ratings
│
└── Logout
```

---

## Patient Menu

```text
Dashboard

Appointments

Consultation History

Packages

Profile

Change Password

Logout
```

---

# 6. Doctor Navigation

## Doctor Layout

```text
Doctor Dashboard
│
├── Profile Management
│
├── Verification
│   ├── Complete Profile
│   ├── Upload Certificates
│   ├── Submit Verification
│   └── Verification Status
│
├── Schedule Management
│   ├── Configure Schedule
│   ├── Unavailable Dates
│   └── Calendar View
│
├── Appointments
│   ├── Requests
│   ├── History
│   └── Consultation Status
│
├── Patient Records
│
├── Treatment Packages
│
├── Blog Management
│   ├── My Blogs
│   ├── Create Blog
│   ├── Edit Blog
│   └── Submit For Review
│
├── Subscription
│   ├── Available Plans
│   ├── Purchase Plan
│   └── Subscription Status
│
└── Logout
```

---

## Doctor Menu

```text
Dashboard

Verification

Schedules

Appointments

Patient Records

Treatment Packages

Blogs

Subscription

Profile

Change Password

Logout
```

---

# 7. Customer Support Navigation

## Customer Support Layout

```text
Customer Support Dashboard
│
├── Doctor Applications
│   ├── Pending Applications
│   ├── Review Application
│   ├── Approve Application
│   └── Reject Application
│
├── Blog Moderation
│   ├── Pending Blogs
│   ├── Review Blog
│   ├── Approve Blog
│   └── Reject Blog
│
└── Logout
```

---

## Customer Support Menu

```text
Dashboard

Doctor Applications

Blog Moderation

Profile

Logout
```

---

# 8. Business Manager Navigation

## Business Manager Layout

```text
Business Manager Dashboard
│
├── Subscription Packages
│
├── Specializations
│
├── Business Analytics
│
├── Operational Reports
│
└── Logout
```

---

## Business Manager Menu

```text
Dashboard

Subscription Packages

Specializations

Analytics

Reports

Profile

Logout
```

---

# 9. System Administrator Navigation

## System Administrator Layout

```text
System Dashboard
│
├── User Management
│
├── Role Management
│
├── Permission Management
│
├── System Settings
│
├── Audit Logs
│
├── Reports
│
└── Logout
```

---

## System Administrator Menu

```text
Dashboard

Users

Roles

Permissions

System Settings

Audit Logs

Reports

Profile

Logout
```

---

# 10. Route Structure

## Public Routes

```text
/

/doctors

/doctors/:id

/doctors/:id/schedule

/blogs

/blogs/:id

/register

/verify-otp

/login

/forgot-password

/track-appointment
```

---

## Patient Routes

```text
/patient

/patient/profile

/patient/appointments

/patient/appointments/:id

/patient/history

/patient/packages

/patient/packages/:id

/patient/feedback

/patient/change-password
```

---

## Doctor Routes

```text
/doctor

/doctor/profile

/doctor/verification

/doctor/schedules

/doctor/appointments

/doctor/appointments/:id

/doctor/patient-records

/doctor/packages

/doctor/blogs

/doctor/blogs/create

/doctor/blogs/:id/edit

/doctor/subscription
```

---

## Customer Support Routes

```text
/cs

/cs/applications

/cs/applications/:id

/cs/blogs

/cs/blogs/:id
```

---

## Business Manager Routes

```text
/bm

/bm/packages

/bm/specializations

/bm/analytics

/bm/reports
```

---

## System Administrator Routes

```text
/admin

/admin/users

/admin/roles

/admin/permissions

/admin/settings

/admin/audit-logs

/admin/reports
```

---

# 11. Route Protection Rules

| Role                 | Accessible Routes                    |
| -------------------- | ------------------------------------ |
| Guest                | Public Routes                        |
| Patient              | Public + Patient Routes              |
| Doctor               | Public + Doctor Routes               |
| Customer Support     | Public + Customer Support Routes     |
| Business Manager     | Public + Business Manager Routes     |
| System Administrator | Public + System Administrator Routes |

---

# 12. React Layout Structure

```text
src/
│
├── layouts/
│   ├── PublicLayout
│   ├── PatientLayout
│   ├── DoctorLayout
│   ├── CustomerSupportLayout
│   ├── BusinessManagerLayout
│   └── AdminLayout
│
├── routes/
│   ├── publicRoutes
│   ├── patientRoutes
│   ├── doctorRoutes
│   ├── customerSupportRoutes
│   ├── businessManagerRoutes
│   └── adminRoutes
```

This structure should be used consistently throughout the React application.
