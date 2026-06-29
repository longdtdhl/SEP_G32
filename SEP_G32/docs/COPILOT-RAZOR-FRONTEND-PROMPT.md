# Prompt: Complete OPCBS Razor Pages Frontend Only

You are working in the existing OPCBS repository. Copilot/Antigravity previously worked on the backend API. Your task now is to complete the Razor Pages frontend only, using the existing backend API contract.

Do not read the entire repository. Do not regenerate backend architecture. Do not rewrite Domain/Application/Infrastructure unless a tiny DTO/API-client compatibility fix is absolutely required for the frontend to compile.

## Read Only These First

Read these files/directories only:

```text
docs/AI-GENERATE-FULL-WEBSITE-PROMPT.md
docs/UI-DESIGN-BRIEF-MINDBRIDGE.md
docs/11-api-design.md
docs/16-screen-components.md
docs/18-permission-matrix.md
docs/ui-template/**/*.html
backend/OPCBS.Web/Program.cs
backend/OPCBS.Web/Pages/**/*.cshtml
backend/OPCBS.Web/Pages/**/*.cs
backend/OPCBS.Web/Services/**/*.cs
backend/OPCBS.Web/DTOs/**/*.cs
backend/OPCBS.Web/Constants/ApiRoutes.cs
backend/OPCBS.Web/wwwroot/css/site.css
backend/OPCBS.Web/wwwroot/js/site.js
```

If you need to understand an API response shape, read only the related controller and DTO files for that feature:

```text
backend/OPCBS/Controllers/<RelevantController>.cs
backend/OPCBS.Application/DTOs/<RelevantFolder>/**/*.cs
backend/OPCBS.Shared/Models/ApiResponse.cs
```

Do not scan the whole backend.

## Current Frontend State

The current `OPCBS.Web` Razor frontend is still mostly skeleton:

- Home page is the default ASP.NET welcome page.
- CSS is mostly default.
- Existing pages are very small: account login/register/OTP/profile, doctor list/detail, blog list/detail, appointment book.
- Role dashboards and most real screens do not exist yet.
- API client services may be outdated because backend API responses use `ApiResponse<T>` wrappers.

Your job is to turn `backend/OPCBS.Web` into a usable responsive Razor Pages frontend.

## Primary Goal

Complete the Razor Pages frontend for OPCBS using Bootstrap 5 and the visual/workflow references in:

```text
docs/ui-template/**/*.html
```

Adapt the templates into maintainable Razor Pages, shared partials, ViewModels/DTOs, and API client services. Do not copy giant static HTML blindly. Extract reusable layout and components.

## Strict Scope

Allowed:

- Edit `backend/OPCBS.Web/**`.
- Add Razor Pages, partials, view components, DTOs, API clients, CSS, and JS.
- Add small shared frontend helper models inside `OPCBS.Web`.
- Read relevant backend controllers/DTOs only when needed to make API calls correct.
- Fix `ApiRoutes.cs` and API client response parsing.

Avoid:

- Editing backend business services.
- Editing Domain/Application/Infrastructure.
- Adding new backend endpoints unless the frontend cannot function and the API docs clearly require the endpoint.
- Reworking database, migrations, or seed data.
- Reading unrelated backend files.

## Frontend Architecture Requirements

Use:

- ASP.NET Core Razor Pages
- Bootstrap 5
- Existing `_Layout.cshtml`, `_Header.cshtml`, `_Footer.cshtml`, `_Sidebar.cshtml`
- `HttpClient` typed API services
- Cookie/JWT handling already present in `JwtCookieService`
- Server-side Razor PageModels for page loading and form posts
- Client-side JavaScript only for UI behavior, filtering widgets, toasts, modals, date/slot selection, and progressive enhancement

Do not put business rules in `.cshtml`. Business rules belong to backend. Frontend should display data, collect input, call API, and show errors.

## Critical API Client Fix

Backend APIs return a wrapper similar to:

```json
{
  "success": true,
  "message": "Operation successful",
  "data": {},
  "errors": [],
  "pagination": {}
}
```

