# Kế hoạch & Checklist Kiểm thử (Unit Test Plan & Checklist)

Tài liệu này chứa kế hoạch kiểm thử đơn vị (Unit Test) cho dự án OPCBS. Tập trung kiểm thử đầy đủ **7 hàm quan trọng bậc nhất** của phần nghiệp vụ, mỗi hàm được viết khoảng **20 test cases** để bao phủ toàn bộ các luồng (success, validation, edge cases, error).

---

## 7 Hàm nghiệp vụ quan trọng được lựa chọn (Mỗi hàm 20 cases)

### 1. `CreateAppointmentAsync` (Đăng ký lịch hẹn - `AppointmentService`)
Hàm này thực hiện kiểm tra khung giờ trống, trạng thái bác sĩ, trạng thái gói dịch vụ của bác sĩ, chống trùng lịch và tạo thông tin cuộc hẹn.
- [ ] **Case 1**: Đặt lịch thành công cho bệnh nhân đã đăng ký tài khoản (Patient).
- [ ] **Case 2**: Đặt lịch thành công cho khách vãng lai (Guest - cung cấp đủ thông tin tên, email, sđt).
- [ ] **Case 3**: Thất bại khi bác sĩ không tồn tại trong hệ thống.
- [ ] **Case 4**: Thất bại khi bác sĩ chưa được xác minh (status = Pending).
- [ ] **Case 5**: Thất bại khi bác sĩ bị từ chối xác minh (status = Rejected).
- [ ] **Case 6**: Thất bại khi bác sĩ không có thông tin gói đăng ký dịch vụ (Subscription).
- [ ] **Case 7**: Thất bại khi gói dịch vụ của bác sĩ đã hết hạn.
- [ ] **Case 8**: Thất bại khi trạng thái gói dịch vụ của bác sĩ không phải là Active.
- [ ] **Case 9**: Thất bại khi khung giờ hẹn (Slot) không tồn tại.
- [ ] **Case 10**: Thất bại khi khung giờ hẹn đã được đặt trước (status = Booked).
- [ ] **Case 11**: Thất bại khi khung giờ hẹn bị khóa (status = Unavailable).
- [ ] **Case 12**: Thất bại khi chọn khung giờ hẹn ở quá khứ.
- [ ] **Case 13**: Thất bại do trùng lịch (Bệnh nhân đã có lịch hẹn khác hoạt động ở cùng khung giờ).
- [ ] **Case 14**: Thất bại khi đặt lịch kiểu Guest nhưng thiếu GuestName.
- [ ] **Case 15**: Thất bại khi đặt lịch kiểu Guest nhưng thiếu GuestEmail.
- [ ] **Case 16**: Thất bại khi đặt lịch kiểu Guest nhưng thiếu GuestPhoneNumber.
- [ ] **Case 17**: Thất bại khi mã gói điều trị (TreatmentPackageId) được truyền vào nhưng không hợp lệ hoặc không tồn tại.
- [ ] **Case 18**: Kiểm tra trạng thái khung giờ tự động chuyển từ `Available` sang `Booked` khi đặt lịch thành công.
- [ ] **Case 19**: Kiểm tra trạng thái lịch hẹn mới tạo phải mặc định là `Pending`.
- [ ] **Case 20**: Kiểm tra mã đặt lịch (BookingCode) được sinh ra đúng định dạng `OPCBS-XXXXXX`.
- [ ] **Case 21**: Xác nhận dữ liệu được lưu xuống Repository và gọi SaveChanges.

