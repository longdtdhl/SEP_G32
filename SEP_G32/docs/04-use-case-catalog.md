# 04-use-case-catalog.md

## Actors Section

### Internal Actors

| Actor                | Description                                                  |
| -------------------- | ------------------------------------------------------------ |
| Patient              | Registered user seeking psychological consultation services. |
| Doctor               | Verified psychologist providing consultation services.       |
| Customer Support     | Reviews doctor applications and blog submissions.            |
| Business Manager     | Manages business-related configurations and analytics.       |
| System Administrator | Manages users, permissions, settings, and audit information. |

### External Human Actors

| Actor | Description                                                                  |
| ----- | ---------------------------------------------------------------------------- |
| Guest | Unregistered visitor who can browse doctors, blogs, and create appointments. |

### External System Actors

| Actor                   | Description                                           |
| ----------------------- | ----------------------------------------------------- |
| Google OAuth Service    | OAuth authentication provider.                        |
| Brevo Email Service     | Sends OTP and notification emails.                    |
| VNPay Payment Gateway   | Processes online payments.                            |
| Cloudinary File Storage | Stores images, certificates, avatars, and blog media. |

---

# Authentication Module

Add:

| ID         | Use Case Name     | Primary Actor |
| ---------- | ----------------- | ------------- |
| UC-AUTH-08 | Login With Google | Guest         |

---

# Consultation Record Module

Replace the entire Patient Record Management Module.

## Consultation Record Module

| ID       | Use Case Name              | Primary Actor   |
| -------- | -------------------------- | --------------- |
| UC-CR-01 | View Consultation Records  | Doctor          |
| UC-CR-02 | Create Consultation Record | Doctor          |
| UC-CR-03 | Update Consultation Record | Doctor          |
| UC-CR-04 | View Consultation History  | Doctor, Patient |

---

# Treatment Package Module

Replace:

```text
UC-CP-04 Recommend Package To Patient
```

with:

```text
UC-CP-04 Assign Treatment Package
```

---

# Doctor Verification Module

Add:

| ID       | Use Case Name                 | Primary Actor |
| -------- | ----------------------------- | ------------- |
| UC-DV-08 | Resubmit Verification Request | Doctor        |

---

# Service Package Module

Rename:

```text
Subscription Module
```

to

```text
Service Package Module
```

Keep:

| ID        | Use Case Name               | Primary Actor |
| --------- | --------------------------- | ------------- |
| UC-SUB-01 | View Service Packages       | Doctor        |
| UC-SUB-02 | Purchase Service Package    | Doctor        |
| UC-SUB-03 | View Service Package Status | Doctor        |

---

# Business Manager Module

Replace:

```text
UC-BM-01 Manage Subscription Packages
```

with:

```text
UC-BM-01 Manage Service Packages
```

---

# Notification Module

Add a new module.

## Notification Module

| ID         | Use Case Name                  | Primary Actor |
| ---------- | ------------------------------ | ------------- |
| UC-NOTI-01 | Send OTP Email                 | System        |
| UC-NOTI-02 | Send Appointment Notification  | System        |
| UC-NOTI-03 | Send Appointment Reminder      | System        |
| UC-NOTI-04 | Send Verification Notification | System        |
| UC-NOTI-05 | Send Subscription Notification | System        |

---

# Updated Summary

| Item                   | Value |
| ---------------------- | ----- |
| Internal Actors        | 5     |
| External Human Actors  | 1     |
| External System Actors | 4     |
| Functional Modules     | 16    |
| Business Use Cases     | 80+   |

---

# Recommended File Name

Rename:

```text
04-use-cases.md
```

to:

```text
04-use-case-catalog.md
```

because this document is an index/catalog of all use cases.

Detailed specifications should be stored separately:

```text
05-use-case-specifications/
├── UC-AUTH-01.md
├── UC-AB-01.md
├── UC-DV-04.md
└── ...
```
