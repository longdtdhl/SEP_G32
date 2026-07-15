# Kế hoạch & Checklist Kiểm thử (Unit Test Plan & Checklist)

Tài liệu này chứa kế hoạch kiểm thử đơn vị (Unit Test) cho dự án OPCBS. Tập trung kiểm thử đầy đủ **17 hàm quan trọng bậc nhất** của phần nghiệp vụ, mỗi hàm được viết khoảng **20 test cases** để bao phủ toàn bộ các luồng (success, validation, edge cases, error).

---

## 17 Hàm nghiệp vụ quan trọng được lựa chọn (Mỗi hàm 20 cases)

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


### 8. `CreateAsync` (Tạo gói điều trị - `TreatmentPackageService`)
Hàm này thực hiện gán gói điều trị mới từ bác sĩ cho bệnh nhân, kiểm tra ràng buộc duy nhất (mỗi bệnh nhân chỉ có tối đa 1 gói hoạt động).
- [ ] **Case 1**: Gán gói điều trị thành công cho bệnh nhân chưa có gói hoạt động nào.
- [ ] **Case 2**: Thất bại khi bác sĩ không tồn tại trong hệ thống.
- [ ] **Case 3**: Thất bại khi bệnh nhân không tồn tại.
- [ ] **Case 4**: Thất bại khi bệnh nhân đã có gói điều trị ở trạng thái `Assigned` (chờ xác nhận).
- [ ] **Case 5**: Thất bại khi bệnh nhân đã có gói điều trị ở trạng thái `Active` (đang hoạt động).
- [ ] **Case 6**: Thất bại khi bệnh nhân đã có gói điều trị ở trạng thái `Suspended`.
- [ ] **Case 7**: Thành công khi tạo gói mới cho bệnh nhân có gói cũ đã ở trạng thái `Completed`.
- [ ] **Case 8**: Thành công khi tạo gói mới cho bệnh nhân có gói cũ đã ở trạng thái `Cancelled`.
- [ ] **Case 9**: Thành công khi tạo gói mới cho bệnh nhân có gói cũ đã ở trạng thái `Rejected`.
- [ ] **Case 10**: Trạng thái mặc định của gói điều trị mới là `Assigned`.
- [ ] **Case 11**: Ngày hết hạn (ExpirationDate) được tính toán đúng dựa trên số ngày hiệu lực `ValidityDays`.
- [ ] **Case 12**: Số buổi còn lại `RemainingSessions` được gán mặc định bằng số buổi đăng ký `SessionQuantity`.
- [ ] **Case 13**: Giá tiền `Price` được lưu trữ chính xác từ DTO.
- [ ] **Case 14**: Tên gói `Name` và mô tả `Description` được gán chính xác.
- [ ] **Case 15**: Kiểm tra hệ thống tự động gửi thông báo cho bệnh nhân khi được gán gói mới.
- [ ] **Case 16**: Kiểm tra thông tin bác sĩ tạo được lấy đúng để đưa vào thông báo.
- [ ] **Case 17**: Kiểm tra ánh xạ dữ liệu gói điều trị sang DTO thành công.
- [ ] **Case 18**: Gọi hàm `EnrichNamesAsync` để điền đúng tên bác sĩ và bệnh nhân.
- [ ] **Case 19**: Lưu thay đổi thành công qua Unit of Work.
- [ ] **Case 20**: Rollback an toàn nếu có lỗi cơ sở dữ liệu khi lưu gói điều trị.

