# 01-scope.md

# 1. Project Scope

## Project Name

Online Psychological Counseling Booking System (OPCBS)

---

# 2. Project Objective

The Online Psychological Counseling Booking System (OPCBS) is a web-based platform that connects Patients with verified Doctors specializing in psychological counseling and mental health services.

The system enables Patients to discover Doctors, book appointments, manage consultation records, receive treatment package recommendations, and provide feedback after consultations.

Doctors can manage schedules, appointments, consultation records, treatment packages, blogs, and service package subscriptions.

Administrative users manage platform operations, content moderation, business configurations, and system security.

---

## 2.1 Package Types

OPCBS supports two distinct package types:

### Service Package

A subscription plan purchased by Doctors to access OPCBS platform features and receive appointment bookings.

### Treatment Package

A counseling package created by Doctors and assigned to Patients as part of a treatment plan.

---

# 3. In-Scope Features

## Authentication & Authorization

* Register Account
* Verify OTP
* Login
* Logout
* Forgot Password
* Manage Profile
* Change Password
* JWT Authentication
* Role-Based Access Control (RBAC)

---

## Doctor Discovery

* View Home Page
* Search Doctors
* View Doctor Profile
* View Doctor Schedule
* View Doctor Ratings

---

## Appointment Booking

* Book Appointment
* Track Appointment (Guest)
* View Appointment Status
* Cancel Appointment
* Reschedule Appointment

---

## Schedule Management

* Configure Working Schedule
* Manage Unavailable Dates
* View Schedule Calendar

---

## Appointment Management

* View Appointment Requests
* Approve Appointment
* Reject Appointment
* View Appointment History
* Update Consultation Status

---

## Consultation Record Management

* View Consultation Records
* Create Consultation Records
* Update Consultation Records
* View Consultation History

---

## Treatment Package Management

* Create Treatment Package
* Update Treatment Package
* Delete Treatment Package
* Recommend Package To Patient
* Accept Package
* Reject Package
* Track Package Progress

---

## Blog Management

* View Blog Articles
* View Blog Details
* Create Blog
* Update Blog
* Delete Blog
* Submit Blog For Review
* Comment Blog
* Review Blog
* Approve Blog
* Reject Blog

---

## Feedback & Rating

* Submit Feedback & Rating
* View Doctor Ratings

---

## Doctor Verification

* Complete Doctor Profile
* Upload Certificates
* Submit Verification Request
* Review Verification Request
* Approve Verification Request
* Reject Verification Request
* View Verification Status

---

## Service Package Management

* View Service Packages
* Purchase Service Package
* View Service Package Status

---

## Payment Integration

* Process Subscription Payment
* Activate Subscription After Successful Payment

---

## Notification System

* OTP Emails
* Appointment Notifications
* Verification Notifications
* Subscription Notifications
* Reminder Emails

---

## Customer Support Operations

* View Staff Dashboard
* View Pending Doctor Applications
* Review Doctor Applications
* View Pending Blogs
* Review Blog Content

---

## Business Management

* Manage Subscription Packages
* Manage Specializations
* View Analytics
* View Operational Reports

---

## System Administration

* Manage Users
* Manage Roles
* Manage Permissions
* Manage System Settings
* View Audit Logs
* Generate Reports

---

# 4. Out-of-Scope Features

The following features are not included in OPCBS Phase 1.

## Real-Time Video Consultation

The system does not provide built-in video conferencing.

External platforms such as Zoom, Google Meet, or Microsoft Teams may be used.

---

## Online Chat Between Patient and Doctor

Direct messaging functionality is not supported.

---

## Mobile Applications

Native Android and iOS applications are not included.

The system is delivered as a responsive web application.

---

## Online Prescription Management

Prescription generation and pharmacy integration are not supported.

---

## Insurance Claim Processing

Health insurance integration is not supported.

---

## Multi-Language Support

The initial version supports English only.

---

## Multi-Tenant Architecture

The system supports only a single organization.

---

# 5. Supported Actors

## Internal Actors

* Patient
* Doctor
* Customer Support
* Business Manager
* System Administrator

---

## External Actors

* Guest
* Brevo Email Service
* VNPay Payment Gateway
* Google OAuth Service
* Cloudinary File Storage

---

# 6. System Boundaries

OPCBS is responsible for:

* User Authentication & Authorization
* Doctor Discovery
* Appointment Management
* Consultation Record Management
* Treatment Package Management
* Blog Management
* Doctor Verification
* Service Package Management
* Notification Management

External systems are responsible for:

* Email Delivery
* Payment Processing
* Google Authentication
* File and Image Storage

---

# 7. Success Criteria

The system shall:

* Allow Guests to book and track appointments.
* Allow Patients to successfully book and manage appointments.
* Allow Doctors to manage schedules, consultations, and treatment packages.
* Allow Doctors to receive appointments only when verified and subscribed.
* Allow Customer Support to verify Doctors and moderate Blogs.
* Allow Business Managers to manage business configurations and subscription plans.
* Allow System Administrators to manage platform operations.
* Prevent double-booking and overlapping schedules.
* Maintain secure Role-Based Access Control (RBAC).
* Support audit logging for administrative activities.
* Provide secure and scalable platform access.
* Support subscription-based Doctor services.
