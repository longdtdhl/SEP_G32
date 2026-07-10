using OPCBS.Domain.Common;

namespace OPCBS.Domain.Entities;

/// <summary>
/// Nhật ký cảm xúc hàng ngày của bệnh nhân.
/// Giúp bệnh nhân tự theo dõi tâm trạng và chia sẻ với bác sĩ trị liệu.
/// </summary>
public class EmotionJournal : BaseEntity
{
    /// <summary>Foreign key to PatientProfile</summary>
    public Guid PatientId { get; set; }

    /// <summary>Tiêu đề nhật ký</summary>
    public required string Title { get; set; }

    /// <summary>Nội dung ghi chép chi tiết</summary>
    public string? Content { get; set; }

    /// <summary>Thang điểm cảm xúc (1 = Rất tệ, 2 = Tệ, 3 = Bình thường, 4 = Tốt, 5 = Rất tốt)</summary>
    public int MoodScale { get; set; }

    /// <summary>Thang điểm căng thẳng (1 = Rất thấp, 2 = Thấp, 3 = Trung bình, 4 = Cao, 5 = Rất cao)</summary>
    public int StressScale { get; set; }

    /// <summary>Có chia sẻ nhật ký này với bác sĩ trị liệu hay không</summary>
    public bool IsShared { get; set; } = false;

    // Navigation
    public virtual required PatientProfile Patient { get; set; }
}