### 2. `CancelAppointmentAsync` (Hủy lịch hẹn - `AppointmentService`)
Hàm này kiểm tra quy tắc hủy hẹn (chính sách trước 24 giờ), giải phóng khung giờ và cập nhật trạng thái.
- [ ] **Case 1**: Bệnh nhân hủy hẹn thành công trước giờ hẹn hơn 24 giờ.
- [ ] **Case 2**: Bác sĩ hủy hẹn thành công (không bị giới hạn 24 giờ).
- [ ] **Case 3**: Bệnh nhân hủy hẹn thất bại khi thời gian hẹn còn dưới 24 giờ.
- [ ] **Case 4**: Thất bại khi ID lịch hẹn không tồn tại.
- [ ] **Case 5**: Thất bại khi tài khoản không có quyền hủy lịch hẹn này (bệnh nhân khác hoặc bác sĩ không phụ trách).
- [ ] **Case 6**: Thất bại khi lịch hẹn đã ở trạng thái Completed.
- [ ] **Case 7**: Thất bại khi lịch hẹn đã ở trạng thái Cancelled trước đó.
- [ ] **Case 8**: Thất bại khi lịch hẹn đã ở trạng thái Rejected trước đó.
- [ ] **Case 9**: Trạng thái khung giờ của lịch hẹn được trả về `Available` khi hủy thành công.
- [ ] **Case 10**: Trạng thái khung giờ giữ nguyên là `Booked` nếu hủy thất bại.
- [ ] **Case 11**: Trạng thái lịch hẹn chuyển sang `Cancelled` khi hủy thành công.
- [ ] **Case 12**: Lý do hủy lịch hẹn được lưu đúng vào cơ sở dữ liệu.
- [ ] **Case 13**: Thời gian hủy `CancelledAt` được ghi nhận đúng.
- [ ] **Case 14**: Kiểm tra hệ thống tạo thông báo cho Bệnh nhân khi Bác sĩ hủy lịch.
- [ ] **Case 15**: Kiểm tra hệ thống tạo thông báo cho Bác sĩ khi Bệnh nhân hủy lịch.
- [ ] **Case 16**: Kiểm tra gửi email thông báo hủy lịch cho cả hai bên.
- [ ] **Case 17**: Kiểm tra bản ghi lịch sử hủy lịch được ghi lại vào `AppointmentHistory`.
- [ ] **Case 18**: Bác sĩ/Bệnh nhân không thể hủy lịch hẹn đã bị Từ chối (Rejected).
- [ ] **Case 19**: Hủy thành công lịch hẹn của khách vãng lai (Guest).
- [ ] **Case 20**: Đảm bảo transaction của cơ sở dữ liệu được commit đầy đủ.

### 3. `RescheduleAppointmentAsync` (Đổi lịch hẹn - `AppointmentService`)
Đổi lịch hẹn sang khung giờ mới, giải phóng khung giờ cũ, áp dụng chính sách 24 giờ.
- [ ] **Case 1**: Đổi lịch hẹn thành công (lịch hẹn hiện tại cách thời gian đổi > 24 giờ, khung giờ mới trống).
- [ ] **Case 2**: Thất bại khi đổi lịch hẹn cách giờ bắt đầu dưới 24 giờ.
- [ ] **Case 3**: Thất bại khi ID lịch hẹn cần đổi không tồn tại.
- [ ] **Case 4**: Thất bại khi tài khoản đổi lịch không có quyền (không phải bệnh nhân đặt lịch).
- [ ] **Case 5**: Thất bại khi khung giờ mới chọn không tồn tại.
- [ ] **Case 6**: Thất bại khi khung giờ mới chọn đã bị đặt (`Booked`).
- [ ] **Case 7**: Thất bại khi khung giờ mới chọn bị khóa (`Unavailable`).
- [ ] **Case 8**: Thất bại khi khung giờ mới chọn nằm ở quá khứ.
- [ ] **Case 9**: Thất bại khi khung giờ mới chọn thuộc về một bác sĩ khác (không được đổi bác sĩ khi đổi lịch).
- [ ] **Case 10**: Thất bại khi lịch hẹn hiện tại đã `Completed`.
- [ ] **Case 11**: Thất bại khi lịch hẹn hiện tại đã `Cancelled`.
- [ ] **Case 12**: Thất bại khi lịch hẹn hiện tại đã `Rejected`.
- [ ] **Case 13**: Khung giờ cũ được trả về trạng thái `Available` sau khi đổi thành công.
- [ ] **Case 14**: Khung giờ mới chuyển sang trạng thái `Booked` sau khi đổi thành công.
- [ ] **Case 15**: Lịch hẹn được cập nhật lại tham chiếu tới ID khung giờ mới.
- [ ] **Case 16**: Trạng thái lịch hẹn được đưa về `Pending` hoặc giữ nguyên `Approved` theo cấu hình.
- [ ] **Case 17**: Kiểm tra log lịch sử đổi lịch hẹn được lưu vào `AppointmentHistory`.
- [ ] **Case 18**: Kiểm tra tạo thông báo gửi tới bác sĩ thông báo về việc đổi lịch.
- [ ] **Case 19**: Kiểm tra gửi email thông báo đổi lịch tới bệnh nhân và bác sĩ.
- [ ] **Case 20**: Đổi lịch thành công đối với lịch hẹn của Guest.

