# Knowledge Items (KI) - MindBridge System

Tài liệu này ghi nhận các kiến thức thực tế (Knowledge Items), các lưu ý (gotchas) và các mẫu thiết kế kiến trúc (architectural patterns) trong dự án MindBridge để hỗ trợ các lập trình viên hoặc AI Agents phát triển dự án tiếp theo một cách nhất quán.

---

## 1. Tự động nhận diện & Khấu trừ phiên Gói điều trị (Treatment Packages Booking)

### Ngữ cảnh & Nghiệp vụ
Khi bệnh nhân đã mua một Gói điều trị tâm lý của Bác sĩ, mỗi khi họ đặt lịch hẹn mới với bác sĩ đó, số phiên còn lại (`RemainingSessions`) của gói phải được khấu trừ đi 1 buổi (`RemainingSessions--`), và lịch hẹn phải được liên kết trực tiếp với gói điều trị thông qua trường `TreatmentPackageId`.

### Thiết kế Kỹ thuật
* **Frontend (`Book.cshtml.cs`):** 
  * Khi bệnh nhân truy cập vào trang đặt lịch mà không truyền kèm `TreatmentPackageId` trên URL (ví dụ: bấm đặt lịch từ trang tìm kiếm chuyên gia hoặc thông tin bác sĩ), hệ thống ở Frontend sẽ tự động gọi API kiểm tra xem bệnh nhân có gói điều trị nào đang hoạt động (`Active`/`Accepted`) với bác sĩ được chọn hay không.
  * Nếu có, giao diện hiển thị thông báo thành công và ngầm điền thuộc tính ẩn `TreatmentPackageId` để gửi lên API.
* **Backend (`DoctorAppointmentServices.cs`):**
  * Tại phương thức `CreateAppointmentAsync`, nếu `dto.TreatmentPackageId` gửi lên bằng rỗng, Backend thực hiện cơ chế tìm kiếm dự phòng (Fallback): Quét toàn bộ danh sách gói khám đang hoạt động, chưa hết hạn, và có `RemainingSessions > 0` của bệnh nhân với bác sĩ đó.
  * Nếu phát hiện gói hợp lệ, hệ thống tự động gán lịch hẹn với gói này và thực hiện trừ số phiên còn lại.

---

## 2. Đồng bộ DTO & Ánh xạ thuộc tính (DTO Mapping Mismatches)

Trong dự án có sự lệch pha thuộc tính giữa dữ liệu JSON do Backend API trả về và cách đặt tên thuộc tính ở Web DTO Layer. Điều này khiến giao diện nhận giá trị rỗng/ngầm định (gây ra các lỗi như toàn bộ tài khoản luôn hiển thị là "Bị khóa" hoặc danh sách Audit log trống trơn).

### Sửa lỗi Cấu trúc Tài khoản (`AdminDtos.cs` -> `UserListItemDto`)
* **API Backend trả về:** `status` (chuỗi "Active"/"Locked"), `isEmailVerified` (bool)
* **Web DTO ban đầu:** `IsActive` (bool), `EmailConfirmed` (bool) -> Mất đồng bộ.
* **Giải pháp:** Ánh xạ đúng tên trường từ API và cung cấp các thuộc tính tính toán động:
  ```csharp
  public string Status { get; set; } = string.Empty;
  public bool IsEmailVerified { get; set; }
  public bool IsActive => Status == "Active" || Status == "0";
  public bool EmailConfirmed => IsEmailVerified;
  ```

### Sửa lỗi Nhật ký Hoạt động (`AdminDtos.cs` -> `AuditLogDto`)
* **API Backend trả về:** `createdAt`, `userEmail`, `entityName`, `actionDescription`
* **Web DTO ban đầu:** `Timestamp`, `UserName`, `EntityType`, `Details` -> Gây hiển thị trắng dòng.
* **Giải pháp:** Lưu trữ đúng trường API và ánh xạ thuộc tính phục vụ View:
  ```csharp
  public string? UserEmail { get; set; }
  public string EntityName { get; set; } = string.Empty;
  public string? ActionDescription { get; set; }
  public DateTime CreatedAt { get; set; }
  
  public string? UserName => UserEmail;
  public string? EntityType => EntityName;
  public string? Details => ActionDescription;
  public DateTime Timestamp => CreatedAt;
  ```

