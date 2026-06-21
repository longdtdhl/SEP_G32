# 01-scope.md

# 1. Project Scope

## Project Name

Online Psychological Counseling Booking System (OPCBS)

---

# 2. Project Objective

The Online Psychological Counseling Booking System (OPCBS) is a web-based platform that connects Patients with verified Doctors specializing in psychological counseling and mental health services.

The system allows Patients to discover Doctors, book appointments, manage consultation records, receive treatment package recommendations, and provide feedback after consultations.

Doctors can manage schedules, appointments, patient records, treatment packages, blogs, and subscriptions.

Administrative users manage platform operations, content moderation, business configurations, and system security.

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
* Filter Doctors
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

## Patient Record Management

* View Patient Records
* Manage Patient Records
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

## Subscription Management

* View Service Packages
* Purchase Subscription Package
* View Subscription Status

---

## Payment Integration

* VNPay Payment Processing
* Payment Confirmation
* Subscription Activation

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
* Review Doctor Applications
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
* Email Service
* Payment Gateway
* Google Authentication Service

---

# 6. System Boundaries

OPCBS is responsible for:

* User authentication
* Doctor discovery
* Appointment management
* Consultation tracking
* Package management
* Blog management
* Doctor verification
* Subscription management

External systems are responsible for:

* Email delivery
* Payment processing
* Google authentication

---

# 7. Success Criteria

The system shall:

* Allow Patients to successfully book appointments.
* Allow Doctors to manage consultations and schedules.
* Allow Customer Support to verify Doctors and moderate Blogs.
* Allow Business Managers to manage business configurations.
* Allow System Administrators to manage platform operations.
* Provide secure and scalable access control.
* Support subscription-based Doctor services.