### 4. `RegisterAsync` (Đăng ký tài khoản - `AuthService`)
Đăng ký người dùng mới, kiểm tra trùng lặp email/sđt, băm mật khẩu, tạo hồ sơ (Patient/Doctor Profile).
- [ ] **Case 1**: Đăng ký thành công cho Bệnh nhân (tạo User & PatientProfile).
- [ ] **Case 2**: Đăng ký thành công cho Bác sĩ (tạo User & DoctorProfile ở trạng thái Pending).
- [ ] **Case 3**: Thất bại khi email đăng ký đã tồn tại trong hệ thống.
- [ ] **Case 4**: Thất bại khi số điện thoại đăng ký đã tồn tại trong hệ thống.
- [ ] **Case 5**: Thất bại khi vai trò (Role) yêu cầu đăng ký không tồn tại.
- [ ] **Case 5**: Đảm bảo mật khẩu của người dùng được mã hóa bằng thuật toán BCrypt trước khi lưu.
- [ ] **Case 7**: Mã OTP xác minh email được sinh tự động và lưu vào bảng `OtpVerification`.
- [ ] **Case 8**: Đảm bảo gửi email chứa mã OTP xác minh về hòm thư người dùng đăng ký.
- [ ] **Case 9**: Thất bại khi email trống hoặc không hợp lệ.
- [ ] **Case 10**: Thất bại khi mật khẩu trống hoặc không đủ độ mạnh.
- [ ] **Case 11**: Thất bại khi số điện thoại trống hoặc không đúng định dạng.
- [ ] **Case 12**: Không cho phép đăng ký trực tiếp vai trò Quản trị viên (Admin) thông qua API công khai này.
- [ ] **Case 13**: Trạng thái mặc định của tài khoản mới tạo là `Inactive`.
- [ ] **Case 14**: Giá trị `IsEmailVerified` mặc định là `false`.
- [ ] **Case 15**: Hồ sơ PatientProfile tạo ra phải liên kết chính xác tới User ID vừa tạo.
- [ ] **Case 16**: Hồ sơ DoctorProfile tạo ra phải liên kết chính xác tới User ID vừa tạo.
- [ ] **Case 17**: Các giá trị hiển thị mặc định của Bác sĩ (visibility, status) được thiết lập chuẩn.
- [ ] **Case 18**: Chuyên khoa của bác sĩ được liên kết chuẩn xác nếu có truyền thông tin trong lúc đăng ký.
- [ ] **Case 19**: Đảm bảo transaction lưu thông tin đăng ký hoạt động an toàn.
- [ ] **Case 20**: Thất bại và rollback nếu xảy ra lỗi ghi file/ghi cơ sở dữ liệu trong quá trình đăng ký.

