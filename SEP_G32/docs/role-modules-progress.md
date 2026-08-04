# OPCBS Role Modules Progress & Audit Document

## Module Audit & Inventory Table

| Role | Menu/Page | Route | Current UI | API/Data Source | Functional Status | Problem | Required Action |
|------|-----------|-------|------------|-----------------|-------------------|---------|-----------------|
| **Customer Support** | Dashboard | `/CustomerSupport/Dashboard` | KPI Cards, Doctor Apps, Blog Queue | `ICustomerSupportApiService.GetDashboardStatsAsync` | Partially working | Uses generic cards; stats format unstandardized | Standardize UI with compact OPCBS design, link to real operational queue counts |
| **Customer Support** | Doctor Applications List | `/CustomerSupport/DoctorApplications/Index` | Table with status filter tabs | `ICustomerSupportApiService.GetDoctorApplicationsAsync` | Working | Missing search by doctor name/license; no pagination controls | Add search bar, pagination component, and status filter tabs |
| **Customer Support** | Application Review Detail | `/CustomerSupport/DoctorApplications/Details` | Doctor profile info, Certificate preview, Approve/Reject forms | `ICustomerSupportApiService.GetApplicationByIdAsync`, `ReviewApplicationAsync` | Partially working | Missing "Request Additional Info" workflow with mandatory reason; certificate status/history display needs standardization | Add Request Additional Info action with mandatory reason, standardize file preview/downloads and review history |
| **Customer Support** | Blog Moderation Queue | `/CustomerSupport/BlogModeration/Index` | List of pending blogs | `ICustomerSupportApiService.GetBlogModerationQueueAsync` | Working | Simple list layout | Standardize list UI, ensure clear empty state and status badges |
| **CustomerSupport** | Blog Moderation Detail | `/CustomerSupport/BlogModeration/Details` | Blog preview, Approve/Reject buttons | `ICustomerSupportApiService.GetBlogForModerationAsync`, `ApproveBlogAsync`, `RejectBlogAsync` | Working | Rejection reason optional in frontend form | Make rejection reason required, validate input and display error messages |
| **System Admin** | Dashboard | `/Admin/Dashboard` | Stat cards for users/doctors/appts | `IAdminApiService.GetDashboardStatsAsync` | Partially working | Generic summary cards without system activity log | Replace with compact OPCBS metrics cards and recent system activity audit feed |
| **System Admin** | User Management List | `/Admin/Users/Index` | User table with lock/unlock actions | `IAdminApiService.GetUsersAsync`, `LockUserAsync`, `UnlockUserAsync` | Partially working | Backend `GetUsersAsync` ignores `Role` filter; Admin can lock own account | Add backend role filtering; add guard preventing Admin self-lockout |
| **System Admin** | User Details | `/Admin/Users/Details` | User profile details and status controls | `IAdminApiService.GetUserByIdAsync` | Working | Basic layout | Standardize UI, add lock/unlock confirmation modals and activity history |
| **System Admin** | Role Management | `/Admin/Roles/Index` | Roles list table | `IAdminApiService.GetRolesAsync` | Broken API | Backend `/api/v1/admin/roles` returns stub reusing Specializations data | Implement real roles endpoint returning system roles and user counts per role |
| **System Admin** | Permission Management | `/Admin/Permissions/Index` | Read-only matrix table | Hardcoded frontend model | UI only | Static permission matrix not backed by real permission query | Connect to system permission matrix model, present clear role-permission capabilities |
| **System Admin** | Audit Logs | `/Admin/AuditLogs/Index` | Audit log table with entity search | `IAdminApiService.GetAuditLogsAsync` | Working | Lacks date/action filters and detail view | Add filter options, entity search, and modal for full log details |
| **System Admin** | System Settings | `/Admin/Settings` | Configuration tabs (Security, App, Email, Maintenance) | `IAdminApiService.GetSystemSettingsAsync`, `UpdateSystemSettingsAsync` | Working | Missing form validation feedback and confirmation on maintenance mode | Enhance UX with validation alerts and maintenance mode warning modal |
| **System Admin** | Reports | `/Admin/Reports` | Blank page with TODO comment | None | Missing / Duplicate | Page contains no functionality | Implement System Usage & Registration Reports with real EF Core aggregation |
| **Business Manager** | Dashboard | `/BusinessManager/Dashboard` | KPI Cards, Packages, Specializations | `IBusinessManagerApiService.GetDashboardStatsAsync` | Partially working | Shows general user stats instead of business revenue/subscription metrics | Update dashboard to fetch real subscription stats, package adoption, and revenue in VND |
| **Business Manager** | Service Packages List | `/BusinessManager/ServicePackages/Index` | Package list table | `IBusinessManagerApiService.GetServicePackagesAsync` | Working | Prices not formatted in VND consistently | Format price in VND, add active/inactive filter |
| **Business Manager** | Service Package Create/Edit | `/BusinessManager/ServicePackages/Create`, `Edit` | Form for package creation/editing | `CreateServicePackageAsync`, `UpdateServicePackageAsync` | Working | Basic validation | Add client and server-side validation, VND price formatting |
| **Business Manager** | Specializations List | `/BusinessManager/Specializations/Index` | Specializations table | `IBusinessManagerApiService.GetSpecializationsAsync` | Working | Simple list | Standardize UI, add doctor count per specialization |
| **Business Manager** | Specialization Create/Edit | `/BusinessManager/Specializations/Create`, `Edit` | Specialization form | `CreateSpecializationAsync`, `UpdateSpecializationAsync` | Working | Basic validation | Enhance form styling and error handling |
| **Business Manager** | Doctor Subscriptions | `/BusinessManager/Subscriptions/Index` | Missing | None | Missing | Business Manager cannot view or manage doctor subscriptions | Create Subscription Management page with Active/Pending/Expired tabs & filters |
| **Business Manager** | Payment Transactions | `/BusinessManager/Payments/Index` | Missing | None | Missing | Business Manager cannot view payment transactions | Create Payment History page with Completed/Failed/Refunded filters and VND totals |
| **Business Manager** | Analytics | `/BusinessManager/Analytics` | Duplicates Dashboard UI | `IBusinessManagerApiService.GetDashboardStatsAsync` | Duplicate / Mock | Duplicates dashboard page | Build dedicated Business Analytics page with revenue trends, subscription breakdown, and package popularity |
| **Business Manager** | Reports | `/BusinessManager/Reports` | Duplicate of Dashboard stats | None | UI only / Mock | Shows duplicate dashboard stats | Build Financial & Subscription Reports page with date range filtering and VND summary |