---

## 3. Bảo toàn Tham số Bộ lọc khi Phân trang (`_Pagination.cshtml`)

### Vấn đề
Khi thực hiện phân trang trên danh sách có bộ lọc (như tìm kiếm bác sĩ, lọc trạng thái lịch hẹn...), việc nhấp vào trang tiếp theo (ví dụ trang 2) sẽ reset toàn bộ tham số lọc về mặc định nếu đường dẫn phân trang chỉ dạng `?page=2`.

### Giải pháp
Nâng cấp tệp partial view dùng chung `_Pagination.cshtml` để tự động quét toàn bộ query string hiện tại của HTTP Request (trừ trường `page`) và đính kèm vào đường dẫn của các trang:
```csharp
@{
    var queryParams = new List<string>();
    foreach (var key in ViewContext.HttpContext.Request.Query.Keys)
    {
        if (key != "page")
        {
            var value = ViewContext.HttpContext.Request.Query[key];
            queryParams.Add($"{key}={System.Net.WebUtility.UrlEncode(value)}");
        }
    }
    var querySuffix = queryParams.Any() ? "&" + string.Join("&", queryParams) : "";
}
```

---

## 4. Định tuyến API Chuyên khoa (`Specializations`)

### Lưu ý về Định tuyến (Route Configuration Gotchas)
Không được sử dụng route `api/v1/specializations` để đọc hoặc thay đổi chuyên khoa vì Backend không đăng ký route này tại gốc (sẽ gây lỗi 404). Các API được thiết kế như sau:
* **Xem toàn bộ chuyên khoa:** `GET api/v1/doctors/specializations` (ủy quyền cho `DoctorsController`).
* **Quản trị chuyên khoa (Thêm/Sửa/Xóa):** `api/v1/business-manager/specializations` (ủy quyền cho `BusinessManagerController`).

---

## 5. Persistence Cấu hình Hệ thống (System Settings)

* Các cấu hình hệ thống (như SMTP Server, Timeout phiên, Số lần đăng nhập tối đa, Chế độ bảo trì...) được lưu trữ động tại bảng dữ liệu `SystemConfigs` theo cấu trúc Key-Value thông qua entity `SystemConfig`.
* Khi gọi các phương thức tương tác với bảng này trong `AdminService`, cần tiêm trực tiếp `IRepository<SystemConfig>` thông qua DI thay vì gọi qua `_uow.GetRepository<SystemConfig>()` để đảm bảo tính tương thích và nhất quán của Generic Repository pattern trong dự án.

---

## 6. Tính năng Trị liệu Chuyên sâu (Therapy Features)

### Bài tập Trị liệu (`TherapyAssignment`)
* **Entity:** `OPCBS.Domain/Entities/TherapyAssignment.cs` - Liên kết với `TreatmentPackage` qua `TreatmentPackageId`.
* **Trạng thái:** `Status` = 0 (Chưa làm), 1 (Đã nộp bài), 2 (Bác sĩ đã nhận xét).
* **Luồng hoạt động:** Bác sĩ tạo → Bệnh nhân nộp bài → Bác sĩ nhận xét.
* **API Route:** `api/v1/therapy/assignments/*` (TherapyController).

### Nhật ký Cảm xúc (`EmotionJournal`)
* **Entity:** `OPCBS.Domain/Entities/EmotionJournal.cs` - Liên kết với `PatientProfile` qua `PatientId`.
* **Thang đo:** `MoodScale` (1-5: Rất tệ → Rất tốt), `StressScale` (1-5: Rất thấp → Rất cao).
* **Chia sẻ:** Thuộc tính `IsShared` cho phép bệnh nhân tùy chọn chia sẻ nhật ký với bác sĩ.
* **API Route:** `api/v1/therapy/journals/*` (TherapyController).

