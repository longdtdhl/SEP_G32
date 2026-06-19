# Actors

## Purpose

This document defines all actors interacting with the Online Psychological Counseling Booking System (OPCBS), including internal users, external users, and third-party systems.

The actor definitions serve as the foundation for use cases, business rules, authorization rules, and system workflows.

---

# Actor Overview

| ID     | Actor                         | Type            |
| ------ | ----------------------------- | --------------- |
| ACT-01 | Guest                         | Human Actor     |
| ACT-02 | Patient                       | Human Actor     |
| ACT-03 | Doctor                        | Human Actor     |
| ACT-04 | Customer Support              | Internal Staff  |
| ACT-05 | Business Manager              | Internal Staff  |
| ACT-06 | System Admin                  | Internal Staff  |
| ACT-07 | Email Service                 | External System |
| ACT-08 | Payment Gateway               | External System |
| ACT-09 | Google Authentication Service | External System |

---

# ACT-01 Guest

## Description

A Guest is an unregistered visitor who accesses the platform without creating an account.

Guests can explore public information and create appointments without registration.

## Responsibilities

* Browse homepage
* Search doctors
* Filter doctors
* View doctor profiles
* View doctor schedules
* Read blog articles
* Book appointments
* Track bookings using booking code

## Restrictions

Guests cannot:

* View patient records
* Submit ratings
* Submit feedback
* Access dashboards
* Manage appointments after authentication-required actions

---

# ACT-02 Patient

## Description

A Patient is a registered user seeking psychological counseling services.

Patients can manage appointments, profiles, consultation history, and package subscriptions.

## Responsibilities

* Manage profile information
* Book appointments
* Cancel appointments
* Reschedule appointments
* View appointment history
* View consultation history
* Purchase consultation packages
* Accept package proposals
* Reject package proposals
* Submit ratings and reviews
* Submit blog comments

## Restrictions

Patients can only access their own information and records.

---

# ACT-03 Doctor

## Description

A Doctor is a certified psychologist providing professional counseling services through the platform.

Doctors manage schedules, consultations, patient records, blogs, and service packages.

## Responsibilities

### Appointment Management

* Review booking requests
* Approve appointments
* Reject appointments
* Update appointment status

### Schedule Management

* Configure schedules
* Configure working days
* Configure consultation duration
* Configure unavailable dates

### Consultation Management

* Access assigned patient records
* Add consultation notes
* Add treatment recommendations
* Add follow-up notes

### Blog Management

* Create blogs
* Edit blogs
* Manage blogs

### Package Management

* Create consultation packages
* Update consultation packages
* Assign packages to patients

### Subscription Management

* Purchase subscriptions
* View subscription status

## Restrictions

Doctors cannot:

* Access administrative functions
* Access other doctors' patients
* Manage platform users
* Manage system configurations

---

# ACT-04 Customer Support

## Description

Customer Support is an internal operational staff role responsible for content moderation and doctor verification processes.

## Responsibilities

### Doctor Verification

* View doctor applications
* Review certificates
* Approve doctor applications
* Reject doctor applications

### Blog Moderation

* Review blog submissions
* Approve blog content
* Reject blog content

### Customer Assistance

* Review booking requests
* Support appointment issues
* Assist customers with booking inquiries

## Restrictions

Customer Support cannot:

* Manage system configurations
* Manage subscriptions
* Manage permissions
* Access audit logs

---

# ACT-05 Business Manager

## Description

Business Managers are responsible for platform business operations, package management, and performance monitoring.

## Responsibilities

### Subscription Management

* Create subscription packages
* Update subscription packages
* Activate subscriptions
* Deactivate subscriptions

### Business Management

* Manage specializations
* Manage treatment categories
* Monitor doctor subscriptions

### Analytics

* View business reports
* View revenue reports
* View operational analytics

## Restrictions

Business Managers cannot:

* Manage user permissions
* Configure authentication settings
* Access infrastructure settings

---

# ACT-06 System Admin

## Description

System Administrators are responsible for platform governance, security, and system configuration.

## Responsibilities

### User Management

* Create accounts
* Lock accounts
* Unlock accounts
* Disable accounts

### Role Management

* Manage roles
* Manage permissions

### System Configuration

* Configure platform settings
* Configure operational settings
* Manage security policies

### Monitoring

* View audit logs
* View system reports
* Monitor system activities

## Restrictions

System Administrators do not participate in doctor verification or blog moderation workflows.

---

# ACT-07 Email Service

## Type

External System

## Description

An external email delivery service integrated with OPCBS.

## Responsibilities

* Send OTP emails
* Send appointment notifications
* Send appointment reminders
* Send package notifications
* Send subscription notifications

## Inputs

* Email content
* Recipient information
* Notification requests

## Outputs

* Delivery status
* Delivery response

---

# ACT-08 Payment Gateway

## Type

External System

## Description

A third-party payment processor responsible for handling subscription transactions.

## Responsibilities

* Process payments
* Generate payment QR codes
* Verify payment status
* Return payment results

## Inputs

* Payment requests
* Transaction information

## Outputs

* Payment status
* Transaction result

---

# ACT-09 Google Authentication Service

## Type

External System

## Description

A third-party identity provider integrated through OAuth 2.0 authentication.

## Responsibilities

* Validate identity tokens
* Authenticate users
* Provide user profile information

## Inputs

* Authentication requests
* Identity tokens

## Outputs

* Authentication result
* User profile information

---

# Actor Relationship Summary

| Actor                         | Authentication Required |
| ----------------------------- | ----------------------- |
| Guest                         | No                      |
| Patient                       | Yes                     |
| Doctor                        | Yes                     |
| Customer Support              | Yes                     |
| Business Manager              | Yes                     |
| System Admin                  | Yes                     |
| Email Service                 | System Integration      |
| Payment Gateway               | System Integration      |
| Google Authentication Service | System Integration      |

---

# Role Hierarchy

Guest

↓

Patient

Doctor

↓

Customer Support

↓

Business Manager

↓

System Admin

The hierarchy represents increasing levels of system authority and access permissions.
