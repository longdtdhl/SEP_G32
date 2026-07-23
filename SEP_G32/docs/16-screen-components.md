# 16-screen-components.md

# Purpose

Defines reusable UI components for OPCBS.

AI must reuse components instead of recreating UI.

---

# Public Components

## Navbar

Used By

```text
Home
Doctor List
Doctor Profile
Blogs
```

Features

```text
Logo
Navigation
Login
Register
```

---

## Footer

Used globally.

---

## SearchBar

Used By

```text
Doctor List
Blogs
```

---

# Doctor Components

## DoctorCard

Displays:

```text
Avatar
Name
Title
Experience
Rating
Specializations
```

Actions

```text
View Profile
```

---

## DoctorFilterPanel

Filters

```text
Specialization
Rating
Experience
```

---

## DoctorScheduleCalendar

Displays

```text
Available Slots
Booked Slots
```

---

# Appointment Components

## AppointmentBookingForm

Fields

```text
Doctor
Date
Time
Guest Information
Notes
```

---

## AppointmentStatusBadge

Statuses

```text
Pending
Approved
Rejected
Completed
Cancelled
```

---

## AppointmentTable

Used By

```text
Doctor Dashboard
Patient Dashboard
```

---

# Treatment Package Components

## PackageCard

Displays

```text
Name
Sessions
Price
Status
```

---

## PackageProgressBar

Displays

```text
Remaining Sessions
Used Sessions
```

---

# Blog Components

## BlogCard

Displays

```text
Thumbnail
Title
Author
Published Date
```

---

## BlogEditor

Rich Text Editor

Used By

```text
Doctor
```

---

# Review Components

## RatingStars

Displays

```text
1 - 5 Stars
```

---

# Dashboard Components

## DashboardStatsCard

Displays

```text
Total Appointments
Revenue
Blogs
Packages
```

---

## DashboardChart

Used By

```text
Business Manager
Admin
```

---

# Admin Components

## UserTable

Manage Users

---

## RoleTable

Manage Roles

---

## AuditLogTable

View Audit Logs

---

# Common Components

## ConfirmDialog

## LoadingSpinner

## EmptyState

## ErrorState

## Pagination

## ToastNotification

## FileUploader

Cloudinary Integration Required.