### Lưu ý Kỹ thuật
* **DI Pattern:** Inject `IRepository<T>` trực tiếp (không dùng `_uow.GetRepository<T>()`).
* **Web Client:** `ITherapyApiService` / `TherapyApiService` gộp cả Assignments + Journals.
* **Chart.js:** Biểu đồ xu hướng tâm trạng dùng Chart.js CDN `https://cdn.jsdelivr.net/npm/chart.js`.

### Liên kết Gói điều trị với Hồ sơ & Ghi chú tư vấn của Bác sĩ
* **Hồ sơ bệnh nhân (`Doctor/Patients/Details`):** Tự động quét và hiển thị toàn bộ gói điều trị của bệnh nhân này với bác sĩ phụ trách. Đi kèm thanh tiến trình và nút điều hướng nhanh tới trang quản lý bài tập trị liệu.
* **Ghi chú tư vấn (`Doctor/ConsultationNotes/Details`):** 
  1. Nếu phiên tư vấn gắn liền với lịch hẹn (`AppointmentId`), hệ thống sẽ tìm kiếm gói điều trị được liên kết trực tiếp với lịch hẹn đó để hiển thị.
  2. Dự phòng (Fallback): Nếu không có gói điều trị trực tiếp, hệ thống tự động hiển thị danh sách các gói điều trị khác của bệnh nhân này với bác sĩ để bác sĩ dễ dàng truy cập và giao bài tập.

---

## 7. Thống nhất Ngôn ngữ Tiếng Anh cho Toàn bộ Hệ thống (English Localization Alignment)

Để đáp ứng trải nghiệm người dùng quốc tế, toàn bộ giao diện và các trường thông tin của MindBridge đã được chuyển đổi đồng bộ sang tiếng Anh chuẩn hóa:
* **Hệ thống Điều hướng & Thanh Menu (`_Header.cshtml`, `_Sidebar.cshtml`, `_Footer.cshtml`):**
  * Đưa tất cả các mục điều hướng chính, tiêu đề cổng thông tin, danh mục chân trang và menu cá nhân về tiếng Anh.
  * Các menu bên (Sidebar) dành cho Bác sĩ và Bệnh nhân được dịch toàn bộ sang tiếng Anh (ví dụ: `Bảng điều khiển` -> `Dashboard`, `Lịch hẹn` -> `Appointments`, `Nhật ký cảm xúc` -> `Mood Journal`, v.v.).
* **Trang chủ (`Index.cshtml`):** Các đề mục Hero, Trust Stats, featured therapists, How it works, Stories of healing, tài nguyên và CTA được đưa lại về tiếng Anh.
* **Màn hình xác thực & Tài khoản (`Pages/Account/*`):**
  * `Register.cshtml`, `Login.cshtml`, `RegisterDoctor.cshtml` được dịch hoàn toàn sang tiếng Anh.
  * Các trang khôi phục tài khoản, OTP và thay đổi thông tin cá nhân (`Profile.cshtml`, `ChangePassword.cshtml`, `ForgotPassword.cshtml`, `ResetPassword.cshtml`, `VerifyOtp.cshtml`) hiển thị hoàn toàn bằng tiếng Anh.
* **Trang điều trị & chuyên sâu của Bệnh nhân & Bác sĩ:** Các trang chi tiết gói trị liệu (`Details.cshtml`), Nhật ký cảm xúc (`Journal/Index.cshtml`) và Sàng lọc tâm lý (`Psychometrics/TakeTest.cshtml`) được chuyển ngữ toàn bộ sang tiếng Anh.

### 5.3 Full English Localization (Comprehensive Pass)

**Objective:** Convert ALL remaining Vietnamese text across the entire website to English. The user explicitly requested zero Vietnamese text visible on the UI.

**Approach:**
* Built a custom C# console translation tool (`TranslateApp`) that runs 6+ replacement passes over all `.cshtml` files.
* Each pass targeted: dictionary-based phrase translation → mixed Vietnamese/English cleanup → single-word Vietnamese cleanup.