Update all `OPCBS.Web` API clients to parse this envelope instead of assuming raw arrays/objects.

Create reusable frontend models, for example:

```csharp
public class ApiResponseDto<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
    public PaginationDto? Pagination { get; set; }
}

public class PaginationDto
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}
```

Make typed services return clean DTOs to PageModels and preserve API error messages for display.

## Required Shared UI

Implement reusable partials/components inspired by `docs/16-screen-components.md`:

- Navbar
- Footer
- Sidebar
- Breadcrumb
- SearchBar
- DoctorCard
- DoctorFilterPanel
- DoctorScheduleCalendar / slot picker
- AppointmentStatusBadge
- AppointmentTable
- BlogCard
- RatingStars
- DashboardStatsCard
- Pagination
- EmptyState
- ErrorState
- ToastNotification
- ConfirmDialog
- FileUploader UI shell

Use Bootstrap 5 components and consistent styling in `site.css`.

## Required Pages

Build these pages in `backend/OPCBS.Web/Pages`.

### Public

```text
/Index
/Doctors/Index
/Doctors/Details/{id}
/Doctors/Ratings/{id}
/Blog/Index
/Blog/Details/{id}
/Appointment/Book/{doctorId?}
/Appointment/Track
```

### Account

```text
/Account/Login
/Account/Register
/Account/VerifyOtp
/Account/ForgotPassword
/Account/ResetPassword
/Account/ChangePassword
/Account/Profile
/Account/Logout
```

### Patient

```text
/Patient/Dashboard
/Patient/Appointments/Index
/Patient/Appointments/Details/{id}
/Patient/Appointments/Reschedule/{id}
/Patient/ConsultationRecords/Index
/Patient/TreatmentPackages/Index
/Patient/TreatmentPackages/Details/{id}
/Patient/Reviews/Create/{appointmentId}
```

### Doctor

```text
/Doctor/Dashboard
/Doctor/Profile
/Doctor/Verification
/Doctor/VerificationStatus
/Doctor/Schedules/Index
/Doctor/Schedules/Create
/Doctor/Schedules/Edit/{id}
/Doctor/Schedules/DaysOff
/Doctor/Appointments/Index
/Doctor/Appointments/Details/{id}
/Doctor/ConsultationRecords/Index
/Doctor/ConsultationRecords/Create/{appointmentId}
/Doctor/ConsultationRecords/Edit/{id}
/Doctor/TreatmentPackages/Index
/Doctor/TreatmentPackages/Create
/Doctor/TreatmentPackages/Edit/{id}
/Doctor/Blogs/Index
/Doctor/Blogs/Create
/Doctor/Blogs/Edit/{id}
/Doctor/ServicePackages/Index
/Doctor/Subscriptions/Status
/Doctor/Subscriptions/History
```

### Customer Support

```text
/CustomerSupport/Dashboard
/CustomerSupport/DoctorApplications/Index
/CustomerSupport/DoctorApplications/Details/{id}
/CustomerSupport/BlogModeration/Index
/CustomerSupport/BlogModeration/Details/{id}
```

### Business Manager

```text
/BusinessManager/Dashboard
/BusinessManager/ServicePackages/Index
/BusinessManager/Specializations/Index
/BusinessManager/Analytics
/BusinessManager/Reports
```

### System Admin

```text
/Admin/Dashboard
/Admin/Users/Index
/Admin/Roles/Index
/Admin/AuditLogs/Index
/Admin/Reports
/Admin/Settings
```

If a backend endpoint is missing for a page, still create the page shell with a professional empty/error state and a TODO comment naming the required API endpoint from `docs/11-api-design.md`.

## Design Direction

Use `docs/ui-template` as reference:

- Apply the MindBridge design direction from `docs/UI-DESIGN-BRIEF-MINDBRIDGE.md`.
- Public/patient pages should feel calm, trustworthy, and easy to book from.
- Doctor pages should feel clinical, organized, and task-focused.
- CustomerSupport/BusinessManager/Admin pages should be dashboard/table driven and efficient.
- Use Bootstrap cards only for repeated items or tool panels. Avoid nested cards.
- Use clear status badges for appointment, verification, blog, subscription, and payment states.
- Use responsive grids and tables that work on mobile.
- Keep management pages dense and scannable.
- Use MindBridge as the user-facing brand consistently. Do not mix random template brands like SerenityPath or Sanctuary.

