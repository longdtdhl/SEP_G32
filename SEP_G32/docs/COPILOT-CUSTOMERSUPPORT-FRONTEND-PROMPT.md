# Prompt: Build Customer Support Razor Pages Frontend Only

You are working in the OPCBS repository. Your task is to complete the **Customer Support frontend module** in `backend/OPCBS.Web` using ASP.NET Core Razor Pages.

Do not read the entire repository. Do not regenerate backend architecture. Do not rewrite Domain/Application/Infrastructure. Only touch backend API when the Customer Support UI cannot work or compile without a very small compatibility fix.

## Scope

Primary scope:

```text
backend/OPCBS.Web/**
```

Allowed small backend fixes only when necessary:

```text
backend/OPCBS/Controllers/AdminSupportController.cs
backend/OPCBS/Controllers/BlogsReviewsController.cs
backend/OPCBS.Application/DTOs/**/*.cs
backend/OPCBS.Application/Interfaces/Services/**/*.cs
backend/OPCBS.Application/Services/**/*.cs
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
docs/ui-template/ui-customersupport/CustomerSupportDashboard.html
docs/ui-template/ui-customersupport/DoctorApplicationsDetail.html
docs/ui-template/ui-customersupport/BlogModerationQueue.html
docs/ui-template/ui-customersupport/BlogDetail&Editors.html
backend/OPCBS.Web/Program.cs
backend/OPCBS.Web/Pages/Shared/_Layout.cshtml
backend/OPCBS.Web/Pages/Shared/_Header.cshtml
backend/OPCBS.Web/Pages/Shared/_Sidebar.cshtml
backend/OPCBS.Web/Pages/Shared/_Footer.cshtml
backend/OPCBS.Web/Constants/ApiRoutes.cs
backend/OPCBS.Web/Services/IServiceInterfaces.cs
backend/OPCBS.Web/Services/ServiceImplementations.cs
backend/OPCBS.Web/Services/ApiServiceBase.cs
backend/OPCBS.Web/DTOs/ApiResponseDto.cs
backend/OPCBS.Web/DTOs/AdminDtos.cs
backend/OPCBS.Web/DTOs/VerificationDtos.cs
backend/OPCBS.Web/DTOs/BlogDtos.cs
backend/OPCBS.Web/wwwroot/css/site.css
backend/OPCBS.Web/wwwroot/js/site.js
backend/OPCBS/Controllers/AdminSupportController.cs
backend/OPCBS/Controllers/BlogsReviewsController.cs
backend/OPCBS.Shared/Models/ApiResponse.cs
```

If you need to understand a response DTO, read only the related DTO/interface/service file. Do not scan all backend code.

## Goal

Build a usable, polished Customer Support UI for:

- Customer Support dashboard
- Doctor application / verification review queue
- Doctor application detail review
- Blog moderation queue
- Blog moderation detail / editor review

Use `docs/ui-template/ui-customersupport/*.html` as visual references. Adapt them into maintainable Razor Pages and shared components. Do not paste giant static template HTML into Razor pages.

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

Use `MindBridge` consistently in UI text. Do not mix template brands like Sanctuary.

## Required Pages

Complete these existing Razor Pages:

```text
backend/OPCBS.Web/Pages/CustomerSupport/Dashboard.cshtml
backend/OPCBS.Web/Pages/CustomerSupport/Dashboard.cshtml.cs

backend/OPCBS.Web/Pages/CustomerSupport/DoctorApplications/Index.cshtml
backend/OPCBS.Web/Pages/CustomerSupport/DoctorApplications/Index.cshtml.cs
backend/OPCBS.Web/Pages/CustomerSupport/DoctorApplications/Details.cshtml
backend/OPCBS.Web/Pages/CustomerSupport/DoctorApplications/Details.cshtml.cs

backend/OPCBS.Web/Pages/CustomerSupport/BlogModeration/Index.cshtml
backend/OPCBS.Web/Pages/CustomerSupport/BlogModeration/Index.cshtml.cs
backend/OPCBS.Web/Pages/CustomerSupport/BlogModeration/Details.cshtml
backend/OPCBS.Web/Pages/CustomerSupport/BlogModeration/Details.cshtml.cs
```