### 9. `AcceptPackageAsync` (Xác nhận gói điều trị - `TreatmentPackageService`)
Bệnh nhân chấp nhận gói điều trị được bác sĩ gán, kích hoạt trạng thái hoạt động của gói.
- [ ] **Case 1**: Bệnh nhân xác nhận thành công gói điều trị (trạng thái chuyển sang `Active`).
- [ ] **Case 2**: Thất bại khi ID gói điều trị không tồn tại.
- [ ] **Case 3**: Thất bại khi bệnh nhân không tồn tại.
- [ ] **Case 4**: Thất bại khi tài khoản bệnh nhân hiện tại không trùng khớp với PatientId trong gói điều trị.
- [ ] **Case 5**: Thất bại khi gói điều trị đã ở trạng thái `Active` trước đó.
- [ ] **Case 6**: Thất bại khi gói điều trị đã ở trạng thái `Completed`.
- [ ] **Case 7**: Thất bại khi gói điều trị đã ở trạng thái `Cancelled`.
- [ ] **Case 8**: Thất bại khi gói điều trị đã ở trạng thái `Rejected`.
- [ ] **Case 9**: Ghi nhận chính xác ngày chấp nhận `AcceptedDate` bằng thời gian hiện tại.
- [ ] **Case 10**: Ghi nhận chính xác ngày hoạt động `ActiveDate` bằng thời gian hiện tại.
- [ ] **Case 11**: Cập nhật dấu vết thời gian chỉnh sửa `UpdatedAt` chính xác.
- [ ] **Case 12**: Xác nhận gọi hàm cập nhật trạng thái trong repository.
- [ ] **Case 13**: Xác nhận gọi SaveChangesAsync trên Unit of Work.
- [ ] **Case 14**: Thất bại và rollback nếu lưu thay đổi cơ sở dữ liệu gặp lỗi.
- [ ] **Case 15**: Chấp nhận gói điều trị thành công cập nhật đúng dữ liệu trong DB.
- [ ] **Case 16**: Kiểm tra quyền truy cập bị từ chối nếu User không có vai trò Patient.
- [ ] **Case 17**: Gói điều trị ở trạng thái `Suspended` không được phép chấp nhận.
- [ ] **Case 18**: Kiểm tra log hoạt động hệ thống được lưu vết.
- [ ] **Case 19**: Đảm bảo các thông tin DTO trả về hiển thị trạng thái `Active`.
- [ ] **Case 20**: Xác nhận hoàn tất thành công giao dịch chấp nhận gói.

### 10. `CancelPackageAsync` (Hủy gói điều trị - `TreatmentPackageService`)
Hủy gói điều trị bởi bác sĩ phụ trách hoặc bệnh nhân được gán.
- [ ] **Case 1**: Bác sĩ phụ trách hủy gói điều trị thành công.
- [ ] **Case 2**: Bệnh nhân được gán hủy gói điều trị thành công.
- [ ] **Case 3**: Thất bại khi ID gói điều trị không tồn tại.
- [ ] **Case 4**: Thất bại khi tài khoản thực hiện hủy không phải là bác sĩ phụ trách lẫn bệnh nhân được gán (Không có quyền).
- [ ] **Case 5**: Thất bại khi gói điều trị đã ở trạng thái `Completed` (không được hủy gói đã hoàn thành).
- [ ] **Case 6**: Thất bại khi gói điều trị đã ở trạng thái `Cancelled` trước đó.
- [ ] **Case 7**: Hủy thành công gói đang ở trạng thái `Assigned`.
- [ ] **Case 8**: Hủy thành công gói đang ở trạng thái `Active`.
- [ ] **Case 9**: Lý do hủy ghi nhận mặc định là "Đã hủy bởi bác sĩ" khi bác sĩ thực hiện hủy mà không truyền lý do.
- [ ] **Case 10**: Lý do hủy ghi nhận mặc định là "Đã hủy bởi bệnh nhân" khi bệnh nhân thực hiện hủy mà không truyền lý do.
- [ ] **Case 11**: Lưu trữ đúng lý do hủy tự chọn (custom reason) nếu người dùng truyền vào.
- [ ] **Case 12**: Cập nhật trạng thái gói thành `Cancelled` thành công.
- [ ] **Case 13**: Thời gian cập nhật `UpdatedAt` được lưu đúng.
- [ ] **Case 14**: Đảm bảo gọi hàm lưu thay đổi thông tin thực tế của gói xuống Repository.
- [ ] **Case 15**: Lưu thành công giao dịch qua SaveChanges.
- [ ] **Case 16**: Kiểm tra thông báo được gửi cho bác sĩ khi bệnh nhân hủy gói.
- [ ] **Case 17**: Kiểm tra thông báo được gửi cho bệnh nhân khi bác sĩ hủy gói.
- [ ] **Case 18**: Không cho phép hủy gói điều trị đã bị từ chối (`Rejected`).
- [ ] **Case 19**: Hủy thành công gói đang bị tạm ngưng (`Suspended`).
- [ ] **Case 20**: Rollback và khôi phục trạng thái cũ của gói nếu lưu dữ liệu thất bại.