**Files Updated:** 94 `.cshtml` files across all modules (Doctor, Patient, Admin, Blog, Appointment, CustomerSupport, etc.).

**Key Areas Translated:**
* All page titles, headers, labels, buttons, placeholders, tooltips, error messages, and empty state text
* CSS comments containing Vietnamese descriptions
* JavaScript strings (e.g. share buttons, clipboard copy feedback, EasyMDE placeholders)
* Appointment statuses, blog moderation workflows, verification flows
* Clinical consultation forms (diagnoses, therapy plans, follow-up notes)
* Treatment package management and subscription UIs

**Known Issue - Aggressive Word Replacement:**
Short Vietnamese words like "cho", "lan", "và" can corrupt C# property names when used in `.Replace()`. Examples encountered:
* `PsychologicalHistory` → `PsyforlogicalHistory` (from "cho" → "for")
* `PsychometricSubmission` → `PsyformetricSubmission`
* `TherapyPlan` → `TherapyPspread` (from "lan" → "spread")
* `Specialization` → `Sspreadization`

**Mitigation:** Post-translation fixup script scans all `.cshtml` files for corrupted identifiers and restores them. Always run `dotnet build` after translation passes to catch any namespace/property corruption.

**Remaining Work:** ~228 lines across 34 files still contain Vietnamese characters, mostly in deeply embedded mixed English/Vietnamese strings that require manual per-file editing for complete cleanup.

---

## 8. Doctor Appointment List UI Redesign (Phase 1)

### Design Architecture
The doctor's appointment list page (`Pages/Doctor/Appointments/Index.cshtml`) was redesigned from a plain table to a card-based layout with the following components:

* **Status Summary Cards:** Four clickable summary cards at the top showing counts for Pending, Approved, Completed, and Cancelled appointments. Clicking a card filters by that status.
* **Filter Section:** Enhanced filters including Status dropdown, Patient name search, Date From/To range pickers, with a clear-all reset button.
* **Card-Based Appointment List:** Appointments are grouped by date with each appointment rendered as a horizontal card showing:
  - Patient avatar (initials-based, gradient background)
  - Patient name + booking code
  - Time badge (start — end)
  - Status badge with color coding
  - Fee or Package indicator
  - Quick-action buttons (Approve/Complete/Cancel) as compact icon buttons
* **Empty State:** Custom empty state with illustration when no appointments match filters.

### Technical Details
* **Code-behind (`Index.cshtml.cs`):** Makes two API calls — one for filtered paginated data and one unfiltered (pageSize=9999) to compute status counts. Uses `AppointmentFilterDto` with `FromDate` and `ToDate` for date range filtering.
* **DTO Enhancement:** `AppointmentListItemDto` (both Web and Application layers) now includes `EndTime` property alongside `StartTime`, populated by `EnrichAppointmentListDtosAsync` from `slot.EndTime.ToString("HH:mm")`.
* **Razor Syntax Note:** Within an `@if` block, declare variables directly (`var grouped = ...;`) — do NOT wrap them in `@{ }` blocks as this causes Razor error `RZ1010`.

---

## 9. Consultation Note Completion Guard (Phase 2)

### Business Rule
Doctors MUST create a consultation note for an appointment before they can mark it as "Completed". This prevents incomplete clinical records.

### Implementation
* **Backend Guard (`DoctorAppointmentServices.cs` → `CompleteAppointmentAsync`):** Queries `IRepository<ConsultationNote>` before allowing status transition. If no non-deleted note exists for the appointment, returns error: "Please create a consultation note before completing this appointment."
* **DI Change:** `AppointmentService` now requires `IRepository<ConsultationNote>` as a constructor parameter. Any code instantiating `AppointmentService` (including unit tests) must pass this dependency.
* **Frontend Guard (`Details.cshtml`):** When status is Approved:
  - If `HasConsultationNote` is true → green success alert + enabled "Mark as Completed" button
  - If false → yellow warning alert + disabled button with tooltip
  - Always shows appropriate CTA: "View/Edit Consultation Note" or "Create Consultation Note" or "Create Patient Record"
