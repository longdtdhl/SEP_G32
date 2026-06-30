# 18-permission-matrix.md

# 1. Purpose

This document defines the Role-Based Access Control (RBAC) model of the Online Psychological Counseling Booking System (OPCBS).

The permission matrix serves as the primary reference for:

* Authorization Policies
* JWT Claims
* Controller Security
* Razor Page Authorization
* API Access Control
* Audit Logging
* AI Code Generation

---

# 2. Roles

| Role            | Description                |
| --------------- | -------------------------- |
| Guest           | Unauthenticated visitor    |
| Patient         | Registered patient         |
| Doctor          | Verified psychologist      |
| CustomerSupport | Internal support staff     |
| BusinessManager | Business operation manager |
| SystemAdmin     | Platform administrator     |

---

# 3. Permission Actions

| Action   | Description           |
| -------- | --------------------- |
| View     | Read data             |
| Create   | Create new data       |
| Update   | Modify data           |
| Archive  | Soft delete / archive |
| Approve  | Approve workflow      |
| Reject   | Reject workflow       |
| Assign   | Assign resource       |
| Purchase | Purchase service      |
| Manage   | Full management       |
| Export   | Export reports        |

---

# 4. Authentication & Profile Permissions

| Feature            | Guest | Patient | Doctor | CustomerSupport | BusinessManager | SystemAdmin |
| ------------------ | ----- | ------- | ------ | --------------- | --------------- | ----------- |
| Register           | ✔     | ❌       | ❌      | ❌               | ❌               | ❌           |
| Login              | ✔     | ✔       | ✔      | ✔               | ✔               | ✔           |
| Logout             | ❌     | ✔       | ✔      | ✔               | ✔               | ✔           |
| Forgot Password    | ✔     | ✔       | ✔      | ✔               | ✔               | ✔           |
| View Own Profile   | ❌     | ✔       | ✔      | ✔               | ✔               | ✔           |
| Update Own Profile | ❌     | ✔       | ✔      | ✔               | ✔               | ✔           |

---

# 5. Doctor Discovery Permissions

| Feature              | Guest | Patient | Doctor | CustomerSupport | BusinessManager | SystemAdmin |
| -------------------- | ----- | ------- | ------ | --------------- | --------------- | ----------- |
| View Doctor List     | ✔     | ✔       | ✔      | ✔               | ✔               | ✔           |
| Search Doctors       | ✔     | ✔       | ✔      | ✔               | ✔               | ✔           |
| View Doctor Profile  | ✔     | ✔       | ✔      | ✔               | ✔               | ✔           |
| View Doctor Schedule | ✔     | ✔       | ✔      | ✔               | ✔               | ✔           |
| View Doctor Ratings  | ✔     | ✔       | ✔      | ✔               | ✔               | ✔           |

---

# 6. Appointment Permissions

| Feature                    | Guest | Patient | Doctor | CustomerSupport | BusinessManager | SystemAdmin |
| -------------------------- | ----- | ------- | ------ | --------------- | --------------- | ----------- |
| Create Appointment         | ✔     | ✔       | ❌      | ❌               | ❌               | ❌           |
| Track Appointment          | ✔     | ✔       | ❌      | ✔               | ✔               | ✔           |
| View Own Appointments      | ❌     | ✔       | ❌      | ❌               | ❌               | ❌           |
| View Assigned Appointments | ❌     | ❌       | ✔      | ✔               | ❌               | ✔           |
| Approve Appointment        | ❌     | ❌       | ✔      | ❌               | ❌               | ❌           |
| Reject Appointment         | ❌     | ❌       | ✔      | ❌               | ❌               | ❌           |
| Cancel Appointment         | ❌     | ✔       | ❌      | ✔               | ❌               | ✔           |
| Reschedule Appointment     | ❌     | ✔       | ❌      | ✔               | ❌               | ✔           |
| Update Appointment Status  | ❌     | ❌       | ✔      | ❌               | ❌               | ❌           |

---

# 7. Consultation Record Permissions

## Rule

One completed appointment generates one consultation record.

## Ownership

Doctor can only access records created from appointments assigned to them.

Patient can only access their own records.

| Feature                    | Patient | Doctor |
| -------------------------- | ------- | ------ |
| View Consultation Records  | ✔       | ✔      |
| Create Consultation Record | ❌       | ✔      |
| Update Consultation Record | ❌       | ✔      |
| View Consultation History  | ✔       | ✔      |

---

# 8. Treatment Package Permissions

## Definition

Treatment Package = Doctor-created treatment program assigned to a patient.

| Feature         | Patient | Doctor |
| --------------- | ------- | ------ |
| View Package    | ✔       | ✔      |
| Create Package  | ❌       | ✔      |
| Update Package  | ❌       | ✔      |
| Assign Package  | ❌       | ✔      |
| Accept Package  | ✔       | ❌      |
| Reject Package  | ✔       | ❌      |
| Track Progress  | ✔       | ✔      |
| Archive Package | ❌       | ✔      |

---

# 9. Blog Permissions