### 11. `CreateScheduleAsync` (Tạo lịch làm việc - `ScheduleService`)
Hàm này thực hiện lưu cấu hình lịch làm việc của bác sĩ và tự động sinh các khung giờ hẹn tương ứng cho N tuần kế tiếp.
- [ ] **Case 1**: Tạo lịch làm việc thành công khi đầu vào hợp lệ và start time trước end time.
- [ ] **Case 2**: Thất bại khi bác sĩ không tồn tại trong hệ thống.
- [ ] **Case 3**: Thất bại khi thời gian bắt đầu trùng với thời gian kết thúc.
- [ ] **Case 4**: Thất bại khi thời gian bắt đầu sau thời gian kết thúc.
- [ ] **Case 5**: Tính toán chính xác số lượng slots trong một ngày dựa trên thời lượng slot.
- [ ] **Case 6**: Tự động sinh slots cho đúng số tuần yêu cầu (WeeksAhead).
- [ ] **Case 7**: Không sinh slots cho những ngày không thuộc WorkingDays.
- [ ] **Case 8**: Không sinh slots cho những ngày trùng với ngày nghỉ (DayOff) của bác sĩ.
- [ ] **Case 9**: Xóa bỏ toàn bộ các slots đang ở trạng thái `Available` trong tương lai để đồng bộ lịch mới.
- [ ] **Case 10**: Giữ nguyên các slots đã được đặt lịch (`Booked`) trong tương lai không bị xóa.
- [ ] **Case 11**: Giữ nguyên các slots bị khóa (`Blocked`) trong tương lai không bị xóa.
- [ ] **Case 12**: Không tạo trùng lặp slot nếu đã tồn tại slot cùng ngày và giờ hoạt động.
- [ ] **Case 13**: Lưu thành công bản ghi Schedule mới vào cơ sở dữ liệu.
- [ ] **Case 14**: Ánh xạ dữ liệu lịch làm việc sang DTO thành công.
- [ ] **Case 15**: Lưu các slots mới tạo xuống DbContext.
- [ ] **Case 16**: Lưu thay đổi thành công qua Unit of Work.
- [ ] **Case 17**: Kiểm tra lỗi phân tích cú pháp thời gian nếu truyền sai định dạng.
- [ ] **Case 18**: Đảm bảo gọi SaveChanges trên DbContext.
- [ ] **Case 19**: Sử dụng WeeksAhead mặc định bằng 4 nếu tham số truyền vào là null.
- [ ] **Case 20**: Rollback và hủy bỏ mọi thay đổi nếu quá trình lưu database gặp lỗi.

### 12. `CreateSlotAsync` (Tạo khung giờ hẹn thủ công - `ScheduleService`)
Hàm này tạo một khung giờ hẹn riêng lẻ và kiểm tra chống trùng lặp giờ của cùng một bác sĩ.
- [ ] **Case 1**: Tạo khung giờ thành công khi thông tin hợp lệ và không trùng giờ.
- [ ] **Case 2**: Thất bại khi bác sĩ không tồn tại.
- [ ] **Case 3**: Thất bại khi định dạng ngày không hợp lệ.
- [ ] **Case 4**: Thất bại khi định dạng thời gian bắt đầu không hợp lệ.
- [ ] **Case 5**: Thất bại khi định dạng thời gian kết thúc không hợp lệ.
- [ ] **Case 6**: Thất bại khi thời gian bắt đầu bằng thời gian kết thúc.
- [ ] **Case 7**: Thất bại khi thời gian bắt đầu sau thời gian kết thúc.
- [ ] **Case 8**: Thất bại do trùng lặp khi thời gian bắt đầu của slot mới nằm trong một slot cũ.
- [ ] **Case 9**: Thất bại do trùng lặp khi thời gian kết thúc của slot mới nằm trong một slot cũ.
- [ ] **Case 10**: Thất bại do trùng lặp khi slot mới bao phủ toàn bộ thời gian của một slot cũ.
- [ ] **Case 11**: Thất bại do trùng lặp khi slot mới hoàn toàn nằm trong một slot cũ.
- [ ] **Case 12**: Thành công khi slot mới bắt đầu đúng thời điểm slot cũ kết thúc (tiếp nối).
- [ ] **Case 13**: Trạng thái mặc định của slot thủ công tạo ra là `Available`.
- [ ] **Case 14**: Giá tiền được gán chính xác nếu có truyền tham số.
- [ ] **Case 15**: Ánh xạ dữ liệu slot sang DTO thành công.
- [ ] **Case 16**: Lưu slot mới thành công vào cơ sở dữ liệu.
- [ ] **Case 17**: Đảm bảo gọi SaveChanges trên DbContext.
- [ ] **Case 18**: Rollback an toàn nếu có lỗi cơ sở dữ liệu.
- [ ] **Case 19**: Đảm bảo chỉ kiểm tra trùng lặp trên các slot của cùng một bác sĩ.
- [ ] **Case 20**: Kiểm tra múi giờ/ngày được chuyển đổi chuẩn xác.