* **Test Impact:** `CompleteAppointment_Success` test must set up `_consultationNoteRepoMock` to return a note for the appointment. Use `null!` for `required` navigation properties (`Doctor`, `PatientRecord`) that aren't exercised in test logic.

---

## 10. Email Service Expansion Pattern

### Architecture
* **Interface:** `IEmailService` (in `OPCBS.Application/Interfaces/Services/ExternalServices.cs`) defines typed email methods instead of requiring callers to build HTML.
* **Implementation:** `SmtpEmailService` (in `OPCBS.Infrastructure/Services/SmtpEmailService.cs`) uses a shared `BuildEmailTemplate(headerTitle, headerSubtitle, headerGradient, bodyHtml)` helper that produces branded, responsive HTML emails.
* **Mock:** `MockEmailService` (in `MockExternalServices.cs`) logs all email sends to console for development.

### Available Email Methods
| Method | Trigger |
|--------|---------|
| `SendOtpEmailAsync` | Registration verification |
| `SendPasswordResetEmailAsync` | Password reset request |
| `SendAppointmentConfirmedEmailAsync` | Doctor approves appointment |
| `SendAppointmentCancelledEmailAsync` | Either party cancels |
| `SendAppointmentCompletedEmailAsync` | Doctor completes appointment |
| `SendAppointmentReminderEmailAsync` | Background reminder service |
| `SendConsultationNoteEmailAsync` | Doctor creates consultation note |

### Adding New Email Types
1. Add method signature to `IEmailService` interface
2. Implement in `SmtpEmailService` using `BuildEmailTemplate()` helper
3. Add no-op mock in `MockEmailService`
4. Call from the relevant service method, wrapped in `try/catch`

---

## 11. Appointment Cancellation Flow (Phase 3)

### Business Rules
* **24-Hour Policy:** Both patients and doctors must cancel at least 24 hours before the scheduled appointment time. The backend enforces this in `CancelAppointmentAsync`.
* **Slot Release:** Upon cancellation, the appointment slot is set back to `Available`, allowing the time to be rebooked.
* **Treatment Package Restore:** If the appointment was linked to a treatment package, `RemainingSessions` is incremented back (capped at `SessionQuantity`).

### UI Design
* **Doctor Side** (`Pages/Doctor/Appointments/Details.cshtml`): Cancel button triggers a Bootstrap modal with:
  - Confirmation message showing patient name and date
  - Category dropdown (Schedule Conflict, Emergency, Patient Request, Unavailable, Other)
  - Free-text textarea for additional details
  - 24h policy info alert
* **Patient Side** (`Pages/Patient/Appointments/Details.cshtml`): Similar modal with patient-oriented categories (Schedule Conflict, Feeling Better, Financial Reasons, Found Another Doctor, Personal Emergency, Other).

### Notification Flow
After cancellation, the system notifies the **other party** (if patient cancels → doctor gets notified, and vice versa) via both:
1. In-app notification (`INotificationService`)
2. Email notification (`IEmailService.SendAppointmentCancelledEmailAsync`)

---

## 12. Email Integration into Appointment Lifecycle (Phase 4)

### DI Change
`AppointmentService` now requires `IEmailService` as a constructor parameter (after `INotificationService`, before `IUnitOfWork`). Unit tests must mock this with `Mock<IEmailService>`.

### Lifecycle Emails Wired
| Event | Method | Recipient |
|-------|--------|-----------|
| Appointment Approved | `ApproveAppointmentAsync` → `SendAppointmentConfirmedEmailAsync` | Patient |
| Appointment Cancelled | `CancelAppointmentAsync` → `SendAppointmentCancelledEmailAsync` | Other party |
| Appointment Completed | `CompleteAppointmentAsync` → `SendAppointmentCompletedEmailAsync` | Patient |

### Pattern
All email calls are placed inside existing `try { ... } catch { }` blocks alongside notification calls. They are fire-and-forget — a failed email will not roll back the appointment state transition.

---

## 13. VNPay Service Package Payment Gateway (Phase 5)