### 5. `LoginAsync` (Đăng nhập - `AuthService`)
Xác thực thông tin, kiểm tra trạng thái khóa/chưa kích hoạt, phát hành token JWT.
- [ ] **Case 1**: Đăng nhập thành công với thông tin chính xác (trả về các Token tương ứng).
- [ ] **Case 2**: Thất bại khi email đăng nhập không tồn tại.
- [ ] **Case 3**: Thất bại khi nhập sai mật khẩu.
- [ ] **Case 4**: Thất bại khi tài khoản đang bị khóa (`Locked`).
- [ ] **Case 5**: Thất bại khi tài khoản chưa được kích hoạt (`Inactive`).
- [ ] **Case 6**: Thất bại khi email chưa được xác minh (`IsEmailVerified` = false).
- [ ] **Case 7**: Đảm bảo tự động sinh mã OTP và gửi email kích hoạt mới nếu người dùng đăng nhập khi chưa xác minh email.
- [ ] **Case 8**: Mã Access Token trả về chứa đúng các claim cần thiết (UserId, Email, Role).
- [ ] **Case 9**: Gọi hàm sinh JWT của TokenService với đúng cấu hình tham số.
- [ ] **Case 10**: Tạo mới và lưu trữ Refresh Token vào cơ sở dữ liệu khi đăng nhập thành công.
- [ ] **Case 11**: Đăng nhập thành công với vai trò Bệnh nhân (Patient).
- [ ] **Case 12**: Đăng nhập thành công với vai trò Bác sĩ (Doctor).
- [ ] **Case 13**: Đăng nhập thành công với vai trò Quản lý doanh nghiệp (BusinessManager).
- [ ] **Case 14**: Đăng nhập thành công với vai trò Admin.
- [ ] **Case 15**: Thất bại khi tài khoản người dùng đã bị xóa logic (soft-deleted).
- [ ] **Case 16**: Kiểm tra quyền hạn và vai trò được lấy chuẩn từ cơ sở dữ liệu.
- [ ] **Case 17**: Cập nhật dấu vết thời gian đăng nhập lần cuối (`LastLoginAt`) nếu hệ thống hỗ trợ.
- [ ] **Case 18**: Thất bại khi xảy ra lỗi ngoài ý muốn trong quá trình băm giải mã BCrypt.
- [ ] **Case 19**: Đăng nhập từ nhiều thiết bị sinh ra các Refresh Token riêng biệt.
- [ ] **Case 20**: Xác nhận cơ sở dữ liệu lưu các thay đổi Refresh Token.

### 6. `SubmitTestAsync` (Nộp kết quả trắc nghiệm - `PsychometricService`)
Kiểm tra tính hợp lệ của bài test, tính điểm tự động cho PHQ9 và DASS21, lưu kết quả.
- [ ] **Case 1**: Nộp bài trắc nghiệm PHQ9 thành công (tính đúng tổng điểm, kết quả chẩn đoán chính xác).
- [ ] **Case 2**: Nộp bài trắc nghiệm DASS21 thành công (tính riêng biệt điểm Trầm cảm, Lo âu, Căng thẳng và chẩn đoán đúng).
- [ ] **Case 3**: Thất bại khi không tìm thấy hồ sơ bệnh nhân tương ứng.
- [ ] **Case 4**: Thất bại khi bài trắc nghiệm (TestId) không tồn tại hoặc đã bị xóa.
- [ ] **Case 5**: Thất bại khi loại bài trắc nghiệm không được hỗ trợ (khác PHQ9 và DASS21).
- [ ] **Case 6**: Thất bại khi số lượng câu trả lời không khớp với số lượng câu hỏi trong bài trắc nghiệm.
- [ ] **Case 7**: Thất bại khi một câu trả lời có số điểm âm (< 0).
- [ ] **Case 8**: Thất bại khi một câu trả lời có số điểm lớn hơn 3 (> 3).
- [ ] **Case 9**: Thất bại khi câu trả lời chứa ID câu hỏi không thuộc về bài trắc nghiệm này.
- [ ] **Case 10**: Kiểm tra phân loại mức độ PHQ9:
  - `0-4` => Tối thiểu (Minimal)
  - `5-9` => Nhẹ (Mild)
  - `10-14` => Vừa (Moderate)
  - `15-19` => Vừa nghiêm trọng (Moderately Severe)
  - `20-27` => Nghiêm trọng (Severe)
