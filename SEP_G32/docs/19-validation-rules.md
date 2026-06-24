# 15-validation-rules.md

# 1. Purpose

This document defines all validation rules used by OPCBS.

Validation rules must be implemented using:

* FluentValidation (Backend)
* HTML Validation (Frontend)
* Business Rule Validation (Service Layer)

---

# 2. User Validation

## Register Account

### Email

Required

Maximum Length

```text
255
```

Must be valid email format.

Must be unique.

### Password

Required

Minimum Length

```text
8
```

Maximum Length

```text
50
```

Must contain:

```text
Uppercase
Lowercase
Number
```

### FullName

Required

Maximum Length

```text
255
```

### PhoneNumber

Required

Unique

Vietnamese phone format.

---

# 3. Doctor Profile Validation

### Professional Title

Required

Maximum Length

```text
255
```

### Biography

Required

Maximum Length

```text
5000
```

### Experience Years

```text
0 - 60
```

### Specialization

At least one specialization required.

---

# 4. Appointment Validation

## Create Appointment

### Doctor

Must exist.

Must be verified.

Must have active subscription.

### Slot

Must exist.

Must be Available.

Must not be expired.

### Patient

Must exist.

### Guest Booking

Guest Name required.

Guest Email required.

Guest Phone required.

### Notes

Maximum Length

```text
2000
```

---

# 5. Schedule Validation

### StartTime

Must be earlier than EndTime.

### SlotDuration

Allowed Values

```text
30
60
90
120
```

### Working Days

At least one day required.

---

# 6. Consultation Record Validation

### Consultation Summary

Required.

Maximum Length

```text
5000
```

### Recommendation

Maximum Length

```text
5000
```

### FollowUp Notes

Maximum Length

```text
5000
```

Appointment must be Approved before record creation.

---

# 7. Treatment Package Validation

### Name

Required

Maximum Length

```text
255
```

### Session Quantity

Minimum

```text
1
```

Maximum

```text
100
```

### Price

Must be greater than zero.

### Validity Days

Minimum

```text
1
```

Maximum

```text
365
```

---

# 8. Blog Validation

### Title

Required

Maximum Length

```text
500
```

### Content

Required

Minimum Length

```text
100
```

### Thumbnail

Required

Cloudinary URL only.

---

# 9. Review Validation

### Rating

Allowed

```text
1
2
3
4
5
```

Only one review per appointment.

Only completed appointment can be reviewed.

---

# 10. Service Package Validation

### Name

Required

Unique.

### Duration Days

Minimum

```text
30
```

Maximum

```text
365
```

### Price

Must be positive.

---

# 11. Verification Validation

At least one certificate required.

Certificate file required.

Only PDF, JPG, PNG allowed.

Maximum File Size

```text
10 MB
```

---

# 12. Business Rule Validation

Doctor must satisfy:

```text
VerificationStatus = Approved

AND

SubscriptionStatus = Active
```

before receiving appointments.

No overlapping appointment slots.

No double booking.

One Appointment = One ConsultationRecord.

One Appointment = One Review.
