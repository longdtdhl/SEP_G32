# Prompt: Build System Admin Razor Pages Frontend Only

You are working in the OPCBS repository. Your task is to implement the **System Admin frontend module** in `backend/OPCBS.Web` using ASP.NET Core Razor Pages.

Do not read the entire repository. Do not regenerate backend architecture. Do not rewrite Domain/Application/Infrastructure. Only touch backend API if the System Admin UI cannot work or compile without a very small compatibility fix.

## Scope

Primary scope:

```text
backend/OPCBS.Web/**
```

Allowed small backend fixes only when necessary:

```text
backend/OPCBS/Controllers/AdminSupportController.cs
backend/OPCBS/Controllers/UsersController.cs
backend/OPCBS.Application/DTOs/Auth/AuthDtos.cs
backend/OPCBS.Application/Interfaces/Services/IBusinessServices.cs
backend/OPCBS.Application/Services/BusinessServices.cs
backend/OPCBS.Shared/Models/ApiResponse.cs
```

Do not edit database migrations, entities, DbContext, seed data, or unrelated backend services unless explicitly required by a compile-breaking mismatch.

## Read Only These First

Read these files only at the start:

```text
docs/UI-DESIGN-BRIEF-MINDBRIDGE.md
docs/COPILOT-RAZOR-FRONTEND-PROMPT.md
docs/11-api-design.md
docs/16-screen-components.md
docs/18-permission-matrix.md
docs/ui-template/ui-systemadmin/AccountManage.html
backend/OPCBS.Web/Program.cs
backend/OPCBS.Web/Pages/Shared/_Layout.cshtml
backend/OPCBS.Web/Pages/Shared/_Header.cshtml
backend/OPCBS.Web/Pages/Shared/_Sidebar.cshtml
backend/OPCBS.Web/Pages/Shared/_Footer.cshtml
backend/OPCBS.Web/Constants/ApiRoutes.cs
backend/OPCBS.Web/Services/**/*.cs
backend/OPCBS.Web/DTOs/**/*.cs
backend/OPCBS.Web/wwwroot/css/site.css
backend/OPCBS.Web/wwwroot/js/site.js
backend/OPCBS/Controllers/AdminSupportController.cs
backend/OPCBS/Controllers/UsersController.cs
backend/OPCBS.Shared/Models/ApiResponse.cs
```

If you need to understand a response DTO, read only the related DTO/interface/service file. Do not scan all backend code.

## Goal

Build a usable, polished System Admin UI for:

- Admin dashboard
- User/account management
- Role management shell
- Permission management shell
- Audit logs
- Reports shell
- Settings shell

Use `docs/ui-template/ui-systemadmin/AccountManage.html` as the main visual reference. Adapt it into maintainable Razor Pages and shared components.

Use MindBridge visual language from `docs/UI-DESIGN-BRIEF-MINDBRIDGE.md`:

- Primary: `#4E8B70`
- Primary hover: `#3F735C`
- Secondary: `#DCEEE4`
- Background: `#FAFBFA`
- Card: `#FFFFFF`
- Border: `#E5E7EB`
- Primary text: `#1F2937`
- Secondary text: `#6B7280`
- Success: `#10B981`
- Warning: `#F59E0B`
- Error: `#EF4444`
- Font: Inter

Do not mix template brands like SerenityPath or Sanctuary. Use `MindBridge` consistently in UI text.

## Required Pages

Create these Razor Pages:

```text
backend/OPCBS.Web/Pages/Admin/Dashboard.cshtml
backend/OPCBS.Web/Pages/Admin/Dashboard.cshtml.cs

backend/OPCBS.Web/Pages/Admin/Users/Index.cshtml
backend/OPCBS.Web/Pages/Admin/Users/Index.cshtml.cs
backend/OPCBS.Web/Pages/Admin/Users/Details.cshtml
backend/OPCBS.Web/Pages/Admin/Users/Details.cshtml.cs

backend/OPCBS.Web/Pages/Admin/Roles/Index.cshtml
backend/OPCBS.Web/Pages/Admin/Roles/Index.cshtml.cs

backend/OPCBS.Web/Pages/Admin/Permissions/Index.cshtml
backend/OPCBS.Web/Pages/Admin/Permissions/Index.cshtml.cs

backend/OPCBS.Web/Pages/Admin/AuditLogs/Index.cshtml
backend/OPCBS.Web/Pages/Admin/AuditLogs/Index.cshtml.cs

backend/OPCBS.Web/Pages/Admin/Reports.cshtml
backend/OPCBS.Web/Pages/Admin/Reports.cshtml.cs

backend/OPCBS.Web/Pages/Admin/Settings.cshtml
backend/OPCBS.Web/Pages/Admin/Settings.cshtml.cs
```

If a backend endpoint is missing, still create a polished page shell with an empty/error state and a TODO comment naming the required endpoint from `docs/11-api-design.md`.

## Required Admin API Client

Create or update:

