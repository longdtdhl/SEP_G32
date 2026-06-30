# Prompt: Build Business Manager Razor Pages Frontend Only

You are working in the OPCBS repository. Your task is to implement the **Business Manager frontend module** in `backend/OPCBS.Web` using ASP.NET Core Razor Pages.

Do not read the entire repository. Do not regenerate backend architecture. Do not rewrite Domain/Application/Infrastructure. Only touch backend API if the Business Manager UI cannot work or compile without a very small compatibility fix.

## Scope

Primary scope:

```text
backend/OPCBS.Web/**
```

Allowed small backend fixes only when necessary:

```text
backend/OPCBS/Controllers/AdminSupportController.cs
backend/OPCBS.Application/DTOs/**/*.cs
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
docs/ui-template/ui-bussinessmanager/AnalysticDashboard.html
docs/ui-template/ui-bussinessmanager/ServicePackageManagr.html
docs/ui-template/ui-bussinessmanager/SpecializationManagement.html
docs/ui-template/ui-bussinessmanager/ManageBlogs.html
docs/ui-template/ui-bussinessmanager/ViewDoctorApplications.html
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
backend/OPCBS.Shared/Models/ApiResponse.cs
```

If you need to understand a response DTO, read only the related DTO/interface/service file. Do not scan all backend code.

## Goal

Build a usable, polished Business Manager UI for:

- Business Manager dashboard
- Analytics overview
- Service package management
- Specialization management
- Reports shell
- Optional operational overview pages when supported by API

Use the `docs/ui-template/ui-bussinessmanager/*.html` files as visual references. Adapt them into maintainable Razor Pages and shared components. Do not copy giant static HTML blindly.

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

Use `MindBridge` consistently in UI text. Do not mix template brands like SerenityPath or Sanctuary.

## Required Pages

Create these Razor Pages:

```text
backend/OPCBS.Web/Pages/BusinessManager/Dashboard.cshtml
backend/OPCBS.Web/Pages/BusinessManager/Dashboard.cshtml.cs

backend/OPCBS.Web/Pages/BusinessManager/Analytics.cshtml
backend/OPCBS.Web/Pages/BusinessManager/Analytics.cshtml.cs

backend/OPCBS.Web/Pages/BusinessManager/ServicePackages/Index.cshtml
backend/OPCBS.Web/Pages/BusinessManager/ServicePackages/Index.cshtml.cs
backend/OPCBS.Web/Pages/BusinessManager/ServicePackages/Create.cshtml
backend/OPCBS.Web/Pages/BusinessManager/ServicePackages/Create.cshtml.cs
backend/OPCBS.Web/Pages/BusinessManager/ServicePackages/Edit.cshtml
backend/OPCBS.Web/Pages/BusinessManager/ServicePackages/Edit.cshtml.cs

backend/OPCBS.Web/Pages/BusinessManager/Specializations/Index.cshtml
backend/OPCBS.Web/Pages/BusinessManager/Specializations/Index.cshtml.cs
backend/OPCBS.Web/Pages/BusinessManager/Specializations/Create.cshtml
backend/OPCBS.Web/Pages/BusinessManager/Specializations/Create.cshtml.cs
backend/OPCBS.Web/Pages/BusinessManager/Specializations/Edit.cshtml
backend/OPCBS.Web/Pages/BusinessManager/Specializations/Edit.cshtml.cs

backend/OPCBS.Web/Pages/BusinessManager/Reports.cshtml
backend/OPCBS.Web/Pages/BusinessManager/Reports.cshtml.cs
```

If a backend endpoint is missing, still create a polished page shell with an empty/error state and a TODO comment naming the required endpoint from `docs/11-api-design.md`.

## Required Business Manager API Client

Create or update:

