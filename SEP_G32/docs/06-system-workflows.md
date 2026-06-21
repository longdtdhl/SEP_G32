# 04-system-workflows.md

## 1. Purpose

This document defines the major business workflows and state transitions within the Online Psychological Counseling Booking System (OPCBS).

The workflows described in this document serve as the primary reference for implementing business logic, status management, notification triggers, and system validations.

---

# 2. Authentication Workflow

## 2.1 User Registration

```text
Guest
↓
Register Account
↓
System Generates OTP
↓
Send OTP Email
↓
Verify OTP
↓
Account Activated
```

### Alternative Flow

```text
Guest
↓
Register Account
↓
OTP Verification Failed
↓
Account Remains Inactive
```

---

## 2.2 Forgot Password

```text
User
↓
Request Password Reset
↓
System Generates OTP
↓
Send OTP Email
↓
Verify OTP
↓
Reset Password
↓
Password Updated
```

---

# 3. Doctor Verification Workflow

## 3.1 Doctor Registration Process

```text
Doctor
↓
Complete Doctor Profile
↓
Upload Certificates
↓
Submit Verification Request
↓
Submitted
```

---

## 3.2 Verification Review Process

```text
Submitted
↓
Customer Support Review
↓
Approved
↓
Doctor Account Activated
```

### Rejection Flow

```text
Submitted
↓
Customer Support Review
↓
Rejected
↓
Doctor Updates Information
↓
Resubmitted
↓
Approved / Rejected
```

---

## 3.3 Doctor Verification Status

| Status      | Description                        |
| ----------- | ---------------------------------- |
| Draft       | Profile is incomplete              |
| Submitted   | Waiting for review                 |
| Approved    | Verification successful            |
| Rejected    | Verification failed                |
| Resubmitted | Updated and resubmitted for review |

---

# 4. Appointment Booking Workflow

## 4.1 Guest Booking

```text
Guest
↓
View Doctor Profile
↓
View Doctor Schedule
↓
Select Available Slot
↓
Enter Booking Information
↓
Submit Booking
↓
System Generates Booking Code
↓
Appointment Status = Pending
↓
Booking Confirmation Email Sent
```

---

## 4.2 Patient Booking

```text
Patient
↓
View Doctor Profile
↓
Select Available Slot
↓
Submit Booking
↓
Appointment Status = Pending
↓
Booking Confirmation Email Sent
```

---

## 4.3 Appointment Approval Process

```text
Pending
↓
Doctor Reviews Request
↓
Approved
↓
Approval Notification Sent
```

### Rejection Flow

```text
Pending
↓
Doctor Reviews Request
↓
Rejected
↓
Rejection Notification Sent
```

### Cancellation Flow

```text
Pending / Approved
↓
Patient Cancels Appointment
↓
Cancelled
```

---

# 5. Appointment Lifecycle Workflow

## 5.1 Appointment Status Flow

```text
Pending
↓
Approved
↓
InProgress
↓
Completed
```

### Rejection Flow

```text
Pending
↓
Rejected
```

### Cancellation Flow

```text
Pending / Approved
↓
Cancelled
```

---

## 5.2 Appointment Status Definitions

| Status     | Description               |
| ---------- | ------------------------- |
| Pending    | Waiting for doctor review |
| Approved   | Approved by doctor        |
| Rejected   | Rejected by doctor        |
| InProgress | Consultation is ongoing   |
| Completed  | Consultation finished     |
| Cancelled  | Appointment cancelled     |

---

# 6. Schedule Management Workflow

## 6.1 Schedule Configuration

```text
Doctor
↓
Configure Working Days
↓
Configure Working Hours
↓
Configure Slot Duration
↓
Save Schedule
↓
System Generates Appointment Slots
```

---

## 6.2 Slot Generation Process

```text
Schedule Saved
↓
Generate Available Slots
↓
Store Slots
↓
Display Booking Calendar
```

---

# 7. Appointment Slot Workflow

## 7.1 Slot Status Flow

```text
Available
↓
Booked
↓
Completed
```

### Cancellation Flow

```text
Booked
↓
Cancelled
```

### Expiration Flow

```text
Available
↓
Expired
```

### Blocked Flow

```text
Available
↓
Blocked
```

---

## 7.2 Slot Status Definitions

| Status    | Description                             |
| --------- | --------------------------------------- |
| Available | Ready for booking                       |
| Booked    | Reserved by appointment                 |
| Completed | Consultation completed                  |
| Cancelled | Released due to cancellation            |
| Expired   | Slot date/time passed                   |
| Blocked   | Unavailable due to doctor configuration |