| Feature            | Guest | Patient | Doctor | CustomerSupport |
| ------------------ | ----- | ------- | ------ | --------------- |
| View Blog          | ✔     | ✔       | ✔      | ✔               |
| View Blog Details  | ✔     | ✔       | ✔      | ✔               |
| Create Blog        | ❌     | ❌       | ✔      | ❌               |
| Update Blog        | ❌     | ❌       | ✔      | ❌               |
| Submit Blog Review | ❌     | ❌       | ✔      | ❌               |
| Comment Blog       | ❌     | ✔       | ❌      | ❌               |
| Approve Blog       | ❌     | ❌       | ❌      | ✔               |
| Reject Blog        | ❌     | ❌       | ❌      | ✔               |
| Archive Blog       | ❌     | ❌       | ✔      | ❌               |

---

# 10. Doctor Verification Permissions

| Feature                      | Doctor | CustomerSupport |
| ---------------------------- | ------ | --------------- |
| Complete Doctor Profile      | ✔      | ❌               |
| Upload Certificates          | ✔      | ❌               |
| Submit Verification Request  | ✔      | ❌               |
| View Verification Request    | ✔      | ✔               |
| Approve Verification Request | ❌      | ✔               |
| Reject Verification Request  | ❌      | ✔               |

---

# 11. Service Package Permissions

## Definition

Service Package = Subscription package purchased by Doctors to use OPCBS services.

| Feature                  | Doctor | BusinessManager |
| ------------------------ | ------ | --------------- |
| View Service Packages    | ✔      | ✔               |
| Purchase Service Package | ✔      | ❌               |
| View Subscription Status | ✔      | ✔               |
| Create Service Package   | ❌      | ✔               |
| Update Service Package   | ❌      | ✔               |
| Archive Service Package  | ❌      | ✔               |

---

# 12. Payment Permissions

## VNPay

Only used for Service Package purchases.

| Feature                | Doctor |
| ---------------------- | ------ |
| Create Payment Request | ✔      |
| View Payment Status    | ✔      |
| View Payment History   | ✔      |

---

# 13. Customer Support Permissions

| Feature                          | CustomerSupport |
| -------------------------------- | --------------- |
| View Staff Dashboard             | ✔               |
| View Pending Doctor Applications | ✔               |
| Review Doctor Applications       | ✔               |
| View Pending Blogs               | ✔               |
| Review Blog Content              | ✔               |
| View Appointment Details         | ✔               |
| Cancel Appointment               | ✔               |
| Reschedule Appointment           | ✔               |

---

# 14. Business Manager Permissions

| Feature                     | BusinessManager |
| --------------------------- | --------------- |
| Manage Service Packages     | ✔               |
| Manage Specializations      | ✔               |
| Manage Treatment Categories | ✔               |
| View Revenue Reports        | ✔               |
| View Business Dashboard     | ✔               |
| View Operational Reports    | ✔               |

## Restrictions

Business Manager cannot:

* Review Doctor Applications
* Review Blogs
* Manage Users
* Manage Roles
* Manage Permissions
* Access Audit Logs

---

# 15. System Administrator Permissions

| Feature                      | SystemAdmin |
| ---------------------------- | ----------- |
| Manage Users                 | ✔           |
| Manage Roles                 | ✔           |
| Manage Permissions           | ✔           |
| Manage System Configurations | ✔           |
| View Audit Logs              | ✔           |
| View System Reports          | ✔           |
| View System Dashboard        | ✔           |

## Restrictions

System Admin does not participate in:

* Doctor Verification Approval
* Blog Moderation
* Service Package Management

---

# 16. System Configuration Permissions

| Feature                  | SystemAdmin |
| ------------------------ | ----------- |
| Manage Roles             | ✔           |
| Manage Permissions       | ✔           |
| Manage Settings          | ✔           |
| Manage Security Policies | ✔           |

---

# 17. Audit Log Permissions

Audit logs must be generated for:

* Login
* Logout
* Password Reset
* Appointment Approval
* Appointment Rejection
* Consultation Record Creation
* Doctor Verification Approval
* Doctor Verification Rejection
* Blog Approval
* Blog Rejection
* Service Package Purchase
* VNPay Payment Result
* Role Changes
* Permission Changes

Access:

| Role        | Access    |
| ----------- | --------- |
| SystemAdmin | Full      |
| Others      | No Access |

---

# 18. Authorization Rules

## Patient

Can access only:

* Own profile
* Own appointments
* Own consultation records
* Own treatment packages

---

## Doctor

Can access only:

* Own profile
* Own schedule
* Own appointments
* Own consultation records
* Own blogs
* Own treatment packages

---

## Customer Support

Can access:

* Doctor verification workflow
* Blog moderation workflow
* Appointment support workflow

Cannot access business configuration.

---

## Business Manager

Can access:

* Service packages
* Reports
* Analytics
* Specializations

Cannot access security configuration.

---

## System Administrator

Has full system administration rights except business approval workflows.

---

# 19. AI Generation Rules

When generating code:

1. Always use Role-Based Authorization.
2. Never hardcode role strings.
3. Use Permission Constants.
4. Protect all management endpoints.
5. Validate resource ownership.
6. Log every sensitive action.
7. Deny access by default.
8. Use JWT Claims for authorization.
9. Follow least privilege principle.
10. Every controller endpoint must declare authorization requirements.

```
```
