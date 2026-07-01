# 02-actors.md

# Actors

## Purpose

This document defines all actors interacting with the Online Psychological Counseling Booking System (OPCBS), including human users, internal staff, and external integrated systems.

Actor definitions provide the foundation for use cases, workflows, authorization rules, and system interactions.

---

# Actor Overview

| ID     | Actor                   | Type            |
| ------ | ----------------------- | --------------- |
| ACT-01 | Guest                   | Human Actor     |
| ACT-02 | Patient                 | Human Actor     |
| ACT-03 | Doctor                  | Human Actor     |
| ACT-04 | Customer Support        | Internal Staff  |
| ACT-05 | Business Manager        | Internal Staff  |
| ACT-06 | System Administrator    | Internal Staff  |
| ACT-07 | Brevo Email Service     | External System |
| ACT-08 | VNPay Payment Gateway   | External System |
| ACT-09 | Google OAuth Service    | External System |
| ACT-10 | Cloudinary File Storage | External System |

---

# ACT-01 Guest

## Description

A Guest is an unauthenticated visitor who accesses public information on the platform without registering an account.

Guests may search for doctors, read blog content, create appointment bookings, and track appointments using booking codes.

## Responsibilities

* View homepage
* Search doctors
* View doctor profiles
* View doctor schedules
* View doctor ratings
* Read blog articles
* Book appointments
* Track appointments using booking code and email

## Restrictions

Guests cannot:

* Access patient information
* Submit ratings or reviews
* Submit blog comments
* Access dashboards
* Manage appointments after authentication-required actions

---

# ACT-02 Patient

## Description

A Patient is a registered user seeking psychological counseling services.

Patients can manage appointments, consultation history, treatment packages, ratings, and personal profiles.

## Responsibilities

### Profile Management

* Update profile information
* Change password

### Appointment Management

* Book appointments
* Cancel appointments
* Reschedule appointments
* View appointment history

### Consultation Management

* View consultation records
* View consultation history

### Treatment Package Management

* View package proposals
* Accept treatment packages
* Reject treatment packages
* View active packages

### Feedback

* Submit ratings
* Submit reviews
* Submit blog comments

## Restrictions

Patients may only access their own information and consultation records.

---

# ACT-03 Doctor

## Description

A Doctor is a verified mental health professional providing counseling services through the platform.

Doctors manage appointments, schedules, consultation records, treatment packages, blogs, and service package subscriptions.

## Responsibilities

### Appointment Management

* View appointment requests
* Approve appointments
* Reject appointments
* Update appointment status

### Schedule Management

* Configure schedules
* Configure working days
* Configure consultation duration
* Configure unavailable dates

### Consultation Management

* View assigned consultation records
* Create consultation records
* Update consultation records
* Add treatment recommendations
* Add follow-up notes

### Treatment Package Management

* Create treatment packages
* Update treatment packages
* Delete treatment packages
* Assign treatment packages to patients

### Blog Management

* Create blogs
* Update blogs
* Submit blogs for review

### Service Package Management

* Purchase service packages
* View subscription status

## Restrictions

Doctors cannot:

* Access administrative functions
* Access other doctors' consultation records
* Manage users
* Manage permissions
* Manage system settings

---

# ACT-04 Customer Support

## Description

Customer Support is responsible for operational moderation and doctor verification activities.

## Responsibilities

### Doctor Verification

* View pending doctor applications
* Review certificates
* Approve doctor applications
* Reject doctor applications

### Blog Moderation

* View pending blogs
* Review blog submissions
* Approve blogs
* Reject blogs

## Restrictions

Customer Support cannot:

* Manage service packages
* Manage permissions
* Manage users
* Access audit logs
* Configure system settings

---

# ACT-05 Business Manager

## Description

Business Managers are responsible for business operations and service package management.

## Responsibilities

### Service Package Management

* Create service packages
* Update service packages
* Activate service packages
* Deactivate service packages

### Business Configuration

* Manage specializations
* Manage treatment categories

### Analytics

* View operational reports
* View revenue reports
* View business analytics

## Restrictions

Business Managers cannot:

* Manage permissions
* Configure authentication settings
* Access infrastructure settings

---

# ACT-06 System Administrator

## Description

System Administrators are responsible for system governance, security, and platform administration.

## Responsibilities

### User Management

* Manage users
* Lock accounts
* Unlock accounts
* Disable accounts

### Authorization Management

* Manage roles
* Manage permissions

### System Management

* Configure system settings
* Configure security policies

### Monitoring

* View audit logs
* View system reports
* Monitor platform activities

## Restrictions

System Administrators do not participate in doctor verification or blog moderation workflows.

---

# ACT-07 Brevo Email Service

## Type

External System

## Responsibilities

* Send OTP emails
* Send appointment notifications
* Send appointment reminders
* Send verification notifications
* Send subscription notifications

---

# ACT-08 VNPay Payment Gateway

## Type

External System

## Responsibilities

* Process subscription payments
* Generate payment transactions
* Verify payment status
* Return payment results

---

# ACT-09 Google OAuth Service

## Type

External System

## Responsibilities

* Authenticate users
* Validate identity tokens
* Return basic user profile information

---

# ACT-10 Cloudinary File Storage

## Type

External System

## Responsibilities

* Store uploaded images
* Store certificates
* Store profile avatars
* Store blog images
* Return file URLs

---

# Authentication Summary

| Actor                   | Authentication Required |
| ----------------------- | ----------------------- |
| Guest                   | No                      |
| Patient                 | Yes                     |
| Doctor                  | Yes                     |
| Customer Support        | Yes                     |
| Business Manager        | Yes                     |
| System Administrator    | Yes                     |
| Brevo Email Service     | System Integration      |
| VNPay Payment Gateway   | System Integration      |
| Google OAuth Service    | System Integration      |
| Cloudinary File Storage | System Integration      |

---

# Role Model

The OPCBS platform uses Role-Based Access Control (RBAC).

Human roles are independent and assigned directly to users.

```text
Guest

Patient

Doctor

Customer Support

Business Manager

System Administrator
```

No role automatically inherits permissions from another role.
