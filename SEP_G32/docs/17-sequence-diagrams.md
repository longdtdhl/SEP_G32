# 17-sequence-diagrams.md

# Purpose

Defines critical business sequences for implementation.

---

# SD-01 Appointment Booking

```text
Patient/Guest
    |
    | Book Appointment
    v
System
    |
    | Validate Doctor
    | Validate Subscription
    | Validate Slot
    v
Create Appointment
    |
    v
Status = Pending
    |
    v
Send Email
```

---

# SD-02 Doctor Approve Appointment

```text
Doctor
    |
    | Approve
    v
System
    |
    | Update Appointment
    v
Status = Approved
    |
    v
Send Notification
```

---

# SD-03 Create Consultation Record

```text
Doctor
    |
    | Create Record
    v
System
    |
    | Validate Appointment
    v
Save Consultation Record
    |
    v
Appointment Completed
```

---

# SD-04 Treatment Package Assignment

```text
Doctor
    |
    | Create Package
    v
System
    |
    v
Assigned
    |
    v
Patient Notification
```

---

# SD-05 Treatment Package Acceptance

```text
Patient
    |
    | Accept
    v
System
    |
    v
Status = Active
```

---

# SD-06 Doctor Verification

```text
Doctor
    |
    | Submit Application
    v
Customer Support
    |
    | Review
    v
Approve / Reject
    |
    v
Update Verification Status
```

---

# SD-07 Blog Publishing

```text
Doctor
    |
    | Submit Blog
    v
Pending
    |
    v
Customer Support Review
    |
    v
Published
```

---

# SD-08 Service Package Purchase

```text
Doctor
    |
    | Select Package
    v
VNPay
    |
    | Payment Success
    v
System
    |
    v
Activate Subscription
```

---

# SD-09 Guest Track Appointment

```text
Guest
    |
    | Enter Booking Code
    v
System
    |
    | Find Appointment
    v
Display Status
```

---

# SD-10 Login

```text
User
    |
    | Login
    v
System
    |
    | Validate Credentials
    v
Generate JWT
    |
    v
Return Access Token
```
