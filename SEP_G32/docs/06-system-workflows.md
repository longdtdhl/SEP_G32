# 04-system-workflows.md

# 1. Purpose

This document defines the major business workflows and state transitions within the Online Psychological Counseling Booking System (OPCBS).

The workflows described in this document serve as the primary reference for:

* Business Logic Implementation
* Status Management
* Notification Triggers
* API Design
* Validation Rules
* Backend Workflow Processing

---

# 2. Authentication Workflow

## 2.1 Registration Workflow

```text
Guest
↓
Register Account
↓
System Generates OTP
↓
Brevo Sends OTP Email
↓
Verify OTP
↓
Account Activated
```

### Failure Flow

```text
Guest
↓
Register Account
↓
Invalid OTP
↓
Account Remains Inactive
```

---

## 2.2 Google Login Workflow

```text
Guest
↓
Login With Google
↓
Google OAuth Authentication
↓
User Information Retrieved
↓
Account Created / Logged In
```

---

## 2.3 Forgot Password Workflow

```text
User
↓
Request Password Reset
↓
Generate OTP
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

## 3.1 Verification Submission

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

## 3.2 Verification Review

```text
Submitted
↓
Customer Support Review
↓
Approved
↓
Doctor Verified
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
Submit Verification Request
↓
Submitted
```

---

## 3.3 Verification Status

| Status    |
| --------- |
| Draft     |
| Submitted |
| Approved  |
| Rejected  |

---

# 4. Doctor Service Package Workflow

## 4.1 Purchase Service Package

```text
Doctor
↓
Select Service Package
↓
Create Payment Request
↓
PendingPayment
```

---

## 4.2 Successful Payment

```text
PendingPayment
↓
VNPay Success
↓
Active
↓
Doctor Can Receive Bookings
↓
Doctor Visible In Search Results
```

---

## 4.3 Failed Payment

```text
PendingPayment
↓
VNPay Failed
↓
Cancelled
```

---

## 4.4 Expiration Flow

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

## 4.5 Service Package Status

| Status         |
| -------------- |
| PendingPayment |
| Active         |
| Expired        |
| Cancelled      |

---

# 5. Appointment Booking Workflow

## Business Rule

Before booking is allowed:

```text
Doctor Verification Status = Approved
AND
Doctor Service Package Status = Active
```

---

## 5.1 Guest Booking

```text
Guest
↓
View Doctor Profile
↓
View Schedule
↓
Select Time Slot
↓
Enter Booking Information
↓
Submit Booking
↓
Generate Booking Code
↓
Appointment Status = Pending
↓
Confirmation Email Sent
```

---

## 5.2 Patient Booking

```text
Patient
↓
Select Doctor
↓
Select Time Slot
↓
Submit Booking
↓
Appointment Status = Pending
↓
Confirmation Email Sent
```

---

## 5.3 Doctor Review

```text
Pending
↓
Doctor Reviews Request
↓
Approved
```

### Rejection Flow

```text
Pending
↓
Doctor Reviews Request
↓
Rejected
```

---

## 5.4 Cancellation Flow

```text
Pending / Approved
↓
Patient Cancels
↓
Cancelled
```

---

# 6. Appointment Lifecycle Workflow

## Appointment Status Flow

```text
Pending
↓
Approved
↓
InProgress
↓
Completed
```

### Alternative Flows

```text
Pending
↓
Rejected
```

```text
Pending / Approved
↓
Cancelled
```

---

## Appointment Status Definitions

| Status     |
| ---------- |
| Pending    |
| Approved   |
| Rejected   |
| InProgress |
| Completed  |
| Cancelled  |

---

# 7. Consultation Workflow

## Consultation Process

```text
Approved Appointment
↓
Consultation Starts
↓
InProgress
↓
Doctor Creates Consultation Record
↓
Consultation Completed
↓
Completed
```

---

## Consultation Record Lifecycle

```text
Create Consultation Record
↓
Save Notes
↓
Save Recommendations
↓
Update Follow-Up Information
```

---

# 8. Treatment Package Workflow

## 8.1 Package Assignment

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

## 8.2 Patient Decision

```text
Assigned
↓
Accept
↓
Accepted
↓
Active
```

### Rejection Flow

```text
Assigned
↓
Reject
↓
Rejected
```

---

## 8.3 Completion Flow

```text
Active
↓
All Sessions Used
↓
Completed
```

---

## 8.4 Expiration Flow

```text
Active
↓
Expiration Date Reached
↓
Expired
```

---

## 8.5 Cancellation Flow

```text
Assigned / Active
↓
Cancelled
```

---

## Treatment Package Status

| Status    |
| --------- |
| Created   |
| Assigned  |
| Accepted  |
| Active    |
| Completed |
| Expired   |
| Rejected  |
| Cancelled |

---

# 9. Blog Workflow

## 9.1 Blog Submission

```text
Doctor
↓
Create Blog
↓
Draft
↓
Submit Blog
↓
Submitted
```

---

## 9.2 Blog Approval

```text
Submitted
↓
Customer Support Review
↓
Approved
↓
Published Automatically
```

---

## 9.3 Blog Rejection

```text
Submitted
↓
Rejected
↓
Doctor Updates Content
↓
Draft
```

---

## Blog Status

| Status    |
| --------- |
| Draft     |
| Submitted |
| Approved  |
| Rejected  |

---

# 10. Payment Workflow

## Service Package Payment

```text
Doctor
↓
Select Service Package
↓
Create Payment Request
↓
Redirect To VNPay
↓
Payment Processing
↓
Return Payment Result
```

### Success

```text
Payment Success
↓
Store Transaction
↓
Activate Service Package
```

### Failure

```text
Payment Failed
↓
Store Transaction
↓
Service Package Remains Inactive
```

---

# 11. Notification Workflow

## Notification Process

```text
Business Event Triggered
↓
Create Notification
↓
Send Email
↓
Brevo Processing
↓
Delivered
```

---

## Notification Triggers

| Event                      | Notification                 |
| -------------------------- | ---------------------------- |
| Register Account           | OTP Email                    |
| Forgot Password            | Reset OTP                    |
| Booking Created            | Booking Confirmation         |
| Appointment Approved       | Appointment Approved         |
| Appointment Rejected       | Appointment Rejected         |
| Appointment Cancelled      | Appointment Cancelled        |
| Appointment Reminder       | Reminder Email               |
| Verification Approved      | Verification Result          |
| Verification Rejected      | Verification Result          |
| Service Package Activated  | Service Package Notification |
| Service Package Expiring   | Expiration Reminder          |
| Treatment Package Assigned | Package Proposal             |
| Treatment Package Accepted | Package Accepted             |
| Treatment Package Rejected | Package Rejected             |

---

# 12. Workflow Dependencies

| Workflow            | Depends On                                  |
| ------------------- | ------------------------------------------- |
| Appointment Booking | Doctor Verification, Active Service Package |
| Consultation        | Approved Appointment                        |
| Treatment Package   | Completed Consultation                      |
| Blog Submission     | Verified Doctor                             |
| Service Package     | Payment Workflow                            |
| Notification        | Business Events                             |

---

# 13. Key Business Rules

BR-01
Only verified doctors can appear in doctor search results.

BR-02
Only doctors with an active service package can receive bookings.

BR-03
A doctor cannot have overlapping appointments.

BR-04
A patient can only review completed appointments.

BR-05
Treatment packages can only be assigned by doctors.

BR-06
Blogs must be approved before publication.

BR-07
All critical actions must generate audit logs.