### API Layer
* `VnPayService.cs` implements `IPaymentService` using the standard VNPay 2.1.0 specification.
* Generates a request URL containing query parameters sorted alphabetically and signed with HMAC-SHA512 using the merchant hash secret.
* Signature verification is executed during callbacks and IPN calls by stripping the incoming secure hash and calculating HMAC-SHA512 over the alphabetically sorted remaining params.

### Web / Razor Flow
* When a doctor registers or renews a service package on `Doctor/ServicePackages/Index.cshtml`, `PurchaseAsync` is called with the package ID and return URL.
* The API generates the VNPay URL, and the Razor Page redirects the user to the VNPay Sandbox page.
* Upon payment completion, VNPay redirects the user back to `/Doctor/Subscriptions/PaymentCallback` which queries the API `ProcessCallbackAsync` to verify signatures, deactivate any older subscriptions (Expired), activate the new subscription (Active), and update `PaymentTransaction` to `Success`.

---

## 14. In-Progress Session & Inline Consultation Note Modal (Phase 2 Upgrade)

### Appointment State Machine
* **InProgress (3)**: Transition action `StartAppointmentAsync` allows a doctor to mark an Approved appointment as "In Progress" when starting consultation.
* **Completion Guard**: If the session has no Consultation Note, completion is blocked. Instead of redirecting the user to a separate note creation page, the details page triggers a Bootstrap modal (`#createNoteModal`).
* **Modal Submit & Complete**: The form in the modal allows the doctor to record diagnostic findings, chief complaints, and session notes, then submits to `OnPostCreateNoteAndCompleteAsync` which creates the note record and completes the appointment in a single request.

---

## 15. Follow-up Appointment Workflow & Doctor Detail Enrichment

### Ngữ cảnh & Nghiệp vụ
Hệ thống cần hỗ trợ tái khám: khi bác sĩ ghi nhận ngày tái khám trong Consultation Note (`NextAppointmentRecommendedDate`), bệnh nhân sẽ nhận email nhắc nhở 1 ngày trước ngày hẹn. Ngoài ra, bệnh nhân đã từng khám bác sĩ (returning patient) không cần điền lại thông tin Pre-Appointment Evaluation, và có thêm nút "Book Follow-up" trên trang chi tiết lịch hẹn đã hoàn thành.

### Returning Patient Detection
* **Backend:** `GetVisitCountAsync(patientUserId, doctorProfileId)` đếm số lượng appointment có `Status = Completed` giữa patient và doctor. `IsReturningPatientAsync` trả về `true` nếu `VisitCount > 0`.
* **API Endpoints:** `GET /api/v1/appointments/visit-count/{doctorId}` và `GET /api/v1/appointments/is-returning/{doctorId}` (Patient role).
* **Web Frontend:**
  * Patient `Details.cshtml` hiển thị badge "Visit #N" cạnh specialization chip và nút "Book Follow-up" (chỉ khi `IsReturningPatient = true` và status = Completed).
  * Nút "Book Follow-up" redirect đến `/Appointment/Book?doctorId=X&returning=true`.

### Skip Pre-Evaluation cho Returning Patient
* `Book.cshtml.cs` nhận query parameter `Returning=true` hoặc gọi `IsReturningAsync` để set `IsReturningPatient`.
* `Book.cshtml` ẩn section Pre-Appointment Evaluation (Symptoms, MedicalHistory, Expectations) và hiển thị banner "Welcome back!" thay thế.

### Follow-up Reminder (Background Service)
* **Trigger:** `AppointmentReminderService.CheckAndSendFollowUpRemindersAsync` chạy mỗi 5 phút, quét `ConsultationNote` có `NextAppointmentRecommendedDate.Date = tomorrow (UTC)`.
* **Actions:** Gửi in-app notification + email (`SendFollowUpReminderEmailAsync`) cho patient.
* **Deduplication:** Sử dụng `RelatedEntityType = "FollowUpReminder"` + `RelatedEntityId = note.Id` trong bảng Notification để tránh gửi trùng.