- [ ] **Case 11**: Kiểm tra phân loại mức độ Trầm cảm của DASS21.
- [ ] **Case 12**: Kiểm tra phân loại mức độ Lo âu của DASS21.
- [ ] **Case 13**: Kiểm tra phân loại mức độ Căng thẳng của DASS21.
- [ ] **Case 14**: Kiểm tra điểm số thô của DASS21 được nhân đôi theo chuẩn y khoa trước khi đối chiếu.
- [ ] **Case 15**: Kiểm tra các bản ghi cũ của cùng lịch hẹn & bài trắc nghiệm này được soft-delete (ẩn đi) trước khi lưu bản ghi mới.
- [ ] **Case 16**: Lưu bản ghi kết quả bài trắc nghiệm mới thành công vào cơ sở dữ liệu.
- [ ] **Case 17**: Định dạng kết quả điểm chi tiết được serialize thành chuỗi JSON lưu trong trường tương ứng.
- [ ] **Case 18**: Liên kết chính xác kết quả bài trắc nghiệm tới mã lịch hẹn `AppointmentId` nếu có.
- [ ] **Case 19**: Lưu chính xác thời gian nộp bài (`SubmittedAt`).
- [ ] **Case 20**: Xác nhận cơ sở dữ liệu hoàn tất giao dịch ghi bài test thành công.

### 7. `CreateAsync` (Tạo bệnh án / ghi chú tư vấn - `ConsultationNoteService` / `BusinessServices`)
Tạo bệnh án sau buổi tư vấn, tạo thông báo cho bệnh nhân, hoàn thành lịch hẹn.
- [ ] **Case 1**: Tạo ghi chú tư vấn thành công kết nối tới bác sĩ, hồ sơ bệnh nhân và cuộc hẹn tương ứng.
- [ ] **Case 2**: Tạo ghi chú tư vấn thành công không cần lịch hẹn (tạo độc lập).
- [ ] **Case 3**: Thất bại khi bác sĩ không tồn tại.
- [ ] **Case 4**: Thất bại khi hồ sơ bệnh nhân (`PatientRecord`) không tồn tại.
- [ ] **Case 5**: Thất bại khi ID lịch hẹn truyền vào không có trong hệ thống.
- [ ] **Case 6**: Thất bại khi bác sĩ tạo ghi chú không phải bác sĩ được chỉ định trong lịch hẹn.
- [ ] **Case 7**: Kiểm tra hệ thống tự động tạo và gửi thông báo cho bệnh nhân khi hồ sơ tư vấn được tạo.
- [ ] **Case 8**: Đảm bảo lấy đúng thông tin tài khoản bệnh nhân liên quan để nhận thông báo.
- [ ] **Case 9**: Nội dung của ghi chú tư vấn (tóm tắt, chẩn đoán, đề xuất) được lưu trữ đầy đủ.
- [ ] **Case 10**: Lưu trữ đúng ngày khuyên tái khám tiếp theo.
- [ ] **Case 11**: Kế hoạch điều trị tiếp theo được lưu đầy đủ.
- [ ] **Case 12**: Lưu trữ bản ghi thành công vào bảng `ConsultationNote`.
- [ ] **Case 13**: Trạng thái lịch hẹn được cập nhật thành `Completed` sau khi tạo xong ghi chú (theo quy trình khép kín).
- [ ] **Case 14**: Dữ liệu ghi chú được map chuẩn sang DTO tương ứng.
- [ ] **Case 15**: Gọi hàm `EnrichRecordsAsync` để bổ sung tên bác sĩ, bệnh nhân vào DTO.
- [ ] **Case 16**: Thất bại khi thiếu các thông tin bắt buộc (ví dụ: tóm tắt tư vấn trống).
- [ ] **Case 17**: Các giá trị mặc định được khởi tạo chính xác khi không truyền dữ liệu tùy chọn.
- [ ] **Case 18**: Bản ghi lịch sử tư vấn được ghi nhận chuẩn xác.
- [ ] **Case 19**: Lưu thay đổi thành công qua Unit of Work.
- [ ] **Case 20**: Rollback an toàn nếu có lỗi ghi cơ sở dữ liệu.

---

## Kế hoạch cho các hàm khác
Đối với các hàm nghiệp vụ khác (như lấy danh sách, CRUD đơn giản cho các cấu hình hoặc hồ sơ):
- Viết **1 case thành công (Happy Path)**.
- Viết **1 case kiểm tra biên/dữ liệu trống (Null/Empty Input)**.
- Viết **1 case tài nguyên không tìm thấy (Resource Not Found)** để đảm bảo bao phủ đầy đủ API.
