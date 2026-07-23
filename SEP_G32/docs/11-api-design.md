# 11-api-design.md

# 1. Purpose

This document defines all REST APIs exposed by the Online Psychological Counseling Booking System (OPCBS).

This document serves as the single source of truth for:

* Controllers
* DTOs
* Swagger Documentation
* Frontend Integration
* Authorization Rules
* API Testing

---

# 2. API Standards

## Base URL

```text
/api/v1
```

Example:

```text
/api/v1/auth/login
```

---

## Response Format

All APIs must return:

```json
{
  "success": true,
  "message": "Operation successful",
  "data": {}
}
```

---

## Error Response

```json
{
  "success": false,
  "message": "Validation failed",
  "errors": [
    "Email is required"
  ]
}
```

---

# 3. Authentication APIs

Base Route

```text
/api/v1/auth
```

| Method | Endpoint         | Description        |
| ------ | ---------------- | ------------------ |
| POST   | /register        | Register account   |
| POST   | /verify-otp      | Verify OTP         |
| POST   | /login           | Login              |
| POST   | /google-login    | Google OAuth Login |
| POST   | /forgot-password | Send reset OTP     |
| POST   | /reset-password  | Reset password     |
| POST   | /refresh-token   | Refresh JWT        |
| POST   | /logout          | Logout             |

---

# 4. User APIs

Base Route

```text
/api/v1/users
```

| Method | Endpoint         |
| ------ | ---------------- |
| GET    | /profile         |
| PUT    | /profile         |
| PUT    | /change-password |

---

# 5. Doctor Discovery APIs

Base Route

```text
/api/v1/doctors
```

| Method | Endpoint         |
| ------ | ---------------- |
| GET    | /                |
| GET    | /{id}            |
| GET    | /{id}/schedule   |
| GET    | /{id}/reviews    |
| GET    | /specializations |

Query Parameters

```text
keyword
specializationId
rating
page
pageSize
```

---

# 6. Doctor Profile APIs

Base Route

```text
/api/v1/doctor-profile
```

Role:

```text
Doctor
```

| Method | Endpoint      |
| ------ | ------------- |
| GET    | /             |
| PUT    | /             |
| POST   | /avatar       |
| POST   | /certificates |

---

# 7. Doctor Verification APIs

Base Route

```text
/api/v1/verifications
```

Doctor APIs

| Method | Endpoint |
| ------ | -------- |
| POST   | /submit  |
| GET    | /status  |

Customer Support APIs

| Method | Endpoint |
| ------ | -------- |
| GET    | /pending |
| GET    | /{id}    |
| PUT    | /approve |
| PUT    | /reject  |

---

# 8. Schedule APIs

Base Route

```text
/api/v1/schedules
```

Role

```text
Doctor
```

| Method | Endpoint          |
| ------ | ----------------- |
| GET    | /                 |
| POST   | /                 |
| PUT    | /                 |
| DELETE | /                 |
| POST   | /unavailable-date |

---

# 9. Appointment APIs

Base Route

```text
/api/v1/appointments
```

Patient / Guest

| Method | Endpoint             |
| ------ | -------------------- |
| POST   | /                    |
| GET    | /my-appointments     |
| GET    | /track/{bookingCode} |
| PUT    | /cancel/{id}         |
| PUT    | /reschedule/{id}     |

Doctor

| Method | Endpoint       |
| ------ | -------------- |
| GET    | /doctor        |
| PUT    | /approve/{id}  |
| PUT    | /reject/{id}   |
| PUT    | /complete/{id} |

---

# 10. Consultation Record APIs

Base Route

```text
/api/v1/consultation-records
```

Doctor

| Method | Endpoint                     |
| ------ | ---------------------------- |
| POST   | /                            |
| PUT    | /{id}                        |
| GET    | /patient/{patientId}         |
| GET    | /appointment/{appointmentId} |

Patient

| Method | Endpoint    |
| ------ | ----------- |
| GET    | /my-records |

---

# 11. Treatment Package APIs

Base Route

```text
/api/v1/treatment-packages
```

Doctor

| Method | Endpoint |
| ------ | -------- |
| POST   | /        |
| PUT    | /{id}    |
| DELETE | /{id}    |
| POST   | /assign  |

Patient

| Method | Endpoint     |
| ------ | ------------ |
| GET    | /my-packages |
| PUT    | /accept/{id} |
| PUT    | /reject/{id} |

Shared

| Method | Endpoint |
| ------ | -------- |
| GET    | /{id}    |

---

# 12. Blog APIs

Base Route

```text
/api/v1/blogs
```

Public

