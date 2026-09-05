# OPCBS Premium UI Polish Handoff Prompt

Use this file when an AI coding session loses context or runs out of credit. Continue from here without reading the entire repository.

## Objective

Polish the following OPCBS screens to the same premium quality level as `Doctor/Appointments/Details`, without breaking existing logic:

1. `backend/OPCBS.Web/Pages/Doctor/TreatmentCases/Details.cshtml`
2. `backend/OPCBS.Web/Pages/Doctor/Patients/Details.cshtml`
3. `backend/OPCBS.Web/Pages/Appointment/Book.cshtml`
4. `backend/OPCBS.Web/Pages/Doctors/Details.cshtml`

## Reference Quality

Use `backend/OPCBS.Web/Pages/Doctor/Appointments/Details.cshtml` as the current benchmark.

Expected visual direction:

- Calm clinical UI.
- OPCBS green/teal identity.
- White, mint, soft green, slate text.
- Soft shadows, subtle gradients, clean borders.
- Compact typography.
- Clear label/value hierarchy.
- Accessible status badges with text, not color only.
- Polished primary/secondary/danger buttons.
- Responsive layout.
- Lightweight animation only.

## Global Rules

Do not rewrite business logic.

Do not change:

- API calls.
- Route handlers.
- `asp-page`, `asp-route`, `asp-page-handler`.
- Form `name`, `id`, hidden inputs, validation fields.
- DTO names or property names.
- Authorization logic.
- Database, migrations, seed data.
- Existing required JavaScript hooks.

Only improve Razor UI, CSS, and lightweight JS interactions.

Prefer CSS classes over new inline styles. If inline styles already exist, reduce them carefully only when safe.

Do not remove existing information fields unless the same information is visually duplicated.

Animations must be subtle:

- Hover lift.
- Soft fade-in.
- Soft shimmer only for active/live status.
- No heavy animation.
- No layout shift.
- No slow page load.

Run build after changes:

```powershell
dotnet build backend\OPCBS.sln --no-restore
```

Fix Razor/CSS/JS errors caused by your changes.

## Page-Specific Requirements

### 1. Doctor/TreatmentCases/Details

This page has many features but currently feels dense and inconsistent.

Improve:

- Treatment case header.
- Patient/package/status/start date/expected date presentation.
- Progress summary.
- Tabs: Overview, Sessions, Goals, Calendar, Homework, Mood, Timeline.
- Session cards.
- Goal cards.
- Homework cards.
- Timeline items.

Design goals:

- Make the current treatment journey easy to understand within 3-5 seconds.
- Important values should stand out: progress, current session, current goal, next session, homework, status.
- Keep treatment package snapshot and treatment case information readable.
- If modals already exist, make them cleaner and larger, but do not break handlers.
- Avoid adding more modal-heavy UX unless required by existing logic.

### 2. Doctor/Patients/Details

Improve:

- Header layout.
- Patient identity and contact details.
- Clinical overview.
- Treatment cases.
- Active goals.
- Mood/risk/absent information.
- Treatment package section.

Design goals:

- Doctor should quickly see who the patient is, current treatment state, risk indicators, and next action.
- Risk/absent indicators should be visually strong only when meaningful.
- Active items should be separate from history.
- Reduce dense blocks; use scannable cards and chips.

### 3. Appointment/Book

This is important for patient/guest booking.

Improve:

- Slot cards.
- Selected slot state.
- Online / in-person / both consultation format display.
- Confirm booking form.
- Package/session warning.
- Alerts and empty states.
- Mobile responsiveness.

Design goals:

- Booking should feel clean, trustworthy, and easy.
- Slot text must be readable and not overflow.
- Important warnings should be clear but not visually noisy.
- Keep all validation and submission logic unchanged.

### 4. Doctors/Details

This is a public-facing conversion page.

Improve:

- Doctor hero.
- Avatar/photo, name, title, verified badge.
- Specialization chips.
- Credentials.
- Consultation formats.
- Contact information.
- Reviews.
- Fee/package section.
- Book Appointment CTA.

Design goals:

- Make the page feel premium and trustworthy.
- Keep only one primary Book Appointment CTA visually dominant.
- Contact information should be clear and professional.
- Credentials and reviews should build confidence.
- Avoid huge empty space and oversized typography.

## Visual Style Guide

Primary color family:

- Deep teal: `#0f766e`
- OPCBS green/teal: `#0d9488`
- Soft mint: `#ecfdf5`, `#f0fdfa`, `#dff7f2`
- Slate text: `#0f172a`, `#334155`, `#64748b`

Avoid:

- Heavy purple/blue dominance.
- Harsh red except actual danger states.
- Giant headings inside cards.
- Too many equal-weight buttons.
- Flat unstyled links that should be buttons.
- Color-only status indicators.

Recommended status style:

- Pending: amber chip with text.
- Accepted/Approved: blue or green chip with text.
- In Progress: green/teal chip with subtle live animation.
- Completed: green chip with text.
- Cancelled/Rejected/Absent: red/rose chip with text.
- Expired: gray chip with text.

## Button Hierarchy

Primary:

- Main action only, such as Book Appointment, Start Session, Save, Generate Schedule.

Secondary:

- View details, reschedule, change slot, edit.

Danger / More:

- Cancel, report, reject.

Do not make all actions look equally important.

## Implementation Strategy

Work page by page.

For each page:

1. Inspect only the Razor page and directly related CSS/JS.
2. Identify existing handlers/forms/hooks before editing.
3. Add or refactor CSS classes safely.
4. Keep all existing data fields.
5. Improve layout and hierarchy.
6. Check mobile responsiveness.
7. Build.

Do not read the whole repo unless necessary.

## Final Report Required

After finishing, report:

- Files changed.
- UI improvements made per page.
- Any logic intentionally untouched.
- Build result.
- Remaining risks or screens that still need manual visual QA.