## Authentication and Navigation

Update header/sidebar behavior:

- Guest sees Home, Doctors, Blogs, Track Appointment, Login, Register.
- Patient sees patient dashboard, my appointments, treatment packages, consultation records, profile.
- Doctor sees doctor dashboard, appointments, schedules, records, packages, blogs, service package/subscription, verification status.
- CustomerSupport sees verification and blog moderation.
- BusinessManager sees service packages, specializations, analytics/reports.
- SystemAdmin sees users, roles/permissions, audit logs, reports/settings.

If role detection is not fully available in cookie claims, implement a minimal helper around existing auth/token storage and show safe defaults. Do not block page rendering because role extraction is incomplete; add TODO comments.

## Page Behavior Requirements

For each page:

- Use `TempData` or a toast partial for success/error messages.
- Show backend validation/API errors clearly.
- Use loading/empty/error states for lists.
- Add pagination UI for list pages when API returns pagination.
- Add search/filter forms for doctor, blog, appointment, user, and moderation list pages.
- Use confirmation modal/dialog for approve, reject, cancel, archive, lock, unlock, delete, and submit-review actions.
- Prefer POST handlers in PageModels for mutations.
- Keep `.cshtml.cs` PageModels small and route API calls through typed services.

## API Client Services To Create/Update

Create or update typed clients under `backend/OPCBS.Web/Services`:

- `IAuthApiService` / `AuthApiService`
- `IUserApiService` / `UserApiService`
- `IDoctorApiService` / `DoctorApiService`
- `IAppointmentApiService` / `AppointmentApiService`
- `IScheduleApiService` / `ScheduleApiService`
- `IConsultationRecordApiService` / `ConsultationRecordApiService`
- `ITreatmentPackageApiService` / `TreatmentPackageApiService`
- `IBlogApiService` / `BlogApiService`
- `IReviewApiService` / `ReviewApiService`
- `IVerificationApiService` / `VerificationApiService`
- `IServicePackageApiService` / `ServicePackageApiService`
- `ISubscriptionApiService` / `SubscriptionApiService`
- `INotificationApiService` / `NotificationApiService`
- `IAdminApiService` / `AdminApiService`
- `ICustomerSupportApiService` / `CustomerSupportApiService`
- `IBusinessManagerApiService` / `BusinessManagerApiService`

Register them in `backend/OPCBS.Web/Program.cs`.

## Suggested Work Order

Do not try to finish everything in one huge pass. Work in this order:

1. Fix frontend API response wrapper and service infrastructure.
2. Improve layout, header, footer, sidebar, CSS, toast/error/empty/pagination partials.
3. Complete public pages: Home, Doctors, Doctor Details, Blogs, Blog Details, Booking, Track Appointment.
4. Complete account pages.
5. Complete patient pages.
6. Complete doctor pages.
7. Complete CustomerSupport pages.
8. Complete BusinessManager pages.
9. Complete Admin pages.
10. Run build and fix compile errors.

At the end of each session, update:

```text
docs/PROGRESS-FRONTEND.md
```

Include:

- Pages completed
- API clients completed
- Files changed
- Build result
- Pages still missing
- Known backend endpoint gaps
- Next recommended task

## Completion Criteria

The frontend phase is complete when:

- `dotnet build backend/OPCBS.sln` succeeds.
- All required Razor Pages exist.
- Public pages render with real layouts, not ASP.NET defaults.
- Role dashboards exist and match their workflow purpose.
- API clients parse `ApiResponse<T>` correctly.
- Common UI states exist: success/error/empty/loading/pagination.
- Navigation is role-aware enough for the current auth implementation.
- Pages use `docs/ui-template` visual ideas but are maintainable Razor Pages.

Do not claim the frontend is complete if most pages are only empty placeholders.