| Method | Endpoint |
| ------ | -------- |
| GET    | /        |
| GET    | /{id}    |

Doctor

| Method | Endpoint            |
| ------ | ------------------- |
| POST   | /                   |
| PUT    | /{id}               |
| DELETE | /{id}               |
| POST   | /submit-review/{id} |
| GET    | /my-blogs           |

Customer Support

| Method | Endpoint      |
| ------ | ------------- |
| GET    | /pending      |
| PUT    | /approve/{id} |
| PUT    | /reject/{id}  |

---

# 13. Blog Comment APIs

Base Route

```text
/api/v1/blog-comments
```

| Method | Endpoint |
| ------ | -------- |
| POST   | /        |
| PUT    | /{id}    |
| DELETE | /{id}    |

---

# 14. Review APIs

Base Route

```text
/api/v1/reviews
```

| Method | Endpoint           |
| ------ | ------------------ |
| POST   | /                  |
| GET    | /doctor/{doctorId} |

Constraint:

```text
One Appointment
=
One Review
```

---

# 15. Service Package APIs

Purpose

Doctor subscription packages provided by OPCBS.

Base Route

```text
/api/v1/service-packages
```

Public

| Method | Endpoint |
| ------ | -------- |
| GET    | /        |

Business Manager

| Method | Endpoint |
| ------ | -------- |
| POST   | /        |
| PUT    | /{id}    |
| DELETE | /{id}    |

---

# 16. Doctor Subscription APIs

Base Route

```text
/api/v1/subscriptions
```

Doctor

| Method | Endpoint         |
| ------ | ---------------- |
| GET    | /my-subscription |
| POST   | /purchase        |
| GET    | /history         |

---

# 17. Payment APIs

Base Route

```text
/api/v1/payments
```

| Method | Endpoint      |
| ------ | ------------- |
| POST   | /create-vnpay |
| GET    | /callback     |
| GET    | /history      |

---

# 18. Notification APIs

Base Route

```text
/api/v1/notifications
```

| Method | Endpoint        |
| ------ | --------------- |
| GET    | /               |
| PUT    | /mark-read/{id} |
| PUT    | /mark-read-all  |

---

# 19. Customer Support APIs

Base Route

```text
/api/v1/customer-support
```

| Method | Endpoint         |
| ------ | ---------------- |
| GET    | /dashboard       |
| GET    | /pending-doctors |
| GET    | /pending-blogs   |

---

# 20. Business Manager APIs

Base Route

```text
/api/v1/business-manager
```

| Method | Endpoint              |
| ------ | --------------------- |
| GET    | /dashboard            |
| GET    | /analytics            |
| GET    | /reports              |
| POST   | /specializations      |
| PUT    | /specializations/{id} |
| DELETE | /specializations/{id} |

---

# 21. Admin APIs

Base Route

```text
/api/v1/admin
```

| Method | Endpoint           |
| ------ | ------------------ |
| GET    | /dashboard         |
| GET    | /users             |
| PUT    | /users/{id}/lock   |
| PUT    | /users/{id}/unlock |
| GET    | /roles             |
| GET    | /permissions       |
| GET    | /audit-logs        |
| GET    | /reports           |

---

# 22. Pagination Standard

Request

```text
?page=1&pageSize=10
```

Response

```json
{
  "success": true,
  "data": [],
  "pagination": {
    "page": 1,
    "pageSize": 10,
    "totalItems": 100,
    "totalPages": 10
  }
}
```

---

# 23. Authorization Matrix

| Module               | Guest | Patient       | Doctor     | CS       | BM   | Admin |
| -------------------- | ----- | ------------- | ---------- | -------- | ---- | ----- |
| Doctors              | R     | R             | R          | R        | R    | R     |
| Appointments         | C     | CRUD Own      | Manage Own | R        | R    | R     |
| Consultation Records | -     | R Own         | CRUD       | -        | -    | R     |
| Treatment Packages   | -     | Accept/Reject | CRUD       | -        | -    | R     |
| Blogs                | R     | Comment       | CRUD       | Moderate | -    | R     |
| Verification         | -     | -             | Submit     | Review   | -    | R     |
| Service Packages     | R     | R             | Purchase   | -        | CRUD | R     |
| Users                | -     | Own           | Own        | -        | -    | CRUD  |

---

# 24. API Generation Rules

All generated APIs must:

* Use JWT Authentication
* Use FluentValidation
* Return DTOs only
* Use Repository Layer
* Use Service Layer
* Support Swagger
* Follow REST conventions
* Follow Database Design
* Follow Domain Model
* Follow System Workflows
* Follow Business Rules