```text
backend/OPCBS.Web/Services/IBusinessManagerApiService.cs
backend/OPCBS.Web/Services/BusinessManagerApiService.cs
backend/OPCBS.Web/DTOs/BusinessManagerDtos.cs
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

Do not parse Business Manager endpoints as raw arrays unless the actual backend returns raw arrays. Prefer a reusable `ApiResponseDto<T>` wrapper.

## Expected API Endpoints

Use endpoints from `docs/11-api-design.md` where available:

```text
GET /api/v1/business-manager/dashboard
GET /api/v1/business-manager/analytics
GET /api/v1/business-manager/reports
POST /api/v1/business-manager/specializations
PUT /api/v1/business-manager/specializations/{id}
DELETE /api/v1/business-manager/specializations/{id}

GET /api/v1/service-packages
POST /api/v1/service-packages
PUT /api/v1/service-packages/{id}
DELETE /api/v1/service-packages/{id}
```

Check current backend route implementations before calling them. If current controller routes differ from the spec, either:

1. Make a tiny backend route compatibility fix when low risk, or
2. Keep the client aligned with the existing backend and add a TODO noting the route mismatch.

Do not invent unsupported mutations. If delete/archive is not supported, show an inactive/activate/deactivate action only if backend supports it.

## UI Requirements

### Layout

Business Manager pages should use a professional operational dashboard layout:

- Business Manager sidebar navigation
- Top bar with page title, breadcrumb, search/profile area
- Content area with analytics cards, tables, forms
- Responsive behavior for tablet/mobile

Update shared sidebar/header only enough to support Business Manager navigation. Do not redesign every role's navigation.

### Dashboard

Show stat cards for:

- Revenue overview, if API provides it
- Active service packages
- Total subscriptions, if API provides it
- Active doctors, if API provides it
- Pending/expired subscriptions, if API provides it
- Operational alerts

If API does not provide metrics, show a graceful placeholder with TODO.

### Analytics

Use `AnalysticDashboard.html` as visual reference. Implement:

- KPI cards
- Revenue/chart placeholder
- Top service packages
- Top specializations
- Recent business events

Use simple Bootstrap/CSS chart placeholders if no chart library exists. Do not add a heavy chart dependency unless already present.

### Service Package Management

Use `ServicePackageManagr.html` as visual reference. Implement:

- Search/filter form
- Package table/cards
- Create form
- Edit form
- Price, duration, active status
- Status badges
- Confirm modal for deactivate/delete/archive actions
- Empty/error states

Fields should align with backend DTO/spec:

- Name
- Description
- Price
- DurationDays
- IsActive

### Specialization Management

Use `SpecializationManagement.html` as visual reference. Implement:

- Search/filter form
- Specialization table
- Create form
- Edit form
- Active status if available
- Empty/error states

Fields should align with backend DTO/spec:

- Name
- Description
- IsActive, if available

### Reports

Create a polished reports shell:

- Revenue report card
- Doctor/service package report card
- Appointment operations report card
- Export buttons disabled if backend is missing
- TODO comments for missing export endpoints

## Components To Add Or Reuse

Prefer partials under:

```text
backend/OPCBS.Web/Pages/Shared
```

Suggested partials:

```text
_BusinessManagerSidebar.cshtml
_StatusBadge.cshtml
_Pagination.cshtml
_EmptyState.cshtml
_ErrorState.cshtml
_ConfirmDialog.cshtml
_ToastNotification.cshtml
_DashboardStatsCard.cshtml
```

Keep components simple and reusable.

## Authorization

Business Manager Razor Pages should require BusinessManager access.

If the project does not yet have robust Razor Page authorization policies, add `[Authorize(Roles = "BusinessManager")]` to the Business Manager PageModels or configure folder authorization in `Program.cs`.

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

- All Business Manager Razor Page files exist.
- `IBusinessManagerApiService` and `BusinessManagerApiService` exist.
- Business Manager routes are linked from sidebar.
- Service package management can call list/create/update endpoints or shows clear TODO for endpoint gaps.
- Specialization management can call list/create/update endpoints or shows clear TODO for endpoint gaps.
- Build succeeds or remaining blocker is explicitly environmental.

## Progress File

At the end, create or update:

```text
docs/PROGRESS-FRONTEND-BUSINESSMANAGER.md
```

Include:

- Pages completed
- API clients completed
- Files changed
- Backend endpoints used
- Backend endpoint gaps
- Build result
- Next task recommendation