```text
backend/OPCBS.Web/Services/IAdminApiService.cs
backend/OPCBS.Web/Services/AdminApiService.cs
backend/OPCBS.Web/DTOs/AdminDtos.cs
backend/OPCBS.Web/DTOs/ApiResponseDtos.cs
```

Register the client in:

```text
backend/OPCBS.Web/Program.cs
```

The backend API uses an envelope:

```json
{
  "success": true,
  "message": "Operation successful",
  "data": {},
  "errors": [],
  "pagination": {}
}
```

Do not parse admin endpoints as raw arrays unless the actual backend returns raw arrays. Prefer a reusable `ApiResponseDto<T>` wrapper.

## Expected Admin API Endpoints

Use endpoints from `docs/11-api-design.md` when available:

```text
GET /api/v1/admin/dashboard
GET /api/v1/admin/users
PUT /api/v1/admin/users/{id}/lock
PUT /api/v1/admin/users/{id}/unlock
GET /api/v1/admin/roles
GET /api/v1/admin/permissions
GET /api/v1/admin/audit-logs
GET /api/v1/admin/reports
```

Check current backend route implementations before calling them. If current controller uses `POST` instead of `PUT` for lock/unlock, either:

1. Make a tiny backend route compatibility fix to support the documented `PUT`, or
2. Keep the client aligned with the existing backend and add a TODO noting the route mismatch.

Prefer fixing tiny route mismatches only when low risk.

## UI Requirements

### Layout

Admin pages should use a professional dashboard layout:

- Admin sidebar navigation
- Top bar with title, breadcrumb, search, profile area
- Content area with cards/tables
- Responsive behavior for tablet/mobile

Update shared sidebar/header only enough to support System Admin navigation. Do not redesign every role's navigation.

### Dashboard

Show stat cards for:

- Total users
- Active users
- Locked users
- Doctors pending verification, if API provides it
- Audit events
- Recent activity

If API does not provide some metrics, show a graceful placeholder with TODO.

### Users Index

Implement:

- Search box
- Role/status filters
- Paginated table
- Status badges
- Role badges
- User detail link
- Lock/unlock actions with confirm modal
- Empty state
- Error state

Columns:

- Full name
- Email
- Phone
- Role
- Status
- Email verified
- Created at
- Actions

### User Details

Implement:

- User profile summary
- Role/status/email verification info
- Recent account activity placeholder
- Lock/unlock action
- Back to list

### Roles and Permissions

If backend supports only list endpoints, implement read-only list shells:

- Role table
- Permission table
- Empty/error states
- TODO comments for create/update/delete if not available

Do not invent unsupported role-management mutations.

### Audit Logs

Implement:

- Search/filter form
- Entity/action/user filters if API data supports it
- Paginated audit log table
- Date/action/entity columns
- Expandable description/details if available

### Reports and Settings

Create polished admin shells:

- Reports: cards for user report, appointment report, audit report, export placeholder.
- Settings: security/system configuration placeholders.

Do not fake destructive functionality. Use disabled buttons or TODO notes when backend is missing.

## Components To Add Or Reuse

Prefer partials under:

```text
backend/OPCBS.Web/Pages/Shared
```

Suggested partials:

```text
_AdminLayoutHeader.cshtml
_AdminSidebar.cshtml
_StatusBadge.cshtml
_Pagination.cshtml
_EmptyState.cshtml
_ErrorState.cshtml
_ConfirmDialog.cshtml
_ToastNotification.cshtml
```

Keep components simple and reusable.

## Authorization

Admin Razor Pages should require SystemAdmin access.

If the project does not yet have robust Razor Page authorization policies, add `[Authorize(Roles = "SystemAdmin")]` to the Admin PageModels or configure folder authorization in `Program.cs`.

If role claims are not reliable yet, add TODO comments, but still structure the page as protected.

## Coding Rules

- PageModels call typed API services.
- `.cshtml` files contain display logic only.
- Do not put business rules in Razor.
- Do not duplicate backend validation logic except basic HTML required fields.
- Keep files readable.
- Use Bootstrap 5.
- Avoid giant copied HTML from template.
- Preserve API errors and display them to the user.
- Use `TempData` for success/error messages after POST actions.

## Verification

After implementation, run:

```powershell
dotnet build backend/OPCBS.sln
```

If build fails due sandbox/NuGet permissions in the AI environment, report that clearly and still fix compile errors visible from the output.

Do not claim completion until:

- All Admin Razor Page files exist.
- `IAdminApiService` and `AdminApiService` exist.
- Admin routes are linked from admin sidebar.
- User management page can call list and lock/unlock endpoints or shows clear TODO for endpoint gaps.
- Build succeeds or remaining blocker is explicitly environmental.

## Progress File

At the end, create or update:

```text
docs/PROGRESS-FRONTEND-SYSTEMADMIN.md
```

Include:

- Pages completed
- API clients completed
- Files changed
- Backend endpoints used
- Backend endpoint gaps
- Build result
- Next task recommendation