## Implementation Plan

### Phase 1: Support Staff Module [COMPLETED]
1. Enhanced Doctor Verification review workflow:
   - Added `RequiresAdditionalInfo` status transition and `RequestAdditionalInfoAsync` workflow.
   - Standardized certificate viewer with type, file name, upload date, status badge, preview, and secure download.
   - Enforced mandatory reasons for both Rejection and Request Additional Information actions.
   - Connected practitioner profile information and complete review history timeline with reviewer name.
   - Added search bar (doctor name, license, specialization) and status filter tabs (Pending, Needs Info, Approved, Rejected).
2. Standardized Customer Support Dashboard:
   - Integrated real operational stats for pending verifications and blog moderation queues.
   - Redesigned with compact OPCBS design system (8px radius, flat border cards).

### Phase 2: System Admin Module [COMPLETED]
1. User Management & Authorization:
   - Added backend `GetUsersAsync` role filtering and search across email, name, and phone.
   - Added `GetUserByIdAsync` for deep profile inspect.
   - Enforced Admin self-lockout guard (`userId == requestingAdminId`) and protected `SystemAdmin` accounts from being locked.
   - Connected Role Directory page to real system roles with active user counts.
   - Added CSV Data Export functionality for Users, Audit Logs, and System Roles on Reports page.
   - Standardized UI across Account List, User Details, Role Matrix, Audit Logs, Settings, and Reports pages.

### Phase 3: Business Manager Module [COMPLETED]
1. Service Package Management:
   - Full CRUD for platform Service Packages (name, description, durationDays, price, capacity).
   - VND currency formatting (`N0` VND / `đ`) across all list views and forms.
   - Deactivation and active status toggling with confirmation modal.
2. Doctor Subscriptions & Payment Monitoring:
   - Added `GetAllSubscriptionsAsync` and `GetSubscriptionByIdAsync` endpoints.
   - Created Doctor Subscriptions index (`Pages/BusinessManager/Subscriptions/Index.cshtml`) and details view with search and status filters (Active, Expired, Cancelled, PendingPayment).
   - Sidebar navigation integration under Business Manager portal.
3. Business Dashboard & Specialization Management:
   - Display real total revenue in VND, active subscriptions, package counts, and active doctors.
   - Medical specializations management.

### Phase 3: Business Manager Module
1. Add Subscription & Payment Management for Business Manager:
   - Implement backend APIs for all doctor subscriptions and payment transactions (`GET /api/v1/business-manager/subscriptions`, `GET /api/v1/business-manager/payments`).
   - Create `Pages/BusinessManager/Subscriptions/Index.cshtml` and `Pages/BusinessManager/Payments/Index.cshtml`.
   - Add navigation links to `_DashboardSidebar.cshtml`.
2. Fix Business Dashboard, Analytics & Reports:
   - Compute real revenue in VND, active subscriptions count, package adoption statistics.
   - Replace duplicate screens with real analytics and financial reports.

---
## Continuation Checkpoint Log
- **Date**: 2026-08-04
- **Current Step**: Feature Audit Complete. Commencing Phase 1 Implementation.