---

# 8. Treatment Package Workflow

## 8.1 Package Proposal Process

```text
Doctor
↓
Create Treatment Package
↓
Assign To Patient
↓
Assigned
```

---

## 8.2 Patient Decision Process

```text
Assigned
↓
Patient Accepts
↓
Accepted
↓
Active
```

### Rejection Flow

```text
Assigned
↓
Patient Rejects
↓
Rejected
```

---

## 8.3 Package Completion Process

```text
Active
↓
All Sessions Consumed
↓
Completed
```

### Expiration Flow

```text
Active
↓
Validity Period Exceeded
↓
Expired
```

### Cancellation Flow

```text
Assigned / Active
↓
Cancelled
```

---

## 8.4 Package Status Definitions

| Status    | Description                 |
| --------- | --------------------------- |
| Created   | Package created             |
| Assigned  | Assigned to patient         |
| Accepted  | Accepted by patient         |
| Active    | Currently usable            |
| Completed | All sessions used           |
| Expired   | Validity period ended       |
| Rejected  | Rejected by patient         |
| Cancelled | Cancelled before completion |

---

# 9. Blog Moderation Workflow

## 9.1 Blog Publishing Process

```text
Doctor
↓
Create Blog
↓
Draft
↓
Submit For Review
↓
PendingReview
↓
Customer Support Review
↓
Published
```

---

## 9.2 Blog Rejection Process

```text
PendingReview
↓
Rejected
↓
Doctor Updates Content
↓
PendingReview
```

---

## 9.3 Blog Archiving Process

```text
Published
↓
Archived
```

---

## 9.4 Blog Status Definitions

| Status        | Description             |
| ------------- | ----------------------- |
| Draft         | Being edited            |
| PendingReview | Waiting for moderation  |
| Published     | Visible to users        |
| Rejected      | Rejected by moderator   |
| Archived      | Hidden from public view |

---

# 10. Subscription Workflow

## 10.1 Subscription Purchase Process

```text
Doctor
↓
Select Subscription Package
↓
Proceed To Payment
↓
PendingPayment
```

---

## 10.2 Successful Payment Flow

```text
PendingPayment
↓
Payment Successful
↓
Active
↓
Doctor Profile Available For Booking
```

---

## 10.3 Failed Payment Flow

```text
PendingPayment
↓
Payment Failed
↓
Cancelled
```

---

## 10.4 Expiration Flow

```text
Active
↓
Expiration Date Reached
↓
Expired
↓
Doctor Hidden From Search Results
```

---

# 11. Payment Workflow

## 11.1 Subscription Payment

```text
Doctor
↓
Select Subscription Package
↓
Create Payment Request
↓
Redirect To VNPAY
↓
Payment Processing
↓
Return Payment Result
```

### Success

```text
Payment Successful
↓
Activate Subscription
↓
Store Transaction
```

### Failure

```text
Payment Failed
↓
Store Transaction
↓
Subscription Remains Inactive
```

---

# 12. Notification Workflow

## 12.1 Email Notification Process

```text
Business Event Triggered
↓
Create Notification
↓
Queue Email
↓
Send Email
↓
Success
```

### Retry Flow

```text
Send Email
↓
Failed
↓
Retry
↓
Success / Failed
```

---

## 12.2 Notification Triggers

| Event                        | Notification                     |
| ---------------------------- | -------------------------------- |
| User Registration            | OTP Email                        |
| Forgot Password              | Reset OTP Email                  |
| Booking Created              | Booking Confirmation             |
| Appointment Approved         | Approval Notification            |
| Appointment Rejected         | Rejection Notification           |
| Appointment Cancelled        | Cancellation Notification        |
| Appointment Reminder         | Reminder Email (24 Hours Before) |
| Doctor Verification Approved | Verification Result              |
| Doctor Verification Rejected | Verification Result              |
| Subscription Activated       | Subscription Notification        |
| Subscription Expiring Soon   | Expiration Reminder              |
| Package Assigned             | Package Proposal Notification    |

---

# 13. System Workflow Dependencies

| Workflow             | Depends On                               |
| -------------------- | ---------------------------------------- |
| Appointment Booking  | Doctor Verification, Schedule Management |
| Appointment Approval | Appointment Booking                      |
| Treatment Package    | Completed Consultations                  |
| Blog Moderation      | Doctor Verification                      |
| Subscription         | Payment Workflow                         |
| Notification         | All Business Events                      |

---
