# 12-state-diagrams.md

# 1. Purpose

This document defines all business state transitions used in the Online Psychological Counseling Booking System (OPCBS).

The state diagrams serve as the source of truth for:

* Domain Status Enums
* Workflow Validation
* Service Layer Logic
* Business Rule Enforcement
* API Authorization Rules

---

# 2. Appointment State Diagram

## Purpose

Represents the lifecycle of an appointment booking.

---

## State Flow

```text
Pending
├── Approve → Approved
├── Reject → Rejected
└── Cancel → Cancelled

Approved
├── Complete → Completed
└── Cancel → Cancelled

Rejected
└── Terminal State

Completed
└── Terminal State

Cancelled
└── Terminal State
```

---

## State Definitions

| Status    | Description                    |
| --------- | ------------------------------ |
| Pending   | Waiting for doctor review      |
| Approved  | Approved by doctor             |
| Rejected  | Rejected by doctor             |
| Completed | Consultation completed         |
| Cancelled | Cancelled by patient or doctor |

---

## Allowed Transitions

| Current State | Action   | Next State |
| ------------- | -------- | ---------- |
| Pending       | Approve  | Approved   |
| Pending       | Reject   | Rejected   |
| Pending       | Cancel   | Cancelled  |
| Approved      | Complete | Completed  |
| Approved      | Cancel   | Cancelled  |

---

## Forbidden Transitions

```text
Completed → Any State
Rejected → Any State
Cancelled → Any State
```

---

# 3. Doctor Verification State Diagram

## Purpose

Represents doctor verification lifecycle.

---

## State Flow

```text
Draft
↓
Submitted
├── Approve → Approved
└── Reject → Rejected

Rejected
↓
Resubmit
↓
Submitted

Approved
└── Terminal State
```

---

## State Definitions

| Status    | Description                |
| --------- | -------------------------- |
| Draft     | Verification not submitted |
| Submitted | Waiting for review         |
| Approved  | Doctor verified            |
| Rejected  | Verification rejected      |

---

## Allowed Transitions

| Current State | Action   | Next State |
| ------------- | -------- | ---------- |
| Draft         | Submit   | Submitted  |
| Submitted     | Approve  | Approved   |
| Submitted     | Reject   | Rejected   |
| Rejected      | Resubmit | Submitted  |

---

## Business Rules

```text
Only Approved doctors can:

- Appear in doctor search results
- Receive appointment bookings
- Purchase service packages
- Publish blogs
```

---

# 4. Blog State Diagram

## Purpose

Represents blog moderation workflow.

---

## State Flow

```text
Draft
↓
Submit
↓
Pending

Pending
├── Approve → Published
└── Reject → Rejected

Rejected
↓
Resubmit
↓
Pending

Published
↓
Archive
↓
Archived
```

---

## State Definitions

| Status    | Description                  |
| --------- | ---------------------------- |
| Draft     | Blog being edited            |
| Pending   | Waiting for review           |
| Published | Publicly visible             |
| Rejected  | Rejected by Customer Support |
| Archived  | Hidden from public           |

---

## Allowed Transitions

| Current State | Action   | Next State |
| ------------- | -------- | ---------- |
| Draft         | Submit   | Pending    |
| Pending       | Approve  | Published  |
| Pending       | Reject   | Rejected   |
| Rejected      | Resubmit | Pending    |
| Published     | Archive  | Archived   |

---

## Business Rules

```text
Approve = Publish

No separate Published workflow required.
```

---

# 5. Treatment Package State Diagram

## Purpose

Represents treatment package lifecycle.

---

## State Flow

```text
Created
↓
Assigned

Assigned
├── Accept → Active
├── Reject → Rejected
└── Cancel → Cancelled

Active
├── Complete → Completed
├── Expire → Expired
└── Cancel → Cancelled

Completed
└── Terminal State

Expired
└── Terminal State

Rejected
└── Terminal State

Cancelled
└── Terminal State
```

---

## State Definitions

| Status    | Description               |
| --------- | ------------------------- |
| Created   | Package created by doctor |
| Assigned  | Proposed to patient       |
| Active    | Accepted and usable       |
| Completed | All sessions consumed     |
| Expired   | Expiration date reached   |
| Rejected  | Rejected by patient       |
| Cancelled | Cancelled by doctor       |
| Completed | Fully consumed            |

---

## Allowed Transitions

| Current State | Action   | Next State |
| ------------- | -------- | ---------- |
| Created       | Assign   | Assigned   |
| Assigned      | Accept   | Active     |
| Assigned      | Reject   | Rejected   |
| Assigned      | Cancel   | Cancelled  |
| Active        | Complete | Completed  |
| Active        | Expire   | Expired    |
| Active        | Cancel   | Cancelled  |

---

# 6. Doctor Subscription State Diagram

## Purpose

Represents OPCBS service package lifecycle.

---

## State Flow

```text
PendingPayment
├── PaymentSuccess → Active
└── PaymentFailed → Cancelled

Active
↓
Expire
↓
Expired

Cancelled
└── Terminal State

Expired
└── Terminal State
```

---

## State Definitions

| Status         | Description                 |
| -------------- | --------------------------- |
| PendingPayment | Waiting for VNPay payment   |
| Active         | Subscription active         |
| Expired        | Subscription expired        |
| Cancelled      | Payment failed or cancelled |

---

## Allowed Transitions

| Current State  | Action         | Next State |
| -------------- | -------------- | ---------- |
| PendingPayment | PaymentSuccess | Active     |
| PendingPayment | PaymentFailed  | Cancelled  |
| Active         | Expire         | Expired    |

---

## Business Rules

```text
Doctor can receive bookings only when:

VerificationStatus = Approved

AND

SubscriptionStatus = Active
```

---

# 7. Payment Transaction State Diagram

## Purpose

Represents VNPay payment lifecycle.

---

## State Flow

```text
Pending
├── Success → Success
└── Failed → Failed
```

---

## State Definitions

| Status  | Description                |
| ------- | -------------------------- |
| Pending | Waiting for VNPay callback |
| Success | Payment successful         |
| Failed  | Payment failed             |

---

## Business Rules

```text
Payment Success
→ Activate Subscription

Payment Failed
→ Keep Subscription Inactive
```

---

# 8. State Ownership Matrix

| State Machine     | Owner            |
| ----------------- | ---------------- |
| Appointment       | Doctor           |
| Verification      | Customer Support |
| Blog              | Customer Support |
| Treatment Package | Doctor + Patient |
| Subscription      | VNPay System     |
| Payment           | VNPay System     |

---

# 9. Enum Generation Rules

AI-generated code must create:

```text
AppointmentStatus

VerificationStatus

BlogStatus

TreatmentPackageStatus

SubscriptionStatus

PaymentStatus
```

Each enum must exactly match the states defined in this document.

---

# 10. Workflow Consistency Rules

All generated services must:

* Validate state transitions before update.
* Reject invalid transitions.
* Log state changes.
* Trigger notifications after successful transitions.
* Follow Business Rules and System Workflows documents.

This document overrides any conflicting workflow implementation.
