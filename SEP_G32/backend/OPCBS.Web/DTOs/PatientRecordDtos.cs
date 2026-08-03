namespace OPCBS.Web.DTOs;

public class PatientRecordDto
{
    public Guid Id { get; set; }
    public Guid? PatientId { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public string? GuestPhone { get; set; }
    public string? GuestEmail { get; set; }
    public DateTime? GuestDateOfBirth { get; set; }
    public string? GuestGender { get; set; }
    public string? GuestAddress { get; set; }

    public string? PsychologicalHistory { get; set; }
    public string? CurrentSymptoms { get; set; }
    public string? StressFactors { get; set; }
    public string? GeneralNotes { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public string DisplayName => string.IsNullOrEmpty(GuestName) ? "Chưa cập nhật" : GuestName;
    public string DisplayPhone => string.IsNullOrEmpty(GuestPhone) ? "Chưa cập nhật" : GuestPhone;
    public string DisplayEmail => string.IsNullOrEmpty(GuestEmail) ? "Chưa cập nhật" : GuestEmail;
    public bool IsGuest => !PatientId.HasValue;
}

public class CreatePatientRecordDto
{
    public Guid? PatientId { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public string? GuestPhone { get; set; }
    public string? GuestEmail { get; set; }
    public DateTime? GuestDateOfBirth { get; set; }
    public string? GuestGender { get; set; }
    public string? GuestAddress { get; set; }

    public string? PsychologicalHistory { get; set; }
    public string? CurrentSymptoms { get; set; }
    public string? StressFactors { get; set; }
    public string? GeneralNotes { get; set; }
}

public class UpdatePatientRecordDto : CreatePatientRecordDto { }
