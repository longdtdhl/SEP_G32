# OPCBS Appointment Lifecycle Progress Checklist

## 1. Three Consecutive Patient Absences
- [x] Analyze current automatic and manual NoShow flows.
- [x] Extract shared no-show consequence handler (`ProcessNoShowConsequencesAsync`).
- [x] Calculate consecutive absences from latest attendance outcomes (`Completed` and `NoShow` only, ordered by slot date/time desc).
- [x] Ensure `Completed` resets the streak.
- [x] When streak == 3:
  - [x] Send in-app warning notification to registered patients with stable key (`AttendanceWarningStreak3`).
  - [x] Send warning email to registered patient or guest (`GuestEmail`).
  - [x] Retain account active (no auto-lock).
  - [x] Sync `AppointmentHistory` and `TreatmentSession` status.
  - [x] Integrate with `ViolationReportService.CreateSystemNoShowReportAsync` (no duplicate reports).
- [x] Call shared handler from both `DoctorAppointmentServices.MarkPatientNoShowAsync` and `AppointmentReminderService.MarkOverdueApprovedAppointmentsAbsentAsync`.

## 2. Pending Doctor Confirmation Deadline
- [x] Ensure Pending deadline is calculated as `min(CreatedAt + 24h, ScheduledStart)`.
- [x] Remove any legacy appointment-date minus 24 hours calculations.
- [x] Expose in API and Web DTOs:
  - `DoctorResponseDeadlineUtc`
  - `RemainingResponseMinutes`
  - `IsResponseOverdue`
  - `IsResponseUrgent`
- [x] Add Pending reminder modal on Doctor Appointment List (`Doctor/Appointments/Index`):
  - [x] Display pending count, oldest waiting duration, nearest deadline, up to 3 urgent requests.
  - [x] Primary action: "Review Pending Requests", secondary: "Not Now".
  - [x] Server-driven calculations, accessible and responsive UI.

## 3. Minimum Consultation Duration (2/3 Rule) Before Completion
- [x] Authoritative backend enforcement in `DoctorAppointmentServices.CompleteAppointmentAsync`:
  - `minimumCompletionAt = actualStartedAt + ceil(scheduledDuration * 2 / 3)`
  - Return clear English error if completion attempted early.
- [x] Expose in DTOs:
  - `CanComplete`
  - `EarliestCompletionAtUtc`
  - `RemainingMinutesBeforeCompletion`
- [x] Update `Doctor/Appointments/Details`:
  - [x] Disable completion action when `CanComplete` is false.
  - [x] Render 2/3 progress component with elapsed, minimum required, and remaining minutes.
  - [x] Allow saving consultation notes before 2/3 duration without data loss.

## 4. Successful Completion Experience
- [x] Show celebration modal on redirected Details page after successful completion.
- [x] Display patient name, completed time, and treatment progress summary.
- [x] Primary action: "View Consultation Record", Secondary action: "Back to Appointments".
- [x] Subtle celebration styling, lightweight CSS confetti, respects `prefers-reduced-motion`.

## 5. Verification & Tests
- [x] Clean unit test suite in `OPCBS.Tests`.
- [x] Build verification for all backend projects (`OPCBS.Domain`, `OPCBS.Shared`, `OPCBS.Application`, `OPCBS.Infrastructure`, `OPCBS.Api`, `OPCBS.Web`, `OPCBS.Tests`).