### Doctor Appointment Detail Enrichment
* `EnrichAppointmentDtoAsync` populate thêm:
  * `Fee` — từ `AppointmentSlot.Price`
  * `TreatmentPackageName` — từ `TreatmentPackage.Name` (nếu có `TreatmentPackageId`)
  * `VisitCount` — đếm completed appointments cùng patient + doctor
* Doctor `Details.cshtml` hiển thị:
  * Treatment Package badge (nếu có)
  * Visit Count badge ("First consultation" hoặc "Visit #N")

### Các file liên quan
* `IAppointmentService.cs`, `DoctorAppointmentServices.cs`, `AppointmentsController.cs` (Backend)
* `ExternalServices.cs`, `SmtpEmailService.cs`, `MockExternalServices.cs` (Email)
* `AppointmentReminderService.cs` (Background Job)
* `AppointmentDtos.cs` (Backend + Web), `IAppointmentApiService.cs`, `AppointmentApiService.cs` (Web)
* Patient `Details.cshtml[.cs]`, `Book.cshtml[.cs]`, Doctor `Details.cshtml` (Razor Pages)

---

## 16. Doctor Booking Flow Improvements (6 fixes)

### 16.1 Trường ngày tái khám trong Consultation Note Modal
* Modal `#createNoteModal` trong Doctor `Details.cshtml` đã thêm input `date` cho `NoteInput.NextAppointmentRecommendedDate`.
* DTO `CreateConsultationNoteDto` đã có sẵn field này — chỉ cần render input trên UI.

### 16.2 Loại bỏ phần giá (Fee)
* OPCBS không quản lý giá — dòng "Consultation Fee" đã bị xóa khỏi Appointment Information section trong Doctor `Details.cshtml`.

### 16.3 Thông tin đầy đủ cho bệnh nhân tái khám
* Doctor `Details.cshtml.cs` thêm 2 properties:
  * `LatestConsultationNote` — ghi chú gần nhất từ lần khám trước (lọc bỏ buổi hiện tại), truy vấn qua `GetByPatientRecordIdAsync`.
  * `ActiveTreatmentPackage` — gói điều trị đang active (Status = Active/Accepted, chưa hết hạn, còn sessions), truy vấn qua `GetMyPackagesAsync` và filter theo `PatientId`.
* UI hiển thị:
  * "Previous Consultation (Latest)" section — diagnosis, session notes, recommendations, therapy plan.
  * "Active Package" row — tên gói, sessions completed/total, remaining, ngày hết hạn.

### 16.4 Appointment List ẩn Cancelled
* `Index.cshtml.cs`: Khi không có filter, lọc bỏ status Cancelled (5) và Rejected (2) khỏi danh sách.
* Stat card "Cancelled" đổi thành "In Progress" (status 3), `CancelledCount` → `InProgressCount`.
* `ApprovedCount` tách riêng, không gộp InProgress nữa.

### 16.5 Nút Create Package ↔ View Package Details
* Nếu `ActiveTreatmentPackage != null` → hiển thị "View Package Details" link đến `/Doctor/TreatmentPackages/Edit/{id}` + badge remaining sessions.
* Nếu không → giữ nút "Create Treatment Package" như cũ.

### 16.6 Badge gói khám trên đầu buổi hẹn
* Khi appointment có `TreatmentPackageId` + `TreatmentPackageName`, hiển thị banner xanh lá phía trên Status Banner: "Treatment Package Appointment — This appointment is part of package: [Name]".

### Các file liên quan
* Doctor `Details.cshtml.cs`, Doctor `Details.cshtml` (Appointment Details)
* Doctor `Index.cshtml.cs`, Doctor `Index.cshtml` (Appointment List)

---

## 17. Patient Booking Flow Improvements (3 fixes + bonus)

### 17.1 Block booking outside treatment package
* `Book.cshtml.cs`: Thêm `HasPackageButNotBookingVia` flag. Khi auto-detect active package nhưng patient không đến qua URL có `treatmentPackageId`, set flag = true và KHÔNG tự gán `TreatmentPackageId`.
* `Book.cshtml`: Khi `HasPackageButNotBookingVia = true`, hiển thị warning banner vàng "You have an active treatment package" với 2 nút "Book via Package" và "View Package Details". Calendar bị ẩn hoàn toàn — patient phải đặt qua gói.

