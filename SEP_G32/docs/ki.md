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