### 13. `ToggleBlockSlotAsync` (Khóa/Mở khóa khung giờ - `ScheduleService`)
Học này thực hiện khóa (Blocked) hoặc mở khóa (Available) một khung giờ hẹn của bác sĩ.
- [ ] **Case 1**: Khóa thành công slot đang ở trạng thái `Available` sang `Blocked`.
- [ ] **Case 2**: Mở khóa thành công slot đang ở trạng thái `Blocked` sang `Available`.
- [ ] **Case 3**: Thất bại khi ID slot không tồn tại.
- [ ] **Case 4**: Thất bại khi bác sĩ không tồn tại trong hệ thống.
- [ ] **Case 5**: Thất bại khi bác sĩ hiện tại không sở hữu slot này (không có quyền).
- [ ] **Case 6**: Thất bại khi slot đã được đặt lịch bởi bệnh nhân (`Booked`).
- [ ] **Case 7**: Thất bại khi slot đã hoàn thành (`Completed`).
- [ ] **Case 8**: Thất bại khi slot ở trạng thái không được hỗ trợ thay đổi.
- [ ] **Case 9**: Cập nhật thời gian chỉnh sửa `UpdatedAt` chính xác.
- [ ] **Case 10**: Lưu trạng thái mới thành công vào cơ sở dữ liệu.
- [ ] **Case 11**: Đảm bảo gọi SaveChanges trên DbContext.
- [ ] **Case 12**: Rollback an toàn nếu lưu database lỗi.
- [ ] **Case 13**: Đảm bảo trạng thái trong database khớp với trạng thái thay đổi.
- [ ] **Case 14**: Kiểm tra hoạt động lưu nhật ký chỉnh sửa slot nếu có.
- [ ] **Case 15**: Cho phép thay đổi trạng thái nhiều lần liên tiếp thành công.
- [ ] **Case 16**: Kiểm tra quyền truy cập bị từ chối nếu User không phải là Doctor.
- [ ] **Case 17**: Kiểm tra lỗi hệ thống ngoài ý muốn được xử lý chuẩn xác.
- [ ] **Case 18**: Trả về dữ liệu trống và lỗi rõ ràng khi không tìm thấy thực thể.
- [ ] **Case 19**: Xác nhận gọi SaveChangesAsync trên Unit of Work.
- [ ] **Case 20**: Hoàn tất giao dịch Toggle thành công.