### 17.2 Persist test results + Retake button
* `Patient/Appointments/Details.cshtml`: Khi `PsychometricSubmission != null`, hiển thị kết quả test + thêm nút "Retake: [Test Title]" (chỉ khi status Pending/Approved). Nút retake link đến `/Patient/Psychometrics/TakeTest` với appointmentId.

### 17.3 Patient Appointment List filter redesign
* **Bug fix quan trọng**: Filter dropdown sử dụng sai status values (value="3" được label "Cancelled" nhưng 3 = InProgress). Sửa thành dùng enum names (Pending, Approved, InProgress, Completed, Cancelled, Rejected).
* **Redesign**: Thêm 4 stat cards (Pending, Confirmed, Completed, All) giống giao diện bác sĩ. Thêm `PendingCount`, `ApprovedCount`, `CompletedCount`, `TotalCount` vào code-behind.
* Sửa text mixed Vietnamese/English.

### 17.4 Bonus: Status mapping bug fix
* `Patient/Appointments/Details.cshtml`: `isCancelled` sai mapping Status == 3 (thực tế là InProgress). Sửa thành Status == 5. Thêm `isInProgress` = Status == 3.
* Xóa "Consultation Fee" khỏi patient details (consistent với doctor side — OPCBS không quản lý giá).

### Các file liên quan
* `Appointment/Book.cshtml[.cs]` (Booking page)
* Patient `Appointments/Details.cshtml` (Patient appointment details)
* Patient `Appointments/Index.cshtml[.cs]` (Patient appointment list)

---

## 18. Sidebar Removal, expanded Dropdown Menu, and Doctor Header Customization

### 18.1 Sidebar Removal
* `_Layout.cshtml`: Loại bỏ partial view `<partial name="_Sidebar" />`. Thay đổi container Dashboard từ `.mb-dashboard-content` sang `.container py-4` chuẩn. Việc này loại bỏ hoàn toàn sidebar trái trên tất cả các trang Portal/Dashboard, đưa layout về dạng căn giữa chuẩn và hiển thị cả footer.

### 18.2 Profile Dropdown Menu Upgrade
* `_Header.cshtml`: Nâng cấp dropdown menu góc phải (ảnh đại diện). Thiết lập `min-width: 265px` và `max-height: 85vh; overflow-y: auto;` để hiển thị danh sách dài một cách tối ưu.
* Đã chuyển toàn bộ liên kết chức năng của mọi vai trò từ sidebar cũ sang dropdown menu này:
  * **Patient**: Dashboard, My Appointments, Consultation Records, Treatment Packages, Mood Journal, Psychometric Tests, Notifications.
  * **Doctor**: Dashboard, Appointments, Schedules, Patient Records, Consultation Notes, Treatment Packages, Blog Posts, Service Packages, Subscription Status, Identity Verification, Profile. (Kiểm tra xác thực `isVerified` qua `IVerificationApiService` để khóa/mở các liên kết tương tự sidebar).
  * **Customer Support**: Dashboard, Doctor Applications, Blog Moderation.
  * **Business Manager**: Dashboard, Service Packages, Specializations, Analytics, Reports.
  * **System Admin**: Dashboard, Users, Roles, Permissions, Audit Logs, Reports, Settings.

### 18.3 Doctor Header Navigation Customization
* `_Header.cshtml`: Đối với menu trái (`navbar-nav me-auto`), nếu người dùng đăng nhập dưới vai trò Bác sĩ (`isDoctor`), chỉ hiển thị duy nhất liên kết **Resources** (`/Blog/Index`). Ẩn tất cả các liên kết Home, Find a Therapist, Track Appointment.

### Các file liên quan
* `_Layout.cshtml` (Main layout)
* `_Header.cshtml` (Main header & dropdown navigation)



