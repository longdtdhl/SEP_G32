using OPCBS.Domain.Common;

namespace OPCBS.Domain.Entities;

/// <summary>
/// Bài tập trị liệu do Bác sĩ giao cho Bệnh nhân trong một Gói điều trị.
/// Ví dụ: "Ghi lại 3 suy nghĩ tiêu cực và cách tái cấu trúc", "Thực hành hít thở sâu 5 phút mỗi ngày"
/// </summary>
public class TherapyAssignment : BaseEntity
{
    /// <summary>Foreign key to TreatmentPackage</summary>
    public Guid TreatmentPackageId { get; set; }

    /// <summary>Tiêu đề bài tập</summary>
    public required string Title { get; set; }

    /// <summary>Nội dung yêu cầu / Mô tả bài tập chi tiết</summary>
    public string? Description { get; set; }

    /// <summary>Hướng dẫn chi tiết cách thực hiện bài tập</summary>
    public string? DetailedInstructions { get; set; }

    /// <summary>Đường link tài liệu, file bài tập, video hướng dẫn (URL)</summary>
    public string? ResourceUrl { get; set; }

    /// <summary>Hạn hoàn thành</summary>
    public DateTime? DueDate { get; set; }

    /// <summary>Trạng thái: 0 = Chưa làm, 1 = Đã nộp bài, 2 = Bác sĩ đã nhận xét</summary>
    public int Status { get; set; } = 0;

    /// <summary>Patient's submission text content</summary>
    public string? PatientSubmission { get; set; }

    /// <summary>Patient's submission link (URL to file, document, video, etc.)</summary>
    public string? PatientSubmissionUrl { get; set; }

    /// <summary>Timestamp when patient submitted the assignment</summary>
    public DateTime? SubmittedAt { get; set; }

    /// <summary>Nhận xét / phản hồi của bác sĩ về bài làm</summary>
    public string? DoctorFeedback { get; set; }

    /// <summary>Thời điểm bác sĩ nhận xét</summary>
    public DateTime? FeedbackAt { get; set; }

    // Navigation
    public virtual required TreatmentPackage TreatmentPackage { get; set; }
}