If a backend endpoint is missing, still create a polished page shell with an empty/error state and a TODO comment naming the required endpoint from `docs/11-api-design.md`.

## Required Customer Support API Client

Update the existing client instead of creating a competing abstraction:

```text
backend/OPCBS.Web/Services/IServiceInterfaces.cs
backend/OPCBS.Web/Services/ServiceImplementations.cs
backend/OPCBS.Web/Constants/ApiRoutes.cs
backend/OPCBS.Web/DTOs/VerificationDtos.cs
backend/OPCBS.Web/DTOs/BlogDtos.cs
backend/OPCBS.Web/DTOs/AdminDtos.cs
backend/OPCBS.Web/DTOs/ApiResponseDto.cs
```

`ICustomerSupportApiService` and `CustomerSupportApiService` already exist. Keep PageModels dependent on that typed service.

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

Do not parse Customer Support endpoints as raw arrays unless the actual backend returns raw arrays. Prefer the existing `ApiResponseDto<T>` / pagination pattern used by other web API services.

## Expected API Endpoints

Use current backend routes when implemented:

```text
GET /api/v1/customer-support/dashboard
GET /api/v1/customer-support/pending-doctors?page=1&pageSize=10
GET /api/v1/customer-support/pending-blogs?page=1&pageSize=10

GET /api/v1/verifications/pending?page=1&pageSize=10
GET /api/v1/verifications/{requestId}
PUT /api/v1/verifications/approve
PUT /api/v1/verifications/reject

GET /api/v1/blogs/pending?page=1&pageSize=10
GET /api/v1/blogs/{blogId}
PUT /api/v1/blogs/approve/{blogId}
PUT /api/v1/blogs/reject/{blogId}
```

Check current backend route implementations before calling them. Current web service route helpers may be stale. In particular, do not assume these unsupported routes exist unless you add a tiny compatibility route:

```text
PUT /api/v1/customer-support/pending-doctors/{id}/review
GET /api/v1/customer-support/pending-doctors/{id}
GET /api/v1/customer-support/pending-blogs/{id}
PUT /api/v1/customer-support/pending-blogs/{id}/approve
PUT /api/v1/customer-support/pending-blogs/{id}/reject
```

Prefer aligning `CustomerSupportApiService` with existing backend routes. Only add backend compatibility endpoints when the change is small, low risk, and improves consistency.

## UI Requirements

### Layout

Customer Support pages should use a professional operational dashboard layout:

- Customer Support sidebar navigation
- Top bar with title, breadcrumb, search, notifications/profile area
- Content area with cards, queues, forms, and review actions
- Responsive behavior for tablet/mobile

Update shared sidebar/header only enough to support Customer Support navigation. Do not redesign every role's navigation.

### Dashboard

Use `CustomerSupportDashboard.html` as visual reference. Implement:

- Pending doctor applications stat
- Pending blog moderation stat
- Approved blogs / reviewed applications stat when API provides it
- Recent doctor application queue
- Recent blog moderation queue
- Operational/status panel
- Empty/error states for missing data

If the dashboard API returns generic admin stats only, show the supported metrics and clear placeholders for Customer Support-specific metrics.

### Doctor Applications Index

Implement:

- Search/filter form
- Status filter, defaulting to pending
- Paginated table or queue cards
- Status badges
- Submitted date
- Doctor name, license number, specialization, experience
- Detail/review link
- Empty state
- Error state

Do not approve/reject directly from the index unless the UI includes a confirmation modal and the PageModel preserves API errors.

### Doctor Application Details

Use `DoctorApplicationsDetail.html` as visual reference. Implement:

- Back to application list
- Doctor profile summary
- License number, specialization, experience, education, notes
- Certificate/document preview or download link when URL exists
- Submitted/reviewed status information
- Approve action
- Reject action with required reason textarea/modal
- Success/error TempData messages

Use the backend request body shape currently required by the controller:

```json
{ "requestId": "..." }
```

for approve, and:

```json
{ "requestId": "...", "reason": "..." }
```

for reject.

### Blog Moderation Index

Use `BlogModerationQueue.html` as visual reference. Implement:

- Pending blog queue
- Search/filter form if supported by API; otherwise keep the UI field local and add a TODO for backend filtering
- Title, summary, author, category, created/submitted date, status
- Priority/moderation badges only when backed by data; otherwise avoid fake flags
- Detail/review link
- Empty/error states
- Pagination

### Blog Moderation Details

Use `BlogDetail&Editors.html` as visual reference, but keep functionality aligned with backend support. Implement:

- Blog title, summary, content preview, category, tags, author, image
- Moderation sidebar with current status and timestamps
- Approve action
- Reject action with optional reason
- Back to moderation queue
- Disabled editor controls or TODO comments if the backend does not support Customer Support editing

Do not implement unsupported destructive actions like delete/trash unless the backend endpoint exists and authorization allows Customer Support.

## Components To Add Or Reuse

Prefer partials under:

```text
backend/OPCBS.Web/Pages/Shared
```

Suggested partials:

```text
_CustomerSupportSidebar.cshtml
_StatusBadge.cshtml
_Pagination.cshtml
_EmptyState.cshtml
_ErrorState.cshtml
_ConfirmDialog.cshtml
_ToastNotification.cshtml
_DashboardStatsCard.cshtml
```

Keep components simple and reusable. Reuse existing shared partials when they already fit.

## Authorization

Customer Support Razor Pages should require CustomerSupport access.

If the project does not yet have robust Razor Page authorization policies, add `[Authorize(Roles = "CustomerSupport,SystemAdmin")]` to the Customer Support PageModels or configure folder authorization in `Program.cs`.

If role claims are not reliable yet, add TODO comments, but still structure the pages as protected.

## Coding Rules

- PageModels call `ICustomerSupportApiService`.
- `.cshtml` files contain display logic only.
- Do not put business rules in Razor.
- Do not duplicate backend validation logic except basic HTML required fields.
- Keep files readable.
- Use Bootstrap 5 and existing project CSS conventions.
- Avoid adding Tailwind, Material Symbols, or remote template dependencies to the Razor implementation unless the project already uses them.
- Avoid giant copied HTML from template.
- Preserve API errors and display them to the user.
- Use `TempData` for success/error messages after POST actions.
- Use POST handlers such as `OnPostApproveAsync` and `OnPostRejectAsync` for review actions.
- Validate reject reason client-side and server-side in the PageModel before calling the API.

## Verification

After implementation, run:

```powershell
dotnet build backend/OPCBS.sln
```

If build fails due sandbox/NuGet permissions in the AI environment, report that clearly and still fix compile errors visible from the output.

Do not claim completion until:

- All Customer Support Razor Page files are implemented.
- `ICustomerSupportApiService` and `CustomerSupportApiService` call real current endpoints or clearly document endpoint gaps.
- Customer Support routes are linked from sidebar/navigation.
- Doctor application list/detail can read pending verifications and approve/reject them, or shows clear TODO for endpoint gaps.
- Blog moderation list/detail can read pending blogs and approve/reject them, or shows clear TODO for endpoint gaps.
- Build succeeds or remaining blocker is explicitly environmental.

## Progress File

At the end, create or update:

```text
docs/PROGRESS-FRONTEND-CUSTOMERSUPPORT.md
```

Include:

- Pages completed
- API clients completed
- Files changed
- Backend endpoints used
- Backend endpoint gaps
- Build result
- Next task recommendation