### 14. `CreateAsync` (Giao bài tập điều trị - `TherapyAssignmentService`)
Bác sĩ giao bài tập về nhà cho bệnh nhân thuộc gói điều trị tương ứng.
- [ ] **Case 1**: Giao bài tập thành công khi gói điều trị hợp lệ.
- [ ] **Case 2**: Thất bại khi gói điều trị không tồn tại.
- [ ] **Case 3**: Thất bại khi gói điều trị đã bị xóa logic (`IsDeleted` = true).
- [ ] **Case 4**: Tiêu đề bài tập `Title` được gán chính xác từ DTO.
- [ ] **Case 5**: Mô tả bài tập `Description` được gán chính xác từ DTO.
- [ ] **Case 6**: Hạn nộp `DueDate` được gán chính xác từ DTO.
- [ ] **Case 7**: Trạng thái mặc định ban đầu là `0` (Chưa nộp).
- [ ] **Case 8**: Bản ghi liên kết đúng tới thực thể Gói điều trị `TreatmentPackage`.
- [ ] **Case 9**: Ánh xạ dữ liệu bài tập vừa tạo sang DTO thành công.
- [ ] **Case 10**: Lưu bài tập thành công vào cơ sở dữ liệu.
- [ ] **Case 11**: Đảm bảo gọi SaveChanges trên DbContext.
- [ ] **Case 12**: Rollback an toàn nếu lưu database lỗi.
- [ ] **Case 13**: Xác nhận ngày tạo `CreatedAt` được tự động ghi nhận.
- [ ] **Case 14**: Phản hồi thành công chứa đúng thông điệp "Đã giao bài tập thành công."
- [ ] **Case 15**: Kiểm tra quyền hạn của bác sĩ tạo bài tập.
- [ ] **Case 16**: Dữ liệu DTO trả về chứa chính xác thông tin bài tập.
- [ ] **Case 17**: Đảm bảo gọi AddAsync trên Repository.
- [ ] **Case 18**: Khung giờ nộp bài không được ở trong quá khứ.
- [ ] **Case 19**: Lưu thay đổi thành công qua Unit of Work.
- [ ] **Case 20**: Đảm bảo hoàn tất giao dịch giao bài tập.

### 15. `SubmitAsync` (Nộp bài tập điều trị - `TherapyAssignmentService`)
Bệnh nhân nộp câu trả lời bài tập điều trị.
- [ ] **Case 1**: Nộp bài tập thành công (cập nhật nội dung nộp và trạng thái).
- [ ] **Case 2**: Thất bại khi ID bài tập không tồn tại.
- [ ] **Case 3**: Thất bại khi bài tập đã bị xóa logic.
- [ ] **Case 4**: Nội dung nộp của bệnh nhân `PatientSubmission` được lưu đúng.
- [ ] **Case 5**: Thời gian nộp `SubmittedAt` được ghi nhận đúng bằng thời gian hiện tại.
- [ ] **Case 6**: Trạng thái chuyển đổi thành `1` (Đã nộp).
- [ ] **Case 7**: Thời gian cập nhật `UpdatedAt` được lưu đúng.
- [ ] **Case 8**: Lưu trạng thái bài tập thành công vào cơ sở dữ liệu.
- [ ] **Case 9**: Đảm bảo gọi SaveChanges trên DbContext.
- [ ] **Case 10**: Rollback an toàn nếu lưu database lỗi.
- [ ] **Case 11**: Phản hồi thành công chứa đúng thông điệp "Đã nộp bài tập thành công."
- [ ] **Case 12**: Ánh xạ dữ liệu bài tập vừa nộp sang DTO thành công.
- [ ] **Case 13**: Bệnh nhân không thể nộp bài tập của người khác.
- [ ] **Case 14**: Không cho phép nộp lại nếu trạng thái bài tập đã có nhận xét.
- [ ] **Case 15**: Cho phép nộp bài tập trễ hạn nếu hệ thống không giới hạn cứng.
- [ ] **Case 16**: Nội dung nộp không được rỗng hoặc null.
- [ ] **Case 17**: Kiểm tra gọi Update trên Repository.
- [ ] **Case 18**: Xác nhận gọi SaveChangesAsync trên Unit of Work.
- [ ] **Case 19**: Dữ liệu DTO trả về hiển thị trạng thái `1`.
- [ ] **Case 20**: Hoàn tất giao dịch nộp bài thành công.

### 16. `FeedbackAsync` (Nhận xét bài tập - `TherapyAssignmentService`)
Bác sĩ nhận xét và chấm điểm bài tập đã nộp của bệnh nhân.
- [ ] **Case 1**: Nhận xét bài tập thành công (cập nhật nội dung nhận xét và trạng thái).
- [ ] **Case 2**: Thất bại khi ID bài tập không tồn tại.
- [ ] **Case 3**: Thất bại khi bài tập bị xóa logic.
- [ ] **Case 4**: Thất bại khi bệnh nhân chưa nộp bài tập (trạng thái bằng 0).
- [ ] **Case 5**: Nội dung nhận xét `DoctorFeedback` được lưu đúng từ DTO.
- [ ] **Case 6**: Thời gian nhận xét `FeedbackAt` được ghi nhận đúng bằng thời gian hiện tại.
- [ ] **Case 7**: Trạng thái chuyển đổi thành `2` (Đã nhận xét).
- [ ] **Case 8**: Thời gian cập nhật `UpdatedAt` được lưu đúng.
- [ ] **Case 9**: Lưu nhận xét thành công vào cơ sở dữ liệu.
- [ ] **Case 10**: Đảm bảo gọi SaveChanges trên DbContext.
- [ ] **Case 11**: Rollback an toàn nếu lưu database lỗi.
- [ ] **Case 12**: Phản hồi thành công chứa đúng thông điệp "Đã nhận xét bài tập."
- [ ] **Case 13**: Ánh xạ dữ liệu bài tập sang DTO thành công.
- [ ] **Case 14**: Bác sĩ không phụ trách gói điều trị này không được phép nhận xét.
- [ ] **Case 15**: Không cho phép nhận xét nếu bài tập đã ở trạng thái đã nhận xét trước đó.
- [ ] **Case 16**: Nội dung nhận xét không được rỗng.
- [ ] **Case 17**: Kiểm tra gọi Update trên Repository.
- [ ] **Case 18**: Xác nhận gọi SaveChangesAsync trên Unit of Work.
- [ ] **Case 19**: Dữ liệu DTO trả về hiển thị trạng thái `2`.
- [ ] **Case 20**: Hoàn tất giao dịch nhận xét bài tập thành công.

### 17. `CreateAsync` (Viết nhật ký cảm xúc - `EmotionJournalService`)
Bệnh nhân viết nhật ký cảm xúc hàng ngày và tự đánh giá chỉ số cảm xúc/căng thẳng.
- [ ] **Case 1**: Lưu nhật ký cảm xúc thành công khi thông tin hợp lệ.
- [ ] **Case 2**: Thất bại khi không tìm thấy hồ sơ bệnh nhân tương ứng với UserId.
- [ ] **Case 3**: Thất bại khi thang điểm cảm xúc (MoodScale) nhỏ hơn 1.
- [ ] **Case 4**: Thất bại khi thang điểm cảm xúc (MoodScale) lớn hơn 5.
- [ ] **Case 5**: Thất bại khi thang điểm căng thẳng (StressScale) nhỏ hơn 1.
- [ ] **Case 6**: Thất bại khi thang điểm căng thẳng (StressScale) lớn hơn 5.
- [ ] **Case 7**: Thành công khi MoodScale bằng 1.
- [ ] **Case 8**: Thành công khi MoodScale bằng 5.
- [ ] **Case 9**: Thành công khi StressScale bằng 1.
- [ ] **Case 10**: Thành công khi StressScale bằng 5.
- [ ] **Case 11**: Tiêu đề nhật ký `Title` được lưu đúng.
- [ ] **Case 12**: Nội dung nhật ký `Content` được lưu đúng.
- [ ] **Case 13**: Cờ chia sẻ nhật ký `IsShared` được lưu đúng.
- [ ] **Case 14**: Tên bệnh nhân `PatientName` được điền đúng từ tên User.
- [ ] **Case 15**: Ánh xạ dữ liệu nhật ký sang DTO thành công.
- [ ] **Case 16**: Lưu bản ghi nhật ký cảm xúc mới thành công vào cơ sở dữ liệu.
- [ ] **Case 17**: Đảm bảo gọi SaveChanges trên DbContext.
- [ ] **Case 18**: Rollback an toàn nếu có lỗi cơ sở dữ liệu.
- [ ] **Case 19**: Xác nhận gọi AddAsync trên Repository.
- [ ] **Case 20**: Đảm bảo hoàn tất giao dịch tạo nhật ký cảm xúc.

---

## Kế hoạch cho các hàm khác
Đối với các hàm nghiệp vụ khác (như lấy danh sách, CRUD đơn giản cho các cấu hình hoặc hồ sơ):
- Viết **1 case thành công (Happy Path)**.
- Viết **1 case kiểm tra biên/dữ liệu trống (Null/Empty Input)**.
- Viết **1 case tài nguyên không tìm thấy (Resource Not Found)** để đảm bảo bao phủ đầy đủ API.
